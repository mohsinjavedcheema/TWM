using System.Runtime.Serialization;
using Newtonsoft.Json;
using Twm.Core.DataProviders.Bybit.Enums;
using Newtonsoft.Json.Converters;

namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    [DataContract]
    public class ExchangeInfoSymbolFilter
    {
        [JsonProperty("minPrice")]
        public double MinPrice { get; set; }

        [JsonProperty("maxPrice")]
        public double MaxPrice { get; set; }

        [JsonProperty("filterType")]
        public string FilterType { get; set; }

        [JsonProperty("tickSize")]
        public double TickSize { get; set; }

        [JsonProperty("stepSize")]
        public double StepSize { get; set; }

        [JsonProperty("maxQty")]
        public double MaxQty { get; set; }

        [JsonProperty("minQty")]
        public double MinQty { get; set; }

        [JsonProperty("limit")]
        public double Limit { get; set; }

        [JsonProperty("notional")]
        public double Notional { get; set; }

        [JsonProperty("multiplierDown")]
        public double MultiplierDown { get; set; }

        [JsonProperty("multiplierUp")]
        public double MultiplierUp { get; set; }

        [JsonProperty("multiplierDecimal")]
        public double MultiplierDecimal { get; set; }
    }
}
