using Newtonsoft.Json;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Interfaces;

namespace Twm.Core.DataProviders.Bybit.Models.Request
{
    /// <summary>
    /// Request object used to retrieve all Bybit positions
    /// </summary>
    [DataContract]
    public class CurrentPositionInformationRequest : IRequest
    {
        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; }

        [JsonProperty(PropertyName = "symbol")]
        public string Symbol { get; set; }

        [JsonProperty(PropertyName = "baseCoin")]
        public string BaseCoin { get; set; }


        [JsonProperty(PropertyName = "settleCoin")]
        public string SettleCoin { get; set; }
    }
}