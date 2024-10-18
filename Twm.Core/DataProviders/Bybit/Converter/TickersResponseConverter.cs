using System;
using System.Linq;
using Twm.Core.DataProviders.Bybit.Models.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Twm.Core.DataProviders.Bybit.Converter
{
    public class TickersResponseConverter : JsonConverter
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var tickers = JArray.Load(reader);

            var response = new TickersResponse();
           /* response.InstrumentTickers = new List<TickerInfo>();
            
            foreach (var ticker in tickers)
            {  
                response.InstrumentTickers.Add(ticker.ToObject<TickerInfo>());
            }*/
            return response;
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }
    }
}