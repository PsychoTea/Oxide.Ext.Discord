namespace Oxide.Ext.Discord.WebSockets
{
    using System;
    using System.Linq;
    using Newtonsoft.Json;
    using Oxide.Core;
    using Oxide.Ext.Discord.DiscordEvents;
    using Oxide.Ext.Discord.DiscordObjects;
    using Oxide.Ext.Discord.Exceptions;
    using Oxide.Ext.Discord.Gateway;
    using WebSocketSharp;

    public class SocketListner
    {
        private DiscordClient client;

        private Socket webSocket;

        public SocketListner(DiscordClient client, Socket socket)
        {
            this.client = client;
            this.webSocket = socket;
        }

        public void SocketOpened(object sender, EventArgs e)
        {
            if (client.Settings.Debugging)
            {
                Interface.Oxide.LogDebug($"Discord WebSocket opened.");
            }

            client.CallHook("DiscordSocket_WebSocketOpened");
        }

        public void SocketClosed(object sender, CloseEventArgs e)
        {
            if (e.Code == 4004)
            {
                throw new APIKeyException();
            }

            if (client.Settings.Debugging)
            {
                Interface.Oxide.LogDebug($"Discord WebSocket closed. Code: {e.Code}, reason: {e.Reason}");
            }

            if (!e.WasClean)
            {
                Interface.Oxide.LogWarning($"[Discord Ext] Discord connection closed uncleanly: code {e.Code}, Reason: {e.Reason}");

                Interface.Oxide.LogWarning($"[Discord Ext] Attempting to reconnect to Discord...");

                webSocket.Connect(client.WSSURL);
            }
            else
            {
                client.Disconnect();
                Discord.CloseClient(client);
            }
            

            client.CallHook("DiscordSocket_WebSocketClosed", null, e.Reason, e.Code, e.WasClean);
        }

        public void SocketErrored(object sender, ErrorEventArgs e)
        {
            Interface.Oxide.LogWarning($"[Discord Ext] An error has occured: Response: {e.Message}");

            client.CallHook("DiscordSocket_WebSocketErrored", null, e.Exception, e.Message);
        }

        public void SocketMessage(object sender, MessageEventArgs e)
        {
            RPayload payload = JsonConvert.DeserializeObject<RPayload>(e.Data);

            if (payload.Sequence.HasValue)
            {
                client.Sequence = payload.Sequence.Value;
            }

            if (client.Settings.Debugging)
            {
                Interface.Oxide.LogDebug($"Recieved socket message, OpCode: {payload.OpCode}");
            }

            switch (payload.OpCode)
            {
                // Dispatch (dispatches an event)
                case OpCodes.Dispatch:
                {
                    if (client.Settings.Debugging)
                    {
                        Interface.Oxide.LogDebug($"Recieved OpCode 0, event: {payload.EventName}");
                    }
                    
                    switch (payload.EventName)
                    {
                        case "READY":
                        {
                            client.UpdatePluginReference();
                            
                            Ready ready = payload.EventData.ToObject<Ready>();

                            if (ready.Guilds.Count > 1)
                            {
                                Interface.Oxide.LogWarning($"[Oxide.Ext.Discord] Your bot was found in more than one Guild. Multiple guilds are not supported by this extension.");
                            }

                            if (ready.Guilds.Count == 0 &&
                                client.Settings.Debugging)
                            {
                                Interface.Oxide.LogDebug($"Ready event but no Guilds sent.");
                            }

                            client.DiscordServer = ready.Guilds.FirstOrDefault();
                            client.SessionID = ready.SessionID;
                            
                            client.CallHook("DiscordSocket_Initialized");
                            client.CallHook("Discord_Ready", null, ready);
                            break;
                        }

                        case "RESUMED":
                        {
                            Resumed resumed = payload.EventData.ToObject<Resumed>();
                            client.CallHook("Discord_Resumed", null, resumed);
                            break;
                        }

                        case "CHANNEL_CREATE":
                        {
                            Channel channelCreate = payload.EventData.ToObject<Channel>();
                            client.DiscordServer.channels.Add(channelCreate);
                            client.CallHook("Discord_ChannelCreate", null, channelCreate);
                            break;
                        }

                        case "CHANNEL_UPDATE":
                        {
                            Channel channelUpdated = payload.EventData.ToObject<Channel>();
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
                            Channel channelDelete = payload.EventData.ToObject<Channel>();

                            client.DiscordServer.channels.Remove(channelDelete);

                            client.CallHook("Discord_ChannelDelete", null, channelDelete);
                            break;
                        }

                        case "CHANNEL_PINS_UPDATE":
                        {
                            ChannelPinsUpdate channelPinsUpdate = payload.EventData.ToObject<ChannelPinsUpdate>();
                            client.CallHook("Discord_ChannelPinsUpdate", null, channelPinsUpdate);
                            break;
                        }

                        // this isn't set up right
                        // https://discordapp.com/developers/docs/topics/gateway#guild-create
                        case "GUILD_CREATE":
                        {
                            Guild guildCreate = payload.EventData.ToObject<Guild>();
                            client.DiscordServer = guildCreate;
                            client.CallHook("Discord_GuildCreate", null, guildCreate);
                            break;
                        }

                        case "GUILD_UPDATE":
                        {
                            Guild guildUpdate = payload.EventData.ToObject<Guild>();
                            client.CallHook("Discord_GuildUpdate", null, guildUpdate);
                            break;
                        }

                        case "GUILD_DELETE":
                        {
                            Guild guildDelete = payload.EventData.ToObject<Guild>();
                            client.CallHook("Discord_GuildDelete", null, guildDelete);
                            break;
                        }

                        case "GUILD_BAN_ADD":
                        {
                            User bannedUser = payload.EventData.ToObject<BanObject>().user;
                            client.CallHook("Discord_GuildBanAdd", null, bannedUser);
                            break;
                        }

                        case "GUILD_BAN_REMOVE":
                        {
                            User unbannedUser = payload.EventData.ToObject<BanObject>().user;
                            client.CallHook("Discord_GuildBanRemove", null, unbannedUser);
                            break;
                        }

                        case "GUILD_EMOJIS_UPDATE":
                        {
                            GuildEmojisUpdate guildEmojisUpdate = payload.EventData.ToObject<GuildEmojisUpdate>();
                            client.CallHook("Discord_GuildEmojisUpdate", null, guildEmojisUpdate);
                            break;
                        }

                        case "GUILD_INTEGRATIONS_UPDATE":
                        {
                            GuildIntergrationsUpdate guildIntergrationsUpdate = payload.EventData.ToObject<GuildIntergrationsUpdate>();
                            client.CallHook("Discord_GuildIntergrationsUpdate", null, guildIntergrationsUpdate);
                            break;
                        }

                        case "GUILD_MEMBER_ADD":
                        {
                            GuildMemberAdd memberAdded = payload.EventData.ToObject<GuildMemberAdd>();
                            GuildMember guildMember = memberAdded as GuildMember;

                            client.DiscordServer.members.Add(guildMember);

                            client.CallHook("Discord_MemberAdded", null, guildMember);
                            break;
                        }

                        case "GUILD_MEMBER_REMOVE":
                        {
                            GuildMemberRemove memberRemoved = payload.EventData.ToObject<GuildMemberRemove>();

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
                            GuildMemberUpdate memberUpdated = payload.EventData.ToObject<GuildMemberUpdate>();

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
                            GuildMembersChunk guildMembersChunk = payload.EventData.ToObject<GuildMembersChunk>();
                            client.CallHook("Discord_GuildMembersChunk", null, guildMembersChunk);
                            break;
                        }

                        case "GUILD_ROLE_CREATE":
                        {
                            GuildRoleCreate guildRoleCreate = payload.EventData.ToObject<GuildRoleCreate>();

                            client.DiscordServer.roles.Add(guildRoleCreate.role);

                            client.CallHook("Discord_GuildRoleCreate", null, guildRoleCreate.role);
                            break;
                        }

                        case "GUILD_ROLE_UPDATE":
                        {
                            GuildRoleUpdate guildRoleUpdate = payload.EventData.ToObject<GuildRoleUpdate>();
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
                            GuildRoleDelete guildRoleDelete = payload.EventData.ToObject<GuildRoleDelete>();

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
                            Message messageCreate = payload.EventData.ToObject<Message>();
                            client.CallHook("Discord_MessageCreate", null, messageCreate);
                            break;
                        }

                        case "MESSAGE_UPDATE":
                        {
                            Message messageUpdate = payload.EventData.ToObject<Message>();
                            client.CallHook("Discord_MessageUpdate", null, messageUpdate);
                            break;
                        }

                        case "MESSAGE_DELETE":
                        {
                            MessageDelete messageDelete = payload.EventData.ToObject<MessageDelete>();
                            client.CallHook("Discord_MessageDelete", null, messageDelete);
                            break;
                        }

                        case "MESSAGE_DELETE_BULK":
                        {
                            MessageDeleteBulk messageDeleteBulk = payload.EventData.ToObject<MessageDeleteBulk>();
                            client.CallHook("Discord_MessageDeleteBulk", null, messageDeleteBulk);
                            break;
                        }

                        case "MESSAGE_REACTION_ADD":
                        {
                            MessageReactionUpdate messageReactionAdd = payload.EventData.ToObject<MessageReactionUpdate>();
                            client.CallHook("Discord_MessageReactionAdd", null, messageReactionAdd);
                            break;
                        }

                        case "MESSAGE_REACTION_REMOVE":
                        {
                            MessageReactionUpdate messageReactionRemove = payload.EventData.ToObject<MessageReactionUpdate>();
                            client.CallHook("Discord_MessageReactionRemove", null, messageReactionRemove);
                            break;
                        }

                        case "MESSAGE_REACTION_REMOVE_ALL":
                        {
                            MessageReactionRemoveAll messageReactionRemoveAll = payload.EventData.ToObject<MessageReactionRemoveAll>();
                            client.CallHook("Discord_MessageReactionRemoveAll", null, messageReactionRemoveAll);
                            break;
                        }

                        case "PRESENCE_UPDATE":
                        {
                            PresenceUpdate presenceUpdate = payload.EventData.ToObject<PresenceUpdate>();

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
                            TypingStart typingStart = payload.EventData.ToObject<TypingStart>();
                            client.CallHook("Discord_TypingStart", null, typingStart);
                            break;
                        }

                        case "USER_UPDATE":
                        {
                            User userUpdate = payload.EventData.ToObject<User>();

                            GuildMember memberUpdate = client.DiscordServer.members.FirstOrDefault(x => x.user.id == userUpdate.id);

                            memberUpdate.user = userUpdate;

                            client.CallHook("Discord_UserUpdate", null, userUpdate);
                            break;
                        }

                        case "VOICE_STATE_UPDATE":
                        {
                            VoiceState voiceStateUpdate = payload.EventData.ToObject<VoiceState>();
                            client.CallHook("Discord_VoiceStateUpdate", null, voiceStateUpdate);
                            break;
                        }

                        case "VOICE_SERVER_UPDATE":
                        {
                            VoiceServerUpdate voiceServerUpdate = payload.EventData.ToObject<VoiceServerUpdate>();
                            client.CallHook("Discord_VoiceServerUpdate", null, voiceServerUpdate);
                            break;
                        }

                        case "WEBHOOKS_UPDATE":
                        {
                            WebhooksUpdate webhooksUpdate = payload.EventData.ToObject<WebhooksUpdate>();
                            client.CallHook("Discord_WebhooksUpdate", null, webhooksUpdate);
                            break;
                        }

                        default:
                        {
                            client.CallHook("Discord_UnhandledEvent", null, payload);
                            Interface.Oxide.LogWarning($"[Discord Ext] [Debug] Unhandled event: {payload.EventName}");
                            break;
                        }
                    }

                    break;
                }

                // Heartbeat
                // https://discordapp.com/developers/docs/topics/gateway#gateway-heartbeat
                case OpCodes.Heartbeat:
                {
                    Interface.Oxide.LogInfo($"[DiscordExt] Manully sent heartbeat (received opcode 1)");
                    client.SendHeartbeat();
                    break;
                }

                // Reconnect (used to tell clients to reconnect to the gateway)
                // we should immediately reconnect here
                case OpCodes.Reconnect:
                {
                    Interface.Oxide.LogInfo($"[DiscordExt] Reconnect has been called (opcode 7)! Reconnecting...");

                    webSocket.Connect(client.WSSURL);
                    break;
                }

                // Invalid Session (used to notify client they have an invalid session ID)
                case OpCodes.InvalidSession:
                {
                    Interface.Oxide.LogInfo($"[DiscordExt] Invalid Session ID opcode recieved!");
                    break;
                }

                // Hello (sent immediately after connecting, contains heartbeat and server debug information)
                case OpCodes.Hello:
                {
                    Hello hello = payload.EventData.ToObject<Hello>();
                    client.CreateHeartbeat(hello.HeartbeatInterval);

                    // Client should now perform identification
                    client.Identify();
                    break;
                }

                // Heartbeat ACK (sent immediately following a client heartbeat
                // that was received)
                // This should be changed: https://discordapp.com/developers/docs/topics/gateway#heartbeating
                // (See 'zombied or failed connections')
                case OpCodes.HeartbeatACK: break;

                default:
                {
                    Interface.Oxide.LogInfo($"[DiscordExt] Unhandled OP code: code {payload.OpCode}");
                    break;
                }
            }
        }
    }
}
