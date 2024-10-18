using System.Runtime.Serialization;
using Twm.Core.DataProviders.Interfaces;

namespace Twm.Core.DataProviders.Binance.Models.Request
{
    /// <summary>
    /// Request object used to retrieve Binance orders
    /// </summary>
    [DataContract]
    public class CurrentOpenOrdersRequest : IRequest
    {
        [DataMember(Order = 1)]
        public string Symbol { get; set; }
    }
}