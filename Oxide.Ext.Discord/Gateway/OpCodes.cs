namespace Oxide.Ext.Discord.Gateway
{
    public enum OpCodes
    {
        // Recieve = sent to the client (this extension)
        // Send = sent to the Discord API

        Dispatch = 0,               // Recieve
        Heartbeat = 1,              // Send/Receive
        Identify = 2,               // Send
        StatusUpdate = 3,           // Send
        VoiceStateUpdate = 4,       // Send
        VoiceServerPing = 5,        // Send
        Resume = 6,                 // Send
        Reconnect = 7,              // Receive
        RequestGuildMembers = 8,    // Send
        InvalidSession = 9,         // Receive
        Hello = 10,                 // Receive
        HeartbeatACK = 11           // Receive
    }
}
