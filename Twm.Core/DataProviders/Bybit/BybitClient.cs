
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using System.Net.Http;
using Twm.DB.DAL.Repositories.Instruments;
using Twm.Chart.Classes;
using Twm.Chart.Interfaces;
using Twm.Core.Controllers;
using Twm.Core.Interfaces;
using Twm.Core.Helpers;
using Twm.Core.Enums;
using Twm.Core.Market;
using Twm.Core.ViewModels.DataSeries;
using Twm.Core.DataProviders.Common;
using Twm.Core.DataProviders.Common.Request;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.DataProviders.Utility;
using Twm.Core.DataProviders.Common.Response;
using Twm.Core.DataProviders.Common.Caching;
using Twm.Core.DataProviders.Bybit.Enums;
using Twm.Core.DataProviders.Bybit.Models.WebSocket;
using Twm.Core.DataProviders.Bybit.Models.Response.Error;
using Twm.Core.DataProviders.Bybit.Models.Response;
using Twm.Core.DataProviders.Bybit.Models.Request;
using Twm.Core.DataProviders.Bybit.Models.Response.Custom;
using Twm.Core.DataProviders.Bybit.Websockets;
using Twm.Core.DataProviders.Bybit.Models.Request.Custom;
using Twm.Core.DataProviders.Bybit.Models.Classes;
using Twm.Core.DataProviders.Bybit.Models.WebSocket.Response;
using Twm.Model.Model;
using NativeOrder = Twm.Core.Market.Order;
using NativePosition = Twm.Core.Market.Position;
using NativeAsset = Twm.Core.Market.Asset;
using OrderAction = Twm.Core.Enums.OrderAction;
using OrderType = Twm.Core.Enums.OrderType;
using SubmitOrderRequest = Twm.Core.DataProviders.Common.Request.SubmitOrderRequest;
using BybitOrderType = Twm.Core.DataProviders.Bybit.Enums.OrderType;
using UserDataWebSocketMessages = Twm.Core.DataProviders.Bybit.Websockets.UserDataWebSocketMessages;
using Twm.Core.DataProviders.Bybit.Models.WebSocket.Reponse;
using Session = Twm.Core.Classes.Session;
using Twm.Core.DataProviders.Common.OrderBooks;




namespace Twm.Core.DataProviders.Bybit
{
    public class BybitClient : IDataProviderClient
    {
        public const int CONTRACT_ID_BASE = 60000000;
        public const int CONTRACT_DETAILS_ID = CONTRACT_ID_BASE + 1;
        public const int FUNDAMENTALS_ID = CONTRACT_ID_BASE + 2;

        private DisposableBybitWebSocketClient _bybitWebSocketClient;

        private const string APIHeader = "X-BAPI-API-KEY";
        private const string SecretHeader = "X-BAPI-SIGN";
        private const string RecWindowHeader = "X-BAPI-RECV-WINDOW";
        private const string ReferenceHeader = "Referer";
        private const string TimestampHeader = "X-BAPI-TIMESTAMP";

        public TimeSpan TimestampOffset
        {
            get { return _timestampOffset; }
            set { _timestampOffset = value; }
        }

        private TimeSpan _timestampOffset;
        private readonly string _apiKey;
        private readonly string _secretKey;
        private int _recWindow;
        private readonly IAPIProcessor _apiProcessor;
        private readonly int _defaultReceiveWindow;


        public bool IsTestMode { get; set; }

        public string Token { get; set; }

        public string Message { get; set; }

        public string ServerName { get; set; }

        private readonly RequestClient _requestClient;

        private readonly ConcurrentDictionary<string, Guid> _klineSubscriptions = new ConcurrentDictionary<string, Guid>();
        private readonly ConcurrentDictionary<string, List<Action<string, IEnumerable<ICandle>>>> _klineCallbacks = new ConcurrentDictionary<string, List<Action<string, IEnumerable<ICandle>>>>();

        private ConcurrentDictionary<Action<string, IEnumerable<ICandle>>, IRequest> _klineCallbackRequests = new ConcurrentDictionary<Action<string, IEnumerable<ICandle>>, IRequest>();

        private readonly ConcurrentDictionary<string, Guid> _liveSubscriptions = new ConcurrentDictionary<string, Guid>();
        private readonly ConcurrentDictionary<string, List<Action<string, double>>> _liveCallbacks = new ConcurrentDictionary<string, List<Action<string, double>>>();




        private readonly ConcurrentDictionary<string, Guid> _depthFuturesSubscriptions = new ConcurrentDictionary<string, Guid>();
        private readonly ConcurrentDictionary<string, Guid> _depthSpotSubscriptions = new ConcurrentDictionary<string, Guid>();
        private readonly ConcurrentDictionary<string, List<Action<string, OrderBookResult>>> _depthFuturesCallbacks = new ConcurrentDictionary<string, List<Action<string, OrderBookResult>>>();
        private readonly ConcurrentDictionary<string, List<Action<string, OrderBookResult>>> _depthSpotCallbacks = new ConcurrentDictionary<string, List<Action<string, OrderBookResult>>>();


        private ConcurrentDictionary<Action<string, OrderBookResult>, IRequest> _depthCallbackRequests = new ConcurrentDictionary<Action<string, OrderBookResult>, IRequest>();





        private ConcurrentDictionary<string, OrderBook> _futuresOrderBooks = new ConcurrentDictionary<string, OrderBook>();
        private ConcurrentDictionary<string, OrderBook> _spotOrderBooks = new ConcurrentDictionary<string, OrderBook>();


        public int ConnectionId { get; set; }

        public EventHandler<IResponse> OrderStatusChanged { get; set; }
        public EventHandler<IResponse> AccountChanged { get; set; }
        public EventHandler<IResponse> PositionChanged { get; set; }

        public EventHandler<IResponse> AssetChanged { get; set; }
        public bool IsPrivate
        {
            get { return !string.IsNullOrEmpty(_apiKey); }
        }

        private SynchronizationContext _sc;


        public event Action<int, int, string, Exception> Error;

        private Dictionary<string, NativeOrder> _orderCollection;
        private Dictionary<string, NativePosition> _positionCollection;

        private Dictionary<string, NativeAsset> _assetCollection;

        UserDataWebSocketMessages _userDataWebSocketMessages;

        /// <summary>
        /// Create a new Interactive brokers Client based on the configuration provided
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="isTestMode"></param>
        public BybitClient(ClientConfiguration configuration, IAPIProcessor apiProcessor = null, bool isTestMode = false)
        {


            _requestClient = new RequestClient();
            _recWindow = 20000;
            _apiKey = configuration.ApiKey;
            _secretKey = configuration.SecretKey;

            _requestClient.SetRecWindow(RecWindowHeader, _recWindow);
            _requestClient.SetRateLimiting(configuration.EnableRateLimiting);
            _requestClient.SetAPIKey(APIHeader, _apiKey);
            _requestClient.SetReference(ReferenceHeader, configuration.Reference);

            ConnectionId = configuration.ConnectionId;
            ServerName = configuration.ServerName;
            IsTestMode = isTestMode;


            _sc = Session.Instance.UiContext;

            if (apiProcessor == null)
            {
                _apiProcessor = new BybitAPIProcessor(_apiKey, _secretKey, new APICacheManager());
                _apiProcessor.SetCacheTime(configuration.CacheTime);
            }
            else
            {
                _apiProcessor = apiProcessor;
            }

            _apiProcessor.RequestClient = _requestClient;

            _userDataWebSocketMessages = new UserDataWebSocketMessages();
            _userDataWebSocketMessages.OrderUpdateMessageHandler = PrivateOrderMessageHandler;
            _userDataWebSocketMessages.PositionMessageHandler = PrivatePositionMessageHandler;
            _userDataWebSocketMessages.WalletMessageHandler = PrivateWalletMessageHandler;
            _userDataWebSocketMessages.DepthMessageHandler = DepthMessageHandler;


        }


        #region Bybit specific

        #region User Stream
        /// <summary>
        /// Starts a user data stream
        /// </summary>
        /// /// <returns><see cref="UserDataStreamResponse"/></returns>
        public async Task<UserDataStreamResponse> StartUserDataStream(MarketType mt)
        {
            return await _apiProcessor.ProcessPostRequest<UserDataStreamResponse>(Endpoints.UserStream.StartUserDataStream(mt, IsTestMode));
        }

        /// <summary>
        /// Pings a user data stream to prevent timeouts
        /// </summary>
        /// <param name="userDataListenKey"></param>
        /// /// <returns><see cref="UserDataStreamResponse"/></returns>
        public async Task<UserDataStreamResponse> KeepAliveUserDataStream(MarketType mt, string userDataListenKey)
        {
            Guard.AgainstNullOrEmpty(userDataListenKey);

            return await _apiProcessor.ProcessPutRequest<UserDataStreamResponse>(Endpoints.UserStream.KeepAliveUserDataStream(mt, userDataListenKey, IsTestMode));
        }

