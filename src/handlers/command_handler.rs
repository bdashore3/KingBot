use twitchchat::messages;
use crate::structures::*;
use crate::commands::{
    general::*
};
use crate::helpers::string_renderer;

pub async fn handle_command(bot: &mut Bot, msg: &messages::Privmsg<'_>, command_map: &CommandMap<'_>) -> BotResult<()> {
    let command = string_renderer::get_command(&*msg.data);
    match command_map.get(command) {
        Some(command) => command(bot, msg).await?,
        None => {}
    };

    Ok(())
}

pub fn insert_commands(command_map: &mut CommandMap<'_>) {
    command_map.insert("ping", Box::new(ping));
    command_map.insert("uptime", Box::new(uptime));
}