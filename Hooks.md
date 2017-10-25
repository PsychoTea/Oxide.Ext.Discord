# Socket Hooks

## DiscordSocket_Initalized
```csharp
void DiscordSocket_Initalized(DiscordClient client)
{
  PrintWarning("Client Initalized!");
}
```

 - Called when the client is created, and the plugin can use it.
 - No return behavior
 
## DiscordSocket_SocketConnecting
```csharp
void DiscordSocket_SocketConnecting(string URL)
{
  PrintWarning($"Discord Extension is using {URL} as its web socket Url!");
}
```

 - Called before the client connects to discords web socket.
 - Return any non-null value to cancel.
 
## DiscordSocket_HeartbeatSent
```csharp
void DiscordSocket_HeartbeatSent()
{
  PrintWarning("Heartbeat sent to discord!");
}
```

 - Called when a packet was sent to discord to keep up the connection.
 - No return behavior.
 
## DiscordSocket_WebSocketOpened
```csharp
void DiscordSocket_WebSocketOpened()
{
  PrintWarning("WebSocket Opened!");
}
```

 - Called when the discord socket connects. 
 - No return behavior.
 
## DiscordSocket_WebSocketClosed
```csharp
void DiscordSocket_WebSocketClosed(string reason, int code, bool clean)
{
  PrintWarning("WebSocket closed!");
}
```

 - Called when the web socket is closed for any reason.
 - No return behavior.
 
## DiscordSocket_WebSocketErrored
```csharp
void DiscordSocket_WebSocketErrored(string exception, string message)
{
  PrintWarning("WebSocket Errored");
}
```

 - Called when the web socket errors.
 - Exception may be type Exception and not a string.
 - No return behavior
 
# Discord Hooks *(No return behavior)*

## Discord_Ready
```csharp
void Discord_Ready(Ready ready)
{
  PrintWarning("Discord is ready!");
}
```

 - Called when discord is ready, and has started sending things.
 
## Discord_Resumed
```csharp
void Discord_Resumed(Resumed resumed)
{
  PrintWarning("Discord Connection Resumed!");
}
```

 - Called when the discord connection has been resumed.
 
## Discord_ChannelCreate
```csharp
void Discord_ChannelCreated(Channel channel)
{
  PrintWarning("Discord Channel Created");
}
```

 - Called when a channel has been created in the discord server.
 
## Discord_ChannelUpdate
```csharp
void Discord_ChannelUpdate(Channel channel)
{
  PrintWarning("Discord Channel Updated");
}
```

 - Called when a channel has been updated.
 
## Discord_ChannelDelete
```csharp
void Discord_ChannelDelete(Channel channel){
  PrintWarning("Discord Channel Deleted!");
}
```

 - Called when a discord channel has been deleted.
 
## Discord_ChannelPinsUpdate
```csharp
void Discord_ChannelPinsUpdate(ChannelPinsUpdate update)
{
  PrintWarning("The pins on a channel have been updated!");
}
```

 - Called when the pins on a channel have been updated.
 
## Discord_GuildUpdate
```csharp
void Discord_GuildUpdate(Guild guild)
{
  PrintWarning("A guild has been updated!");
}
```

 - Called when a guild has been updated.

## Discord_GuildBanAdd
```csharp
void Discord_GuildBanAdd(GuildBan ban)
{
  PrintWarning("A user has been banned!");
}
```

 - Called when a user has been banned
 
## Discord_GuildBanRemove
```csharp
void Discord_GuildBanRemove(GuildBan ban)
{
  PrintWarning("A user has been unbanned!");
}
```

 - Called when a user is unbanned.
 
## Discord_GuildEmojisUpdate
```csharp
void Discord_GuildEmojisUpdate(GuildEmojisUpdate update)
{
  PrintWarning("The emoji's have been updated!");
}
```

 - Called when the emoji's in a guild have been updated.
 
## *Discord_GuildIntergrationsUpdate*
```csharp
void Discord_GuildIntergrationsUpdate(GuildIntergrationsUpdate update)
{
  PrintWarning("Guild Intergrations Updated!");
}
```

 - *Called when the intergrations in a guild have been updated?*
 
## Discord_MemberAdded
```csharp
void Discord_MemberAdded(GuildMemberAdd add)
{
  PrintWarning("A user has been added to the server!");
}
```

 - Called when a user joins the discord server.
 
## Discord_MemberRemoved
```csharp
void Discord_MemberRemoved()
{
  PrintWarning("A user has been removed from the server!");
}
```

 - Called when a user is removed from the server.
 
## Discord_GuildMemberUpdate
```csharp
void Discord_GuildMemberUpdate(GuildMemberUpdate update)
{
  PrintWarning("A guild member has been updated!");
}
```

 - Called when a guild member is updated.
