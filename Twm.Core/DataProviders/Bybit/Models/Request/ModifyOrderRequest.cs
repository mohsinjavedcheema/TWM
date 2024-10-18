using Newtonsoft.Json;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Interfaces;

namespace Twm.Core.DataProviders.Bybit.Models.Request
{
    /// <summary>
    /// Request object used to modify a Bybit order
    /// </summary>
    [DataContract]
    public class ModifyOrderRequest: IRequest
    {

        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; }

        [JsonProperty(PropertyName = "symbol")]
        public string Symbol { get; set; }
   

        [JsonProperty(PropertyName = "orderLinkId")]
        public string OrderLinkId { get; set; }


        [JsonProperty(PropertyName = "price")]
        public string Price { get; set; }


        [JsonProperty(PropertyName = "triggerPrice")]
        public string TriggerPrice { get; set; }


        [JsonProperty(PropertyName = "qty")]
        public string Qty { get; set; }







    }
}
