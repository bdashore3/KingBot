use std::collections::HashMap;

use crate::structures::{CommandMap, Command};
use crate::modules::{
    other::*
};

pub fn register_commands() -> CommandMap {
    let mut command_map: HashMap<String, Command> = HashMap::new();
    command_map.insert("!ping".into(), Box::new(ping));

    command_map
}
