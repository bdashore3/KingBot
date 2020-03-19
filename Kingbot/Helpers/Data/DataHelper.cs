using Aerospike.Client;
using Kingbot.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kingbot.Helpers.Data
{
    class DataHelper
    {
        private static AsyncClient AeroClient;
        private static WritePolicy policy = new WritePolicy();
        private static string SelfDB = CredentialsHelper.SelfDB;
        public static void InitDb()
        {
            AeroClient = new AsyncClient(CredentialsHelper.AeroIP, 3000);
        }

        public static void Write(string table, string index, string message)
        {
            Key key = new Key(SelfDB, table, index);
            Bin bin1 = new Bin("index", index);
            Bin bin2 = new Bin("message", message);

            AeroClient.Put(policy, key, bin1, bin2);
        }

        public static string Read(string table, string index)
        {
            Key key = new Key("kingbot", table, index);
            Record record = AeroClient.Get(policy, key, "message");
            if (record != null)
            {
                return record.GetValue("message").ToString();
            }

            return null;
        }

        public static void Delete(string table, string index)
        {
            Key key = new Key("kingbot", table, index);

            AeroClient.Delete(policy, key);
        }

        public static bool Ensure(string table, string index)
        {
            return !(Read(table, index) == null);
        }

        public static void CloseDb()
        {
            AeroClient.Close();
        }
    }
}
