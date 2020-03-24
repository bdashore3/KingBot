namespace Kingbot.Modules
{
    class Other
    {
        /*
         * Command Module for all other commands
         * This is a miscellaneous commands file.
         */
        static string channel = TwitchBot.channel;
        public static void Ping()
        {
            TwitchBot.client.SendMessage(channel, "Pong!");
        }

        public static void Shoutout(string username)
        {
            TwitchBot.client.SendMessage(channel, $"Hey! This streamer is cool! Go support at: https://twitch.tv/{username}");
        }
    }
}
