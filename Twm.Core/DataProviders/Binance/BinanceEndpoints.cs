using System;
using System.Linq;
using System.Net;
using Twm.Core.DataProviders.Binance.Enums;
using Twm.Core.DataProviders.Binance.Models.Request;
using Twm.Core.DataProviders.Common;
using Twm.Core.DataProviders.Enums;
using Twm.Core.DataProviders.Interfaces;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Twm.Core.DataProviders.Binance
{
    public static class Endpoints
    {

        //USD-M Futures Endpoints
        private const string BaseEndpointUSDMFutures = "https://fapi.binance.com";
        private const string PingEndpointUSDMFutures = "/fapi/v1/ping";
        private const string ExchangeInfoEndpointUSDMFutures = "/fapi/v1/exchangeInfo";
        private const string KlinesEndpointUSDMFutures = "fapi/v1/klines";
        private const string BaseEndpointSSLUSDMFutures = "wss://fstream.binance.com";
        private const string BaseEndpointUSDMFuturesTest = "https://testnet.binancefuture.com";


        //Coint-M Futures EndPoints
        private const string BaseEndpointCoinMFutures = "https://dapi.binance.com";
        private const string PingEndpointCoinMFutures = "/dapi/v1/ping";
        private const string ExchangeInfoEndpointCoinMFutures = "/dapi/v1/exchangeInfo";
        private const string KlinesEndpointCoinMFutures = "dapi/v1/klines";
        private const string BaseEndpointSSLCoinMFutures = "wss://dstream.binance.com";

        //Spot Endpoints
        private const string BaseEndpointSpot = "https://api.binance.com";
        private const string PingEndpointSpot = "/api/v3/ping";
        private const string ExchangeInfoEndpointSpot = "/api/v3/exchangeInfo";
        private const string KlinesEndpointSpot = "api/v3/klines";
        private const string BaseEndpointSSLSpot = "wss://api.binance.com";




        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            FloatParseHandling = FloatParseHandling.Decimal
        };

        /// <summary>
        /// Defaults to V1
        /// </summary>

        /// <summary>
        /// Defaults to API binance domain (https)
        /// </summary>
        internal static string APIBaseUrl = "https://api.binance.com/api";
        internal static string APIBaseUrlCoinMFut = "https://dapi.binance.com/dapi";
        internal static string APIBaseUrlUsdMFutures = "https://fapi.binance.com/fapi";

        /// <summary>
        /// Defaults to Test API binance domain (https)
        /// </summary>        
        internal static string APIBaseUrlTest = "https://testnet.binancefuture.com/api";
        internal static string APIBaseUrlUsdMFuturesTest = "https://testnet.binancefuture.com/fapi";




        /// <summary>
        /// Defaults to WAPI binance domain (https)
        /// </summary>
        internal static string WAPIBaseUrl = "https://api.binance.com/wapi";

        private static string APIPrefix { get; } = $"{APIBaseUrl}";
        private static string APIPrefixCoinMFut { get; } = $"{APIBaseUrlCoinMFut}";
        private static string APIPrefixUsdMFut { get; } = $"{APIBaseUrlUsdMFutures}";

        private static string APIPrefixTest { get; } = $"{APIBaseUrlTest}";

        private static string APIPrefixUsdMFutTest { get; } = $"{APIBaseUrlUsdMFuturesTest}";
        private static string WAPIPrefix { get; } = $"{WAPIBaseUrl}";

        public static class UserStream
        {
            internal static string ApiVersion = "v1";

            /// <summary>
            /// Start a user data stream
            /// </summary>
            //public static EndpointData StartUserDataStream => new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/userDataStream"), EndpointSecurityType.ApiKey);

            public static EndpointData StartUserDataStream(MarketType mt, bool isTestMode)
            {
                if (mt == MarketType.UsdM)
                {
                    if (!isTestMode)
                        return new EndpointData(new Uri($"{APIPrefixUsdMFut}/{ApiVersion}/listenKey"), EndpointSecurityType.ApiKey);
                    else
                        return new EndpointData(new Uri($"{APIPrefixUsdMFutTest}/{ApiVersion}/listenKey"), EndpointSecurityType.ApiKey);
                }

                if (mt == MarketType.CoinM)
                {
                    return new EndpointData(new Uri($"{APIPrefixCoinMFut}/{ApiVersion}/listenKey"), EndpointSecurityType.ApiKey);
                }


                return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/userDataStream"), EndpointSecurityType.ApiKey);
            }

            /// <summary>
            /// Ping a user data stream to prevent a timeout
            /// </summary>
           /* public static EndpointData KeepAliveUserDataStream(string listenKey)
            {
                return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/userDataStream?listenKey={listenKey}"),
                    EndpointSecurityType.ApiKey);
            }*/


            public static EndpointData KeepAliveUserDataStream(MarketType mt, string listenKey, bool isTestMode)
            {
                if (mt == MarketType.UsdM)
                {
                    if (!isTestMode)
                        return new EndpointData(new Uri($"{APIPrefixUsdMFut}/{ApiVersion}/listenKey"), EndpointSecurityType.ApiKey);
                    else
                        return new EndpointData(new Uri($"{APIPrefixUsdMFutTest}/{ApiVersion}/listenKey"), EndpointSecurityType.ApiKey);
                }

                if (mt == MarketType.CoinM)
                    return new EndpointData(new Uri($"{APIPrefixCoinMFut}/{ApiVersion}/listenKey"), EndpointSecurityType.ApiKey);


                return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/userDataStream?listenKey={listenKey}"), EndpointSecurityType.ApiKey);
            }



            /// <summary>
            /// Close a user data stream to prevent
            /// </summary>
            public static EndpointData CloseUserDataStream(MarketType mt, string listenKey, bool isTestMode)
            {
                if (mt == MarketType.UsdM)
                {
                    if (!isTestMode)
                        return new EndpointData(new Uri($"{APIPrefixUsdMFut}/{ApiVersion}/listenKey"), EndpointSecurityType.ApiKey);
                    else
                        return new EndpointData(new Uri($"{APIPrefixUsdMFutTest}/{ApiVersion}/listenKey"), EndpointSecurityType.ApiKey);
                }

                if (mt == MarketType.CoinM)
                    return new EndpointData(new Uri($"{APIPrefixCoinMFut}/{ApiVersion}/listenKey"), EndpointSecurityType.ApiKey);

                return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/userDataStream?listenKey={listenKey}"),
                    EndpointSecurityType.ApiKey);
            }
        }

        public static class General
        {
            internal static string ApiVersion = "v1";

            /// <summary>
            /// Test connectivity to the Rest API.
            /// </summary>
            //public static EndpointData TestConnectivity => new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/ping"), EndpointSecurityType.None);


            public static EndpointData TestConnectivity(MarketType mt, bool isTestMode)
            {


                if (mt == MarketType.UsdM)
                {
                    if (!isTestMode)
                        return new EndpointData(new Uri($"{APIPrefixUsdMFut}/{ApiVersion}/ping"), EndpointSecurityType.None);
                    else
                        return new EndpointData(new Uri($"{APIPrefixUsdMFutTest}/{ApiVersion}/ping"), EndpointSecurityType.None);
                }


                if (mt == MarketType.CoinM)
                    return new EndpointData(new Uri($"{APIPrefixCoinMFut}/{ApiVersion}/ping"), EndpointSecurityType.None);

                return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/ping"), EndpointSecurityType.None);


          
            }



            /// <summary>
            /// Test connectivity to the Rest API and get the current server time.
            /// </summary>
            public static EndpointData ServerTime => new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/time"), EndpointSecurityType.None);

            /// <summary>
            /// Current exchange trading rules and symbol information.
            /// </summary>
            public static EndpointData ExchangeInfo(MarketType mt, bool isTestMode)
            {

                if (mt == MarketType.UsdM)
                {
                    if (!isTestMode)
                        return new EndpointData(new Uri($"{APIPrefixUsdMFut}/{ApiVersion}/exchangeInfo"), EndpointSecurityType.None);
                    else
                        return new EndpointData(new Uri($"{APIPrefixUsdMFutTest}/{ApiVersion}/exchangeInfo"), EndpointSecurityType.None);
                }
               

                if (mt == MarketType.CoinM)
                    return new EndpointData(new Uri($"{APIPrefixCoinMFut}/{ApiVersion}/exchangeInfo"), EndpointSecurityType.None);

                return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/exchangeInfo"), EndpointSecurityType.None);


            }


            public static EndpointData TickerInfo(MarketType mt, bool isTestMode)
            {
                if (mt == MarketType.UsdM)
                {
                    if (!isTestMode)
                        return new EndpointData(new Uri($"{APIPrefixUsdMFut}/{ApiVersion}/ticker/24hr"), EndpointSecurityType.None);
                    else
                        return new EndpointData(new Uri($"{APIPrefixUsdMFutTest}/{ApiVersion}/ticker/24hr"), EndpointSecurityType.None);
                }


                if (mt == MarketType.CoinM)
                    return new EndpointData(new Uri($"{APIPrefixCoinMFut}/{ApiVersion}/ticker/24hr"), EndpointSecurityType.None);

                return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/ticker/24hr"), EndpointSecurityType.None);
            }

        }

        public static class MarketData
        {
            internal static string ApiVersion = "v1";

            /// <summary>
            /// Gets the order book with all bids and asks
            /// </summary>
            public static EndpointData OrderBook(MarketType mt, bool isTestMode, string symbol, int limit, bool useCache = false)
            {

                if (mt == MarketType.UsdM)
                {
                    if (!isTestMode)
                        return new EndpointData(new Uri($"{APIPrefixUsdMFut}/{ApiVersion}/depth?symbol={symbol}&limit={limit}"), EndpointSecurityType.None);
                    else
                        return new EndpointData(new Uri($"{APIPrefixUsdMFutTest}/{ApiVersion}/depth?symbol={symbol}&limit={limit}"), EndpointSecurityType.None);
                }


                if (mt == MarketType.CoinM)
                    return new EndpointData(new Uri($"{APIPrefixCoinMFut}/{ApiVersion}/depth?symbol={symbol}&limit={limit}"), EndpointSecurityType.None);


                return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/depth?symbol={symbol}&limit={limit}"), EndpointSecurityType.None, useCache);
            }

            /// <summary>
            /// Get compressed, aggregate trades. Trades that fill at the time, from the same order, with the same price will have the quantity aggregated.
            /// </summary>
            public static EndpointData CompressedAggregateTrades(GetCompressedAggregateTradesRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/aggTrades?{queryString}"), EndpointSecurityType.None);
            }

            /// <summary>
            /// Kline/candlestick bars for a symbol. Klines are uniquely identified by their open time.
            /// </summary>
            public static EndpointData KlineCandlesticks(GetKlinesCandlesticksRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);

                if (request.MarketType == MarketType.UsdM)
                {
                    if (!request.IsTestMode)
                        return new EndpointData(new Uri($"{APIPrefixUsdMFut}/{ApiVersion}/klines?{queryString}"), EndpointSecurityType.None);
                    else
                        return new EndpointData(new Uri($"{APIBaseUrlUsdMFuturesTest}/{ApiVersion}/klines?{queryString}"), EndpointSecurityType.None);
                }

                if (request.MarketType == MarketType.CoinM)
                {
                    return new EndpointData(new Uri($"{APIPrefixCoinMFut}/{ApiVersion}/klines?{queryString}"), EndpointSecurityType.None);
                }

                return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/klines?{queryString}"), EndpointSecurityType.None);
            }

            /// <summary>
            /// 24 hour price change statistics.
            /// </summary>
            public static EndpointData DayPriceTicker(string symbol)
            {
                return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/ticker/24hr?symbol={symbol}"),
                    EndpointSecurityType.None);
            }

            /// <summary>
            /// Latest price for all symbols.
            /// </summary>
            public static EndpointData AllSymbolsPriceTicker => new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/ticker/allPrices"), EndpointSecurityType.ApiKey);

            /// <summary>
            /// Best price/qty on the order book for all symbols.
            /// </summary>
            public static EndpointData SymbolsOrderBookTicker => new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/ticker/allBookTickers"), EndpointSecurityType.ApiKey);
        }

        public static class MarketDataV3
        {
            internal static string ApiVersion = "v3";

            /// <summary>
            /// Current Price
            /// </summary>
            public static EndpointData CurrentPrice(string symbol)
            {
                return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/ticker/price?symbol={symbol}"),
                    EndpointSecurityType.None);
            }

            /// <summary>
            /// Current Price for all symbols.
            /// </summary>
            public static EndpointData AllPrices => new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/ticker/price"), EndpointSecurityType.None);

            /// <summary>
            /// Book ticker for a single symbol
            /// </summary>
            public static EndpointData BookTicker(string symbol)
            {
                return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/ticker/bookTicker?symbol={symbol}"),
                    EndpointSecurityType.None);
            }

        }

        public static class Account
        {
            internal static string ApiVersionOne = "v1";

            internal static string ApiVersionTwo = "v2";

            internal static string ApiVersion = "v3";

            public static EndpointData NewOrder(MarketType mt, CreateOrderRequest request, bool isTestMode = false)
            {
                var queryString = GenerateQueryStringFromData(request);

                if (mt == MarketType.UsdM)
                {
                    if (!isTestMode)
                        return new EndpointData(new Uri($"{APIBaseUrlUsdMFutures}/{ApiVersionOne}/order?{queryString}"), EndpointSecurityType.Signed);
                    else
                        return new EndpointData(new Uri($"{APIBaseUrlUsdMFuturesTest}/{ApiVersionOne}/order?{queryString}"), EndpointSecurityType.Signed);
                }

                if (mt == MarketType.CoinM)
                {
                    return new EndpointData(new Uri($"{APIBaseUrlCoinMFut}/{ApiVersionOne}/order?{queryString}"), EndpointSecurityType.Signed);
                }



                return new EndpointData(new Uri($"{APIBaseUrl}/{ApiVersion}/order?{queryString}"), EndpointSecurityType.Signed);
            }


            public static EndpointData ModifyOrder(MarketType mt, ModifyOrderRequest request, bool isTestMode = false)
            {
                var queryString = GenerateQueryStringFromData(request);

                if (mt == MarketType.UsdM)
                {
                    if (!isTestMode)
                        return new EndpointData(new Uri($"{APIBaseUrlUsdMFutures}/{ApiVersionOne}/order?{queryString}"), EndpointSecurityType.Signed);
                    else
                        return new EndpointData(new Uri($"{APIBaseUrlUsdMFuturesTest}/{ApiVersionOne}/order?{queryString}"), EndpointSecurityType.Signed);
                }

                if (mt == MarketType.CoinM)
                {
                    return new EndpointData(new Uri($"{APIBaseUrlCoinMFut}/{ApiVersionOne}/order?{queryString}"), EndpointSecurityType.Signed);
                }



                return new EndpointData(new Uri($"{APIBaseUrl}/{ApiVersion}/order?{queryString}"), EndpointSecurityType.Signed);
            }


            public static EndpointData NewOrderTest(CreateOrderRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/order/test?{queryString}"), EndpointSecurityType.Signed);
            }

            public static EndpointData QueryOrder(QueryOrderRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/order?{queryString}"), EndpointSecurityType.Signed);
            }

            public static EndpointData CancelOrder(MarketType mt, CancelOrderRequest request, bool isTestMode)
            {
                var queryString = GenerateQueryStringFromData(request);
                if (mt == MarketType.UsdM)
                {
                    if (!isTestMode)
                        return new EndpointData(new Uri($"{APIBaseUrlUsdMFutures}/{ApiVersionOne}/order?{queryString}"), EndpointSecurityType.Signed);
                    else
                        return new EndpointData(new Uri($"{APIBaseUrlUsdMFuturesTest}/{ApiVersionOne}/order?{queryString}"), EndpointSecurityType.Signed);
                }

                if (mt == MarketType.CoinM)
                {
                    return new EndpointData(new Uri($"{APIBaseUrlCoinMFut}/{ApiVersionOne}/order?{queryString}"), EndpointSecurityType.Signed);
                }


                return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/order?{queryString}"), EndpointSecurityType.Signed);
            }

            public static EndpointData CurrentOpenOrders(MarketType mt, CurrentOpenOrdersRequest request, bool isTestMode)
            {
                var queryString = GenerateQueryStringFromData(request);
                if (mt == MarketType.UsdM)
                {
                    if (!isTestMode)
                        return new EndpointData(new Uri($"{APIBaseUrlUsdMFutures}/{ApiVersionOne}/openOrders?{queryString}"), EndpointSecurityType.Signed);
                    else
                        return new EndpointData(new Uri($"{APIBaseUrlUsdMFuturesTest}/{ApiVersionOne}/openOrders?{queryString}"), EndpointSecurityType.Signed);
                }

                if (mt == MarketType.CoinM)
                {
                    return new EndpointData(new Uri($"{APIBaseUrlCoinMFut}/{ApiVersionOne}/openOrders?{queryString}"), EndpointSecurityType.Signed);
                }


                return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/openOrders?{queryString}"), EndpointSecurityType.Signed);
            }


            public static EndpointData CurrentPositions(MarketType mt, CurrentPositionInformationRequest request, bool isTestMode)
            {
                var queryString = GenerateQueryStringFromData(request);
                if (mt == MarketType.UsdM)
                {
                    if (!isTestMode)
                        return new EndpointData(new Uri($"{APIBaseUrlUsdMFutures}/{ApiVersionTwo}/positionRisk?{queryString}"), EndpointSecurityType.Signed);
                    else
                        return new EndpointData(new Uri($"{APIBaseUrlUsdMFuturesTest}/{ApiVersionTwo}/positionRisk?{queryString}"), EndpointSecurityType.Signed);
                }

                if (mt == MarketType.CoinM)
                {
                    return new EndpointData(new Uri($"{APIBaseUrlCoinMFut}/{ApiVersionTwo}/positionRisk?{queryString}"), EndpointSecurityType.Signed);
                }


                return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/positionRisk?{queryString}"), EndpointSecurityType.Signed);
            }

            public static EndpointData GetSpotAccountInfo( bool isTestMode)
            {                
                if (!isTestMode)
                    return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/account"), EndpointSecurityType.Signed);
                else
                    return new EndpointData(new Uri($"{APIBaseUrlTest}/{ApiVersion}/account"), EndpointSecurityType.Signed);                
            }

            


            public static EndpointData AllOrders(AllOrdersRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/allOrders?{queryString}"), EndpointSecurityType.Signed);
            }

            public static EndpointData AccountInformation(MarketType mt, bool isTestMode)
            {

                if (mt == MarketType.UsdM)
                {
                    if (!isTestMode)
                        return new EndpointData(new Uri($"{APIBaseUrlUsdMFutures}/{ApiVersionTwo}/account"), EndpointSecurityType.Signed);
                    else
                        return new EndpointData(new Uri($"{APIBaseUrlUsdMFuturesTest}/{ApiVersionTwo}/account"), EndpointSecurityType.Signed);
                }

                if (mt == MarketType.CoinM)
                {
                    return new EndpointData(new Uri($"{APIBaseUrlCoinMFut}/{ApiVersionTwo}/account"), EndpointSecurityType.Signed);
                }

                return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/account"), EndpointSecurityType.Signed);
            }






            public static EndpointData AccountTradeList(AllTradesRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/myTrades?{queryString}"), EndpointSecurityType.Signed);
            }

            public static EndpointData Withdraw(WithdrawRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new EndpointData(new Uri($"{WAPIPrefix}/{ApiVersion}/withdraw.html?{queryString}"), EndpointSecurityType.Signed);
            }

            public static EndpointData DepositHistory(FundHistoryRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new EndpointData(new Uri($"{WAPIPrefix}/{ApiVersion}/depositHistory.html?{queryString}"), EndpointSecurityType.Signed);
            }

            public static EndpointData WithdrawHistory(FundHistoryRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new EndpointData(new Uri($"{WAPIPrefix}/{ApiVersion}/withdrawHistory.html?{queryString}"), EndpointSecurityType.Signed);
            }

            public static EndpointData DepositAddress(DepositAddressRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new EndpointData(new Uri($"{WAPIPrefix}/{ApiVersion}/depositAddress.html?{queryString}"), EndpointSecurityType.Signed);
            }

            public static EndpointData SystemStatus()
            {
                return new EndpointData(new Uri($"{WAPIPrefix}/{ApiVersion}/systemStatus.html"), EndpointSecurityType.None);
            }
        }

        private static string GenerateQueryStringFromData(IRequest request)
        {
            if (request == null)
            {
                throw new Exception("No request data provided - query string can't be created");
            }

            //TODO: Refactor to not require double JSON loop
            var obj = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(request, _settings), _settings);

            return String.Join("&", obj.Children()
                .Cast<JProperty>()
                .Where(j => j.Value != null)
                .Select(j => j.Name + "=" + System.Net.WebUtility.UrlEncode(j.Value.ToString())));
        }
    }
}
