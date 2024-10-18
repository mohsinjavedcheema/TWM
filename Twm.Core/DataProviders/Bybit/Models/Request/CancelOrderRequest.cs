using System.Runtime.Serialization;
using Twm.Core.DataProviders.Interfaces;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Bybit.Models.Request
{
    /// <summary>
    /// Request object used to cancel a Bybit order
    /// </summary>
    [DataContract]
    public class CancelOrderRequest : IRequest
    {
        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; }

        [JsonProperty(PropertyName = "symbol")]
        public string Symbol { get; set; }

        [JsonProperty(PropertyName = "orderId")]
        public string OrderId { get; set; }
        
        [JsonProperty(PropertyName = "orderLinkId")]
        public string OrderLinkId { get; set; }

        
    }
}