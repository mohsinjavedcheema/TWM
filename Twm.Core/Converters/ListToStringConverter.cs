using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

namespace Twm.Core.Converters
{
    [ValueConversion(typeof(ObservableCollection<string>), typeof(string))]
    public class ListToStringConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(string))
                throw new InvalidOperationException("The target must be a String");

            return String.Join("\r\n", ((ObservableCollection<string>)value).ToArray());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();

        }
    }
}
