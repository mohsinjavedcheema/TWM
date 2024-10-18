using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Twm.Core.DataProviders.Binance.Converter;
using Twm.Core.DataProviders.Binance.Models.Response.Interfaces;

namespace Twm.Core.DataProviders.Binance.Models.Response
{
   [DataContract]
    public class ExchangeInfoSymbolFilterPercentPrice : ExchangeInfoSymbolFilter
    {
        [DataMember(Order = 1)]
        public Decimal MultiplierUp { get; set; }

        [DataMember(Order = 2)]
        public Decimal MultiplierDown { get; set; }

        [DataMember(Order = 3)]
        public Decimal AvgPriceMins { get; set; }
    }
}
