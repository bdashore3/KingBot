use std::collections::HashMap;
use futures::future::BoxFuture;
use tokio::sync::Mutex;
use twitchchat::{messages::Privmsg, Writer};

pub type KingResult<T = ()> = Result<T, Box<dyn std::error::Error + Send + Sync>>;
pub type Command = Box<dyn for<'fut> Fn(&'fut Bot, &'fut Privmsg<'_>, CommandInfo) -> BoxFuture<'fut, KingResult> + Send + Sync>;
pub type CommandMap = HashMap<String, Command>;

pub struct Bot {
    pub writer: Mutex<Writer>,
    pub commands: CommandMap
}

pub struct CommandInfo {
    pub length: usize,
    pub words: Vec<String>
}
