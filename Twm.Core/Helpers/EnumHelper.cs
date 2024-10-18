using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using Twm.Core.Enums;

namespace Twm.Core.Helpers
{
    public static class EnumHelper
    {
        public static string Description(this Enum value)
        {
            var attributes = value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Any())
                return (attributes.First() as DescriptionAttribute)?.Description;

            // If no description is found, the least we can do is replace underscores with spaces
            // You can add your own custom default formatting logic here
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            return ti.ToTitleCase(ti.ToLower(value.ToString().Replace("_", " ")));
        }

        public static IEnumerable<ValueDescription> GetAllValuesAndDescriptions(Type t, object[] excludes = null)
        {
            if (!t.IsEnum)
                throw new ArgumentException($"{nameof(t)} must be an enum type");


            if (excludes != null && excludes.Length > 0)
            {

                return Enum.GetValues(t).Cast<Enum>().Where(x => !excludes.Contains(x))
                    .Select((e) => new ValueDescription() { Value = e, Description = e.Description() }).ToList();
                
            }

            return Enum.GetValues(t).Cast<Enum>()
                .Select((e) => new ValueDescription() { Value = e, Description = e.Description() }).ToList();

        }


        public static string ToAbbr(this DataSeriesType EnumValue) 
        {
            switch (EnumValue)
            {
                case DataSeriesType.Tick:
                    return "t";
                case DataSeriesType.Second:
                    return "s";
                case DataSeriesType.Minute:
                    return "m";
                case DataSeriesType.Hour:
                    return "h";
                case DataSeriesType.Day:
                    return "d";
                case DataSeriesType.Week:
                    return "w";
                case DataSeriesType.Month:
                    return "mo";
            }

            return "";
        }

        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }


        public static string GetEnumMemberAttrValue<T>(T enumVal)
        {
            var enumType = typeof(T);
            var memInfo = enumType.GetMember(enumVal.ToString());
            var attr = memInfo.FirstOrDefault()?.GetCustomAttributes(false).OfType<EnumMemberAttribute>().FirstOrDefault();
            if (attr != null)
            {
                return attr.Value;
            }

            return null;
        }


    }

    public class ValueDescription
    {
        public Enum Value { get; set; }

        public string Description { get; set; }
    }
}