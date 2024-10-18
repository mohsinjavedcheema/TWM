using Twm.Core.DataProviders.Bybit.Models.WebSocket.Reponse;
using Twm.Core.DataProviders.Bybit.Websockets;

namespace Twm.Core.DataProviders.Bybit.Models.WebSocket
{
    public class UserDataWebSocketMessages
    {
        public BybitWebSocketMessageHandler<BybitOrderBookResponse> DepthMessageHandler { get; set; }
        

    }
}
