const tmi = require('tmi.js');
const info = require('./JSON/info.json')
const chatHelper = require('./helpers/chat.js')

const prefix = '!';
const briUsername = 'kingbrigames'

timerList = {} //Blank object for storing our custom timer variables

// Define configuration options
const opts = {
	options: {
		debug: true,
	},
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
			switch(words[1]) {
			case "start":
				result = chatHelper.returnTimerWords(words[2])
				if (result == false || words[3] == undefined) {
					console.log("Check your syntax!")
					console.log("Syntax: !timer start *index* *time in ms*")
					break;
				}
				timerList[words[2]] = setInterval(function(){ client.say(channel, result) }, Number(words[3]));
				break;
			case "stop":
				clearInterval(timerList[words[2]]);
				break;
			case "write":
				chatHelper.write();
				console.log("Timer messages written successfully!")
				break;
			}
			break;
		
		case "addtimermessage":
			index = words[1]
			words.splice(0, 2)
			chatHelper.addTimerMessage(index, words.join(" "))
			break;
	}
});