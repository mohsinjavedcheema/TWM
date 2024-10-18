using Twm.Chart.Interfaces;
using Twm.Core.Classes;
using Twm.Core.Controllers;
using Twm.Core.DataProviders.Common;
using Twm.Core.DataProviders.Common.Request;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.DataProviders.Utility;
using Twm.Model.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NativeOrder = Twm.Core.Market.Order;
using NativePosition = Twm.Core.Market.Position;
using NativeAsset = Twm.Core.Market.Asset;
using OrderAction = Twm.Core.Enums.OrderAction;
using OrderType = Twm.Core.Enums.OrderType;
using Twm.Core.Helpers;
using Twm.Core.DataProviders.Common.Response;
using Twm.Core.DataProviders.Common.Caching;
using Twm.Core.DataProviders.Binance.Models.Response;
using Twm.Core.DataProviders.Binance.Models.Request;
using Twm.Core.DataProviders.Binance.Enums;
using Twm.Core.DataProviders.Binance.Models.Response.Abstract;
using BinanceOrderType = Twm.Core.DataProviders.Binance.Enums.OrderType;
using BinanceOrderStatus = Twm.Core.DataProviders.Binance.Enums.OrderStatus;
using Microsoft.CodeAnalysis;
using Twm.Core.Interfaces;
using Twm.Core.DataProviders.Binance.Models.Response.Custom;
using Twm.Core.DataProviders.Binance.Websockets;
using Twm.Core.DataProviders.Binance.Models.Request.Custom;
using System.Collections.Concurrent;
using Twm.Chart.Classes;
using Twm.Core.DataProviders.Binance.Models.WebSocket;
using Twm.Core.Market;
using Twm.Core.DataProviders.Binance.Models.Response.Error;
using SubmitOrderRequest = Twm.Core.DataProviders.Common.Request.SubmitOrderRequest;
using Twm.Core.Enums;
using Twm.DB.DAL.Repositories.Instruments;
using Twm.Core.DataProviders.Binance.Models.Response.Futures;
using Twm.Core.ViewModels.DataSeries;
using System.Windows;
using Twm.Core.DataProviders.Common.OrderBooks;
using Twm.Core.DataProviders.Enums;
using Twm.Core.DataProviders.Bybit.Websockets;





namespace Twm.Core.DataProviders.Binance
{
    public class BinanceClient : IDataProviderClient
    {
        public const int CONTRACT_ID_BASE = 60000000;
        public const int CONTRACT_DETAILS_ID = CONTRACT_ID_BASE + 1;
        public const int FUNDAMENTALS_ID = CONTRACT_ID_BASE + 2;



        private DisposableBinanceWebSocketClient _binanceWebSocketClient;


        private const string APIHeader = "X-MBX-APIKEY";
        private const string ReferenceHeader = "Referer";

        public TimeSpan TimestampOffset
        {
            get { return _timestampOffset; }
            set { _timestampOffset = value; }
        }

        private TimeSpan _timestampOffset;
        private readonly string _apiKey;
        private readonly string _secretKey;
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




        private ConcurrentDictionary<string, List<BinanceDepthData>> _futuresBuffers = new ConcurrentDictionary<string, List<BinanceDepthData>>();
        private ConcurrentDictionary<string, List<BinanceDepthData>> _spotBuffers = new ConcurrentDictionary<string, List<BinanceDepthData>>();


        private ConcurrentDictionary<string, OrderBook> _futuresOrderBooks = new ConcurrentDictionary<string, OrderBook>();
        private ConcurrentDictionary<string, OrderBook> _spotOrderBooks = new ConcurrentDictionary<string, OrderBook>();


        public int ConnectionId { get; set; }

        public EventHandler<IResponse> OrderStatusChanged { get; set; }
        public EventHandler<IResponse> AccountChanged { get; set; }
        public EventHandler<IResponse> PositionChanged { get; set; }
        public EventHandler<IResponse> AssetChanged { get; set; }

        public bool IsPrivate
        {
            get
            {
                return !string.IsNullOrEmpty(_apiKey);
            }
        }




        public event Action<int, int, string, Exception> Error;

        private Dictionary<string, NativeOrder> _orderCollection;
        private Dictionary<string, NativePosition> _positionCollection;
        private Dictionary<string, NativeAsset> _assetCollection;

        UserDataFuturesWebSocketMessages _userDataWebSocketFutMessages;
        UserDataSpotWebSocketMessages _userDataWebSocketSpotMessages;


        /// <summary>
        /// Create a new Interactive brokers Client based on the configuration provided
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="isTestMode"></param>
        public BinanceClient(ClientConfiguration configuration, IAPIProcessor apiProcessor = null, bool isTestMode = false)
        {
            Guard.AgainstNull(configuration);
            // Guard.AgainstNullOrEmpty(configuration.ApiKey, "API Key");
            // Guard.AgainstNull(configuration.SecretKey, "Secret Key");
            Guard.AgainstNull(configuration.Password);
            Guard.AgainstNull(configuration.UserName);
            Guard.AgainstNull(configuration.ConnectionId);

            _requestClient = new RequestClient();
            _defaultReceiveWindow = configuration.DefaultReceiveWindow;
            _apiKey = configuration.ApiKey;
            _secretKey = configuration.SecretKey;

            _requestClient.SetTimestampOffset(configuration.TimestampOffset);
            _requestClient.SetRateLimiting(configuration.EnableRateLimiting);
            _requestClient.SetAPIKey(APIHeader, _apiKey);
            _requestClient.SetReference(ReferenceHeader, configuration.Reference);

            ConnectionId = configuration.ConnectionId;
            ServerName = configuration.ServerName;
            IsTestMode = isTestMode;

            if (apiProcessor == null)
            {
                _apiProcessor = new BinanceAPIProcessor(_apiKey, _secretKey, new APICacheManager());
                _apiProcessor.SetCacheTime(configuration.CacheTime);
            }
            else
            {
                _apiProcessor = apiProcessor;
            }

            _apiProcessor.RequestClient = _requestClient;
        }


        #region Binance specific

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
        public async Task<ExchangeInfoResponse> GetExchangeInfo(MarketType mt)
        {
            return await _apiProcessor.ProcessGetRequest<ExchangeInfoResponse>(Endpoints.General.ExchangeInfo(mt, IsTestMode));
        }


