using System.Runtime.Serialization;

namespace Twm.Core.DataProviders.Binance.Enums
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