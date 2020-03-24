using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kingbot.Helpers.Data;
using Kingbot.Helpers.Security;
using Kingbot.Modules;
using TwitchLib.Client;
using TwitchLib.Client.Events;

namespace Kingbot.Commands
{
    class CommandHandler
    {
        // Global variables for use in the handler
        public static TwitchClient client = TwitchBot.client;
        public static string channel = TwitchBot.channel;

        // Private variables for dependency injection references
        private readonly Quotes _quotes;
        private readonly Intervals _intervals;
        private readonly Custom _custom;
        private readonly DatabaseHelper<Command> _dataCommands;

        // Since dependency injection is cascading, put a constructor here to assign variables.
        public CommandHandler(Quotes quotes, Intervals intervals, Custom custom, DatabaseHelper<Command> dataCommands)
        {
            _quotes = quotes;
            _intervals = intervals;
            _custom = custom;
            _dataCommands = dataCommands;
        }

        // If a message contains the prefix, handle it. The try/catch is to prevent crashing of the program
        public async void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (e.ChatMessage.Message.Contains(CredentialsHelper.Prefix))
            {
                try
                {
                    if (e.ChatMessage.IsModerator || e.ChatMessage.IsBroadcaster)
                        await HandleCommand(e.ChatMessage.Message, e.ChatMessage.Username, e.ChatMessage.DisplayName, true);
                    else
                        await HandleCommand(e.ChatMessage.Message, e.ChatMessage.Username, e.ChatMessage.DisplayName, false);
                }
                catch
                {
                    client.SendMessage(channel, "This command syntax doesn't work! Check the help?");
                }
            }
        }

        private async Task HandleCommand(string og, string username, string displayName, bool IsMod)
        {
            /*
             * Flow:
             * 1. Remove the message prefix
             * 2. Convert the words into a List (Not an array!)
             * 3. Make the command lowercase for the switch statement
             */

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
                    await _quotes.Handle(words, username, IsMod);
                    break;

                case "interval":
                    if (!CredentialsHelper.CheckAdmin(IsMod))
                        break;
                    await _intervals.Handle(words, username);
                    break;

                case "command":
                    if (!CredentialsHelper.CheckAdmin(IsMod))
                        break;
                    await _custom.Handle(words, username);
                    break;

                case "so":
                case "shoutout":
                    if (!CredentialsHelper.CheckAdmin(IsMod))
                        break;
                    Other.Shoutout(displayName);
                    break;
            }

            // If the command exists in custom commands, send the message
            if (await _dataCommands.Ensure(command))
                TwitchBot.client.SendMessage(TwitchBot.channel, await _dataCommands.Read(command));
        }
    }
}
