using Newtonsoft.Json;
using Twm.Core.DataProviders.Bybit.Converter;


namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    public class AssetResult
    {
        
        [JsonProperty(PropertyName = "coin")]
        public string Coin { get; set; }

        [JsonProperty(PropertyName = "transferBalance")]
        [JsonConverter(typeof(StringDecimalConverter))]
        public decimal? TransferBalance { get; set; }

        [JsonProperty(PropertyName = "walletBalance")]
        [JsonConverter(typeof(StringDecimalConverter))]
        public decimal? WalletBalance { get; set; }

        [JsonProperty(PropertyName = "bonus")]        
        public string Bonus { get; set; }

    }
}
