using System.Runtime.Serialization;
using Twm.Core.DataProviders.Binance.Models.Response.Interfaces;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Binance.Models.WebSocket.Futures
{
    [DataContract]
    public class PositionData
    {
        [JsonProperty(PropertyName = "s")]
        public string Symbol { get; set; }

        [JsonProperty(PropertyName = "pa")]
        public decimal PositionAmount { get; set; }

        [JsonProperty(PropertyName = "ep")]
        public decimal EntryPrice { get; set; }

        [JsonProperty(PropertyName = "cr")]
        public decimal AccumulatedRealized { get; set; }

        [JsonProperty(PropertyName = "up")]
        public decimal UneealizedPNL { get; set; }

        [JsonProperty(PropertyName = "mt")]
        public string MarginType { get; set; }

        [JsonProperty(PropertyName = "iw")]
        public decimal IsolatedWallet { get; set; }

        [JsonProperty(PropertyName = "ps")]
        public string PositionSide { get; set; }
    }
}