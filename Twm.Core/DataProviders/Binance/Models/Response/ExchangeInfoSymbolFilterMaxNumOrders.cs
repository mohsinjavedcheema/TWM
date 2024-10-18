using System.Runtime.Serialization;

namespace Twm.Core.DataProviders.Binance.Models.Response
{
    [DataContract]
    public class ExchangeInfoSymbolFilterMaxNumOrders : ExchangeInfoSymbolFilter
    {
        [DataMember(Order = 1)]
        public int Limit { get; set; }
    }
}