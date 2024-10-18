using Twm.Core.DataProviders.Bybit.Models.WebSocket;
using Twm.Core.DataProviders.Bybit.Models.WebSocket.Reponse;
using Twm.Core.DataProviders.Bybit.Models.WebSocket.Response;

namespace Twm.Core.DataProviders.Bybit.Websockets
{
    public class UserDataWebSocketMessages
    {
        public BybitWebSocketMessageHandler<BybitOrderResponse> OrderUpdateMessageHandler { get; set; }

        public BybitWebSocketMessageHandler<BybitPositionResponse> PositionMessageHandler { get; set; }

        public BybitWebSocketMessageHandler<BybitKlineResponse> KlineMessageHandler { get; set; }

        public BybitWebSocketMessageHandler<BybitWalletResponse> WalletMessageHandler { get; set; }

        public BybitWebSocketMessageHandler<BybitOrderBookResponse> DepthMessageHandler { get; set; }


    }
}