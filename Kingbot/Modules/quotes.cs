using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kingbot.Data;
using Kingbot.Helpers.Data;

namespace Kingbot.Modules
{
    class quotes
    {
        public static async Task<string> PostQuote(string index)
        {
            Quote quote = await DataHelper.GetQuote(index);
            return quote.Message;
        }
    }
}
