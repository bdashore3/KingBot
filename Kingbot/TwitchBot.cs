using System;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Api;
using TwitchLib.Api.Services;
using Kingbot.Commands;
using Kingbot.Helpers.Security;
using Kingbot.Helpers.Data;
using Kingbot.Helpers.API;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Kingbot
{
    class TwitchBot
    {
        // Make the client and channel have scope across all files
        public static TwitchClient client;
        public static TwitchAPI api;
        private static LiveStreamMonitorService Monitor;
        public static string channel;
        private readonly CommandHandler _commandHandler;

        public TwitchBot(CommandHandler commandHandler)
        {
            _commandHandler = commandHandler;
        }

        // From Program.cs. Starts the bot
        public async Task Start(string CredsPath)
        {
            await Connect(CredsPath);
            await Task.Delay(-1);
        }

        /*
         * Flow:
         * 1. Create a new TwitchClient instance
         * 2. Create a new API client instance
         * 3. Read the credentials from CredentialsHelper
         * 4. Connect to the Database
         * 5. Startup the API monitor service
         * 6. Set all credentials and the channel variable
         * 7. Start our client and listen to events
         */
        private async Task Connect(string CredsPath)
        {
            bool logging = false;
            client = new TwitchClient();
            api = new TwitchAPI();
            CredentialsHelper.ReadCreds(CredsPath);
            await Task.Run(() => StartApi());
            ConnectionCredentials credentials = new ConnectionCredentials(CredentialsHelper.BotUsername, CredentialsHelper.BotToken);
            channel = CredentialsHelper.Channel;
            Console.WriteLine("Connecting...");
            client.Initialize(credentials, channel);

            if (logging) 
                client.OnLog += Client_OnLog;

            client.OnConnectionError += Client_OnConnectionError;
            client.OnMessageReceived += _commandHandler.OnMessageReceived;

            client.Connect();
            Console.WriteLine($"Connected to {channel}");
        }

        /*
         * Put the API handler here since we're starting it in
         * the same file and we can easily access this file if we
         * need to edit any code.
         * 
         * Pass all events to the ApiHelper which interprets
         * them to clean up any cruft.
         */
        public async Task StartApi()
        {
            api.Settings.ClientId = CredentialsHelper.ApiId;
            api.Settings.AccessToken = CredentialsHelper.ApiToken;

            Console.WriteLine("Starting API service...");

            ApiHelper apiHelper = new ApiHelper();

            Monitor = new LiveStreamMonitorService(api, 60);

            Monitor.SetChannelsByName(ChannelProvider.GetChannels());

            Monitor.OnStreamOnline += apiHelper.OnStreamOnline;
            Monitor.OnStreamOffline += apiHelper.OnStreamOffline;

            Monitor.Start();

            Console.WriteLine("Api service started.");
        }

        // If there's an error, log it.
        private void Client_OnConnectionError(object sender, OnConnectionErrorArgs e)
        {
            Console.WriteLine($"Error! {e.Error}");
        }

        // If logging is enabled in Connect(), put logs in console
        private void Client_OnLog(object sender, OnLogArgs e)
        {
            Console.WriteLine(e.Data);
        }
    }
}
