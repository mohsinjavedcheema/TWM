using System.Runtime.Serialization;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.Market;

namespace Twm.Core.DataProviders.Common.Response
{
    public class PositionChangedResponse : IResponse
    {
        [DataMember(Order = 1, Name = "position")]
        public Position Position { get; set; }

    }
}