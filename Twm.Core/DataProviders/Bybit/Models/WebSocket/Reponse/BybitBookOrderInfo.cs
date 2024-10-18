using Newtonsoft.Json;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Converter;

namespace Twm.Core.DataProviders.Bybit.Models.WebSocket.Reponse
{
    /// <summary>
    /// Response following a call to the order info
    /// </summary>
    [DataContract]
    [JsonConverter(typeof(BybitBookOrderInfoConverter))]
    public class BybitBookOrderInfo
    {
        [JsonProperty(PropertyName = "price")]
        [DataMember(Order = 1)]
        public double Price { get; set; }

        [JsonProperty(PropertyName = "symbol")]
        [DataMember(Order = 2)]
        public double Size { get; set; }


    }
}
