use std::time::Duration;

use hook::hook;
use messages::Privmsg;
use tokio::time::delay_for;
use twitchchat::{IntoOwned, PrivmsgExt, commands::whisper, messages};
use crate::{ConnectionPool, structures::{Bot, CommandInfo, IntervalInfo, KingResult, cmd_data::IntervalMap}};

#[hook]
pub async fn dispatch_interval(bot: &Bot, msg: &Privmsg<'_>, info: CommandInfo) -> KingResult {
    match &info.get(0) {
        Some(word) => {
            match word.as_str() {
                "add" => add(bot, msg, info).await?,
                "remove" => remove(bot, msg, info).await?,
                "list" => list(bot, msg).await?,
                "stop" => stop(bot, msg, info).await?,
                "start" => {
                    let bot_clone = (*bot).clone();
                    let msg_clone = msg.into_owned();
                    tokio::spawn(async move {
                        if let Err(e) = start(bot_clone, msg_clone, info).await {
                            eprintln!("Error in starting interval! {}", e);
                        };
                    });
                },
                _ => {}
            }
        },
        None => {
            let mut writer = bot.writer.lock().await;

            writer.say(msg, "Please enter a subcommand!")?;
        }
    }

    Ok(())
}

async fn start(bot: Bot, msg: Privmsg<'_>, info: CommandInfo) -> KingResult {
    let mut writer = bot.writer.lock().await;

    let (pool, interval_lock) = {
        let data = bot.data.read().await;
        let pool = data.get::<ConnectionPool>().cloned().unwrap();
        let interval_lock = data.get::<IntervalMap>().cloned().unwrap();

        (pool, interval_lock)
    };

    let alias = match info.get(1) {
        Some(alias) => alias,
        None => {
            writer.say(&msg, "Please provide a valid interval alias! Check the list?")?;
            
            return Ok(())
        }
    };

    let time = match info.get(2) {
        Some(time) => match time.parse::<u64>() {
            Ok(time) => time,
            Err(_) => {
                writer.say(&msg, "Please provide a valid interval time!")?;
            
                return Ok(())
            }
        },
        None => {
            writer.say(&msg, "Please provide a valid interval time!")?;
            
            return Ok(())
        }
    };

    let interval_info = IntervalInfo {
        channel: msg.channel().to_owned(),
        alias
    };

    let interval_data = sqlx::query!("SELECT message FROM intervals WHERE channel_name = $1 AND alias = $2",
            interval_info.channel, interval_info.alias)
        .fetch_one(&pool).await?;

    let interval_map = interval_lock.read().await;

    loop {
        delay_for(Duration::from_secs(time)).await;

        if interval_map.contains(&interval_info) {
            return Ok(())
        } 

        writer.say(&msg, &interval_data.message)?;
    }
}

async fn stop(bot: &Bot, msg: &Privmsg<'_>, info: CommandInfo) -> KingResult {
    let mut writer = bot.writer.lock().await;

    let alias = match info.get(1) {
        Some(alias) => alias,
        None => {
            writer.say(msg, "Please provide a valid interval alias! Check the list?")?;

            return Ok(())
        }
    };

    let interval_lock = bot.data.read().await
        .get::<IntervalMap>().cloned().unwrap();

    let mut interval_map = interval_lock.write().await;

    let interval_info = IntervalInfo {
        channel: msg.channel().to_owned(),
        alias
    };

    interval_map.push(interval_info);

    Ok(())
}

async fn add(bot: &Bot, msg: &Privmsg<'_>, mut info: CommandInfo) -> KingResult {
    let mut writer = bot.writer.lock().await;
    let pool = bot.data.read().await
        .get::<ConnectionPool>().cloned().unwrap();

    if !msg.is_moderator() {
        writer.say(msg, "You can't access this command because you aren't a moderator!")?;
        return Ok(())
    }

    let interval_phrase = match info.get(1) {
        Some(phrase) => phrase,
        None => {
            writer.say(msg, "Please provide an alias for the quote!")?;
            return Ok(())
        }
    };

    let joined_string = match info.join(1) {
        Ok(joined) => joined,
        Err(_e) => {
            writer.say(msg, "Please provide some content for the quote's alias!")?;
            return Ok(())
        }
    };

    match sqlx::query!("INSERT INTO intervals VALUES ($1, $2, $3)", msg.channel(), interval_phrase, joined_string)
        .execute(&pool).await {
            Ok(_) => writer.say(msg, &format!("Interval {} sucessfully added!", interval_phrase))?,
            Err(_e) => writer.say(msg, &format!("Interval {} already exists! Please remove it first or use a different alias!", interval_phrase))?
    };

    Ok(())
}

async fn remove(bot: &Bot, msg: &messages::Privmsg<'_>, info: CommandInfo) -> KingResult {
    let mut writer = bot.writer.lock().await;
    let pool = bot.data.read().await
        .get::<ConnectionPool>().cloned().unwrap();

    if !msg.is_moderator() {
        writer.say(msg, "You can't access this command because you aren't a moderator!")?;

        return Ok(())
    }

    let interval_name = match info.get(1) {
        Some(name) => name, 
        None => {
            writer.say(msg, "Please provide an alias for the interval!")?;

            return Ok(())
        }
    };

    match sqlx::query!("DELETE FROM intervals WHERE channel_name = $1 AND alias = $2", msg.channel(), interval_name)
        .execute(&pool).await {
            Ok(_) => writer.say(msg, &format!("Sucessfully removed interval {}!", interval_name))?,
            Err(_e) => writer.say(msg, &format!("Interval {} doesn't exist in the database! Consider adding it?", interval_name))?
    };

    Ok(())
}

pub async fn list(bot: &Bot, msg: &messages::Privmsg<'_>) -> KingResult {
    let mut writer = bot.writer.lock().await;
    let pool = bot.data.read().await
        .get::<ConnectionPool>().cloned().unwrap();

    let interval_data = sqlx::query!("SELECT alias, message FROM intervals WHERE channel_name = $1", &msg.channel())
        .fetch_all(&pool).await?;

    if interval_data.len() <= 0 {
        writer.say(msg, "There are no quotes in this channel! Maybe add some?")?;
        return Ok(())
    }

    writer.encode_many(&[
        whisper(msg.name(), "Interval List"),
        whisper(msg.name(), "---------------------------------------")
    ]).await?;

    for interval in interval_data.iter() {
        let message = format!("{}: {}", interval.alias, interval.message);
        let w = whisper(msg.name(), &message);

        writer.encode(&w).await?;
    }

    writer.encode(whisper(msg.name(), "---------------------------------------")).await?;

    Ok(())
}
