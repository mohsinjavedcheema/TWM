using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Twm.Core.DataCalc;

namespace Twm.Core.CustomProperties.Converters
{
    public class SeriesNameConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[1] is IndicatorBase ib)
                return ib.GetType().Name;

            return values[0];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}