        /// <summary>
        /// Closes a user data stream
        /// </summary>
        /// <param name="userDataListenKey"></param>
        /// /// <returns><see cref="UserDataStreamResponse"/></returns>
        public async Task<UserDataStreamResponse> CloseUserDataStream(MarketType mt, string userDataListenKey)
        {
            Guard.AgainstNullOrEmpty(userDataListenKey);

            return await _apiProcessor.ProcessDeleteRequest<UserDataStreamResponse>(Endpoints.UserStream.CloseUserDataStream(mt, userDataListenKey, IsTestMode));
        }
        #endregion

        #region General
        /// <summary>
        /// Test the connectivity to the API
        /// </summary>
        public async Task<IResponse> TestConnectivity(MarketType mt)
        {
            return await _apiProcessor.ProcessGetRequest<EmptyResponse>(Endpoints.General.TestConnectivity(mt, IsTestMode));
        }

        /// <summary>
        /// Get the current server time (UTC)
        /// </summary>
        /// <returns><see cref="ServerTimeResponse"/></returns>
        public async Task<ServerTimeResponse> GetServerTime()
        {
            return await _apiProcessor.ProcessGetRequest<ServerTimeResponse>(Endpoints.General.ServerTime);
        }

        /// <summary>
        /// Current exchange trading rules and symbol information
        /// </summary>
        /// <returns><see cref="ExchangeInfoResponse"/></returns>
        public async Task<GetInstrumentInfoResponse> GetInstrumentsInfo(IRequest request, MarketType mt)
        {
            return await _apiProcessor.ProcessGetRequest<GetInstrumentInfoResponse>(Endpoints.General.InstrumentsInfo(request, IsTestMode));
        }


        public async Task<TickersResponse> GetTickers(IRequest request, MarketType mt)
        {
            return await _apiProcessor.ProcessGetRequest<TickersResponse>(Endpoints.General.TickerInfo(request, IsTestMode));
        }

        #endregion

        #region Market Data
        /// <summary>
        /// Gets the current depth order book for the specified symbol
        /// </summary>
        /// <param name="symbol">The symbole to retrieve the order book for</param>
        /// <param name="useCache"></param>
        /// <param name="limit">Amount to request - defaults to 100</param>
        /// <returns></returns>
        public async Task<OrderBookResponse> GetOrderBook(string symbol, bool useCache = false, int limit = 100)
        {
            Guard.AgainstNull(symbol);
            if (limit > 5000)
            {
                throw new ArgumentException("When requesting the order book, you can't request more than 5000 at a time.", nameof(limit));
            }
            return await _apiProcessor.ProcessGetRequest<OrderBookResponse>(Endpoints.MarketData.OrderBook(symbol, limit, useCache));
        }

        /// <summary>
        /// Gets the Compressed aggregated trades in the specified window
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<CompressedAggregateTradeResponse>> GetCompressedAggregateTrades(GetCompressedAggregateTradesRequest request)
        {
            Guard.AgainstNull(request);
            Guard.AgainstNull(request.Symbol);
            if (request.Limit == null || (request.Limit <= 0 || request.Limit > 500))
            {
                request.Limit = 500;
            }

            return await _apiProcessor.ProcessGetRequest<List<CompressedAggregateTradeResponse>>(Endpoints.MarketData.CompressedAggregateTrades(request));
        }

        /// <summary>
        /// Gets the Klines/Candlesticks for the provided request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<KlineCandleStickResponse> GetKlinesCandlesticks(GetKlinesCandlesticksRequest request)
        {
            Guard.AgainstNull(request.Symbol);
            Guard.AgainstNull(request.Interval);

            if (request.Limit == 0 || request.Limit > 1000)
            {
                request.Limit = 1000;
            }

            return await _apiProcessor.ProcessGetRequest<KlineCandleStickResponse>(Endpoints.MarketData.KlineCandlesticks(request));
        }

        /// <summary>
        /// Gets the Daily price ticker for the provided symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public async Task<SymbolPriceChangeTickerResponse> GetDailyTicker(string symbol)
        {
            Guard.AgainstNull(symbol);

            return await _apiProcessor.ProcessGetRequest<SymbolPriceChangeTickerResponse>(Endpoints.MarketData.DayPriceTicker(symbol));
        }

        /// <summary>
        /// Gets all prices for all symbols
        /// </summary>
        /// <returns></returns>
        public async Task<List<SymbolPriceResponse>> GetSymbolsPriceTicker()
        {
            return await _apiProcessor.ProcessGetRequest<List<SymbolPriceResponse>>(Endpoints.MarketData.AllSymbolsPriceTicker);
        }

        /// <summary>
        /// Gets the best and quantity on the order book for all symbols
        /// </summary>
        /// <returns></returns>
        public async Task<List<SymbolOrderBookResponse>> GetSymbolOrderBookTicker()
        {
            return await _apiProcessor.ProcessGetRequest<List<SymbolOrderBookResponse>>(Endpoints.MarketData.SymbolsOrderBookTicker);
        }

        #region Market v3
        /// <summary>
        /// Gets the best and quantity on the order book for the provided symbol
        /// </summary>
        /// <returns></returns>
        public async Task<SymbolOrderBookResponse> GetSymbolOrderBookTicker(string symbol)
        {
            Guard.AgainstNull(symbol);

            return await _apiProcessor.ProcessGetRequest<SymbolOrderBookResponse>(Endpoints.MarketDataV3.BookTicker(symbol));
        }

        /// <summary>
        /// Gets the price for the provided symbol.  This is lighter weight than the daily ticker
        /// data.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public async Task<SymbolPriceResponse> GetPrice(string symbol)
        {
            Guard.AgainstNull(symbol);

            return await _apiProcessor.ProcessGetRequest<SymbolPriceResponse>(Endpoints.MarketDataV3.CurrentPrice(symbol));
        }

        /// <summary>
        /// Gets the current price for all symbols
        /// </summary>
        /// <returns></returns>
        public async Task<List<SymbolPriceResponse>> GetAllPrices()
        {
            return await _apiProcessor.ProcessGetRequest<List<SymbolPriceResponse>>(Endpoints.MarketDataV3.AllPrices);
        }

        #endregion
        #endregion

        #region Account and Market
        /// <summary>
        /// Creates an order based on the provided request
        /// </summary>
        /// <param name="request">The <see cref="CreateOrderRequest"/> that is used to define the order</param>
        /// <returns>This method can return <see cref="AcknowledgeCreateOrderResponse"/>, <see cref="FullCreateOrderResponse"/> 
        /// or <see cref="ResultCreateOrderResponse"/> based on the provided NewOrderResponseType enum in the request.
        /// </returns>
        public async Task<NewOrderResponse> CreateOrder(CreateOrderRequest request)
        {
            Guard.AgainstNull(request.Symbol);
            Guard.AgainstNull(request.Side);
            Guard.AgainstNull(request.OrderType);
            Guard.AgainstNull(request.Qty);

            var endpoint = Endpoints.Account.NewOrder(IsTestMode);
            var stringPayload = JsonConvert.SerializeObject(request);
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            _requestClient.SetSignature(SecretHeader, TimestampHeader, _secretKey, await httpContent.ReadAsStringAsync());

            return await _apiProcessor.ProcessPostRequest<NewOrderResponse>(endpoint, 5000, httpContent);

        }

        public async Task<ModifyOrderResponse> ModifyOrder(ModifyOrderRequest request)
        {

            Guard.AgainstNull(request.Symbol);

            var endpoint = Endpoints.Account.ModifyOrder(IsTestMode);
            var stringPayload = JsonConvert.SerializeObject(request);
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            _requestClient.SetSignature(SecretHeader, TimestampHeader, _secretKey, await httpContent.ReadAsStringAsync());

            return await _apiProcessor.ProcessPostRequest<ModifyOrderResponse>(endpoint, 5000, httpContent);
        }

        /// <summary>
        /// Queries an order based on the provided request
        /// </summary>
        /// <param name="request">The <see cref="QueryOrderRequest"/> that is used to define the order</param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<OrdersResponse> QueryOrder(QueryOrderRequest request, int receiveWindow = -1)
        {
            receiveWindow = SetReceiveWindow(receiveWindow);
            Guard.AgainstNull(request.Symbol);

            return await _apiProcessor.ProcessGetRequest<OrdersResponse>(Endpoints.Account.QueryOrder(request), receiveWindow);
        }


