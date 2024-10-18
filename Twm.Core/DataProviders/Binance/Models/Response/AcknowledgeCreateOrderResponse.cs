using System.Runtime.Serialization;
using Twm.Core.DataProviders.Binance.Models.Response.Abstract;

namespace Twm.Core.DataProviders.Binance.Models.Response
{
    /// <summary>
    /// Acknowledge Response following a call to the Create Order endpoint
    /// </summary>
    [DataContract]
    public class AcknowledgeCreateOrderResponse : BaseCreateOrderResponse
    {
    }
}