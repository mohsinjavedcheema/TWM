using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Enums;
using Twm.Core.DataProviders.Bybit.Models.Response.Interfaces;
using Twm.Core.DataProviders.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    [DataContract]
    public class SystemStatusResponse : IResponse
    {
        [DataMember(Order = 1)]
        [JsonConverter(typeof(StringEnumConverter))]
        public SystemStatus Status { get; set; }

        [DataMember(Order = 2)]
        [JsonProperty(PropertyName = "msg")]
        public string Message { get; set; }
    }
}