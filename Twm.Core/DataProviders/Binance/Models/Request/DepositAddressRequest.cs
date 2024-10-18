using System.Runtime.Serialization;
using Twm.Core.DataProviders.Interfaces;

namespace Twm.Core.DataProviders.Binance.Models.Request
{
    [DataContract]
    public class DepositAddressRequest: IRequest
    {
        [DataMember(Order = 1)]
        public string Asset { get; set; }
    }
}