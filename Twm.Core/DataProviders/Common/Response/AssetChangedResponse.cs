using System.Runtime.Serialization;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.Market;

namespace Twm.Core.DataProviders.Common.Response
{
    public class AssetChangedResponse : IResponse
    {
        [DataMember(Order = 1, Name = "asset")]
        public Asset Asset { get; set; }

    }
}