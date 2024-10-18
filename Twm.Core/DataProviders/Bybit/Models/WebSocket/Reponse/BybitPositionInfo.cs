using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Converter;
using Twm.Core.DataProviders.Bybit.Enums;

namespace Twm.Core.DataProviders.Bybit.Models.WebSocket
{
    /// <summary>
    /// Response following a call to the position info
    /// </summary>
    [DataContract]
    public class BybitPositionInfo
    {
        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; }

        [JsonProperty(PropertyName = "symbol")]
        public string Symbol { get; set; }


        [JsonProperty(PropertyName = "side")]
        [JsonConverter(typeof(StringEnumConverter))]
        public OrderSide Side { get; set; }

        [JsonProperty(PropertyName = "size")]
        public decimal Size { get; set; }

        [JsonProperty(PropertyName = "positionIdx")]
        public int PositionIdx { get; set; }

        [JsonProperty(PropertyName = "positionValue")]
        public decimal? PositionValue { get; set; }



        [JsonProperty(PropertyName = "avgPrice")]
        public decimal? AvgPrice { get; set; }


        [JsonProperty(PropertyName = "entryPrice")]
        public decimal? EntryPrice { get; set; }

       


      


        [JsonProperty(PropertyName = "createdTime")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime CreatedTime { get; set; }

        [JsonProperty(PropertyName = "updatedTime")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime UpdatedTime { get; set; }

    }
}
