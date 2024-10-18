using System.Runtime.Serialization;
using Twm.Core.DataProviders.Binance.Models.Response.Interfaces;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Binance.Models.WebSocket.Futures
{
    [DataContract]
    public class BalanceData
    {
        [JsonProperty(PropertyName = "a")]
        public string Asset { get; set; }

        [JsonProperty(PropertyName = "wb")]
        public decimal WalletBalabce { get; set; }

        [JsonProperty(PropertyName = "cw")]
        public decimal CrossWalletBalance { get; set; }

        [JsonProperty(PropertyName = "bc")]
        public decimal BalanceChange { get; set; }
    }
}