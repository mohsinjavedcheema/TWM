using System;

namespace Twm.Core.DataProviders.Binance.Models.WebSocket.Interfaces
{
    public interface IWebSocketResponse
    {
        string EventType { get; set; }

        DateTime EventTime { get; set; }
    }
}