using System;
using System.Threading.Tasks;
using Twm.Core.DataProviders.Bybit.Enums;
using Twm.Core.DataProviders.Bybit.Models.WebSocket;
using Twm.Core.Helpers;

namespace Twm.Core.DataProviders.Bybit.Websockets
{
    public interface IBybitWebSocketClient
    {
        /// <summary>
        /// Connect to the Kline WebSocket
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <param name="messageEventHandler"></param>
        /// <returns></returns>
        Guid ConnectToKlineWebSocket(BybitWebSocketMessageHandler<BybitKlineResponse> messageEventHandler, MarketType mt, string symbol, bool isTestMode, KlineInterval interval);


        /// <summary>
        /// Connect to the Depth WebSocket
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="messageEventHandler"></param>
        /// <returns></returns>
        Guid ConnectToDepthWebSocket(UserDataWebSocketMessages messageEventHandler, MarketType mt, string symbol, bool isTestMode, int levels);

        /// <summary>
        /// Connect to the Trades WebSocket
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="messageEventHandler"></param>
        /// <returns></returns>
        Guid ConnectToTradesWebSocket(UserDataWebSocketMessages messageEventHandler, MarketType mt, string symbol, bool isTestMode);

        /// <summary>
        /// Connect to the UserData WebSocket
        /// </summary>
        /// <param name="userDataMessageHandlers"></param>
        /// <param name="userDataFuturesMessageHandlers"></param>
        /// <returns></returns>
        Task<Guid> ConnectToUserDataWebSocket(UserDataWebSocketMessages messageEventHandler,  bool isTestMode);

        /// <summary>
        /// Close a specific WebSocket instance using the Guid provided on creation
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fromError"></param>
        void CloseWebSocketInstance(Guid id, bool fromError = false);
    }
}