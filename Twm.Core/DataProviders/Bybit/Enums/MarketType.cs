using System.ComponentModel;

namespace Twm.Core.DataProviders.Bybit.Enums
{
    public enum MarketType
    {
        [Description("Spot")]
        Spot = 0,
        [Description("UsdM")]
        UsdM = 1,
        [Description("CoinM")]
        CoinM = 2,
    }
}