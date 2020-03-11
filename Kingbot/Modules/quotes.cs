using System.Threading.Tasks;
using Kingbot.Helpers.Data;
using MongoDB.Bson;

namespace Kingbot.Modules
{
    class quotes
    {
        public static async Task<string> ReturnQuote(string index)
        {
            var result = await DataHelper.GetQuote(index, "message");

            if (result == null)
                return "This quote doesn't exist! Try adding it?";
            else
                return result;
        }

        public static async Task AddQuote(string index, string message)
        {
            var doc = new BsonDocument
            {
                {"index", index},
                {"message", message}
            };

            await DataHelper.Create("quotes", doc);
        }

        public static async Task RemoveQuote(string index)
        {
            await DataHelper.Delete("quotes", index);
        }
    }
}
