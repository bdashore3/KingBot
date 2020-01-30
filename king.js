const tmi = require('tmi.js');
const info = require('./JSON/info.json')
const chat = require('./helpers/chat.js')

const prefix = '!';

// Define configuration options
const opts = {
  identity: {
    username: info.username,
    password: info.token
  },
  channels: [
    info.channel
  ]
};
// Create a client with our options
const client = new tmi.client(opts);

// Register our event handlers (defined below)
client.on('message', onMessageHandler);
client.on('connected', onConnectedHandler);

// Connect to Twitch:
client.connect();

// Called every time a message comes in
function onMessageHandler (target, context, msg, self) {
    if (self) { return; } // Ignore messages from the bot

    // Only respond to the prefix with something after the message
    if (!msg.startsWith(prefix) && msg.length > 1) {
	    return;
    }
	/*
	 * 1) Strip the ! prefix from the message
	 * 2) Clean extra whitespaces
	 * 3) Break words at whitespaces
	 * 4) Take the first word
	 * 5) Convert to lowercase and assign as command
	 */
	words = msg
			.substr(1, msg.length)
			.replace(/\s+/g, " ")
			.split(' ');
	
    command = words[0].toLowerCase();
    
    switch(command) {
        case "dice":
 
            client.say(target, `You rolled a ` + chat.rollDice());
    }
}

// Called every time the bot connects to Twitch chat
function onConnectedHandler (addr, port) {
  console.log(`* Connected to ${addr}:${port}`);
}