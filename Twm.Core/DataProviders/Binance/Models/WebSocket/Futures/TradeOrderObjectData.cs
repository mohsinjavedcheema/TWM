using Twm.Core.DataProviders.Binance.Enums;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Binance.Converter;
using System;

namespace Twm.Core.DataProviders.Binance.Models.WebSocket.Futures
{
    [DataContract]
    public class TradeOrderObjectData
    {

        [JsonProperty(PropertyName = "s")]
        public string Symbol { get; set; }

        [JsonProperty(PropertyName = "c")]
        public string NewClientOrderId { get; set; }

        [JsonProperty(PropertyName = "S")]
        [JsonConverter(typeof(StringEnumConverter))]
        public OrderSide Side { get; set; }

        [JsonProperty(PropertyName = "o")]
        [JsonConverter(typeof(StringEnumConverter))]
        public OrderType Type { get; set; }

        [JsonProperty(PropertyName = "f")]
        [JsonConverter(typeof(StringEnumConverter))]
        public TimeInForce TimeInForce { get; set; }

        [JsonProperty(PropertyName = "q")]
        public decimal Quantity { get; set; }

        [JsonProperty(PropertyName = "p")]
        public decimal Price { get; set; }

        [JsonProperty(PropertyName = "ap")]
        public decimal AveragePrice { get; set; }

        [JsonProperty(PropertyName = "sp")]
        public decimal StopPrice { get; set; }

        [JsonProperty(PropertyName = "x")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ExecutionType ExecutionType { get; set; }

        [JsonProperty(PropertyName = "X")]
        [JsonConverter(typeof(StringEnumConverter))]
        public OrderStatus OrderStatus { get; set; }

        [JsonProperty(PropertyName = "i")]
        public long OrderId { get; set; }

        [JsonProperty(PropertyName = "l")]
        public decimal QuantityOfLastFilledTrade { get; set; }

        [JsonProperty(PropertyName = "z")]
        public decimal AccumulatedQuantityOfFilledTradesThisOrder { get; set; }

        [JsonProperty(PropertyName = "L")]
        public decimal PriceOfLastFilledTrade { get; set; }

        /// <summary>
        /// Asset on which commission taken
        /// </summary>
        [JsonProperty(PropertyName = "N")]
        public string AssetCommissionTakenFrom { get; set; }

        [JsonProperty(PropertyName = "n")]
        public decimal Commission { get; set; }

        /// <summary>
        /// Represents Order or Trade time
        /// </summary>
        [JsonProperty(PropertyName = "T")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime OrderTradeTime { get; set; }


        [JsonProperty(PropertyName = "t")]
        public long TradeId { get; set; }

        [JsonProperty(PropertyName = "b")]
        public decimal BidsNotional { get; set; }

        [JsonProperty(PropertyName = "a")]
        public decimal AskssNotional { get; set; }

        [JsonProperty(PropertyName = "m")]
        public bool IsBuyerMaker { get; set; }

        [JsonProperty(PropertyName = "R")]
        public bool IsReduceOnly { get; set; }

        [JsonProperty(PropertyName = "ps")]
        public string PositionSide { get; set; }

        [JsonProperty(PropertyName = "rp")]
        public decimal RealizedProfit { get; set; }


    }

        

        

}
