using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using static System.Double;

namespace Twm.Core.Converters
{
    [ValueConversion(typeof(bool), typeof(Brush))]
    public class CheckToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is bool boolValue)
            {
                if (boolValue)
                    return Brushes.Green;
                return Brushes.Red;
            }

            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    
}