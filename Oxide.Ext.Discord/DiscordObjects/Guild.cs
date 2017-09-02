using System;
using System.Collections.Generic;
using Oxide.Ext.Discord.WebSockets;
using Oxide.Ext.Discord.RESTObjects;
using Newtonsoft.Json.Linq;

namespace Oxide.Ext.Discord.DiscordObjects
{
    public class Guild
    {
        public string id { get; set; }
        public string name { get; set; }
        public string icon { get; set; }
        public string splash { get; set; }
        public string owner_id { get; set; }
        public string region { get; set; }
        public string afk_channel_id { get; set; }
        public int? afk_timeout { get; set; }
        public bool? embed_enabled { get; set; }
        public string embed_channel_id { get; set; }
        public int? verification_level { get; set; }
        public int? default_message_notifications { get; set; }
        public int? explicit_content_filter { get; set; }
        public List<Role> roles { get; set; }
        public List<Emoji> emojis { get; set; }
        public List<string> features { get; set; }
        public int? mfa_level { get; set; }
        public string application_id { get; set; }
        public bool? widget_enabled { get; set; }
        public string widget_channel_id { get; set; }
        public string joined_at { get; set; }
        public bool? large { get; set; }
        public bool? unavailable { get; set; }
        public int? member_count { get; set; }
        public List<VoiceState> voice_states { get; set; }
        public List<Member> members { get; set; }
        public List<Channel> channels { get; set; }
        public List<Presence> presences { get; set; }

        public static void CreateGuild(DiscordClient client, string name, string region, string icon, int? verificationLevel, int? defaultMessageNotifications, List<Role> roles, List<Channel> channels, Action<Guild> callback = null)
        {
            var jsonObj = new Dictionary<string, object>()
            {
                { "name", name },
                { "region", region },
                { "icon", icon },
                { "verification_level", verificationLevel },
                { "default_message_notifications", defaultMessageNotifications },
                { "roles", roles },
                { "channels", channels }
            };
            client.REST.DoRequest<Guild>($"/guilds", "POST", jsonObj, (returnValue) =>
            {
                callback?.Invoke(returnValue as Guild);
            });
        }

        public static void GetGuild(DiscordClient client, string guildID, Action<Guild> callback = null)
        {
            client.REST.DoRequest<Guild>($"/guilds/{guildID}", "GET", null, (returnValue) =>
            {
                callback?.Invoke(returnValue as Guild);
            });
        }

        public void ModifyGuild(DiscordClient client, Action<Guild> callback = null)
        {
            client.REST.DoRequest<Guild>($"/guilds/{id}", "PATCH", this, (returnValue) =>
            {
                callback?.Invoke(returnValue as Guild);
            });
        }

        public void DeleteGuild(DiscordClient client, Action callback = null)
        {
            client.REST.DoRequest($"/guilds/{id}", "DELETE", null, () =>
            {
                callback?.Invoke();
            });
        }

        public void GetGuildChannels(DiscordClient client, Action<List<Channel>> callback = null)
        {
            client.REST.DoRequest<List<Channel>>($"/guilds/{id}/channels", "GET", null, (returnValue) =>
            {
                callback?.Invoke(returnValue as List<Channel>);
            });
        }

        public void CreateGuildChannel(DiscordClient client, Channel channel, Action<Channel> callback = null) => CreateGuildChannel(client, channel.name, channel.type, channel.bitrate, channel.user_limit, channel.permission_overwrites, callback);
        public void CreateGuildChannel(DiscordClient client, string name, string type, int? bitrate, int? userLimit, List<Overwrite> permissionOverwrites, Action<Channel> callback = null)
        {
            var jsonObj = new Dictionary<string, object>()
            {
                { "name", name },
                { "type", type },
                { "bitrate", bitrate },
                { "user_limit", userLimit },
                { "permission_overwrites", permissionOverwrites }
            };
            client.REST.DoRequest<Channel>($"/guilds/{id}/channels", "POST", jsonObj, (returnValue) =>
            {
                callback?.Invoke(returnValue as Channel);
            });
        }

