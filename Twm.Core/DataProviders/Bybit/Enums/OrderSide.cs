using System.Runtime.Serialization;

namespace Twm.Core.DataProviders.Bybit.Enums
{
    public enum OrderSide
    {
        [EnumMember(Value = "Buy")]
        Buy,
        [EnumMember(Value = "Sell")]
        Sell,
        [EnumMember(Value = "")]
        Unknown,
    }
}