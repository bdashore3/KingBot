pub mod cmd_data;
pub mod webhooks;

use std::{collections::HashMap, sync::Arc};
use futures::future::BoxFuture;
use tokio::sync::{Mutex, RwLock};
use twitchchat::{messages::Privmsg, Writer};
use typemap_rev::TypeMap;

pub type KingResult<T = ()> = Result<T, Box<dyn std::error::Error + Send + Sync>>;
pub type Command = Arc<dyn for<'fut> Fn(&'fut Bot, &'fut Privmsg<'_>, CommandInfo) -> BoxFuture<'fut, KingResult> + Send + Sync>;
pub type CommandMap = HashMap<String, Command>;

#[derive(Clone)]
pub struct Bot {
    pub writer: Arc<Mutex<Writer>>,
    pub commands: CommandMap,
    pub data: Arc<RwLock<TypeMap>>,
    pub channel: String
}

#[derive(Clone, Debug, Eq, PartialEq, Hash)]
pub struct IntervalInfo {
    pub channel: String,
    pub alias: String
}

#[derive(Clone, Debug)]
pub struct CommandInfo {
    pub length: usize,
    pub command: String,
    pub words: Vec<String>
}

impl CommandInfo {
    pub fn get(&self, index: usize) -> Option<String> {
        self.words.get(index).cloned()
    }

    pub fn join(&mut self, begin_index: usize) -> KingResult<String> {
        self.words.drain(0..begin_index);
        Ok(self.words.join(" "))
    }

    pub fn is_empty(&self) -> bool {
        self.words.is_empty()
    }
}