        public void ModifyGuildChannelPositions(DiscordClient client, List<ObjectPosition> positions, Action callback = null)
        {
            client.REST.DoRequest($"/guilds/{id}/channels", "PATCH", positions, () =>
            {
                callback?.Invoke();
            });
        }

        public void GetGuildMember(DiscordClient client, string userID, Action<GuildMember> callback = null)
        {
            client.REST.DoRequest<GuildMember>($"/guilds/{id}/members/{userID}", "GET", null, (returnValue) =>
            {
                callback?.Invoke(returnValue as GuildMember);
            });
        }

        public void ListGuildMembers(DiscordClient client, Action<List<GuildMember>> callback = null)
        {
            client.REST.DoRequest<List<GuildMember>>($"/guilds/{id}/members", "GET", null, (returnValue) =>
            {
                callback?.Invoke(returnValue as List<GuildMember>);
            });
        }

        public void AddGuildMember(DiscordClient client, GuildMember member, string accessToken, List<Role> roles, Action<GuildMember> callback = null) => AddGuildMember(client, member.user.id, accessToken, member.nick, roles, member.mute, member.deaf, callback);
        public void AddGuildMember(DiscordClient client, string userID, string accessToken, string nick, List<Role> roles, bool mute, bool deaf, Action<GuildMember> callback = null)
        {
            var jsonObj = new Dictionary<string, object>()
            {
                { "access_token", accessToken },
                { "nick", nick },
                { "roles", roles },
                { "mute", mute },
                { "deaf", deaf }
            };
            client.REST.DoRequest<GuildMember>($"/guilds/{id}/members/{userID}", "PUT", jsonObj, (returnValue) =>
            {
                callback?.Invoke(returnValue as GuildMember);
            });
        }

        public void ModifyGuildMember(DiscordClient client, GuildMember member, List<Role> roles, string channelId, Action callback = null) => ModifyGuildMember(client, member.user.id, member.nick, roles, member.deaf, member.mute, channelId, callback); 
        public void ModifyGuildMember(DiscordClient client, string userID, string nick, List<Role> roles, bool mute, bool deaf, string channelId, Action callback = null)
        {
            var jsonObj = new Dictionary<string, object>()
            {
                { "nick", nick },
                { "roles", roles },
                { "mute", mute },
                { "deaf", deaf },
                { "channel_id", channelId }
            };
            client.REST.DoRequest($"/guilds/{id}/members/{userID}", "PATCH", jsonObj, () =>
            {
                callback?.Invoke();
            });
        }

        public void ModifyCurrentUsersNick(DiscordClient client, string nick, Action<string> callback = null)
        {
            var jsonObj = new Dictionary<string, object>()
            {
                { "nick", nick }
            };
            client.REST.DoRequest<string>($"/guilds/{id}/members/@me/nick", "PATCH", jsonObj, (returnValue) =>
            {
                callback?.Invoke(returnValue as string);
            });
        }

        public void AddGuildMemberRole(DiscordClient client, User user, Role role, Action callback = null) => AddGuildMemberRole(client, user.id, role.id, callback);
        public void AddGuildMemberRole(DiscordClient client, string userID, string roleID, Action callback = null)
        {
            client.REST.DoRequest($"/guilds/{id}/members/{userID}/roles/{roleID}", "PUT", null, () =>
            {
                callback?.Invoke();
            });
        }

        public void RemoveGuildMemberRole(DiscordClient client, User user, Role role, Action callback = null) => RemoveGuildMemberRole(client, user.id, role.id, callback);
        public void RemoveGuildMemberRole(DiscordClient client, string userID, string roleID, Action callback = null)
        {
            client.REST.DoRequest($"/guilds/{id}/members/{userID}/{roleID}", "DELETE", null, () =>
            {
                callback?.Invoke();
            });
        }

