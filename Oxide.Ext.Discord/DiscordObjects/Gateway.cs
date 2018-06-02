namespace Oxide.Ext.Discord.DiscordObjects
{
    using System;
    using Newtonsoft.Json;

    class Gateway
    {
        [JsonProperty("url")]
        public string URL { get; private set; }

        public static void GetGateway(DiscordClient client, Action<Gateway> callback)
        {
            client.REST.DoRequest("/gateway", REST.RequestMethod.GET, null, callback);
        }
    }
}
