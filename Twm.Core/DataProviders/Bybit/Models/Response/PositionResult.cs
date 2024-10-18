using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using Twm.Core.DataProviders.Bybit.Converter;
using Twm.Core.DataProviders.Bybit.Enums;

namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    public class PositionResult
    {
        
        [JsonProperty(PropertyName = "positionIdx")]
        public int PositionIdx { get; set; }

        [JsonProperty(PropertyName = "riskId")]
        public int RiskId { get; set; }

        [JsonProperty(PropertyName = "riskLimitValue")]
        public string RiskLimitValue { get; set; }

        [JsonProperty(PropertyName = "symbol")]
        public string Symbol { get; set; }

        [JsonProperty(PropertyName = "side")]
        [JsonConverter(typeof(StringEnumConverter))]
        public OrderSide Side { get; set; }


        [JsonProperty(PropertyName = "avgPrice")]
        public decimal? AvgPrice { get; set; }

        [JsonProperty(PropertyName = "size")]
        public decimal Size { get; set; }

      
        [JsonProperty(PropertyName = "positionValue")]
        public decimal? PositionValue { get; set; }


        [JsonProperty(PropertyName = "positionBalance")]
        public decimal? PositionBalance { get; set; }


        [JsonProperty(PropertyName = "createdTime")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime CreatedTime { get; set; }

        [JsonProperty(PropertyName = "updatedTime")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime UpdatedTime { get; set; }



    }
}
