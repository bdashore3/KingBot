using System;
using Kingbot.Helpers.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kingbot.Helpers.Security;

namespace Kingbot.Modules
{
    class Quotes
    {
        // For dependency injection
        private readonly DatabaseHelper<Quote> _data;
        public Quotes(DatabaseHelper<Quote> data)
        {
            _data = data;
        }

        /*
         * Handles all quote related commands
         * Links up to the Postgres database for reading
         * and saying in the twitch client.
         */

        public async Task Handle(List<string> words, string username, bool IsMod)
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
                    if (await _data.Ensure(index))
                    {
                        TwitchBot.client.SendMessage(TwitchBot.channel, $"Quote {index} already exists!");
                        break;
                    }
                    await AddQuote(index, message);
                    break;
                case "remove":
                    if (!CredentialsHelper.CheckAdmin(IsMod))
                        break;
                    await _data.Delete(index);
                    TwitchBot.client.SendMessage(TwitchBot.channel, $"Quote {index} successfully deleted!");
                    break;
            }
        }

        /*
         * Get the quote from the database and return it to the
         * calling function.
         */

        private async Task<string> ReturnQuote(string index)
        {
            var result = await _data.Read(index);

            if (result == null)
                return $"Quote {index} doesn't exist! Try adding it?";
            else
                return result;
        }


        /*
         * Add a new quote into the database
         * 
         * Quotes cannot be updated, they have to be removed first
         * by an admin.
         */

        private async Task AddQuote(string index, string message)
        {
            Quote FileToAdd = new Quote
            {
                Index = index,
                Message = message
            };
            await _data.Write(FileToAdd);
            TwitchBot.client.SendMessage(TwitchBot.channel, $"Quote {index} successfully written!");
        }
    }
}
