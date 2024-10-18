using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Timers;
using Twm.Core.Controllers;
using Twm.Core.DataProviders.Bybit.Enums;
using Twm.Core.DataProviders.Bybit.Models.Response.Error;
using Twm.Core.DataProviders.Bybit.Models.WebSocket;
using Twm.Core.DataProviders.Bybit.Utility;
using Newtonsoft.Json;
using WebSocketSharp;
using Twm.Core.Helpers;
using Twm.Core.DataProviders.Bybit.Models.WebSocket.Response;
using Microsoft.EntityFrameworkCore.Internal;
using Twm.Core.DataProviders.Bybit.Models.WebSocket.Reponse;

namespace Twm.Core.DataProviders.Bybit.Websockets
{
    /// <summary>
    /// Abstract class for creating WebSocketClients 
    /// </summary>
    public class AbstractBybitWebSocketClient
    {
        protected SslProtocols SupportedProtocols { get; } = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;


        /// <summary>
        /// Used for deletion on the fly
        /// </summary>
        protected Dictionary<Guid, BybitWebSocket> ActiveWebSockets;
        protected List<BybitWebSocket> AllSockets;
        protected readonly BybitClient BybitClient;

        protected Dictionary<string, Guid> SpotWebSockets;
        protected Dictionary<string, Guid> PerpWebSockets;


        protected Guid PrivateWebSocket;





        private Timer _keepAliveTimer;

      
        public AbstractBybitWebSocketClient(BybitClient binanceClient)
        {
            BybitClient = binanceClient;
            ActiveWebSockets = new Dictionary<Guid, BybitWebSocket>();
            AllSockets = new List<BybitWebSocket>();
            SpotWebSockets = new Dictionary<string, Guid>();
            PerpWebSockets = new Dictionary<string, Guid>();

        }


        private void StartUserDataTimers(BybitWebSocket webSocket)
        {
            _keepAliveTimer = new System.Timers.Timer((int)TimeSpan.FromSeconds(20).TotalMilliseconds);
            _keepAliveTimer.Elapsed += (sender, e) =>
            {
                if (webSocket != null && webSocket.IsAlive)
                {
                    
                    var sendData = "{\"op\": \"ping\"}";
                    webSocket.Send(sendData);
                }
            };
            _keepAliveTimer.Start();
        }


        // Expose the listenKey that gets returned by Bybit REST request "POST /api/v1/userDataStream" to start a user data stream
        /// <summary>
        /// Connect to the UserData WebSocket
        /// </summary>
        /// <param name="userDataMessageHandlers"></param>
        /// <returns>Guid of connection.</returns>
        /// sets the Bybit Listen Key (binanceListenKey)
        public async Task<Guid> ConnectToUserDataWebSocket(UserDataWebSocketMessages messageEventHandler, bool isTestMode)
        {
            Guard.AgainstNull(BybitClient, nameof(BybitClient));
            return CreateBybitWebSocket(Endpoints.WebSocket.GetPrivateStream(isTestMode).Uri, messageEventHandler);
        }




        /// <summary>
        /// Connect to the Kline WebSocket
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <param name="messageEventHandler"></param>
        /// <returns></returns>
        public Guid ConnectToKlineWebSocket(BybitWebSocketMessageHandler<BybitKlineResponse> messageEventHandler, MarketType mt, string symbol, bool isTestMode, KlineInterval interval)
        {
            Guard.AgainstNullOrEmpty(symbol, nameof(symbol));
            var command = $"kline.{EnumHelper.GetEnumMemberAttrValue(interval)}.{symbol}";
            var userDataWebSocketMessages = new UserDataWebSocketMessages();
            userDataWebSocketMessages.KlineMessageHandler = messageEventHandler;
            var websocketId = CreateBybitWebSocket(Endpoints.WebSocket.GetPublicStream(mt, isTestMode).Uri, userDataWebSocketMessages);
            Subscribe(websocketId, command);
            return websocketId;
        }

        /// <summary>
        /// Connect to the Depth WebSocket
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="messageEventHandler"></param>
        /// <returns></returns>
        public Guid ConnectToDepthWebSocket(UserDataWebSocketMessages messageEventHandler, MarketType mt, string symbol, bool isTestMode, int levels)
        {
            Guard.AgainstNullOrEmpty(symbol, nameof(symbol));            
            var command = $"orderbook.{levels}.{symbol.ToUpper()}";
            var websocketId = CreateBybitWebSocket(Endpoints.WebSocket.GetPublicStream(mt, isTestMode).Uri, messageEventHandler);
            Subscribe(websocketId, command);
            return websocketId;

        }


