using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Twm.Core.DataProviders.Bybit.Converter;
using Twm.Core.DataProviders.Bybit.Models.WebSocket.Interfaces;

namespace Twm.Core.DataProviders.Bybit.Models.WebSocket.Reponse
{
    /// <summary>
    /// Bybit Depth data returned from the Depth WebSocket endpoint
    /// </summary>
    [DataContract]
    /*[JsonConverter(typeof(BybitOrderBookResponseConverter))]*/
    public class BybitOrderBookResponse : IWebSocketResponse
    {
        [DataMember(Order = 1)]
        [JsonProperty(PropertyName = "topic")]
        public string Topic { get; set; }

        [DataMember(Order = 2)]
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "data")]
        [DataMember(Order = 3)]
        public BybitBookOrdersInfo Orders { get; set; }

        [JsonProperty(PropertyName = "cts")]
        [DataMember(Order = 4)]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime Cts { get; set; }


        public string Category { get; set; }
    }
}
