use twitchchat::messages;
use crate::structures::*;
use crate::commands::{
    general::*,
    quotes::*,
};
use crate::helpers::string_renderer;

pub async fn handle_command(bot: &Bot, msg: &messages::Privmsg<'_>, command_map: &CommandMap) -> BotResult<()> {
    let command = string_renderer::get_command(&*msg.data);
    match command_map.get(command) {
        Some(command) => command(bot, msg).await?,
        None => {}
    };

    Ok(())
}

pub fn insert_commands(command_map: &mut CommandMap) {
    command_map.insert("ping", Box::new(ping));
    command_map.insert("uptime", Box::new(uptime));
    command_map.insert("quote", Box::new(dispatch_quote));
}