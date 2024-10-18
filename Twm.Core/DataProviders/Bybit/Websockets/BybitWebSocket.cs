using System;
using WebSocketSharp;

namespace Twm.Core.DataProviders.Bybit.Websockets
{
    /// <summary>
    /// Built around WebSocket for future improvements. For all intents and purposes, this is the same as the WebSocket
    /// </summary>
    public class BybitWebSocket : WebSocket
    {
        public Guid Id;
        public BybitWebSocket(string url, params string[] protocols) : base(url, protocols)
        {
            Id = Guid.NewGuid();
        }
    }
}