        public void RemoveGuildMember(DiscordClient client, GuildMember member, Action callback = null) => RemoveGuildMember(client, member.user.id, callback);
        public void RemoveGuildMember(DiscordClient client, string userID, Action callback = null)
        {
            client.REST.DoRequest($"/guilds/{id}/members/{userID}", "DELETE", null, () =>
            {
                callback?.Invoke();
            });
        }

        public void GetGuildBans(DiscordClient client, Action<List<Ban>> callback = null)
        {
            client.REST.DoRequest<List<Ban>>($"/guilds/{id}/bans", "GET", null, (returnValue) =>
            {
                callback?.Invoke(returnValue as List<Ban>);
            });
        }

        public void CreateGuildBan(DiscordClient client, GuildMember member, int? deleteMessageDays, Action callback = null) => CreateGuildBan(client, member.user.id, deleteMessageDays, callback);
        public void CreateGuildBan(DiscordClient client, string userID, int? deleteMessageDays, Action callback = null)
        {
            var jsonObj = new Dictionary<string, object>()
            {
                { "delete-message-days", deleteMessageDays }
            };
            client.REST.DoRequest($"/guilds/{id}/bans/{userID}", "PUT", jsonObj, () =>
            {
                callback?.Invoke();
            });
        }
        
        public void RemoveGuildBan(DiscordClient client, string userID, Action callback = null)
        {
            client.REST.DoRequest($"/guilds/{id}/bans/{userID}", "DELETE", null, () =>
            {
                callback?.Invoke();
            });
        }

        public void GetGuildRoles(DiscordClient client, Action<List<Role>> callback = null)
        {
            client.REST.DoRequest<List<Role>>($"/guilds/{id}/roles", "GET", null, (returnValue) =>
            {
                callback?.Invoke(returnValue as List<Role>);
            });
        }

        public void CreateGuildRole(DiscordClient client, Role role, Action<Role> callback = null)
        {
            client.REST.DoRequest<Role>($"/guilds/{id}/roles", "POST", role, (returnValue) =>
            {
                callback?.Invoke(returnValue as Role);
            });
        }

        public void ModifyGuildRolePositions(DiscordClient client, List<ObjectPosition> positions, Action<List<Role>> callback = null)
        {
            client.REST.DoRequest<List<Role>>($"/guilds/{id}/roles", "PATCH", positions, (returnValue) =>
            {
                callback?.Invoke(returnValue as List<Role>);
            });
        }

        public void ModifyGuildRole(DiscordClient client, Role role, Action<Role> callback = null) => ModifyGuildRole(client, role.id, role, callback);
        public void ModifyGuildRole(DiscordClient client, string roleID, Role role, Action<Role> callback = null)
        {
            client.REST.DoRequest<Role>($"/guilds/{id}/roles/{roleID}", "PATCH", role, (returnValue) =>
            {
                callback?.Invoke(returnValue as Role);
            });
        }

        public void DeleteGuildRole(DiscordClient client, Role role, Action callback = null) => DeleteGuildRole(client, role.id, callback);
        public void DeleteGuildRole(DiscordClient client, string roleID, Action callback = null)
        {
            client.REST.DoRequest($"/guilds/{id}/roles/{roleID}", "DELETE", null, () =>
            {
                callback?.Invoke();
            });
        }

        public void GetGuildPruneCount(DiscordClient client, int? days, Action<int?> callback = null)
        {
            client.REST.DoRequest<JObject>($"/guilds/{id}/prune?days={days}", "GET", null, (returnValue) =>
            {
                callback?.Invoke((int?)(returnValue as JObject).GetValue("pruned").ToObject((typeof(int?))));
            });
        }

        public void BeginGuildPrune(DiscordClient client, int? days, Action<int?> callback = null)
        {
            client.REST.DoRequest<JObject>($"/guilds/{id}/prune?days={days}", "POST", null, (returnValue) =>
            {
                callback?.Invoke((int?)(returnValue as JObject).GetValue("pruned").ToObject(typeof(int?)));
            });
        }

