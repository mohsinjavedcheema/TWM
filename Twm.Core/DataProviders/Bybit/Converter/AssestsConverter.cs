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
    public class AssestsConverter : JsonConverter
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var assets = JArray.Load(reader);
            List<AssetResult> assetList = new List<AssetResult>();


            foreach (var asset in assets)
            {
                var assetResult = new AssetResult();
                
                /*assetResult.Coin = Epoch.AddMilliseconds((long)k.ElementAt(0));
                assetResult.TransferBalance = (decimal)k ElementAt(1);
                assetResult.WalletBalance = (decimal)k.ElementAt(1);*/


                assetList.Add(assetResult);
            }

            return assetList;
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }
    }
}