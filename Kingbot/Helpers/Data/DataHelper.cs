using System.Threading.Tasks;
using Aerospike.Client;
using Kingbot.Helpers.Security;

namespace Kingbot.Helpers.Data
{
    class DataHelper
    {
        // Globally defined variables across the class
        private static AsyncClient AeroClient;
        private static WritePolicy policy = new WritePolicy();
        private static string SelfDB = CredentialsHelper.SelfDB;

        // Connect to the aerospike server using the IP from CredentialsHelper
        public static void InitDB()
        {
            AeroClient = new AsyncClient(CredentialsHelper.AeroIP, 3000);
        }

        // Generic write function. Takes 2 bins (elements), and writes them
        public static async Task Write(string table, string index, string message)
        {
            Key key = new Key(SelfDB, table, index);
            Bin bin1 = new Bin("index", index);
            Bin bin2 = new Bin("message", message);

            AeroClient.Put(policy, key, bin1, bin2);
        }

        // Generic Read function
        public static async Task<string> Read(string table, string index)
        {
            Key key = new Key("kingbot", table, index);
            Record record = AeroClient.Get(policy, key, "message");
            if (record != null)
            {
                return record.GetValue("message").ToString();
            }

            return null;
        }

        // Generic delete function
        public static async Task Delete(string table, string index)
        {
            Key key = new Key("kingbot", table, index);

            AeroClient.Delete(policy, key);
        }

        // Ensures that the key exists in the data set (A table for SQL nerds)
        public static async Task<bool> Ensure(string table, string index)
        {
            return !(await Read(table, index) == null);
        }

        // Wrapper to close the connection
        public static void CloseDb()
        {
            AeroClient.Close();
        }
    }
}
