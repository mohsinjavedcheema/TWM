using System;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Binance.Converter;
using Twm.Core.DataProviders.Binance.Enums;
using Twm.Core.DataProviders.Binance.Models.Response.Interfaces;
using Twm.Core.DataProviders.Interfaces;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Binance.Models.Response
{
    [DataContract]
    public class WithdrawListItem : IResponse
    {
        [DataMember(Order = 1)]
        public string Id { get; set; }

        [DataMember(Order = 2)]
        public decimal Amount { get; set; }

        [DataMember(Order = 3)]
        public string Address { get; set; }

        [DataMember(Order = 4)]
        public string AddressTag { get; set; }

        [DataMember(Order = 5)]
        [JsonProperty(PropertyName = "txId")]
        public string TransactionId { get; set; }

        [DataMember(Order = 6)]
        public string Asset { get; set; }

        [DataMember(Order = 7)]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime ApplyTime { get; set; }

        [DataMember(Order = 8)]
        public WithdrawHistoryStatus Status { get; set; }
    }
}