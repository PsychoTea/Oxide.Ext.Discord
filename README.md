# Oxide.Ext.Discord
An extension for Oxide, which acts as a bridge between Oxide plugins and the Discord API. Currently being developed by PsychoTea, and co-developed by DylanSMR. Please submit any questions to us via Discord, Oxide, or GitHub.

Should you encounter an issue, please feel free to create an issue here.

Want to contribute? Create a fork of the repo and create a pull request for any changes you wish to make!

### Getting your API Key
An API key is used to authenticate requests made to and from Discord.

NOTE: DO NOT SHARE YOUR API KEY/TOKEN. Sharing your token may result in punishments from Discord if the token is used to abuse the API.

Steps to obtain an API Key.

1) Visit the official Discord Developers page here [Discord Developer Documenation](https://discordapp.com/developers/applications/me)
2) Click "New App".
3) Name your app and click create! A description isn't required.
4) You will now be redirected to your created app, at the point you will need to click "Create a Bot User".
5) Under the newly created bot section you will see "Token" below "Username", Reveal the token and copy it into your plugin.

### Plugin Example
```csharp
using Oxide.Ext.Discord;
using Oxide.Ext.Discord.Attributes;
using Oxide.Ext.Discord.DiscordObjects;

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

        // Called when the connection is completed
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
Some hooks are available in the [Hooks.md file](Hooks.md)

TODO: add all the hooks
