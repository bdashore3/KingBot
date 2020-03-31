using System;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Api;
using TwitchLib.Api.Services;
using Kingbot.Commands;
using Kingbot.Helpers.Security;
using Kingbot.Helpers.API;
using TwitchLib.Api.Services.Events;

namespace Kingbot
{
    class TwitchBot
    {
        // Make the client and channel have scope across all files
        public static TwitchClient client;
        public static TwitchAPI api;

        // Variables for Dependency injection
        private static LiveStreamMonitorService Monitor;
        private static FollowerService FollowMonitor;
        public static string channel;
        private readonly CommandHandler _commandHandler;
        private readonly ApiHelper _apiHelper;

        public TwitchBot(CommandHandler commandHandler, ApiHelper apiHelper)
        {
            _commandHandler = commandHandler;
            _apiHelper = apiHelper;
        }

        // From Program.cs. Starts the bot
        public async Task Start()
        {
            Connect();
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
        private void Connect()
        {
            bool logging = false;
            client = new TwitchClient();
            api = new TwitchAPI();
            StartApi();
            ConnectionCredentials credentials = new ConnectionCredentials(CredentialsHelper.BotUsername, CredentialsHelper.BotToken);
            channel = CredentialsHelper.Channel;
            Console.WriteLine("Connecting...");
            client.Initialize(credentials, channel);

            if (logging) 
                client.OnLog += Client_OnLog;

            client.OnConnectionError += Client_OnConnectionError;
            client.OnMessageReceived += _commandHandler.OnMessageReceived;

            client.OnNewSubscriber += _apiHelper.OnNewSubscriber;
            client.OnReSubscriber += _apiHelper.OnReSubscriber;
            client.OnGiftedSubscription += _apiHelper.OnGiftedSubscription;

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
        private void StartApi()
        {
            api.Settings.ClientId = CredentialsHelper.ApiId;
            api.Settings.AccessToken = CredentialsHelper.ApiToken;

            Console.WriteLine("Starting services...");

            Monitor = new LiveStreamMonitorService(api, 60);
            FollowMonitor = new FollowerService(api);

            // This stops follower spam on startup


            Monitor.SetChannelsByName(ChannelProvider.GetChannels());
            FollowMonitor.SetChannelsByName(ChannelProvider.GetChannels());

            Monitor.OnServiceStarted += StreamMonitor_OnServiceStarted;
            Monitor.OnStreamOnline += _apiHelper.OnStreamOnline;
            Monitor.OnStreamOffline += _apiHelper.OnStreamOffline;

            FollowMonitor.OnServiceStarted += FollowMonitor_OnServiceStarted;
            FollowMonitor.OnNewFollowersDetected += _apiHelper.OnNewFollow;

            FollowMonitor.Start();
            // This fixes follow chat spam on startup
            FollowMonitor.UpdateLatestFollowersAsync(false);
            Monitor.Start();

            Console.WriteLine("Services are started.");
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

        public void StreamMonitor_OnServiceStarted(object sender, OnServiceStartedArgs e)
        {
            Console.WriteLine("Stream Monitor service started!");
        }

        public void FollowMonitor_OnServiceStarted(object sender, OnServiceStartedArgs e)
        {
            Console.WriteLine("Follow service started!");
        }
    }
}
