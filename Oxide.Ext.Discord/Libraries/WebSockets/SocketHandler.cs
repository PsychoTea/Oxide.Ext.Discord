using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oxide.Core;
using Oxide.Ext.Discord.Libraries.DiscordObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp;

namespace Oxide.Ext.Discord.Libraries.WebSockets
{
    public class SocketHandler
    {
        private WebSocketClient client;
        public SocketHandler(WebSocketClient client)
        {
            this.client = client;
        }
        public void SocketOpened(object sender, EventArgs e)
        {
            Interface.Oxide.LogWarning($"[Discord Ext] Connection started, authorizing to discord servers.");
            var payload = new DiscordObjects.Handshake()
            {
                Op = 2,
                payload = new DiscordObjects.Payload()
                {
                    Token = Discord.settings.ApiToken,
                    Property = new DiscordObjects.Payload_Property()
                    {
                        os = Environment.OSVersion.ToString(),
                        browser = "orfbotpp",
                        device = "orfbotpp",
                        referrer = "",
                        referring_domain = ""
                    },
                    Compress = false,
                    Large_Threshold = 250,
                }
            };
            var sp = JsonConvert.SerializeObject(payload);
            client.socket.Send(sp);
            Interface.Oxide.CallHook("DiscordSocket_WebsocketOpened");
        }
        public void SocketClosed(object sender, CloseEventArgs e)
        {
            client.Disconnect();
            Interface.Oxide.LogWarning($"[Discord Ext] Discord connection closed: \nCode: {e.Code}\nResponse:{e.Reason}\nClean:{e.WasClean}");
            Interface.Oxide.CallHook("DiscordSocket_WebsocketClosed", e.Reason, e.Code, e.WasClean);
        }
        public void SocketErrored(object sender, ErrorEventArgs e)
        {
            if (client.socket.IsAlive) client.Disconnect();
            Interface.Oxide.LogWarning($"[Discord Ext] An error has occured: Response: {e.Message}");
            Interface.Oxide.CallHook("DiscordSocket_WebsocketError", e.Exception);
        }
        Core.Libraries.Timer.TimerInstance timer;
        public void SocketMessage(object sender, MessageEventArgs e)
        {
            JObject obj = JObject.Parse(e.Data);
            JToken token;
            int tempS;
            if (obj.TryGetValue("s", out token) && int.TryParse(token.ToString(), out tempS)) client.lastS = tempS;
            switch (obj.GetValue("op").ToString())
            {
                case "10":
                    JObject info = JObject.Parse(e.Data);
                    double time = (double)info["d"]["heartbeat_interval"] / 1000.0;
                    Interface.Oxide.LogWarning($"[Discord Ext] DEBUG: SENDING PACKETS EVERY {time}ms");
                    timer = client.timer.Repeat(Convert.ToSingle(time), -1, () =>
                    {
                        if (!client.socket.IsAlive)
                        {
                            timer.Destroy();
                            return;
                        }
                        var packet = new Packet()
                        {
                            op = 1,
                            d = client.lastS
                        };
                        string spacket = JsonConvert.SerializeObject(packet);
                        client.socket.Send(spacket);
                        Interface.Oxide.CallHook("DiscordSocket_HeartbeatSent");
                    });
                    break;
                case "7":
                    Interface.Oxide.LogWarning($"[Discord Ext] Reconnecting to discord servers.");
                    Interface.Oxide.CallHook("DiscordSocket_ReconnectingStarted");
                    client.Disconnect();
                    client.Connect();
                    break;
                case "0":
                    switch (obj["t"].ToString())
                    {
                        case "MESSAGE_CREATE":
                            Message message = JsonConvert.DeserializeObject<Message>(obj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_TextMessage", message);
                            break;
                        case "GUILD_CREATE":
                            client.server = JsonConvert.DeserializeObject<Server>(obj["d"].ToString());
                            break;
                        case "GUILD_MEMBER_ADD":
                            GuildAdd add = JsonConvert.DeserializeObject<GuildAdd>(obj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_MemberAdded", add);
                            break;
                        case "GUILD_MEMBER_REMOVE":
                            GuildRemove remove = JsonConvert.DeserializeObject<GuildRemove>(obj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_MemberRemoved", remove);
                            break;
                        default:
                            Interface.Oxide.CallHook("Discord_RawMessage", obj);
                            break;
                    }
                    break;
            }
        }
    }
}
