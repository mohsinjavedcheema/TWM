using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using Twm.Core.Enums;
using Twm.Core.Helpers;

namespace Twm.Core.Converters
{
    [ValueConversion(typeof(Enum), typeof(IEnumerable<ValueDescription>))]
    public class FilterEnumToCollectionConverter : IMultiValueConverter
    {
       

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            if (values[0] is StrategyPerformanceSection section)
            {
                if (values[1] is object[] excludeSections && excludeSections.Length >0)
                {
                    return EnumHelper.GetAllValuesAndDescriptions(section.GetType(), excludeSections);

                }
                return EnumHelper.GetAllValuesAndDescriptions(section.GetType());
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

       
    }
}