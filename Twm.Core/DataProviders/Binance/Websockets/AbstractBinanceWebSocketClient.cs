using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Timers;
using Twm.Core.Controllers;
using Twm.Core.DataProviders.Binance.Enums;
using Twm.Core.DataProviders.Binance.Extensions;
using Twm.Core.DataProviders.Binance.Models.Response.Error;
using Twm.Core.DataProviders.Binance.Models.WebSocket;
using Twm.Core.DataProviders.Binance.Utility;
using Twm.Core.DataProviders.Interfaces;
using Newtonsoft.Json;
using WebSocketSharp;
using IWebSocketResponse = Twm.Core.DataProviders.Binance.Models.WebSocket.Interfaces.IWebSocketResponse;

namespace Twm.Core.DataProviders.Binance.Websockets
{
    /// <summary>
    /// Abstract class for creating WebSocketClients 
    /// </summary>
    public class AbstractBinanceWebSocketClient
    {
        protected SslProtocols SupportedProtocols { get; } = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;

        /// <summary> 
        /// Base WebSocket URI for Binance API
        /// </summary>
        protected string BaseWebsocketUri = "wss://stream.binance.com:9443/ws";


        //protected string BaseWebsocketUriUsdMFut = "wss://fstream.binance.com:9443/ws";
        protected string BaseWebsocketUriUsdMFut = "wss://fstream.binance.com/ws";
        protected string BaseWebsocketUriUsdMFutTest = "wss://fstream.binancefuture.com/ws";

        //protected string BaseWebsocketUriCoinMFut = "wss://dstream.binance.com:9443/ws";
        protected string BaseWebsocketUriCoinMFut = "wss://dstream.binance.com/ws";

        /// <summary>
        /// Combined WebSocket URI for Binance API
        /// </summary>
        protected string CombinedWebsocketUri = "wss://stream.binance.com:9443/stream?streams";

        /// <summary>
        /// Used for deletion on the fly
        /// </summary>
        protected Dictionary<Guid, BinanceWebSocket> ActiveWebSockets;
        protected List<BinanceWebSocket> AllSockets;
        protected readonly BinanceClient BinanceClient;

        protected const string AccountEventType = "outboundAccountInfo";
        protected const string OrderTradeEventType = "executionReport";
        public string ListenKey { get; private set; }

        private Timer _keepAliveTimer;

        private Timer _listenKeyTimer;

        private Timer _marketDataListenKeyTimer;

        public AbstractBinanceWebSocketClient(BinanceClient binanceClient)
        {
            BinanceClient = binanceClient;
            ActiveWebSockets = new Dictionary<Guid, BinanceWebSocket>();
            AllSockets = new List<BinanceWebSocket>();

        }

        // Expose the listenKey that gets returned by Binance REST request "POST /api/v1/userDataStream" to start a user data stream
        /// <summary>
        /// Connect to the UserData WebSocket
        /// </summary>
        /// <param name="userDataMessageHandlers"></param>
        /// <returns>Guid of connection.</returns>
        /// sets the Binance Listen Key (binanceListenKey)
        public async Task<Guid> ConnectToUserDataWebSocket(MarketType mt, UserDataSpotWebSocketMessages userDataMessageHandlers, UserDataFuturesWebSocketMessages userDataFuturesMessageHandlers, bool isTestMode)
        {
            Guard.AgainstNull(BinanceClient, nameof(BinanceClient));
            var streamResponse = await BinanceClient.StartUserDataStream(mt);
            ListenKey = streamResponse.ListenKey;

            var endpoint = new Uri($"{BaseWebsocketUri}/{ListenKey}");

            if (mt == MarketType.UsdM)
            {
                if (!isTestMode)
                    endpoint = new Uri($"{BaseWebsocketUriUsdMFut}/{ListenKey}");
                else
                    endpoint = new Uri($"{BaseWebsocketUriUsdMFutTest}/{ListenKey}");

                var guid = CreateUserDataBinanceWebSocketFuture(endpoint, userDataFuturesMessageHandlers);
                return guid;
            }
            if (mt == MarketType.CoinM)
            {
                endpoint = new Uri($"{BaseWebsocketUri}/{ListenKey}");
                return CreateUserDataBinanceWebSocketFuture(endpoint, userDataFuturesMessageHandlers);
            }


            return CreateUserDataBinanceWebSocketSpot(endpoint, userDataMessageHandlers);
        }




