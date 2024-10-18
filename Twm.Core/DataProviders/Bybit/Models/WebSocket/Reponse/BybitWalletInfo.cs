using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Converter;

namespace Twm.Core.DataProviders.Bybit.Models.WebSocket
{
    /// <summary>
    /// Response following a call to the position info
    /// </summary>
    [DataContract]
    public class BybitWalletInfo
    {

        [JsonProperty(PropertyName = "totalEquity")]
        [JsonConverter(typeof(StringDecimalConverter))]
        public decimal? TotalEquity { get; set; }

        [JsonProperty(PropertyName = "totalPerpUPL")]
        [JsonConverter(typeof(StringDecimalConverter))]
        public decimal? TotalPerpUPL { get; set; }


        [JsonProperty(PropertyName = "coin")]
        public List<BybitCoinInfo> Coins { get; set; }



    }
}
