using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twm.Core.DataProviders.Bybit.Models.WebSocket.Reponse;
using System.Globalization;

namespace Twm.Core.DataProviders.Bybit.Converter
{
    public class BybitBookOrderInfoConverter : JsonConverter
    {

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var orderInfo = JArray.Load(reader);

            var bybitOrderInfo = new BybitBookOrderInfo();

            if (double.TryParse(orderInfo[0].ToString(), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                bybitOrderInfo.Price = result;
            }

            if (double.TryParse(orderInfo[1].ToString(), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out var result2))
            {
                bybitOrderInfo.Size = result2;
            }



            return bybitOrderInfo;
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }
    }
}
