const client = require('./botClient.js')
const apiClient = require('./apiClient.js')
const chatHelper = require('./helpers/chat.js')
const WebHookListener = require('twitch-webhooks').default;
const info = require('./JSON/info.json')

const prefix = '?'; // Whatever the prefix is. NOTE: streamlabs uses !
const briUsername = 'kingbrigames' // My username: Used for dev check
const userId = info.userID

// Connect to Twitch:
client.connect();


// Checks if the user is an admin aka a mod.
function isAdmin(name) {
	return (isDev(name))
}

// Do NOT edit this.
function isDev(name) {
	return (name == briUsername);
}

/*
 * Webhook for listening to any stream updates
 * If update happens, stuff is executed
 * Functions like a startup script
 */
const listenerInit = async () => {
	const listener = await WebHookListener.create(apiClient, {
			hostName: "**Ngrok url**.ngrok.io",
			port: 8090,
			reverseProxy: { port: 443, ssl: true }
		});
	listener.listen();
	return subscription(listener);
}

const subscription = async (listener) => { 
	return await listener.subscribeToStreamChanges(userId, async (stream) => {
		if (stream) {
			console.log(`${stream.userDisplayName} just went live with title: ${stream.title}`);
		} else {
			// no stream, no display name
			const user = await twitchClient.helix.users.getUserById(userId);
			console.log(`${user.displayName} just went offline`);
		}
	});
}

listenerInit().then(res => { })
	.catch((err) => console.log("listenerErr: ", err))

// On connection, update all json files
client.on('connected', (address, port) => {
	chatHelper.updateTimerWords();
	chatHelper.updateQuotes();
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
		// Check if the API says if the current channel is live!
		case "checklive":
			if (!chatHelper.isStreamLive(info.channel)) {
				client.say("Stream is not live...")
			} else {
				client.say("The stream IS live according to twitch API")
			}
			break;

		// PONG
		case "ping":
			client.say(channel, `Pong!`);
			break;

		// Roll some dice (From twitch bot sample)
		case "dice":
			client.say(channel, user['display-name'] + `: You rolled a ` + chatHelper.rollDice());
			break;

		// Frontend for messages over an interval in chat
		case "timer":
			if (!isAdmin(user['username'])) {
				client.action(channel, "You can't execute this command!")
				break;
			}
			chatHelper.timer(channel, words[1], words[2], words[3])
			break;
		
		// Adds message for timer. Will be combined with timer
		case "addtimermessage":
			if (!isAdmin(user['username'])) {
				client.action(channel, "You can't execute this command!")
				break;
			}
			index = words[1]
			words.splice(0, 2);
			chatHelper.addTimerMessage(index, words.join(" "))
			break;

		/*
		 * Frontend for the quotes system.
		 * Users cannot use the write command.
		 * They can use all other commands.
		 */
		case "quote":
			if (words[1] == "write" || words[1] == "remove") {
				if (!isAdmin(user['username'])) {
					client.action(channel, "You can't write to files! But you can do other stuff!")
					break;
				}
			}
			else if (words[1] == "add") {
				index = words[2]
				words.splice(0, 3);
				if (chatHelper.ensureQuote(index)) {
					client.say(channel, "This number is taken!")
					break;
				}
				chatHelper.quote("add", index, words.join(" "))
				break;
			}
			out = chatHelper.quote(words[1], words[2], words[3])
			if (out == undefined) {
				break;
			}
			client.say(channel, out);
			break;
	}
});