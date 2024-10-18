using System.Runtime.Serialization;

namespace Twm.Core.DataProviders.Binance.Models.Response
{
    /// <summary>
    /// User Data Stream response
    /// </summary>
    [DataContract]
    public class UserDataStreamResponse
    {
        [DataMember(Order = 1)]
        public string ListenKey { get; set; }
    }
}
