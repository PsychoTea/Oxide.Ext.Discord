# Oxide.Ext.Discord
An extension for oxide that handles the discord api. Currently being developed by PsychoTea, and co-developed by DylanSMR. Any question can be asked to either of us on Discord, Oxide, or github. 

### Getting your API Key
Will do this at some point.

### Plugin Example
```csharp
using Oxide.Ext.Discord;
using Oxide.Ext.Discord.DiscordObjects;
using Oxide.Ext.Discord.WebSockets;

namespace Oxide.Plugins
{
    [Info("Cool Plugin Title", "Your Name", "0.0.1")]
    [Description("Discord Stuff")]

    class DiscordToChat : RustPlugin
    {
        DiscordClient Client; // Capital Variables because Psycho likes those.
        
        public readonly string ApiKey = "Your Super Secret Key Here";
        
        void Loaded()
        {
            Discord.CreateClient(this, ApiKey); //Creates the client, params: Plugin, Key
        }
        
        void DiscordSocket_Initialized(DiscordClient Client) //Called when the client is created and is ready to be used, no return value.
        {
            this.Client = Client;       
            timer.Every(1f, () => Client.UpHandler.SendBeat()); //Sends a alert to the client confirming the plugin is active.
            
            //You can do anything else here. 
        }
        
        void Discord_MessageCreate(Message Message){ //Called when a message is created in discord, params: Message
            Message.CreateReaction(Client, ":sad:"); //Adds a sad emoji to the message. 
            PrintToChat($"Discord Message: [{Message.channel_id}]-{Message.author.name} : {Message.content}");
        }
    }
}
```

### Hooks
~~Up to date hooks at dylansmr.us/discord/~~ Not available, hooks will be available at dylansmr.us at some point.
