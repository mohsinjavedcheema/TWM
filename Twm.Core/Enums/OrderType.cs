using System.ComponentModel;

namespace Twm.Core.Enums
{
    public enum OrderType
    {
        Limit = 0,
        Market = 1,
        MIT = 2,
        StopLimit = 3,
        StopMarket = 4,
        [Browsable(false)] Unknown = 99
    }

}