const fs = require('fs')

var timerList = {};

module.exports = {
	// Function called when the "dice" command is issued
	rollDice: function () {
		const sides = 6;
		return Math.floor(Math.random() * sides) + 1;
	},

	timerWords: function(parameter) {
		switch (parameter) {
			case "follow":
				return "Want to see more content? Hit that follow button!";
		}
	},

	timer: function(client, channel, parameter, type, ms) {
		if (parameter == "start") {
			timerList[type] = setInterval(function(){ client.say(channel, module.exports.timerWords(type)) }, ms);
		}
		if (parameter == "stop") {
			clearInterval(timerList[type]);
		}
	}
}