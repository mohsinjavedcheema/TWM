using System.Runtime.Serialization;

namespace Twm.Core.DataProviders.Binance.Enums
{
    public enum OrderType
    {
        [EnumMember(Value = "LIMIT")]
        Limit,
        [EnumMember(Value = "MARKET")]
        Market,
        [EnumMember(Value = "STOP_MARKET")]
        StopMarket,
        [EnumMember(Value = "STOP")]
        StopLimit,
        [EnumMember(Value = "STOP_LOSS_LIMIT")]
        StopLossLimit,
        [EnumMember(Value = "STOP_LOSS")]
        StopLoss,
    }
}
