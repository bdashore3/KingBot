using Kingbot.Helpers.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace Kingbot.Modules
{
    class Interval
    {
        // Dictionary for storing all the timers. This is the heart of the interval system
        private static Dictionary<string, Timer> intervals = new Dictionary<string, Timer>();

        // Handles command string given from the CommandHandler
        public static async Task Handle(List<String> words)
        {
            string instruction = words[1].ToLower();
            string name = words[2];

            switch (instruction)
            {
                case "start":
                    string ms = words[3];

                    await StartInterval(name, int.Parse(ms));
                    break;
                case "stop":
                    StopInterval(name);
                    break;
                case "add":
                    words.RemoveRange(0, 3);
                    string message = String.Join(" ", words.ToArray());
                    if (await DataHelper.Ensure("intervals", name))
                    {
                        TwitchBot.client.SendMessage(TwitchBot.channel, $"Interval message {name} already exists!");
                        break;
                    }
                    await AddInterval(name, message);
                    break;
                case "remove":
                    await DataHelper.Delete("intervals", name);
                    TwitchBot.client.SendMessage(TwitchBot.channel, $"Deleted the interval message: {name}");
                    break;
            }
        }

        // Add a new interval phrase and message into the database
        
        // TODO: Add ensure check
        private static async Task AddInterval(string name, string message)
        {
            await DataHelper.Write("intervals", name, message);
            TwitchBot.client.SendMessage(TwitchBot.channel, $"Interval message {name} written!");
        }

        /*
         * Flow
         * 1. Store the name and the interval timer inside the dictionary for easy access
         * 2. Set the interval's ms, event, and start it
         * 3. Keep posting a message to the channel until the stop command is executed
         * 
         * Public function due to API access for stream starts
         */
        public static async Task StartInterval(string name, int ms)
        {
            string message = await DataHelper.Read("intervals", name);
            intervals[name] = new Timer(ms);
            intervals[name].Elapsed += (sender, e) => OnTimedEvent(sender, message);
            intervals[name].AutoReset = true;
            intervals[name].Start();
        }

        private static void OnTimedEvent(object sender, string message)
        {
            TwitchBot.client.SendMessage(TwitchBot.channel, message);
        }

        /*
         * Stops already existing interval message.
         * Public function due to API access for stream starts
         */

        public static void StopInterval(string name)
        {
            if (intervals.ContainsKey(name))
                intervals[name].Stop();
            else
                TwitchBot.client.SendMessage(TwitchBot.channel, $"Interval {name}! Perhaps you never started it?");
        }
    }
}
