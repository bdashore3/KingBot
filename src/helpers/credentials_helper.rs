use serde::{Deserialize, Serialize};
use std::fs;
use std::io::BufReader;
use crate::structures::KingResult;

#[derive(Serialize, Deserialize)]
pub struct Credentials {
    pub bot_username: String,
    pub bot_token: String,
    pub default_prefix: String,
    pub channel: String,
    pub db_connection: String
}

pub fn read_creds(path: &str) -> KingResult<Credentials> {
    let file = fs::File::open(path)?;
    let reader = BufReader::new(file);

    let info: Credentials = serde_json::from_reader(reader).unwrap();

    Ok(info)
}
