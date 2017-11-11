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
            if (!(messageObj.TryGetValue("s", out heartbeatToken) && 
                  int.TryParse(heartbeatToken.ToString(), out lastHeartbeat)))
                lastHeartbeat = 0;

            switch (messageObj.GetValue("op").ToString())
            {
                case "10":
                    JObject info = JObject.Parse(e.Data);
                    float time = (float)info["d"]["heartbeat_interval"];
                    Client.CreateHeartbeat(time, lastHeartbeat);
                    break;
                    
                case "0":
                    switch (messageObj["t"].ToString())
                    {
                        case "READY":
                            Ready ready = JsonConvert.DeserializeObject<Ready>(messageObj["d"].ToString());
                            Client.CallHook("Discord_Ready", null, ready);
                            break;

                        case "RESUMED":
                            Resumed resumed = JsonConvert.DeserializeObject<Resumed>(messageObj["d"].ToString());
                            Client.CallHook("Discord_Resumed", null, resumed);
                            break;

                        case "CHANNEL_CREATE":
                            Channel channelCreate = JsonConvert.DeserializeObject<Channel>(messageObj["d"].ToString());
                            Client.DiscordServer.channels.Add(channelCreate);
                            Client.CallHook("Discord_ChannelCreate", null, channelCreate);
                            break;

                        case "CHANNEL_UPDATE":
                            Channel channelUpdated = JsonConvert.DeserializeObject<Channel>(messageObj["d"].ToString());
                            Channel channelOld = Client.DiscordServer.channels.ToList().Find(x => x.id.Equals(channelUpdated.id));
                            Client.DiscordServer.channels.Remove(channelOld);
                            Client.DiscordServer.channels.Add(channelUpdated);
                            Client.CallHook("Discord_ChannelUpdate", null, channelUpdated, channelOld);
                            break;

                        case "CHANNEL_DELETE":
                            Channel channelDelete = JsonConvert.DeserializeObject<Channel>(messageObj["d"].ToString());
                            Client.DiscordServer.channels.Remove(channelDelete);
                            Client.CallHook("Discord_ChannelDelete", null, channelDelete);
                            break;

                        case "CHANNEL_PINS_UPDATE":
                            ChannelPinsUpdate channelPinsUpdate = JsonConvert.DeserializeObject<ChannelPinsUpdate>(messageObj["d"].ToString());
                            Client.CallHook("Discord_ChannelPinsUpdate", null, channelPinsUpdate);
                            break;

                        // this isn't set up right
                        // https://discordapp.com/developers/docs/topics/gateway#guild-create
                        case "GUILD_CREATE":
                            Guild guildCreate = JsonConvert.DeserializeObject<Guild>(messageObj["d"].ToString());
                            Client.DiscordServer = guildCreate;
                            Client.UpdatePluginReference();
                            Client.CallHook("DiscordSocket_Initialized");
                            Client.CallHook("Discord_GuildCreate", null, guildCreate);
                            break;

                        case "GUILD_UPDATE":
                            Guild guildUpdate = JsonConvert.DeserializeObject<Guild>(messageObj["d"].ToString());
                            Client.CallHook("Discord_GuildUpdate", null, guildUpdate);
                            break;

                        case "GUILD_DELETE":
                            Guild guildDelete = JsonConvert.DeserializeObject<Guild>(messageObj["d"].ToString());
                            Client.CallHook("Discord_GuildDelete", null, guildDelete);
                            break;

                        case "GUILD_BAN_ADD":
                            User bannedUser = JsonConvert.DeserializeObject<User>(messageObj["d"].ToString());
                            Client.CallHook("Discord_GuildBanAdd", null, bannedUser);
                            break;

                        case "GUILD_BAN_REMOVE":
                            User unbannedUser = JsonConvert.DeserializeObject<User>(messageObj["d"].ToString());
                            Client.CallHook("Discord_GuildBanRemove", null, unbannedUser);
                            break;

                        case "GUILD_EMOJIS_UPDATE":
                            GuildEmojisUpdate guildEmojisUpdate = JsonConvert.DeserializeObject<GuildEmojisUpdate>(messageObj["d"].ToString());
                            Client.CallHook("Discord_GuildEmojisUpdate", null, guildEmojisUpdate);
                            break;

                        case "GUILD_INTEGRATIONS_UPDATE":
                            GuildIntergrationsUpdate guildIntergrationsUpdate = JsonConvert.DeserializeObject<GuildIntergrationsUpdate>(messageObj["d"].ToString());
                            Client.CallHook("Discord_GuildIntergrationsUpdate", null, guildIntergrationsUpdate);
                            break;

                        case "GUILD_MEMBER_ADD":
                            GuildMemberAdd memberAdded = JsonConvert.DeserializeObject<GuildMemberAdd>(messageObj["d"].ToString());
                            User userAdded = memberAdded.user;
                            userAdded.guild_id = memberAdded.guild_id;
                            userAdded.joined_at = memberAdded.joined_at;
                            userAdded.mute = memberAdded.mute;
                            userAdded.deaf = memberAdded.deaf;
                            userAdded.nick = memberAdded.nick;
                            userAdded.roles = memberAdded.roles;
                            Member member = new Member();
                            member.user = userAdded;
                            member.roles = userAdded.roles;
                            member.deaf = userAdded.deaf;
                            member.joined_at = userAdded.joined_at;
                            member.mute = userAdded.mute;
                            member.nick = userAdded.nick;
                            Client.DiscordServer.members.Add(member);
                            Client.CallHook("Discord_MemberAdded", null, userAdded);
                            break;

                        case "GUILD_MEMBER_REMOVE":
                            GuildMemberRemove memberRemoved = JsonConvert.DeserializeObject<GuildMemberRemove>(messageObj["d"].ToString());
                            User userRemoved = memberRemoved.user;
                            userRemoved.guild_id = userRemoved.guild_id;
                            userRemoved.joined_at = userRemoved.joined_at;
                            userRemoved.mute = userRemoved.mute;
                            userRemoved.deaf = userRemoved.deaf;
                            userRemoved.nick = userRemoved.nick;
                            userRemoved.roles = userRemoved.roles;
                            Client.DiscordServer.members.Remove(Client.DiscordServer.members.Find(x => x.user.id.Equals(userRemoved.id)));
                            Client.CallHook("Discord_MemberRemoved", null, userRemoved);
                            break;

                        case "GUILD_MEMBER_UPDATE":
                            GuildMemberUpdate memberUpdated = JsonConvert.DeserializeObject<GuildMemberUpdate>(messageObj["d"].ToString());
                            User userUpdated = memberUpdated.user;
                            User oldUser = Client.DiscordServer.members.Find(x => x.user.id.Equals(userUpdated.id)).user;
                            userUpdated.guild_id = memberUpdated.guild_id;
                            userUpdated.nick = memberUpdated.nick;
                            userUpdated.roles = memberUpdated.roles;
                            Client.CallHook("Discord_GuildMemberUpdate", null, userUpdated, oldUser);
                            break;

                        case "GUILD_MEMBERS_CHUNK":
                            GuildMembersChunk guildMembersChunk = JsonConvert.DeserializeObject<GuildMembersChunk>(messageObj["d"].ToString());
                            Client.CallHook("Discord_GuildMembersChunk", null, guildMembersChunk);
                            break;

                        case "GUILD_ROLE_CREATE":
                            GuildRoleCreate guildRoleCreate = JsonConvert.DeserializeObject<GuildRoleCreate>(messageObj["d"].ToString());
                            Role newRole = new Role();
                            newRole = guildRoleCreate.role;
                            newRole.guild_id = guildRoleCreate.guild_id;
                            Client.DiscordServer.roles.Add(newRole);
                            Client.CallHook("Discord_GuildRoleCreate", null, newRole);
                            break;

                        case "GUILD_ROLE_UPDATE":
                            GuildRoleUpdate guildRoleUpdate = JsonConvert.DeserializeObject<GuildRoleUpdate>(messageObj["d"].ToString());
                            Role oldRole = Client.DiscordServer.roles.ToList().Find(x => x.id.Equals(guildRoleUpdate.role.id));
                            Role currentRole = guildRoleUpdate.role;
                            currentRole.guild_id = guildRoleUpdate.guild_id;
                            Client.DiscordServer.roles.Remove(oldRole);
                            Client.DiscordServer.roles.Add(currentRole);
                            Client.CallHook("Discord_GuildRoleUpdate", null, currentRole, oldRole);
                            break;

                        case "GUILD_ROLE_DELETE":
                            GuildRoleDelete guildRoleDelete = JsonConvert.DeserializeObject<GuildRoleDelete>(messageObj["d"].ToString());
                            Role deletedRole = Client.DiscordServer.roles.ToList().Find(x => x.id.Equals(guildRoleDelete.role_id));
                            Client.DiscordServer.roles.Remove(deletedRole);
                            Client.CallHook("Discord_GuildRoleDelete", null, deletedRole);
                            break;

                        case "MESSAGE_CREATE":
                            Message messageCreate = JsonConvert.DeserializeObject<Message>(messageObj["d"].ToString());
                            Client.CallHook("Discord_MessageCreate", null, messageCreate);
                            break;

                        case "MESSAGE_UPDATE":
                            Message messageUpdate = JsonConvert.DeserializeObject<Message>(messageObj["d"].ToString());
                            Client.CallHook("Discord_MessageUpdate", null, messageUpdate);
                            break;

                        case "MESSAGE_DELETE":
                            MessageDelete messageDelete = JsonConvert.DeserializeObject<MessageDelete>(messageObj["d"].ToString());
                            Client.CallHook("Discord_MessageDelete", null, messageDelete);
                            break;

                        case "MESSAGE_DELETE_BULK":
                            MessageDeleteBulk messageDeleteBulk = JsonConvert.DeserializeObject<MessageDeleteBulk>(messageObj["d"].ToString());
                            Client.CallHook("Discord_MessageDeleteBulk", null, messageDeleteBulk);
                            break;

                        case "MESSAGE_REACTION_ADD":
                            MessageReactionUpdate messageReactionAdd = JsonConvert.DeserializeObject<MessageReactionUpdate>(messageObj["d"].ToString());
                            Client.CallHook("Discord_MessageReactionAdd", null, messageReactionAdd);
                            break;

                        case "MESSAGE_REACTION_REMOVE":
                            MessageReactionUpdate messageReactionRemove = JsonConvert.DeserializeObject<MessageReactionUpdate>(messageObj["d"].ToString());
                            Client.CallHook("Discord_MessageReactionRemove", null, messageReactionRemove);
                            break;

                        case "MESSAGE_REACTION_REMOVE_ALL":
                            MessageReactionRemoveAll messageReactionRemoveAll = JsonConvert.DeserializeObject<MessageReactionRemoveAll>(messageObj["d"].ToString());
                            Client.CallHook("Discord_MessageReactionRemoveAll", null, messageReactionRemoveAll);
                            break;

                        case "PRESENCE_UPDATE":
                            PresenceUpdate presenceUpdate = JsonConvert.DeserializeObject<PresenceUpdate>(messageObj["d"].ToString());
                            User updatedPres = presenceUpdate.user;
                            User oldPresUser = Client.DiscordServer.members.ToList().Find(x => x.user.id.Equals(updatedPres.id)).user;
                            Client.DiscordServer.members.Remove(Client.DiscordServer.members.Find(x => x.user.id.Equals(updatedPres.id)));
                            Member member1 = new Member();
                            member1.user = updatedPres;
                            member1.roles = updatedPres.roles;
                            member1.deaf = updatedPres.deaf;
                            member1.joined_at = updatedPres.joined_at;
                            member1.mute = updatedPres.mute;
                            member1.nick = updatedPres.nick;
                            Client.DiscordServer.members.Add(member1);
                            Client.CallHook("Discord_PresenceUpdate", null, updatedPres, oldPresUser);
                            break;

                        case "TYPING_START":
                            TypingStart typingStart = JsonConvert.DeserializeObject<TypingStart>(messageObj["d"].ToString());
                            Client.CallHook("Discord_TypingStart", null, typingStart);
                            break;

                        case "USER_UPDATE":
                            User userUpdate = JsonConvert.DeserializeObject<User>(messageObj["d"].ToString());
                            User oldestUser = Client.DiscordServer.members.ToList().Find(x => x.user.id.Equals(userUpdate.id)).user;
                            Client.DiscordServer.members.Remove(Client.DiscordServer.members.Find(x => x.user.id.Equals(userUpdate.id)));
                            Member member2 = new Member();
                            member2.user = userUpdate;
                            member2.roles = userUpdate.roles;
                            member2.deaf = userUpdate.deaf;
                            member2.joined_at = userUpdate.joined_at;
                            member2.mute = userUpdate.mute;
                            member2.nick = userUpdate.nick;
                            Client.DiscordServer.members.Add(member2);
                            Client.CallHook("Discord_UserUpdate", null, userUpdate, oldestUser);
                            break;

                        case "VOICE_STATE_UPDATE":
                            VoiceState voiceStateUpdate = JsonConvert.DeserializeObject<VoiceState>(messageObj["d"].ToString());
                            Client.CallHook("Discord_VoiceStateUpdate", null, voiceStateUpdate);
                            break;

                        case "VOICE_SERVER_UPDATE":
                            VoiceServerUpdate voiceServerUpdate = JsonConvert.DeserializeObject<VoiceServerUpdate>(messageObj["d"].ToString());
                            Client.CallHook("Discord_VoiceServerUpdate", null, voiceServerUpdate);
                            break;

                        case "WEBHOOKS_UPDATE":
                            WebhooksUpdate webhooksUpdate = JsonConvert.DeserializeObject<WebhooksUpdate>(messageObj["d"].ToString());
                            Client.CallHook("Discord_WebhooksUpdate", null, webhooksUpdate);
                            break;

                        default:
                            Client.CallHook("Discord_UnhandledMessage", null, messageObj);
                            Interface.Oxide.LogWarning($"[Discord Ext] [Debug] Unhandled message: {messageObj["t"]}, data: {messageObj["d"]}");
                            break;
                    }
                    break;
            }
        }
    }
}
