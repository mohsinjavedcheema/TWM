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
    [ValueConversion(typeof(ObservableCollection<GridColumnInfo>), typeof(Visibility))]
    public class ColumnToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null && parameter is string columnName)
            {
                if (value is ObservableCollection<GridColumnInfo> columns)
                {
                    
                    var column = columns.FirstOrDefault(x => x.Name == columnName);
                    if (column != null && column.Visibility == Visibility.Visible)
                    {
                        return Visibility.Visible;
                    }
                }
            }

            return Visibility.Collapsed;
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