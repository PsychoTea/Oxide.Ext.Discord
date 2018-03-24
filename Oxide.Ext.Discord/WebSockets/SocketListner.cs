namespace Oxide.Ext.Discord.WebSockets
{
    using System;
    using System.Timers;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Oxide.Core;
    using Oxide.Ext.Discord.DiscordEvents;
    using Oxide.Ext.Discord.DiscordObjects;
    using Oxide.Ext.Discord.Exceptions;
    using WebSocketSharp;

    public class SocketListner
    {
        private DiscordClient client;

        private Socket webSocket;

        private Timer reconnectTimer = new Timer();

        public SocketListner(DiscordClient client, Socket socket)
        {
            this.client = client;
            this.webSocket = socket;
        }

        public void SocketOpened(object sender, EventArgs e)
        {
            if (webSocket.resume == null)
            {
                var payload = new Handshake()
                {
                    op = 2,
                    Payload = new Payload()
                    {
                        token = client.Settings.ApiToken,
                        properties = new PayloadProperty()
                        {
                            os = Environment.OSVersion.ToString(),
                            browser = "orfbotpp",
                            device = "orfbotpp",
                            referrer = string.Empty,
                            referring_domain = string.Empty
                        },
                        compress = false,
                        large_threshold = 250
                    }
                };
                var sp = JsonConvert.SerializeObject(payload);
                webSocket.Send(sp);

            } else
            {
                var payload = new Packet()
                {
                    op = 6,
                    d = webSocket.resume
                };
                var sp = JsonConvert.SerializeObject(payload);
                webSocket.Send(sp);
            }
            reconnectTimer?.Stop();
            reconnectTimer?.Dispose();
            client.CallHook("DiscordSocket_WebSocketOpened");
        }

        public void SocketClosed(object sender, CloseEventArgs e)
        {
            if (e.Code == 4004)
            {
                throw new APIKeyException();
            }
            
            if (!Interface.Oxide.IsShuttingDown)
            {
                Interface.Oxide.LogWarning($"[Discord Ext] Connection closed.. CODE: {e.Code}");

                webSocket.reconnectAttempts++;
                int interval = webSocket.reconnectAttempts < 50 ? 3 : webSocket.reconnectAttempts < 100 ?  10 : 60;
                reconnectTimer.Interval = (interval*1000);
                reconnectTimer.Elapsed += ConnectTimer;
                reconnectTimer.Start();
                Interface.Oxide.LogWarning($"[Discord Ext] Attempting to reconnect in {interval} seconds...");
            }

            client.CallHook("DiscordSocket_WebSocketClosed", null, e.Reason, e.Code, e.WasClean);
        }

        private void ConnectTimer(Object source, System.Timers.ElapsedEventArgs e) 
        {
            webSocket.Connect(client.WSSURL);
        }

        public void SocketErrored(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Interface.Oxide.LogWarning($"[Discord Ext] An error has occured: Response: {e.Message}");
            client.CallHook("DiscordSocket_WebSocketErrored", null, e.Exception, e.Message);
        }

        public void SocketMessage(object sender, MessageEventArgs e)
        {
            JsonReader reader = new JsonTextReader(new StringReader(e.Data))
            {
                DateParseHandling = DateParseHandling.None
            };
            JObject messageObj = JObject.Load(reader);

            if (messageObj.TryGetValue("s", out JToken heartbeatToken))
            {
                if (!int.TryParse(heartbeatToken.ToString(), out webSocket.lastHeartbeat))
                {
                    webSocket.lastHeartbeat = 0;
                }
                if (webSocket.resume != null) 
                {
                    webSocket.resume.seq = webSocket.lastHeartbeat;
                }
            }

            string eventCode = messageObj["op"].ToString();
            string eventData = messageObj["d"].ToString();
            string eventName = messageObj["t"].ToString();

            switch (eventCode)
            {
                // Dispatch (dispatches an event)
                case "0":
                    {
                        switch (eventName)
                        {
                            case "READY":
                                {
                                    Ready ready = JsonConvert.DeserializeObject<Ready>(eventData);
                                    webSocket.resume = new Resume()
                                    {
                                        token = client.Settings.ApiToken,
                                        session_id = ready.session_id,
                                        seq = webSocket.lastHeartbeat
                                    };
                                    client.CallHook("Discord_Ready", null, ready);
                                    break;
                                }

                            case "RESUMED":
                                {
                                    Resumed resumed = JsonConvert.DeserializeObject<Resumed>(eventData);
                                    Interface.Oxide.LogInfo("[Discord Ext] Succesfully resumed session.");
                                    client.CallHook("Discord_Resumed", null, resumed);
                                    break;
                                }

                            case "CHANNEL_CREATE":
                                {
                                    Channel channelCreate = JsonConvert.DeserializeObject<Channel>(eventData);
                                    client.DiscordServer.channels.Add(channelCreate);
                                    client.CallHook("Discord_ChannelCreate", null, channelCreate);
                                    break;
                                }

                            case "CHANNEL_UPDATE":
                                {
                                    Channel channelUpdated = JsonConvert.DeserializeObject<Channel>(eventData);
                                    Channel channelPrevious = client.DiscordServer.channels.FirstOrDefault(x => x.id == channelUpdated.id);

                                    if (channelPrevious != null)
                                    {
                                        client.DiscordServer.channels.Remove(channelPrevious);
                                    }

                                    client.DiscordServer.channels.Add(channelUpdated);

                                    client.CallHook("Discord_ChannelUpdate", null, channelUpdated, channelPrevious);
                                    break;
                                }

                            case "CHANNEL_DELETE":
                                {
                                    Channel channelDelete = JsonConvert.DeserializeObject<Channel>(eventData);

                                    client.DiscordServer.channels.Remove(channelDelete);

                                    client.CallHook("Discord_ChannelDelete", null, channelDelete);
                                    break;
                                }

                            case "CHANNEL_PINS_UPDATE":
                                {
                                    ChannelPinsUpdate channelPinsUpdate = JsonConvert.DeserializeObject<ChannelPinsUpdate>(eventData);
                                    client.CallHook("Discord_ChannelPinsUpdate", null, channelPinsUpdate);
                                    break;
                                }

                            // this isn't set up right
                            // https://discordapp.com/developers/docs/topics/gateway#guild-create
                            case "GUILD_CREATE":
                                {
                                    Guild guildCreate = JsonConvert.DeserializeObject<Guild>(eventData);
                                    client.DiscordServer = guildCreate;
                                    client.UpdatePluginReference();
                                    client.CallHook("DiscordSocket_Initialized");
                                    client.CallHook("Discord_GuildCreate", null, guildCreate);
                                    break;
                                }

                            case "GUILD_UPDATE":
                                {
                                    Guild guildUpdate = JsonConvert.DeserializeObject<Guild>(eventData);
                                    client.CallHook("Discord_GuildUpdate", null, guildUpdate);
                                    break;
                                }

                            case "GUILD_DELETE":
                                {
                                    Guild guildDelete = JsonConvert.DeserializeObject<Guild>(eventData);
                                    client.CallHook("Discord_GuildDelete", null, guildDelete);
                                    break;
                                }

                            case "GUILD_BAN_ADD":
                                {
                                    User bannedUser = JsonConvert.DeserializeObject<User>(eventData);
                                    client.CallHook("Discord_GuildBanAdd", null, bannedUser);
                                    break;
                                }

                            case "GUILD_BAN_REMOVE":
                                {
                                    User unbannedUser = JsonConvert.DeserializeObject<User>(eventData);
                                    client.CallHook("Discord_GuildBanRemove", null, unbannedUser);
                                    break;
                                }

                            case "GUILD_EMOJIS_UPDATE":
                                {
                                    GuildEmojisUpdate guildEmojisUpdate = JsonConvert.DeserializeObject<GuildEmojisUpdate>(eventData);
                                    client.CallHook("Discord_GuildEmojisUpdate", null, guildEmojisUpdate);
                                    break;
                                }

                            case "GUILD_INTEGRATIONS_UPDATE":
                                {
                                    GuildIntergrationsUpdate guildIntergrationsUpdate = JsonConvert.DeserializeObject<GuildIntergrationsUpdate>(eventData);
                                    client.CallHook("Discord_GuildIntergrationsUpdate", null, guildIntergrationsUpdate);
                                    break;
                                }

                            case "GUILD_MEMBER_ADD":
                                {
                                    GuildMemberAdd memberAdded = JsonConvert.DeserializeObject<GuildMemberAdd>(eventData);
                                    GuildMember guildMember = memberAdded as GuildMember;

                                    client.DiscordServer.members.Add(guildMember);

                                    client.CallHook("Discord_MemberAdded", null, guildMember);
                                    break;
                                }

                            case "GUILD_MEMBER_REMOVE":
                                {
                                    GuildMemberRemove memberRemoved = JsonConvert.DeserializeObject<GuildMemberRemove>(eventData);

                                    GuildMember member = client.DiscordServer.members.FirstOrDefault(x => x.user.id == memberRemoved.user.id);
                                    if (member != null)
                                    {
                                        client.DiscordServer.members.Remove(member);
                                    }

                                    client.CallHook("Discord_MemberRemoved", null, member);
                                    break;
                                }

                            case "GUILD_MEMBER_UPDATE":
                                {
                                    GuildMemberUpdate memberUpdated = JsonConvert.DeserializeObject<GuildMemberUpdate>(eventData);

                                    GuildMember oldMember = client.DiscordServer.members.FirstOrDefault(x => x.user.id == memberUpdated.user.id);
                                    if (oldMember != null)
                                    {
                                        client.DiscordServer.members.Remove(oldMember);
                                    }

                                    client.CallHook("Discord_GuildMemberUpdate", null, memberUpdated, oldMember);
                                    break;
                                }

                            case "GUILD_MEMBERS_CHUNK":
                                {
                                    GuildMembersChunk guildMembersChunk = JsonConvert.DeserializeObject<GuildMembersChunk>(eventData);
                                    client.CallHook("Discord_GuildMembersChunk", null, guildMembersChunk);
                                    break;
                                }

                            case "GUILD_ROLE_CREATE":
                                {
                                    GuildRoleCreate guildRoleCreate = JsonConvert.DeserializeObject<GuildRoleCreate>(eventData);

                                    client.DiscordServer.roles.Add(guildRoleCreate.role);

                                    client.CallHook("Discord_GuildRoleCreate", null, guildRoleCreate.role);
                                    break;
                                }

                            case "GUILD_ROLE_UPDATE":
                                {
                                    GuildRoleUpdate guildRoleUpdate = JsonConvert.DeserializeObject<GuildRoleUpdate>(eventData);
                                    Role newRole = guildRoleUpdate.role;

                                    Role oldRole = client.DiscordServer.roles.FirstOrDefault(x => x.id == newRole.id);
                                    if (oldRole != null)
                                    {
                                        client.DiscordServer.roles.Remove(oldRole);
                                    }

                                    client.DiscordServer.roles.Add(newRole);

                                    client.CallHook("Discord_GuildRoleUpdate", null, newRole, oldRole);
                                    break;
                                }

                            case "GUILD_ROLE_DELETE":
                                {
                                    GuildRoleDelete guildRoleDelete = JsonConvert.DeserializeObject<GuildRoleDelete>(eventData);

                                    Role deletedRole = client.DiscordServer.roles.FirstOrDefault(x => x.id == guildRoleDelete.role_id);
                                    if (deletedRole != null)
                                    {
                                        client.DiscordServer.roles.Remove(deletedRole);
                                    }

                                    client.CallHook("Discord_GuildRoleDelete", null, deletedRole);
                                    break;
                                }

                            case "MESSAGE_CREATE":
                                {
                                    Message messageCreate = JsonConvert.DeserializeObject<Message>(eventData);
                                    client.CallHook("Discord_MessageCreate", null, messageCreate);
                                    break;
                                }

                            case "MESSAGE_UPDATE":
                                {
                                    Message messageUpdate = JsonConvert.DeserializeObject<Message>(eventData);
                                    client.CallHook("Discord_MessageUpdate", null, messageUpdate);
                                    break;
                                }

                            case "MESSAGE_DELETE":
                                {
                                    MessageDelete messageDelete = JsonConvert.DeserializeObject<MessageDelete>(eventData);
                                    client.CallHook("Discord_MessageDelete", null, messageDelete);
                                    break;
                                }

                            case "MESSAGE_DELETE_BULK":
                                {
                                    MessageDeleteBulk messageDeleteBulk = JsonConvert.DeserializeObject<MessageDeleteBulk>(eventData);
                                    client.CallHook("Discord_MessageDeleteBulk", null, messageDeleteBulk);
                                    break;
                                }

                            case "MESSAGE_REACTION_ADD":
                                {
                                    MessageReactionUpdate messageReactionAdd = JsonConvert.DeserializeObject<MessageReactionUpdate>(eventData);
                                    client.CallHook("Discord_MessageReactionAdd", null, messageReactionAdd);
                                    break;
                                }

                            case "MESSAGE_REACTION_REMOVE":
                                {
                                    MessageReactionUpdate messageReactionRemove = JsonConvert.DeserializeObject<MessageReactionUpdate>(eventData);
                                    client.CallHook("Discord_MessageReactionRemove", null, messageReactionRemove);
                                    break;
                                }

                            case "MESSAGE_REACTION_REMOVE_ALL":
                                {
                                    MessageReactionRemoveAll messageReactionRemoveAll = JsonConvert.DeserializeObject<MessageReactionRemoveAll>(eventData);
                                    client.CallHook("Discord_MessageReactionRemoveAll", null, messageReactionRemoveAll);
                                    break;
                                }

                            case "PRESENCE_UPDATE":
                                {
                                    PresenceUpdate presenceUpdate = JsonConvert.DeserializeObject<PresenceUpdate>(eventData);

                                    User updatedPresence = presenceUpdate?.user;

                                    if (updatedPresence != null)
                                    {
                                        var updatedMember = client.DiscordServer.members.FirstOrDefault(x => x.user.id == updatedPresence.id);

                                        if (updatedMember != null)
                                        {
                                            updatedMember.user = updatedPresence;
                                        }
                                    }

                                    client.CallHook("Discord_PresenceUpdate", null, updatedPresence);
                                    break;
                                }

                            case "TYPING_START":
                                {
                                    TypingStart typingStart = JsonConvert.DeserializeObject<TypingStart>(eventData);
                                    client.CallHook("Discord_TypingStart", null, typingStart);
                                    break;
                                }

                            case "USER_UPDATE":
                                {
                                    User userUpdate = JsonConvert.DeserializeObject<User>(eventData);

                                    GuildMember memberUpdate = client.DiscordServer.members.FirstOrDefault(x => x.user.id == userUpdate.id);

                                    memberUpdate.user = userUpdate;

                                    client.CallHook("Discord_UserUpdate", null, userUpdate);
                                    break;
                                }

                            case "VOICE_STATE_UPDATE":
                                {
                                    VoiceState voiceStateUpdate = JsonConvert.DeserializeObject<VoiceState>(eventData);
                                    client.CallHook("Discord_VoiceStateUpdate", null, voiceStateUpdate);
                                    break;
                                }

                            case "VOICE_SERVER_UPDATE":
                                {
                                    VoiceServerUpdate voiceServerUpdate = JsonConvert.DeserializeObject<VoiceServerUpdate>(eventData);
                                    client.CallHook("Discord_VoiceServerUpdate", null, voiceServerUpdate);
                                    break;
                                }

                            case "WEBHOOKS_UPDATE":
                                {
                                    WebhooksUpdate webhooksUpdate = JsonConvert.DeserializeObject<WebhooksUpdate>(eventData);
                                    client.CallHook("Discord_WebhooksUpdate", null, webhooksUpdate);
                                    break;
                                }

                            default:
                                {
                                    client.CallHook("Discord_UnhandledEvent", null, messageObj);
                                    Interface.Oxide.LogWarning($"[Discord Ext] [Debug] Unhandled event: {eventName}, data: {eventData}");
                                    break;
                                }
                        }

                        break;
                    }

                // Heartbeat (used for ping checking)
                // Note: I think we should be manually sending a heartbeat
                // when this is received (not 100% sure)
                // https://discordapp.com/developers/docs/topics/gateway#gateway-heartbeat
                case "1":
                    {
                        Interface.Oxide.LogInfo($"[DiscordExt] Manully sent heartbeat (received opcode 1)");
                        client.SendHeartbeat();
                        break;
                    }
                // Reconnect (used to tell clients to reconnect to the gateway)
                // we should immediately reconnect here
                case "7":
                    {
                        Interface.Oxide.LogInfo($"[DiscordExt] Reconnect has been called (opcode 7)! Reconnecting...");

                        webSocket.Connect(client.WSSURL);
                        break;
                    }

                // Invalid Session (used to notify client they have an invalid session ID)
                case "9":
                    {
                        Interface.Oxide.LogInfo($"[DiscordExt] Invalid Session ID opcode recieved!");
                        webSocket.resume = null;
                        webSocket.Connect(client.WSSURL);
                        break;
                    }

                // Hello (sent immediately after connecting, contains heartbeat
                // and server debug information)
                case "10":
                    {
                        JToken heartbeatInterval = messageObj["d"]["heartbeat_interval"];
                        float heartbeatTime = (float)heartbeatInterval;
                        client.CreateHeartbeat(heartbeatTime, webSocket.lastHeartbeat);
                        break;
                    }

                // Heartbeat ACK (sent immediately following a client heartbeat
                // that was received)
                case "11": break;

                default:
                    {
                        Interface.Oxide.LogInfo($"[DiscordExt] Unhandled OP code: code {eventCode}, message: {e.Data}");
                        break;
                    }
            }
        }
    }
}
