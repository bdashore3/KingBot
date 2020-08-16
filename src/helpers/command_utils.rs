use crate::structures::BotResult;

pub fn get_time(initial_time: &u64, parameter: &str) -> BotResult<u64> {
    let value = match parameter {
        "seconds" => initial_time % 60,
        "minutes" => (initial_time / 60) % 60,
        "hours" => (initial_time / 3600) % 24,
        _ => {
            return Err("Invalid parameter input".into())
        }
    };

    Ok(value)
}