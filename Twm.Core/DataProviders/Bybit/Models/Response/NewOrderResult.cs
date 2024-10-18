using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    /// <summary>
    /// Full Response following a call to the Create Order endpoint
    /// </summary>
    [DataContract]
    public class NewOrderResult 
    {

        [JsonProperty(PropertyName = "orderId")]
        public string OrderId { get;set; }

      
        [JsonProperty(PropertyName = "orderLinkId")]
        public string OrderLinkId { get; set; }
    }
}

    