use crate::structures::*;
use twitchchat::messages;
use hook::*;

#[hook]
pub async fn ping(bot: &mut Bot, msg: &messages::Privmsg<'_>) -> BotResult<()> {
    bot.writer.privmsg(&msg.channel, "Pong!").await?;

    Ok(())
}

#[hook]
pub async fn uptime(bot: &mut Bot, msg: &messages::Privmsg<'_>) -> BotResult<()> {
    let dur = std::time::Instant::now() - bot.start;
    let resp = format!("I've been running for.. {:.2?}.", dur);
    bot.writer.privmsg(&msg.channel, &resp).await?;

    Ok(())
}