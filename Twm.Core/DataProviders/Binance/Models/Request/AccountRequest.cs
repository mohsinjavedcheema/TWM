using System;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Binance.Converter;
using Twm.Core.DataProviders.Interfaces;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Binance.Models.Request
{
    /// <summary>
    /// Request object used to retrieve current account information
    /// </summary>
    [DataContract]
    public class AccountRequest : IRequest
    {
        [DataMember(Order = 1)]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime TimeStamp { get; set; }
    }
}