        /// <summary>
        /// Connect to the Trades WebSocket
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="messageEventHandler"></param>
        /// <returns></returns>
        public Guid ConnectToTradesWebSocket(UserDataWebSocketMessages messageEventHandler, MarketType mt, string symbol, bool isTestMode)
        {
            Guard.AgainstNullOrEmpty(symbol, nameof(symbol));
            var command = $"{symbol.ToLower()}@aggTrade";
            var websocketId = CreateBybitWebSocket(Endpoints.WebSocket.GetPublicStream(mt, isTestMode).Uri, messageEventHandler);
            Subscribe(websocketId, command);
            return websocketId;
        }






        private Guid CreateBybitWebSocket(Uri endpoint, UserDataWebSocketMessages messageEventHandler)
        {
            var websocket = new BybitWebSocket(endpoint.AbsoluteUri);
            websocket.OnOpen += (sender, e) =>
            {
                StartUserDataTimers(websocket);
            };
            websocket.OnMessage += (sender, e) =>
            {
                var primitive = JsonConvert.DeserializeObject<BybitWebSocketResponse>(e.Data);

                if (!string.IsNullOrEmpty(primitive.Topic))
                {
                    var paramArr = primitive.Topic.ToUpper().Split('.');
                    if (paramArr.Any())
                    {
                        switch (paramArr[0])
                        {
                            case "ORDER":
                                var orderResponse = JsonConvert.DeserializeObject<BybitOrderResponse>(e.Data);
                                messageEventHandler.OrderUpdateMessageHandler?.Invoke(orderResponse);
                                break;
                            case "POSITION":
                                var positionResponse = JsonConvert.DeserializeObject<BybitPositionResponse>(e.Data);
                                messageEventHandler.PositionMessageHandler?.Invoke(positionResponse);
                                break;
                            case "KLINE":
                                var klineResponse = JsonConvert.DeserializeObject<BybitKlineResponse>(e.Data);
                                messageEventHandler.KlineMessageHandler?.Invoke(klineResponse);
                                break;
                            case "WALLET":
                                var walletResponse = JsonConvert.DeserializeObject<BybitWalletResponse>(e.Data);
                                messageEventHandler.WalletMessageHandler?.Invoke(walletResponse);
                                break;
                            case "ORDERBOOK":
                                var orderBookResponse = JsonConvert.DeserializeObject<BybitOrderBookResponse>(e.Data);
                                orderBookResponse.Category = endpoint.AbsoluteUri.ToUpper().Contains("/SPOT") ? "spot" : "linear";
                                messageEventHandler.DepthMessageHandler?.Invoke(orderBookResponse);
                                break;
                            default:
                                Debug.WriteLine("Topic " + primitive.Topic + " not handled");
                                break;
                        }
                    }
                }
                else
                {
                    //Debug.WriteLine(e.Data);
                }


            };
            websocket.OnError += (sender, e) =>
            {
                StopDataTimers();
                CloseWebSocketInstance(websocket.Id, true);
                throw new Exception("WebSocket failed")
                {
                    Data =
                    {
                        {"ErrorEventArgs", e}
                    }
                };
            };
            websocket.OnClose += (sender, e) =>
            {
                StopDataTimers();


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




        private void StopDataTimers()
        {
            _keepAliveTimer?.Stop();

        }

      


        private void _marketDataListenKeyTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                BybitClient.UnsubcribeMarketDataWebSockets();
                LogController.Print("Disconnect from data streams");
                BybitClient.SubcribeMarketDataWebSockets();
                LogController.Print("Succesfully reconnect to data streams ");

            }
            catch (BybitException bex)
            {
                LogController.Print(bex.Message + Environment.NewLine + bex.ErrorDetails);
            }
        }



        private void Websocket_OnClose(object sender, CloseEventArgs e)
        {
            throw new NotImplementedException();

        }



        /*  private Guid CreateBybitWebSocket<T>(Uri endpoint, string authCommand, string command, string type, BybitWebSocketMessageHandler<T> messageEventHandler) where T : IWebSocketResponse
          {
              var websocket = new BybitWebSocket(endpoint.AbsoluteUri);
              websocket.OnOpen += (sender, e) =>
              {
                  StartMarketDataTimers(websocket);
              };
              websocket.OnMessage += (sender, e) =>
              {
                  //TODO: Log message received               
                  if (e.Data.Contains("\"topic\":"))
                  {
                      var data = JsonConvert.DeserializeObject<T>(e.Data);
                      messageEventHandler(data);
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
                  throw new Exception("Bybit WebSocket failed")
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
          }*/

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

        public void Auth(Guid websocketId, string authCommand)
        {
            var websocket = ActiveWebSockets[websocketId];
            if (!string.IsNullOrEmpty(authCommand))
            {
                var sendAuth = "{\"op\": \"auth\", \"args\": [" + authCommand + "]}";
                websocket.Send(sendAuth);
            }
        }

        public void Subscribe(Guid websocketId, string command)
        {
            var websocket = ActiveWebSockets[websocketId];
            if (!string.IsNullOrEmpty(command))
            {
                var sendData = "{\"op\": \"subscribe\", \"args\": [\"" + command + "\"]}";
                websocket.Send(sendData);
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
