using System;
using System.Threading.Tasks;
using Kingbot.Helpers.Data;
using MongoDB.Bson;
using System.Collections.Generic;

namespace Kingbot.Modules
{
    class Quotes
    {
        /*
         * Handles all quote related commands
         * Links up to the mongodb database for reading
         * and saying in the twitch client.
         */

        public static async Task Handle(List<String> words)
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
                    await AddQuote(index, message);
                    break;
                case "delete":
                    await DataHelper.Delete("quotes", index);
                    break;
            }
        }

        // TODO: Make EnsureQuote work.
        /*
        private static async Task<bool> EnsureQuote(string index)
        {
            if (Convert.ToBoolean(await DataHelper.ReturnQuote(index, "message"))) 
            {
                return true;
            }

            return false;
        }
        */

        /*
         * Get the quote from the database and return it to the
         * calling function.
         */
        private static async Task<string> ReturnQuote(string index)
        {
            var result = await DataHelper.Fetch("quotes", index, "message");

            if (result == null)
                return "This quote doesn't exist! Try adding it?";
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
            var doc = new BsonDocument
            {
                {"index", index},
                {"message", message}
            };

            await DataHelper.Create("quotes", doc);
        }
    }
}
