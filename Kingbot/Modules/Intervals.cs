using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Kingbot.Helpers.Data;

namespace Kingbot.Modules
{
    class Intervals
    {
        // For dependency injection
        private readonly DatabaseHelper<Interval> _data;
        public Intervals(DatabaseHelper<Interval> data)
        {
            _data = data;
        }

        // Dictionary for storing all the timers. This is the heart of the interval system
        private ConcurrentDictionary<string, Timer> intervals = new ConcurrentDictionary<string, Timer>();

        // Handles command string given from the CommandHandler
        public async Task Handle(List<String> words, string username)
        {
            switch (words[1].ToLower())
            {
                case "start":
                    string ms = words[3];

                    if (!await _data.Ensure(words[2]))
                    {
                        TwitchBot.client.SendMessage(TwitchBot.channel, $"Interval message {words[2]} doesn't exist! Try adding it?");
                        break;
                    }
                    await StartInterval(words[2], int.Parse(ms));
                    break;
                case "stop":
                    StopInterval(words[2]);
                    break;
                case "add":
                    string addIndex = words[2];
                    words.RemoveRange(0, 3);
                    string message = String.Join(" ", words.ToArray());

                    if (await _data.Ensure(addIndex))
                    {
                        TwitchBot.client.SendMessage(TwitchBot.channel, $"Interval message {addIndex} already exists!");
                        break;
                    }
                    await AddInterval(addIndex, message);
                    break;
                case "remove":
                    await _data.Delete(words[2]);
                    TwitchBot.client.SendMessage(TwitchBot.channel, $"Deleted interval message {words[2]}!");
                    break;
                case "list":
                    await _data.GetList("Intervals", username);
                    break;
                case "clear":
                    ClearIntervalDict();
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
            await _data.Write(FileToAdd);
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
            string message = await _data.Read(name);
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
            {
                intervals[name].Stop();
                intervals.TryRemove(name, out Timer value);
            }
            else
                TwitchBot.client.SendMessage(TwitchBot.channel, $"Interval {name} doesn't exist! Perhaps you never started it?");
        }

        public void ClearIntervalDict()
        {
            intervals.Clear();
        }
    }
}
