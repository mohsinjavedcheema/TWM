using System.ComponentModel;

namespace Twm.Core.Enums
{
    public enum OrderAction
    {
        [Description("Buy")]
        Buy,
        [Description("BuyToCover")]
        BuyToCover,
        [Description("Sell")]
        Sell,
        [Description("SellShort")]
        SellShort,
    }
}