const fs = require('fs')
const jsonPath = "./JSON/";
const client = require('.././botClient.js')
const apiClient = require('.././apiClient.js')

const allObjects = {
	timerList: {},
	timerWords: {},
	quotes: {},
	custom: {}
}

/*
 * One function to write to file x based on parameter entered.
 */
function writeInternal(name) {
	return fs.promises.writeFile(`${jsonPath}${name}.json`, JSON.stringify(allObjects[name], null, 4));
}

function readInternal(name) {
	allObjects[name] = JSON.parse(fs.readFileSync(`${jsonPath}${name}.json`));
}
// Checks if the stream is live from the twitch api
async function isStreamLiveInternal(userName) {
	const user = await apiClient.helix.users.getUserByName(userName);
	if (!user) {
		return false;
	}
	return await user.getStream() !== null;
}

module.exports = {
	get custom() {
		return allObjects.custom;
	},

	// calls writeInternal
	write: function(name) {
		writeInternal(name);
	},

	read: function(name) {
		readInternal(name);
	},

	// Function called when the "dice" command is issued
	rollDice: function () {
		const sides = 6;
		return Math.floor(Math.random() * sides) + 1;
	},

	// Returns if timerword exists
	returnTimerWords: function(index) {
		if (index == undefined) {
			return false;
		}
		return allObjects.timerWords[index].message;
	},

	/*
	 * Backend for messages sent over a certain interval.
	 * If one parameter is missing or malformed, the bot will return an error to the user
	 * In the start case, the timer is started over the configured milliseconds
	 * Fetches the message from the timerWords object and sets an interval
	 * The stop case stops the interval. You will have to restart it using the timer start command
	 * The write case is to write the timerWords object to the timerWords.json file.
	 */
	timer: function(channel, command, index, ms) {
		switch(command) {
			case "start":
				if (this.returnTimerWords(index) == false || ms == undefined) {
					console.log("Check your syntax!")
					console.log("Syntax: !timer start *index* *time in ms*")
					break;
				}
				allObjects.timerList[index] = setInterval(function(){ client.say(channel, allObjects.timerWords[index].message) }, Number(ms));
				break;
			case "stop":
				clearInterval(allObjects.timerList[index]);
				break;
			case "write":
				writeInternal("timerWords");
				console.log("Timer messages written successfully!")
				break;
		}
	},

	/*
	 * Seperate function for adding timer messages. Will be combined with timer in future release.
	 */
	addTimerMessage: function(index, message) {
		allObjects.timerWords[index] = {
			message: message
		}
	},

	// module export for isStreamLiveInternal.
	isStreamLive: function(userName) {
		isStreamLiveInternal(userName).then(function(result) {
			console.log(result);
		})
	},

	/*
	 * Backend for quotes
	 * Has 4 cases: add, remove, retrieve, and write
	 * add: User can add a quote to the quotes object under a number (index)
	 * remove: remove said quote from that index. Only admins can execute this!
	 * retrieve: Takes the quote from the object and returns it
	 *     to be said in the twitch chat
	 * write: Writes quotes object to JSON file. Only admins can execute this!
	 *
	 * TODO: Add ability to list all quotes in a user's DMs (Requires known bot permission.)
	 */
	quote: function(command, index, newMessage) {
		switch(command) {
			case "add":
				allObjects.quotes[index] = {
					message: newMessage
				}
				break;

			case "remove":
				delete allObjects.quotes[index]
				break;

			case "retrieve":
				return allObjects.quotes[index].message;

			case "write":
				writeInternal("quotes");
				console.log("Quotes successfully written!");
				break;
		}
	},

	/*
	 * Makes sure the phrase exists in the corresponding object
	 * Basic idiotproofing function so stuff isn't overwitten
	 * Still uses classic index format.
	 */
	ensurePhrase: function(index, parameter) {
		if (allObjects[parameter].hasOwnProperty(index))
			return true;
	},

	/*
	 * Backend for custom commands
	 * Has 3 cases: add, remove, and write
	 * Add: adds a new command based on the name and the message
	 * Remove: gets rid of the added command
	 * Write: Writes commands object to JSON
	 * 
	 * Add has an ensurePhrase check. See the ensurePhrase docs.
	 * 
	 * TODO: List by whisper
	 */
	customCommand: function(instruction, name, words) {
		switch(instruction) {
			case "add":
				allObjects.custom[name] = {
					message: words
				}
				break;

			case "remove":
				delete allObjects.custom[name]
				break;
			
			case "write":
				writeInternal("custom");
				console.log("Custom Command successfully written!")
				break;
		}
	}
}