        /// <summary>
        /// Cancels an order based on the provided request
        /// </summary>
        /// <param name="request">The <see cref="CancelOrderRequest"/> that is used to define the order</param>        
        /// <returns></returns>
        public async Task<CancelOrderResponse> CancelOrder(Models.Request.CancelOrderRequest request)
        {
            var endpoint = Endpoints.Account.CancelOrder(IsTestMode);
            var stringPayload = JsonConvert.SerializeObject(request);
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            _requestClient.SetSignature(SecretHeader, TimestampHeader, _secretKey, await httpContent.ReadAsStringAsync());
            return await _apiProcessor.ProcessPostRequest<CancelOrderResponse>(endpoint, 5000, httpContent);
        }

        /// <summary>
        /// Queries all orders based on the provided request
        /// </summary>
        /// <param name="request">The <see cref="CurrentOpenOrdersRequest"/> that is used to define the orders</param>        
        /// <returns></returns>
        public async Task<OrdersResponse> GetCurrentOpenOrders(CurrentOpenOrdersRequest request)
        {

            var endpoint = Endpoints.Account.CurrentOpenOrders(request, IsTestMode);
            _requestClient.SetSignature(SecretHeader, TimestampHeader, _secretKey, endpoint.QueryString);
            return await _apiProcessor.ProcessGetRequest<OrdersResponse>(endpoint, _recWindow);
        }

        /// <summary>
        /// Queries all orders based on the provided request
        /// </summary>
        /// <param name="request">The <see cref="AllOrdersRequest"/> that is used to define the orders</param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<List<OrdersResponse>> GetAllOrders(AllOrdersRequest request, int receiveWindow = -1)
        {
            receiveWindow = SetReceiveWindow(receiveWindow);
            Guard.AgainstNull(request.Symbol);

            return await _apiProcessor.ProcessGetRequest<List<OrdersResponse>>(Endpoints.Account.AllOrders(request, IsTestMode), receiveWindow);
        }


        /// <summary>
        /// Queries current position information based on the provided request
        /// </summary>
        /// <param name="request">The <see cref="CurrentPositionInformationRequest"/> that is used to define the orders</param>        
        /// <returns></returns>
        public async Task<PositionsResponse> GetCurrentPositions(CurrentPositionInformationRequest request)
        {
            var endpoint = Endpoints.Account.CurrentPositions(request, IsTestMode);
            _requestClient.SetSignature(SecretHeader, TimestampHeader, _secretKey, endpoint.QueryString);
            return await _apiProcessor.ProcessGetRequest<PositionsResponse>(endpoint, _recWindow);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AssetsResponse> GetAssetInfo(GetAssetInfoRequest request)
        {
            var endpoint = Endpoints.Account.GetAssetInfo(request, IsTestMode);
            _requestClient.SetSignature(SecretHeader, TimestampHeader, _secretKey, endpoint.QueryString);
            return await _apiProcessor.ProcessGetRequest<AssetsResponse>(endpoint, _recWindow);
        }


        /// <summary>
        /// Queries the current account information
        /// </summary>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<AccountWalletResponse> GetAccountWalletInformation(GetAssetInfoRequest request)
        {
            var endpoint = Endpoints.Account.AccountWalletInformation(request, IsTestMode);
            _requestClient.SetSignature(SecretHeader, TimestampHeader, _secretKey, endpoint.QueryString);
            return await _apiProcessor.ProcessGetRequest<AccountWalletResponse>(endpoint, _recWindow);
        }


        /// <summary>
        /// Queries the all trades against this account
        /// </summary>
        /// <param name="request"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<List<AccountTradeReponse>> GetAccountTrades(AllTradesRequest request, int receiveWindow = -1)
        {
            receiveWindow = SetReceiveWindow(receiveWindow);
            return await _apiProcessor.ProcessGetRequest<List<AccountTradeReponse>>(Endpoints.Account.AccountTradeList(request), receiveWindow);
        }


        private int SetReceiveWindow(int receiveWindow)
        {
            if (receiveWindow == -1)
            {
                receiveWindow = _defaultReceiveWindow;
            }

            return receiveWindow;
        }

        #endregion

        Guid _userDataStremGuid;




        private async void PrivateOrderMessageHandler(BybitOrderResponse response)
        {
            if (response != null)
            {
                if (response.Orders.Any())
                {
                    foreach (var changedOrder in response.Orders)
                    {
                        try
                        {
                            var obj = changedOrder;
                            NativeOrder order = null;

                            if (_orderCollection.ContainsKey(obj.OrderLinkId))
                            {
                                order = _orderCollection[obj.OrderLinkId];
                            }


                            if (order != null)
                            {


                                if (order.OrderType == OrderType.Market)
                                    order.Quantity = (double)obj.CumExecQty;
                                else
                                    order.Quantity = (double)obj.Qty;

                                order.OrderState = BybitOrderStatusToNative(obj.OrderStatus);

                                if (order.OrderState == Core.Enums.OrderState.Filled)
                                {
                                    order.OrderFillDate = obj.UpdatedTime;
                                    order.FillPrice = (double)obj.AvgPrice;
                                }
                                else
                                {
                                    if (order.OrderType == OrderType.Limit)
                                    {
                                        order.LimitPrice = (double)obj.Price;
                                    }

                                    if (order.OrderType == OrderType.StopLimit)
                                    {
                                        order.LimitPrice = (double)obj.TriggerPrice;
                                        order.StopPrice = (double)obj.Price;
                                    }

                                    if (order.OrderType == OrderType.StopMarket)
                                    {
                                        order.StopPrice = (double)obj.TriggerPrice;
                                    }
                                }


                                OrderStatusChanged.Invoke(this, new OrderStatusChangedResponse()
                                {
                                    Order = order
                                });
                            }

                        }
                        catch (Exception ex)
                        {
                            var t = 0;
                        }
                    }
                }
            }

        }


        private async void PrivatePositionMessageHandler(BybitPositionResponse response)
        {
            if (response != null)
            {
                if (response.Positions.Any())
                {
                    foreach (var position in response.Positions)
                    {
                        try
                        {
                            NativePosition pos;
                            if (!_positionCollection.ContainsKey(position.Symbol))
                            {
                                pos = new NativePosition();
                                _positionCollection.Add(position.Symbol, pos);
                            }
                            else
                            {
                                pos = _positionCollection[position.Symbol];
                            }

                            pos.Quantity = (double)position.Size;
                            pos.MarketPosition = position.Side == OrderSide.Buy ? MarketPosition.Long : MarketPosition.Short;
                            pos.Quantity = Math.Abs(pos.Quantity);
                            pos.Instrument = await GetNativeInstrument(position.Symbol, "FUTURE");
                            if (pos.Instrument == null)
                            {
                                var connection = Session.Instance.GetConnection(ConnectionId);
                                if (connection != null && connection is ConnectionBase cb)
                                {
                                    pos.Instrument = await cb.CreateInstrument(position.Symbol, "FUTURE");
                                }

                            }



                            pos.AverageEntryPrice = (double)position.EntryPrice;


                            if (pos.Instrument != null)
                            {
                                if (pos.AverageEntryPrice == 0)
                                {
                                    var dataSeriesParams = new DataSeriesParamsEmpty() { DataSeriesType = DataSeriesType.Minute, DataSeriesValue = 1, Instrument = pos.Instrument };
                                    UnSubscribeFromLive(GetLiveDataRequest(dataSeriesParams), UpdateLive);
                                    pos.Unrealized = 0;
                                }
                                else
                                {
                                    var dataSeriesParams = new DataSeriesParamsEmpty() { DataSeriesType = DataSeriesType.Minute, DataSeriesValue = 1, Instrument = pos.Instrument };
                                    SubscribeToLive(GetLiveDataRequest(dataSeriesParams), UpdateLive);
                                }
                            }

                            PositionChanged?.Invoke(this, new PositionChangedResponse()
                            {
                                Position = pos
                            });

                        }
                        catch (Exception ex)
                        {
                            var t = 0;
                        }
                    }
                }
            }

        }


        private async void PrivateWalletMessageHandler(BybitWalletResponse response)
        {
            if (response != null)
            {
                if (response.WalletInfos.Any())
                {
                    var walletInfo = response.WalletInfos.FirstOrDefault();

                    foreach (var coin in walletInfo.Coins)
                    {
                        try
                        {
                            NativeAsset asset;
                            if (!_assetCollection.ContainsKey(coin.Coin))
                            {
                                if (coin.WalletBalance == 0)
                                    continue;

                                asset = new NativeAsset();
                                _assetCollection.Add(coin.Coin, asset);
                            }
                            else
                            {
                                asset = _assetCollection[coin.Coin];
                            }

                            asset.AssetName = coin.Coin;
                            asset.Balance = (double)coin.WalletBalance;

                            AssetChanged?.Invoke(this, new AssetChangedResponse()
                            {
                                Asset = asset
                            });

                        }
                        catch (Exception ex)
                        {
                            var t = 0;
                        }
                    }



                    AccountChanged?.Invoke(this, new AccountChangedResponse()
                    {
                        Realized = (double)walletInfo.TotalEquity,
                        Unrealized = (double)walletInfo.TotalPerpUPL
                    }); ;


                }
            }

        }


        private void DepthMessageHandler(BybitOrderBookResponse response)
        {
            if (response != null)
            {

                ConcurrentDictionary<string, OrderBook> symbolOrderBooks;
                List<Action<string, OrderBookResult>> depthCallbacks;
                var type = response.Category;
                if (response.Category == "linear")
                {
                    symbolOrderBooks = _futuresOrderBooks;
                    depthCallbacks = _depthFuturesCallbacks[response.Orders.Symbol];
                }
                else
                {
                    symbolOrderBooks = _spotOrderBooks;
                    depthCallbacks = _depthSpotCallbacks[response.Orders.Symbol];
                }


                OrderBook orderBook;
                if (!symbolOrderBooks.ContainsKey(response.Orders.Symbol))
                {
                    orderBook = new OrderBook();
                    symbolOrderBooks.TryAdd(response.Orders.Symbol, orderBook);
                }
                else
                {
                    orderBook = symbolOrderBooks[response.Orders.Symbol];
                }

                var localTime = response.Cts.ToLocalTime();

                if (response.Type == "snapshot")
                {
                    foreach (var order in response.Orders.Asks)
                    {
                        var price = order.Price;
                        var quantity = order.Size;
                        orderBook.Insert(MarketDataType.Ask, price, quantity);
                    }

                    foreach (var order in response.Orders.Bids)
                    {
                        var price = order.Price;
                        var quantity = order.Size;
                        orderBook.Insert(MarketDataType.Bid, price, quantity);
                    }

                    lock (_depthLock)
                    {
                        foreach (var callback in depthCallbacks)
                        {
                            callback?.Invoke(response.Orders.Symbol, new OrderBookResult() { Snapshot = orderBook, IsNewBook = true });
                        }
                    }

                }
                else
                {
                    foreach (var order in response.Orders.Asks)
                    {
                        var price = order.Price;
                        var quantity = order.Size;
                        orderBook.Update(MarketDataType.Ask, price, quantity);
                        
                    }

                    foreach (var order in response.Orders.Bids)
                    {
                        var price = order.Price;
                        var quantity = order.Size;
                        orderBook.Update(MarketDataType.Bid, price, quantity);                        
                    }

                    var priceLevels = new OrderBookResult() { IsIncrement = true, Snapshot = orderBook };

                    lock (_depthLock)
                    {
                        foreach (var callback in depthCallbacks)
                        {

                            callback?.Invoke(response.Orders.Symbol, priceLevels);
                        }
                    }
                }
            }
        }


        #endregion

        #region IDataProviderClient


        public async Task<bool> Connect(IRequest request)
        {
            try
            {
                var states = new List<bool>();

                _orderCollection = new Dictionary<string, NativeOrder>();
                _positionCollection = new Dictionary<string, Position>();
                _assetCollection = new Dictionary<string, NativeAsset>();

                if (_bybitWebSocketClient == null)
                    _bybitWebSocketClient = new DisposableBybitWebSocketClient(this);

                if (!string.IsNullOrEmpty(_apiKey))
                    await ConnectToUserDataWebSocket();


                return true;
            }
            catch (Exception ex)
            {
                Message = ex.Message;
                return false;
            }
        }

        public async Task<Guid> ConnectToUserDataWebSocket()
        {

            _userDataStremGuid = await _bybitWebSocketClient.ConnectToUserDataWebSocket(_userDataWebSocketMessages, IsTestMode);


            long expires = DateTimeOffset.Now.ToUnixTimeMilliseconds() + 10000;
            string _val = $"GET/realtime{expires}";

            var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey));
            var hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(_val));
            string signature = BitConverter.ToString(hash).Replace("-", "").ToLower();

