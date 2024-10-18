using System;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Binance.Converter;
using Twm.Core.DataProviders.Binance.Models.WebSocket.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Twm.Core.DataProviders.Binance.Models.WebSocket.Futures
{
    /// <summary>
    /// Shared class that represents either a trade or an order, and the data returned from the WebSocket endpoint
    /// </summary>
    [DataContract]
    public class BinanceTradeOrderData : IWebSocketResponse
    {
        [JsonProperty(PropertyName = "e")]
        public string EventType { get; set; }

        [JsonProperty(PropertyName = "E")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime EventTime { get; set; }

        /// <summary>
        /// Represents Order or Trade time
        /// </summary>
        [JsonProperty(PropertyName = "T")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime TransactionTime { get; set; }

        [JsonProperty(PropertyName = "o")]
        public TradeOrderObjectData TradeOrderObjectData { get; set; }


    }
}