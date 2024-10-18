using System.ComponentModel;

namespace Twm.Core.Enums
{
    public enum AccountType
    {

        [Description("Local paper")]
        LocalPaper = 0,
        [Description("Server paper")]
        ServerPaper = 1,
        [Description("Broker")]
        Broker = 2,
        [Description("Playback")]
        Playback = 3
    }
}