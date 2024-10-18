using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Twm.Core.Classes;
using Twm.Core.Helpers;
using Microsoft.EntityFrameworkCore.Internal;

namespace Twm.Core.Converters
{
    [ValueConversion(typeof(ObservableCollection<GridColumnInfo>), typeof(int))]
    public class ColumnToIndexConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null && parameter is string columnName)
            {
                var columns = value as ObservableCollection<GridColumnInfo>;

                if (columns != null)
                {
                    var visibleColumns = columns.Where(x => x.Visibility == Visibility.Visible).ToList();

                    
                    var column = visibleColumns.FirstOrDefault(x => x.Name == columnName);
                    if (column != null)
                    {
                        return visibleColumns.IndexOf(column);
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