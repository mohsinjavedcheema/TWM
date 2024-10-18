using System;
using System.Linq;
using Twm.Core.DataProviders.Bybit.Models.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using MigraDoc.DocumentObjectModel.Tables;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace Twm.Core.DataProviders.Bybit.Converter
{
    public class KlineCandleSticksConverter : JsonConverter
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var klineCandlesticks = JArray.Load(reader);
            List<KlineCandleStick> klineCandleSticks = new List<KlineCandleStick>();


            foreach (var k in klineCandlesticks)
            {
                var KlineCandleStick = new KlineCandleStick();
                KlineCandleStick.OpenTime = Epoch.AddMilliseconds((long)k.ElementAt(0));
                KlineCandleStick.Open = (decimal)k.ElementAt(1);
                KlineCandleStick.High = (decimal)k.ElementAt(2);
                KlineCandleStick.Low = (decimal)k.ElementAt(3);
                KlineCandleStick.Close = (decimal)k.ElementAt(4);
                KlineCandleStick.Volume = (decimal)k.ElementAt(5);
                KlineCandleStick.Turnover = (string)k.ElementAt(6);

                klineCandleSticks.Add(KlineCandleStick);
            }

            return klineCandleSticks;
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }
    }
}