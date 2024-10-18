using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Binance.Converter;
using Twm.Core.DataProviders.Binance.Models.Response;
using Twm.Core.DataProviders.Binance.Models.WebSocket.Interfaces;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Binance.Models.WebSocket
{
    /// <summary>
    /// Binance Depth data returned from the Depth WebSocket endpoint
    /// </summary>
    [DataContract]
    public class BinanceDepthData : IWebSocketResponse
    {
        
        [JsonProperty(PropertyName = "e")]
        public string EventType { get; set; }

        
        [JsonProperty(PropertyName = "E")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime EventTime { get; set; }

        
        [JsonProperty(PropertyName = "s")]
        public string Symbol { get; set; }


        [JsonProperty(PropertyName = "U")]
        public long FirstUpdateId { get; set; }

        [JsonProperty(PropertyName = "u")]
        public long FinalUpdateId { get; set; }

        
        [JsonProperty(PropertyName = "b")]
        [JsonConverter(typeof(TraderPriceConverter))]
        public List<TradeResponse> BidDepthDeltas { get; set; }

        
        [JsonProperty(PropertyName = "a")]
        [JsonConverter(typeof(TraderPriceConverter))]
        public List<TradeResponse> AskDepthDeltas { get; set; }
    }
}