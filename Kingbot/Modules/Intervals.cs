using Kingbot.Helpers.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace Kingbot.Modules
{
    class Intervals
    {
        private readonly DataIntervals _data;
        
        public Intervals(DataIntervals data)
        {
            _data = data;
        }

        // Dictionary for storing all the timers. This is the heart of the interval system
        private Dictionary<string, Timer> intervals = new Dictionary<string, Timer>();

        // Handles command string given from the CommandHandler
        public async Task Handle(List<String> words, string username)
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
                    if (await _data.EnsureInterval(name))
                    {
                        TwitchBot.client.SendMessage(TwitchBot.channel, $"Interval message {name} already exists!");
                        break;
                    }
                    await AddInterval(name, message);
                    break;
                case "remove":
                    await _data.DeleteInterval(name);
                    TwitchBot.client.SendMessage(TwitchBot.channel, $"Deleted interval message {name}!");
                    break;
            }
        }

        // Add a new interval phrase and message into the database
        private async Task AddInterval(string name, string message)
        {
            Interval FileToAdd = new Interval
            {
                Index = name,
                Message = message
            };
            await _data.WriteInterval(FileToAdd);
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
        public async Task StartInterval(string name, int ms)
        {
            string message = await _data.ReadInterval(name);
            intervals[name] = new Timer(ms);
            intervals[name].Elapsed += (sender, e) => OnTimedEvent(sender, message);
            intervals[name].AutoReset = true;
            intervals[name].Start();
        }

        private void OnTimedEvent(object sender, string message)
        {
            TwitchBot.client.SendMessage(TwitchBot.channel, message);
        }

        /*
         * Stops already existing interval message.
         * Public function due to API access for stream starts
         */

        public void StopInterval(string name)
        {
            if (intervals.ContainsKey(name))
                intervals[name].Stop();
            else
                TwitchBot.client.SendMessage(TwitchBot.channel, $"Interval {name}! Perhaps you never started it?");
        }
    }
}
