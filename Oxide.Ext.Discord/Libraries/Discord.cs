using System.IO;
using Oxide.Core;
using Oxide.Core.Libraries;

namespace Oxide.Ext.Discord.Libraries
{
    public class Discord : Library
    {
        public static bool IsConfigured = true;
        private readonly string _ConfigDirectory;
        private readonly string _DataDirectory;
        DiscordExtension extension;
        private readonly string _PluginDirectory;
        public static DiscordSettings Settings;
        private readonly DataFileSystem _DataFileSystem;

        public Discord(DiscordExtension ext)
        {
            _DataFileSystem = Interface.Oxide.DataFileSystem;
            _ConfigDirectory = Interface.Oxide.ConfigDirectory;
            _DataDirectory = Interface.Oxide.DataDirectory;
            _PluginDirectory = Interface.Oxide.PluginDirectory;
            this.extension = ext;
            Load();
        }

        public void CheckConfig()
        {
            string path = Path.Combine(_ConfigDirectory, "Discord");
            if (_DataFileSystem.ExistsDatafile(path))
            {
                Settings = _DataFileSystem.ReadObject<DiscordSettings>(path);
                if (Settings.Version != extension.Version.ToString())
                {
                    Settings.Version = extension.Version.ToString();
                    SaveConfig();
                }
                return;
            }

            Interface.Oxide.LogInfo("[Discord Ext] Creating Default Configuration");
            Settings = new DiscordSettings();
            Settings.ApiToken = "change-me-please";
            Settings.Version = extension.Version.ToString();

            SaveConfig();
        }

        private void SaveConfig() => _DataFileSystem.WriteObject(Path.Combine(_ConfigDirectory, "Discord"), Settings);

        internal void Load()
        {
            CheckConfig();
            if (Settings.ApiToken == "change-me-please")
            {
                Interface.Oxide.LogWarning("[Discord Ext] Please enter in a APIKEY within the config to use this plugin! ");
                IsConfigured = false;
                return;
            }
        }
    }
}
