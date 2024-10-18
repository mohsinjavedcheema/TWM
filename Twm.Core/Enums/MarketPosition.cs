using System.ComponentModel;

namespace Twm.Core.Enums
{
    public enum MarketPosition
    {
        [Description("Long")]
        Long,
        [Description("Short")]
        Short,
        [Description("Flat")]
        Flat,
    }
}