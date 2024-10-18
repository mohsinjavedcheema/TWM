using System.Runtime.Serialization;

namespace Twm.Core.DataProviders.Bybit.Models.WebSocket
{
    [DataContract]
    public class TradeDepthDelta
    {
        [DataMember(Order = 1)]
        public decimal Price { get; set; }
        [DataMember(Order = 2)]
        public decimal Quanity { get; set; }
    }
}