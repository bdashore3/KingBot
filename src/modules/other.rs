use twitchchat::{messages::Privmsg, PrivmsgExt};
use hook::hook;

use crate::structures::{Bot, KingResult, CommandInfo};

#[hook]
pub async fn ping(bot: &Bot, msg: &Privmsg<'_>, _: CommandInfo) -> KingResult {
    let mut writer = bot.writer.lock().await;

    writer.say(msg, "Pong!")?;

    Ok(())
}
