use hook::hook;
use messages::Privmsg;
use twitchchat::{
    messages,
    PrivmsgExt,
    commands::whisper
};
use rand::{prelude::StdRng, Rng, SeedableRng};
use crate::{
    ConnectionPool,
    structures::{
        Bot,
        KingResult,
        CommandInfo,
    }
};

#[hook]
pub async fn dispatch_quote(bot: &Bot, msg: &Privmsg<'_>, info: CommandInfo) -> KingResult {
    if info.is_empty() {
        retrieve(bot, msg, info).await?;
        return Ok(())
    }

    let subcommand = &info.get(0).unwrap();

    match subcommand.as_str() {
        "add" => add(bot, msg, info).await?,
        "remove" => remove(bot, msg, info).await?,
        "list" => list(bot, msg).await?,
        _ => retrieve(bot, msg, info).await?
    }

    Ok(())
}

pub async fn add(bot: &Bot, msg: &messages::Privmsg<'_>, mut info: CommandInfo) -> KingResult {
    let mut writer = bot.writer.lock().await;
    let pool = bot.data.read().await
        .get::<ConnectionPool>().cloned().unwrap();

    let quote_phrase = match info.get(1) {
        Some(phrase) => phrase,
        None => {
            writer.say(msg, "Please provide an alias for the quote!")?;
            return Ok(())
        }
    };
    
    if quote_phrase.parse::<u64>().is_ok() {
        writer.say(msg, "You can't set a number as an alias!")?;
        return Ok(());
    }

    let joined_string = match info.join(1) {
        Ok(joined) => joined,
        Err(_e) => {
            writer.say(msg, "Please provide some content for the quote's alias!")?;
            return Ok(())
        }
    };

    match sqlx::query!("INSERT INTO quotes VALUES ($1, $2, $3)", msg.channel(), quote_phrase, joined_string)
        .execute(&pool).await {
            Ok(_) => writer.say(msg, &format!("Quote {} sucessfully added!", quote_phrase))?,
            Err(_e) => writer.say(msg, &format!("Quote {} already exists! Please remove it first or use a different alias!", quote_phrase))?
    };

    Ok(())
}

pub async fn remove(bot: &Bot, msg: &messages::Privmsg<'_>, info: CommandInfo) -> KingResult {
    let mut writer = bot.writer.lock().await;
    let pool = bot.data.read().await
        .get::<ConnectionPool>().cloned().unwrap();

    if !msg.is_moderator() {
        writer.say(msg, "You can't remove quotes because you aren't a moderator")?;
        return Ok(())
    }

    let quote_phrase = match info.words.get(1) {
        Some(phrase) => phrase,
        None => {
            writer.say(msg, "Please provide an alias to remove the quote (not a number)! Check the list?")?;
            return Ok(())
        }
    };

    match sqlx::query!("DELETE FROM quotes WHERE channel_name = $1 AND alias = $2", msg.channel(), quote_phrase)
        .execute(&pool).await {
            Ok(_) => writer.say(msg, &format!("Sucessfully removed quote {}!", quote_phrase))?,
            Err(_e) => writer.say(msg, &format!("Quote {} doesn't exist in the database! Consider adding it?", quote_phrase))?
    }

    Ok(())
}

pub async fn list(bot: &Bot, msg: &messages::Privmsg<'_>) -> KingResult {
    let mut writer = bot.writer.lock().await;
    let pool = bot.data.read().await
        .get::<ConnectionPool>().cloned().unwrap();

    let quote_data = sqlx::query!("SELECT alias, content FROM quotes WHERE channel_name = $1", &msg.channel())
        .fetch_all(&pool).await?;

    if quote_data.len() <= 0 {
        writer.say(msg, "There are no quotes in this channel! Maybe add some?")?;
        return Ok(())
    }

    writer.encode_many(&[
        whisper(msg.name(), "Quote List"),
        whisper(msg.name(), "---------------------------------------")
    ]).await?;

    for (position, quote) in quote_data.iter().enumerate() {
        let message = format!("{}. {}: {}", position + 1, quote.alias, quote.content);
        let w = whisper(msg.name(), &message);

        writer.encode(&w).await?;
    }

    writer.encode(whisper(msg.name(), "---------------------------------------")).await?;

    Ok(())
}

pub async fn retrieve(bot: &Bot, msg: &messages::Privmsg<'_>, info: CommandInfo) -> KingResult {
    let mut writer = bot.writer.lock().await;
    let pool = bot.data.read().await
        .get::<ConnectionPool>().cloned().unwrap();

    let quote_data = sqlx::query!("SELECT alias, content FROM quotes WHERE channel_name = $1", msg.channel())
        .fetch_all(&pool).await?;

    if quote_data.len() <= 0 {
        writer.say(msg, "There are no quotes in this channel! Maybe add some?")?;
        return Ok(())
    }

    if info.is_empty() {
        let mut rng = StdRng::from_entropy();
        let index = rng.gen_range(0, quote_data.len());
        let quote = quote_data.get(index).unwrap();
        writer.say(msg, &quote.content)?;

        return Ok(())
    }

    let quote_string = &info.words[0];

    match quote_string.parse::<isize>() {
        Ok(quote_num) => {
            if quote_num <= 0 {
                writer.say(msg, "Please enter a number greater than 0!")?;
                return Ok(())
            }

            match quote_data.get(quote_num as usize - 1) {
                Some(quote) => writer.say(&msg, &quote.content)?,
                None => writer.say(msg, "That number doesn't point to a quote. Check the list!")?
            }
        },
        Err(_e) => {
            match quote_data.iter().position(|x| &x.alias == quote_string) {
                Some(index) => {
                    let quote = quote_data.get(index).unwrap();
                    writer.say(&msg, &quote.content)?;
                },
                None => writer.say(msg, "That alias doesn't point to a quote. Check the list!")?
            }

        }
    }


    Ok(())
}
