# Oxide.Ext.Discord
An extension for Oxide, which acts as a bridge between Oxide plugins and the Discord API. Currently being developed by PsychoTea, and co-developed by DylanSMR. Please submit any questions to us via Discord, Oxide, or GitHub.

Should you encounter an issue, please feel free to create an issue here.

Want to contribute? Create a fork of the repo and create a pull request for any changes you wish to make!

### Getting your API Key
TODO: Explain how to get an API key

### Plugin Example
```csharp
using Oxide.Ext.Discord;
using Oxide.Ext.Discord.Attributes;
using Oxide.Ext.Discord.DiscordObjects;
using Oxide.Ext.Discord.WebSockets;

namespace Oxide.Plugins
{
    [Info("DiscordExtExample", "Your Name", "1.0.0")]

    class DiscordExtExample : CovalencePlugin
    {
        // Define the DiscordClient field that will be set
        // to when our DiscordClient is created & connected
        [DiscordClient] DiscordClient Client;

        void OnServerInitialized()
        {
            Discord.CreateClient(this, "<api key here>"); // Create a new DiscordClient
        }

        // Called when 
        void DiscordSocket_Initialized()
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
            message.CreateReaction(Client, ":sad:"); // Add a sad reaction to the message

            // Post the message to chat
            server.Broadcast($"Discord Message: {message.author.username} - {message.content}");
        }
    }
}
```

### Hooks
~~Up to date hooks at dylansmr.us/discord/~~ Not available, hooks will be available at dylansmr.us at some point.
