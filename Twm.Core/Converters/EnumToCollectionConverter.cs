﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Twm.Core.Helpers;

namespace Twm.Core.Converters
{
    [ValueConversion(typeof(Enum), typeof(IEnumerable<ValueDescription>))]
    public class EnumToCollectionConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            return EnumHelper.GetAllValuesAndDescriptions(value.GetType());
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