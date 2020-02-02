const fs = require('fs')
const jsonPath = "./JSON/";

var timerWords = {};

/* 
 * Simple function for writing so that I don't have to keep entering the same command
 */
function writeInternal() {
	fs.writeFileSync(jsonPath + 'timerWords.json', JSON.stringify(timerWords, null, 4));
	return "Timer Messages Successfully written";
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

	addTimerMessage: function(index, message) {
		timerWords[index] = {
			message: message
		}
	}
}