using System.Runtime.Serialization;

namespace Twm.Core.DataProviders.Binance.Enums
{
    public enum ExecutionType
    {
        [EnumMember(Value = "NEW")]
        New,
        [EnumMember(Value = "CANCELED")]
        Cancelled,
        [EnumMember(Value = "REPLACED")]
        Replaced,
        [EnumMember(Value = "REJECTED")]
        Rejected,
        [EnumMember(Value = "TRADE")]
        Trade,
        [EnumMember(Value = "EXPIRED")]
        Expired,
        [EnumMember(Value = "CALCULATED")]
        Calculated,
        [EnumMember(Value = "AMENDMENT")]
        Amendent,
    }
}
