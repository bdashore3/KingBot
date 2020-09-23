use std::{collections::HashMap, sync::Arc};

use twitchchat::messages::Privmsg;

use crate::structures::{Bot, Command, CommandInfo, CommandMap, cmd_data::PrefixMap, cmd_data::PubCreds};
use crate::modules::{
    other::*,
    quotes::*,
    intervals::*
};

pub fn register_commands() -> CommandMap {
    let mut command_map: HashMap<String, Command> = HashMap::new();
    command_map.insert("ping".to_owned(), Arc::new(ping));
    command_map.insert("quote".to_owned(), Arc::new(dispatch_quote));
    command_map.insert("interval".to_owned(), Arc::new(dispatch_interval));

    command_map
}

pub async fn fetch_prefix(bot: &Bot, msg: &Privmsg<'_>) -> String {
    let (prefixes, default_prefix) = {
        let data = bot.data.read().await;
        let prefixes = data.get::<PrefixMap>().cloned().unwrap();
        let default_prefix = data.get::<PubCreds>().unwrap()
            .get("default prefix").cloned().unwrap();
        
        (prefixes, default_prefix)
    };

    match prefixes.get(msg.channel()) {
        Some(prefix_guard) => prefix_guard.value().to_owned(),
        None => default_prefix
    }
}

pub fn fetch_command<'a>(msg: &'a Privmsg, prefix: &str) -> &'a str {
    let prefix_len = prefix.len();

    let command = msg.data().split(" ").next().unwrap();

    &command[prefix_len..]
}

pub fn generate_info(msg: &Privmsg, command: &str) -> CommandInfo {
    let raw_message = msg.data();

    let mut words = raw_message.split_whitespace().map(|x| x.to_owned()).collect::<Vec<String>>();
    words.remove(0);

    CommandInfo {
        length: words.len(),
        command: command.to_owned(),
        words
    }
}
