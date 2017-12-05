# Oxide.Ext.Discord [![Build](https://ci.appveyor.com/api/projects/status/github/PsychoTea/Oxide.Ext.Discord?svg=true)](https://ci.appveyor.com/project/PsychoTea/oxide-ext-discord)
**Oxide.Ext.Discord** is an Oxide extension which acts as a bridge between Oxide and the Discord API. It is currently being developed by PsychoTea, and co-developed by DylanSMR. Please submit any questions to us via Discord, Oxide, or here on GitHub.

Should you encounter a problem or bug with the extension, please feel free to create an issue here. Try to include as much detail as possible, including steps to reproduce the issue. A code example is highly appreciated.

Want to contribute? Create a fork of the repo and create a pull request for any changes you wish to make!

### Installation

To install the extension to your Oxide server, you must follow a few simple steps:
1) Shutdown the server.
2) Open the server files, and navigate to the "Managed" folder (eg "RustDedicated/Managed")
3) Download the latest release from the [Releases](https://github.com/PsychoTea/Oxide.Ext.Discord/releases) page.
4) Unzip the release.
5) Copy the "Oxide.Ext.Discord.dll" file into your "Managed" folder.
6) Start your server!

### Getting your API Key
An API key is used to authenticate requests made to and from Discord.

**NOTE: DO NOT SHARE YOUR API KEY.** Sharing your key may result in punishments from Discord (including a platform-wide ban) if the token is used to abuse the API.

Obtaining an API Key:
1) Visit the official Discord Developers page here: [Discord Developer Documenation](https://discordapp.com/developers/applications/me)
2) Click "New App".
3) Name your app and click create! *Note: A description isn't required.*
4) You will now be redirected to your created app, at the point you will need to click "Create a Bot User".
5) Under the newly created bot section, under "Username" you will see "Token". Reveal the token and copy it into your plugin.
6) Now it's time to add your new bot to your guild! To add your bot to your guild you must visit the following link, and replace "botUserID" with the client ID found at the top of your Discordapp settings page.
https://discordapp.com/oauth2/authorize?client_id=botUserID&scope=bot&permissions=8 

*NOTE: "permissions=8" in the link will provide the bot with administrative permissions so you won't have to give it some.*

### Plugin Example

The following plugin is a simple example on how to use the extension.
This plugin simply adds a 'sad' reaction to any message posted on the Discord server, and then broadcasts the message to the server chat.

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
            // Add a sad reaction to the message
            message.CreateReaction(Client, ":sad:");

            // Post the message to chat
            server.Broadcast($"Discord Message: {message.author.username} - {message.content}");
        }
    }
}
```

### Hooks
All hooks are available in the [Hooks.md file](Hooks.md).
If you wish for an additional hook to be added, please create an Issue on the [Issues](https://github.com/PsychoTea/Oxide.Ext.Discord/issues) page. Make sure to include what you want the hook to do, and what you are trying to achieve.