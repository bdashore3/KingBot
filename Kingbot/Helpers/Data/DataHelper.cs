using System;
using Kingbot.Helpers.Security;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Kingbot.Helpers.Data
{
    class DataHelper
    {
        // Connect and set the database we're using
        private static MongoClient dbClient = new MongoClient(CredentialsHelper.MongoConnection);
        private static IMongoDatabase SelfDB;
        public static void InitDB()
        {
            SelfDB = dbClient.GetDatabase(CredentialsHelper.SelfDB);
        }

        // Generic method to fetch info from the database
        public static async Task<string> Fetch(string CollectionName, string value, string result)
        {
            var collection = SelfDB.GetCollection<BsonDocument>(CollectionName);

            if (collection == null)
                return null;

            var filter = Builders<BsonDocument>.Filter.Eq("index", value);
            var firstDocument = await collection.Find(filter).FirstOrDefaultAsync();

            if (firstDocument == null)
                return null;

            return (firstDocument.GetElement(result).Value.ToString());
        }

        // Generic method to create a new entry in the database
        public static async Task Create(string list, BsonDocument document)
        {
            var collection = SelfDB.GetCollection<BsonDocument>(list);
            await collection.InsertOneAsync(document);
        }

        // Generic method to delete from the database
        public static async Task Delete(string list, string value)
        {
            var collection = SelfDB.GetCollection<BsonDocument>(list);
            var filter = Builders<BsonDocument>.Filter.Eq("index", value);
            await collection.DeleteOneAsync(filter);
            Console.WriteLine("Deleted successfully");
        }

        public static async Task<long> GetAmount(string list)
        {
            var collection = SelfDB.GetCollection<BsonDocument>(list);
            long result = await collection.CountDocumentsAsync(new BsonDocument());
            return result;
        }

        public static async Task GetCommands()
        {
            long num = await GetAmount("commands");

            ConcurrentDictionary<string, string> custom = new ConcurrentDictionary<string, string>();

            var collection = SelfDB.GetCollection<BsonDocument>("commands");

            int n = 0;
            while (n <= num) 
            {

            }
        }

        /*
        public static async Task ConvertToJson()
        {
            var collection = SelfDB.GetCollection<BsonDocument>("commands");
            var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Strict };
            JObject json = JObject.Parse(collection.ToJson<MongoDB.Bson.BsonDocument>(jsonWriterSettings));
        }
        */

        // TODO: Add a method to ensure a key is in the database

        public static async Task<bool> Ensure(string name)
        {
            if (await Fetch("commands", name, "message") == null)
            {
                return false;
            }

            return true;
        }
    }
}