        public void GetGuildVoiceRegions(DiscordClient client, Action<List<VoiceRegion>> callback = null)
        {
            client.REST.DoRequest<List<VoiceRegion>>($"/guilds/{id}/regions", "GET", null, (returnValue) =>
            {
                callback?.Invoke(returnValue as List<VoiceRegion>);
            });
        }

        public void GetGuildInvites(DiscordClient client, Action<List<Invite>> callback = null)
        {
            client.REST.DoRequest<List<Invite>>($"/guilds/{id}/invites", "GET", null, (returnValue) =>
            {
                callback?.Invoke(returnValue as List<Invite>);
            });
        }

        public void GetGuildIntegrations(DiscordClient client, Action<List<Integration>> callback = null)
        {
            client.REST.DoRequest<List<Integration>>($"/guilds/{id}/int?egrations", "GET", null, (returnValue) =>
            {
                callback?.Invoke(returnValue as List<Integration>);
            });
        }

        public void CreateGuildIntegration(DiscordClient client, Integration integration, Action callback = null) => CreateGuildIntegration(client, integration.type, integration.id, callback);
        public void CreateGuildIntegration(DiscordClient client, string type, string id, Action callback = null)
        {
            var jsonObj = new Dictionary<string, object>()
            {
                { "type", type },
                { "id", id }
            };
            client.REST.DoRequest($"/guilds/{id}/int?egrations", "POST", jsonObj, () =>
            {
                callback?.Invoke();
            });
        }

        public void ModifyGuildIntegration(DiscordClient client, Integration integration, bool? enableEmoticons, Action callback = null) => ModifyGuildIntegration(client, integration.id, integration.expire_behaviour, integration.expire_grace_peroid, enableEmoticons, callback);
        public void ModifyGuildIntegration(DiscordClient client, string integrationID, int? expireBehaviour, int? expireGracePeroid, bool? enableEmoticons, Action callback = null)
        {
            var jsonObj = new Dictionary<string, object>()
            {
                { "expire_behaviour", expireBehaviour },
                { "expire_grace_peroid", expireGracePeroid },
                { "enable_emoticons", enableEmoticons }
            };
            client.REST.DoRequest($"/guilds/{id}/int?egrations/{integrationID}", "PATCH", jsonObj, () =>
            {
                callback?.Invoke();
            });
        }

        public void DeleteGuildIntegration(DiscordClient client, Integration integration, Action callback = null) => DeleteGuildIntegration(client, integration.id, callback);
        public void DeleteGuildIntegration(DiscordClient client, string integrationID, Action callback = null)
        {
            client.REST.DoRequest($"/guilds/{id}/int?egrations/{integrationID}", "DELETE", null, () =>
            {
                callback?.Invoke();
            });
        }

        public void SyncGuildIntegration(DiscordClient client, Integration integration, Action callback = null) => SyncGuildIntegration(client, integration.id, callback);
        public void SyncGuildIntegration(DiscordClient client, string integrationID, Action callback = null)
        {
            client.REST.DoRequest($"/guilds/{id}/int?egrations/{integrationID}/sync", "POST", null, () =>
            {
                callback?.Invoke();
            });
        }

        public void GetGuildEmbed(DiscordClient client, Action<GuildEmbed> callback = null)
        {
            client.REST.DoRequest<GuildEmbed>($"/guilds/{id}/embed", "GET", null, (returnValue) =>
            {
                callback?.Invoke(returnValue as GuildEmbed);
            });
        }

        public void ModifyGuildEmbed(DiscordClient client, GuildEmbed guildEmbed, Action<GuildEmbed> callback = null)
        {
            client.REST.DoRequest<GuildEmbed>($"/guilds/{id}/embed", "PATCH", guildEmbed, (returnValue) =>
            {
                callback.Invoke(returnValue as GuildEmbed);
            });
        }
    }
}
