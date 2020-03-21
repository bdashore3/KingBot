using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kingbot.Helpers.Data;
using Kingbot.Modules;

namespace Kingbot.Commands
{
    class CommandHandler
    {
        public static async Task HandleCommand(string og, bool IsMod)
        {
            /*
             * Flow:
             * 1. Remove the message prefix
             * 2. Convert the words into a List (Not an array!)
             * 3. Make the command lowercase for the switch statement
             */

            Console.WriteLine(IsMod);
            string msg = og.Substring(1);
            List<string> words = msg.Split(" ").ToList();
            string command = words[0].ToLower();

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
                    Other.Ping();
                    break;

                case "quote":
                    await Quotes.Handle(words, IsMod);
                    break;

                case "interval":
                    if (IsMod)
                    {
                        await Interval.Handle(words);
                        break;
                    }
                    else
                    {
                        TwitchBot.client.SendMessage(TwitchBot.channel, "You can't execute this command!");
                        break;
                    }

                case "command":
                    if (IsMod)
                    {
                        await Custom.Handle(words);
                    }
                    else
                    {
                        TwitchBot.client.SendMessage(TwitchBot.channel, "You can't execute this command!");
                        break;
                    }
                    break;
            }

            // If the command exists in custom commands, send the message
            if (await DataHelper.Ensure("commands", command))
                TwitchBot.client.SendMessage(TwitchBot.channel, await DataHelper.Read("commands", command));
        }
    }
}
