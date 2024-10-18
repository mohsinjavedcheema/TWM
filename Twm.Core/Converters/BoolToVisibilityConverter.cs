using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Twm.Core.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNegate = parameter is string mode && mode.ToLower() == "negate";

            if (value is bool boolValue)
            {
                if (!isNegate)
                {
                    if (boolValue)
                        return Visibility.Visible;
                }
                else
                {
                    if (!boolValue)
                        return Visibility.Visible;
                }
            }

            return Visibility.Collapsed;

            
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNegate = parameter is string mode && mode.ToLower() == "negate";

            if (value is Visibility visibility)
            {
                if (!isNegate)
                {
                    if (visibility == Visibility.Visible)
                        return true;
                }
                else
                {
                    if (visibility == Visibility.Visible)
                        return false;
                }
            }

            return false;
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}