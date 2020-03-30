using Kingbot.Helpers.Security;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Kingbot.Modules
{
    class Lurk
    {
        private ConcurrentDictionary<string, long> lurkTimes = new ConcurrentDictionary<string, long>(); 
        public void Handle(List<string> words, string username)
        {
            if (words.Count < 2)
                words.Add("0");

            if (words[1] == "cancel")
            {
                CancelLurk(username);
                return;
            }

            RetrieveLurks(username);
        }

        private void RetrieveLurks(string username)
        {
            long curTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (!lurkTimes.ContainsKey(username))
            {
                lurkTimes[username] = curTime;
                TwitchBot.client.SendMessage(TwitchBot.channel, $"User @{username} is now lurking!");
                return;
            }
            TimeSpan newTime = TimeSpan.FromSeconds(curTime - lurkTimes[username]);
            TwitchBot.client.SendMessage(TwitchBot.channel, $"@{username} has been lurking for: {newTime.Hours} hours, {newTime.Minutes} minutes, {newTime.Seconds} seconds");
        }

        public void CancelLurk(string username)
        {
            if (!lurkTimes.TryRemove(username, out long value1))
            {
                TwitchBot.client.SendMessage(TwitchBot.channel, "You're not lurking! Or at least you didn't indicate that to me...");
                TwitchBot.client.SendMessage(TwitchBot.channel, $"Use {CredentialsHelper.Prefix}lurk to start the clock @{username}!");
            }
            return;
        }
    }
}
