using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kingbot.Modules;

namespace Kingbot.Commands
{
    class CommandHandler
    {
        public static async Task HandleCommand(string og)
        {
            string msg = og.ToLower().Substring(1);
            List<String> words = msg.Split(" ").ToList();
            string command = words[0];
            string channel = TwitchBot.channel;

            switch (command)
            {
                case "ping":
                    Console.WriteLine("Command Ping Recieved");
                    TwitchBot.client.SendMessage(channel, "Pong");
                    break;

                case "quote":
                    {
                        Console.WriteLine("Command Quote Recieved");
                        string instruction = words[1];
                        string index = words[2];

                        switch (instruction)
                        {
                            case "retrieve":
                                TwitchBot.client.SendMessage(channel, await quotes.ReturnQuote(index));
                                break;
                            case "add":
                                words.RemoveRange(0, 3);
                                string message = String.Join(" ", words.ToArray());
                                await quotes.AddQuote(index, message);
                                break;
                            case "delete":
                                await quotes.RemoveQuote(index);
                                break;
                        }
                        break;
                    }
            }
        }
    }
}
