use twitchchat::messages;
use crate::structures::*;
use crate::commands::{
    general::*,
    quotes::*,
    custom::*,
};
use crate::helpers::string_renderer;
use cmd_data::ConnectionPool;

pub async fn handle_command(bot: &Bot, msg: &messages::Privmsg<'_>, command_map: &CommandMap) -> BotResult<()> {
    let command = string_renderer::get_command(&*msg.data);
    match command_map.get(command) {
        Some(command) => command(bot, msg).await?,
        None => {
            let data = bot.data.read().await;
            let pool = data.get::<ConnectionPool>().unwrap();
            let command_data = sqlx::query!("SELECT content FROM commands WHERE channel_name = $1 AND name = $2",
                        &*msg.channel, command)
                .fetch_optional(pool.as_ref()).await?;

            if let Some(command_data) = command_data {
                let mut writer = bot.writer.lock().await;
                let content = command_data.content
                    .replace("{user}", &msg.name)
                    .replace("{channel}", &msg.channel[1..]);
                writer.privmsg(&msg.channel, &content).await?;
            }
        }
    };

    Ok(())
}

pub fn insert_commands(command_map: &mut CommandMap) {
    command_map.insert("ping", Box::new(ping));
    command_map.insert("uptime", Box::new(uptime));
    command_map.insert("quote", Box::new(dispatch_quote));
    command_map.insert("command", Box::new(dispatch_custom));
}