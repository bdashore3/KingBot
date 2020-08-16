use crate::{helpers::string_renderer, helpers::command_utils, structures::{Bot, BotResult, cmd_data::LurkTimes}};
use twitchchat::{Writer, messages};
use hook::*;
use std::{sync::Arc, time::{UNIX_EPOCH, SystemTime}};
use dashmap::DashMap;

#[hook]
pub async fn dispatch_lurk(bot: &Bot, msg: &messages::Privmsg<'_>) -> BotResult<()> {
    let data = bot.data.read().await;
    let lurk_map = data.get::<LurkTimes>().unwrap();
    let mut writer = bot.writer.lock().await;

    match string_renderer::get_message_word(&msg.data, 1) {
        Ok(subcommand) => {
            match subcommand {
                "cancel" => cancel_lurk(lurk_map, msg, &mut writer).await?,
                "clear" => clear_lurks(lurk_map, msg, &mut writer).await?,
                _ => {}
            }
        }
        Err(_) => fetch_lurk(lurk_map, msg, &mut writer).await?
    }

    Ok(())
}

async fn fetch_lurk(lurk_map: &Arc<DashMap<String, u64>>, msg: &messages::Privmsg<'_>, writer: &mut Writer) -> BotResult<()> {
    let start = SystemTime::now();
    let since_epoch = start.duration_since(UNIX_EPOCH).expect("Time went backwards?").as_secs();

    let display_name_option = &msg.display_name();
    let display_name = match display_name_option {
        Some(name) => name,
        None => msg.name.as_ref()
    };

    if let Some(initial_time) = lurk_map.get(&msg.name.to_string()) {
        let diff_time = since_epoch - initial_time.value();
        let hours = command_utils::get_time(&diff_time, "hours").unwrap();
        let minutes = command_utils::get_time(&diff_time, "minutes").unwrap();
        let seconds = command_utils::get_time(&diff_time, "seconds").unwrap();

        writer.privmsg(&msg.channel, 
                &format!("{} has been lurking for {}h {}m {}s", display_name, hours, minutes, seconds)).await?;
    } else {
        lurk_map.insert(msg.name.to_string(), since_epoch);
        
        writer.privmsg(&msg.channel, &format!("{} is now lurking! Try using the command again!", display_name)).await?;
    }

    Ok(())
}

pub async fn cancel_lurk(lurk_map: &Arc<DashMap<String, u64>>, msg: &messages::Privmsg<'_>, writer: &mut Writer) -> BotResult<()> {
    let display_name_option = &msg.display_name();
    let display_name = match display_name_option {
        Some(name) => name,
        None => msg.name.as_ref()
    };

    if lurk_map.remove(&msg.name.to_string()) {
        writer.privmsg(&msg.channel, &format!("{} lurk was sucessfully cancelled! Welcome back!", display_name)).await?;
    } else {
        writer.privmsg(&msg.channel, &format!("{} is not currently lurking. Use lurk to start the clock!", display_name)).await?;
    }

    Ok(())
}

pub async fn clear_lurks(lurk_map: &Arc<DashMap<String, u64>>, msg: &messages::Privmsg<'_>, writer: &mut Writer) -> BotResult<()> {
    if !msg.is_moderator() {
        writer.privmsg(&msg.channel, "You can't access this command because you aren't a moderator!").await?;
        return Ok(())
    }

    lurk_map.clear();
    writer.privmsg(&msg.channel, "Lurks sucessfully cleared!").await?;

    Ok(())
}