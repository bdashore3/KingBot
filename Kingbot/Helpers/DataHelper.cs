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

        public static async Task<string> GetQuote(string value, string result)
        {
            var collection = SelfDB.GetCollection<BsonDocument>("quotes");

            if (collection == null)
                return null;

            var filter = Builders<BsonDocument>.Filter.Eq("index", value);
            var firstDocument = await collection.Find(filter).FirstOrDefaultAsync();

            if (firstDocument == null)
                return null;

            return (firstDocument.GetElement(result).Value.ToString());
        }

        public static async Task Create(string list, BsonDocument document)
        {
            var collection = SelfDB.GetCollection<BsonDocument>(list);
            await collection.InsertOneAsync(document);
        }

        public static async Task Delete(string list, string value)
        {
            var collection = SelfDB.GetCollection<BsonDocument>(list);
            var filter = Builders<BsonDocument>.Filter.Eq("index", value);
            await collection.DeleteOneAsync(filter);
            Console.WriteLine("Deleted successfully");
        }
    }
}
