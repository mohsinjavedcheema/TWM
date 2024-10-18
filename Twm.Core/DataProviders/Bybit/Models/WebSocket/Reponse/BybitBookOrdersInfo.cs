using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Twm.Core.DataProviders.Bybit.Models.WebSocket.Reponse
{
    /// <summary>
    /// Response following a call to the order info
    /// </summary>
    [DataContract]
    public class BybitBookOrdersInfo
    {
        [DataMember(Order = 1)]
        [JsonProperty(PropertyName = "s")]
        public string Symbol { get; set; }

        [JsonProperty(PropertyName = "b")]
        [DataMember(Order = 2)]
        public List<BybitBookOrderInfo> Bids { get; set; }

        [JsonProperty(PropertyName = "a")]
        [DataMember(Order = 3)]
        public List<BybitBookOrderInfo> Asks { get; set; }

        [DataMember(Order = 4)]
        [JsonProperty(PropertyName = "u")]
        public long UpdateId { get; set; }

        [DataMember(Order = 4)]
        [JsonProperty(PropertyName = "seq")]
        public long CrossSequence { get; set; }

    }
}
