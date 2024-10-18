using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Twm.Core.DataProviders.Binance.Converter;


namespace Twm.Core.DataProviders.Binance.Models.Response
{
    [DataContract]
    public class ExchangeInfoSymbol
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("pair")]
        public string Pair { get; set; }

        [JsonProperty("contractType")]
        public string ContractType { get; set; }

        [JsonProperty("deliveryDate")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime DeliveryDate { get; set; }

        [JsonProperty("onboardDate")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime OnboardDate { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("maintMarginPercent")]
        public double MaintMarginPercent { get; set; }

        [JsonProperty("requiredMarginPercent")]
        public double RequiredMarginPercent { get; set; }

        [JsonProperty("baseAsset")]
        public string BaseAsset { get; set; }

        [JsonProperty("quoteAsset")]
        public string QuoteAsset { get; set; }

        [JsonProperty("marginAsset")]
        public string MarginAsset { get; set; }

        [JsonProperty("pricePrecision")]
        public double PricePrecision { get; set; }

        [JsonProperty("quantityPrecision")]
        public double QuantityPrecision { get; set; }

        [JsonProperty("baseAssetPrecision")]
        public int BaseAssetPrecision { get; set; }

        [JsonProperty("quotePrecision")]
        public int QuotePrecision { get; set; }

        [JsonProperty("underlyingType")]
        public string UnderlyingType { get; set; }

        [JsonProperty("underlyingSubType")]
        public string[] UnderlyingSubType { get; set; }

        [JsonProperty("settlePlan")]
        public double SettlePlan { get; set; }

        [JsonProperty("triggerProtect")]
        public double TriggerProtect { get; set; }

        [JsonProperty("liquidationFee")]
        public double LiquidationFee { get; set; }

        [JsonProperty("marketTakeBound")]
        public double MarketTakeBound { get; set; }

        [JsonProperty("maxMoveOrderLimit")]
        public double MaxMoveOrderLimit { get; set; }

        [JsonProperty("filters")]        
        public List<ExchangeInfoSymbolFilter> Filters { get; set; }
       
    }
}
