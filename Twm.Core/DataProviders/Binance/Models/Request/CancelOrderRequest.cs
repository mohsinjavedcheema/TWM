using System.Runtime.Serialization;
using Twm.Core.DataProviders.Interfaces;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Binance.Models.Request
{
    /// <summary>
    /// Request object used to cancel a Binance order
    /// </summary>
    [DataContract]
    public class CancelOrderRequest : IRequest
    {
        [DataMember(Order = 1)]
        public string Symbol { get; set; }

        [DataMember(Order = 2)]
        public long? OrderId { get; set; }

        [DataMember(Order = 3)]
        [JsonProperty(PropertyName = "origClientOrderId")]
        public string OriginalClientOrderId { get; set; }

        [DataMember(Order = 4)]
        public string NewClientOrderId { get; set; }
    }
}