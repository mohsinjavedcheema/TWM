using System.ComponentModel;

namespace Twm.Core.Enums
{

    public enum DataSeriesFormat
    {        
        [Description("1m")]
        OneMinute = 0,
        [Description("3m")]
        ThreeMinutes = 1,
        [Description("5m")]
        FiveMinutes = 2,
        [Description("15m")]
        FifteenMinutes = 3,
        [Description("30m")]
        ThirtyMinutes = 4,
        [Description("1h")]
        OneHour = 5,
        [Description("2h")]
        TwoHours = 6,
        [Description("4h")]
        FourHour = 7,
        [Description("6h")]
        SixHours = 8,
        [Description("8h")]
        EightHours = 9,
        [Description("12h")]
        TwelveHours = 10,
        [Description("1d")]
        OneDay = 11,
        [Description("3d")]
        ThreeDay = 12,
        [Description("1w")]
        OneWeek = 13

    }


}