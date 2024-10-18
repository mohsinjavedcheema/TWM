using Twm.Core.DataProviders.Binance.Models.WebSocket;

namespace Twm.Core.DataProviders.Binance.Websockets
{
    public class UserDataFuturesWebSocketMessages
    {
        public BinanceWebSocketMessageHandler<Models.WebSocket.Futures.BinanceAccountUpdateData> AccountUpdateMessageHandler { get; set; }
        public BinanceWebSocketMessageHandler<Models.WebSocket.Futures.BinanceTradeOrderData> OrderUpdateMessageHandler { get; set; }
        public BinanceWebSocketMessageHandler<Models.WebSocket.Futures.BinanceTradeOrderData> TradeUpdateMessageHandler { get; set; }
    }


}