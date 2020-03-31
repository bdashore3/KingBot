using Kingbot.Modules;
using System;
using TwitchLib.Api.Services.Events.FollowerService;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.Client.Events;

namespace Kingbot.Helpers.API
{
    class ApiHelper
    {
        private readonly Lurk _lurk;
        private readonly Intervals _intervals;
        public ApiHelper(Lurk lurk, Intervals intervals)
        {
            _lurk = lurk;
            _intervals = intervals;
        }

        // When the stream is online, do this
        public void OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            TwitchBot.client.SendMessage(TwitchBot.channel, "The stream is LIVE! Get turnt!");
            // Add function calls here. Make sure to make them public!
        }

        // When the stream is offline, do this
        public void OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            TwitchBot.client.SendMessage(TwitchBot.channel, "We are now offline...");
            _lurk.ClearLurkDict();
            _intervals.ClearIntervalDict();

            // Same as above.
        }

        public void OnNewFollow(object sender, OnNewFollowersDetectedArgs e)
        {
            foreach (var follower in e.NewFollowers)
            {
                Console.WriteLine($"Hey @{follower.FromUserName}! Thanks for the follow! I really appreciate it!");
            }
        }

        public void OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            TwitchBot.client.SendMessage(TwitchBot.channel, $"Wow... @{e.Subscriber.DisplayName} just subscribed for the first time! Enjoy the emotes and welcome!");
        }

        public void OnReSubscriber(object sender, OnReSubscriberArgs e)
        {
            TwitchBot.client.SendMessage(TwitchBot.channel, $"Omigosh! @{e.ReSubscriber.DisplayName} just resubscribed for {e.ReSubscriber.Months}! You're amazing!");
        }

        public void OnGiftedSubscription(object sender, OnGiftedSubscriptionArgs e)
        {
            TwitchBot.client.SendMessage(TwitchBot.channel, $"Daaaaaaammmm! Thanks @{e.GiftedSubscription.DisplayName} for the subs! You're a GOAT");
        }
    }
}
