using Kingbot.Helpers.Data;
using System;
using System.Collections.Generic;

namespace Kingbot.Modules
{
    class Custom
    {
        public static void Handle(List<String> words)
        {
            string instruction = words[1].ToLower();
            string name = words[2];

            switch (instruction)
            {
                case "add":
                    words.RemoveRange(0, 3);
                    string message = String.Join(" ", words.ToArray());

                    AddCommand(name, message);
                    break;
                case "remove":
                    DataHelper.Delete("commands", name);
                    break;
            }
        }

        private static void AddCommand(string name, string message)
        {
            DataHelper.Write("commands", name, message);
        }
    }
}
