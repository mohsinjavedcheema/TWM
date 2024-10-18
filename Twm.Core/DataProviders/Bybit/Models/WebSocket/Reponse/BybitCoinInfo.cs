using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Converter;
using Twm.Core.DataProviders.Bybit.Enums;

namespace Twm.Core.DataProviders.Bybit.Models.WebSocket
{
    /// <summary>
    /// Response following a call to the position info
    /// </summary>
    [DataContract]
    public class BybitCoinInfo
    {
     
        [JsonProperty(PropertyName = "coin")]
        public string Coin { get; set; }


        [JsonProperty(PropertyName = "equity")]
        [JsonConverter(typeof(StringDecimalConverter))]
        public decimal? Equity { get; set; }


        [JsonProperty(PropertyName = "usdValue")]
        [JsonConverter(typeof(StringDecimalConverter))]
        public decimal? UsdValue { get; set; }
      

        [JsonProperty(PropertyName = "walletBalance")]
        [JsonConverter(typeof(StringDecimalConverter))]
        public decimal? WalletBalance { get; set; }





    }
}