        public async Task<TickersResponse> GetTicker(MarketType mt)
        {
            return await _apiProcessor.ProcessGetRequest<TickersResponse>(Endpoints.General.TickerInfo(mt, IsTestMode));
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
        public async Task<OrderBookResponse> GetOrderBook(MarketType mt, string symbol, bool useCache = false, int limit = 100)
        {
            Guard.AgainstNull(symbol);
            if (limit > 5000)
            {
                throw new ArgumentException("When requesting the order book, you can't request more than 5000 at a time.", nameof(limit));
            }
            return await _apiProcessor.ProcessGetRequest<OrderBookResponse>(Endpoints.MarketData.OrderBook(mt, IsTestMode, symbol, limit, useCache));
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
        public async Task<List<KlineCandleStickResponse>> GetKlinesCandlesticks(GetKlinesCandlesticksRequest request)
        {
            Guard.AgainstNull(request.Symbol);
            Guard.AgainstNull(request.Interval);

            if (request.Limit == 0 || request.Limit > 1000)
            {
                request.Limit = 1000;
            }

            return await _apiProcessor.ProcessGetRequest<List<KlineCandleStickResponse>>(Endpoints.MarketData.KlineCandlesticks(request));
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
        public async Task<BaseCreateOrderResponse> CreateOrder(MarketType mt, CreateOrderRequest request)
        {
            Guard.AgainstNull(request.Symbol);
            Guard.AgainstNull(request.Side);
            Guard.AgainstNull(request.Type);
            Guard.AgainstNull(request.Quantity);

            switch (request.NewOrderResponseType)
            {
                case NewOrderResponseType.Acknowledge:
                    return await _apiProcessor.ProcessPostRequest<AcknowledgeCreateOrderResponse>(Endpoints.Account.NewOrder(mt, request, IsTestMode));
                case NewOrderResponseType.Full:
                    return await _apiProcessor.ProcessPostRequest<FullCreateOrderResponse>(Endpoints.Account.NewOrder(mt, request, IsTestMode));
                default:
                    return await _apiProcessor.ProcessPostRequest<ResultCreateOrderResponse>(Endpoints.Account.NewOrder(mt, request, IsTestMode));
            }

        }

        /// <summary>
        /// Creates a test order based on the provided request
        /// </summary>
        /// <param name="request">The <see cref="CreateOrderRequest"/> that is used to define the order</param>
        /// <returns></returns>
        public async Task<EmptyResponse> CreateTestOrder(CreateOrderRequest request)
        {
            Guard.AgainstNull(request.Symbol);
            Guard.AgainstNull(request.Side);
            Guard.AgainstNull(request.Type);
            Guard.AgainstNull(request.Quantity);

            return await _apiProcessor.ProcessPostRequest<EmptyResponse>(Endpoints.Account.NewOrderTest(request));
        }


        public async Task<OrderResponse> ModifyOrder(MarketType mt, ModifyOrderRequest request, int receiveWindow = -1)
        {
            receiveWindow = SetReceiveWindow(receiveWindow);
            Guard.AgainstNull(request.Symbol);

            return await _apiProcessor.ProcessPutRequest<OrderResponse>(Endpoints.Account.ModifyOrder(mt, request, IsTestMode), receiveWindow);
        }

        /// <summary>
        /// Queries an order based on the provided request
        /// </summary>
        /// <param name="request">The <see cref="QueryOrderRequest"/> that is used to define the order</param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<OrderResponse> QueryOrder(QueryOrderRequest request, int receiveWindow = -1)
        {
            receiveWindow = SetReceiveWindow(receiveWindow);
            Guard.AgainstNull(request.Symbol);

            return await _apiProcessor.ProcessGetRequest<OrderResponse>(Endpoints.Account.QueryOrder(request), receiveWindow);
        }


        /// <summary>
        /// Cancels an order based on the provided request
        /// </summary>
        /// <param name="request">The <see cref="CancelOrderRequest"/> that is used to define the order</param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<CancelOrderResponse> CancelOrder(MarketType mt, Models.Request.CancelOrderRequest request, int receiveWindow = -1)
        {
            receiveWindow = SetReceiveWindow(receiveWindow);
            Guard.AgainstNull(request.Symbol);

            return await _apiProcessor.ProcessDeleteRequest<CancelOrderResponse>(Endpoints.Account.CancelOrder(mt, request, IsTestMode), receiveWindow);
        }

        /// <summary>
        /// Queries all orders based on the provided request
        /// </summary>
        /// <param name="request">The <see cref="CurrentOpenOrdersRequest"/> that is used to define the orders</param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<List<OrderResponse>> GetCurrentOpenOrders(MarketType mt, CurrentOpenOrdersRequest request, int receiveWindow = -1)
        {
            receiveWindow = SetReceiveWindow(receiveWindow);
            return await _apiProcessor.ProcessGetRequest<List<OrderResponse>>(Endpoints.Account.CurrentOpenOrders(mt, request, IsTestMode), receiveWindow);
        }

        /// <summary>
        /// Queries all orders based on the provided request
        /// </summary>
        /// <param name="request">The <see cref="AllOrdersRequest"/> that is used to define the orders</param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<List<OrderResponse>> GetAllOrders(AllOrdersRequest request, int receiveWindow = -1)
        {
            receiveWindow = SetReceiveWindow(receiveWindow);
            Guard.AgainstNull(request.Symbol);

            return await _apiProcessor.ProcessGetRequest<List<OrderResponse>>(Endpoints.Account.AllOrders(request), receiveWindow);
        }


        /// <summary>
        /// Queries current position information based on the provided request
        /// </summary>
        /// <param name="request">The <see cref="CurrentPositionInformationRequest"/> that is used to define the orders</param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<List<PositionResponse>> GetCurrentPositions(MarketType mt, CurrentPositionInformationRequest request, int receiveWindow = 30000)
        {
            receiveWindow = SetReceiveWindow(receiveWindow);
            return await _apiProcessor.ProcessGetRequest<List<PositionResponse>>(Endpoints.Account.CurrentPositions(mt, request, IsTestMode), receiveWindow);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AccountInformationResponse> GetAssetInfo(int receiveWindow = 30000)
        {
            return await _apiProcessor.ProcessGetRequest<AccountInformationResponse>(Endpoints.Account.GetSpotAccountInfo(IsTestMode), receiveWindow);
        }


        /// <summary>
        /// Queries the current account information
        /// </summary>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<AccountInformationResponse> GetAccountInformation(MarketType mt, int receiveWindow = -1)
        {
            receiveWindow = SetReceiveWindow(receiveWindow);
            return await _apiProcessor.ProcessGetRequest<AccountInformationResponse>(Endpoints.Account.AccountInformation(mt, IsTestMode), receiveWindow);
        }

        /// <summary>
        /// Queries the current account futures information
        /// </summary>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<Twm.Core.DataProviders.Binance.Models.WebSocket.Futures.AccountInformationResponse> GetAccountFututresInformation(MarketType mt, int receiveWindow = -1)
        {
            receiveWindow = SetReceiveWindow(receiveWindow);
            return await _apiProcessor.ProcessGetRequest<Twm.Core.DataProviders.Binance.Models.WebSocket.Futures.AccountInformationResponse>(Endpoints.Account.AccountInformation(mt, IsTestMode), receiveWindow);
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

        /// <summary>
        /// Sends a request to withdraw to an address
        /// </summary>
        /// <param name="request"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<WithdrawResponse> CreateWithdrawRequest(WithdrawRequest request, int receiveWindow = -1)
        {
            receiveWindow = SetReceiveWindow(receiveWindow);
            Guard.AgainstNullOrEmpty(request.Asset);
            Guard.AgainstNullOrEmpty(request.Address);
            Guard.AgainstNull(request.Amount);

            return await _apiProcessor.ProcessPostRequest<WithdrawResponse>(Endpoints.Account.Withdraw(request), receiveWindow);
        }

        /// <summary>
        /// Gets the Deposit history
        /// </summary>
        /// <param name="request"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<DepositListResponse> GetDepositHistory(FundHistoryRequest request, int receiveWindow = -1)
        {
            receiveWindow = SetReceiveWindow(receiveWindow);
            return await _apiProcessor.ProcessGetRequest<DepositListResponse>(Endpoints.Account.DepositHistory(request), receiveWindow);
        }

        /// <summary>
        /// Gets the Withdraw history
        /// </summary>
        /// <param name="request"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<WithdrawListResponse> GetWithdrawHistory(FundHistoryRequest request, int receiveWindow = -1)
        {
            receiveWindow = SetReceiveWindow(receiveWindow);
            Guard.AgainstNull(request);

            return await _apiProcessor.ProcessGetRequest<WithdrawListResponse>(Endpoints.Account.WithdrawHistory(request), receiveWindow);
        }

        /// <summary>
        /// Gets the the Deposit address
        /// </summary>
        /// <param name="request"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<DepositAddressResponse> DepositAddress(DepositAddressRequest request, int receiveWindow = -1)
        {
            receiveWindow = SetReceiveWindow(receiveWindow);
            Guard.AgainstNull(request);
            Guard.AgainstNullOrEmpty(request.Asset);

            return await _apiProcessor.ProcessGetRequest<DepositAddressResponse>(Endpoints.Account.DepositAddress(request), receiveWindow);
        }

        /// <summary>
        /// Returns the current Binance API System Status
        /// </summary>
        /// <param name="request"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<DepositAddressResponse> GetSystemStatus(int receiveWindow = -1)
        {
            receiveWindow = SetReceiveWindow(receiveWindow);
            return await _apiProcessor.ProcessGetRequest<DepositAddressResponse>(Endpoints.Account.SystemStatus(), receiveWindow);
        }
        #endregion

        private int SetReceiveWindow(int receiveWindow)
        {
            if (receiveWindow == -1)
            {
                receiveWindow = _defaultReceiveWindow;
            }

            return receiveWindow;
        }

        #endregion

        Guid _userDataStremGuidFut;

        Guid _userDataStremGuidSpot;



        #region IDataProviderClient


        public async Task<bool> Connect(IRequest request)
        {
            try
            {
                var states = new List<bool>();

                var resultUSDM = await _apiProcessor.ProcessGetRequest<EmptyResponse>(Endpoints.General.TestConnectivity(MarketType.UsdM, IsTestMode));
                var resultCoinM = await _apiProcessor.ProcessGetRequest<EmptyResponse>(Endpoints.General.TestConnectivity(MarketType.CoinM, IsTestMode));

                _orderCollection = new Dictionary<string, NativeOrder>();
                _positionCollection = new Dictionary<string, Position>();
                _assetCollection = new Dictionary<string, NativeAsset>();

                if (_binanceWebSocketClient == null)
                    _binanceWebSocketClient = new DisposableBinanceWebSocketClient(this);

                if (!string.IsNullOrEmpty(_apiKey))
                {
                    await ConnectToUserDataFutWebSocket();
                    if (!IsTestMode)
                        await ConnectToUserDataSpotWebSocket();
                }


                return true;
            }
            catch (Exception ex)
            {
                Message = ex.Message;
                return false;
            }
        }

        public async Task<Guid> ConnectToUserDataFutWebSocket()
        {
            _userDataWebSocketFutMessages = new UserDataFuturesWebSocketMessages();
            _userDataWebSocketFutMessages.AccountUpdateMessageHandler = AccountUpdate;
            _userDataWebSocketFutMessages.OrderUpdateMessageHandler = OrderUpdate;
            _userDataWebSocketFutMessages.TradeUpdateMessageHandler = TradeUpdate;
            _userDataStremGuidFut = await _binanceWebSocketClient.ConnectToUserDataWebSocket(MarketType.UsdM, null, _userDataWebSocketFutMessages, IsTestMode);
            return _userDataStremGuidFut;
        }


        public async Task<Guid> ConnectToUserDataSpotWebSocket()
        {
            _userDataWebSocketSpotMessages = new UserDataSpotWebSocketMessages();
            _userDataWebSocketSpotMessages.AccountUpdateMessageHandler = WalletUpdate;
            _userDataWebSocketSpotMessages.OrderUpdateMessageHandler = OrderUpdate;
            //_userDataWebSocketSpotMessages.TradeUpdateMessageHandler = TradeUpdate;
            _userDataStremGuidSpot = await _binanceWebSocketClient.ConnectToUserDataWebSocket(MarketType.Spot, _userDataWebSocketSpotMessages, null, IsTestMode);
            return _userDataStremGuidSpot;
        }


        public void DisconnectUserDataWebSocket()
        {
            _binanceWebSocketClient.CloseWebSocketInstance(_userDataStremGuidFut);
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
                await InitFutOrders();
                await InitPositions();
                if (!IsTestMode)
                    await InitSpotOrders();

                await InitAssets();

            }
            catch (BinanceException be)
            {
                MessageBox.Show("Init account error: " + be.ErrorDetails.Message);
            }
            catch (Exception e)
            {
                MessageBox.Show("Init account error: " + e.Message);
            }
        }


        private async Task InitFutOrders()
        {
            var currentOpenOrdersRequest = new CurrentOpenOrdersRequest();
            var allOpenOrders = await GetCurrentOpenOrders(MarketType.UsdM, currentOpenOrdersRequest);

            foreach (var order in allOpenOrders)
            {
                var nativeOrder = new NativeOrder();
                nativeOrder.OrderState = BinanceOrderStatusToNative(order.Status);
                nativeOrder.OrderType = BinanceOrderTypeToNative(order.Type);
                nativeOrder.OrderAction = BinanceOrderActionToNative(order.Side);
                nativeOrder.Guid = order.ClientOrderId;
                nativeOrder.Instrument = await GetNativeInstrument(order.Symbol, "FUTURE");
                nativeOrder.Id = order.OrderId;
                nativeOrder.OrderInitDate = order.Time;


                if (order.Type == BinanceOrderType.StopMarket)
                {
                    nativeOrder.StopPrice = (double)order.StopPrice;
                    nativeOrder.Quantity = (double)order.OriginalQuantity;
                }

                if (order.Type == BinanceOrderType.StopLimit)
                {
                    nativeOrder.StopPrice = (double)order.StopPrice;
                    nativeOrder.LimitPrice = (double)order.Price;
                    nativeOrder.Quantity = (double)order.OriginalQuantity;
                }

                if (order.Type == BinanceOrderType.Limit)
                {
                    nativeOrder.LimitPrice = (double)order.Price;
                    nativeOrder.Quantity = (double)order.OriginalQuantity;
                }


                _orderCollection.Add(nativeOrder.Guid, nativeOrder);

                OrderStatusChanged.Invoke(this, new OrderStatusChangedResponse()
                {
                    Order = nativeOrder
                });
            }
        }

        private async Task InitSpotOrders()
        {
            var currentOpenOrdersRequest = new CurrentOpenOrdersRequest();
            var allOpenOrders = await GetCurrentOpenOrders(MarketType.Spot, currentOpenOrdersRequest);

            foreach (var order in allOpenOrders)
            {
                var nativeOrder = new NativeOrder();
                nativeOrder.OrderState = BinanceOrderStatusToNative(order.Status);
                nativeOrder.OrderType = BinanceOrderTypeToNative(order.Type);
                nativeOrder.OrderAction = BinanceOrderActionToNative(order.Side);
                nativeOrder.Guid = order.ClientOrderId;
                nativeOrder.Instrument = await GetNativeInstrument(order.Symbol, "SPOT");
                nativeOrder.Id = order.OrderId;
                nativeOrder.OrderInitDate = order.Time;


                if (order.Type == BinanceOrderType.StopMarket)
                {
                    nativeOrder.StopPrice = (double)order.StopPrice;
                    nativeOrder.Quantity = (double)order.OriginalQuantity;
                }

                if (order.Type == BinanceOrderType.StopLimit)
                {
                    nativeOrder.StopPrice = (double)order.StopPrice;
                    nativeOrder.LimitPrice = (double)order.Price;
                    nativeOrder.Quantity = (double)order.OriginalQuantity;
                }

                if (order.Type == BinanceOrderType.Limit)
                {
                    nativeOrder.LimitPrice = (double)order.Price;
                    nativeOrder.Quantity = (double)order.OriginalQuantity;
                }


                _orderCollection.Add(nativeOrder.Guid, nativeOrder);

                OrderStatusChanged.Invoke(this, new OrderStatusChangedResponse()
                {
                    Order = nativeOrder
                });
            }
        }


        private async Task InitPositions()
        {
            var accountFuturesUnformation = await GetAccountFututresInformation(MarketType.UsdM);

            var positions = accountFuturesUnformation.Positions.Where(x => x.PositionAmt != 0);
            foreach (var position in positions)
            {
                var nativePosition = new NativePosition();

                _positionCollection.Add(position.Symbol, nativePosition);
                nativePosition.Quantity = (double)position.PositionAmt;
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

                nativePosition.AverageEntryPrice = (double)position.EntryPrice;
                nativePosition.Unrealized = (double)position.UnrealizedProfit;

                PositionChanged.Invoke(this, new PositionChangedResponse()
                {
                    Position = nativePosition
                });


                var dataSeriesParams = new DataSeriesParamsEmpty() { DataSeriesType = DataSeriesType.Minute, DataSeriesValue = 1, Instrument = nativePosition.Instrument };
                SubscribeToLive(GetLiveDataRequest(dataSeriesParams), UpdateLive);
            }


            AccountChanged?.Invoke(this, new AccountChangedResponse()
            {
                Realized = (double)accountFuturesUnformation.TotalWalletBalance,
                Unrealized = _positionCollection.Sum(x => x.Value.Unrealized) ?? 0
            });

        }



        private async Task InitAssets()
        {
            var accountInformationResponse = await GetAssetInfo();

            if (accountInformationResponse != null && accountInformationResponse.Balances != null)
            {
                var assets = accountInformationResponse.Balances.Where(x => x.Free != 0);
                foreach (var asset in assets)
                {
                    var nativeAsset = new NativeAsset();

                    _assetCollection.Add(asset.Asset, nativeAsset);
                    nativeAsset.Balance = (double)asset.Free;
                    nativeAsset.AssetName = asset.Asset;

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


        private async void AccountUpdate(Twm.Core.DataProviders.Binance.Models.WebSocket.Futures.BinanceAccountUpdateData accountUpdateData)
        {
            if (accountUpdateData.BinanceAccountData.Positions.Any())
            {

                foreach (var position in accountUpdateData.BinanceAccountData.Positions)
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


                if (accountUpdateData.BinanceAccountData.Balances.Any())
                {
                    AccountChanged?.Invoke(this, new AccountChangedResponse()
                    {
                        Realized = (double)accountUpdateData.BinanceAccountData.Balances[0].WalletBalabce,
                        Unrealized = _positionCollection.Sum(x => x.Value.Unrealized) ?? 0
                    });
                }

            }



        }


        private async void WalletUpdate(BinanceAccountUpdateData accountUpdateData)
        {
            if (accountUpdateData.Balances.Any())
            {

                foreach (var balance in accountUpdateData.Balances)
                {

                    NativeAsset asset;
                    if (!_assetCollection.ContainsKey(balance.Asset))
                    {
                        if (balance.Free == 0)
                            continue;

                        asset = new NativeAsset();
                        _assetCollection.Add(balance.Asset, asset);
                    }
                    else
                    {
                        asset = _assetCollection[balance.Asset];
                    }

                    asset.AssetName = balance.Asset;
                    asset.Balance = (double)balance.Free;




                    AssetChanged?.Invoke(this, new AssetChangedResponse()
                    {
                        Asset = asset
                    });
                }


            }



        }


        private void OrderUpdate(Twm.Core.DataProviders.Binance.Models.WebSocket.Futures.BinanceTradeOrderData tradeOrderData)
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

                    order.OrderState = BinanceOrderStatusToNative(obj.OrderStatus);

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


        private void OrderUpdate(Twm.Core.DataProviders.Binance.Models.WebSocket.BinanceTradeOrderData tradeOrderData)
        {
            try
            {
                var obj = tradeOrderData;
                NativeOrder order = null;
                if (obj.OrderStatus.ToString().ToUpper() == "CANCELLED")
                {
                    if (_orderCollection.ContainsKey(obj.C))
                    {
                        order = _orderCollection[obj.C];
                    }
                }
                else if (_orderCollection.ContainsKey(obj.NewClientOrderId))
                {
                    order = _orderCollection[obj.NewClientOrderId];

                }

                if (order != null)
                {
                    order.FillPrice = (double)obj.PriceOfLastFilledTrade;

                    if (order.OrderType == OrderType.Market)
                        order.Quantity = (double)obj.AccumulatedQuantityOfFilledTradesThisOrder;
                    else
                        order.Quantity = (double)obj.Quantity;

                    order.OrderState = BinanceOrderStatusToNative(obj.OrderStatus);

                    if (order.OrderState == Core.Enums.OrderState.Filled)
                        order.OrderFillDate = obj.EventTime;


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


        private void TradeUpdate(Twm.Core.DataProviders.Binance.Models.WebSocket.Futures.BinanceTradeOrderData tradeOrderData)
        {

        }




        public async Task<IResponse> GetInstruments(IRequest request)
        {
            var instrumentResponse = new Models.Response.InstrumentsResponse();
            try
            {

                instrumentResponse.Instruments = new List<Models.BinanceInstrument>();
                var resultUSDM = await GetExchangeInfo(MarketType.UsdM);
                foreach (var symbol in resultUSDM.Symbols)
                {
                    instrumentResponse.Instruments.Add(CreateInsrument(symbol, GetTickSize(symbol), MarketType.UsdM));
                };

                var resultSpot = await GetExchangeInfo(MarketType.Spot);

                foreach (var symbol in resultSpot.Symbols)
                {
                    instrumentResponse.Instruments.Add(CreateInsrument(symbol, GetTickSize(symbol), MarketType.Spot));
                };

            }
            catch
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

                var resultUSDM = await GetTicker(MarketType.UsdM);
                foreach (var ticker in resultUSDM.InstrumentTickers)
                {
                    tickerResponse.InstrumentTickers.Add(new Models.InstrumentTicker()
                    {
                        Symbol = ticker.Symbol,
                        LastPrice = ticker.LastPrice,
                        Volume = ticker.Volume
                    });
                };

                var resultSpot = await GetTicker(MarketType.Spot);

                foreach (var ticker in resultSpot.InstrumentTickers)
                {
                    tickerResponse.InstrumentTickers.Add(new Models.InstrumentTicker()
                    {
                        Symbol = ticker.Symbol,
                        LastPrice = ticker.LastPrice,
                        Volume = ticker.Volume
                    });
                };

            }
            catch
            {

            }
            return tickerResponse;
        }


        private Models.BinanceInstrument CreateInsrument(ExchangeInfoSymbol symbol, double tickSize, MarketType mt)
        {
            var minLotSize = 0.0;
            var lotSizeFilter = symbol.Filters.FirstOrDefault(x => x.FilterType == "LOT_SIZE");
            if (lotSizeFilter != null)
            {
                minLotSize = lotSizeFilter.MinQty;
            }

            var notional = 0.0;
            var notionalFilter = symbol.Filters.FirstOrDefault(x => x.FilterType == "MIN_NOTIONAL");
            if (notionalFilter != null)
            {
                notional = notionalFilter.Notional;
            }


            return new Models.BinanceInstrument()
            {
                Symbol = symbol.Symbol,
                Pair = symbol.Pair,
                ContractType = symbol.ContractType,
                DeliveryDate = symbol.DeliveryDate,
                OnboardDate = symbol.OnboardDate,
                Status = symbol.Status,
                MaintMarginPercent = symbol.MaintMarginPercent,
                RequiredMarginPercent = symbol.RequiredMarginPercent,
                BaseAsset = symbol.BaseAsset,
                QuoteAsset = symbol.QuoteAsset,
                MarginAsset = symbol.MarginAsset,
                PricePrecision = symbol.PricePrecision,
                BaseAssetPrecision = symbol.BaseAssetPrecision,
                QuotePrecision = symbol.QuotePrecision,
                UnderlyingType = symbol.UnderlyingType,
                UnderlyingSubType = symbol.UnderlyingSubType,
                SettlePlan = symbol.SettlePlan,
                TriggerProtect = symbol.TriggerProtect,
                LiquidationFee = symbol.LiquidationFee,
                MarketTakeBound = symbol.MarketTakeBound,
                MaxMoveOrderLimit = symbol.MaxMoveOrderLimit,
                MinLotSize = minLotSize,
                Notional = notional,
                Type = mt,
                TickSize = tickSize,




            };
        }

        private double GetTickSize(ExchangeInfoSymbol symbol)
        {
            var tickSize = 0.0;
            if (symbol.Filters != null && symbol.Filters.Any())
            {
                var priceFilter = symbol.Filters.FirstOrDefault(x => x.FilterType == "PRICE_FILTER");
                if (priceFilter != null)
                    tickSize = priceFilter.TickSize;
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
            catch (BinanceException bex)
            {
                throw new Exception(bex.Message + Environment.NewLine + bex.ErrorDetails);
            }

        }


        private async void SubmitOrder(NativeOrder order)
        {
            var createOrderRequest = new CreateOrderRequest();

            createOrderRequest.Type = NativeOrderTypeToBinance(order.OrderType, order.Instrument.Type);
            createOrderRequest.Side = NativeOrderActionToBinance(order.OrderAction);
            createOrderRequest.Quantity = (decimal)order.Quantity;
            createOrderRequest.Symbol = order.Instrument.Symbol;



            if (createOrderRequest.Type == BinanceOrderType.StopMarket ||
                createOrderRequest.Type == BinanceOrderType.StopLoss)
            {
                createOrderRequest.StopPrice = (decimal)order.StopPrice;
            }

            if (createOrderRequest.Type == BinanceOrderType.StopLimit ||
                createOrderRequest.Type == BinanceOrderType.StopLossLimit)
            {
                createOrderRequest.StopPrice = (decimal)order.StopPrice;
                createOrderRequest.Price = (decimal)order.LimitPrice;
                createOrderRequest.TimeInForce = TimeInForce.GTC;
            }

            if (createOrderRequest.Type == BinanceOrderType.Limit)
            {
                createOrderRequest.Price = (decimal)order.LimitPrice;
                createOrderRequest.TimeInForce = TimeInForce.GTC;
            }

            createOrderRequest.NewClientOrderId = order.Guid;

            _orderCollection.Add(createOrderRequest.NewClientOrderId, order);

            try
            {
                var response = await CreateOrder(GetInstrumentMarketType(order.Instrument), createOrderRequest);
                order.Id = response.OrderId;
            }
            catch (BinanceException bex)
            {
                _orderCollection.Remove(createOrderRequest.NewClientOrderId);
                throw new Exception(bex.Message + Environment.NewLine + bex.ErrorDetails);
            }
        }

        private MarketType GetInstrumentMarketType(Instrument instrument)
        {
            if (instrument.Type.ToUpper() == "FUTURE")
            {
                return MarketType.UsdM;
            }
            if (instrument.Type.ToUpper() == "SPOT")
            {
                return MarketType.Spot;
            }

            return MarketType.UsdM;
        }

        public async void ChangeOrder(IRequest request)
        {
            try
            {
                if (request is ChangeOrderRequest changeOrderRequest)
                {
                    if (changeOrderRequest.Order.OrderType == OrderType.StopMarket ||
                        changeOrderRequest.Order.OrderType == OrderType.StopLimit ||
                        changeOrderRequest.Order.Instrument.Type == "SPOT"
                        )
                    {
                        var newOrder = changeOrderRequest.Order.CloneTo(new NativeOrder());
                        await CancelOrder(changeOrderRequest.Order);
                        newOrder.Guid = Guid.NewGuid().ToString();
                        newOrder.Id = 0;
                        SubmitOrder(newOrder);

                    }
                    else
                    {
                        var modifyRequest = new ModifyOrderRequest();
                        modifyRequest.OrderId = changeOrderRequest.Order.Id;
                        modifyRequest.OrigClientOrderId = changeOrderRequest.Order.Guid;
                        modifyRequest.Quantity = (decimal)changeOrderRequest.Order.Quantity;
                        modifyRequest.Symbol = changeOrderRequest.Order.Instrument.Symbol;
                        modifyRequest.Side = NativeOrderActionToBinance(changeOrderRequest.Order.OrderAction);

                        if (changeOrderRequest.Order.OrderType == OrderType.Limit)
                        {
                            modifyRequest.Price = (decimal)changeOrderRequest.Order.LimitPrice;
                        }

                        var orderResponse = await ModifyOrder(GetInstrumentMarketType(changeOrderRequest.Order.Instrument), modifyRequest);

                        if (_orderCollection.ContainsKey(orderResponse.ClientOrderId))
                        {
                            var order = _orderCollection[orderResponse.ClientOrderId];

                            if (order.OrderType == OrderType.StopMarket)
                            {
                                order.StopPrice = (double)orderResponse.StopPrice;
                            }

                            if (order.OrderType == OrderType.Limit)
                            {
                                order.LimitPrice = (double)orderResponse.Price;
                            }

                            order.Quantity = (double)orderResponse.OriginalQuantity;


                            OrderStatusChanged.Invoke(this, new OrderStatusChangedResponse()
                            {
                                Order = order
                            });
                        }
                    }


                }
            }
            catch (BinanceException bex)
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
                    CancelOrder(cancelOrderRequest.Order);
                }
            }
            catch (BinanceException bex)
            {
                throw new Exception(bex.Message + Environment.NewLine + bex.ErrorDetails);
            }
        }

        private async Task<CancelOrderResponse> CancelOrder(NativeOrder order)
        {
            var cancelOrder = new Models.Request.CancelOrderRequest();

            cancelOrder.Symbol = order.Instrument.Symbol;
            cancelOrder.OrderId = order.Id;
            cancelOrder.OriginalClientOrderId = order.Guid;

            var response = await CancelOrder(GetInstrumentMarketType(order.Instrument), cancelOrder);
            return response;
        }


        public async Task<IResponse> GetHistoricalData(IRequest request)
        {

            if (request is GetKlinesCandlesticksRequest klinesCandlesticksRequest)
            {
                HistoricalDataResponse response = new HistoricalDataResponse();
                response.Candles = new List<IHistoricalCandle>();

                DateTime stopDate = klinesCandlesticksRequest.StartTime ?? DateTime.MinValue.ToUniversalTime();
                var endTime = klinesCandlesticksRequest.EndTime;
                klinesCandlesticksRequest.StartTime = null;
                while (true)
                {
                    if (endTime != null)
                    {
                        klinesCandlesticksRequest.EndTime = endTime;
                    }

                    var klineResults = await GetKlinesCandlesticks(klinesCandlesticksRequest);

                    klineResults.ForEach(k =>
                    {
                        response.Candles.Add(new HistoricalCandle()
                        {
                            Close = (double)k.Close,
                            High = (double)k.High,
                            Low = (double)k.Low,
                            Open = (double)k.Open,
                            Volume = (double)k.Volume,
                            CloseTime = k.CloseTime,
                            Time = k.OpenTime
                        });
                    });

                    if (!klineResults.Any())
                    {
                        break;
                    }

                    endTime = klineResults.Min(x => x.OpenTime).AddMinutes(-1);
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




        private OrderSide NativeOrderActionToBinance(OrderAction orderAction)
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

            throw new Exception("Unknown order action " + orderAction.Description() + " for Binance request");
        }

        private OrderAction BinanceOrderActionToNative(OrderSide orderAction)
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

        private BinanceOrderType NativeOrderTypeToBinance(OrderType orderType, string type)
        {
            switch (orderType)
            {
                case OrderType.Limit:
                    return BinanceOrderType.Limit;
                case OrderType.Market:
                    return BinanceOrderType.Market;
                case OrderType.StopMarket:
                    if (type == "FUTURE")
                    {
                        return BinanceOrderType.StopMarket;
                    }
                    else
                    {
                        return BinanceOrderType.StopLoss;
                    }
                case OrderType.StopLimit:
                    if (type == "FUTURE")
                    {
                        return BinanceOrderType.StopLimit;
                    }
                    else
                    {
                        return BinanceOrderType.StopLossLimit;
                    }

            }

            throw new Exception("Unknown order type " + orderType.Description() + " for Binance request");
        }


        private OrderType BinanceOrderTypeToNative(BinanceOrderType orderType)
        {
            switch (orderType)
            {
                case BinanceOrderType.Limit:
                    return OrderType.Limit;
                case BinanceOrderType.Market:
                    return OrderType.Market;
                case BinanceOrderType.StopMarket:
                    return OrderType.StopMarket;
                case BinanceOrderType.StopLimit:
                    return OrderType.StopLimit;
            }

            return OrderType.Unknown;
        }

        private Core.Enums.OrderState BinanceOrderStatusToNative(BinanceOrderStatus orderStatus)
        {
            switch (orderStatus)
            {
                /*case BinanceOrderStatus.:
                    return Core.Enums.OrderState.Working;*/
                case BinanceOrderStatus.New:
                    return Core.Enums.OrderState.Working;
                case BinanceOrderStatus.Rejected:
                    return Core.Enums.OrderState.Rejected;
                /*case "PendingCancel":
                    return Core.Enums.OrderState.CancelPending;
                case "PreSubmitted":
                    return Core.Enums.OrderState.Working;
                case "Submitted":
                    return Core.Enums.OrderState.Working;
                case "ApiCancelled":
                    return Core.Enums.OrderState.CancelPending;*/
                case BinanceOrderStatus.Filled:
                    return Core.Enums.OrderState.Filled;
                case BinanceOrderStatus.Expired:
                    return Core.Enums.OrderState.Expired;
                /*case "Inactive":
                    return Core.Enums.OrderState.Rejected;*/
                case BinanceOrderStatus.Cancelled:
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
                        .FindBy(x => x.ConnectionId == ConnectionId && x.Symbol == symbol && x.Type == type)
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


        private async void KlineHandler(BinanceKlineData data, List<Action<string, IEnumerable<ICandle>>> callbackList, string symbol)
        {
            var ticks = new List<Candle>()
            {
                new Candle(TimeZoneInfo.ConvertTimeFromUtc(data.Kline.StartTime, SystemOptions.Instance.TimeZone), (double)data.Kline.Open, (double)data.Kline.High, (double)data.Kline.Low, (double)data.Kline.Close, (double)data.Kline.Volume, true, false, true)
            };

            lock (_listLock)
            {
                foreach (var cb in callbackList)
                {
                    cb.BeginInvoke(symbol, ticks.ToArray(), null, null);
                }
            }
        }


        private async void LiveHandler(BinanceTradeData data, List<Action<string, double>> callbackList, string symbol)
        {
            lock (_listLock)
            {
                foreach (var cb in callbackList)
                {
                    cb.BeginInvoke(symbol, (double)data.LastPrice, null, null);
                }
            }
        }


        private async void DepthHandler(BinanceDepthData data, string symbol, MarketType mt)
        {
            if (data != null)
            {


                ConcurrentDictionary<string, OrderBook> symbolOrderBooks;
                List<Action<string, OrderBookResult>> depthCallbacks;
                ConcurrentDictionary<string, List<BinanceDepthData>> buffers;
                if (mt == MarketType.UsdM)
                {
                    symbolOrderBooks = _futuresOrderBooks;
                    depthCallbacks = _depthFuturesCallbacks[symbol];
                    buffers = _futuresBuffers;
                }
                else
                {
                    symbolOrderBooks = _spotOrderBooks;
                    depthCallbacks = _depthSpotCallbacks[symbol];
                    buffers = _spotBuffers;
                }


                OrderBook orderBook;
                if (!symbolOrderBooks.ContainsKey(symbol))
                {
                    orderBook = new OrderBook() { Levels = 100 };
                    symbolOrderBooks.TryAdd(symbol, orderBook);
                }
                else
                {
                    orderBook = symbolOrderBooks[symbol];
                }


                if (!orderBook.IsInit)
                {
                    lock (_lock)
                    {
                        if (!orderBook.IsInit)
                        {
                            if (buffers.TryGetValue(symbol, out var list))
                            {
                                list.Add(data);
                            }
                            return;
                        }
                    }
                }



                foreach (var order in data.AskDepthDeltas)
                {
                    var price = (double)order.Price;
                    var quantity = (double)order.Quantity;
                    orderBook.Update(MarketDataType.Ask, price, quantity);

                }

                foreach (var order in data.BidDepthDeltas)
                {
                    var price = (double)order.Price;
                    var quantity = (double)order.Quantity;
                    orderBook.Update(MarketDataType.Bid, price, quantity);
                }


                orderBook.TruncateAsks();
                orderBook.TruncateBids();

                var priceLevels = new OrderBookResult() { IsIncrement = true, Snapshot = orderBook };

                var depthCallbacksList = depthCallbacks.ToList();

                foreach (var callback in depthCallbacksList)
                {

                    callback?.Invoke(symbol, priceLevels);
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

                if (_binanceWebSocketClient == null)
                    _binanceWebSocketClient = new DisposableBinanceWebSocketClient(this);


                _klineCallbackRequests.TryAdd(klineCallback, request);

                if (!_klineSubscriptions.ContainsKey(symbol))
                {

                    var klineCallbackList = new List<Action<string, IEnumerable<ICandle>>> { klineCallback };
                    _klineCallbacks.TryAdd(symbol, klineCallbackList);
                    var guid = _binanceWebSocketClient.ConnectToKlineWebSocket(mt, symbol, IsTestMode, liveDataRequest.KlineInterval, data => KlineHandler(data, klineCallbackList, symbol));
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


                /* if (!_liveSubscriptions.ContainsKey(symbol))
                 {
                     var liveCallbackList = new List<Action<string, double>> { liveCallback };
                     _liveCallbacks.TryAdd(symbol, liveCallbackList);
                     var guid = _binanceWebSocketClient.ConnectToIndividualSymbolTickerWebSocket(mt, symbol, IsTestMode, data => LiveHandler(data, liveCallbackList, symbol));
                     _liveSubscriptions.TryAdd(symbol, guid);

                 }
                 else
                 {

                     if (_liveCallbacks.TryGetValue(liveDataRequest.Symbol, out var callBackList))
                     {
                         lock (_listLock)
                         {
                             if (!callBackList.Contains(liveCallback))
                                 callBackList.Add(liveCallback);
                         }
                     }
                 }*/
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
                                    _binanceWebSocketClient.CloseWebSocketInstance(guid);



                                }
                            }
                        }
                    }
                }

                /* if (_liveCallbacks.ContainsKey(liveDataRequest.Symbol))
                 {
                     if (_liveCallbacks.TryGetValue(liveDataRequest.Symbol, out var callbackList))
                     {
                         if (callbackList.Count > 1)
                         {
                             callbackList.Remove(liveCallback);
                         }
                         else
                         {
                             if (_liveCallbacks.TryRemove(liveDataRequest.Symbol, out var callbackListRemove))
                             {
                                 callbackListRemove.Remove(liveCallback);
                                 if (_liveSubscriptions.ContainsKey(liveDataRequest.Symbol))
                                 {
                                     _liveSubscriptions.TryRemove(liveDataRequest.Symbol, out var guid);
                                     _binanceWebSocketClient.CloseWebSocketInstance(guid);



                                 }
                             }
                         }
                     }
                 }*/
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


        public IRequest GetHistoricalDataRequest(DataSeriesParams dataSeriesParams, DateTime startDate, DateTime endDate)
        {
            var request = new GetKlinesCandlesticksRequest();


            request.Interval = GetKlineInterval(dataSeriesParams.DataSeriesValue, dataSeriesParams.DataSeriesType.ToAbbr());
            request.Symbol = dataSeriesParams.Instrument.Symbol;
            request.StartTime = startDate;
            request.EndTime = endDate;
            request.Limit = 1000;


            request.MarketType = MarketType.UsdM;

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
            if (dataSeriesValue == 8 && dataSeriesType == "h")
                return KlineInterval.EightHours;
            if (dataSeriesValue == 12 && dataSeriesType == "h")
                return KlineInterval.TwelveHours;

            if (dataSeriesValue == 1 && dataSeriesType == "d")
                return KlineInterval.OneDay;
            if (dataSeriesValue == 3 && dataSeriesType == "d")
                return KlineInterval.ThreeDays;

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



            if (_binanceWebSocketClient != null && IsPrivate)
            {
                _binanceWebSocketClient.CloseWebSocketInstance(_userDataStremGuidFut, false);
                await CloseUserDataStream(MarketType.UsdM, _binanceWebSocketClient.ListenKey);
                _binanceWebSocketClient.Dispose();
            }


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

        private object _lock = new object();

        public async void SubscribeToDepth(IRequest request, Action<string, OrderBookResult> depthCallback)
        {
            if (request is DepthDataRequest depthDataRequest)
            {
                var mt = depthDataRequest.MarketType;
                var symbol = depthDataRequest.Symbol;
                var levels = depthDataRequest.Levels;

                if (_binanceWebSocketClient == null)
                    _binanceWebSocketClient = new DisposableBinanceWebSocketClient(this);

                _depthCallbackRequests.TryAdd(depthCallback, request);
                ConcurrentDictionary<string, Guid> depthSubscriptions;
                ConcurrentDictionary<string, List<Action<string, OrderBookResult>>> depthCallbacks;
                ConcurrentDictionary<string, OrderBook> symbolOrderBooks;
                ConcurrentDictionary<string, List<BinanceDepthData>> buffers;
                if (mt == MarketType.UsdM)
                {
                    depthSubscriptions = _depthFuturesSubscriptions;
                    depthCallbacks = _depthFuturesCallbacks;
                    symbolOrderBooks = _futuresOrderBooks;
                    buffers = _futuresBuffers;
                }
                else
                {
                    depthSubscriptions = _depthSpotSubscriptions;
                    depthCallbacks = _depthSpotCallbacks;
                    symbolOrderBooks = _spotOrderBooks;
                    buffers = _spotBuffers;
                }

                if (!depthSubscriptions.ContainsKey(symbol))
                {

                    var depthCallbackList = new List<Action<string, OrderBookResult>> { depthCallback };
                    lock (_depthLock)
                    {
                        depthCallbacks.TryAdd(symbol, depthCallbackList);
                    }

                    var guid = _binanceWebSocketClient.ConnectToDepthWebSocket(mt, symbol, IsTestMode, data => DepthHandler(data, symbol, mt));
                    depthSubscriptions.TryAdd(symbol, guid);

                    var snapshot = await GetOrderBook(mt, symbol);
                    lock (_lock)
                    {
                        List<BinanceDepthData> list;
                        if (!buffers.ContainsKey(symbol))
                        {
                            list = new List<BinanceDepthData>();
                            buffers.TryAdd(symbol, list);
                        }
                        else
                        {
                            list = buffers[symbol];
                        }

                        list.RemoveAll(x => x.FinalUpdateId <= snapshot.LastUpdateId);

                        OrderBook orderBook;
                        if (!symbolOrderBooks.ContainsKey(symbol))
                        {
                            orderBook = new OrderBook() {Levels = 100 };
                            symbolOrderBooks.TryAdd(symbol, orderBook);
                        }
                        else
                        {
                            orderBook = symbolOrderBooks[symbol];
                        }


                        foreach (var order in snapshot.Asks)
                        {
                            var price = (double)order.Price;
                            var quantity = (double)order.Quantity;
                            orderBook.Insert(MarketDataType.Ask, price, quantity);
                        }

                        foreach (var order in snapshot.Bids)
                        {
                            var price = (double)order.Price;
                            var quantity = (double)order.Quantity;
                            orderBook.Insert(MarketDataType.Bid, price, quantity);
                        }

                        foreach (var data in list)
                        {
                            foreach (var order in data.AskDepthDeltas)
                            {
                                var price = (double)order.Price;
                                var quantity = (double)order.Quantity;
                                orderBook.Update(MarketDataType.Ask, price, quantity);
                            }

                            foreach (var order in data.BidDepthDeltas)
                            {
                                var price = (double)order.Price;
                                var quantity = (double)order.Quantity;
                                orderBook.Update(MarketDataType.Bid, price, quantity);
                            }
                        }
                        list.Clear();

                        depthCallback.Invoke(depthDataRequest.Symbol, new OrderBookResult() { Snapshot = orderBook, IsNewBook = true });
                        orderBook.IsInit = true;


                    }
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
                    _orderBooks = _futuresOrderBooks;
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
                                _binanceWebSocketClient.CloseWebSocketInstance(guid);
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


        #endregion



    }
}