# KingBot

This is an opensource twitch bot! It's mainly used for **kingbrigames's** twitch channel, but it's an easy to use bot which can be added on any channel!

## Sunsetting JS

I will be sunsetting the JS version of this bot due to the newer C# version. This branch will still be avaliable, but will no longer be updated. If you want updated
features, please use the C# version of this bot.

## Command List

All commands are in the comments of the king.js file, but here is a list if you're too lazy:

- checklive: Checks if the stream is live using the twitch API
- ping: Prints "Pong"
- dice: From the twitch example bot js file
- timer/addtimermessage: Sets a recurring message of whatever the admin wants for a custom amount of ms. The messages/words are stored in a JSON file
- quote: Store quotes from the streamer into RAM until the stream ends where the quotes will be saved to a JSON file. Engineered with a simple CRUD system so users can retrieve quotes as well. Quotes cannot be overwritten without admin permission.

## Preparation

Follow [this guide](https://dev.twitch.tv/docs/irc) to get an OAuth token and create another twitch account for your bot to use in the info.json file. 

## Installation

Package hooks are not included within this bot, so you need to install these packages yourself via npm:

- twitch-webhooks
- twitch
- tmi.js
- fs

Then, copy the JSON files from the samples folder into a new directory called JSON and put the right credentials in info.json

Once you're done, type the following command in the terminal:
> node king.js

## Running in a server

The included systemd service is REQUIRED to run this bot in a server. Running in interactive mode is not advised. Copy the twitch.service file into /etc/systemd/system/twitch.service. Then, run these commands. The bot assumes you have a user twitch under a group twitch:
> sudo systemctl reload-daemon

> sudo systemctl enable twitch.service

> sudo systemctl start twitch.service

Check with:
> sudo systemctl status twitch.service

> sudo journalctl -u twitch -f

## Using the webhooks:

Currently, these work if you have a development server such as a Vultr VPS. If not, you can either use ngrok (Which I'll add flags for in a future update) Or port forward the port shown in king.js. I highly recommend using a server since permanently using ngrok will cost money anyways, so it's better to spend it on a cheap VPS.

## Removing the bot

It's easy! All you have to do is delete the bot directory and brush your hands from the dust!

# Developers and Permissions

Currently, this bot is allowed for use outside of the developer's channel since I want people to understand how Twitch bots are coded and how to run them.

Creator/Developer: Brian Dashore

Developer Discord: kingbri#1588

Follow me on twitch (I stream bot creation along with games): [https://twitch.tv/kingbrigames](https://twitch.tv/kingbrigames)

Join the Discord server here: [https://discord.gg/pswt7by](https://discord.gg/pswt7by)
