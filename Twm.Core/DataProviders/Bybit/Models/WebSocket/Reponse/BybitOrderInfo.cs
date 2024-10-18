using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Converter;
using Twm.Core.DataProviders.Bybit.Enums;

namespace Twm.Core.DataProviders.Bybit.Models.WebSocket
{
    /// <summary>
    /// Response following a call to the order info
    /// </summary>
    [DataContract]
    public class BybitOrderInfo
    {        
        [JsonProperty(PropertyName = "symbol")]
        public string Symbol { get; set; }
        
        [JsonProperty(PropertyName = "orderId")]
        public string OrderId { get; set; }
        
        [JsonProperty(PropertyName = "side")]
        [JsonConverter(typeof(StringEnumConverter))]
        public OrderSide Side  { get; set; }
        
        [JsonProperty(PropertyName = "orderType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public OrderType OrderType  { get; set; }

        [JsonProperty(PropertyName = "cancelType")]
        public string   CancelType  { get; set; }

        [JsonProperty(PropertyName = "price")]
        [JsonConverter(typeof(StringDecimalConverter))]
        public decimal? Price { get; set; }

        [JsonProperty(PropertyName = "triggerPrice")]
        [JsonConverter(typeof(StringDecimalConverter))]
        public decimal? TriggerPrice { get; set; }

        [JsonProperty(PropertyName = "qty")]
        [JsonConverter(typeof(StringDecimalConverter))]
        public decimal? Qty  { get; set; }

        [JsonProperty(PropertyName = "orderIv")]
        public string   OrderIv  { get; set; }

        [JsonProperty(PropertyName = "timeInForce")]
        public string TimeInForce { get; set; }
        
        [JsonProperty(PropertyName = "orderStatus")]
        [JsonConverter(typeof(StringEnumConverter))]
        public OrderStatus OrderStatus  { get; set; }

        [JsonProperty(PropertyName = "orderLinkId")]
        public string OrderLinkId { get; set; }

        [JsonProperty(PropertyName = "leavesQty")]
        [JsonConverter(typeof(StringDecimalConverter))]
        public decimal? LeavesQty { get; set; }

        [JsonProperty(PropertyName = "cumExecQty")]
        [JsonConverter(typeof(StringDecimalConverter))]
        public decimal? CumExecQty { get; set; }

        [JsonProperty(PropertyName = "cumExecValue")]
        [JsonConverter(typeof(StringDecimalConverter))]
        public decimal? CumExecValue { get; set; }

        [JsonProperty(PropertyName = "avgPrice")]
        [JsonConverter(typeof(StringDecimalConverter))]
        public decimal? AvgPrice { get; set; }

        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; }

        [JsonProperty(PropertyName = "createdTime")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime CreatedTime { get; set; }

        [JsonProperty(PropertyName = "updatedTime")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime UpdatedTime { get; set; }

    }
}
