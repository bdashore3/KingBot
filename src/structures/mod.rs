use twitchchat::{Control, Writer, Dispatcher, IntoChannel, events, messages};
use tokio::stream::StreamExt as _;
use std::collections::HashMap;
use futures::future::BoxFuture;
use crate::handlers::command_handler::*;

pub type BotResult<T> = Result<T, Box<dyn std::error::Error>>;
pub type CommandMap<'a> = HashMap<&'a str, Box<dyn for<'fut> Fn(&'fut mut Bot, &'fut messages::Privmsg<'_>) -> BoxFuture<'fut, BotResult<()>> + Send + Sync>>;
pub struct Bot {
    pub writer: Writer,
    pub control: Control,
    pub default_prefix: String,
    pub start: std::time::Instant,
}

impl Bot {
    pub async fn run(mut self, dispatcher: Dispatcher, channel: impl IntoChannel, command_map: CommandMap<'_>) -> BotResult<()> {
        let mut privmsg = dispatcher.subscribe::<events::Privmsg>();

        let mut join = dispatcher.subscribe::<events::Join>();

        match dispatcher.wait_for::<events::IrcReady>().await {
            Ok(ready) => println!("Connected! our name is: {}", ready.nickname),
            Err(e) => {
                eprintln!("There was an error when connecting to twitch!: {}", e);
                return Err("Error when connecting to Twitch".into());
            }
        }

        match self.control.writer().join(channel).await {
            Ok(_) => println!("Sucessfully joined the channel"),
            Err(e) => {
                eprintln!("There was an error when joining the channel!: {}", e);
                return Err("Error when joining the channel".into());
            }
        }


        loop {
            tokio::select! {
                Some(join_msg) = join.next() => {
                    eprintln!("{} joined {}", join_msg.name, join_msg.channel);
                }

                Some(msg) = privmsg.next() => {
                    if &msg.data[..1] == &self.default_prefix {
                        handle_command(&mut self, &msg, &command_map).await?;
                    }
                }
            }
        }
    }
}