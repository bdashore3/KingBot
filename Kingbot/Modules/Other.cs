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
    }
}
