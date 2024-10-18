﻿using Twm.Core.DataProviders.Interfaces;
using System.Collections.Generic;
using System.Runtime.Serialization;


namespace Twm.Core.DataProviders.Binance.Models.Response
{
    [DataContract]
    public class InstrumentTickersResponse : IResponse
    {
        [DataMember(Order = 1)]
        public List<InstrumentTicker> InstrumentTickers { get; set; }
    }
}
