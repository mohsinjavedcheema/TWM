using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Twm.Core.DataProviders.Bybit.Converter;
using Twm.Core.DataProviders.Bybit.Models.Response.Interfaces;

namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    [DataContract]
    public class ExchangeInfoResponse : DataProviders.Interfaces.IResponse
    {
        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        [JsonProperty("serverTime")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime ServerTime { get; set; }

        [JsonProperty("rateLimits")]
        public List<ExchangeInfoRateLimit> RateLimits { get; set; }

        // ExchangeFilters, array of unknown type

        [JsonProperty("symbols")]
        public List<ExchangeInfoSymbol> Symbols { get; set; }
    }
}
