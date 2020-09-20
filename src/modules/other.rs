use twitchchat::{messages::Privmsg, PrivmsgExt};
use hook::hook;

use crate::structures::{Bot, KingResult};

#[hook]
pub async fn ping(bot: &Bot, msg: &Privmsg<'_>) -> KingResult {
    println!("Ping called");
    let mut writer = bot.writer.lock().await;

    writer.say(msg, "Pong!")?;

    Ok(())
}
