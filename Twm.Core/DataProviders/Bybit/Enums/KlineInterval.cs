using System.Runtime.Serialization;

namespace Twm.Core.DataProviders.Bybit.Enums
{
    public enum KlineInterval
    {
        [EnumMember(Value = "1")] OneMinute,
        [EnumMember(Value = "3")] ThreeMinutes,
        [EnumMember(Value = "5")] FiveMinutes,
        [EnumMember(Value = "15")] FifteenMinutes,
        [EnumMember(Value = "30")] ThirtyMinutes,
        [EnumMember(Value = "60")] OneHour,
        [EnumMember(Value = "120")] TwoHours,
        [EnumMember(Value = "240")] FourHours,
        [EnumMember(Value = "360")] SixHours,        
        [EnumMember(Value = "720")] TwelveHours,
        [EnumMember(Value = "D")] OneDay,        
        [EnumMember(Value = "M")] OneWeek,
        [EnumMember(Value = "W")] OneMonth
    }
}