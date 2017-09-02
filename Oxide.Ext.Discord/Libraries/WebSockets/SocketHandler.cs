using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oxide.Core;
using Oxide.Ext.Discord.Libraries.DiscordObjects;
using Oxide.Ext.Discord.Libraries.Exceptions;
using WebSocketSharp;

namespace Oxide.Ext.Discord.Libraries.WebSockets
{
    public class SocketHandler
    {
        private DiscordClient Client;

        public SocketHandler(DiscordClient client) { Client = client; }

        public void SocketOpened(object sender, EventArgs e)
        {
            var payload = new Handshake()
            {
                Op = 2,
                payload = new Payload()
                {
                    Token = Client.Settings.ApiToken,
                    Property = new Payload_Property()
                    {
                        os = Environment.OSVersion.ToString(),
                        browser = "orfbotpp",
                        device = "orfbotpp",
                        referrer = "",
                        referring_domain = ""
                    },
                    Compress = false,
                    Large_Threshold = 250
                }
            };

            var sp = JsonConvert.SerializeObject(payload);
            Client.SendData(sp);
            Interface.Oxide.LogInfo($"[Discord Ext] Connected to Discord.");
            Interface.Oxide.CallHook("DiscordSocket_WebSocketOpened");
        }
        public void SocketClosed(object sender, CloseEventArgs e)
        {
            if (e.Code == 4004) throw new APIKeyException();
            Interface.Oxide.LogInfo($"[Discord Ext] Discord connection closed (code: {e.Code}) {(!e.WasClean ? $"\nReason: {e.Reason}" : "")}");
            Interface.Oxide.CallHook("DiscordSocket_WebSocketClosed", e.Reason, e.Code, e.WasClean);
        }

        public void SocketErrored(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Interface.Oxide.LogWarning($"[Discord Ext] An error has occured: Response: {e.Message}");
            Interface.Oxide.CallHook("DiscordSocket_WebSocketErrored", e.Exception, e.Message);
        }
        
        public void SocketMessage(object sender, MessageEventArgs e)
        {
            JsonReader reader = new JsonTextReader(new StringReader(e.Data))
            {
                DateParseHandling = DateParseHandling.None
            };
            JObject messageObj = JObject.Load(reader);

            JToken heartbeatToken;
            int lastHeartbeat = 0;
            if (!(messageObj.TryGetValue("s", out heartbeatToken) && int.TryParse(heartbeatToken.ToString(), out lastHeartbeat))) lastHeartbeat = 0;

            switch (messageObj.GetValue("op").ToString())
            {
                case "10":
                    JObject info = JObject.Parse(e.Data);
                    float time = (float)info["d"]["heartbeat_interval"] / 1000f;
                    Client.CreateHeartbeat(time, lastHeartbeat);
                    break;

                case "7":
                    break;

                case "0":
                    switch (messageObj["t"].ToString())
                    {
                        case "MESSAGE_CREATE":
                            Message message = JsonConvert.DeserializeObject<Message>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_TextMessage", message);
                            break;

                        case "GUILD_CREATE":
                            Client.DiscordServer = JsonConvert.DeserializeObject<Server>(messageObj["d"].ToString());
                            break;

                        case "GUILD_MEMBER_ADD":
                            User add = JsonConvert.DeserializeObject<User>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_MemberAdded", add);
                            break;

                        case "GUILD_MEMBER_REMOVE":
                            User remove = JsonConvert.DeserializeObject<User>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_MemberRemoved", remove);
                            break;

                        default:
                            Interface.Oxide.CallHook("Discord_RawMessage", messageObj);
                            break;
                    }
                    break;
            }
        }
    }
}