            _bybitWebSocketClient.Auth(_userDataStremGuid, $"\"{_apiKey}\", \"{expires}\", \"{signature}\"");
            _bybitWebSocketClient.Subscribe(_userDataStremGuid, $"order");
            _bybitWebSocketClient.Subscribe(_userDataStremGuid, $"position");
            _bybitWebSocketClient.Subscribe(_userDataStremGuid, $"wallet");



            return _userDataStremGuid;
        }


        public void DisconnectUserDataWebSocket()
        {
            _bybitWebSocketClient.CloseWebSocketInstance(_userDataStremGuid);
        }


        public async void ConnectToMarketDataWebSocket()
        {

        }


        public void UnsubcribeMarketDataWebSockets()
        {

            var temp = new ConcurrentDictionary<Action<string, IEnumerable<ICandle>>, IRequest>();


            foreach (var callbackRequest in _klineCallbackRequests)
            {
                temp.TryAdd(callbackRequest.Key, callbackRequest.Value);
            }


            foreach (var callbackRequest in temp)
            {
                UnSubscribeFromLive(callbackRequest.Value, callbackRequest.Key);
            }

            _klineCallbackRequests = temp;
        }


        public void SubcribeMarketDataWebSockets()
        {
            foreach (var callbackRequest in _klineCallbackRequests)
            {
                SubscribeToLive(callbackRequest.Value, callbackRequest.Key);
            }
        }



        public async void InitAccount()
        {
            try
            {
                await InitOrders();
                await InitPositions();
                await InitWallet();
                await InitAssets();
            }
            catch (BybitException be)
            {
                MessageBox.Show("Init account error: " + be.ErrorDetails.Message);
            }
            catch (Exception e)
            {
                MessageBox.Show("Init account error: " + e.Message);
            }
        }


        private async Task InitOrders()
        {
            var currentSpotOpenOrdersRequest = new CurrentOpenOrdersRequest();
            currentSpotOpenOrdersRequest.Category = "spot";
            var allSpotOpenOrders = await GetCurrentOpenOrders(currentSpotOpenOrdersRequest);
            var list = allSpotOpenOrders.Result.List;

            foreach (var order in list)
            {
                await CreateLocalOrder(order, "spot");
            }

            var currentLinearOpenOrdersRequest = new CurrentOpenOrdersRequest();
            currentLinearOpenOrdersRequest.Category = "linear";
            currentLinearOpenOrdersRequest.SettleCoin = "USDT";
            var allLinearOpenOrders = await GetCurrentOpenOrders(currentLinearOpenOrdersRequest);
            list = allLinearOpenOrders.Result.List;

            foreach (var order in list)
            {
                await CreateLocalOrder(order, "future");
            }



        }

        private async Task CreateLocalOrder(OrderResult order, string category)
        {
            var nativeOrder = new NativeOrder();
            nativeOrder.OrderState = BybitOrderStatusToNative(order.OrderStatus);
            nativeOrder.OrderAction = BybitOrderActionToNative(order.Side);
            nativeOrder.Guid = order.OrderLinkId;
            nativeOrder.Instrument = await GetNativeInstrument(order.Symbol, category);
            nativeOrder.Sid = order.OrderId;
            nativeOrder.OrderInitDate = order.CreatedTime;
            nativeOrder.Quantity = (double)order.Qty;

            if (order.OrderType == BybitOrderType.Market)
            {

                nativeOrder.OrderType = OrderType.Market;

                if (order.TriggerPrice != null)
                {
                    nativeOrder.OrderType = OrderType.StopMarket;
                    nativeOrder.StopPrice = (double)order.TriggerPrice;
                }

            }


            if (order.OrderType == BybitOrderType.Limit)
            {
                nativeOrder.OrderType = OrderType.Limit;

                if (order.TriggerPrice != null)
                {
                    nativeOrder.OrderType = OrderType.StopLimit;
                    nativeOrder.LimitPrice = (double)order.Price;
                    nativeOrder.StopPrice = (double)order.TriggerPrice;


                }
            }


            _orderCollection.Add(nativeOrder.Guid, nativeOrder);

            OrderStatusChanged.Invoke(this, new OrderStatusChangedResponse()
            {
                Order = nativeOrder
            });
        }


        private async Task InitPositions()
        {
            var currentPositionInformationRequest = new CurrentPositionInformationRequest();
            currentPositionInformationRequest.Category = "linear";
            currentPositionInformationRequest.SettleCoin = "USDT";
            var positionsResponse = await GetCurrentPositions(currentPositionInformationRequest);

            if (positionsResponse.Result != null)
            {
                var positions = positionsResponse.Result.List.Where(x => x.Size != 0);
                foreach (var position in positions)
                {
                    var nativePosition = new NativePosition();

                    _positionCollection.Add(position.Symbol, nativePosition);
                    nativePosition.Quantity = (double)position.Size;
                    nativePosition.MarketPosition = nativePosition.Quantity < 0 ? MarketPosition.Short : MarketPosition.Long;
                    nativePosition.Quantity = Math.Abs(nativePosition.Quantity);
                    nativePosition.Instrument = await GetNativeInstrument(position.Symbol, "FUTURE");
                    if (nativePosition.Instrument == null)
                    {
                        var connection = Session.Instance.GetConnection(this.ConnectionId);
                        if (connection != null && connection is ConnectionBase cb)
                        {
                            nativePosition.Instrument = await cb.CreateInstrument(position.Symbol, "FUTURE");
                        }

                    }
                    nativePosition.AverageEntryPrice = (double)position.AvgPrice;
                    // nativePosition.Unrealized = (double)position.;

                    PositionChanged.Invoke(this, new PositionChangedResponse()
                    {
                        Position = nativePosition
                    });


                    var dataSeriesParams = new DataSeriesParamsEmpty() { DataSeriesType = DataSeriesType.Minute, DataSeriesValue = 1, Instrument = nativePosition.Instrument };
                    SubscribeToLive(GetLiveDataRequest(dataSeriesParams), UpdateLive);
                }


                /* AccountChanged?.Invoke(this, new AccountChangedResponse()
                 {
                     Realized = (double)positionsResponse.Result.List.Sum(x => x.PositionBalance)
                 }); */
            }

        }



        private async Task InitWallet()
        {
            var getAssetInfoRequest = new GetAssetInfoRequest();
            getAssetInfoRequest.AccountType = "UNIFIED";

            var assetsResponse = await GetAccountWalletInformation(getAssetInfoRequest);

            if (assetsResponse.Result != null && assetsResponse.Result.List != null)
            {
                var walletInfo = assetsResponse.Result.List.FirstOrDefault();

                AccountChanged?.Invoke(this, new AccountChangedResponse()
                {
                    Unrealized = (double)walletInfo.TotalPerpUPL,
                    Realized = (double)walletInfo.TotalEquity
                });

            }

        }

        private async Task InitAssets()
        {
            var getAssetInfoRequest = new GetAssetInfoRequest();
            getAssetInfoRequest.AccountType = "UNIFIED";

            var assetsResponse = await GetAssetInfo(getAssetInfoRequest);

            if (assetsResponse.Result != null && assetsResponse.Result.Assets != null)
            {
                var assets = assetsResponse.Result.Assets.Where(x => x.WalletBalance != 0);
                foreach (var asset in assets)
                {
                    var nativeAsset = new NativeAsset();

                    _assetCollection.Add(asset.Coin, nativeAsset);
                    nativeAsset.Balance = (double)asset.WalletBalance;
                    nativeAsset.AssetName = asset.Coin;

                    AssetChanged?.Invoke(this, new AssetChangedResponse()
                    {
                        Asset = nativeAsset
                    });
                }
            }

        }


        private void UpdateLive(string symbol, IEnumerable<ICandle> ticks)
        {

            if (_positionCollection.ContainsKey(symbol))
            {
                foreach (var tick in ticks)
                {
                    var pos = _positionCollection[symbol];
                    pos.Unrealized = pos.Quantity * (pos.MarketPosition == MarketPosition.Long ? 1 : -1) * (tick.C - pos.AverageEntryPrice);

                    PositionChanged?.Invoke(this, new PositionChangedResponse()
                    {
                        Position = pos
                    });


                }
            }


            AccountChanged?.Invoke(this, new AccountChangedResponse()
            {
                Unrealized = _positionCollection.Sum(x => x.Value.Unrealized) ?? 0
            });




        }


        /*private async void AccountUpdate(Twm.Core.DataProviders.Bybit.Models.WebSocket.Futures.BybitAccountUpdateData accountUpdateData)
        {
            if (accountUpdateData.BybitAccountData.Positions.Any())
            {

                foreach (var position in accountUpdateData.BybitAccountData.Positions)
                {

                    NativePosition pos;
                    if (!_positionCollection.ContainsKey(position.Symbol))
                    {
                        pos = new NativePosition();
                        _positionCollection.Add(position.Symbol, pos);
                    }
                    else
                    {
                        pos = _positionCollection[position.Symbol];
                    }

                    pos.Quantity = (double)position.PositionAmount;
                    pos.MarketPosition = pos.Quantity < 0 ? MarketPosition.Short : MarketPosition.Long;
                    pos.Quantity = Math.Abs(pos.Quantity);
                    pos.Instrument = await GetNativeInstrument(position.Symbol);
                    pos.AverageEntryPrice = (double)position.EntryPrice;



                    if (pos.AverageEntryPrice == 0)
                    {
                        var dataSeriesParams = new DataSeriesParamsEmpty() { DataSeriesType = DataSeriesType.Minute, DataSeriesValue = 1, Instrument = pos.Instrument };
                        UnSubscribeFromLive(GetLiveDataRequest(dataSeriesParams), UpdateLive);
                        pos.Unrealized = 0;
                    }
                    else
                    {
                        var dataSeriesParams = new DataSeriesParamsEmpty() { DataSeriesType = DataSeriesType.Minute, DataSeriesValue = 1, Instrument = pos.Instrument };
                        SubscribeToLive(GetLiveDataRequest(dataSeriesParams), UpdateLive);
                    }

                    PositionChanged?.Invoke(this, new PositionChangedResponse()
                    {
                        Position = pos
                    });
                }


                if (accountUpdateData.BybitAccountData.Balances.Any())
                {
                    AccountChanged?.Invoke(this, new AccountChangedResponse()
                    {
                        Realized = (double)accountUpdateData.BybitAccountData.Balances[0].WalletBalabce,
                        Unrealized = _positionCollection.Sum(x => x.Value.Unrealized) ?? 0
                    });
                }

            }



        }*/

        /*private void OrderUpdate(Twm.Core.DataProviders.Bybit.Models.WebSocket.Futures.BybitTradeOrderData tradeOrderData)
        {
            try
            {
                var obj = tradeOrderData.TradeOrderObjectData;
                if (_orderCollection.ContainsKey(obj.NewClientOrderId))
                {
                    var order = _orderCollection[obj.NewClientOrderId];
                    order.FillPrice = (double)obj.PriceOfLastFilledTrade;

                    if (order.OrderType == OrderType.Market)
                        order.Quantity = (double)obj.AccumulatedQuantityOfFilledTradesThisOrder;
                    else
                        order.Quantity = (double)obj.Quantity;

                    order.OrderState = BybitOrderStatusToNative(obj.OrderStatus);

                    if (order.OrderState == Core.Enums.OrderState.Filled)
                        order.OrderFillDate = obj.OrderTradeTime;


                    OrderStatusChanged.Invoke(this, new OrderStatusChangedResponse()
                    {
                        Order = order
                    });
                }
            }
            catch (Exception ex)
            {
                var t = 0;
            }
        }

        private void TradeUpdate(Twm.Core.DataProviders.Bybit.Models.WebSocket.Futures.BybitTradeOrderData tradeOrderData)
        {

        }*/




        public async Task<IResponse> GetInstruments(IRequest request)
        {
            var instrumentResponse = new Models.Response.InstrumentsResponse();
            try
            {
                instrumentResponse.Instruments = new List<Models.BybitInstrument>();
                request = new GetInstrumentsRequest() { Category = "linear" };
                var resultUSDM = await GetInstrumentsInfo(request, MarketType.UsdM);

                foreach (var symbol in resultUSDM.Result.List)
                {
                    instrumentResponse.Instruments.Add(CreateInsrument(symbol, GetTickSize(symbol), MarketType.UsdM));
                };

                request = new GetInstrumentsRequest() { Category = "spot" };
                var resultSpot = await GetInstrumentsInfo(request, MarketType.Spot);

                foreach (var symbol in resultSpot.Result.List)
                {
                    instrumentResponse.Instruments.Add(CreateInsrument(symbol, GetTickSize(symbol), MarketType.Spot));
                };
            }
            catch (Exception ex)
            {

            }
            return instrumentResponse;
        }


        public async Task<IResponse> GetTickers(IRequest request)
        {
            var tickerResponse = new Models.Response.InstrumentTickersResponse();
            try
            {

                tickerResponse.InstrumentTickers = new List<Models.InstrumentTicker>();

                request = new GetTickersRequest() { Category = "linear" };
                var resultUSDM = await GetTickers(request, MarketType.UsdM);

                foreach (var ticker in resultUSDM.Result.List)
                {
                    var instrumentTicker = new Models.InstrumentTicker()
                    {
                        Symbol = ticker.Symbol
                    };

                    if (double.TryParse(ticker.LastPrice, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                    {
                        instrumentTicker.LastPrice = result;
                    }

                    if (double.TryParse(ticker.Volume24h, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out var result2))
                    {
                        instrumentTicker.Volume = result2;
                    }


                    tickerResponse.InstrumentTickers.Add(instrumentTicker);
                };

                request = new GetTickersRequest() { Category = "spot" };
                var resultSpot = await GetTickers(request, MarketType.Spot);

                foreach (var ticker in resultSpot.Result.List)
                {
                    var instrumentTicker = new Models.InstrumentTicker()
                    {
                        Symbol = ticker.Symbol
                    };

                    if (double.TryParse(ticker.LastPrice, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                    {
                        instrumentTicker.LastPrice = result;
                    }

                    if (double.TryParse(ticker.Volume24h, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out var result2))
                    {
                        instrumentTicker.Volume = result2;
                    }


                    tickerResponse.InstrumentTickers.Add(instrumentTicker);
                };

            }
            catch
            {

            }
            return tickerResponse;
        }


        private Models.BybitInstrument CreateInsrument(InstrumentInfo symbol, double tickSize, MarketType mt)
        {
            var minLotSize = 0.0;
            var notional = 0.0;
            var lotSizeFilter = symbol.LotSizeFilter;
            if (lotSizeFilter != null)
            {
                if (double.TryParse(lotSizeFilter.MinOrderQty, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                {
                    minLotSize = result;
                }

                if (double.TryParse(lotSizeFilter.MinNotionalValue, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out var result2))
                {
                    notional = result2;
                }


            }




            return new Models.BybitInstrument()
            {
                Symbol = symbol.Symbol,
                ContractType = symbol.ContractType,
                Status = symbol.Status,
                BaseAsset = symbol.BaseCoin,
                QuoteAsset = symbol.QuoteCoin,
                MinLotSize = minLotSize,
                Notional = notional,
                Type = mt,
                TickSize = tickSize,

            };
        }

        private double GetTickSize(InstrumentInfo symbol)
        {
            var tickSize = 0.0;
            if (symbol.PriceFilter != null)
            {
                if (double.TryParse(symbol.PriceFilter.TickSize, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                {


                    tickSize = result;
                }
            }
            return tickSize;
        }



        public async Task<IResponse> FindInstruments(IRequest request)
        {
            //Not used
            return null;
        }


        public async Task<object> GetInstrument(IRequest request)
        {
            return null;
        }



        public async Task<bool> SubscribeToOrderRouting(IRequest request)
        {
            return true;
        }




        public async void SubmitOrder(IRequest request)
        {
            try
            {
                if (request is SubmitOrderRequest submitOrderRequest)
                {

                    SubmitOrder(submitOrderRequest.Order);
                }

            }
            catch (BybitException bex)
            {
                throw new Exception(bex.Message + Environment.NewLine + bex.ErrorDetails);
            }

        }


        private async void SubmitOrder(NativeOrder order)
        {
            var createOrderRequest = new CreateOrderRequest();

            //createOrderRequest.OrderType = NativeOrderTypeToBybit(order.OrderType);
            createOrderRequest.Side = NativeOrderActionToBybit(order.OrderAction);
            createOrderRequest.Qty = (decimal)order.Quantity;
            createOrderRequest.Symbol = order.Instrument.Symbol;
            createOrderRequest.MarketUnit = "baseCoin";

            if (order.OrderType == OrderType.Market || order.OrderType == OrderType.StopMarket)
            {
                createOrderRequest.OrderType = BybitOrderType.Market;

                if (order.OrderType == OrderType.StopMarket)
                {
                    createOrderRequest.OrderFilter = "StopOrder";

                    createOrderRequest.TriggerPrice = (decimal)order.StopPrice;
                    if (createOrderRequest.Side == OrderSide.Buy)
                        createOrderRequest.TriggerDirection = 1;
                    else
                        createOrderRequest.TriggerDirection = 2;
                }
            }

            if (order.OrderType == OrderType.Limit || order.OrderType == OrderType.StopLimit)
            {
                createOrderRequest.OrderType = BybitOrderType.Limit;
                createOrderRequest.Price = (decimal)order.LimitPrice;

                if (order.OrderType == OrderType.Limit)
                {
                    createOrderRequest.TimeInForce = TimeInForce.GTC;
                }

                if (order.OrderType == OrderType.StopLimit)
                {
                    createOrderRequest.OrderFilter = "StopOrder";

                    createOrderRequest.TriggerPrice = (decimal)order.LimitPrice;
                    if (createOrderRequest.Side == OrderSide.Buy)
                        createOrderRequest.TriggerDirection = 1;
                    else
                        createOrderRequest.TriggerDirection = 2;
                }
            }


            createOrderRequest.OrderLinkId = order.Guid;

            createOrderRequest.Category = order.Instrument.Type == "FUTURE" ? "linear" : "spot";

            _orderCollection.Add(createOrderRequest.OrderLinkId, order);

            try
            {
                var response = await CreateOrder(createOrderRequest);
                order.Sid = response.Result.OrderId;

                if (response.RetCode != 0)
                {
                    _orderCollection.Remove(createOrderRequest.OrderLinkId);
                    throw new Exception(response.RetMsg);
                }

            }
            catch (BybitException bex)
            {
                _orderCollection.Remove(createOrderRequest.OrderLinkId);
                throw new Exception(bex.Message + Environment.NewLine + bex.ErrorDetails);
            }
        }

        public async void ChangeOrder(IRequest request)
        {
            try
            {
                if (request is ChangeOrderRequest changeOrderRequest)
                {
                    /* if (changeOrderRequest.Order.OrderType == OrderType.StopMarket ||
                         changeOrderRequest.Order.OrderType == OrderType.StopLimit)*/
                    /*{
                        var newOrder = changeOrderRequest.Order.CloneTo(new NativeOrder());
                        await CancelOrder(changeOrderRequest.Order);
                        newOrder.Guid = Guid.NewGuid().ToString();
                        newOrder.Id = 0;
                        SubmitOrder(newOrder);

                    }
                    else
                    {*/
                    var modifyRequest = new ModifyOrderRequest();
                    modifyRequest.Category = GetByBitInstrumentCategory(changeOrderRequest.Order.Instrument);
                    //modifyRequest.OrderId = changeOrderRequest.Order.Sid;
                    modifyRequest.OrderLinkId = changeOrderRequest.Order.Guid;
                    modifyRequest.Qty = changeOrderRequest.Order.Quantity.ToString().Replace(',', '.');
                    modifyRequest.Symbol = changeOrderRequest.Order.Instrument.Symbol;


                    if (changeOrderRequest.Order.OrderType == OrderType.Limit)
                    {
                        modifyRequest.Price = changeOrderRequest.Order.LimitPrice.ToString().Replace(',', '.');
                    }

                    if (changeOrderRequest.Order.OrderType == OrderType.StopMarket)
                    {
                        modifyRequest.TriggerPrice = changeOrderRequest.Order.StopPrice.ToString().Replace(',', '.');
                    }

                    if (changeOrderRequest.Order.OrderType == OrderType.StopLimit)
                    {
                        modifyRequest.Price = changeOrderRequest.Order.LimitPrice.ToString().Replace(',', '.');
                        modifyRequest.TriggerPrice = changeOrderRequest.Order.LimitPrice.ToString().Replace(',', '.');
                    }


                    var orderResponse = await ModifyOrder(modifyRequest);

                    /* var orderResult = orderResponse.Result;



                     if (_orderCollection.ContainsKey(orderResult.OrderLinkId))
                     {
                         var order = _orderCollection[orderResult.OrderLinkId];
 *//*
                         if (order.OrderType == OrderType.StopMarket)
                         {
                             order.StopPrice = (double)orderResult.StopPrice;
                         }*//*

                         if (order.OrderType == OrderType.Limit)
                         {
                             order.LimitPrice = (double)orderResult.Price;
                         }

                         order.Quantity = (double)orderResult.;


                         OrderStatusChanged.Invoke(this, new OrderStatusChangedResponse()
                         {
                             Order = order
                         });
                     }*/





                }
            }
            catch (BybitException bex)
            {
                throw new Exception(bex.Message + Environment.NewLine + bex.ErrorDetails);
            }
        }


        public async void CancelOrder(IRequest request)
        {
            try
            {
                if (request is Common.Request.CancelOrderRequest cancelOrderRequest)
                {
                    await CancelOrder(cancelOrderRequest.Order);
                }
            }
            catch (BybitException bex)
            {
                throw new Exception(bex.Message + Environment.NewLine + bex.ErrorDetails);
            }
        }

        private async Task<CancelOrderResponse> CancelOrder(NativeOrder order)
        {
            var cancelOrder = new Models.Request.CancelOrderRequest();

            cancelOrder.Category = GetByBitInstrumentCategory(order.Instrument);
            cancelOrder.Symbol = order.Instrument.Symbol;
            cancelOrder.OrderId = order.Sid;
            cancelOrder.OrderLinkId = order.Guid;

            var response = await CancelOrder(cancelOrder);
            return response;
        }

        private string GetByBitInstrumentCategory(Instrument instrument)
        {
            if (instrument.Type == "FUTURE")
                return "linear";

            return "spot";
        }


        public async Task<IResponse> GetHistoricalData(IRequest request)
        {

            if (request is GetKlinesCandlesticksRequest klinesCandlesticksRequest)
            {
                HistoricalDataResponse response = new HistoricalDataResponse();
                response.Candles = new List<IHistoricalCandle>();

                DateTime stopDate = klinesCandlesticksRequest.Start ?? DateTime.MinValue.ToUniversalTime();
                var endTime = klinesCandlesticksRequest.End;
                klinesCandlesticksRequest.Start = null;
                while (true)
                {
                    if (endTime != null)
                    {
                        klinesCandlesticksRequest.End = endTime;
                    }

                    var klineResults = await GetKlinesCandlesticks(klinesCandlesticksRequest);


                    klineResults.Result.List.ForEach(k =>
                    {
                        response.Candles.Add(new HistoricalCandle()
                        {
                            Close = (double)k.Close,
                            High = (double)k.High,
                            Low = (double)k.Low,
                            Open = (double)k.Open,
                            Volume = (double)k.Volume,
                            /*CloseTime = k.CloseTime,*/
                            Time = k.OpenTime
                        });
                    });

                    if (!klineResults.Result.List.Any())
                    {
                        break;
                    }

                    endTime = klineResults.Result.List.Min(x => x.OpenTime).AddMinutes(-1);
                    if (endTime < stopDate)
                    {
                        response.Candles.RemoveAll(x => x.Time.Date < stopDate.Date);

                        break;
                    }

                }

                return response;
            }
            return new EmptyResponse();
        }






        public Task<IResponse> TestConnection(IRequest request)
        {
            return null;
        }




        private OrderSide NativeOrderActionToBybit(OrderAction orderAction)
        {
            switch (orderAction)
            {
                case OrderAction.Buy:
                    return OrderSide.Buy;

                case OrderAction.Sell:
                    return OrderSide.Sell;

                case OrderAction.SellShort:
                    return OrderSide.Sell;

                case OrderAction.BuyToCover:
                    return OrderSide.Buy;
            }

            throw new Exception("Unknown order action " + orderAction.Description() + " for Bybit request");
        }

        private OrderAction BybitOrderActionToNative(OrderSide orderAction)
        {
            switch (orderAction)
            {
                case OrderSide.Buy:
                    return OrderAction.Buy;

                case OrderSide.Sell:
                    return OrderAction.Sell;

            }

            throw new Exception($"Unknown order action {orderAction}");
        }

        private BybitOrderType NativeOrderTypeToBybit(OrderType orderType)
        {
            switch (orderType)
            {
                case OrderType.Limit:
                    return BybitOrderType.Limit;
                case OrderType.Market:
                    return BybitOrderType.Market;
                    /* case OrderType.StopMarket:
                         return BybitOrderType.StopMarket;
                     case OrderType.StopLimit:
                         return BybitOrderType.StopLimit;*/
            }

            throw new Exception("Unknown order type " + orderType.Description() + " for Bybit request");
        }


        private OrderType BybitOrderTypeToNative(BybitOrderType orderType)
        {
            switch (orderType)
            {
                case BybitOrderType.Limit:
                    return OrderType.Limit;
                case BybitOrderType.Market:
                    return OrderType.Market;
                    /*case BybitOrderType.StopMarket:
                        return OrderType.StopMarket;
                    case BybitOrderType.StopLimit:
                        return OrderType.StopLimit;*/
            }

            return OrderType.Unknown;
        }

        private Core.Enums.OrderState BybitOrderStatusToNative(OrderStatus orderStatus)
        {
            switch (orderStatus)
            {
                case OrderStatus.Untriggered:
                    return Core.Enums.OrderState.Working;
                case OrderStatus.New:
                    return Core.Enums.OrderState.Working;
                case OrderStatus.Rejected:
                    return Core.Enums.OrderState.Rejected;
                case OrderStatus.Deactivated:
                    return Core.Enums.OrderState.Cancelled;
                /*case "PreSubmitted":
                    return Core.Enums.OrderState.Working;
                case "Submitted":
                    return Core.Enums.OrderState.Working;
                case "ApiCancelled":
                    return Core.Enums.OrderState.CancelPending;*/
                case OrderStatus.Filled:
                    return Core.Enums.OrderState.Filled;
                /*case "Inactive":
                    return Core.Enums.OrderState.Rejected;*/
                case OrderStatus.Cancelled:
                    return Core.Enums.OrderState.Cancelled;
            }

            return Core.Enums.OrderState.Unknown;
        }

        private async Task<Instrument> GetNativeInstrument(string symbol, string type)
        {
            await Session.DbSemaphoreSlim.WaitAsync(new CancellationToken());
            try
            {

                using (var context = Session.Instance.DbContextFactory.GetContext())
                {
                    var repository = new InstrumentRepository(context);

                    var instrument = repository
                        .FindBy(x => x.ConnectionId == ConnectionId && x.Symbol == symbol && x.Type.ToUpper() == type.ToUpper())
                        .FirstOrDefault();

                    if (instrument == null)
                        LogController.Print(
                            $"Retrieve position information error. Can`t find native instrument {symbol} for connection {Session.Instance.GetConnection(ConnectionId).Name}");


                    return instrument;
                }


            }
            finally
            {
                Session.DbSemaphoreSlim.Release(1);
            }
        }




        private object _listLock = new object();
        private object _depthLock = new object();


        private async void KlineHandler(BybitKlineResponse data, List<Action<string, IEnumerable<ICandle>>> callbackList, string symbol)
        {
            var ticks = new List<Candle>();
            foreach (var kline in data.Klines)
            {
                ticks.Add(new Candle(TimeZoneInfo.ConvertTimeFromUtc(kline.StartTime, SystemOptions.Instance.TimeZone), (double)kline.Open, (double)kline.High, (double)kline.Low, (double)kline.Close, (double)kline.Volume, true, false, true));
            }


            lock (_listLock)
            {
                foreach (var cb in callbackList)
                {
                    cb.BeginInvoke(symbol, ticks.ToArray(), null, null);
                }
            }
        }


        private async void LiveHandler(BybitTradeData data, List<Action<string, double>> callbackList, string symbol)
        {
            lock (_listLock)
            {
                foreach (var cb in callbackList)
                {
                    cb.BeginInvoke(symbol, (double)data.LastPrice, null, null);
                }
            }
        }


        private void UpdatePostionPnl(List<PositionResponse> positionList)
        {
            foreach (var postion in positionList)
            {
                if (_positionCollection.ContainsKey(postion.Symbol))
                {

                    var pos = _positionCollection[postion.Symbol];
                    pos.Unrealized = (double)postion.UnRealizedProfit;

                    PositionChanged?.Invoke(this, new PositionChangedResponse()
                    {
                        Position = pos
                    });
                }
            }
        }

        public void SubscribeToLive(IRequest request, Action<string, IEnumerable<ICandle>> klineCallback)
        {

            if (request is LiveDataRequest liveDataRequest)
            {
                var mt = liveDataRequest.MarketType;
                var symbol = liveDataRequest.Symbol;

                if (_bybitWebSocketClient == null)
                    _bybitWebSocketClient = new DisposableBybitWebSocketClient(this);


                _klineCallbackRequests.TryAdd(klineCallback, request);

                if (!_klineSubscriptions.ContainsKey(symbol))
                {

                    var klineCallbackList = new List<Action<string, IEnumerable<ICandle>>> { klineCallback };
                    _klineCallbacks.TryAdd(symbol, klineCallbackList);



                    var guid = _bybitWebSocketClient.ConnectToKlineWebSocket(data => KlineHandler(data, klineCallbackList, symbol), mt, symbol, IsTestMode, liveDataRequest.KlineInterval);
                    _klineSubscriptions.TryAdd(symbol, guid);
                }
                else
                {
                    if (_klineCallbacks.TryGetValue(liveDataRequest.Symbol, out var callBackList))
                    {
                        lock (_listLock)
                        {
                            if (!callBackList.Contains(klineCallback))
                                callBackList.Add(klineCallback);
                        }
                    }
                }
            }
        }

        public void UnSubscribeFromLive(IRequest request, Action<string, IEnumerable<ICandle>> klineCallback)
        {
            if (request is LiveDataRequest liveDataRequest)
            {
                if (_klineCallbacks.ContainsKey(liveDataRequest.Symbol))
                {
                    if (_klineCallbacks.TryGetValue(liveDataRequest.Symbol, out var callbackList))
                    {

                        _klineCallbackRequests.TryRemove(klineCallback, out var result);

                        if (callbackList.Count > 1)
                        {
                            callbackList.Remove(klineCallback);
                        }
                        else
                        {
                            if (_klineCallbacks.TryRemove(liveDataRequest.Symbol, out var callbackListRemove))
                            {
                                callbackListRemove.Remove(klineCallback);
                                if (_klineSubscriptions.ContainsKey(liveDataRequest.Symbol))
                                {
                                    _klineSubscriptions.TryRemove(liveDataRequest.Symbol, out var guid);
                                    _bybitWebSocketClient.CloseWebSocketInstance(guid);



                                }
                            }
                        }
                    }
                }
            }

        }


        public void SubscribeToDepth(IRequest request, Action<string, OrderBookResult> depthCallback)
        {

            if (request is DepthDataRequest depthDataRequest)
            {
                var mt = depthDataRequest.MarketType;
                var symbol = depthDataRequest.Symbol;
                var levels = depthDataRequest.Levels;

                if (_bybitWebSocketClient == null)
                    _bybitWebSocketClient = new DisposableBybitWebSocketClient(this);

                _depthCallbackRequests.TryAdd(depthCallback, request);
                ConcurrentDictionary<string, Guid> depthSubscriptions;
                ConcurrentDictionary<string, List<Action<string, OrderBookResult>>> depthCallbacks;
                ConcurrentDictionary<string, OrderBook> symbolOrderBooks;
                if (mt == MarketType.UsdM)
                {
                    depthSubscriptions = _depthFuturesSubscriptions;
                    depthCallbacks = _depthFuturesCallbacks;
                    symbolOrderBooks = _futuresOrderBooks;
                }
                else
                {
                    depthSubscriptions = _depthSpotSubscriptions;
                    depthCallbacks = _depthSpotCallbacks;
                    symbolOrderBooks = _spotOrderBooks;
                }

                if (!depthSubscriptions.ContainsKey(symbol))
                {

                    var depthCallbackList = new List<Action<string, OrderBookResult>> { depthCallback };
                    lock (_depthLock)
                    {
                        depthCallbacks.TryAdd(symbol, depthCallbackList);
                    }

                    var guid = _bybitWebSocketClient.ConnectToDepthWebSocket(_userDataWebSocketMessages, mt, symbol, IsTestMode, depthDataRequest.Levels);
                    depthSubscriptions.TryAdd(symbol, guid);
                }
                else
                {
                    if (depthCallbacks.TryGetValue(depthDataRequest.Symbol, out var callBackList))
                    {
                        lock (_depthLock)
                        {
                            if (!callBackList.Contains(depthCallback))
                            {
                                if (symbolOrderBooks.TryGetValue(depthDataRequest.Symbol, out var orderBook))
                                    depthCallback.Invoke(depthDataRequest.Symbol, new OrderBookResult() { Snapshot = orderBook, IsNewBook = true });
                                callBackList.Add(depthCallback);
                                
                            }
                        }
                    }
                }
            }
        }




        public void UnSubscribeFromDepth(IRequest request, Action<string, OrderBookResult> depthCallback)
        {
            if (request is DepthDataRequest depthDataRequest)
            {

                ConcurrentDictionary<string, Guid> depthSubscriptions;
                ConcurrentDictionary<string, List<Action<string, OrderBookResult>>> depthCallbacks;
                ConcurrentDictionary<string, OrderBook> _orderBooks;
                if (depthDataRequest.MarketType == MarketType.UsdM)
                {
                    depthSubscriptions = _depthFuturesSubscriptions;
                    depthCallbacks = _depthFuturesCallbacks;
                    _orderBooks =_futuresOrderBooks;
                }
                else
                {
                    depthSubscriptions = _depthSpotSubscriptions;
                    depthCallbacks = _depthSpotCallbacks;
                    _orderBooks = _spotOrderBooks;
                }

                if (depthCallbacks.ContainsKey(depthDataRequest.Symbol))
                {
                    if (depthCallbacks.TryGetValue(depthDataRequest.Symbol, out var callbackList))
                    {

                        _depthCallbackRequests.TryRemove(depthCallback, out var result);

                        if (callbackList.Count > 1)
                        {
                            callbackList.Remove(depthCallback);
                        }
                        else
                        {
                            if (depthSubscriptions.ContainsKey(depthDataRequest.Symbol))
                            {
                                depthSubscriptions.TryRemove(depthDataRequest.Symbol, out var guid);
                                _bybitWebSocketClient.CloseWebSocketInstance(guid);
                            }

                            if (_orderBooks.ContainsKey(depthDataRequest.Symbol))
                            {
                                _orderBooks.TryRemove(depthDataRequest.Symbol, out var book);
                            }

                            if (depthCallbacks.TryRemove(depthDataRequest.Symbol, out var callbackListRemove))
                            {
                                callbackListRemove.Remove(depthCallback);
                                
                            }
                        }
                    }
                }
            }

        }


        public IRequest GetLiveDataRequest(DataSeriesParams dataSeriesParams)
        {
            if (dataSeriesParams.Instrument == null)
                return null;

            var request = new LiveDataRequest()
            {
                Symbol = dataSeriesParams.Instrument.Symbol
            };

            if (dataSeriesParams.Instrument.Type.ToUpper() == "SPOT")
                request.MarketType = MarketType.Spot;
            else
                request.MarketType = MarketType.UsdM;

            request.KlineInterval = GetKlineInterval(dataSeriesParams.DataSeriesValue, dataSeriesParams.DataSeriesType.ToAbbr());

            return request;

        }


        public IRequest GetDepthDataRequest(DataSeriesParams dataSeriesParams)
        {
            if (dataSeriesParams.Instrument == null)
                return null;

            var request = new DepthDataRequest()
            {
                Symbol = dataSeriesParams.Instrument.Symbol
            };

            if (dataSeriesParams.Instrument.Type.ToUpper() == "SPOT")
                request.MarketType = MarketType.Spot;
            else
                request.MarketType = MarketType.UsdM;

            request.Levels = 50;

            return request;

        }


        public IRequest GetHistoricalDataRequest(DataSeriesParams dataSeriesParams, DateTime startDate, DateTime endDate)
        {
            var request = new GetKlinesCandlesticksRequest();


            request.Interval = GetKlineInterval(dataSeriesParams.DataSeriesValue, dataSeriesParams.DataSeriesType.ToAbbr());
            request.Symbol = dataSeriesParams.Instrument.Symbol;
            request.Start = startDate;
            request.End = endDate;
            request.Limit = 1000;


            if (dataSeriesParams.Instrument.Type.ToUpper() == "FUTURE")
            {
                request.Category = "linear";
            }
            else
                request.Category = "spot";

            if (request.Limit == 0 || request.Limit > 1000)
            {
                request.Limit = 1000;
            }

            request.IsTestMode = IsTestMode;


            return request;
        }



        private KlineInterval GetKlineInterval(int dataSeriesValue, string dataSeriesType)
        {
            if (dataSeriesValue == 1 && dataSeriesType == "m")
                return KlineInterval.OneMinute;
            if (dataSeriesValue == 3 && dataSeriesType == "m")
                return KlineInterval.ThreeMinutes;
            if (dataSeriesValue == 5 && dataSeriesType == "m")
                return KlineInterval.FiveMinutes;
            if (dataSeriesValue == 15 && dataSeriesType == "m")
                return KlineInterval.FifteenMinutes;
            if (dataSeriesValue == 30 && dataSeriesType == "m")
                return KlineInterval.ThreeMinutes;

            if (dataSeriesValue == 1 && dataSeriesType == "h")
                return KlineInterval.OneHour;
            if (dataSeriesValue == 2 && dataSeriesType == "h")
                return KlineInterval.TwoHours;
            if (dataSeriesValue == 4 && dataSeriesType == "h")
                return KlineInterval.FourHours;
            if (dataSeriesValue == 6 && dataSeriesType == "h")
                return KlineInterval.SixHours;
            if (dataSeriesValue == 12 && dataSeriesType == "h")
                return KlineInterval.TwelveHours;

            if (dataSeriesValue == 1 && dataSeriesType == "d")
                return KlineInterval.OneDay;

            if (dataSeriesValue == 1 && dataSeriesType == "w")
                return KlineInterval.OneWeek;

            if (dataSeriesValue == 1 && dataSeriesType == "mo")
                return KlineInterval.OneMonth;

            return KlineInterval.OneMinute;
        }


        public async void Disconnect()
        {
            _positionCollection?.Clear();
            _orderCollection?.Clear();
            _assetCollection?.Clear();


            if (_bybitWebSocketClient != null && IsPrivate)
            {
                _bybitWebSocketClient.CloseWebSocketInstance(_userDataStremGuid, false);
                _bybitWebSocketClient.Dispose();
            }


        }


        #endregion



    }
}