# KingBot

This is an opensource twitch bot! It's mainly used for **kingbrigames's** twitch channel, but it's an easy to use bot which can be added on any channel!

## Feature List
All commands are within `Modules`, but here is a list of the features if you're too lazy:

- Ping: Prints "Pong!"
- Custom Commands: A simple syntax to add a custom command to reference a message such as `!discord` and `!twitter` which is commonly used in streams.
- Interval: The ability to set a recurring message of whatever the admin wants for a custom amount of ms. The messages/words are stored in the SQL database. Only executable by admins.
- Quote: Store quotes from the streamer into the SQL database. Engineered with a CRUD system so users can retrieve quotes as well. Quotes cannot be overwritten or deleted without admin permission.
- Shoutouts: Typing `!so` or `!shoutout` will allow you to shout out a streamer. This is only executable by admins.
- Stream Events: Has events for: new follows, first time subscribers, recurring subscribers, gifted subs. Hosting is not supported yet and Raids may be supported.
- Automatically execute functions when the stream goes online/offline. Editable via the API helper file.
- Lurking: Allow lurkers to have some fun! Type `!lurk` and the bot will know that the user is lurking. When the user types `!lurk` again, he/she will see how much time he/she has lurked for. The user can cancel the lurk state by typing `!lurk cancel`. All lurks are automatically cleared when the stream ends.
- A universal CRUD interface which allows users to easily program commands that store 2 values in the database.

## Preparation

### Client

If you want a seperate bot account, create a new twitch account and follow [this guide](https://dev.twitch.tv/docs/irc) to get an OAuth token. Put the bot's username and OAuth token in **BotToken** and **BotUsername** inside info.json.

### API
For API events to work, you need an API Client ID and an API Token. Follow these steps:

1. Go to [dev.twitch.tv](dev.twitch.tv) and create a new application
2. Use `http://localhost` as an OAuth URL (We're not using this, so it's fine)
3. Make the application in the scope of a bot
4. Put the **Client ID** in **ApiId** and **Client Secret** in **ApiToken** inside info.json.

### Database setup
Follow [this guide](https://www.digitalocean.com/community/tutorials/how-to-install-and-use-postgresql-on-ubuntu-18-04) up until step 3 to get postgres set up on ubuntu. Afterwards, go on pgAdmin4 and follow these steps

 1. Log into a sudo shell and change the postgres user's password by:
	 `passwd postgres`
	 
 2. Add a new server using postgres as the username, and the password that you set for postgres. The IP is your VPS IP or localhost depending on where you're hosting.
 3. Once connected, create a new database and call it whatever you want. You will be using this database name in your ConnectionString and leave the database BLANK.
 
 Your ConnectionString should look like this: `"Host=*Your IP*;Database=*Your DB name*;Username=postgres;Password=*Password you set for postgres user*"`

If you have a connection refused error, follow [this forum post](https://www.digitalocean.com/community/questions/remote-connect-to-postgresql-with-pgadmin) on DigitalOcean:

## Installation

All package hooks ARE included by default. You just need the dotnet runtime, a postgres database, and an EF Core migration set up. Follow the instructions in Preparation to get started with EF Core.

### Setting up the dotnet runtime

To set up the dotnet runtime for ubuntu: [MSDN docs](https://docs.microsoft.com/en-us/dotnet/core/install/linux-package-manager-ubuntu-1804)

Then, copy **info.sample.json** to **info.json** in the project directory. From there, add all of your credentials.

### Entity Framework Core Setup
Once you clone the repository, change into the project directory (KingBot/Kingbot), install the EF Core tools by:
`dotnet tool install --global dotnet-ef`

Then run the following commands:
```
dotnet ef migrations add InitialCreate
dotnet ef database update
```
If you have errors, run `dotnet build` and show them to me in the [discord server](https://discord.gg/pswt7by) if you can't figure out the reason.

This initializes the database for the first time with all the required tables, rows, and columns. If you plan on updating the model, please read the [Entity Framework Core docs](https://docs.microsoft.com/en-us/ef/core/).

### Finally:
Once you're done, type the following command in the terminal inside the project directory (KingBot/Kingbot):
`dotnet build && dotnet run info.json`

## Running in a server

The included systemd service is REQUIRED to run this bot in a server. Running in interactive mode is not advised. Copy the twitch.service file into /etc/systemd/system/twitch.service. Then, run these commands
```
sudo systemctl reload-daemon
sudo systemctl enable twitch.service
sudo systemctl start twitch.service
```

Check with:
```
sudo systemctl status twitch.service
sudo journalctl -u twitch -f
```

## Removing the bot

It's easy! All you have to do is delete the bot directory and the systemd file from `/etc/systemd/twitch.service`

# Contributing Modules
The C# version of this bot features modular commands that can be swapped out as needed. To successfully have your module added, you need to follow the guidelines:

1. The module must be commented with a description on what each function does.
2. A module is NOT a wrapper! If you want to make a wrapper for something, use the Other class in modules.
3. You must be familiar with the CommandHandler syntax and link the module with the CommandHandler using a switch case. Nothing goes past it.
4. If you are using the database, stick to the universal DatabaseHelper class which allows for an easy way to CRUD. If you need more columns/rows in your table, modify the EF Core model accordingly and put a comment as to why you did this.
5. Use Dependency Injection as MUCH as possible. Reference the current modules for an example.

# Developers and Permissions

Currently, this bot is allowed for use outside of the developer's channel since I want people to understand how Twitch bots are coded and how to run them. I try to make the comments as detailed as possible, but if you don't understand something, please contact me via the Discord server! I'm always happy to talk!

Creator/Developer: Brian Dashore

Developer Discord: kingbri#6666

Follow me on twitch (I stream bot creation along with games): [https://twitch.tv/kingbrigames](https://twitch.tv/kingbrigames)

Join the Discord server here: [https://discord.gg/pswt7by](https://discord.gg/pswt7by)
