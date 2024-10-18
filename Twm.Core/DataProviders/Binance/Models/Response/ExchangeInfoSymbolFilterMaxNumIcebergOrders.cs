using System.Runtime.Serialization;

namespace Twm.Core.DataProviders.Binance.Models.Response
{
    [DataContract]
    public class ExchangeInfoSymbolFilterMaxNumIcebergOrders : ExchangeInfoSymbolFilter
    {
        [DataMember(Order = 1)]
        public int MaxNumIcebergOrders { get; set; }
    }
}