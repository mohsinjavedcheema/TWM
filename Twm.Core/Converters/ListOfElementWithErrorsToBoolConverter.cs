using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Twm.Core.Converters
{
    public class ListOfElementWithErrorsToBoolConverter : IMultiValueConverter
    {
        public object Convert(
            object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                return true;
            }

            var list = values[0] as IEnumerable<IDataErrorInfo>;
            if (list == null)
            {
                return true;
            }
            return !list.All(data => !string.IsNullOrEmpty(data.Error));
        }

        public object[] ConvertBack(
            object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
