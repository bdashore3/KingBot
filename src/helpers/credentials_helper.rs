use serde::{Deserialize, Serialize};
use std::fs;
use std::io::BufReader;
use crate::structures::BotResult;

#[derive(Serialize, Deserialize)]
pub struct Credentials {
    pub bot_username: String,
    pub bot_token: String,
    pub default_prefix: String,
    pub channel: String,
    pub db_connection: String
}

pub fn read_creds(path: &str) -> BotResult<Credentials> {
    let file = fs::File::open(path)?;
    let reader = BufReader::new(file);

    let info: Credentials = serde_json::from_reader(reader).unwrap();

    Ok(info)
}