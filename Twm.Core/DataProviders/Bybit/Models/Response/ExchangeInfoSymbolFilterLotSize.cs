using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Twm.Core.DataProviders.Bybit.Converter;
using Twm.Core.DataProviders.Bybit.Models.Response.Interfaces;

namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    [DataContract]
    public class ExchangeInfoSymbolFilterLotSize : ExchangeInfoSymbolFilter
    {
        [DataMember(Order = 1)]
        public Decimal MinQty { get; set; }

        [DataMember(Order = 2)]
        public Decimal MaxQty { get; set; }

        [DataMember(Order = 3)]
        public Decimal StepSize { get; set; }
    }
}
