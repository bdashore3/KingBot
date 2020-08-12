use hook::*;
use crate::{helpers::string_renderer, structures::{Bot, BotResult, cmd_data::ConnectionPool}};
use twitchchat::{messages, Writer};
use sqlx::PgPool;
use rand::{prelude::StdRng, Rng, SeedableRng};

#[hook]
pub async fn dispatch_quote(bot: &Bot, msg: &messages::Privmsg<'_>) -> BotResult<()> {
    let mut writer = bot.writer.lock().await;
    let length = string_renderer::get_command_length(&*msg.data);

    let data = bot.data.read().await;
    let pool = data.get::<ConnectionPool>().unwrap();

    if length == 1 {
        retrieve(pool, msg, &mut writer, true).await?;
        return Ok(())
    }

    let subcommand = string_renderer::get_message_word(&*msg.data, 1).unwrap();

    match subcommand {
        "add" => add(pool.as_ref(), msg, &mut writer).await?,
        "remove" => remove(pool.as_ref(), msg, &mut writer).await?,
        "list" => list(pool.as_ref(), msg, &mut writer).await?,
        _ => retrieve(pool, msg, &mut writer, false).await?
    }

    Ok(())
}

pub async fn add(pool: &PgPool, msg: &messages::Privmsg<'_>, writer: &mut Writer) -> BotResult<()> {
    if string_renderer::get_command_length(&*msg.data) < 4 {
        writer.privmsg(&msg.channel, "Please provide an alias AND content for the quote!").await?;
        return Ok(())
    }

    let quote_phrase = match string_renderer::get_message_word(&*msg.data, 2) {
        Ok(phrase) => phrase,
        Err(_e) => {
            writer.privmsg(&msg.channel, "Please provide an alias for the quote!").await?;
            return Ok(())
        }
    };
    
    if quote_phrase.parse::<u64>().is_ok() {
        writer.privmsg(&msg.channel, "You can't set a number as an alias!").await?;
        return Ok(());
    }

    let joined_string = match string_renderer::join_string(&*msg.data, 2) {
        Ok(joined) => joined,
        Err(_e) => {
            writer.privmsg(&msg.channel, "Please provide some content for the quote's alias!").await?;
            return Ok(())
        }
    };

    match sqlx::query!("INSERT INTO quotes VALUES ($1, $2, $3)", &*msg.channel, quote_phrase, joined_string)
        .execute(pool).await {
            Ok(_) => writer.privmsg(&msg.channel, &format!("Quote {} sucessfully added!", quote_phrase)).await?,
            Err(_e) => writer.privmsg(&msg.channel, &format!("Quote {} already exists! Please remove it first or use a different alias!", quote_phrase)).await?
    };

    Ok(())
}

pub async fn remove(pool: &PgPool, msg: &messages::Privmsg<'_>, writer: &mut Writer) -> BotResult<()> {
    if !msg.is_moderator() {
        writer.privmsg(&msg.channel, "You can't remove quotes because you aren't a moderator").await?;
        return Ok(())
    }

    let quote_phrase = match string_renderer::get_message_word(&*msg.data, 2) {
        Ok(phrase) => phrase,
        Err(_e) => {
            writer.privmsg(&msg.channel, "Please provide an alias to remove the quote (not a number)! Check the list?").await?;
            return Ok(())
        }
    };

    match sqlx::query!("DELETE FROM quotes WHERE channel_name = $1 AND alias = $2", &*msg.channel, quote_phrase)
        .execute(pool).await {
            Ok(_) => writer.privmsg(&msg.channel, &format!("Sucessfully removed quote {}!", quote_phrase)).await?,
            Err(_e) => writer.privmsg(&msg.channel, &format!("Quote {} doesn't exist in the database! Consider adding it?", quote_phrase)).await?
    };

    Ok(())
}

pub async fn list(pool: &PgPool, msg: &messages::Privmsg<'_>, writer: &mut Writer) -> BotResult<()> {
    let quote_data = sqlx::query!("SELECT alias, content FROM quotes WHERE channel_name = $1", &*msg.channel)
    .fetch_all(pool).await?;

    if quote_data.len() <= 0 {
        writer.privmsg(&msg.channel, "There are no quotes in this channel! Maybe add some?").await?;
        return Ok(())
    }

    writer.whisper(&msg.name, "Quote List").await?;
    writer.whisper(&msg.name, "---------------------------------------").await?;

    for (position, quote) in quote_data.iter().enumerate() {
        writer.whisper(&msg.name, &format!("{}. {}: {}", position + 1, quote.alias, quote.content)).await?;
    }

    writer.whisper(&msg.name, "---------------------------------------").await?;

    Ok(())
}

pub async fn retrieve(pool: &PgPool, msg: &messages::Privmsg<'_>, writer: &mut Writer, random: bool) -> BotResult<()> {
    let quote_data = sqlx::query!("SELECT alias, content FROM quotes WHERE channel_name = $1", &*msg.channel)
        .fetch_all(pool).await?;

    if quote_data.len() <= 0 {
        writer.privmsg(&msg.channel, "There are no quotes in this channel! Maybe add some?").await?;
        return Ok(())
    }

    if random {
        let mut rng = StdRng::from_entropy();
        let index = rng.gen_range(0, quote_data.len());
        let quote = quote_data.get(index).unwrap();
        writer.privmsg(&msg.channel, &quote.content).await?;
        return Ok(())
    }

    let quote_string = string_renderer::get_message_word(&*msg.data, 1).unwrap();

    match quote_string.parse::<isize>() {
        Ok(quote_num) => {
            if quote_num < 1 {
                writer.privmsg(&msg.channel, "Please enter a number greater than 0!").await?;
                return Ok(())
            }

            match quote_data.get(quote_num as usize - 1) {
                Some(quote) => writer.privmsg(&msg.channel, &quote.content).await?,
                None => writer.privmsg(&msg.channel, "That number doesn't point to a quote. Check the list!").await?
            };
        },
        Err(_e) => {
            match quote_data.iter().position(|x| x.alias == quote_string) {
                Some(index) => {
                    let quote = quote_data.get(index).unwrap();
                    writer.privmsg(&msg.channel, &quote.content).await?;
                },
                None => writer.privmsg(&msg.channel, "That alias doesn't point to a quote. Check the list!").await?
            };

        }
    };


    Ok(())
}