using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Kingbot.Helpers.Security
{
    class CredentialsHelper
    {
        // All variables are initialized here
        public static string BotToken { get; private set; }
        public static string ApiToken { get; private set; }
        public static string ApiId { get; private set; }
        public static string Channel { get; private set; }
        public static string BotUsername { get; private set; }
        public static string Prefix { get; private set; }
        public static string AeroIP { get; private set; }
        public static string SelfDB { get; private set; }

        // This struct might show warnings about no initialized value
        // It is assigned by the JSON read operation in ReadCreds()
#pragma warning disable 0649
        private struct CredsJson
        {
            [JsonProperty("BotToken")]
            public string BotToken;

            [JsonProperty("ApiToken")]
            public string ApiToken;

            [JsonProperty("ApiId")]
            public string ApiId;

            [JsonProperty("Channel")]
            public string Channel;

            [JsonProperty("BotUsername")]
            public string BotUsername;

            [JsonProperty("Prefix")]
            public string Prefix;

            [JsonProperty("AeroIP")]
            public string AeroIP;

            [JsonProperty("SelfDB")]
            public string SelfDB;
        }
#pragma warning restore 0649
        public static async Task<bool> ReadCreds(string path)
        {
            // Read credentials as Token and DevID into a struct object from creds.json
            string info = "";
            using (FileStream fs = File.OpenRead(path))
            using (StreamReader sr = new StreamReader(fs))
                info = await sr.ReadToEndAsync();

            CredsJson creds = JsonConvert.DeserializeObject<CredsJson>(info);
            BotToken = creds.BotToken;
            ApiToken = creds.ApiToken;
            ApiId = creds.ApiId;
            Channel = creds.Channel;
            BotUsername = creds.BotUsername;
            Prefix = creds.Prefix;
            AeroIP = creds.AeroIP;
            SelfDB = creds.SelfDB;
            return true;
        }

        // Empty the tokens from RAM once we've authenticated
        public static void WipeToken()
        {
            BotToken = "";
            ApiToken = "";
        }

    }
}
