# Oxide.Ext.Discord - Readme
This is the readme for the oxide extension discord. More stuff will go here later.

### Getting your API Key
I will do this later

### Plugin Example
```csharp
using Oxide.Ext.Discord.Libraries.WebSockets;
using Oxide.Ext.Discord.Libraries;
using Oxide.Core.Libraries.Covalence;

namespace Oxide.Plugins
{
    [Info("Test Discord Alerts", "DylanSMR", 0.1)]
    [Description("A test plugin for the Oxide.Ext.Discord Extension.")]
    class DiscordAlerts : CovalencePlugin
    {
		DiscordClient client;
		private const string token = "Insert_Api_Key_Here";
		void Loaded()
		{
			client = Discord.GetClient(token, true);
		}
		void DiscordSocket_WebSocketOpened(){
			LogWarning("Web Socket Opened!");
		}
		void Unload(){
			var localClient = Discord.GetClient(token, false, true);
			if(localClient != null) 
				if(Discord.CloseClient(localClient)) LogWarning("Discord Connection Ended");
				else LogWarning("Failed to end discord connection.");
		}
    }
}
```

### Hooks
 - DiscordSocket_APIKeyException | Called when the api key is not provided in the configuration file
 - DiscordSocket_SocketConnecting(string url) | Called when the extension is getting ready to connect to the servers | Return anything to override.
 - DiscordSocket_SocketReady(string url, DiscordClient client) | Called when the socket is ready to connect to the servers
 - DiscordSocket_SocketUrlError | Called when there is an error getting the wss url
 - DiscordSocket_WebsocketOpened | Called when the web socket is opened and sent data
 - DiscordSocket_WebsocketClosed(string reason, ushort code, bool wasClean) | Called when the web socket closes.
 - DiscordSocket_WebsocketError(string exception) | Called when an exception has been raised.
 - DiscordSocket_HeartbeatSent | Called when a heartbeat is sent. Roughly every 41.25 seconds.
 - DiscordSocket_ReconnectingStarted | Called when the client starts to reconnect to the server.
 - Discord_TextMessage(Message message) | Called when a message is received from discord.
 - Discord_MemberAdded(User user) | Called when a member is added to the discord server.
 - Discord_MemberRemoved(User user) | Called when a member is removed from the discord server.
 - Discord_RawMessage(JObject object) | Called when a unhandled message is caught. 
