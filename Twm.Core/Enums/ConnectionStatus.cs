using System.ComponentModel;

namespace Twm.Core.Enums
{
    public enum ConnectionStatus
    {
        [Description("Connected")]
        Connected = 0,
        [Description("Disconnected")]
        Disconnected = 1,
        [Description("Connecting")]
        Connecting = 2

    }
}