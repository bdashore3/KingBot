const tmi = require ('tmi.js');
const info = require('./JSON/info.json')

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

let client = new tmi.client(opts);

module.exports = client