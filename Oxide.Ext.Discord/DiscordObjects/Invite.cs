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
            client.REST.DoRequest<Invite>($"/invites/{inviteCode}", RequestMethod.GET, null, (returnValue) =>
            {
                callback?.Invoke(returnValue as Invite);
            });
        }

        public void DeleteInvite(DiscordClient client, Action<Invite> callback = null)
        {
            client.REST.DoRequest<Invite>($"/invites/{code}", RequestMethod.DELETE, null, (returnValue) =>
            {
                callback?.Invoke(returnValue as Invite);
            });
        }

        public void AcceptInvite(DiscordClient client, Action<Invite> callback = null)
        {
            client.REST.DoRequest<Invite>($"/invites/{code}", RequestMethod.POST, null, (returnValue) =>
            {
                callback?.Invoke(returnValue as Invite);
            });
        }
    }
}
