using System;
using System.Runtime.Serialization;

namespace Twm.Core.DataProviders.Binance.Models.Response
{
    [DataContract]
    public class ExchangeInfoSymbolFilterMaxNumAlgoOrders : ExchangeInfoSymbolFilter
    {
        [DataMember(Order = 1)]
        public Decimal MaxNumAlgoOrders { get; set; }
    }
}
