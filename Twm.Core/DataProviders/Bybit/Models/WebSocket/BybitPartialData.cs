using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Converter;
using Twm.Core.DataProviders.Bybit.Models.Response;
using Twm.Core.DataProviders.Bybit.Models.WebSocket.Interfaces;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Bybit.Models.WebSocket
{
    [DataContract]
    public class BybitPartialData : IWebSocketResponse
    {
        public string EventType { get; set; } = "PartialDepthBook";

        public DateTime EventTime { get; set; } = DateTime.UtcNow;

        [DataMember(Order = 1)]
        [JsonProperty(PropertyName = "lastUpdateId")]
        public int LastUpdateId { get; set; }

        [DataMember(Order = 2)]
        [JsonProperty(PropertyName = "bids")]
        [JsonConverter(typeof(TraderPriceConverter))]
        public List<TradeResponse> Bids { get; set; }

        [DataMember(Order = 3)]
        [JsonProperty(PropertyName = "asks")]
        [JsonConverter(typeof(TraderPriceConverter))]
        public List<TradeResponse> Asks { get; set; }
 
    }
}
