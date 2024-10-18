using System.ComponentModel;

namespace Twm.Core.Enums
{
    public enum TradeSourceType
    {
        [Description("Total")]
        Total,
        [Description("Historical")]
        Historical,
        [Description("RealTime")]
        RealTime,
    }
}