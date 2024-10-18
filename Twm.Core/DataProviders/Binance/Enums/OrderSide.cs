using System.Runtime.Serialization;

namespace Twm.Core.DataProviders.Binance.Enums
{
    public enum OrderSide
    {
        [EnumMember(Value = "BUY")]
        Buy,
        [EnumMember(Value = "SELL")]
        Sell,
    }
}