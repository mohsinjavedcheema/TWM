using System.Runtime.Serialization;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.Market;

namespace Twm.Core.DataProviders.Common.Request
{
    [DataContract]
    public class SubmitOrderRequest : IRequest
    {
        [DataMember(Order = 1, Name = "order")]
        public Order Order { get; set; }

        /*[DataMember(Order = 2, Name = "symbol")]
        public string Symbol { get; set; }*/

        
    }
}