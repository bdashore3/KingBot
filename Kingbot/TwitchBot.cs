using System;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using Kingbot.Commands;
using Kingbot.Helpers.Security;
using Kingbot.Helpers.Data;
using System.Threading.Tasks;

namespace Kingbot
{
    class TwitchBot
    {
        // Make the client and channel have scope across all files
        public static TwitchClient client;
        public static string channel;

        // From Program.cs. Starts the bot
        public static async Task Start(string CredsPath)
        {
            await Connect(CredsPath);
            await Task.Delay(-1);
        }

        /*
         * Flow:
         * 1. Create a new TwitchClient instance
         * 2. Read the credentials from CredentialsHelper
         * 3. Connect to the Database
         * 4. Set all credentials and the channel variable
         * 5. Start our client and listen to events
         */
        private static async Task Connect(string CredsPath)
        {
            bool logging = false;
            client = new TwitchClient();
            await CredentialsHelper.ReadCreds(CredsPath);
            DataHelper.InitDB();
            ConnectionCredentials credentials = new ConnectionCredentials(CredentialsHelper.BotUsername, CredentialsHelper.BotToken);
            channel = CredentialsHelper.Channel;
            Console.WriteLine("Connecting...");
            client.Initialize(credentials, channel);

            if (logging) 
                client.OnLog += Client_OnLog;

            client.OnConnectionError += Client_OnConnectionError;
            client.OnMessageReceived += Client_OnMessageReceived;

            client.Connect();
            Console.WriteLine($"Connected to {channel}");
        }

        // If there's an error, log it.
        private static void Client_OnConnectionError(object sender, OnConnectionErrorArgs e)
        {
            Console.WriteLine($"Error! {e.Error}");
        }

        // If logging is enabled in Connect(), put logs in console
        private static void Client_OnLog(object sender, OnLogArgs e)
        {
            Console.WriteLine(e.Data);
        }

        // If a message contains the prefix, handle it. The try/catch is to prevent crashing of the program
        private static void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (e.ChatMessage.Message.Contains(CredentialsHelper.Prefix))
            {
                Console.WriteLine($"Command Recieved: {e.ChatMessage.Message}");
                try
                {
                    CommandHandler.HandleCommand(e.ChatMessage.Message);
                }
                catch
                {
                    client.SendMessage(channel, "This command syntax doesn't work! Check the help?");
                }
            }
        }
    }
}
