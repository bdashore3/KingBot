using Kingbot.Helpers.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kingbot.Modules
{
    class Custom
    {
        public static async Task Handle(List<String> words)
        {
            string instruction = words[1].ToLower();
            string name = words[2];

            switch (instruction)
            {
                case "add":
                    words.RemoveRange(0, 3);
                    string message = String.Join(" ", words.ToArray());
                    if (await DataHelper.Ensure("commands", name))
                    {
                        TwitchBot.client.SendMessage(TwitchBot.channel, $"Command {name} already exists!");
                        break;
                    }
                    await AddCommand(name, message);
                    break;
                case "remove":
                    await DataHelper.Delete("commands", name);
                    TwitchBot.client.SendMessage(TwitchBot.channel, $"Command {name} successfully deleted!");
                    break;
            }
        }

        private static async Task AddCommand(string name, string message)
        {
            await DataHelper.Write("commands", name, message);
            TwitchBot.client.SendMessage(TwitchBot.channel, $"New command {name} successfully written!");
        }
    }
}
