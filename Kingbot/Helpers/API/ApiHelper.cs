using TwitchLib.Api.Services.Events.FollowerService;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.Client.Events;

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

        public async void OnNewFollow(object sender, OnNewFollowersDetectedArgs e)
        {
            foreach (var follower in e.NewFollowers)
            {
                TwitchBot.client.SendMessage(TwitchBot.channel, $"Hey {follower.ToUserName}! Thanks for the follow! I really appreciate it!");
            }
        }

        public async void OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            TwitchBot.client.SendMessage(TwitchBot.channel, $"Wow... {e.Subscriber.DisplayName} just subscribed for the first time! Enjoy the emotes and welcome!");
        }

        public async void OnReSubscriber(object sender, OnReSubscriberArgs e)
        {
            TwitchBot.client.SendMessage(TwitchBot.channel, $"Omigosh! {e.ReSubscriber.DisplayName} just resubscribed for {e.ReSubscriber.Months}! You're amazing!");
        }

        public async void OnGiftedSubscription(object sender, OnGiftedSubscriptionArgs e)
        {
            TwitchBot.client.SendMessage(TwitchBot.channel, $"Daaaaaaammmm! Thanks {e.GiftedSubscription.DisplayName} for the subs! You're a GOAT");
        }

        public async void OnAnonGiftedSubscription(object sender, OnAnonGiftedSubscriptionArgs e)
        {
            TwitchBot.client.SendMessage(TwitchBot.channel, "Daaaaaaammmm! Thanks for the subs! Even though you're anonymous, you're still appreciated!");
        }

        public async void OnBeingHosted(object sender, OnBeingHostedArgs e)
        {
            TwitchBot.client.SendMessage(TwitchBot.channel, $"Heya! Thanks for the host {e.BeingHostedNotification.HostedByChannel}!");
        }
    }
}
