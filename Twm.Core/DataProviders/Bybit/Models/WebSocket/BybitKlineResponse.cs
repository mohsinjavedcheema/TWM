using System;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Converter;
using Twm.Core.DataProviders.Bybit.Models.WebSocket.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Twm.Core.DataProviders.Bybit.Models.WebSocket
{
    /// <summary>
    /// Data returned from the Bybit WebSocket Kline endpoint
    /// </summary>
    [DataContract]
    public class BybitKlineResponse:IWebSocketResponse
    {
                
        [JsonProperty(PropertyName = "topic")]
        public string Topic { get; set; }
        

        [JsonProperty(PropertyName = "data")]        
        public List<BybitKlineData> Klines { get; set; }

        [JsonProperty(PropertyName = "ts")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime Ts { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
    }
}
