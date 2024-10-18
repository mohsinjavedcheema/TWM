using System.Collections.Generic;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Converter;
using Twm.Core.DataProviders.Bybit.Models.Response.Interfaces;
using Twm.Core.DataProviders.Interfaces;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    [DataContract]
    public class OrderBookResponse : IResponse
    {
        [DataMember(Order = 1)]
        public long LastUpdateId { get; set; }

        [DataMember(Order = 2)]
        [JsonConverter(typeof(TraderPriceConverter))]
        public List<TradeResponse> Bids { get; set; }

        [DataMember(Order = 3)]
        [JsonConverter(typeof(TraderPriceConverter))]
        public List<TradeResponse> Asks { get; set; }
    }
}