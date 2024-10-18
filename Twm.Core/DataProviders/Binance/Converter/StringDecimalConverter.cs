using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Twm.Core.DataProviders.Binance.Converter
{
    public class StringDecimalConverter : JsonConverter
    {
        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(decimal) || objectType == typeof(decimal?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value.ToString();

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            throw new NotSupportedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((decimal)value).ToString(CultureInfo.InvariantCulture));
        }
    }
}
