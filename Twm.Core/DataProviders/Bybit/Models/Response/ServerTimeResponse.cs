using System;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Converter;
using Twm.Core.DataProviders.Bybit.Models.Response.Interfaces;
using Twm.Core.DataProviders.Interfaces;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    /// <summary>
    /// The current server time
    /// </summary>
    [DataContract]
    public class ServerTimeResponse: IResponse
    {
        [DataMember(Order = 1)]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime ServerTime { get; set; }
    }
}