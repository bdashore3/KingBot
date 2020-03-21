using Kingbot.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Services;

namespace Kingbot.Helpers.API
{
    class ChannelProvider
    {
        private static LiveStreamMonitorService Monitor;
        private static TwitchAPI api = TwitchBot.api;

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
