using System;
using System.Collections.Generic;
using Oxide.Ext.Discord.WebSockets;

namespace Oxide.Ext.Discord.DiscordObjects
{
    public class VoiceRegion
    {
        public string id;
        public string name;
        public string sample_hostname;
        public int sample_port;
        public bool vip;
        public bool optimal;
        public bool deprecated;
        public bool custom;

        public static void ListVoiceRegions(DiscordClient client, Action<List<VoiceRegion>> callback = null)
        {
            client.REST.DoRequest<List<VoiceRegion>>($"/voice/regions", "GET", null, (returnValue) =>
            {
                callback?.Invoke(returnValue as List<VoiceRegion>);
            });
        }
    }
}
