using System;
using System.Collections.Generic;
using Kingbot.Security;
using System.Linq;

namespace Kingbot.Commands
{
    class CommandHandler
    {
        public static void HandleCommand(string og)
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
            }
        }
    }
}
