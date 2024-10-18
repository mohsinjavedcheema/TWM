using System;
using System.Linq;
using System.Net;
using Twm.Core.DataProviders.Bybit.Enums;
using Twm.Core.DataProviders.Bybit.Models.Request;
using Twm.Core.DataProviders.Common;
using Twm.Core.DataProviders.Enums;
using Twm.Core.DataProviders.Interfaces;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Twm.Core.DataProviders.Bybit
{
    public static class Endpoints
    {

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
        internal static string APIBaseUrl = "https://api.bybit.com";

        /// <summary>
        /// Defaults to Test API binance domain (https)
        /// </summary>        
        internal static string APIBaseUrlTest = "https://api-testnet.bybit.com";
       
        internal static string WebSocketBaseUrl = "wss://stream.bybit.com/v5";

        internal static string WebSocketBaseUrlTest = "wss://stream-testnet.bybit.com/v5";

        private static string APIPrefix { get; } = $"{APIBaseUrl}";
        private static string APIPrefixTest { get; } = $"{APIBaseUrlTest}";


        public static class WebSocket
        {
            public static EndpointData GetPublicStream(MarketType mt, bool isTest)
            {
                var type = "";
                if (mt == MarketType.UsdM)
                {
                    type = "linear";
                }
                if (mt == MarketType.Spot)
                {
                    type = "spot";
                }
                if (isTest)
                {
                    return new EndpointData(new Uri($"{WebSocketBaseUrlTest}/public/{type}"), EndpointSecurityType.None);
                }
                return new EndpointData(new Uri($"{WebSocketBaseUrl}/public/{type}"), EndpointSecurityType.None);
            }

            public static EndpointData GetPrivateStream(bool isTest)
            {
                if (isTest)
                {
                    return new EndpointData(new Uri($"{WebSocketBaseUrlTest}/private"), EndpointSecurityType.None);
                }
                return new EndpointData(new Uri($"{WebSocketBaseUrl}/private"), EndpointSecurityType.None);
            }

        }

        public static class UserStream
        {
            internal static string ApiVersion = "v5";

            /// <summary>
            /// Start a user data stream
            /// </summary>
            //public static EndpointData StartUserDataStream => new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/userDataStream"), EndpointSecurityType.ApiKey);

            public static EndpointData StartUserDataStream(MarketType mt, bool isTestMode)
            {

                if (!isTestMode)
                {
                    return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/listenKey"), EndpointSecurityType.ApiKey);
                }
                else
                {
                    return new EndpointData(new Uri($"{APIPrefixTest}/{ApiVersion}/listenKey"), EndpointSecurityType.ApiKey);
                }
            }


            public static EndpointData KeepAliveUserDataStream(MarketType mt, string listenKey, bool isTestMode)
            {

                if (!isTestMode)
                    return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/listenKey"), EndpointSecurityType.ApiKey);
                else
                    return new EndpointData(new Uri($"{APIPrefixTest}/{ApiVersion}/listenKey"), EndpointSecurityType.ApiKey);

            }



            /// <summary>
            /// Close a user data stream to prevent
            /// </summary>
            public static EndpointData CloseUserDataStream(MarketType mt, string listenKey, bool isTestMode)
            {

                if (!isTestMode)
                    return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/listenKey"), EndpointSecurityType.ApiKey);
                else
                    return new EndpointData(new Uri($"{APIPrefixTest}/{ApiVersion}/listenKey"), EndpointSecurityType.ApiKey);

            }
        }

        public static class General
        {
            internal static string ApiVersion = "v5";

            /// <summary>
            /// Test connectivity to the Rest API.
            /// </summary>
            //public static EndpointData TestConnectivity => new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/ping"), EndpointSecurityType.None);


            public static EndpointData TestConnectivity(MarketType mt, bool isTestMode)
            {

                if (!isTestMode)
                    return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/ping"), EndpointSecurityType.ApiKey);
                else
                    return new EndpointData(new Uri($"{APIPrefixTest}/{ApiVersion}/ping"), EndpointSecurityType.ApiKey);

            }



            /// <summary>
            /// Test connectivity to the Rest API and get the current server time.
            /// </summary>
            public static EndpointData ServerTime => new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/time"), EndpointSecurityType.None);

            /// <summary>
            /// Current exchange trading rules and symbol information.
            /// </summary>
            public static EndpointData InstrumentsInfo(IRequest request, bool isTestMode = false)
            {
                var queryString = GenerateQueryStringFromData(request);

                if (!isTestMode)
                    return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/market/instruments-info?{queryString}"), EndpointSecurityType.None);
                else
                    return new EndpointData(new Uri($"{APIPrefixTest}/{ApiVersion}/market/instruments-info?{queryString}"), EndpointSecurityType.None);
            }


            public static EndpointData TickerInfo(IRequest request, bool isTestMode = false)
            {

                var queryString = GenerateQueryStringFromData(request);

                if (!isTestMode)
                    return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/market/tickers?{queryString}"), EndpointSecurityType.None);
                else
                    return new EndpointData(new Uri($"{APIPrefixTest}/{ApiVersion}/market/tickers?{queryString}"), EndpointSecurityType.None);
            }

        }

        public static class MarketData
        {
            internal static string ApiVersion = "v5";

            /// <summary>
            /// Gets the order book with all bids and asks
            /// </summary>
            public static EndpointData OrderBook(string symbol, int limit, bool useCache = false)
            {
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

                if (!request.IsTestMode)
                    return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/market/kline?{queryString}"), EndpointSecurityType.None);
                else
                    return new EndpointData(new Uri($"{APIPrefixTest}/{ApiVersion}/market/kline?{queryString}"), EndpointSecurityType.None);
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

            internal static string ApiVersion = "v5";

            public static EndpointData NewOrder(bool isTestMode = false)
            {
                if (!isTestMode)
                    return new EndpointData(new Uri($"{APIBaseUrl}/{ApiVersion}/order/create"), EndpointSecurityType.ApiKey);
                else
                    return new EndpointData(new Uri($"{APIBaseUrlTest}/{ApiVersion}/order/create"), EndpointSecurityType.ApiKey);
            }


            public static EndpointData ModifyOrder(bool isTestMode = false)
            {
                if (!isTestMode)
                    return new EndpointData(new Uri($"{APIBaseUrl}/{ApiVersion}/order/amend"), EndpointSecurityType.ApiKey);
                else
                    return new EndpointData(new Uri($"{APIBaseUrlTest}/{ApiVersion}/order/amend"), EndpointSecurityType.ApiKey);
            }

            public static EndpointData QueryOrder(QueryOrderRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/order?{queryString}"), EndpointSecurityType.Signed);
            }

            public static EndpointData CancelOrder(bool isTestMode)
            {
                if (!isTestMode)
                    return new EndpointData(new Uri($"{APIBaseUrl}/{ApiVersion}/order/cancel"), EndpointSecurityType.ApiKey);
                else
                    return new EndpointData(new Uri($"{APIBaseUrlTest}/{ApiVersion}/order/cancel"), EndpointSecurityType.ApiKey);
            }

            public static EndpointData CurrentOpenOrders(CurrentOpenOrdersRequest request, bool isTestMode)
            {
                var queryString = GenerateQueryStringFromData(request);

                if (!isTestMode)
                    return new EndpointData(new Uri($"{APIBaseUrl}/{ApiVersion}/order/realtime?{queryString}"), EndpointSecurityType.ApiKey, false, queryString);
                else
                    return new EndpointData(new Uri($"{APIBaseUrlTest}/{ApiVersion}/order/realtime?{queryString}"), EndpointSecurityType.ApiKey, false,queryString);
            }


            public static EndpointData CurrentPositions(CurrentPositionInformationRequest request, bool isTestMode)
            {
                var queryString = GenerateQueryStringFromData(request);

                if (!isTestMode)
                    return new EndpointData(new Uri($"{APIBaseUrl}/{ApiVersion}/position/list?{queryString}"), EndpointSecurityType.ApiKey, false, queryString);
                else
                    return new EndpointData(new Uri($"{APIBaseUrlTest}/{ApiVersion}/position/list?{queryString}"), EndpointSecurityType.ApiKey, false, queryString);

            }

            public static EndpointData GetAssetInfo(GetAssetInfoRequest request, bool isTestMode)
            {
                var queryString = GenerateQueryStringFromData(request);

                if (!isTestMode)
                    return new EndpointData(new Uri($"{APIBaseUrl}/{ApiVersion}/asset/transfer/query-account-coins-balance?{queryString}"), EndpointSecurityType.ApiKey, false, queryString);
                else
                    return new EndpointData(new Uri($"{APIBaseUrlTest}/{ApiVersion}/asset/transfer/query-account-coins-balance?{queryString}"), EndpointSecurityType.ApiKey, false, queryString);

            }

            public static EndpointData AllOrders(AllOrdersRequest request, bool isTestMode)
            {
                var queryString = GenerateQueryStringFromData(request);
                if (!isTestMode)
                    return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/order/realtime?{queryString}"), EndpointSecurityType.Signed);
                else
                    return new EndpointData(new Uri($"{APIBaseUrlTest}/{ApiVersion}/order/realtime?{queryString}"), EndpointSecurityType.Signed);
                
            }

            public static EndpointData AccountWalletInformation(GetAssetInfoRequest request, bool isTestMode)
            {
                var queryString = GenerateQueryStringFromData(request);

                if (!isTestMode)
                    return new EndpointData(new Uri($"{APIBaseUrl}/{ApiVersion}/account/wallet-balance?{queryString}"), EndpointSecurityType.ApiKey, false, queryString);
                else
                    return new EndpointData(new Uri($"{APIBaseUrlTest}/{ApiVersion}/account/wallet-balance?{queryString}"), EndpointSecurityType.ApiKey, false, queryString);

            }






            public static EndpointData AccountTradeList(AllTradesRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new EndpointData(new Uri($"{APIPrefix}/{ApiVersion}/myTrades?{queryString}"), EndpointSecurityType.Signed);
            }

            /* public static EndpointData Withdraw(WithdrawRequest request)
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
             }*/
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

