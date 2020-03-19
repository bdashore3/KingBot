using System;
using System.Collections.Generic;
using System.Linq;
using Kingbot.Helpers.Data;
using Kingbot.Modules;

namespace Kingbot.Commands
{
    class CommandHandler
    {
        public static void HandleCommand(string og)
        {

            /*
             * Flow:
             * 1. Remove the message prefix
             * 2. Convert the words into a List (Not an array!)
             * 3. Make the command lowercase for the switch statement
             */

            string msg = og.Substring(1);
            List<String> words = msg.Split(" ").ToList();
            string command = words[0].ToLower();
            string channel = TwitchBot.channel;

            /*
             * The array words at index 0 (words[0]) is
             * passed through the switch statement.
             * 
             * From here, the appropriate class is executed
             * with all words being passed to said class
             * 
             * TODO: Add checks for administrators (Moderators)
             */
            switch (command)
            {
                case "ping":
                    Console.WriteLine("Command Ping Recieved");
                    Other.Ping();
                    break;

                case "quote":
                    Console.WriteLine("Command Quote Recieved");
                    Quotes.Handle(words);
                    break;

                case "interval":
                    Console.WriteLine("Command Interval Received");
                    Interval.Handle(words);
                    break;

                case "command":
                    Console.WriteLine("Command Custom Recieved");
                    Custom.Handle(words);
                    break;
            }

            if (DataHelper.Ensure("commands", command))
                TwitchBot.client.SendMessage(TwitchBot.channel, DataHelper.Read("commands", command));
        }
    }
}
