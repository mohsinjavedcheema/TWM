using System.Runtime.Serialization;
using Newtonsoft.Json;


namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    [DataContract]
    public class TickerInfo
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("lastPrice")]
        public double LastPrice { get; set; }

        [JsonProperty("volume")]
        public double Volume { get; set; }
              
    }
}
