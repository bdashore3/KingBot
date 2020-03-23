using Kingbot.Helpers.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kingbot.Modules
{
    class Custom
    {
        // For dependency injection
        private readonly DatabaseHelper<Command> _data;
        public Custom(DatabaseHelper<Command> data)
        {
            _data = data;
        }

        // Handle the instruction from CommandHandler
        public async Task Handle(List<String> words, string username)
        {
            string instruction = words[1].ToLower();
            string name = words[2];

            switch (instruction)
            {
                case "add":
                    words.RemoveRange(0, 3);
                    string message = String.Join(" ", words.ToArray());
                    if (await _data.Ensure(name))
                    {
                        TwitchBot.client.SendMessage(TwitchBot.channel, $"Command {name} already exists!");
                        break;
                    }
                    await AddCommand(name, message);
                    break;
                case "remove":
                    await _data.Ensure(name);
                    TwitchBot.client.SendMessage(TwitchBot.channel, $"Command {name} successfully deleted!");
                    break;
            }
        }

        // Seperate method to add a command
        private async Task AddCommand(string name, string message)
        {
            Command FileToAdd = new Command
            {
                Index = name,
                Message = message
            };
            await _data.Write(FileToAdd);
            TwitchBot.client.SendMessage(TwitchBot.channel, $"New command {name} successfully written!");
        }
    }
}
