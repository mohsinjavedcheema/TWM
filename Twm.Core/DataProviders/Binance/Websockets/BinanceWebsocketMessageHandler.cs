using Twm.Core.DataProviders.Binance.Models.WebSocket.Interfaces;

namespace Twm.Core.DataProviders.Binance.Websockets
{
    public delegate void BinanceWebSocketMessageHandler<in T>(T data) where T: IWebSocketResponse;
}