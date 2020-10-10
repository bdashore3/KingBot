use twitchchat::commands::privmsg;

use crate::{modules::lurks::clear_lurks_internal, structures::{Bot, KingResult, webhooks::*}};

pub async fn handle_stream(bot: &Bot, stream_data: Option<&StreamEvent>) -> KingResult {
    let mut writer = bot.writer.lock().await;

    if let Some(data) = stream_data {
        let msg = format!("User went live!");

        let channel = format!("#{}", data.user_name);

        writer.encode(privmsg(&channel, &msg)).await?;
    } else {
        if let Err(e) = clear_lurks_internal(bot).await {
            eprintln!("There was an error! {}", e);
        }

        println!("User didn't go live!");
    }

    Ok(())
}

pub async fn handle_follow(bot: &Bot, follow_data: &FollowEvent) -> KingResult {
    let mut writer = bot.writer.lock().await;

    let msg =  format!("New follow from: {}", follow_data.from_name);

    let channel = format!("#{}", follow_data.to_name);

    writer.encode(privmsg(&channel, &msg)).await?;

    Ok(())
}
