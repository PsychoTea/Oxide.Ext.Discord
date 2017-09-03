using System;
using System.Reflection;
using Oxide.Core;
using Oxide.Core.Extensions;

namespace Oxide.Ext.Discord
{
    public class DiscordExtension : Extension
    {
        internal static readonly Version AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;

        public DiscordExtension(ExtensionManager manager) : base(manager) { }

        public override string Name => "Discord";
        public override string Author => "DylanSMR";
        public override VersionNumber Version => new VersionNumber(AssemblyVersion.Major, AssemblyVersion.Minor, AssemblyVersion.Build);

        public override void OnModLoad()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Interface.Oxide.LogException("An exception was thrown!", e.ExceptionObject as Exception);
        }
    }
}
