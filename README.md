# Oxide.Ext.Discord - Readme
This is the readme for the oxide extension discord. More stuff will go here later.

### Getting your API Key
I will do this later

### Plugin Example
```csharp
using Oxide.Ext.Discord;
using Oxide.Ext.Discord.DiscordObjects;
using Oxide.Ext.Discord.WebSockets;

namespace Oxide.Plugins
{
    [Info("DiscordToChat", "DylanSMR", 0.1)]
    [Description("Some discord stuff.")]

    class DiscordToChat : RustPlugin
    {
        DiscordClient client;
        void Loaded()
        {
            client = Discord.GetClient("My_Api_Key_Here", true);
        }
        void Unload()
        {
            Discord.CloseClient(client);
        }
        void Discord_MessageCreate(Message message)
        {
            if (!message.content.StartsWith("!")) return;

            var args = message.content.Split(' ');
            switch (args[0].ToLower())
            {
                case "!hello":
                    Message replyMessage = new Message();
                    replyMessage.content = $"Hello {message.author.username}";
                    message.Reply(client, replyMessage, null);
                    break;
            }
        }
    }
}
```

### Hooks
Up to date hooks at dylansmr.us/discord/
