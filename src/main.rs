mod handlers;
mod structures;
mod commands;
mod helpers;

use twitchchat::*;
use crate::structures::*;
use std::{
    env,
    collections::HashMap
};
use crate::{
    handlers::command_handler,
    helpers::credentials_helper
};

fn fetch_info(path: &str) -> (String, String) {
    let creds = credentials_helper::read_creds(path).unwrap();
    (creds.bot_username, format!("oauth:{}", creds.bot_token))
}

#[tokio::main]
async fn main() -> BotResult<()> {
    let args: Vec<String> = env::args().collect();
    let creds = credentials_helper::read_creds(&args[1]).unwrap();

    let dispatcher = Dispatcher::new();
    let (mut runner, mut control) = Runner::new(dispatcher.clone());

    let mut command_map = HashMap::new();

    let channel = creds.channel;

    command_handler::insert_commands(&mut command_map);

    let bot = Bot {
        writer: control.writer().clone(),
        control,
        default_prefix: creds.default_prefix.to_string(),
        start: std::time::Instant::now()
    }
    .run(dispatcher, channel, command_map);

    let connector = twitchchat::Connector::new(|| async move {
        let args: Vec<String> = env::args().collect();
        let info = fetch_info(&args[1]);
        let (nick, pass) = (info.0, info.1);
        twitchchat::native_tls::connect_easy(&nick, &pass).await
    });

    let done = runner.run_with_retry(connector, twitchchat::RetryStrategy::on_timeout);

    tokio::select! {
        _ = bot => { eprintln!("done running the bot") }
        status = done => {
            match status {
                Ok(()) => { eprintln!("we're done") }
                Err(err) => { eprintln!("error running: {}", err) }
            }
        }
    }

    Ok(())
}