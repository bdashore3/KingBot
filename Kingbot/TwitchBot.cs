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
        public static TwitchClient client;
        public static string channel;

        public static async Task Start(string CredsPath)
        {
            await Connect(CredsPath);

            Console.ReadLine();

            Disconnect();
        }

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

        private static void Disconnect()
        {
            Console.WriteLine("Disconnecting...");
        }

        private static void Client_OnConnectionError(object sender, OnConnectionErrorArgs e)
        {
            Console.WriteLine($"Error! {e.Error}");
        }

        private static void Client_OnLog(object sender, OnLogArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private static async void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (e.ChatMessage.Message.Contains(CredentialsHelper.Prefix))
            {
                Console.WriteLine($"Command Recieved: {e.ChatMessage.Message}");
                await CommandHandler.HandleCommand(e.ChatMessage.Message);
            }
        }
    }
}
