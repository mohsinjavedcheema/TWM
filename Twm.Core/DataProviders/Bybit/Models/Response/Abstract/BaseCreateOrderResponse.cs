using System;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Converter;
using Twm.Core.DataProviders.Bybit.Models.Response.Interfaces;
using Twm.Core.DataProviders.Interfaces;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Bybit.Models.Response.Abstract
{
    [DataContract]
    public abstract class BaseCreateOrderResponse : IResponse
    {
        [DataMember(Order = 1)]
        public string Symbol { get; set; }

        [DataMember(Order = 2)]
        public long OrderId { get; set; }

        [DataMember(Order = 3)]
        public string ClientOrderId { get; set; }

        [DataMember(Order = 4)]
        [JsonProperty("transactTime")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime TransactionTime { get; set; }
    }
}
