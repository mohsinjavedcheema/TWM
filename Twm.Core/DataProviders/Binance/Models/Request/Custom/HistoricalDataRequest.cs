using System;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Binance.Enums;
using Twm.Core.DataProviders.Interfaces;



namespace Twm.Core.DataProviders.Binance.Models.Request.Custom
{
    [DataContract]
    public class HistoricalDataRequest : IRequest
    {
        [DataMember(Order = 1, Name = "interval")]
        public KlineInterval interval { get; set; }

        [DataMember(Order = 2, Name = "symbol")]
        public string Symbol { get; set; }

        [DataMember(Order = 3, Name = "start")]
        public DateTime StartDate { get; set; }

        [DataMember(Order = 4, Name = "stop")]
        public DateTime EndDate { get; set; }

        [DataMember(Order = 5, Name = "market_type")]
        public MarketType MarketType { get; set; }

    }
}
