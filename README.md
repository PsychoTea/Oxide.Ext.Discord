# Oxide.Ext.Discord - Readme
This is the readme for the oxide extension discord. More stuff will go here later.

### Getting your API Key
I will do this later

### Plugin Example
```
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
Also gonna do this later.
