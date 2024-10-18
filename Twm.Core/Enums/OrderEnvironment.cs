using System.ComponentModel;

namespace Twm.Core.Enums
{
    public enum OrderEnvironment
    {
        LocalHistorical = 0,
        Realtime = 1,
        [Browsable(false)] Unknown = 99
    }
}