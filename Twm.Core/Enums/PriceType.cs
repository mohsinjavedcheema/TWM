using System.ComponentModel;
using Twm.Core.CustomProperties.Converters;

namespace Twm.Core.Enums
{
    [TypeConverter(typeof(ExcludePriceTypeConverter))]
    public enum PriceType
    {
        Unset = 0,
        Open = 1,
        High = 2,
        Low = 3,
        Close = 4,
    }
}