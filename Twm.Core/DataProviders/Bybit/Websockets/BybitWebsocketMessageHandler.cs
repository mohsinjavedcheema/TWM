using System.Collections.Generic;
using System;
using Twm.Core.DataProviders.Bybit.Models.WebSocket.Interfaces;
using Twm.Chart.Interfaces;

namespace Twm.Core.DataProviders.Bybit.Websockets
{
    public delegate void BybitWebSocketMessageHandler<in T>(T data) where T: IWebSocketResponse;

}