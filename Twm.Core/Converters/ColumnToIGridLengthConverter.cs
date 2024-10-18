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
    [ValueConversion(typeof(ObservableCollection<GridColumnInfo>), typeof(GridLength))]
    public class ColumnToGridLengthConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null && parameter is string columnIndexStr)
            {
                if (value is ObservableCollection<GridColumnInfo> columns)
                {
                    if (int.TryParse(columnIndexStr, out var columnIndex))
                    {
                        var count = columns.Count(x => x.Visibility == Visibility.Visible);
                        if (columnIndex < count)
                            return new GridLength(1, GridUnitType.Star);
                    }
                }
            }

               return new GridLength(0); 
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