        /// <summary>
        /// Connect to the Kline WebSocket
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <param name="messageEventHandler"></param>
        /// <returns></returns>
        public Guid ConnectToKlineWebSocket(MarketType mt, string symbol, bool isTestMode, KlineInterval interval, BinanceWebSocketMessageHandler<BinanceKlineData> messageEventHandler)
        {
            Guard.AgainstNullOrEmpty(symbol, nameof(symbol));

            var endpoint = new Uri($"{BaseWebsocketUri}/{symbol.ToLower()}@kline_{EnumExtensions.GetEnumMemberValue(interval)}");

            if (mt == MarketType.UsdM)
            {
                if (!isTestMode)
                    endpoint = new Uri($"{BaseWebsocketUriUsdMFut}/{symbol.ToLower()}@kline_{EnumExtensions.GetEnumMemberValue(interval)}");
                else
                    endpoint = new Uri($"{BaseWebsocketUriUsdMFutTest}/{symbol.ToLower()}@kline_{EnumExtensions.GetEnumMemberValue(interval)}");
            }

            if (mt == MarketType.CoinM)
                endpoint = new Uri($"{BaseWebsocketUriCoinMFut}/{symbol.ToLower()}@kline_{EnumExtensions.GetEnumMemberValue(interval)}");

            return CreateBinanceWebSocket(endpoint, messageEventHandler);
        }

        /// <summary>
        /// Connect to the Depth WebSocket
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="messageEventHandler"></param>
        /// <returns></returns>
        public Guid ConnectToDepthWebSocket(MarketType mt, string symbol, bool isTestMode, BinanceWebSocketMessageHandler<BinanceDepthData> messageEventHandler)
        {
            Guard.AgainstNullOrEmpty(symbol, nameof(symbol));

            var endpoint = new Uri($"{BaseWebsocketUri}/{symbol.ToLower()}@depth");



            if (mt == MarketType.UsdM)
            {
                if (!isTestMode)
                    endpoint = new Uri($"{BaseWebsocketUriUsdMFut}/{symbol.ToLower()}@depth");
                else
                    endpoint = new Uri($"{BaseWebsocketUriUsdMFutTest}/{symbol.ToLower()}@depth");
            }

            if (mt == MarketType.CoinM)
                endpoint = new Uri($"{BaseWebsocketUriCoinMFut}/{symbol.ToLower()}@depth");


            return CreateBinanceWebSocket(endpoint, messageEventHandler);
        }

        /// <summary>
        /// Connect to thePartial Book Depth Streams
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="messageEventHandler"></param>
        /// <returns></returns>
        public Guid ConnectToPartialDepthWebSocket(string symbol, PartialDepthLevels levels, BinanceWebSocketMessageHandler<BinancePartialData> messageEventHandler)
        {
            Guard.AgainstNullOrEmpty(symbol, nameof(symbol));
            var endpoint = new Uri($"{BaseWebsocketUri}/{symbol.ToLower()}@depth{(int)levels}");
            return CreateBinanceWebSocket(endpoint, messageEventHandler);
        }

        /// <summary>
        /// Connect to Partial Book Depth Steam with 100ms frequency
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="levels"></param>
        /// <param name="messageEventHandler"></param>
        /// <returns></returns>
        public Guid ConnectToFastPartialDepthWebSocket(string symbol, PartialDepthLevels levels, BinanceWebSocketMessageHandler<BinancePartialData> messageEventHandler)
        {
            Guard.AgainstNullOrEmpty(symbol, nameof(symbol));
            var endpoint = new Uri($"{BaseWebsocketUri}/{symbol.ToLower()}@depth{(int)levels}@100ms");
            return CreateBinanceWebSocket(endpoint, messageEventHandler);
        }

        /// <summary>
        /// Connect to the Combined Depth WebSocket
        /// </summary>
        /// <param name="symbols"></param>
        /// <param name="messageEventHandler"></param>
        /// <returns></returns>
        public Guid ConnectToDepthWebSocketCombined(string symbols, BinanceWebSocketMessageHandler<BinanceCombinedDepthData> messageEventHandler)
        {
            Guard.AgainstNullOrEmpty(symbols, nameof(symbols));
            symbols = PrepareCombinedSymbols.CombinedDepth(symbols);
            var endpoint = new Uri($"{CombinedWebsocketUri}={symbols}");
            return CreateBinanceWebSocket(endpoint, messageEventHandler);
        }
        /// <summary>
        /// Connect to the Combined Partial Depth WebSocket
        /// </summary>
        /// <param name="symbols"></param>
        /// <param name="depth"></param>
        /// <param name="messageEventHandler"></param>
        /// <returns></returns>
        public Guid ConnectToDepthWebSocketCombinedPartial(string symbols, string depth, BinanceWebSocketMessageHandler<BinancePartialDepthData> messageEventHandler)
        {
            Guard.AgainstNullOrEmpty(symbols, nameof(symbols));
            Guard.AgainstNullOrEmpty(depth, nameof(depth));
            symbols = PrepareCombinedSymbols.CombinedPartialDepth(symbols, depth);
            var endpoint = new Uri($"{CombinedWebsocketUri}={symbols}");
            return CreateBinanceWebSocket(endpoint, messageEventHandler);
        }

