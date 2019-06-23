**Oxide.Ext.Discord** is an Oxide extension which acts as a bridge between Oxide and the Discord API.

Should you encounter a problem or bug with the extension, please feel free to create an issue here. Try to include as much detail as possible, including steps to reproduce the issue. A code example is highly appreciated.

## Installation

To install the extension to your Oxide server, you must follow a few simple steps:
1) Shutdown the server.
2) Open the server files, and navigate to the "Managed" folder (eg. "RustDedicated/Managed")
3) Download the latest release.
4) Unzip the release.
5) Copy the "Oxide.Ext.Discord.dll" file into your "Managed" folder.
6) Start your server!

## Getting your API Key

An API key is used to authenticate requests made to and from Discord.

**Note: DO NOT SHARE YOUR API KEY!** Sharing your key may result in punishments from Discord (including a platform-wide ban) if the token is used to abuse the API.

Obtaining an API Key:
1) Visit the official Discord Developers page here: [Discord Developer Documenation](https://discordapp.com/developers/applications/me)
2) Click "New App".
3) Name your app and click create! *Note: A description isn't required.*
4) You will now be redirected to your created app, at the point you will need to click "Create a Bot User".
5) Under the newly created bot section, under "Username" you will see "Token". Reveal the token and copy it into your plugin.
6) Now it's time to add your new bot to your guild! To add your bot to your guild you must visit the following link, and replace "botUserID" with the client ID found at the top of your Discord app settings page:
https://discordapp.com/oauth2/authorize?client_id=botUserID&scope=bot&permissions=8

**Note:** "permissions=8" in the link will provide the bot with administrative permissions so you won't have to give it some.

## Plugin Example

The following plugin is a simple example on how to use the extension.
This plugin simply adds a 'sad' reaction to any message posted on the Discord server, and then broadcasts the message to the server chat.

```csharp
using Oxide.Ext.Discord;
using Oxide.Ext.Discord.Attributes;
using Oxide.Ext.Discord.DiscordObjects;

namespace Oxide.Plugins
{
    [Info("Discord Ext Example", "Your Name", "1.0.0")]
    class DiscordExtExample : CovalencePlugin
    {
        // Define the DiscordClient field that will be set
        // to when our DiscordClient is created & connected
        [DiscordClient] DiscordClient Client;

        void OnServerInitialized()
        {
            Discord.CreateClient(this, "<api key here>"); // Create a new DiscordClient
        }

        // Called when the connection is completed
        void Discord_Ready(Ready ready)
        {
            Puts("Discord connected!");

            // When this hook is called, our Client variable
            // will be set to the main DiscordClient that
            // has been created for us

            Puts($"Connected to server: {Client.DiscordServer.name}");
        }

        // Called when a message is created on the Discord server
        void Discord_MessageCreate(Message message)
        {
            // Add a sad reaction to the message
            message.CreateReaction(Client, ":sad:");

            // Post the message to chat
            server.Broadcast($"Discord Message: {message.author.username} - {message.content}");
        }
    }
}
```

## Socket Hooks

### DiscordSocket_Initalized

```csharp
void DiscordSocket_Initalized(DiscordClient client)
{
    Puts("Client Initalized!");
}
```

 - Called when the client is created, and the plugin can use it.
 - No return behavior.

### DiscordSocket_HeartbeatSent

```csharp
void DiscordSocket_HeartbeatSent()
{
    Puts("Heartbeat sent to discord!");
}
```

 - Called when a packet was sent to discord to keep up the connection.
 - No return behavior.

### DiscordSocket_WebSocketOpened

```csharp
void DiscordSocket_WebSocketOpened()
{
    Puts("WebSocket Opened!");
}
```

 - Called when the discord socket connects.
 - No return behavior.

### DiscordSocket_WebSocketClosed

```csharp
void DiscordSocket_WebSocketClosed(string reason, int code, bool clean)
{
    Puts("WebSocket closed!");
}
```

 - Called when the web socket is closed for any reason.
 - No return behavior.

### DiscordSocket_WebSocketErrored

```csharp
void DiscordSocket_WebSocketErrored(Exception exception, string message)
{
    Puts($"WebSocket errored:.");
}
```

 - Called when the web socket errors.
 - No return behavior.

## Discord API Event Hooks

### Discord_Ready

```csharp
void Discord_Ready(Ready ready)
{
    Puts("Discord is ready!");
}
```

 - Called when discord is ready, and has started sending data.

### Discord_Resumed

```csharp
void Discord_Resumed(Resumed resumed)
{
    Puts("Discord Connection Resumed!");
}
```

 - Called when the discord connection has been resumed.

### Discord_ChannelCreate

```csharp
void Discord_ChannelCreated(Channel channel)
{
    Puts("Discord Channel Created");
}
```

 - Called when a channel has been created.

### Discord_ChannelUpdate

```csharp
void Discord_ChannelUpdate(Channel updatedChannel, Channel oldChannel)
{
    Puts("Discord Channel Updated");
}
```

 - Called when a channel has been updated.

### Discord_ChannelDelete

```csharp
void Discord_ChannelDelete(Channel channel)
{
    Puts("Discord Channel Deleted!");
}
```

 - Called when a discord channel has been deleted.

### Discord_ChannelPinsUpdate

```csharp
void Discord_ChannelPinsUpdate(ChannelPinsUpdate update)
{
    Puts("The pins on a channel have been updated!");
}
```

 - Called when the pins on a channel have been updated.

### Discord_GuildCreate

```csharp
void Discord_GuildCreate(Guild guild)
{
	Puts("A guild has been created!");
}
```

 - Called when a new guild is created.
 - This is not currently set up correctly.

