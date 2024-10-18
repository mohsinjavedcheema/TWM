using System.Runtime.Serialization;

namespace Twm.Core.DataProviders.Bybit.Enums
{
    public enum OrderStatus
    {
        [EnumMember(Value = "New")]
        New,
        [EnumMember(Value = "PartiallyFilled")]
        PartiallyFilled,
        [EnumMember(Value = "Untriggered")]
        Untriggered,
        [EnumMember(Value = "Filled")]
        Filled,
        [EnumMember(Value = "Cancelled")]
        Cancelled,
        [EnumMember(Value = "PartiallyFilledCanceled")]
        PartiallyFilledCanceled,
        [EnumMember(Value = "Rejected")]
        Rejected,
        [EnumMember(Value = "Deactivated")]
        Deactivated,
        [EnumMember(Value = "Triggered")]
        Triggered,
    }
}