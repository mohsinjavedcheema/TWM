using System;
using System.Windows.Data;

namespace Twm.Core.Converters
{
    public class IntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {



            int f;
            if (value is string s )
            {
                if (string.IsNullOrEmpty(s))
                    return "";

                if (int.TryParse(s, out f))
                {
                    return f;
                }
            }

            return 0f;
        }
    }
}
