using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oxide.Core;
using Oxide.Ext.Discord.DiscordObjects;
using Oxide.Ext.Discord.Exceptions;
using WebSocketSharp;
using Oxide.Ext.Discord.DiscordEvents;

namespace Oxide.Ext.Discord.WebSockets
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
                    
                case "0":
                    Interface.Oxide.LogInfo($"got message type 0: {messageObj["t"]}");
                    switch (messageObj["t"].ToString())
                    {
                        case "READY":
                            Ready ready = JsonConvert.DeserializeObject<Ready>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_Ready", ready);
                            break;

                        case "RESUMED":
                            Resumed resumed = JsonConvert.DeserializeObject<Resumed>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_Resumed", resumed);
                            break;

                        case "CHANNEL_CREATE":
                            Channel channelCreate = JsonConvert.DeserializeObject<Channel>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_ChannelCreate", channelCreate);
                            break;

                        case "CHANNEL_UPDATE":
                            Channel channelUpdate = JsonConvert.DeserializeObject<Channel>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_ChannelUpdate", channelUpdate);
                            break;

                        case "CHANNEL_DELETE":
                            Channel channelDelete = JsonConvert.DeserializeObject<Channel>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_ChannelDelete", channelDelete);
                            break;

                        case "CHANNEL_PINS_UPDATE":
                            ChannelPinsUpdate channelPinsUpdate = JsonConvert.DeserializeObject<ChannelPinsUpdate>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_ChannelPinsUpdate", channelPinsUpdate);
                            break;

                        // this isn't set up right
                        // https://discordapp.com/developers/docs/topics/gateway#guild-create
                        case "GUILD_CREATE":
                            Guild guildCreate = JsonConvert.DeserializeObject<Guild>(messageObj["d"].ToString());
                            Client.DiscordServer = guildCreate;
                            Interface.Oxide.CallHook("Discord_GuildCreate", guildCreate);
                            break;

                        case "GUILD_UPDATE":
                            Guild guildUpdate = JsonConvert.DeserializeObject<Guild>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_GuildUpdate", guildUpdate);
                            break;

                        case "GUILD_DELETE":
                            Guild guildDelete = JsonConvert.DeserializeObject<Guild>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_GuildDelete", guildDelete);
                            break;

                        case "GUILD_BAN_ADD":
                            GuildBan guildBanAdd = JsonConvert.DeserializeObject<GuildBan>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_GuildBanAdd", guildBanAdd);
                            break;

                        case "GUILD_BAN_REMOVE":
                            GuildBan guildBanRemove = JsonConvert.DeserializeObject<GuildBan>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_GuildBanRemove", guildBanRemove);
                            break;

                        case "GUILD_EMOJIS_UPDATE":
                            GuildEmojisUpdate guildEmojisUpdate = JsonConvert.DeserializeObject<GuildEmojisUpdate>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_GuildEmojisUpdate", guildEmojisUpdate);
                            break;

                        case "GUILD_INTEGRATIONS_UPDATE":
                            GuildIntergrationsUpdate guildIntergrationsUpdate = JsonConvert.DeserializeObject<GuildIntergrationsUpdate>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_GuildIntergrationsUpdate", guildIntergrationsUpdate);
                            break;

                        case "GUILD_MEMBER_ADD":
                            GuildMemberAdd guildMemberAdd = JsonConvert.DeserializeObject<GuildMemberAdd>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_MemberAdded", guildMemberAdd);
                            break;

                        case "GUILD_MEMBER_REMOVE":
                            GuildMemberRemove guildMemberRemove = JsonConvert.DeserializeObject<GuildMemberRemove>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_MemberRemoved", guildMemberRemove);
                            break;

                        case "GUILD_MEMBER_UPDATE":
                            GuildMemberUpdate guildMemberUpdate = JsonConvert.DeserializeObject<GuildMemberUpdate>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_GuildMemberUpdate", guildMemberUpdate);
                            break;

                        case "GUILD_MEMBERS_CHUNK":
                            GuildMembersChunk guildMembersChunk = JsonConvert.DeserializeObject<GuildMembersChunk>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_GuildMembersChunk", guildMembersChunk);
                            break;

                        case "GUILD_ROLE_CREATE":
                            GuildRoleCreate guildRoleCreate = JsonConvert.DeserializeObject<GuildRoleCreate>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_GuildRoleCreate", guildRoleCreate);
                            break;

                        case "GUILD_ROLE_UPDATE":
                            GuildRoleUpdate guildRoleUpdate = JsonConvert.DeserializeObject<GuildRoleUpdate>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_GuildRoleUpdate", guildRoleUpdate);
                            break;

                        case "GUILD_ROLE_DELETE":
                            GuildRoleDelete guildRoleDelete = JsonConvert.DeserializeObject<GuildRoleDelete>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_GuildRoleDelete", guildRoleDelete);
                            break;

                        case "MESSAGE_CREATE":
                            Message messageCreate = JsonConvert.DeserializeObject<Message>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_MessageCreate", messageCreate);
                            break;

                        case "MESSAGE_UPDATE":
                            Message messageUpdate = JsonConvert.DeserializeObject<Message>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_MessageUpdate", messageUpdate);
                            break;

                        case "MESSAGE_DELETE":
                            MessageDelete messageDelete = JsonConvert.DeserializeObject<MessageDelete>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_MessageDelete", messageDelete);
                            break;

                        case "MESSAGE_DELETE_BULK":
                            MessageDeleteBulk messageDeleteBulk = JsonConvert.DeserializeObject<MessageDeleteBulk>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_MessageDeleteBulk", messageDeleteBulk);
                            break;

                        case "MESSAGE_REACTION_ADD":
                            MessageReactionUpdate messageReactionAdd = JsonConvert.DeserializeObject<MessageReactionUpdate>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_MessageReactionAdd", messageReactionAdd);
                            break;

                        case "MESSAGE_REACTION_REMOVE":
                            MessageReactionUpdate messageReactionRemove = JsonConvert.DeserializeObject<MessageReactionUpdate>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_MessageReactionRemove", messageReactionRemove);
                            break;

                        case "MESSAGE_REACTION_REMOVE_ALL":
                            MessageReactionRemoveAll messageReactionRemoveAll = JsonConvert.DeserializeObject<MessageReactionRemoveAll>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_MessageReactionRemoveAll", messageReactionRemoveAll);
                            break;

                        case "PRESENCE_UPDATE":
                            PresenceUpdate presenceUpdate = JsonConvert.DeserializeObject<PresenceUpdate>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_PresenceUpdate", presenceUpdate);
                            break;

                        case "TYPING_START":
                            TypingStart typingStart = JsonConvert.DeserializeObject<TypingStart>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_TypingStart", typingStart);
                            break;

                        case "USER_UPDATE":
                            User userUpdate = JsonConvert.DeserializeObject<User>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_UserUpdate", userUpdate);
                            break;

                        case "VOICE_STATE_UPDATE":
                            VoiceState voiceStateUpdate = JsonConvert.DeserializeObject<VoiceState>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_VoiceStateUpdate", voiceStateUpdate);
                            break;

                        case "VOICE_SERVER_UPDATE":
                            VoiceServerUpdate voiceServerUpdate = JsonConvert.DeserializeObject<VoiceServerUpdate>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_VoiceServerUpdate", voiceServerUpdate);
                            break;

                        case "WEBHOOKS_UPDATE":
                            WebhooksUpdate webhooksUpdate = JsonConvert.DeserializeObject<WebhooksUpdate>(messageObj["d"].ToString());
                            Interface.Oxide.CallHook("Discord_WebhooksUpdate", webhooksUpdate);
                            break;

                        default:
                            Interface.Oxide.CallHook("Discord_UnhandledMessage", messageObj);
                            Interface.Oxide.LogWarning($"[Discord Ext] [Debug] Unhandled message: {messageObj["t"]}, data: {messageObj["d"]}");
                            break;
                    }
                    break;
            }
        }
    }
}
