using System.Runtime.Serialization;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.Market;

namespace Twm.Core.DataProviders.Common.Response
{
    public class OrderStatusChangedResponse : IResponse
    {
        [DataMember(Order = 1, Name = "order")]
        public Order Order { get; set; }

    }
}