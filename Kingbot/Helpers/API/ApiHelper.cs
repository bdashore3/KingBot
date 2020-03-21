using System;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace Kingbot.Helpers.API
{
    class ApiHelper
    {
        // When the stream is online, do this
        public static async void OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            Console.WriteLine("The stream is LIVE! Get turnt!");
            TwitchBot.client.SendMessage(TwitchBot.channel, "The stream is LIVE! Get turnt!");
        }

        // When the stream is offline, do this
        public static async void OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            Console.WriteLine("We are now offline...");
            TwitchBot.client.SendMessage(TwitchBot.channel, "We are now offline...");
        }
    }
}
