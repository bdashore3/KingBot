const fs = require('fs')
const jsonPath = "./JSON/";
const client = require('.././botClient.js')
const apiClient = require('.././apiClient.js')

var timerWords = {};
var timerList = {};
var quotes = {};

/*
 * One function to write to file x based on parameter entered.
 */
function writeInternal(parameter) {
	if (parameter == "timerWords") {
		fs.writeFileSync(jsonPath + 'timerWords.json', JSON.stringify(timerWords, null, 4));
		return "Timer Messages Successfully written";
	}
	if (parameter == "quotes") {
		fs.writeFileSync(jsonPath + 'quotes.json', JSON.stringify(quotes, null, 4));
		return "quotes successfully written"
	}
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
	// calls writeInternal
	write: function(parameter) {
		writeInternal(parameter);
	},

	// Function called when the "dice" command is issued
	rollDice: function () {
		const sides = 6;
		return Math.floor(Math.random() * sides) + 1;
	},

	// Updates timerwords object. Will be made into a universal function
	updateTimerWords: function() {
		timerWords = JSON.parse(fs.readFileSync(jsonPath + 'timerWords.json'));
	},

	// See above.
	updateQuotes: function() {
		quotes = JSON.parse(fs.readFileSync(jsonPath + 'quotes.json'))
	},

	// Returns if timerword exists
	returnTimerWords: function(index) {
		if (index == undefined) {
			return false;
		}
		return timerWords[index].message;
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
				if (this.returnTimerWords(words[2]) == false || ms == undefined) {
					console.log("Check your syntax!")
					console.log("Syntax: !timer start *index* *time in ms*")
					break;
				}
				timerList[words[2]] = setInterval(function(){ client.say(channel, timerWords[index].message) }, Number(ms));
				break;
			case "stop":
				clearInterval(timerList[index]);
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
		timerWords[index] = {
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
	 * 	     to be said in the twitch chat
	 * write: Writes quotes object to JSON file. Only admins can execute this!
	 *
	 * TODO: Add ability to list all quotes in a user's DMs (Requires known bot permission.)
	 */
	quote: function(command, index, newMessage) {
		switch(command) {
			case "add":
				quotes[index] = {
					message: newMessage
				}
				break;

			case "remove":
				delete quotes[index]
				break;

			case "retrieve":
				return quotes[index].message;

			case "write":
				writeInternal("quotes");
				console.log("Quotes successfully written!");
				break;
		}
	},

	// Makes sure the quote is in the json file. If it's there, return true.
	ensureQuote: function(index) {
		if (quotes.hasOwnProperty(index)) {
			return true;
		}
	}
}
