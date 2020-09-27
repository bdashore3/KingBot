use std::time::Duration;

use reqwest::Url;
use rocket::{State, get, post, request::Form, routes};
use rocket_contrib::json::Json;
use tokio::time::delay_for;
use crate::{api::webhook_handler::handle_follow, api::webhook_handler::handle_stream, structures::Bot, structures::webhooks::*};

use crate::structures::KingResult;

#[get("/", format = "text/html")]
async fn front_page() -> String {
    "Looks like you stumbled upon a webhook server. Good for you!".to_owned()
}

#[get("/?<query..>")]
async fn challenge_check(query: Form<Challenge>) -> String {
    println!("Incoming GET request on /");

    let res = if let Some(challenge) = &query.challenge {
        challenge
    } else {
        "Invalid request!"
    };

    res.to_owned()

}

#[post("/", data = "<data>", format = "json")]
async fn incoming_event(data: Json<Data>, bot: State<'_, Bot>) {
    println!("New POST request on /");

    match &data.data.get(0) {
        Some(EventType::FollowEvent(follow_data)) => {
            let _ = handle_follow(&*bot, follow_data).await;
        },
        Some(EventType::StreamEvent(stream_data)) => {
            let _ = handle_stream(&*bot, Some(stream_data)).await;
        },
        None => {
            let _ = handle_stream(&*bot, None).await;
        }
    }
}

pub async fn start_rocket(bot: Bot, api_id: String, api_token: String) -> KingResult {
    let rocket = rocket::ignite()
        .manage(bot)
        .mount("/", routes![challenge_check, front_page, incoming_event]);

    tokio::spawn(async move {
        let _ = init_url(api_id, api_token).await;
    });

    rocket.launch().await?;

    Ok(())
}

async fn init_url(api_id: String, api_token: String) -> KingResult{
    let client = reqwest::Client::new();

    delay_for(Duration::from_secs(3)).await;

    loop {
        let url = Url::parse_with_params("https://api.twitch.tv/helix/webhooks/hub", 
        &[
                ("hub.callback", "http://dd4e195f13c4.ngrok.io"),
                ("hub.lease_seconds", "10000"),
                ("hub.topic", "https://api.twitch.tv/helix/users/follows?first=1&to_id=489208504"),
                ("hub.mode", "subscribe")
            ])?;

        client.post(url)
            .header("Client-Id", &api_id)
            .header("Authorization", format!("Bearer {}", api_token))
            .send().await?;

        delay_for(Duration::from_secs(86400)).await;
    }
}
