using Twm.Core.DataProviders.Bybit.Enums;
using Twm.Core.DataProviders.Interfaces;
using System;
using System.Runtime.Serialization;

namespace Twm.Core.DataProviders.Bybit.Models.Request.Custom
{
    [DataContract]
    public class LiveDataRequest : IRequest
    {
        [DataMember(Order = 1, Name = "symbol")]
        public string Symbol { get; set; }

        [DataMember(Order = 2, Name = "market_type")]
        public MarketType MarketType { get; set; }


        [DataMember(Order = 2, Name = "kline_interval")]
        public KlineInterval KlineInterval { get; set; }






    }
}
