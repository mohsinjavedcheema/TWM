using System;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Binance.Converter;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Binance.Models.WebSocket.Futures
{
    [DataContract]
    public class AssetInformation
    {
        [JsonProperty(PropertyName = "asset")]
        public string Asset { get; set; }

        [JsonProperty(PropertyName = "walletBalance")]
        public decimal WalletBalance { get; set; }
        
        [JsonProperty(PropertyName = "unrealizedProfit")]
        public decimal UnrealizedProfit { get; set; }
        
        [JsonProperty(PropertyName = "marginBalance")]
        public decimal MarginBalance { get; set; }

        [JsonProperty(PropertyName = "maintMargin")]
        public decimal MaintMargin { get; set; }

        [JsonProperty(PropertyName = "initialMargin")]
        public decimal InitialMargin { get; set; }

        [JsonProperty(PropertyName = "positionInitialMargin")]
        public decimal PositionInitialMargin { get; set; }

        [JsonProperty(PropertyName = "openOrderInitialMargin")]
        public decimal OpenOrderInitialMargin { get; set; }

        [JsonProperty(PropertyName = "crossWalletBalance")]
        public decimal CrossWalletBalance { get; set; }

        [JsonProperty(PropertyName = "crossUnPnl")]
        public decimal CrossUnPnl { get; set; }

        [JsonProperty(PropertyName = "availableBalance")]
        public decimal AvailableBalance { get; set; }

        [JsonProperty(PropertyName = "maxWithdrawAmount")]
        public decimal MaxWithdrawAmount { get; set; }

        [JsonProperty(PropertyName = "marginAvailable")]
        public bool MarginAvailable { get; set; }

        [JsonProperty(PropertyName = "updateTime")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime UpdateTime { get; set; }
    }
}