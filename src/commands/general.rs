use crate::structures::*;
use twitchchat::messages;
use hook::*;

#[hook]
pub async fn ping(bot: &Bot, msg: &messages::Privmsg<'_>) -> BotResult<()> {
    println!("Command called");
    let mut writer = bot.writer.lock().await;
    writer.privmsg(&msg.channel, "Pong!").await?;

    Ok(())
}

#[hook]
pub async fn uptime(bot: &Bot, msg: &messages::Privmsg<'_>) -> BotResult<()> {
    let mut writer = bot.writer.lock().await;
    let dur = std::time::Instant::now() - bot.start;
    let resp = format!("I've been running for.. {:.2?}.", dur);
    writer.privmsg(&msg.channel, &resp).await?;

    Ok(())
}