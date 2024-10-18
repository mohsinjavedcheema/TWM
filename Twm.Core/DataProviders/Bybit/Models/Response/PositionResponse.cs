using System;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Converter;
using Twm.Core.DataProviders.Bybit.Enums;
using Twm.Core.DataProviders.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    /// <summary>
    /// Response object received when querying a Bybit position
    /// </summary>
    [DataContract]
    public class PositionResponse: IResponse
    {
        [JsonProperty(PropertyName = "symbol")]
        public string Symbol { get; set; }

        [JsonProperty(PropertyName = "entryPrice")]
        public decimal EntryPrice { get; set; }

        [JsonProperty(PropertyName = "marginType")]
        public string MarginType { get; set; }

        [JsonProperty(PropertyName = "isAutoAddMargin")]
        public bool IsAutoAddMargin { get; set; }

        [JsonProperty(PropertyName = "isolatedMargin")]
        public decimal IsolatedMargin { get; set; }

        [JsonProperty(PropertyName = "leverage")]
        public decimal Leverage { get; set; }

        [JsonProperty(PropertyName = "liquidationPrice")]
        public decimal LiquidationPrice { get; set; }

        [JsonProperty(PropertyName = "markPrice")]
        public decimal MarkPrice { get; set; }

        [JsonProperty(PropertyName = "maxNotionalValue")]
        public decimal MaxNotionalValue { get; set; }

        [JsonProperty(PropertyName = "positionAmt")]
        public decimal PositionAmt { get; set; }

        [JsonProperty(PropertyName = "notional")]
        public decimal Notional { get; set; }

        [JsonProperty(PropertyName = "isolatedWallet")]
        public decimal IsolatedWallet { get; set; }

        [JsonProperty(PropertyName = "unRealizedProfit")]
        public decimal UnRealizedProfit { get; set; }

        [JsonProperty(PropertyName = "positionSide")]
        public string PositionSide { get; set; }

        [JsonProperty(PropertyName = "updateTime")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime UpdateTime { get; set; }

        
    }
}