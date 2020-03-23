using Kingbot.Helpers.Security;
using System.Collections.Generic;

namespace Kingbot.Helpers.API
{
    class ChannelProvider
    {
        private static List<string> channels= new List<string>
        {
            CredentialsHelper.Channel
        };

        public static List<string> GetChannels()
        {
            return channels;
        }
    }
}
