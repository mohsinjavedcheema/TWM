using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Twm.Core.DataProviders.Bybit.Converter
{
    public class EpochTimeConverter : DateTimeConverterBase
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if ((DateTime)value == DateTime.MinValue)
            {
                writer.WriteNull();
                return;
            }
            writer.WriteRawValue(Math.Floor(((DateTime)value - Epoch).TotalMilliseconds).ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
            {
                return null;
            }

            if (long.TryParse(reader.Value.ToString(), out var result))
            {
                return Epoch.AddMilliseconds(result);
            }
            throw new Exception("Invalid cast");
        }
    }
}