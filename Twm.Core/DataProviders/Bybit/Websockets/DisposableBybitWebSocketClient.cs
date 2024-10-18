using System;
using System.Collections.Generic;
using WebSocketSharp;

namespace Twm.Core.DataProviders.Bybit.Websockets
{
    /// <summary>
    /// A Disposable instance of the BybitWebSocketClient is used when wanting to open a connection to retrieve data through the WebSocket protocol. 
    /// Implements IDisposable so that cleanup is managed
    /// </summary>
    public class DisposableBybitWebSocketClient : AbstractBybitWebSocketClient, IDisposable, IBybitWebSocketClient
    {
        public DisposableBybitWebSocketClient(BybitClient binanceClient) : base(binanceClient)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            AllSockets.ForEach(ws =>
            {
                if (ws.IsAlive) ws.Close(CloseStatusCode.Normal);
            });
            AllSockets = new List<BybitWebSocket>();
            ActiveWebSockets = new Dictionary<Guid, BybitWebSocket>();
        }
    }
}