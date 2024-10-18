using System.Runtime.Serialization;
using Twm.Core.DataProviders.Interfaces;

namespace Twm.Core.DataProviders.Bybit.Models.Request
{
    /// <summary>
    /// Request object used to retrieve all Bybit orders
    /// </summary>
    [DataContract]
    public class AllOrdersRequest : IRequest
    {
        [DataMember(Order = 1)]
        public string Symbol { get; set; }

        [DataMember(Order = 2)]
        public long? OrderId { get; set; }

        [DataMember(Order = 3)]
        public int? Limit { get; set; }
    }
}