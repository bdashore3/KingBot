const fs = require('fs')
const jsonPath = "./JSON/";
const client = require('.././botClient.js')
const apiClient = require('.././apiClient.js')

const allObjects = {
	timerList: {},
	timerWords: {},
	quotes: {},
	custom: {},
	lurkTimes: {}
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
	
	// calls readInternal
	read: function(name) {
		readInternal(name);
	},

	/*
	 * Universal function for adding to an object
	 * This is called in other functions to cut down
	 * on cruft and redundancy.
	 */
	add: function(objName, name, newMessage) {
		allObjects[objName][name] = {
			message: newMessage
		}
	},

	/*
	 * Universal remove function
	 * Has the same purpose as add, but removes things
	 */
	remove: function(objName, name) {
		delete allObjects[objName][name];
	},

	/*
	 * Universal list function
	 * Whispers are a pain to repeat over and over again, so
	 * just do it all here.
	 */
	list: function(username, listName, helpCommand, objName) {
		client.whisper(username, listName + " List " + helpCommand);
		client.whisper(username, "---------------------------------------")
		for (const i of Object.keys(allObjects[objName])) {
			console.log(i + " = " + allObjects[objName][i].message)
			client.whisper(username, i + " = " + allObjects[objName][i].message);
		}
		client.whisper(username, "---------------------------------------")
	},

	// Function called when the "dice" command is issued
	rollDice: function () {
		const sides = 6;
		return Math.floor(Math.random() * sides) + 1;
	},

	// Returns if timerword exists
	checkExists: function(objName, name) {
		if (name == undefined || allObjects[objName][name] == undefined) {
			return false;
		}
		return allObjects[objName][name].message;
	},

	/*
	 * Backend for messages sent over a certain interval.
	 * If one parameter is missing or malformed, the bot will return an error to the user
	 * In the start case, the timer is started over the configured milliseconds
	 * Fetches the message from the timerWords object and sets an interval
	 * The stop case stops the interval. You will have to restart it using the timer start command
	 * The write case is to write the timerWords object to the timerWords.json file.
	 */
	timer: function(channel, instruction, name, ms, username) {
		switch(instruction) {
			case "remove":
				this.remove("timerWords", name)
				break;

			case "start":
				if (!this.ensurePhrase(name, "timerWords")) {
					console.log("Timer phrase doesn't exist! Try adding it?")
					break;
				}
				if (this.checkExists("timerWords", name) == false || ms == undefined) {
					console.log("Check your syntax!")
					console.log("Syntax: !timer start *index* *time in ms*")
					break;
				}
				allObjects.timerList[name] = setInterval(function(){ client.say(channel, allObjects.timerWords[name].message) }, Number(ms));
				break;

			case "stop":
				clearInterval(allObjects.timerList[name]);
				break;

			case "list":
				this.list(username, "Timer Phrase", "(Start by ?timer start *index* *ms*)", "timerWords");
				break;

			case "write":
				writeInternal("timerWords");
				console.log("Quotes successfully written!");
				break;
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
	 * add: See documentation in add function
	 * remove: See documentation in remove function
	 * retrieve: Takes the quote from the object and returns it
	 *     to be said in the twitch chat
	 * write: Writes quotes object to JSON file. Only admins can execute this!
	 * list: Lists all quotes in a user's whispers in the format:
	 *     number = quote
	 * Since whispers are a oneline thing, every quote is sent in one line in the for loop
	 */
	quote: function(command, name, username) {
		switch(command) {
			case "remove":
				this.remove("quotes", name)
				break;
			
			case "list":
				this.list(username, "Quotes", "(Get the quote by ?quote retrieve #)", "quotes");
				break;

			case "retrieve":
				if (this.checkExists("quotes", name) == false) {
					console.log("Quote doesn't exist! Try adding one!")
					break;
				}
				return allObjects.quotes[name].message;

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
		return false;
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
	customCommand: function(instruction, name, words, username) {
		switch(instruction) {
			case "add":
				if (this.ensurePhrase(name, "custom")) {
					client.say(channel, "This command already exists")
					break;
				}
				this.add("custom", name, words)
				break;
			
			case "list":
				this.list(username, "Custom command", "(Execute by ?commandname)", "custom");
				break;

			case "remove":
				this.remove("custom", name)
				break;
			
			case "write":
				writeInternal("custom");
				console.log("Custom Command successfully written!")
				break;
		}
	},

	/*
	 * Check for an integer between 0 and 1
	 * If the integer is 1, user passes vibecheck
	 * Otherwise, user fails
	 */
	vibeRes: function() {
		vibeNum = Math.floor(Math.random() * 2)
		if (vibeNum) {
			return " has failed the vibecheck. GIMME THAT LICENSE SIR";
		}
		return " has passed the vibecheck. Continue vibing to the stream";
	},

	lurk: function(username) {
		if (!this.ensurePhrase(username, "lurkTimes")) {
			store = new Date();
			this.add("lurkTimes", username, store.getTime())
			return "false";
		}
		now = new Date()
		millis = now.getTime() - Number(allObjects.lurkTimes[username].message)
		newTime = this.msToTime(millis);
		return newTime;
	},

	msToTime: function(s) {
		var ms = s % 1000;
		s = (s - ms) / 1000;
		var secs = s % 60;
		s = (s - secs) / 60;
		var mins = s % 60;
		var hrs = (s - mins) / 60;
		
		return hrs + ':' + mins + ':' + secs + '.' + ms;
	}
}