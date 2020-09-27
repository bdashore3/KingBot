use twitchchat::commands::privmsg;

use crate::structures::{Bot, KingResult, webhooks::*};

pub async fn handle_stream(bot: &Bot, stream_data: Option<&StreamEvent>) -> KingResult {
    let mut writer = bot.writer.lock().await;

    if stream_data.is_some() {
        let msg = format!("User went live!");

        writer.encode(privmsg(&bot.channel, &msg)).await?;
    } else {
        println!("User didn't go live!");
    }

    Ok(())
}

pub async fn handle_follow(bot: &Bot, follow_data: &FollowEvent) -> KingResult {
    let mut writer = bot.writer.lock().await;

    let msg =  format!("New follow from: {}", follow_data.from_name);

    writer.encode(privmsg(&bot.channel, &msg)).await?;

    Ok(())
}
