using System.Runtime.Serialization;
using Twm.Core.DataProviders.Interfaces;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    /// <summary>
    /// Response following a call to the Cancel Order endpoint
    /// </summary>
    [DataContract]
    public class CancelOrderResponse : IResponse
    {        
        [JsonProperty(PropertyName = "orderId")]
        public string OrderId { get; set; }
     
        [JsonProperty(PropertyName = "orderLinkId")]
        public string OrderLinkId { get; set; }

        

    }
}