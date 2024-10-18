using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;
using Twm.Core.Classes;

namespace Twm.Core.Converters
{
    [ValueConversion(typeof(ObservableCollection<GridColumnInfo>), typeof(double))]
    public class ColumnToWidthConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null && parameter is string columnName)
            {
                if (value is ObservableCollection<GridColumnInfo> columns)
                {
                    var column = columns.FirstOrDefault(x => x.Name == columnName);
                    if (column != null)
                    {
                        return column.Width;
                    }
                }
            }

            return 0;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}