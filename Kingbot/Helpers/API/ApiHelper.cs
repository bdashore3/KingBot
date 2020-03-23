using System;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace Kingbot.Helpers.API
{
    class ApiHelper
    {
        // When the stream is online, do this
        public async void OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            TwitchBot.client.SendMessage(TwitchBot.channel, "The stream is LIVE! Get turnt!");
            // Add function calls here. Make sure to make them public!
        }

        // When the stream is offline, do this
        public async void OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            TwitchBot.client.SendMessage(TwitchBot.channel, "We are now offline...");
            // Same as above.
        }
    }
}
