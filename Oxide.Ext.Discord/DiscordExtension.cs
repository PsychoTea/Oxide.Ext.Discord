using System;
using System.Reflection;
using Oxide.Core;
using Oxide.Core.Extensions;

namespace Oxide.Ext.Discord
{
    public class DiscordExtension : Extension
    {
        internal static readonly Version AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;

        public DiscordExtension(ExtensionManager manager) : base(manager)
        {
        }

        public override string Name => "Discord";
        public override string Author => "DylanSMR";

        public override VersionNumber Version => new VersionNumber(AssemblyVersion.Major, AssemblyVersion.Minor, AssemblyVersion.Build);
        private Libraries.Discord connection;
        public override void Load()
        {
            connection = new Libraries.Discord(this);
            Manager.RegisterLibrary("Discord", connection);
        }
        public override void LoadPluginWatchers(string plugindir)
        {
        }

        public override void OnModLoad()
        {
        }
    }
}
