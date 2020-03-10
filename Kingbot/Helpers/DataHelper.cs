using System;
using System.Collections.Generic;
using System.Text;
using Kingbot.Security;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;
using Kingbot.Data;

namespace Kingbot.Helpers.Data
{
    class DataHelper
    {
        private static MongoClient dbClient = new MongoClient(CredentialsHelper.info.MongoConnection);
        private static IMongoDatabase SelfDB;
        public static void InitDB()
        {
            SelfDB = dbClient.GetDatabase(CredentialsHelper.info.SelfDB);
        }

        public static async Task<Quote> GetQuote(string quotes)
        {
            var collection = SelfDB.GetCollection<BsonDocument>("quotes");

            if (collection == null)
                return null;

            var filter = Builders<BsonDocument>.Filter.Eq("index", quotes);
            var firstDocument = await collection.Find(filter).FirstOrDefaultAsync();

            if (firstDocument == null)
                return null;

            return new Quote(quotes, firstDocument.GetElement("message").Value.ToString());
        }
    }
}
