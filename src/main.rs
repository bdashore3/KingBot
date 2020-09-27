mod structures;
mod modules;
mod helpers;
mod webhooks;
mod api;

use std::{collections::HashMap, env, sync::Arc};
use credentials_helper::Credentials;
use dashmap::DashMap;
use helpers::{credentials_helper, database_helper};
use structures::{cmd_data::*, KingResult, Bot};
use tokio::sync::{Mutex, RwLock};
use twitchchat::{PrivmsgExt, UserConfig, connector::TokioConnector as Connector, messages::{self, Commands::*}, runner::{AsyncRunner, Status}};
use helpers::command_utils;
use typemap_rev::TypeMap;

async fn event_handler(bot: &Bot, event: messages::Commands<'_>) -> KingResult {
    match event {
        Privmsg(msg) => {
            let prefix = command_utils::fetch_prefix(bot, &msg).await;

            if msg.data()[..prefix.len()] == prefix {
                let word = command_utils::fetch_command(&msg, &prefix);

                if let Some(command) = bot.commands.get(word) {
                    let info = command_utils::generate_info(&msg, word);
    
                    command(&bot, &msg, info).await?;
                } else {
                    unrecognized_command(bot, &msg, word).await?
                }
            }
        },
        Ready(_) => {
            println!("Bot is now ready to work!");
        }
        _ => {}
    }

    Ok(())
}

async fn unrecognized_command(bot: &Bot, msg: &messages::Privmsg<'_>, command: &str) -> KingResult {
    let pool = bot.data.read().await
        .get::<ConnectionPool>().cloned().unwrap();
    let command_data = sqlx::query!("SELECT content FROM commands WHERE channel_name = $1 AND name = $2",
            msg.channel(), command)
        .fetch_optional(&pool).await?;

    if let Some(command_data) = command_data {
        let mut writer = bot.writer.lock().await;
        let content = command_data.content
            .replace("{user}", msg.name())
            .replace("{channel}", &msg.channel()[1..]);
        writer.say(&msg, &content)?;
    }

    Ok(())
}

async fn main_loop(bot: Bot, mut runner: AsyncRunner) -> KingResult {
    loop {
        match runner.next_message().await? {
            Status::Message(event) => {
                event_handler(&bot, event).await?;
            }

            Status::Eof => {
                println!("we got a 'normal' eof");
                break;
            }

            _ => {}
        }
    }

    Ok(())
}

#[tokio::main]
async fn main() -> KingResult {
    let args: Vec<String> = env::args().collect();
    let creds = credentials_helper::read_creds(&args[1]).unwrap();

    let (user_config, channels) = get_info(&creds)?;

    let runner = connect(&user_config, &channels).await?;

    let command_map = command_utils::register_commands();

    let pool = database_helper::obtain_db_pool(&creds.db_connection).await?;
    let prefixes = database_helper::fetch_prefixes(&pool).await?;
    
    let mut pub_creds = HashMap::new();
    pub_creds.insert("default prefix".to_owned(), creds.default_prefix.clone());

    let bot = Bot {
        commands: command_map,
        writer: Arc::new(Mutex::new(runner.writer())),
        data: Arc::new(RwLock::new(TypeMap::new())),
        channel: creds.channel.clone()
    };

    {
        let mut data = bot.data.write().await;

        data.insert::<ConnectionPool>(pool);
        data.insert::<PrefixMap>(Arc::new(prefixes));
        data.insert::<PubCreds>(Arc::new(pub_creds));
        data.insert::<LurkTimes>(Arc::new(DashMap::new()));
        data.insert::<IntervalMap>(Arc::new(RwLock::new(Vec::new())));
    }

    let bot_clone = bot.clone();

    tokio::spawn(async move {
        let _ = webhooks::rocket::start_rocket(bot_clone, creds.api_id, creds.api_token).await;
    });

    main_loop(bot, runner).await
}

fn get_info(creds: &Credentials) -> KingResult<(twitchchat::UserConfig, Vec<String>)> {
    let config = UserConfig::builder()
        .name(&creds.bot_username)
        .token(&creds.bot_token)
        .enable_all_capabilities()
        .build()?;

    let channels = creds.channel
        .split(',')
        .map(ToString::to_string)
        .collect();

    Ok((config, channels))
}

async fn connect(user_config: &UserConfig, channels: &[String]) -> KingResult<AsyncRunner> {
    let connector = Connector::twitch();

    let mut runner = AsyncRunner::connect(connector, user_config).await?;
    println!("Connected to twitch!");

    for channel in channels {
        println!("attempting to join '{}'", channel);
        runner.join(&channel).await?;
        println!("joined '{}'!", channel);
    }

    Ok(runner)
}
