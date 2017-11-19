using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oxide.Core;
using Oxide.Ext.Discord.DiscordEvents;
using Oxide.Ext.Discord.DiscordObjects;
using Oxide.Ext.Discord.Exceptions;
using WebSocketSharp;

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
                    Property = new PayloadProperty()
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

            Client.CallHook("DiscordSocket_WebSocketOpened");
        }

        public void SocketClosed(object sender, CloseEventArgs e)
        {
            if (e.Code == 4004)
                throw new APIKeyException();

            if (!e.WasClean)
            {
                Interface.Oxide.LogWarning($"[Discord Ext] Discord connection closed uncleanly: code {e.Code}, Reason: {e.Reason}");
            }

            Client.CallHook("DiscordSocket_WebSocketClosed", null, e.Reason, e.Code, e.WasClean);
        }

        public void SocketErrored(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Interface.Oxide.LogWarning($"[Discord Ext] An error has occured: Response: {e.Message}");
            Client.CallHook("DiscordSocket_WebSocketErrored", null, e.Exception, e.Message);
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

            if (!messageObj.TryGetValue("s", out heartbeatToken))
            {
                if (!int.TryParse(heartbeatToken.ToSentence(), out lastHeartbeat))
                {
                    lastHeartbeat = 0;
                }
            }

            string opCode = messageObj["op"].ToString();
            string eventData = messageObj["d"].ToString();
            string eventName = messageObj["t"].ToString();

            switch (opCode)
            {
                // Dispatch (dispatches an event)
                case "0":
                    switch (eventName)
                    {
                        case "READY":
                            Ready ready = JsonConvert.DeserializeObject<Ready>(eventData);
                            Client.CallHook("Discord_Ready", null, ready);
                            break;

                        case "RESUMED":
                            Resumed resumed = JsonConvert.DeserializeObject<Resumed>(eventData);
                            Client.CallHook("Discord_Resumed", null, resumed);
                            break;

                        case "CHANNEL_CREATE":
                            Channel channelCreate = JsonConvert.DeserializeObject<Channel>(eventData);
                            Client.DiscordServer.channels.Add(channelCreate);
                            Client.CallHook("Discord_ChannelCreate", null, channelCreate);
                            break;

                        case "CHANNEL_UPDATE":
                            Channel channelUpdated = JsonConvert.DeserializeObject<Channel>(eventData);
                            Channel channelPrevious = Client.DiscordServer.channels.FirstOrDefault(x => x.id == channelUpdated.id);

                            if (channelPrevious != null)
                            {
                                Client.DiscordServer.channels.Remove(channelPrevious);
                            }

                            Client.DiscordServer.channels.Add(channelUpdated);

                            Client.CallHook("Discord_ChannelUpdate", null, channelUpdated, channelPrevious);
                            break;

                        case "CHANNEL_DELETE":
                            Channel channelDelete = JsonConvert.DeserializeObject<Channel>(eventData);

                            Client.DiscordServer.channels.Remove(channelDelete);

                            Client.CallHook("Discord_ChannelDelete", null, channelDelete);
                            break;

                        case "CHANNEL_PINS_UPDATE":
                            ChannelPinsUpdate channelPinsUpdate = JsonConvert.DeserializeObject<ChannelPinsUpdate>(eventData);
                            Client.CallHook("Discord_ChannelPinsUpdate", null, channelPinsUpdate);
                            break;

                        // this isn't set up right
                        // https://discordapp.com/developers/docs/topics/gateway#guild-create
                        case "GUILD_CREATE":
                            Guild guildCreate = JsonConvert.DeserializeObject<Guild>(eventData);
                            Client.DiscordServer = guildCreate;
                            Client.UpdatePluginReference();
                            Client.CallHook("DiscordSocket_Initialized");
                            Client.CallHook("Discord_GuildCreate", null, guildCreate);
                            break;

                        case "GUILD_UPDATE":
                            Guild guildUpdate = JsonConvert.DeserializeObject<Guild>(eventData);
                            Client.CallHook("Discord_GuildUpdate", null, guildUpdate);
                            break;

                        case "GUILD_DELETE":
                            Guild guildDelete = JsonConvert.DeserializeObject<Guild>(eventData);
                            Client.CallHook("Discord_GuildDelete", null, guildDelete);
                            break;

                        case "GUILD_BAN_ADD":
                            User bannedUser = JsonConvert.DeserializeObject<User>(eventData);
                            Client.CallHook("Discord_GuildBanAdd", null, bannedUser);
                            break;

                        case "GUILD_BAN_REMOVE":
                            User unbannedUser = JsonConvert.DeserializeObject<User>(eventData);
                            Client.CallHook("Discord_GuildBanRemove", null, unbannedUser);
                            break;

                        case "GUILD_EMOJIS_UPDATE":
                            GuildEmojisUpdate guildEmojisUpdate = JsonConvert.DeserializeObject<GuildEmojisUpdate>(eventData);
                            Client.CallHook("Discord_GuildEmojisUpdate", null, guildEmojisUpdate);
                            break;

                        case "GUILD_INTEGRATIONS_UPDATE":
                            GuildIntergrationsUpdate guildIntergrationsUpdate = JsonConvert.DeserializeObject<GuildIntergrationsUpdate>(eventData);
                            Client.CallHook("Discord_GuildIntergrationsUpdate", null, guildIntergrationsUpdate);
                            break;

                        case "GUILD_MEMBER_ADD":
                            GuildMemberAdd memberAdded = JsonConvert.DeserializeObject<GuildMemberAdd>(eventData);
                            GuildMember guildMember = memberAdded as GuildMember;

                            Client.DiscordServer.members.Add(guildMember);

                            Client.CallHook("Discord_MemberAdded", null, guildMember);
                            break;

                        case "GUILD_MEMBER_REMOVE":
                            GuildMemberRemove memberRemoved = JsonConvert.DeserializeObject<GuildMemberRemove>(eventData);

                            GuildMember member = Client.DiscordServer.members.FirstOrDefault(x => x.user.id == memberRemoved.user.id);
                            if (member != null)
                            {
                                Client.DiscordServer.members.Remove(member);
                            }

                            Client.CallHook("Discord_MemberRemoved", null, member);
                            break;

                        case "GUILD_MEMBER_UPDATE":
                            GuildMemberUpdate memberUpdated = JsonConvert.DeserializeObject<GuildMemberUpdate>(eventData);

                            GuildMember oldMember = Client.DiscordServer.members.FirstOrDefault(x => x.user.id == memberUpdated.user.id);
                            if (oldMember != null)
                            {
                                Client.DiscordServer.members.Remove(oldMember);
                            }

                            Client.CallHook("Discord_GuildMemberUpdate", null, memberUpdated, oldMember);
                            break;

                        case "GUILD_MEMBERS_CHUNK":
                            GuildMembersChunk guildMembersChunk = JsonConvert.DeserializeObject<GuildMembersChunk>(eventData);
                            Client.CallHook("Discord_GuildMembersChunk", null, guildMembersChunk);
                            break;

                        case "GUILD_ROLE_CREATE":
                            GuildRoleCreate guildRoleCreate = JsonConvert.DeserializeObject<GuildRoleCreate>(eventData);

                            Client.DiscordServer.roles.Add(guildRoleCreate.role);

                            Client.CallHook("Discord_GuildRoleCreate", null, guildRoleCreate.role);
                            break;

                        case "GUILD_ROLE_UPDATE":
                            GuildRoleUpdate guildRoleUpdate = JsonConvert.DeserializeObject<GuildRoleUpdate>(eventData);
                            Role newRole = guildRoleUpdate.role;

                            Role oldRole = Client.DiscordServer.roles.FirstOrDefault(x => x.id == newRole.id);
                            if (oldRole != null)
                            {
                                Client.DiscordServer.roles.Remove(oldRole);
                            }

                            Client.DiscordServer.roles.Add(newRole);

                            Client.CallHook("Discord_GuildRoleUpdate", null, newRole, oldRole);
                            break;

                        case "GUILD_ROLE_DELETE":
                            GuildRoleDelete guildRoleDelete = JsonConvert.DeserializeObject<GuildRoleDelete>(eventData);

                            Role deletedRole = Client.DiscordServer.roles.FirstOrDefault(x => x.id == guildRoleDelete.role_id);
                            if (deletedRole != null)
                            {
                                Client.DiscordServer.roles.Remove(deletedRole);
                            }

                            Client.CallHook("Discord_GuildRoleDelete", null, deletedRole);
                            break;

                        case "MESSAGE_CREATE":
                            Message messageCreate = JsonConvert.DeserializeObject<Message>(eventData);
                            Client.CallHook("Discord_MessageCreate", null, messageCreate);
                            break;

                        case "MESSAGE_UPDATE":
                            Message messageUpdate = JsonConvert.DeserializeObject<Message>(eventData);
                            Client.CallHook("Discord_MessageUpdate", null, messageUpdate);
                            break;

                        case "MESSAGE_DELETE":
                            MessageDelete messageDelete = JsonConvert.DeserializeObject<MessageDelete>(eventData);
                            Client.CallHook("Discord_MessageDelete", null, messageDelete);
                            break;

                        case "MESSAGE_DELETE_BULK":
                            MessageDeleteBulk messageDeleteBulk = JsonConvert.DeserializeObject<MessageDeleteBulk>(eventData);
                            Client.CallHook("Discord_MessageDeleteBulk", null, messageDeleteBulk);
                            break;

                        case "MESSAGE_REACTION_ADD":
                            MessageReactionUpdate messageReactionAdd = JsonConvert.DeserializeObject<MessageReactionUpdate>(eventData);
                            Client.CallHook("Discord_MessageReactionAdd", null, messageReactionAdd);
                            break;

                        case "MESSAGE_REACTION_REMOVE":
                            MessageReactionUpdate messageReactionRemove = JsonConvert.DeserializeObject<MessageReactionUpdate>(eventData);
                            Client.CallHook("Discord_MessageReactionRemove", null, messageReactionRemove);
                            break;

                        case "MESSAGE_REACTION_REMOVE_ALL":
                            MessageReactionRemoveAll messageReactionRemoveAll = JsonConvert.DeserializeObject<MessageReactionRemoveAll>(eventData);
                            Client.CallHook("Discord_MessageReactionRemoveAll", null, messageReactionRemoveAll);
                            break;

                        case "PRESENCE_UPDATE":
                            PresenceUpdate presenceUpdate = JsonConvert.DeserializeObject<PresenceUpdate>(eventData);

                            User updatedPresence = presenceUpdate?.user;

                            if (updatedPresence != null)
                            {
                                var updatedMember = Client.DiscordServer.members.FirstOrDefault(x => x.user.id == updatedPresence.id);

                                if (updatedMember != null)
                                {
                                    updatedMember.user = updatedPresence;
                                }
                            }

                            Client.CallHook("Discord_PresenceUpdate", null, updatedPresence);
                            break;

                        case "TYPING_START":
                            TypingStart typingStart = JsonConvert.DeserializeObject<TypingStart>(eventData);
                            Client.CallHook("Discord_TypingStart", null, typingStart);
                            break;

                        case "USER_UPDATE":
                            User userUpdate = JsonConvert.DeserializeObject<User>(eventData);

                            GuildMember memberUpdate = Client.DiscordServer.members.FirstOrDefault(x => x.user.id == userUpdate.id);

                            memberUpdate.user = userUpdate;

                            Client.CallHook("Discord_UserUpdate", null, userUpdate);
                            break;

                        case "VOICE_STATE_UPDATE":
                            VoiceState voiceStateUpdate = JsonConvert.DeserializeObject<VoiceState>(eventData);
                            Client.CallHook("Discord_VoiceStateUpdate", null, voiceStateUpdate);
                            break;

                        case "VOICE_SERVER_UPDATE":
                            VoiceServerUpdate voiceServerUpdate = JsonConvert.DeserializeObject<VoiceServerUpdate>(eventData);
                            Client.CallHook("Discord_VoiceServerUpdate", null, voiceServerUpdate);
                            break;

                        case "WEBHOOKS_UPDATE":
                            WebhooksUpdate webhooksUpdate = JsonConvert.DeserializeObject<WebhooksUpdate>(eventData);
                            Client.CallHook("Discord_WebhooksUpdate", null, webhooksUpdate);
                            break;

                        default:
                            Client.CallHook("Discord_UnhandledEvent", null, messageObj);
                            Interface.Oxide.LogWarning($"[Discord Ext] [Debug] Unhandled event: {eventName}, data: {eventData}");
                            break;
                    }
                    break;

                // Heartbeat (used for ping checking)
                // Note: I think we should be manually sending a heartbeat
                // when this is received
                // https://discordapp.com/developers/docs/topics/gateway#gateway-heartbeat
                case "1":
                    Interface.Oxide.LogInfo($"[DiscordExt] Manully sent heartbeat (received opcode 1)");
                    Client.SendHeartbeat();
                    break;

                // Reconnect (used to tell clients to reconnect to the gateway)
                // we should immediately reconnect here
                case "7":
                    Interface.Oxide.LogInfo($"[DiscordExt] Reconnect has been called (opcode 7)! Reconnecting...");
                    Client.Socket.ConnectAsync();
                    break;

                // Invalid Session (used to notify client they have an invalid 
                // session ID)
                case "9":
                    Interface.Oxide.LogInfo($"[DiscordExt] Invalid Session ID opcode recieved!");
                    break;

                // Hello (sent immediately after connecting, contains heartbeat
                // and server debug information)
                case "10":
                    JToken heartbeatInterval = messageObj["d"]["heartbeat_interval"];
                    float heartbeatTime = (float)heartbeatInterval;
                    Client.CreateHeartbeat(heartbeatTime, lastHeartbeat);
                    break;

                // Heartbeat ACK (sent immediately following a client heartbeat
                // that was received)
                case "11":
                    break;

                default:
                    Interface.Oxide.LogInfo($"[DiscordExt] Unhandled OP code: code {opCode}, message: {e.Data}");
                    break;
            }
        }
    }
}
