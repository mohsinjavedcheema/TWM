using System.Runtime.Serialization;
using Twm.Core.DataProviders.Binance.Converter;
using Newtonsoft.Json;
using System;
using Twm.Core.DataProviders.Binance.Enums;
using Twm.Core.DataProviders.Binance.Models.Response.Interfaces;
using Twm.Core.DataProviders.Interfaces;

namespace Twm.Core.DataProviders.Binance.Models.Response
{

    [DataContract]
    public class DepositListItem : IResponse
    {
        [DataMember(Order = 1)]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime InsertTime { get; set; }

        [DataMember(Order = 2)]
        public decimal Amount { get; set; }

        [DataMember(Order = 3)]
        public string Symbol { get; set; }

        [DataMember(Order = 4)]
        public string Address { get; set; }

        [DataMember(Order = 5)]
        [JsonProperty(PropertyName ="txId")]
        public string TransactionId { get; set; }

        [DataMember(Order = 6)]
        public DepositHistoryStatus Status { get; set; }
    }
}