using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Models.Response.Interfaces;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    public class WithdrawResponse : IConfirmationResponse
    {
        [DataMember(Order = 1)]
        [JsonProperty(PropertyName = "msg")]
        public string Message { get; set; }

        [DataMember(Order = 2)]
        public bool Success { get; set; }

        [DataMember(Order = 3)]
        public string Id { get; set; }
    }
}
