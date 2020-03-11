using System;
using System.Collections.Generic;
using Kingbot.Security;
using System.Linq;
using Kingbot.Modules;

namespace Kingbot.Commands
{
    class CommandHandler
    {
        public static async void HandleCommand(string og)
        {
            string msg = og.ToLower().Substring(1);
            List<String> words = msg.Split(" ").ToList();
            string command = words[0];
            string channel = CredentialsHelper.info.Channel;

            switch (command)
            {
                case "ping":
                    Console.WriteLine("Command Ping Recieved");
                    TwitchBot.client.SendMessage(channel, "Pong");
                    break;
                case "quote":
                    Console.WriteLine("Command Quote Recieved");
                    string instruction = words[1];
                    string index = words[2];

                    if (instruction == "retrieve")
                    {
                        TwitchBot.client.SendMessage(channel, await quotes.ReturnQuote(index));
                    }
                    if (instruction == "add")
                    {
                        words.RemoveRange(0, 3);
                        string message = String.Join(" ", words.ToArray());
                        await quotes.AddQuote(index, message);
                    }
                    if (instruction == "delete")
                    {
                        await quotes.RemoveQuote(index);
                    }
                    break;
            }
        }
    }
}
