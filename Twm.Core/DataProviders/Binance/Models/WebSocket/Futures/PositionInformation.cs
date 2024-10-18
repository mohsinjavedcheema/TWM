using System;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Binance.Converter;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Binance.Models.WebSocket.Futures
{
    [DataContract]
    public class PositionInformation
    {
        [JsonProperty(PropertyName = "symbol")]
        public string Symbol { get; set; }

        [JsonProperty(PropertyName = "initialMargin")]
        public decimal InitialMargin { get; set; }

        [JsonProperty(PropertyName = "maintMargin")]
        public decimal MaintMargin { get; set; }

        [JsonProperty(PropertyName = "unrealizedProfit")]
        public decimal UnrealizedProfit { get; set; }

        [JsonProperty(PropertyName = "positionInitialMargin")]
        public decimal PositionInitialMargin { get; set; }

        [JsonProperty(PropertyName = "openOrderInitialMargin")]
        public decimal OpenOrderInitialMargin { get; set; }

        [JsonProperty(PropertyName = "leverage")]
        public decimal Leverage { get; set; }

        [JsonProperty(PropertyName = "entryPrice")]
        public decimal EntryPrice { get; set; }

        [JsonProperty(PropertyName = "maxNotional")]
        public decimal MaxNotional { get; set; }

        [JsonProperty(PropertyName = "bidNotional")]
        public decimal BbidNotional { get; set; }

        [JsonProperty(PropertyName = "askNotional")]
        public decimal AskNotional { get; set; }

        [JsonProperty(PropertyName = "positionSide")]
        public string PositionSide { get; set; }

        [JsonProperty(PropertyName = "positionAmt")]
        public decimal PositionAmt { get; set; }

        [JsonProperty(PropertyName = "updateTime")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime UpdateTime { get; set; }
    }
}