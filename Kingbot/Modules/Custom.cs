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
            switch (words[1].ToLower())
            {
                case "add":
                    string addIndex = words[2];
                    words.RemoveRange(0, 3);
                    string message = String.Join(" ", words.ToArray());

                    if (await _data.Ensure(addIndex))
                    {
                        TwitchBot.client.SendMessage(TwitchBot.channel, $"Command {addIndex} already exists!");
                        break;
                    }
                    await AddCommand(addIndex, message);
                    break;
                case "remove":
                    await _data.Delete(words[2]);
                    TwitchBot.client.SendMessage(TwitchBot.channel, $"Command {words[2]} successfully deleted!");
                    break;
                case "list":
                    await _data.GetList("Commands", username);
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
