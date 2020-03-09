using System;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using Kingbot.Commands;
using Kingbot.Security;

namespace Kingbot
{
    internal class TwitchBot
    {
        public static TwitchClient client;

        public void Connect()
        {
            bool logging = false;

            Console.WriteLine("Connecting...");
            client = new TwitchClient();
            CredentialsHelper.ReadInfo();
            ConnectionCredentials credentials = new ConnectionCredentials(CredentialsHelper.info.BotUsername, CredentialsHelper.info.BotToken);
            client.Initialize(credentials, CredentialsHelper.info.Channel);

            if (logging) 
                client.OnLog += Client_OnLog;

            client.OnConnectionError += Client_OnConnectionError;
            client.OnMessageReceived += client_OnMessageReceived;

            client.Connect();
        }

        private void Disconnect()
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

        private void client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (e.ChatMessage.Message.Contains(CredentialsHelper.info.Prefix))
            {
                Console.WriteLine($"Command Recieved: {e.ChatMessage.Message}");
                CommandHandler.HandleCommand(e.ChatMessage.Message);
            }
        }
    }
}
