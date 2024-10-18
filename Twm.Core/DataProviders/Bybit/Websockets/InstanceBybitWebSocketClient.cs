namespace Twm.Core.DataProviders.Bybit.Websockets
{
    /// <summary>
    /// Used for manual management of WebSockets per instance. Only use this if you want to manually manage, open, and close your websockets.
    /// </summary>
    public class InstanceBybitWebSocketClient : AbstractBybitWebSocketClient, IBybitWebSocketClient
    {
        public InstanceBybitWebSocketClient(BybitClient binanceClient) :
            base(binanceClient)
        {
        }
    }
}