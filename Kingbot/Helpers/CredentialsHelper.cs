using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Kingbot.Security
{
    public static class CredentialsHelper
    {
        public static InfoData info { get; set; } = new InfoData();
        public static void ReadInfo()
        {
            string information = new StreamReader(File.OpenRead("info.json"), new UTF8Encoding(false)).ReadToEnd();
            info = JsonConvert.DeserializeObject<InfoData>(information);
        }
    }

    public class InfoData
    {
        [JsonProperty("BotToken")]
        public string BotToken;

        [JsonProperty("ApiSecret")]
        public string ApiToken;

        [JsonProperty("ApiId")]
        public string ApiId;

        [JsonProperty("Channel")]
        public string Channel;

        [JsonProperty("BotUsername")]
        public string BotUsername;

        [JsonProperty("Prefix")]
        public string Prefix;
    }
}
