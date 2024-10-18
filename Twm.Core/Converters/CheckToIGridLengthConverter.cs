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
    [ValueConversion(typeof(bool), typeof(GridLength))]
    public class CheckToGridLengthConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCheck)
            {
                if (!isCheck)
                    return new GridLength(0);

                if (parameter != null && parameter is string columnWidth)
                {
                    if (int.TryParse(columnWidth, out var width))
                    {
                        return new GridLength(width, GridUnitType.Pixel);
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