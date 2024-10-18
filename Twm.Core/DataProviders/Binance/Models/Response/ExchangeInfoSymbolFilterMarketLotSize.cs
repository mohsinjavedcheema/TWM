using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Twm.Core.DataProviders.Binance.Converter;
using Twm.Core.DataProviders.Binance.Models.Response.Interfaces;

namespace Twm.Core.DataProviders.Binance.Models.Response
{
    [DataContract]
    public class ExchangeInfoSymbolFilterMarketLotSize : ExchangeInfoSymbolFilterLotSize
    {
    }
}
