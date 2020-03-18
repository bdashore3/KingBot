using Kingbot.Helpers.Data;
using MongoDB.Bson;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kingbot.Modules
{
    class Custom
    {
        public static ConcurrentDictionary<string, string> commands = new ConcurrentDictionary<string, string>();
        public static async Task Handle(List<String> words)
        {
            string instruction = words[1].ToLower();

            switch(instruction)
            {
                case "add":
                    string name = words[2];
                    words.RemoveRange(0, 3);
                    string message = String.Join(" ", words.ToArray());

                    await AddCommand(name, message);
                    break;
                case "remove":
                    string dataname = words[2];
                    await DataHelper.Delete("quotes", dataname);
                    break;
                case "fetch":
                    await DataHelper.Ensure("bruh");
                    break;
            }
        }

        private static async Task AddCommand(string name, string message)
        {
            var doc = new BsonDocument
            {
                {"index", name},
                {"message", message}
            };

            await DataHelper.Create("commands", doc);
        }
    }
}
