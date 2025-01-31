﻿using System;
using System.ComponentModel;
using System.Globalization;

namespace Twm.Core.Converters
{
    public class MySmartExpandableObjectConverter : ExpandableObjectConverter
    {
        TypeConverter actualConverter = null;

        private void InitConverter(ITypeDescriptorContext context)
        {
            if (actualConverter == null)
            {
                TypeConverter parentConverter = TypeDescriptor.GetConverter(context.Instance);
                PropertyDescriptorCollection coll = parentConverter.GetProperties(context.Instance);
                PropertyDescriptor pd = coll[context.PropertyDescriptor.Name];

                if (pd.PropertyType == typeof(object))
                    actualConverter = TypeDescriptor.GetConverter(pd.GetValue(context.Instance));
                else
                    actualConverter = this;
            }
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (context == null)
                return false;

            InitConverter(context);

            return actualConverter.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (context == null)
                return null;


            InitConverter(context); // I guess it is not needed here

            return actualConverter.ConvertFrom(context, culture, value);
        }
    }
}