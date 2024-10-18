using System.Runtime.Serialization;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.Market;

namespace Twm.Core.DataProviders.Common.Response
{
    public class AccountChangedResponse : IResponse
    {
        [DataMember(Order = 1, Name = "realized")]
        public double? Realized { get; set; }

        [DataMember(Order = 2, Name = "unrealized")]
        public double? Unrealized { get; set; }

    }
}