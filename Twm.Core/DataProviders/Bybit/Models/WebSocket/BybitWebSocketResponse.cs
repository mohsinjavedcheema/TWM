using System;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Converter;
using Twm.Core.DataProviders.Bybit.Models.WebSocket.Interfaces;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Bybit.Models.WebSocket
{
    [DataContract]
    public class BybitWebSocketResponse : IWebSocketResponse
    {        
        [JsonProperty(PropertyName = "topic")]
        public string Topic { get; set; }
     
        [JsonProperty(PropertyName = "creationTime")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime CreationTime { get; set; }
    }
}