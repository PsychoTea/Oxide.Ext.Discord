namespace Oxide.Ext.Discord.Gateway
{
    using Newtonsoft.Json;

    public class SPayload
    {
        [JsonProperty("op")]
        public OpCodes OP;

        [JsonProperty("d")]
        public object Payload;
    }
}
