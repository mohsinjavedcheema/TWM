using System;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Binance.Converter;
using Twm.Core.DataProviders.Binance.Models.WebSocket.Interfaces;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Binance.Models.WebSocket
{
    /// <summary>
    /// Data returned from the Binance WebSocket Kline endpoint
    /// </summary>
    [DataContract]
    public class BinanceKlineData: ISymbolWebSocketResponse
    {
        [JsonProperty(PropertyName = "e")]
        [DataMember(Order = 1)]
        public string EventType { get; set; }

        [JsonProperty(PropertyName = "E")]
        [DataMember(Order = 2)]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime EventTime { get; set; }

        [JsonProperty(PropertyName = "s")]
        [DataMember(Order = 3)]
        public string Symbol { get; set; }

        [JsonProperty(PropertyName = "k")]
        [DataMember(Order = 4)]
        public BinanceKline Kline { get; set; }
    }
}
