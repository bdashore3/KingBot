using System;
using Kingbot.Helpers.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kingbot.Modules
{
    class Quotes
    {
        /*
         * Handles all quote related commands
         * Links up to the aerospike database for reading
         * and saying in the twitch client.
         * 
         * TODO: Make the admin check less repetitive
         */

        public static async Task Handle(List<string> words, bool IsMod)
        {
            string instruction = words[1].ToLower();
            string index = words[2];

            switch (instruction)
            {
                case "retrieve":
                    TwitchBot.client.SendMessage(TwitchBot.channel, await ReturnQuote(index));
                    break;
                case "add":
                    words.RemoveRange(0, 3);
                    string message = String.Join(" ", words.ToArray());
                    if (await DataHelper.Ensure("quotes", index))
                    {
                        TwitchBot.client.SendMessage(TwitchBot.channel, $"Quote {index} already exists!");
                        break;
                    }
                    await AddQuote(index, message);
                    break;
                case "remove":
                    if (IsMod)
                    {
                        await DataHelper.Delete("quotes", index);
                        TwitchBot.client.SendMessage(TwitchBot.channel, $"Quote {index} successfully deleted!");
                        break;
                    }
                    else
                    {
                        TwitchBot.client.SendMessage(TwitchBot.channel, "You can't execute this command!");
                        break;
                    }
            }
        }

        /*
         * Get the quote from the database and return it to the
         * calling function.
         */
        private static async Task<string> ReturnQuote(string index)
        {
            var result = await DataHelper.Read("quotes", index);

            if (result == null)
                return $"Quote {index} doesn't exist! Try adding it?";
            else
                return result;
        }

        /*
         * Add a new quote into the database
         * 
         * TODO: Use the EnsureQuote check to see if the quote exists
         * already to prevent overwriting.
         * 
         * Quotes cannot be updated unless executed by an admin
         */

        private static async Task AddQuote(string index, string message)
        {
            await DataHelper.Write("quotes", index, message);
            TwitchBot.client.SendMessage(TwitchBot.channel, $"Quote {index} successfully written!");
        }
    }
}
