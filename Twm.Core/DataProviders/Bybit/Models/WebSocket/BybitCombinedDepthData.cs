using System;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Models.WebSocket.Interfaces;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Bybit.Models.WebSocket
{
    [DataContract]
    public class BybitCombinedDepthData : IWebSocketResponse
    {
        [DataMember(Order = 1)]
        [JsonProperty(PropertyName = "stream")]
        public string Stream { get; set; }

        [DataMember(Order = 2)]
        [JsonProperty(PropertyName = "data")]
        public BybitDepthData Data { get; set; }

        public string EventType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime EventTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
