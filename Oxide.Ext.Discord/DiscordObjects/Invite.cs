namespace Oxide.Ext.Discord.DiscordObjects
{
    using System;
    using Oxide.Ext.Discord.REST;

    public class Invite
    {
        public string code { get; set; }

        public Guild guild { get; set; }

        public Channel channel { get; set; }

        public static void GetInvite(DiscordClient client, string inviteCode, Action<Invite> callback = null)
        {
            client.REST.DoRequest($"/invites/{inviteCode}", RequestMethod.GET, null, callback);
        }

        public void DeleteInvite(DiscordClient client, Action<Invite> callback = null)
        {
            client.REST.DoRequest($"/invites/{code}", RequestMethod.DELETE, null, callback);
        }

        public void AcceptInvite(DiscordClient client, Action<Invite> callback = null)
        {
            client.REST.DoRequest($"/invites/{code}", RequestMethod.POST, null, callback);
        }
    }
}
