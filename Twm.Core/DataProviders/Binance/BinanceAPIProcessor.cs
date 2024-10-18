using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Twm.Core.DataProviders.Binance.Models.Response.Error;
using Twm.Core.DataProviders.Common;
using Twm.Core.DataProviders.Common.Caching;
using Twm.Core.DataProviders.Enums;
using Twm.Core.DataProviders.Interfaces;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Binance
{
    /// <summary>
    /// The API Processor is the chief piece of functionality responsible for handling and creating requests to the API
    /// </summary>
    public class BinanceAPIProcessor : IAPIProcessor
    {
        private readonly string _apiKey;
        private readonly string _secretKey;
        private IAPICacheManager _apiCache;        
        private bool _cacheEnabled;
        private TimeSpan _cacheTime;

        public BinanceAPIProcessor(string apiKey, string secretKey, IAPICacheManager apiCache)
        {
            _apiKey = apiKey;
            _secretKey = secretKey;
            if (apiCache != null)
            {
                _apiCache = apiCache;
                _cacheEnabled = true;
            }
           
        }

        /// <summary>
        /// Set the cache expiry time
        /// </summary>
        /// <param name="time"></param>
        public void SetCacheTime(TimeSpan time)
        {
            _cacheTime = time;
        }

        private async Task<T> ProcessJsonResponse<T>(HttpResponseMessage message, string requestMessage, string fullCacheKey) where T : class
        {
            var messageJson = await message.Content.ReadAsStringAsync();
            T messageObject;
            try
            {
                messageObject = JsonConvert.DeserializeObject<T>(messageJson);
            }
            catch (Exception ex)
            {
                string deserializeErrorMessage = $"Unable to deserialize message from: {requestMessage}. Exception: {ex.Message}";

                throw new BinanceException(deserializeErrorMessage, new BinanceError()
                {
                    RequestMessage = requestMessage,
                    Message = ex.Message
                });
            }



            if (messageObject == null)
            {
                throw new Exception("Unable to deserialize to provided type");
            }
            if (_apiCache.Contains(fullCacheKey))
            {
                _apiCache.Remove(fullCacheKey);
            }
            _apiCache.Add(messageObject, fullCacheKey, _cacheTime);
            return messageObject;
        }

        private async Task<T> HandleResponse<T>(HttpResponseMessage message, string requestMessage, string fullCacheKey,
            string responseType = "json") where T : class
        {
            BinanceError errorObject = null;
            BinanceException exception;

            if (message.IsSuccessStatusCode)
            {
                if (responseType == "json")
                    return await ProcessJsonResponse<T>(message, requestMessage, fullCacheKey);
            }
            if (responseType == "json")
            {
                try
                {
                    var errorJson = await message.Content.ReadAsStringAsync();
                    errorObject = JsonConvert.DeserializeObject<BinanceError>(errorJson);
                }
                catch
                {
                    errorObject = new BinanceError();
                    errorObject.RequestMessage = requestMessage;
                    errorObject.Message = "Can`t read response";
                    exception = CreateBinanceException(message.StatusCode, errorObject);
                    throw exception;
                }

                if (errorObject == null)
                    throw new BinanceException("Unexpected Error whilst handling the response", null);
                errorObject.RequestMessage = requestMessage;
                exception = CreateBinanceException(message.StatusCode, errorObject);
                throw exception;
            }

            errorObject = new BinanceError
            {
                RequestMessage = requestMessage,
                Message = "Unknown response type: " + responseType
            };
            exception = CreateBinanceException(message.StatusCode, errorObject);
            throw exception;
        }

        private BinanceException CreateBinanceException(HttpStatusCode statusCode, BinanceError errorObject)
        {
            if (statusCode == HttpStatusCode.GatewayTimeout)
            {
                return new BinanceTimeoutException(errorObject);
            }
            if (statusCode == HttpStatusCode.Unauthorized)
            {
                return new BinanceUnauthorizedException(errorObject);
            }
            var parsedStatusCode = (int)statusCode;
            if (parsedStatusCode >= 400 && parsedStatusCode <= 500)
            {
                return new BinanceBadRequestException(errorObject);
            }
            return parsedStatusCode >= 500 ? 
                new BinanceServerException(errorObject) : 
                new BinanceException("Binance API Error", errorObject);
        }

        /// <summary>
        /// Checks the cache for an item, and if it exists returns it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="partialKey">The absolute Uri of the endpoint being hit. This is used in combination with the Type name to generate a unique key</param>
        /// <param name="item"></param>
        /// <returns>Whether the item exists</returns>
        private bool CheckAndRetrieveCachedItem<T>(string fullKey, out T item) where T : class
        {
            item = null;
            var result = _apiCache.Contains(fullKey);
            item =  result ? _apiCache.Get<T>(fullKey) : null;
            return result;
        }

        /// <summary>
        /// Processes a GET request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<T> ProcessGetRequest<T>(EndpointData endpoint, int receiveWindow = 5000, string responseType = "json") where T : class
        {
            var fullKey = $"{typeof(T).Name}-{endpoint.Uri.AbsoluteUri}";
            if (_cacheEnabled && endpoint.UseCache)
            {
                if (CheckAndRetrieveCachedItem<T>(fullKey, out var item))
                {
                    return item;
                }
            }
            HttpResponseMessage message;
            switch (endpoint.SecurityType) { 
                case EndpointSecurityType.ApiKey:
                case EndpointSecurityType.None:
                    message = await RequestClient.GetRequest(endpoint.Uri);
                    break;
                case EndpointSecurityType.Signed:
                    message = await RequestClient.SignedGetRequest(endpoint.Uri, _apiKey, _secretKey, endpoint.Uri.Query, receiveWindow);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return await HandleResponse<T>(message, endpoint.ToString(), fullKey);
        }

        /// <summary>
        /// Processes a DELETE request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<T> ProcessDeleteRequest<T>(EndpointData endpoint, int receiveWindow = 5000) where T : class
        {
            var fullKey = $"{typeof(T).Name}-{endpoint.Uri.AbsoluteUri}";
            if (_cacheEnabled && endpoint.UseCache)
            {
                T item;
                if (CheckAndRetrieveCachedItem<T>(fullKey, out item))
                {
                    return item;
                }
            }
            HttpResponseMessage message;
            switch (endpoint.SecurityType) { 
                case EndpointSecurityType.ApiKey:
                    message = await RequestClient.DeleteRequest(endpoint.Uri);
                    break;
                case EndpointSecurityType.Signed:
                    message = await RequestClient.SignedDeleteRequest(endpoint.Uri, _apiKey, _secretKey, endpoint.Uri.Query, receiveWindow);
                    break;
                case EndpointSecurityType.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return await HandleResponse<T>(message, endpoint.ToString(), fullKey);
        }

        /// <summary>
        /// Processes a POST request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<T> ProcessPostRequest<T>(EndpointData endpoint, int receiveWindow = 5000, HttpContent content = null) where T : class
        {
            var fullKey = $"{typeof(T).Name}-{endpoint.Uri.AbsoluteUri}";
            if (_cacheEnabled && endpoint.UseCache)
            {
                T item;
                if (CheckAndRetrieveCachedItem<T>(fullKey, out item))
                {
                    return item;
                }
            }
            HttpResponseMessage message;
            switch (endpoint.SecurityType) { 
                case EndpointSecurityType.ApiKey:
                    message = await RequestClient.PostRequest(endpoint.Uri, content );
                    break;
                case EndpointSecurityType.None:
                    throw new ArgumentOutOfRangeException();
                case EndpointSecurityType.Signed:
                    message = await RequestClient.SignedPostRequest(endpoint.Uri, _apiKey, _secretKey, endpoint.Uri.Query, receiveWindow);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return await HandleResponse<T>(message, endpoint.ToString(), fullKey);
        }

        /// <summary>
        /// Processes a PUT request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<T> ProcessPutRequest<T>(EndpointData endpoint, int receiveWindow = 5000) where T : class
        {
            var fullKey = $"{typeof(T).Name}-{endpoint.Uri.AbsoluteUri}";
            if (_cacheEnabled && endpoint.UseCache)
            {
                T item;
                if (CheckAndRetrieveCachedItem<T>(fullKey, out item))
                {
                    return item;
                }
            }
            HttpResponseMessage message;
            switch (endpoint.SecurityType) { 
                case EndpointSecurityType.ApiKey:
                    message = await RequestClient.PutRequest(endpoint.Uri);
                    break;                
                case EndpointSecurityType.Signed:
                    message = await RequestClient.SignedPutRequest(endpoint.Uri, _apiKey, _secretKey, endpoint.Uri.Query, receiveWindow);
                    break;
                case EndpointSecurityType.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return await HandleResponse<T>(message, endpoint.ToString(), fullKey);
        }

        public RequestClient RequestClient { get; set; }
    }
}