using Twm.Core.DataProviders.Bybit.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.DataProviders.Bybit.Converter;

namespace Twm.Core.DataProviders.Bybit.Models.Request
{
    /// <summary>
    /// Request object used to create a new Bybit order
    /// </summary>
    [DataContract]
    public class CreateOrderRequest: IRequest
    {
        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("side")]
        [JsonConverter(typeof(StringEnumConverter))]
        public OrderSide Side { get; set; }

        [JsonProperty("orderType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public OrderType OrderType { get; set; }

        [JsonProperty("timeInForce")]
        [JsonConverter(typeof(StringEnumConverter))]
        public TimeInForce? TimeInForce { get; set; }
        
        [JsonConverter(typeof(StringDecimalConverter))]
        [JsonProperty("qty")]
        public decimal Qty { get; set; }

        [JsonProperty("price")]
        [JsonConverter(typeof(StringDecimalConverter))]
        public decimal? Price { get; set; }

        [JsonProperty("orderLinkId")]
        public string OrderLinkId { get; set; }


        [JsonProperty("marketUnit")]
        public string MarketUnit { get; set; }

        [JsonProperty("orderFilter")]
        public string OrderFilter { get; set; }


        [JsonProperty("icebergQty")]
        [JsonConverter(typeof(StringDecimalConverter))]
        public decimal? IcebergQuantity { get; set; }

        [JsonProperty("triggerDirection")]
        public int TriggerDirection { get; set; }

        [JsonProperty("triggerPrice")]
        [JsonConverter(typeof(StringDecimalConverter))]
        public decimal? TriggerPrice { get; set; }




    }
}
