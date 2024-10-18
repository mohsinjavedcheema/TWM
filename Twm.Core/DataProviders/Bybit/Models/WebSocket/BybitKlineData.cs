using System;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Converter;
using Twm.Core.DataProviders.Bybit.Models.WebSocket.Interfaces;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Bybit.Models.WebSocket
{
    /// <summary>
    /// Data returned from the Bybit WebSocket Kline endpoint
    /// </summary>
    [DataContract]
    public class BybitKlineData
    {
        [JsonProperty(PropertyName = "start")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime StartTime { get; set; }

        [JsonProperty(PropertyName = "end")]        
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime EndTime { get; set; }

        [JsonProperty(PropertyName = "interval")]     
        public int Interval { get; set; }

        [JsonProperty(PropertyName = "open")]       
        public double Open { get; set; }

        [JsonProperty(PropertyName = "high")]
        public double High { get; set; }

        [JsonProperty(PropertyName = "low")]
        public double Low { get; set; }

        [JsonProperty(PropertyName = "close")]
        public double Close { get; set; }

        [JsonProperty(PropertyName = "volume")]
        public double Volume { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public double Timestamp { get; set; }
    }
}
