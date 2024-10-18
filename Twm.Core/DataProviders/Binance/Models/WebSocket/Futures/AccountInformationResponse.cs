using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Binance.Converter;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Binance.Models.WebSocket.Futures
{
    [DataContract]
    public class AccountInformationResponse
    {
        [JsonProperty(PropertyName = "feeTier")]
        public decimal FeeTier { get; set; }

        [JsonProperty(PropertyName = "canTrade")]
        public bool CanTrade { get; set; }

        [JsonProperty(PropertyName = "canDeposit")]
        public bool CanDeposit { get; set; }

        [JsonProperty(PropertyName = "canWithdraw")]
        public bool CanWithdraw { get; set; }

        [JsonProperty(PropertyName = "updateTime")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime UpdateTime { get; set; }

        [JsonProperty(PropertyName = "multiAssetsMargin")]
        public bool MultiAssetsMargin { get; set; }


        [JsonProperty(PropertyName = "totalInitialMargin")]
        public decimal TotalInitialMargin { get; set; }

        [JsonProperty(PropertyName = "totalMaintMargin")]
        public decimal TotalMaintMargin { get; set; }


        [JsonProperty(PropertyName = "totalWalletBalance")]
        public decimal TotalWalletBalance { get; set; }

        [JsonProperty(PropertyName = "totalUnrealizedProfit")]
        public decimal TotalUnrealizedProfit { get; set; }


        [JsonProperty(PropertyName = "totalMarginBalance")]
        public decimal TotalMarginBalance { get; set; }

        [JsonProperty(PropertyName = "totalPositionInitialMargin")]
        public decimal TotalPositionInitialMargin { get; set; }

        [JsonProperty(PropertyName = "totalOpenOrderInitialMargin")]
        public decimal TotalOpenOrderInitialMargin { get; set; }

        [JsonProperty(PropertyName = "totalCrossWalletBalance")]
        public decimal TotalCrossWalletBalance { get; set; }

        [JsonProperty(PropertyName = "totalCrossUnPnl")]
        public decimal TotalCrossUnPnl { get; set; }


        [JsonProperty(PropertyName = "availableBalance")]
        public decimal AvailableBalance { get; set; }

        [JsonProperty(PropertyName = "maxWithdrawAmount")]
        public decimal MaxWithdrawAmount { get; set; }





        [JsonProperty(PropertyName = "assets")]
        public List<AssetInformation> Assets { get; set; }


        [JsonProperty(PropertyName = "positions")]
        public List<PositionInformation> Positions { get; set; }


    }
}