using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Twm.Core.Converters
{
    [ValueConversion(typeof(Visibility), typeof(bool))]
    public class VisibilityToBoolConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                if (visibility == Visibility.Visible)
                    return true;
            }
            return false;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool visible)
            {
                if (visible)
                    return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}