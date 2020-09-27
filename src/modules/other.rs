use twitchchat::{messages::Privmsg, PrivmsgExt};
use hook::hook;

use crate::structures::{Bot, CommandInfo, KingResult, cmd_data::ConnectionPool};

#[hook]
pub async fn ping(bot: &Bot, msg: &Privmsg<'_>, _: CommandInfo) -> KingResult {
    let mut writer = bot.writer.lock().await;

    writer.say(msg, "Pong!")?;

    Ok(())
}

#[hook]
pub async fn shoutout(bot: &Bot, msg: &Privmsg<'_>, mut info: CommandInfo) -> KingResult {
    let mut writer = bot.writer.lock().await;

    if !msg.is_moderator() {
        writer.say(msg, "You can't access this command because you aren't a moderator!")?;

        return Ok(())
    }

    let param = match info.get(0) {
        Some(username) => username,
        None => {
            writer.say(msg, "Please provide a username!")?;

            return Ok(())
        }
    };

    let pool = bot.data.read().await
        .get::<ConnectionPool>().cloned().unwrap();

    if param == "message" {
        let message = match info.join(1) {
            Ok(message) => message,
            Err(_) => {
                writer.say(msg, "Please provide a message!")?;

                return Ok(())
            }
        };

        sqlx::query!("INSERT INTO messages VALUES($1, $2)
                    ON CONFLICT (channel_name)
                    DO UPDATE
                    SET shoutout = EXCLUDED.shoutout",
                    msg.channel(), message)
            .execute(&pool).await?;

        writer.say(msg, "Shoutout message successfully set!")?;
    } else {
        let so_data = sqlx::query!("SELECT shoutout FROM messages WHERE channel_name = $1", msg.channel())
            .fetch_optional(&pool).await?;
            
        let message = match so_data {
            Some(so_data) => {
                match so_data.shoutout {
                    Some(raw_message) => {
                        raw_message
                            .replace("{user}", &param)
                            .replace("{url}", &format!("twitch.tv/{}", param))
                    },
                    None => {
                        format!(
                            "Shoutout! Feel free to watch and follow {0} on his amazing channel! twitch.tv/{0}", param)
                    }
                }
            },
            None => {
                format!(
                    "Shoutout! Feel free to watch and follow {0} on his amazing channel! twitch.tv/{0}", param)
            }
        };
                
        writer.say(msg, &message)?;
    }

    Ok(())
}
