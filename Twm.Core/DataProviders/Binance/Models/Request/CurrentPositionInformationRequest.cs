using System.Runtime.Serialization;
using Twm.Core.DataProviders.Interfaces;

namespace Twm.Core.DataProviders.Binance.Models.Request
{
    /// <summary>
    /// Request object used to retrieve all Binance positions
    /// </summary>
    [DataContract]
    public class CurrentPositionInformationRequest : IRequest
    {
        [DataMember(Order = 1)]
        public string Symbol { get; set; }
    }
}