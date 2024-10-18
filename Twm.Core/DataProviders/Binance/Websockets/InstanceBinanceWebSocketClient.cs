namespace Twm.Core.DataProviders.Binance.Websockets
{
    /// <summary>
    /// Used for manual management of WebSockets per instance. Only use this if you want to manually manage, open, and close your websockets.
    /// </summary>
    public class InstanceBinanceWebSocketClient : AbstractBinanceWebSocketClient, IBinanceWebSocketClient
    {
        public InstanceBinanceWebSocketClient(BinanceClient binanceClient) :
            base(binanceClient)
        {
        }
    }
}