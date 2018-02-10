namespace Oxide.Ext.Discord
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Oxide.Core;
    using Oxide.Core.Extensions;
    using Oxide.Core.Plugins;

    public class DiscordExtension : Extension
    {
        internal static readonly Version AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;

        public DiscordExtension(ExtensionManager manager) : base(manager)
        {
        }

        public override string Name => "Discord";

        public override string Author => "PsychoTea & DylanSMR";

        public override VersionNumber Version => new VersionNumber(AssemblyVersion.Major, AssemblyVersion.Minor, AssemblyVersion.Build);

        public override void OnModLoad()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, exception) =>
            {
                Interface.Oxide.LogException("An exception was thrown!", exception.ExceptionObject as Exception);
            };
        }

        [HookMethod("OnPluginUnloaded")]
        void OnPluginUnloaded(Plugin name)
        {
            foreach (var client in new List<DiscordClient>(Discord.Clients))
            {
                if (client.OptionalSettings.KeepLoadedAfterUnload) continue;
                if (client.Plugins.Count == 1)
                {
                    if (client.Plugins[0] != name) continue;
                    if (client.OptionalSettings.ExtensiveLogging) Interface.Oxide.LogWarning($"[Discord Ext] Shutting down discord bot registered for plugin {name.Name} as it was set to disconnect upon unload.");
                    Discord.CloseClient(client);
                    continue;
                }
                if(client.Plugins.Count == 0)
                {
                    Discord.CloseClient(client);
                    continue;
                }

                if (client.Plugins.Contains(name))
                {
                    if (client.OptionalSettings.ExtensiveLogging) Interface.Oxide.LogWarning($"[Discord Ext] Handling plugin unload for plugin {name.Name}.");
                    client.Plugins.Remove(name);
                }
            }
        }

        public override void OnShutdown()
        {
            // new List prevents against InvalidOperationException
            foreach (var client in new List<DiscordClient>(Discord.Clients))
            {
                Discord.CloseClient(client);
            }

            Interface.Oxide.LogInfo("[Discord Ext] Disconnected all clients - server shutdown.");
        }
    }
}
