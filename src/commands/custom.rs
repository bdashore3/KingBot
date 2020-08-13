use hook::*;
use twitchchat::{messages, Writer};
use sqlx::PgPool;
use crate::{
    helpers::string_renderer,
    structures::{Bot, BotResult, cmd_data::ConnectionPool, CommandMap}
};

#[hook]
pub async fn dispatch_custom(bot: &Bot, msg: &messages::Privmsg<'_>) -> BotResult<()> {
    let mut writer = bot.writer.lock().await;

    let data = bot.data.read().await;
    let pool = data.get::<ConnectionPool>().unwrap().as_ref();

    let subcommand = match string_renderer::get_message_word(&*msg.data, 1) {
        Ok(subcommand) => subcommand,
        Err(_) => {
            writer.privmsg(&msg.channel, "Please provide a subcommand (Add/Remove/List)!").await?;
            return Ok(())
        }
    };

    match subcommand {
        "add" => add(pool, &bot.command_map, msg, &mut writer).await?,
        "remove" => remove(pool, msg, &mut writer).await?,
        "list" => list(pool, msg, &mut writer).await?,
        _ => {}
    }

    Ok(())
}

async fn add(pool: &PgPool, command_map: &CommandMap, msg: &messages::Privmsg<'_>, writer: &mut Writer) -> BotResult<()> {
    if !msg.is_moderator() {
        writer.privmsg(&msg.channel, "You can't access this command because you aren't a moderator!").await?;
        return Ok(())
    }

    let command_name = match string_renderer::get_message_word(&*msg.data, 2) {
        Ok(name) => name, 
        Err(_e) => {
            writer.privmsg(&msg.channel, "Please provide a name for the command!").await?;
            return Ok(())
        }
    };

    if command_map.contains_key(command_name) {
        writer.privmsg(&msg.channel, "This command name is hardcoded! Please use a different one!").await?;
        return Ok(())
    }

    let command_content = match string_renderer::join_string(&*msg.data, 2) {
        Ok(content) => content,
        Err(_e) => {
            writer.privmsg(&msg.channel, "Please provide some content for the command!").await?;
            return Ok(())
        }
    };

    sqlx::query!("INSERT INTO commands(channel_name, name, content)
            VALUES($1, $2, $3)
            ON CONFLICT (channel_name, name)
            DO UPDATE
            SET content = EXCLUDED.content", 
            &*msg.channel, command_name, command_content)
        .execute(pool).await?;

    writer.privmsg(&msg.channel, &format!("Command {} successfully updated!", command_name)).await?;

    Ok(())
}

async fn list(pool: &PgPool, msg: &messages::Privmsg<'_>, writer: &mut Writer) -> BotResult<()> {
    let cmd_data = sqlx::query!("SELECT name, content FROM commands WHERE channel_name = $1", &*msg.channel)
        .fetch_all(pool).await?;

    if cmd_data.len() <= 0 {
        writer.privmsg(&msg.channel, "There are no quotes in this channel! Maybe add some?").await?;
        return Ok(())
    }

    writer.whisper(&msg.name, "Command List").await?;
    writer.whisper(&msg.name, "---------------------------------------").await?;

    for  cmd in cmd_data {
        writer.whisper(&msg.name, &format!("- {}: {}", cmd.name, cmd.content)).await?;
    }

    writer.whisper(&msg.name, "---------------------------------------").await?;

    Ok(())
}

async fn remove(pool: &PgPool, msg: &messages::Privmsg<'_>, writer: &mut Writer) -> BotResult<()> {
    if !msg.is_moderator() {
        writer.privmsg(&msg.channel, "You can't access this command because you aren't a moderator!").await?;
        return Ok(())
    }

    let command_name = match string_renderer::get_message_word(&*msg.data, 2) {
        Ok(name) => name, 
        Err(_e) => {
            writer.privmsg(&msg.channel, "Please provide a name for the command!").await?;
            return Ok(())
        }
    };

    match sqlx::query!("DELETE FROM commands WHERE channel_name = $1 AND name = $2", &*msg.channel, command_name)
        .execute(pool).await {
            Ok(_) => writer.privmsg(&msg.channel, &format!("Sucessfully removed command {}!", command_name)).await?,
            Err(_e) => writer.privmsg(&msg.channel, &format!("Command {} doesn't exist in the database! Consider adding it?", command_name)).await?
    };

    Ok(())
}