using System;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Converter;
using Twm.Core.DataProviders.Bybit.Models.Response.Interfaces;
using Twm.Core.DataProviders.Interfaces;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    /// <summary>
    /// Respone following a call to the Get Compressed Aggregate trades endpoint
    /// </summary>
    [DataContract]
    public class CompressedAggregateTradeResponse: IResponse
    {
        [DataMember(Order = 1)]
        [JsonProperty(PropertyName = "a")]
        public long AggregateTradeId { get; set; }

        [DataMember(Order = 2)]
        [JsonProperty(PropertyName = "p")]
        public decimal Price { get; set; }

        [DataMember(Order = 3)]
        [JsonProperty(PropertyName = "q")]
        public decimal Quantity { get; set; }

        [DataMember(Order = 4)]
        [JsonProperty(PropertyName = "f")]
        public long FirstTradeId { get; set; }

        [DataMember(Order = 5)]
        [JsonProperty(PropertyName = "l")]
        public long LastTradeId { get; set; }

        [DataMember(Order = 6)]
        [JsonProperty("T")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime Timestamp { get; set; }

        [DataMember(Order = 7)]
        [JsonProperty("m")]
        public bool WasBuyerMaker { get; set; }

        [DataMember(Order = 8)]
        [JsonProperty("M")]
        public bool WasBestPriceMatch { get; set; }
    }
}