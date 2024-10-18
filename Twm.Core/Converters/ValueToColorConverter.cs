using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using static System.Double;

namespace Twm.Core.Converters
{
    [ValueConversion(typeof(double), typeof(Brush))]
    public class ValueToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && TryParse(value.ToString(), out var result))
            {
                if (result < 0)
                    return Brushes.Red;

                if (parameter is string mode && mode == "State3")
                {
                    if (result > 0)
                        return Brushes.Green;

                    return Brushes.Black;
                }

            }

            if (parameter == null)
                return Brushes.Black;

            if (parameter is bool isLive)
            {
                if (isLive)
                    return Brushes.Green;
            }

            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(double), typeof(Brush))]
    public class ValueToColorConverterMulti : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length > 0)
            {
                if (values[0] != null && TryParse(values[0].ToString(), out var result))
                {
                    if (result < 0)
                        return Brushes.Red;
                }

                
                if (values[1] != null && values[1] is bool isLive)
                {
                    if (isLive)
                        return Brushes.Green;
                }
            }

            return Brushes.Black;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}