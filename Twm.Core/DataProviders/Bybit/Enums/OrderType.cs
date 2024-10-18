using System.Runtime.Serialization;

namespace Twm.Core.DataProviders.Bybit.Enums
{
    public enum OrderType
    {
        [EnumMember(Value = "Limit")]
        Limit,
        [EnumMember(Value = "Market")]
        Market,
       /* [EnumMember(Value = "StopMarket")]
        StopMarket,
        [EnumMember(Value = "StopLimit")]
        StopLimit*/
    }
}
