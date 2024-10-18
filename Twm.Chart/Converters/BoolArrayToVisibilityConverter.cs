using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Twm.Chart.Converters
{
    internal sealed class BoolArrayToVisibilityConverter : IMultiValueConverter
    {
        // values[0] - bool bool1
        // values[1] - bool bool2
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
            values?.OfType<bool>()?.All(_ => _) ?? false ? Visibility.Visible : Visibility.Collapsed;

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
