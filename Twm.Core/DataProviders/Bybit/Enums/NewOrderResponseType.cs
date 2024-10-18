using System.Runtime.Serialization;

namespace Twm.Core.DataProviders.Bybit.Enums
{
    public enum NewOrderResponseType
    {
        [EnumMember(Value = "RESULT")]
        Result,
        [EnumMember(Value = "ACK")]
        Acknowledge,
        [EnumMember(Value = "FULL")]
        Full,
    }
}