        /// <summary>
        /// Connect to the Trades WebSocket
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="messageEventHandler"></param>
        /// <returns></returns>
        public Guid ConnectToTradesWebSocket(MarketType mt, string symbol, bool isTestMode, BinanceWebSocketMessageHandler<BinanceAggregateTradeData> messageEventHandler)
        {
            Guard.AgainstNullOrEmpty(symbol, nameof(symbol));
            var endpoint = new Uri($"{BaseWebsocketUri}/{symbol.ToLower()}@aggTrade");

            if (mt == MarketType.UsdM)
            {
                if (!isTestMode)
                    endpoint = new Uri($"{BaseWebsocketUriUsdMFut}/{symbol.ToLower()}@aggTrade");
                else
                    endpoint = new Uri($"{BaseWebsocketUriUsdMFutTest}/{symbol.ToLower()}@aggTrade");
            }

            if (mt == MarketType.CoinM)
                endpoint = new Uri($"{BaseWebsocketUriCoinMFut}/{symbol.ToLower()}@aggTrade");

            return CreateBinanceWebSocket(endpoint, messageEventHandler);
        }

        /// <summary>
        /// Connect to the Individual Symbol Ticker WebSocket
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="messageEventHandler"></param>
        /// <returns></returns>
        public Guid ConnectToIndividualSymbolTickerWebSocket(MarketType mt, string symbol, bool isTestMode, BinanceWebSocketMessageHandler<BinanceTradeData> messageEventHandler)
        {
            Guard.AgainstNullOrEmpty(symbol, nameof(symbol));
            var endpoint = new Uri($"{BaseWebsocketUri}/{symbol.ToLower()}@ticker");

            if (mt == MarketType.UsdM)
            {
                if (!isTestMode)
                    endpoint = new Uri($"{BaseWebsocketUriUsdMFut}/{symbol.ToLower()}@ticker");
                else
                    endpoint = new Uri($"{BaseWebsocketUriUsdMFutTest}/{symbol.ToLower()}@ticker");
            }

            if (mt == MarketType.CoinM)
                endpoint = new Uri($"{BaseWebsocketUriCoinMFut}/{symbol.ToLower()}@ticker");

            return CreateBinanceWebSocket(endpoint, messageEventHandler);
        }

        /// <summary>
        /// Connect to the All Market Symbol Ticker WebSocket
        /// </summary>
        /// <param name="messageEventHandler"></param>
        /// <returns></returns>
        public Guid ConnectToIndividualSymbolTickerWebSocket(BinanceWebSocketMessageHandler<BinanceAggregateTradeData> messageEventHandler)
        {
            var endpoint = new Uri($"{BaseWebsocketUri}/!ticker@arr");
            return CreateBinanceWebSocket(endpoint, messageEventHandler);
        }

        private Guid CreateUserDataBinanceWebSocketSpot(Uri endpoint, UserDataSpotWebSocketMessages userDataWebSocketMessages)
        {
            var websocket = new BinanceWebSocket(endpoint.AbsoluteUri);
            websocket.OnOpen += (sender, e) =>
            {

            };
            websocket.OnMessage += (sender, e) =>
            {

                var primitive = JsonConvert.DeserializeObject<BinanceWebSocketResponse>(e.Data);
                switch (primitive.EventType)
                {
                    case "ACCOUNT_UPDATE":
                    case "outboundAccountPosition":
                        var userData = JsonConvert.DeserializeObject<BinanceAccountUpdateData>(e.Data);
                        userDataWebSocketMessages.AccountUpdateMessageHandler?.Invoke(userData);
                        break;
                    case "ORDER_TRADE_UPDATE":
                    case "executionReport":

                        var orderTradeData = JsonConvert.DeserializeObject<BinanceTradeOrderData>(e.Data);
                        userDataWebSocketMessages.OrderUpdateMessageHandler?.Invoke(orderTradeData);
                        break;
                    default:
                        Debug.WriteLine("Event " + primitive.EventType + " not handled");
                        break;
                }
            };
            websocket.OnError += (sender, e) =>
            {
                CloseWebSocketInstance(websocket.Id, true);
                throw new Exception("Binance UserData WebSocket failed")
                {
                    Data =
                    {
                        {"ErrorEventArgs", e}
                    }
                };
            };

            if (!ActiveWebSockets.ContainsKey(websocket.Id))
            {
                ActiveWebSockets.Add(websocket.Id, websocket);
            }

            AllSockets.Add(websocket);
            websocket.SslConfiguration.EnabledSslProtocols = SupportedProtocols;
            websocket.Connect();




            return websocket.Id;
        }


