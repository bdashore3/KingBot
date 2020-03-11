using System;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using Kingbot.Commands;
using Kingbot.Security;
using Kingbot.Helpers.Data;

namespace Kingbot
{
    class TwitchBot
    {
        public static TwitchClient client;
        public static string channel;

        public void Connect()
        {
            bool logging = false;
            client = new TwitchClient();
            CredentialsHelper.ReadInfo();
            DataHelper.InitDB();
            ConnectionCredentials credentials = new ConnectionCredentials(CredentialsHelper.info.BotUsername, CredentialsHelper.info.BotToken);
            channel = CredentialsHelper.info.Channel;
            Console.WriteLine("Connecting...");
            client.Initialize(credentials, channel);

            if (logging) 
                client.OnLog += Client_OnLog;

            client.OnConnectionError += Client_OnConnectionError;
            client.OnMessageReceived += Client_OnMessageReceived;

            client.Connect();
            Console.WriteLine($"Connected to {CredentialsHelper.info.Channel}");
        }

        public void Disconnect()
        {
            Console.WriteLine("Disconnecting...");
        }

        private void Client_OnConnectionError(object sender, OnConnectionErrorArgs e)
        {
            Console.WriteLine($"Error! {e.Error}");
        }

        private void Client_OnLog(object sender, OnLogArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (e.ChatMessage.Message.Contains(CredentialsHelper.info.Prefix))
            {
                Console.WriteLine($"Command Recieved: {e.ChatMessage.Message}");
                CommandHandler.HandleCommand(e.ChatMessage.Message);
            }
        }
    }
}
