use hook::hook;
use twitchchat::{PrivmsgExt, commands::whisper, messages::Privmsg};
use crate::{structures::{Bot, CommandInfo, KingResult, cmd_data::ConnectionPool}};

#[hook]
pub async fn dispatch_custom(bot: &Bot, msg: &Privmsg<'_>, info: CommandInfo) -> KingResult {
    match info.get(0) {
        Some(subcommand) => {
            match subcommand.as_str() {
                "add" => add(bot, msg, info).await?,
                "remove" => remove(bot, msg, info).await?,
                "list" => list(bot, msg).await?,
                _ => {}
            }
        },
        None => {
            let mut writer = bot.writer.lock().await;

            writer.say(msg, "Please provide a subcommand (Add/Remove/List)!")?;
        }
    };

    Ok(())
}

async fn add(bot: &Bot, msg: &Privmsg<'_>, mut info: CommandInfo) -> KingResult {
    let mut writer = bot.writer.lock().await;

    if !msg.is_moderator() {
        writer.say(msg, "You can't access this command because you aren't a moderator!")?;
        return Ok(())
    }

    let command_name = match info.get(1) {
        Some(name) => name, 
        None => {
            writer.say(msg, "Please provide a name for the command!")?;
            return Ok(())
        }
    };

    if bot.commands.contains_key(&command_name) {
        writer.say(msg, "This command name is hardcoded! Please use a different one!")?;

        return Ok(())
    }

    let pool = bot.data.read().await
        .get::<ConnectionPool>().cloned().unwrap();

    let command_content = match info.join(2) {
        Ok(content) => content,
        Err(_e) => {
            writer.say(msg, "Please provide some content for the command!")?;
            return Ok(())
        }
    };

    sqlx::query!("INSERT INTO commands(channel_name, name, content)
            VALUES($1, $2, $3)
            ON CONFLICT (channel_name, name)
            DO UPDATE
            SET content = EXCLUDED.content", 
            msg.channel(), command_name, command_content)
        .execute(&pool).await?;

    writer.say(msg, &format!("Command {} successfully updated!", command_name))?;

    Ok(())
}

async fn list(bot: &Bot, msg: &Privmsg<'_>) -> KingResult {
    let mut writer = bot.writer.lock().await;
    let pool = bot.data.read().await
        .get::<ConnectionPool>().cloned().unwrap();

    let cmd_data = sqlx::query!("SELECT name, content FROM commands WHERE channel_name = $1", msg.channel())
        .fetch_all(&pool).await?;

    if cmd_data.len() <= 0 {
        writer.say(msg, "There are no commands in this channel! Maybe add some?")?;

        return Ok(())
    }

    writer.encode_many(&[
        whisper(msg.name(), "Command List"),
        whisper(msg.name(), "---------------------------------------")
    ]).await?;

    for cmd in cmd_data {
        let message = format!("- {}: {}", cmd.name, cmd.content);
        let w = whisper(msg.name(), &message);

        writer.encode(w).await?;
    }

    writer.encode(whisper(msg.name(), "---------------------------------------")).await?;

    Ok(())
}

async fn remove(bot: &Bot, msg: &Privmsg<'_>, info: CommandInfo) -> KingResult<()> {
    let mut writer = bot.writer.lock().await;

    if !msg.is_moderator() {
        writer.say(msg, "You can't access this command because you aren't a moderator!")?;
        return Ok(())
    }

    let command_name = match info.get(1) {
        Some(name) => name, 
        None => {
            writer.say(msg, "Please provide a name for the command!")?;

            return Ok(())
        }
    };

    let pool = bot.data.read().await
        .get::<ConnectionPool>().cloned().unwrap();

    match sqlx::query!("DELETE FROM commands WHERE channel_name = $1 AND name = $2", msg.channel(), command_name)
        .execute(&pool).await {
            Ok(_) => writer.say(msg, &format!("Sucessfully removed command {}!", command_name))?,
            Err(_e) => writer.say(msg, &format!("Command {} doesn't exist in the database! Consider adding it?", command_name))?
    };

    Ok(())
}
