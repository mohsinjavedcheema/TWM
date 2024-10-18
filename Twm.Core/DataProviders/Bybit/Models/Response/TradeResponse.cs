using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Models.Response.Interfaces;
using Twm.Core.DataProviders.Interfaces;

namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    /// <summary>
    /// Trade response, providing price and quantity information
    /// </summary>
    [DataContract]
    public class TradeResponse: IResponse
    {
        [DataMember(Order = 1)]
        public decimal Price { get; set; }

        [DataMember(Order = 2)]
        public decimal Quantity { get; set; }
    }
}