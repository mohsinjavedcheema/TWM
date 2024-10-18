using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;


namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    [DataContract]
    public class ExchangeInfoRateLimit
    {
        [JsonProperty("rateLimitType")]
        public string RateLimitType { get; set; }

        [JsonProperty("interval")]
        public string Interval { get; set; }

        [JsonProperty("limit")]
        public int Limit { get; set; }
    }
}
