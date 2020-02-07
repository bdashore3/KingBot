const TwitchClient = require('twitch').default
const info = require('./JSON/info.json')

// Define configuration options
const clientID = info.apiId
const clientSecret = info.apiSecret
const apiClient = TwitchClient.withClientCredentials(clientID, clientSecret);

module.exports = apiClient