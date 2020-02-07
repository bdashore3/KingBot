const fs = require('fs')
const jsonPath = "./JSON/";
const client = require('.././botClient.js')
const apiClient = require('.././apiClient.js')

var timerWords = {};
var timerList = {};

/* 
 * Simple function for writing so that I don't have to keep entering the same command
 */
function writeInternal() {
	fs.writeFileSync(jsonPath + 'timerWords.json', JSON.stringify(timerWords, null, 4));
	return "Timer Messages Successfully written";
}

async function isStreamLiveInternal(userName) {
	const user = await apiClient.helix.users.getUserByName(userName);
	if (!user) {
		return false;
	}
	return await user.getStream() !== null;
}

module.exports = {

	write: function() {
		writeInternal();
	},

	// Function called when the "dice" command is issued
	rollDice: function () {
		const sides = 6;
		return Math.floor(Math.random() * sides) + 1;
	},

	updateTimerWords: function() {
		timerWords = JSON.parse(fs.readFileSync(jsonPath + 'timerWords.json'));
	},

	returnTimerWords: function(index) {
		if (index == undefined) {
			return false;
		}
		return timerWords[index].message;
	},

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
				writeInternal();
				console.log("Timer messages written successfully!")
				break;
		}
	},

	addTimerMessage: function(index, message) {
		timerWords[index] = {
			message: message
		}
	},

	isStreamLive: function(userName) {
		isStreamLiveInternal(userName).then(function(result) {
			return result;
		}) 
	}
}