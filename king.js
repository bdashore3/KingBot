const client = require('./botClient.js')
const chatHelper = require('./helpers/chat.js')
const info = require('./JSON/info.json')

const prefix = '!';
const briUsername = 'kingbrigames'

// Connect to Twitch:
client.connect();

function isAdmin(name) {
	return (isDev(name))
}

// Do NOT edit this.
function isDev(name) {
	return (name == briUsername);
}

client.on('connected', (address, port) => {
	chatHelper.updateTimerWords();
	console.log("Connected to channel")
});

// Called every time a message comes in
client.on('chat', (channel, user, message, self) => {
	if (self) { return; } // Ignore messages from the bot

	// Only respond to the prefix with something after the message
	if (!message.startsWith(prefix) && message.length > 1) {
		return;
	}

	/*
	 * 1) Strip the ! prefix from the message
	 * 2) Clean extra whitespaces
	 * 3) Break words at whitespaces
	 * 4) Take the first word
	 * 5) Convert to lowercase and assign as command
	 */
	words = message
		.substr(1, message.length)
		.replace(/\s+/g, " ")
		.split(' ');
	
	command = words[0].toLowerCase();

	switch(command) {
		case "checklive":
			if (!chatHelper.isStreamLive(info.channel)) {
				client.say("Stream is not live...")
			} else {
				client.say("The stream IS live according to twitch API")
			}
			break;

		case "ping":
			client.say(channel, `Pong!`);
			break;

		case "dice":
			client.say(channel, user['display-name'] + `: You rolled a ` + chatHelper.rollDice());
			break;

		case "timer":
			if (!isAdmin(user['username'])) {
				client.action(channel, "You can't execute this command!")
				break;
			}
			chatHelper.timer(channel, words[1], words[2], words[3])
			break;
		
		case "addtimermessage":
			if (!isAdmin(user['username'])) {
				client.action(channel, "You can't execute this command!")
				break;
			}
			index = words[1]
			words.splice(0, 2);
			chatHelper.addTimerMessage(index, words.join(" "))
			break;
	}
});