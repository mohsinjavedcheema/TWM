using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Twm.Core.DataProviders.Bybit.Converter;
using Twm.Core.DataProviders.Bybit.Models.Response.Interfaces;

namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    [DataContract]
    public class ExchangeInfoSymbolFilterPrice : ExchangeInfoSymbolFilter
    {
        [DataMember(Order = 1)]
        public Decimal MinPrice { get; set; }

        [DataMember(Order = 2)]
        public Decimal MaxPrice { get; set; }

        [DataMember(Order = 3)]
        public Decimal TickSize { get; set; }
    }
}
