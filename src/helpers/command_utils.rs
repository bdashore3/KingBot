use std::collections::HashMap;

use twitchchat::messages::Privmsg;

use crate::structures::{CommandMap, Command, CommandInfo};
use crate::modules::{
    other::*
};

pub fn register_commands() -> CommandMap {
    let mut command_map: HashMap<String, Command> = HashMap::new();
    command_map.insert("!ping".into(), Box::new(ping));

    command_map
}

pub fn generate_info(msg: &Privmsg) -> CommandInfo {
    let raw_message = msg.data();

    let mut words = raw_message.split_whitespace().map(|x| x.to_owned()).collect::<Vec<String>>();
    words.remove(0);

    CommandInfo {
        length: words.len(),
        words
    }
}
