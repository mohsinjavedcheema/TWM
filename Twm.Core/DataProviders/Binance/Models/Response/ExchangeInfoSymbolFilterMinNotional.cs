using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Twm.Core.DataProviders.Binance.Converter;
using Twm.Core.DataProviders.Binance.Models.Response.Interfaces;

namespace Twm.Core.DataProviders.Binance.Models.Response
{
    [DataContract]
    public class ExchangeInfoSymbolFilterMinNotional : ExchangeInfoSymbolFilter
    {
        [DataMember(Order = 1)]
        public Decimal MinNotional { get; set; }
    }
}
