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

        public async Task Handle(List<string> words, string username, bool IsMod, string Id)
        {
            switch (words[1].ToLower())
            {
                case "retrieve":
                    TwitchBot.client.SendMessage(TwitchBot.channel, await ReturnQuote(words[2]));
                    break;
                case "add":
                    string addIndex = words[2];
                    words.RemoveRange(0, 3);
                    string message = String.Join(" ", words.ToArray());

                    if (await _data.Ensure(addIndex))
                    {
                        TwitchBot.client.SendMessage(TwitchBot.channel, $"Quote {addIndex} already exists!");
                        break;
                    }
                    await AddQuote(addIndex, message);
                    break;
                case "remove":
                    if (!CredentialsHelper.CheckAdmin(IsMod, Id))
                        break;

                    if (await _data.Ensure(words[2]))
                    {
                        TwitchBot.client.SendMessage(TwitchBot.channel, $"Quote {words[2]} already exists!");
                        break;
                    }
                    await _data.Delete(words[2]);
                    TwitchBot.client.SendMessage(TwitchBot.channel, $"Quote {words[2]} successfully deleted!");
                    break;
                case "list":
                    await _data.GetList("Quotes", username);
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
