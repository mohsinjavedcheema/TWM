﻿using Twm.Core.DataProviders.Binance.Models.WebSocket;

namespace Twm.Core.DataProviders.Binance.Websockets
{
    public class UserDataWebSocketMessages
    {
        public BinanceWebSocketMessageHandler<BinanceAccountUpdateData> AccountUpdateMessageHandler { get; set; }
        public BinanceWebSocketMessageHandler<BinanceTradeOrderData> OrderUpdateMessageHandler { get; set; }
        public BinanceWebSocketMessageHandler<BinanceTradeOrderData> TradeUpdateMessageHandler { get; set; }
    }
}