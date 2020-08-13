use crate::structures::BotResult;

fn get_split_string(message_string: &str) -> Vec<&str> {
    message_string.split_whitespace().collect::<Vec<&str>>()
}

pub fn get_message_word(message_string: &str, index: usize) -> BotResult<&str> {
    match get_split_string(message_string).get(index) {
        Some(word) => return Ok(word),
        None => return Err("Couldn't find the string!".into())
    }
}

pub fn get_command(message_string: &str) -> &str {
    let command = get_split_string(message_string)[0];
    &command[1..]
}


pub fn join_string(message_string: &str, end_index: usize) -> BotResult<String> {
    let mut words = get_split_string(message_string);
    
    if words.get(end_index + 1).is_none() {
        return Err("No words provided!".into())
    }

    words.remove(0);
    words.drain(0.. end_index);
    Ok(words.join(" "))
}

pub fn get_command_length(message_string: &str) -> usize {
    get_split_string(message_string).len()
}