using Newtonsoft.Json;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Interfaces;

namespace Twm.Core.DataProviders.Bybit.Models.Request
{
    /// <summary>
    /// Request object used to retrieve all Bybit assets
    /// </summary>
    [DataContract]
    public class GetAssetInfoRequest : IRequest
    {
        [JsonProperty(PropertyName = "accountType")]
        public string AccountType { get; set; }

        [JsonProperty(PropertyName = "coin")]
        public string Coin { get; set; }

      
    }
}