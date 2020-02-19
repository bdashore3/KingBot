const client = require('./botClient.js')
const apiClient = require('./apiClient.js')
const chatHelper = require('./helpers/chat.js')
const WebHookListener = require('twitch-webhooks').default;
const info = require('./JSON/info.json')
const custom = require('./JSON/custom.json')

const prefix = '?'; // Whatever the prefix is. NOTE: streamlabs uses prefix: !
const briUsername = 'kingbrigames' // My username: DO NOT EDIT THIS.
const userId = info.userID // User's ID for webhooks. Required in string format otherwise 400 error will fire (The JSON file handles it for you).

// Connect to Twitch:
client.connect();


// Checks if the user is an admin aka a mod.
function isAdmin(name) {
	return (isDev(name) || 'regalbot1')
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
			port: 8090
		});
	listener.listen();
	return subscription(listener);
}

/*
 * Subscribes to the listener
 * If x happens, then the code below is executed and returned to the bot
 * This webhook is better than API polling because it only turns on when needed to instead
 * of constantly sending a request through the API.
 * The first comment is a testing return statment where we check for followers instead of
 * if the user is live.
 */

const subscription = async (listener) => {
	//return await listener.subscribeToFollowsToUser(userId, follow => console.log(follow))
	return await listener.subscribeToStreamChanges(userId, async (stream) => {
		if (stream) {
			console.log(`${stream.userDisplayName} just went live with title: ${stream.title}`);
			client.action(info.channel, `Executing startup commands...`);
			chatHelper.timer(info.channel, "start", "follow", "600000");
		} else {
			// no stream, no display name
			const user = await apiClient.helix.users.getUserById(userId);
			console.log(`${user.displayName} just went offline`);
			client.action(info.channel, `Executing stop commands...`)
			chatHelper.timer(info.channel, "stop", "follow");
		}
	});
}

listenerInit().then(res => { })
	.catch((err) => console.log("listenerErr: ", err))

// On connection, update all json files
client.on('connected', (address, port) => {
	chatHelper.read("timerWords");
	chatHelper.read("quotes");
	chatHelper.read("custom");
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
		// Check if the API says if the current channel is live! (Logs in console!)
		case "checklive":
			chatHelper.isStreamLive(info.channel)
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
			instruction = words[1]
			name = words[2]
			ms = words[3]

			if (!isAdmin(user['username'])) {
				client.action(channel, "You can't execute this command!")
				break;
			}
			if (instruction == "add") {
				words.splice(0, 3)
				chatHelper.add("timerWords", name, words.join(" "))
			}
			else if (words[1] == "list") {
				chatHelper.timer(channel, instruction, 0, 0, user['username'])
				break;
			}
			chatHelper.timer(channel, instruction, name, ms)
			break;

		/*
		 * Frontend for the quotes system.
		 * Users cannot use the write command.
		 * They can use all other commands.
		 * 
		 * There are seperate cases for add, list, write, and remove
		 * because these call different functions or have special
		 * code that needs to be executed.
		 */
		case "quote":
			instruction = words[1];
			name = words[2]

			if (instruction == "write" || instruction == "remove") {
				if (!isAdmin(user['username'])) {
					client.action(channel, "You can't write to files! But you can do other stuff!")
					break;
				}
			}
			else if (instruction == "add") {
				words.splice(0, 3);
				if (chatHelper.ensurePhrase(name, "quotes")) {
					client.say(channel, "This number is taken!")
					break;
				}
				chatHelper.add("quotes", name, words.join(" "))
				break;
			}
			else if (instruction == "list") {
				chatHelper.quote(instruction, 0, user['username'])
				break;
			}

			out = chatHelper.quote(instruction, name)
			if (out == undefined) {
				break;
			}
			client.say(channel, out);
			break;

		// Frontend for writing commands Admins only!
		case "command":
			instruction = words[1];
			name = words[2];

			if (instruction == "list") {
				chatHelper.customCommand(words[1], 0, 0, user['username'])
				break;
			}

			if (!isAdmin(user['username'])) {
				client.action(channel, "You can only list commands!")
				break;
			}

			words.splice(0, 3);
			chatHelper.customCommand(instruction, name, words.join(" "));
			break;

		// Whisper stuff to yourself!
		case "whisper":
			words.splice(0, 1)
			client.whisper(user['username'], words.join(" "))
			break;

		/*
		case "lurk":
			out = chatHelper.lurk(user['username'])
			if (!out) {
				client.say(channel, user['username'] + " Is currently lurking!")
				break;
			}
			client.say(channel, user['username'] + " Has been lurking for " + out)
			break;
		*/
		
		/*
		 * Check that vibe in stream!
		 * A very simple case where a timeout of 2 seconds is set for the response to be sent
		 * Puts it in action form because vibe checks are very important.
		 * 
		 * NOTE: tmi.js complains when you execute client.say directly with a setTimeout. A new
		 * function will have to be created inside the setTimeout!
		 */
		case "vibecheck":
			client.say(channel, "initating vibe check for " + user['username'])
			setTimeout(function(){ client.action(channel, user['username'] + chatHelper.vibeRes()) }, 2000)
			break;
	}

	// If the command doesn't exist, check if in custom command object.
	if (chatHelper.custom[command]) {
		client.say(channel, chatHelper.custom[command].message);
	}

});
