# Oxide.Ext.Discord - Readme
This is the readme for the oxide extension discord. More stuff will go here later.

### Getting your API Key
I will do this later

### Plugin Example
```csharp
namespace Oxide.Plugins
{
    [Info("Discord Alerts", "DylanSMR", 0.1, ResourceId = 000)]
    [Description("A discord alerts plugin.")]
    class DiscordAlerts : CovalencePlugin
    {
		WebSocketClient client; //Just a variable.
		void DiscordSocket_WebsocketOpened(WebSocketClient client){
			PrintWarning("Websocket Opened Confirmed"); //Prints that the hook is open.
		}
		void DiscordSocket_SocketReady(string url, WebSocketClient cl){
			cl.CreateSocket(); //Creates the actual socket when the extension says its ready.
		}
		void Discord_TextMessage(Message message){
			PrintWarning(message.content); //Prints a message to console
		}
		void Loaded()
		{
			client = new WebSocketClient(false); //Creates the web socket, false meaning it does not create it yet. Can be created at another point.
		}
		void Unload(){
			client.Disconnect(); //Disconnects  the client via unload. If this is not done, the wss server will never unload(probably).
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