        private Guid CreateUserDataBinanceWebSocketFuture(Uri endpoint, UserDataFuturesWebSocketMessages userDataWebSocketMessages)
        {
            var websocket = new BinanceWebSocket(endpoint.AbsoluteUri);
            websocket.OnOpen += (sender, e) =>
            {
                StartUserDataTimers();
            };
            websocket.OnMessage += async (sender, e) =>
            {

                var primitive = JsonConvert.DeserializeObject<BinanceWebSocketResponse>(e.Data);
                switch (primitive.EventType)
                {
                    case "listenKeyExpired":
                        StopDataTimers();
                        BinanceClient.DisconnectUserDataWebSocket();
                        LogController.Print("Try get new listen key");
                        await BinanceClient.ConnectToUserDataFutWebSocket();
                        LogController.Print("Succesfully reconnect with new listen key: " + ListenKey);
                        break;
                    case "ACCOUNT_UPDATE":
                        var userData = JsonConvert.DeserializeObject<Models.WebSocket.Futures.BinanceAccountUpdateData>(e.Data);
                        userDataWebSocketMessages.AccountUpdateMessageHandler?.Invoke(userData);
                        break;
                    case "ORDER_TRADE_UPDATE":
                        var orderTradeData = JsonConvert.DeserializeObject<Models.WebSocket.Futures.BinanceTradeOrderData>(e.Data);
                        userDataWebSocketMessages.OrderUpdateMessageHandler?.Invoke(orderTradeData);

                        break;
                    default:
                        Debug.WriteLine("Event " + primitive.EventType + " not handled");
                        break;
                }
            };
            websocket.OnClose += (sender, e) =>
            {
                StopDataTimers();
            };

            websocket.OnError += (sender, e) =>
            {
                StopDataTimers();
                CloseWebSocketInstance(websocket.Id, true);
                throw new Exception("Binance UserData WebSocket failed")
                {
                    Data =
                    {
                        {"ErrorEventArgs", e}
                    }
                };
            };

            if (!ActiveWebSockets.ContainsKey(websocket.Id))
            {
                ActiveWebSockets.Add(websocket.Id, websocket);
            }

            AllSockets.Add(websocket);
            websocket.SslConfiguration.EnabledSslProtocols = SupportedProtocols;
            websocket.Connect();

            return websocket.Id;
        }


        private void StartUserDataTimers()
        {
            //_keepAliveTimer = new Timer((int)TimeSpan.FromSeconds(29).TotalMilliseconds);
            _keepAliveTimer = new Timer((int)TimeSpan.FromMinutes(51).TotalMilliseconds);
            _keepAliveTimer.Elapsed += _keepAliveTimer_Elapsed;
            _keepAliveTimer.Start();

            //_listenKeyTimer = new Timer((int)TimeSpan.FromSeconds(59).TotalMilliseconds);
            _listenKeyTimer = new Timer((int)TimeSpan.FromHours(23).TotalMilliseconds);
            _listenKeyTimer.Elapsed += _listenKeyTimer_Elapsed;
            _listenKeyTimer.Start();
        }

        private void StopDataTimers()
        {
            _keepAliveTimer?.Stop();
            _listenKeyTimer?.Stop();
        }

        private void StartMarketDataTimers()
        {
            if (_marketDataListenKeyTimer == null)
            {
                _marketDataListenKeyTimer = new Timer((int)TimeSpan.FromHours(23).TotalMilliseconds);
                //_marketDataListenKeyTimer = new Timer((int)TimeSpan.FromSeconds(60).TotalMilliseconds);
                _marketDataListenKeyTimer.Elapsed += _marketDataListenKeyTimer_Elapsed; ;
            }

            if (!_marketDataListenKeyTimer.Enabled)
                _marketDataListenKeyTimer.Start();
        }


