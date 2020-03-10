using System;

namespace Kingbot
{
    class Program
    {
        static void Main(string[] args)
        {
            TwitchBot bot  = new TwitchBot();

            bot.Connect();

            Console.ReadLine();

            bot.Disconnect();
        }
    }
}
