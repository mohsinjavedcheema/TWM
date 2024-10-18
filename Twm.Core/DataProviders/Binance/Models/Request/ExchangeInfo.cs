using System;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Binance.Converter;
using Twm.Core.DataProviders.Interfaces;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Binance.Models.Request
{
    /// <summary>
    /// Request object used to retrieve exchange information
    /// </summary>
    [DataContract]
    public class ExchangeInfo : IRequest
    {
    }
}
