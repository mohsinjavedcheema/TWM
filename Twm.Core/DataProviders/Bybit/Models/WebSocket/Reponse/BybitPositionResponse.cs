
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Converter;
using Twm.Core.DataProviders.Bybit.Models.WebSocket.Interfaces;

namespace Twm.Core.DataProviders.Bybit.Models.WebSocket.Response
{
    
    [DataContract]

    public class BybitPositionResponse : IWebSocketResponse
    {
        [DataMember(Order = 1)]
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }


        [DataMember(Order = 2)]
        [JsonProperty(PropertyName = "topic")]
        public string Topic { get; set; }

        [DataMember(Order = 3)]
        [JsonProperty(PropertyName = "creationTime")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime CreationTime { get; set; }

        [JsonProperty(PropertyName = "data")]
        [DataMember(Order = 4)]
        public List<BybitPositionInfo> Positions { get; set; }
        
    }
}
