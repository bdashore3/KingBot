pub mod cmd_data;

use twitchchat::{Control, Writer, Dispatcher, IntoChannel, events, messages};
use tokio::{sync::{Mutex, RwLock}, stream::StreamExt as _};
use std::{sync::Arc, collections::HashMap};
use futures::future::BoxFuture;
use typemap_rev::TypeMap;
use cmd_data::PubCreds;
use crate::handlers::command_handler::*;

pub type BotResult<T> = Result<T, Box<dyn std::error::Error>>;
pub type CommandMap = HashMap<&'static str, Box<dyn for<'fut> Fn(&'fut Bot, &'fut messages::Privmsg<'_>) -> BoxFuture<'fut, BotResult<()>> + Send + Sync>>;
pub struct Bot {
    pub writer: Mutex<Writer>,
    pub control: Control,
    pub command_map: CommandMap,
    pub data: Arc<RwLock<TypeMap>>,
    pub start: std::time::Instant,
}

impl Bot {
    pub async fn run(mut self, dispatcher: Dispatcher, channel: impl IntoChannel) {
        let mut privmsg = dispatcher.subscribe::<events::Privmsg>();

        let mut join = dispatcher.subscribe::<events::Join>();

        match dispatcher.wait_for::<events::IrcReady>().await {
            Ok(ready) => println!("Connected! our name is: {}", ready.nickname),
            Err(e) => {
                eprintln!("There was an error when connecting to twitch!: {}", e);
                return;
            }
        }

        match self.control.writer().join(channel).await {
            Ok(_) => println!("Sucessfully joined the channel"),
            Err(e) => {
                eprintln!("There was an error when joining the channel!: {}", e);
                return;
            }
        }

        loop {
            tokio::select! {
                Some(join_msg) = join.next() => {
                    eprintln!("{} joined {}", join_msg.name, join_msg.channel);
                }

                Some(msg) = privmsg.next() => {
                    let data = self.data.read().await;
                    let default_prefix = data.get::<PubCreds>().unwrap().get("default prefix").unwrap();
                    if &msg.data[..1] == default_prefix {
                        if let Err(e) = handle_command(&self, &msg, &self.command_map).await {
                            eprintln!("Error in command!: {}", e);
                            return
                        };
                    }
                }
            }
        }
    }
}