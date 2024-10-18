using System;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Binance.Converter;
using Twm.Core.DataProviders.Binance.Models.WebSocket.Interfaces;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Binance.Models.WebSocket
{
    [DataContract]
    public class BinanceWebSocketResponse : IWebSocketResponse
    {
        [DataMember(Order = 1)]
        [JsonProperty(PropertyName = "e")]
        public string EventType { get; set; }

        [DataMember(Order = 2)]
        [JsonProperty(PropertyName = "E")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime EventTime { get; set; }
    }
}