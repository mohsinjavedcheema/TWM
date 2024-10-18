using System;
using System.Runtime.Serialization;

namespace Twm.Core.DataProviders.Binance.Models.Response
{
    [DataContract]
    public class ExchangeInfoSymbolFilterMaxPosition : ExchangeInfoSymbolFilter
    {
        [DataMember(Order = 1)]
        public Decimal MaxPosition { get; set; }
    }
}