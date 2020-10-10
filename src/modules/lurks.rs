use crate::{helpers::command_utils, structures::{Bot, CommandInfo, KingResult, cmd_data::LurkTimes}};
use messages::Privmsg;
use twitchchat::{PrivmsgExt, messages};
use hook::*;
use std::{time::{UNIX_EPOCH, SystemTime}};

#[hook]
pub async fn dispatch_lurk(bot: &Bot, msg: &messages::Privmsg<'_>, info: CommandInfo) -> KingResult {
    match info.get(0) {
        Some(subcommand) => {
            match subcommand.as_str() {
                "cancel" => cancel_lurk(bot, msg).await?,
                "clear" => clear_lurks(bot, msg).await?,
                _ => {}
            }
        }
        None => fetch_lurk(bot, msg).await?
    }

    Ok(())
}

async fn fetch_lurk(bot: &Bot, msg: &messages::Privmsg<'_>) -> KingResult {
    let mut writer = bot.writer.lock().await;
    let lurk_map = bot.data.read().await
        .get::<LurkTimes>().cloned().unwrap();

    let start = SystemTime::now();
    let since_epoch = start.duration_since(UNIX_EPOCH).expect("Time went backwards?").as_secs();

    let display_name_option = msg.display_name();
    let display_name = match display_name_option {
        Some(name) => name,
        None => msg.name()
    };

    if let Some(initial_time) = lurk_map.get(&msg.name().to_owned()) {
        let diff_time = since_epoch - initial_time.value();
        let hours = command_utils::get_time(&diff_time, "hours").unwrap();
        let minutes = command_utils::get_time(&diff_time, "minutes").unwrap();
        let seconds = command_utils::get_time(&diff_time, "seconds").unwrap();

        writer.say(msg, &format!("{} has been lurking for {}h {}m {}s", display_name, hours, minutes, seconds))?;
    } else {
        lurk_map.insert(msg.name().to_owned(), since_epoch);
        
        writer.say(msg, &format!("{} is now lurking! Try using the command again!", display_name))?;
    }

    Ok(())
}

pub async fn cancel_lurk(bot: &Bot, msg: &Privmsg<'_>) -> KingResult {
    let mut writer = bot.writer.lock().await;
    let lurk_map = bot.data.read().await
        .get::<LurkTimes>().cloned().unwrap();

    let display_name_option = msg.display_name();
    let display_name = match display_name_option {
        Some(name) => name,
        None => msg.name()
    };

    if lurk_map.remove(&msg.name().to_string()) {
        writer.say(msg, &format!("{} lurk was sucessfully cancelled! Welcome back!", display_name))?;
    } else {
        writer.say(msg, &format!("{} is not currently lurking. Use lurk to start the clock!", display_name))?;
    }

    Ok(())
}

pub async fn clear_lurks(bot: &Bot, msg: &Privmsg<'_>) -> KingResult {
    let mut writer = bot.writer.lock().await;

    if !msg.is_moderator() {
        writer.say(msg, "You can't access this command because you aren't a moderator!")?;
        return Ok(())
    }

    match clear_lurks_internal(bot).await {
        Ok(_) => {
            writer.say(msg, "Lurks sucessfully cleared!")?;
        },
        Err(e) => {
            writer.say(msg, &format!("Looks like there was an error! {}", e))?;
        }
    };

    Ok(())
}

pub async fn clear_lurks_internal(bot: &Bot) -> KingResult {
    let lurk_map = bot.data.read().await
        .get::<LurkTimes>().cloned().unwrap();

    lurk_map.clear();

    Ok(())
}