### Discord_GuildUpdate

```csharp
void Discord_GuildUpdate(Guild guild)
{
    Puts("A guild has been updated!");
}
```

 - Called when a guild has been updated.

### Discord_GuildDelete

```csharp
void Discord_GuildDelete(Guild guild)
{
	Puts("A guild has been deleted!");
}
```

 - Called when a guild is deleted.

### Discord_GuildBanAdd

```csharp
void Discord_GuildBanAdd(User user)
{
    Puts("A user has been banned!");
}
```

 - Called when a user is banned.

### Discord_GuildBanRemove

```csharp
void Discord_GuildBanRemove(User user)
{
    Puts("A user has been unbanned!");
}
```

 - Called when a user is unbanned.

### Discord_GuildEmojisUpdate

```csharp
void Discord_GuildEmojisUpdate(GuildEmojisUpdate update)
{
    Puts("The emoji's have been updated!");
}
```

 - Called when the emoji's in a guild have been updated.

### Discord_GuildIntergrationsUpdate

```csharp
void Discord_GuildIntergrationsUpdate(GuildIntergrationsUpdate update)
{
    Puts("Guild Intergrations Updated!");
}
```

 - Called when the intergrations in a guild have been updated?

### Discord_MemberAdded

```csharp
void Discord_MemberAdded(GuildMember member)
{
    Puts("A user has been added to the server!");
}
```

 - Called when a user joins the discord server.

### Discord_MemberRemoved

```csharp
void Discord_MemberRemoved(GuildMember member)
{
    Puts("A user has been removed from the server!");
}
```

 - Called when a user is removed from the server.

### Discord_GuildMemberUpdate
```csharp
void Discord_GuildMemberUpdate(GuildMemberUpdate update, GuildMember oldMember)
{
    Puts("A guild member has been updated!");
}
```

 - Called when a guild member is updated.

### Discord_GuildMembersChunk

```csharp
void Discord_GuildMembersChunk(GuildMembersChunk chunk)
{
    Puts("A guild members chunk has been recieved!")
}
```

 - Called in response to a 'Gateway Request Guild Members'.

### Discord_GuildRoleCreate

```csharp
void Discord_GuildRoleCreate(Role role)
{
    Puts("A new role has been created!");
}
```

 - Called when a new role is created

### Discord_GuildRoleUpdate

```csharp
void Discord_GuildRoleUpdate(Role newRole, Role oldRole)
{
    Puts("A role has been updated!");
}
```

 - Called when a role is updated.

### Discord_GuildRoleDelete

```csharp
void Discord_GuildRoleDelete(Role role)
{
    Puts("A role has been deleted!");
}
```

 - Called when a role is deleted.

### Discord_MessageCreate

```csharp
void Discord_MessageCreate(Message message)
{
    Puts("A new message has been created!");
}
```

 - Called when a new message is created.

### Discord_MessageUpdate

```csharp
void Discord_MessageUpdate(Message message)
{
    Puts("A message has been updated!");
}
```

 - Called when a message is updated.

### Discord_MessageDelete

```csharp
void Discord_MessageDelete(MessageDelete message)
{
    Puts("A message has been deleted!");
}
```

 - Called when a message is deleted.

### Discord_MessageDeleteBulk

```csharp
void Discord_MessageDeleteBulk(MessageDeleteBulk bulk)
{
    Puts("A bulk of messages have been deleted!");
}
```

 - Called when a bulk set of messages have been deleted.

### Discord_MessageReactionAdd

```csharp
void Discord_MessageReactionAdd(MessageReactionUpdate update)
{
    Puts("A reaction has been added to a message!");
}
```

 - Called when a reaction is added to a message.

### Discord_MessageReactionRemove

```csharp
void Discord_MessageReactionRemove(MessageReactionUpdate update)
{
    Puts("A reaction has been removed from a message!");
}
```

 - Called when a reaction is removed from a message.

### Discord_ReactionRemoveAll

```csharp
void Discord_MessageReactionRemoveAll(MessageReactionRemoveAll reactions)
{
    Puts("All reactions have been removed from a message!");
}
```

 - Called when all reactions are removed from a message.

### Discord_PresenceUpdate

```csharp
void Discord_PresenceUpdate(PResenceUpdate update)
{
    Puts("Someone's presence has been updated!");
}
```

 - Called when a user's presence is updated.

### Discord_TypingStart

```csharp
void Discord_TypingStart(TypingStart start)
{
    Puts("Someone has started typing!");
}
```

 - Called when someone starts typing.

### Discord_UserUpdate

```csharp
void Discord_UserUpdate(User user)
{
    Puts("A user has been updated!");
}
```

 - Called when a user is updated.

### Discord_VoiceStateUpdate

```csharp
void Discord_VoiceStateUpdate(VoiceState state)
{
    Puts("A users voice state has been updated!");
}
```

 - Called when a user's voice state is updated.

### Discord_VoiceServerUpdate

```csharp
void Discord_VoiceServerUpdate(VoiceServerUpdate update)
{
    Puts("The voice server has been updated!");
}
```

 - Called when the voice server is updated.

### Discord_WebhooksUpdate

```csharp
void Discord_WebhooksUpdate(WebhooksUpdate webhooks)
{
    Puts("The webhooks have been updated!");
}
```

 - Called when the webhooks are updated.

### Discord_UnhandledEvent

```csharp
void Discord_UnhandledEvent(JObject messageObject)
{
    Puts("An unhandlded event has occured!");
}
```

 - Called when an event that is not handlded by the extension was raised
 - Please create an issue on the GitHub if this error ever occurs

## Contributing

Want to contribute? Create a fork of the repo and create a pull request for any changes you wish to make!