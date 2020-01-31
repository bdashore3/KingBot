const fs = require('fs')

module.exports = {
	// Function called when the "dice" command is issued
	rollDice: function () {
		const sides = 6;
		return Math.floor(Math.random() * sides) + 1;
	},

	timerWords: function(parameter) {
		if (parameter == "follow") {
			return "Want to see more content? Hit that follow button!";
		}
	}
}