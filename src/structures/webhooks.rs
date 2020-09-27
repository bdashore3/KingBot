use rocket::request::FromForm;
use serde::{Serialize, Deserialize};

#[derive(FromForm)]
pub struct Challenge {
    #[form(field = "hub.challenge")]
    pub challenge: Option<String>,
    #[form(field = "hub.lease_seconds")]
    pub lease_seconds: Option<u64>,
    #[form(field = "hub.mode")]
    pub mode: String,
    #[form(field = "hub.topic")]
    pub topic: String
}

#[derive(Debug, Serialize, Deserialize)]
pub struct Data {
    pub data: Vec<EventType>
}

#[derive(Debug, Serialize, Deserialize)]
#[serde(untagged)]
pub enum EventType {
    StreamEvent(StreamEvent),
    FollowEvent(FollowEvent),
}

#[derive(Debug, Serialize, Deserialize)]
pub struct StreamEvent {
    pub id: String,
    pub user_id: String,
    pub user_name: String,
    pub game_id: Option<String>,
    pub community_ids: Option<Vec<String>>,
    #[serde(rename = "type")]
    pub stream_type: String,
    pub title: String,
    pub viewer_count: u64,
    started_at: String,
    language: String,
    thumbnail_url: String
}
#[derive(Debug, Serialize, Deserialize)]
pub struct FollowEvent {
    pub from_id: String,
    pub from_name: String,
    pub to_id: String,
    pub to_name: String,
    pub followed_at: String
}
