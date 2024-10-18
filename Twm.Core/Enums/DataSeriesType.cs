using System.ComponentModel;

namespace Twm.Core.Enums
{

    public enum DataSeriesType
    {
        [Description("Tick")]
        Tick = 6,
        [Description("Second")]
        Second = 5,
        [Description("Minute")]
        Minute = 0,
        [Description("Hour")]
        Hour = 1,
        [Description("Day")]
        Day = 2,
        [Description("Week")]
        Week = 3,
        [Description("Month")]
        Month = 4

    }


}