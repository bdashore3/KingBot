using System;
using System.Collections.Generic;
using Kingbot.Security;
using System.Linq;
using Kingbot.Modules;
using Kingbot.Data;

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
                    string quote = await quotes.PostQuote(words[1]);
                    TwitchBot.client.SendMessage(channel, quote);
                    break;
            }
        }
    }
}