        private void StopMarketDataTimers()
        {

            if (_marketDataListenKeyTimer != null)
                _marketDataListenKeyTimer.Stop();
        }

        private async void _keepAliveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await BinanceClient.KeepAliveUserDataStream(MarketType.UsdM, ListenKey);
            LogController.Print("Keep alive listen Key: " + ListenKey);
        }

        private async void _listenKeyTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                BinanceClient.DisconnectUserDataWebSocket();
                await BinanceClient.CloseUserDataStream(MarketType.UsdM, ListenKey);
                LogController.Print("Try get new listen key");
                await BinanceClient.ConnectToUserDataFutWebSocket();
                LogController.Print("Succesfully reconnect with new listen key: " + ListenKey);
            }
            catch (BinanceException bex)
            {
                LogController.Print(bex.Message + Environment.NewLine + bex.ErrorDetails);
            }

        }


        private void _marketDataListenKeyTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                BinanceClient.UnsubcribeMarketDataWebSockets();
                LogController.Print("Disconnect from data streams");
                BinanceClient.SubcribeMarketDataWebSockets();
                LogController.Print("Succesfully reconnect to data streams ");

            }
            catch (BinanceException bex)
            {
                LogController.Print(bex.Message + Environment.NewLine + bex.ErrorDetails);
            }
        }



        private void Websocket_OnClose(object sender, CloseEventArgs e)
        {
            throw new NotImplementedException();

        }

        public void HandleLog(LogData logdata, string s)
        {
            
            Debug.WriteLine(logdata.Message);
        }


        private Guid CreateBinanceWebSocket<T>(Uri endpoint, BinanceWebSocketMessageHandler<T> messageEventHandler) where T : IWebSocketResponse
        {
            var websocket = new BinanceWebSocket(endpoint.AbsoluteUri);
            websocket.EmitOnPing = true;
           // websocket.Log.Level = LogLevel.Trace;
           // websocket.Log.Output = HandleLog;
            websocket.OnOpen += (sender, e) =>
            {
                StartMarketDataTimers();
            };

            
            websocket.OnMessage += (sender, e) =>
            {
                //TODO: Log message received               


                //Debug.WriteLine(e.Data);
                if (!e.IsPing)
                {
                    var primitive = JsonConvert.DeserializeObject<BinanceWebSocketResponse>(e.Data);
                    if (!string.IsNullOrEmpty(primitive.EventType))
                    {
                        switch (primitive.EventType)
                        {
                            case "depthUpdate":
                                var data = JsonConvert.DeserializeObject<T>(e.Data);
                                messageEventHandler(data);
                                break;
                            default:
                                //Debug.WriteLine("Event " + primitive.EventType + " not handled");
                                break;
                        }
                    }
                 
                }
                else
                {
                    

                }



            };
            websocket.OnClose += (sender, e) =>
            {
                StopMarketDataTimers();
                StopDataTimers();
            };

            websocket.OnError += (sender, e) =>
            {
                StopMarketDataTimers();
                CloseWebSocketInstance(websocket.Id, true);
                throw new Exception("Binance WebSocket failed")
                {
                    Data =
                    {
                        {"ErrorEventArgs", e}
                    }
                };
            };

            if (!ActiveWebSockets.ContainsKey(websocket.Id))
            {
                ActiveWebSockets.Add(websocket.Id, websocket);
            }

            AllSockets.Add(websocket);
            websocket.SslConfiguration.EnabledSslProtocols = SupportedProtocols;
            websocket.Connect();

            return websocket.Id;
        }

        /// <summary>
        /// Close a specific WebSocket instance using the Guid provided on creation
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fromError"></param>
        public void CloseWebSocketInstance(Guid id, bool fromError = false)
        {
            if (ActiveWebSockets.ContainsKey(id))
            {
                var ws = ActiveWebSockets[id];
                ActiveWebSockets.Remove(id);
                if (!fromError)
                {
                    ws.Close(CloseStatusCode.PolicyViolation);
                }
            }
            else
            {
                throw new Exception($"No Websocket exists with the Id {id.ToString()}");
            }
        }

        /// <summary>
        /// Checks whether a specific WebSocket instance is active or not using the Guid provided on creation
        /// </summary>
        public bool IsAlive(Guid id)
        {
            if (ActiveWebSockets.ContainsKey(id))
            {
                var ws = ActiveWebSockets[id];
                return ws.IsAlive;
            }
            else
            {
                throw new Exception($"No Websocket exists with the Id {id.ToString()}");
            }
        }
    }
}
