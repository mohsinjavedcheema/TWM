using System.Runtime.Serialization;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.Market;

namespace Twm.Core.DataProviders.Common.Request
{
    [DataContract]
    public class ChangeOrderRequest : IRequest
    {
        [DataMember(Order = 1, Name = "order")]
        public Order Order { get; set; }

        
    }
}