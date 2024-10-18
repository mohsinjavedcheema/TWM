using Twm.Core.DataProviders.Binance.Models.WebSocket;

namespace Twm.Core.DataProviders.Binance.Websockets
{
    public class UserDataSpotWebSocketMessages
    {
        public BinanceWebSocketMessageHandler<Models.WebSocket.BinanceAccountUpdateData> AccountUpdateMessageHandler { get; set; }
        public BinanceWebSocketMessageHandler<Models.WebSocket.BinanceTradeOrderData> OrderUpdateMessageHandler { get; set; }
        public BinanceWebSocketMessageHandler<Models.WebSocket.BinanceTradeOrderData> TradeUpdateMessageHandler { get; set; }
    }
}