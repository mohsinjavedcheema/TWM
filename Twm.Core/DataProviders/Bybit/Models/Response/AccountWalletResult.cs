using System.Runtime.Serialization;
using Twm.Core.DataProviders.Interfaces;
using System.Collections.Generic;
using Twm.Core.DataProviders.Bybit.Models.WebSocket;

namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    /// <summary>
    /// Response object received when querying a Bybit assets
    /// </summary>
    [DataContract]
    public class AccountWalletResult: IResponse
    {
        [DataMember(Order = 1, Name = "list")]
        public List<BybitWalletInfo> List { get; set; }
    }
}