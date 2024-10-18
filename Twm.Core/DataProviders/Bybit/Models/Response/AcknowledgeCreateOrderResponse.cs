using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Models.Response.Abstract;

namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    /// <summary>
    /// Acknowledge Response following a call to the Create Order endpoint
    /// </summary>
    [DataContract]
    public class AcknowledgeCreateOrderResponse : BaseCreateOrderResponse
    {
    }
}