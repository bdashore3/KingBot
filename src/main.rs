mod structures;
mod modules;
mod helpers;

use std::env;
use helpers::credentials_helper;
use structures::{KingResult, Bot};
use tokio::sync::Mutex;
use twitchchat::{
    connector::TokioConnector as Connector, messages::{self, Commands::*},
    runner::{AsyncRunner, Status},
    UserConfig,
};
use helpers::command_utils;

async fn event_handler(bot: &Bot, event: messages::Commands<'_>) -> KingResult {
    // All sorts of messages
    match event {
        // This is the one users send to channels
        Privmsg(msg) => {
            if let Some(command) = bot.commands.get(msg.data()) {
                let info = command_utils::generate_info(&msg);

                command(&bot, &msg, info).await?;
            }
        },
        Ready(_) => {
            println!("Bot is now ready to work!");
        }
        _ => {}
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
    let (user_config, channels) = get_info()?;

    let runner = connect(&user_config, &channels).await?;

    let command_map = command_utils::register_commands();

    let bot = Bot {
        commands: command_map,
        writer: Mutex::new(runner.writer())
    };

    main_loop(bot, runner).await
}

fn get_info() -> KingResult<(twitchchat::UserConfig, Vec<String>)> {
    let args: Vec<String> = env::args().collect();
    let creds = credentials_helper::read_creds(&args[1]).unwrap();

    let config = UserConfig::builder()
        .name(creds.bot_username)
        .token(creds.bot_token)
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
