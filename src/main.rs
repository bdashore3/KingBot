mod handlers;
mod structures;
mod commands;
mod helpers;

use twitchchat::*;
use std::{
    env,
    collections::HashMap, sync::Arc
};
use typemap_rev::TypeMap;
use tokio::sync::{Mutex, RwLock};
use crate::{
    handlers::command_handler,
    helpers::{
        credentials_helper,
        database_helper
    },
    structures::*,
    cmd_data::*
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
    let data = Arc::new(RwLock::new(TypeMap::new()));

    let pool = database_helper::obtain_db_pool(creds.db_connection).await?;

    let mut pub_creds = HashMap::new();
    pub_creds.insert("default prefix".to_string(), creds.default_prefix);

    let channel = creds.channel;

    {
        let mut data = data.write().await;
        data.insert::<PubCreds>(Arc::new(pub_creds));
        data.insert::<ConnectionPool>(Arc::new(pool));
    }

    command_handler::insert_commands(&mut command_map);

    let bot = Bot {
        writer: Mutex::new(control.writer().clone()),
        control,
        command_map,
        start: std::time::Instant::now(),
        data
    }
    .run(dispatcher, channel);

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