using System;
using System.Collections.Generic;
using System.Text;

namespace Kingbot.Data
{
    class Quote
    {
        public readonly string Index;
        public readonly string Message;

        public Quote(string index, string message)
        {
            Index = index;
            Message = message;
        }
    }
}
