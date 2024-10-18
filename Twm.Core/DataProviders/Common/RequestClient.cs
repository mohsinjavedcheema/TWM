using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Twm.Core.DataProviders.Enums;
using Twm.Core.DataProviders.Extensions;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Common
{
    public  class RequestClient
    {
        private  readonly HttpClient HttpClient;
        private  SemaphoreSlim _rateSemaphore;
        private  int _limit = 10;

        /// <summary>
        /// Number of seconds the for the Limit of requests (10 seconds for 10 requests etc)
        /// </summary>
        public static int SecondsLimit = 10;

        
        private  bool RateLimitingEnabled = false;
        private  readonly Stopwatch Stopwatch;
        private  int _concurrentRequests = 0;
        private  TimeSpan _timestampOffset;
        private  readonly object LockObject = new object();

        private string _apiKey;
        private string _secret;
        private long _recWindow;

        public RequestClient()
        {
            var httpClientHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            };
            HttpClient = new HttpClient(httpClientHandler);
            HttpClient.Timeout = TimeSpan.FromMinutes(10);
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //HttpClient.DefaultRequestHeaders.Add("Content-Type", new []{"application/json"});
            _rateSemaphore = new SemaphoreSlim(_limit, _limit);
            Stopwatch = new Stopwatch();
        }

        public void SetTimestamp(string timestampHeader, long value = 0)
        {
            if (HttpClient.DefaultRequestHeaders.Contains(timestampHeader))
            {
                lock (LockObject)
                {
                    if (HttpClient.DefaultRequestHeaders.Contains(timestampHeader))
                    {
                        HttpClient.DefaultRequestHeaders.Remove(timestampHeader);
                    }
                }
            }
            var timestamp = "";

            if (value == 0)
                timestamp = DateTime.UtcNow.AddMilliseconds(_timestampOffset.TotalMilliseconds).ConvertToUnixTime().ToString();
            else
                timestamp = value.ToString();

            HttpClient.DefaultRequestHeaders.Add(timestampHeader, timestamp);
        }


        /// <summary>
        /// Recreates the Semaphore, and reassigns a Limit
        /// </summary>
        /// <param name="limit">Request limit</param>
        public  void SetRequestLimit(int limit)
        {
            _limit = limit;
            _rateSemaphore = new SemaphoreSlim(limit, limit);
        }

        /// <summary>
        /// Used to adjust the client timestamp
        /// </summary>
        /// <param name="time">TimeSpan to adjust timestamp by</param>
        public  void SetTimestampOffset(TimeSpan time)
        {
            _timestampOffset = time;
        }

        /// <summary>
        /// Sets whether Rate limiting is enabled or disabled
        /// </summary>
        /// <param name="enabled"></param>
        public  void SetRateLimiting(bool enabled)
        {
            var set = enabled ? "enabled" : "disabled";
            RateLimitingEnabled = enabled;
        }

        /// <summary>
        /// Assigns a new seconds limit
        /// </summary>
        /// <param name="limit">Seconds limit</param>
        public  void SetSecondsLimit(int limit)
        {
            SecondsLimit = limit;
        }



      
        public void SetRecWindow(string recWindowHeader, long value)
        {
            if (HttpClient.DefaultRequestHeaders.Contains(recWindowHeader))
            {
                lock (LockObject)
                {
                    if (HttpClient.DefaultRequestHeaders.Contains(recWindowHeader))
                    {
                        HttpClient.DefaultRequestHeaders.Remove(recWindowHeader);
                    }
                }
            }

            _recWindow = value;

            HttpClient.DefaultRequestHeaders.Add(recWindowHeader, value.ToString());
        }



        /// <summary>
        /// Assigns a new seconds limit
        /// </summary>
        /// <param name="key">Your API Key</param>
        public void SetAPIKey(string apiHeader, string key)
        {
            if (HttpClient.DefaultRequestHeaders.Contains(apiHeader))
            {
                lock (LockObject)
                {
                    if (HttpClient.DefaultRequestHeaders.Contains(apiHeader))
                    {
                        HttpClient.DefaultRequestHeaders.Remove(apiHeader);
                    }
                }
            }

            _apiKey = key;

            HttpClient.DefaultRequestHeaders.Add(apiHeader, new[] {key});
        }

        public void SetReference(string referenceHeader, string key)
        {
            if (HttpClient.DefaultRequestHeaders.Contains(referenceHeader))
            {
                lock (LockObject)
                {
                    if (HttpClient.DefaultRequestHeaders.Contains(referenceHeader))
                    {
                        HttpClient.DefaultRequestHeaders.Remove(referenceHeader);
                    }
                }
            }
            if (!string.IsNullOrEmpty(key))
                HttpClient.DefaultRequestHeaders.Add(referenceHeader, key);
        }



        public void ClearAPIKey(string apiHeader)
        {
            if (HttpClient.DefaultRequestHeaders.Contains(apiHeader))
            {
                lock (LockObject)
                {
                    if (HttpClient.DefaultRequestHeaders.Contains(apiHeader))
                    {
                        HttpClient.DefaultRequestHeaders.Remove(apiHeader);
                    }
                }
            }
        }

        public void SetSignature(string signHeader, string timeStampHeader, string secret, string param)
        {
            if (HttpClient.DefaultRequestHeaders.Contains(signHeader))
            {
                lock (LockObject)
                {
                    if (HttpClient.DefaultRequestHeaders.Contains(signHeader))
                    {
                        HttpClient.DefaultRequestHeaders.Remove(signHeader);
                    }
                }
            }

            var timestamp = DateTime.UtcNow.AddMilliseconds(_timestampOffset.TotalMilliseconds).ConvertToUnixTime();
            SetTimestamp(timeStampHeader, timestamp);
            var signature = timestamp + _apiKey + _recWindow + param;

            var signSignature = CreateHMACSignature(secret, signature);

            HttpClient.DefaultRequestHeaders.Add(signHeader, signSignature);
        }



        /// <summary>
        /// Create a generic GetRequest to the specified endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> GetRequest(Uri endpoint)
        {
            return await CreateRequest(endpoint);
        }

        /// <summary>
        /// Creates a generic GET request that is signed
        /// </summary>s
        /// <param name="endpoint"></param>
        /// <param name="apiKey"></param>
        /// <param name="secretKey"></param>
        /// <param name="signatureRawData"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public  async Task<HttpResponseMessage> SignedGetRequest(Uri endpoint, string apiKey, string secretKey,
            string signatureRawData, long receiveWindow = 5000)
        {
            var uri = CreateValidUri(endpoint, secretKey, signatureRawData, receiveWindow);
            return await CreateRequest(uri, HttpVerb.GET);
        }

        /// <summary>
        /// Create a generic PostRequest to the specified endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public  async Task<HttpResponseMessage> PostRequest(Uri endpoint, HttpContent content)
        {
            return await CreateRequest(endpoint, HttpVerb.POST, content);
        }

        /// <summary>
        /// Create a generic DeleteRequest to the specified endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public  async Task<HttpResponseMessage> DeleteRequest(Uri endpoint)
        {
            return await CreateRequest(endpoint, HttpVerb.DELETE);
        }

        /// <summary>
        /// Create a generic PutRequest to the specified endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public  async Task<HttpResponseMessage> PutRequest(Uri endpoint)
        {
            return await CreateRequest(endpoint, HttpVerb.PUT);
        }

        /// <summary>
        /// Creates a generic GET request that is signed
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="apiKey"></param>
        /// <param name="secretKey"></param>
        /// <param name="signatureRawData"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public  async Task<HttpResponseMessage> SignedPostRequest(Uri endpoint, string apiKey, string secretKey,
            string signatureRawData, long receiveWindow = 5000)
        {
            var uri = CreateValidUri(endpoint, secretKey, signatureRawData, receiveWindow);
            return await CreateRequest(uri, HttpVerb.POST);
        }


        public async Task<HttpResponseMessage> SignedPutRequest(Uri endpoint, string apiKey, string secretKey,
           string signatureRawData, long receiveWindow = 5000)
        {
            var uri = CreateValidUri(endpoint, secretKey, signatureRawData, receiveWindow);
            return await CreateRequest(uri, HttpVerb.PUT);
        }

        /// <summary>
        /// Creates a generic DELETE request that is signed
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="apiKey"></param>
        /// <param name="secretKey"></param>
        /// <param name="signatureRawData"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public  async Task<HttpResponseMessage> SignedDeleteRequest(Uri endpoint, string apiKey, string secretKey,
            string signatureRawData, long receiveWindow = 5000)
        {
            var uri = CreateValidUri(endpoint, secretKey, signatureRawData, receiveWindow);
            return await CreateRequest(uri, HttpVerb.DELETE);
        }


        /// <summary>
        /// Creates a valid Uri with signature
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="secretKey"></param>
        /// <param name="signatureRawData"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        /// 
        private  Uri CreateValidUri(Uri endpoint, string secretKey, string signatureRawData, long receiveWindow)
        {
            string timestamp;
#if NETSTANDARD2_0
            timestamp =
 DateTimeOffset.UtcNow.AddMilliseconds(_timestampOffset.TotalMilliseconds).ToUnixTimeMilliseconds().ToString();
#else
            timestamp = DateTime.UtcNow.AddMilliseconds(_timestampOffset.TotalMilliseconds).ConvertToUnixTime()
                .ToString();
#endif
            var qsDataProvided = !string.IsNullOrEmpty(signatureRawData);
            var argEnding = $"timestamp={timestamp}&recvWindow={receiveWindow}";
            var adjustedSignature = !string.IsNullOrEmpty(signatureRawData)
                ? $"{signatureRawData.Substring(1)}&{argEnding}"
                : $"{argEnding}";
            var hmacResult = CreateHMACSignature(secretKey, adjustedSignature);
            var connector = !qsDataProvided ? "?" : "&";
            var uri = new Uri($"{endpoint}{connector}{argEnding}&signature={hmacResult}");
            return uri;
        }

        /// <summary>
        /// Creates a HMACSHA256 Signature based on the key and total parameters
        /// </summary>
        /// <param name="key">The secret key</param>
        /// <param name="totalParams">URL Encoded values that would usually be the query string for the request</param>
        /// <returns></returns>
        private  string CreateHMACSignature(string key, string totalParams)
        {
            var messageBytes = Encoding.UTF8.GetBytes(totalParams);
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var hash = new HMACSHA256(keyBytes);
            var computedHash = hash.ComputeHash(messageBytes);
            return BitConverter.ToString(computedHash).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Makes a request to the specifed Uri, only if it hasn't exceeded the call limit 
        /// </summary>
        /// <param name="endpoint">Endpoint to request</param>
        /// <param name="verb"></param>
        /// <returns></returns>
        private  async Task<HttpResponseMessage> CreateRequest(Uri endpoint, HttpVerb verb = HttpVerb.GET,
            HttpContent content = null)
        {
            Task<HttpResponseMessage> task = null;

            if (RateLimitingEnabled)
            {
                await _rateSemaphore.WaitAsync();
                if (Stopwatch.Elapsed.Seconds >= SecondsLimit || _rateSemaphore.CurrentCount == 0 ||
                    _concurrentRequests == _limit)
                {
                    var seconds = (SecondsLimit - Stopwatch.Elapsed.Seconds) * 1000;
                    var sleep = seconds > 0 ? seconds : seconds * -1;
                    Thread.Sleep(sleep);
                    _concurrentRequests = 0;
                    Stopwatch.Restart();
                }

                ++_concurrentRequests;
            }

            var taskFunction = new Func<Task<HttpResponseMessage>, Task<HttpResponseMessage>>(t =>
            {
                if (!RateLimitingEnabled) return t;
                _rateSemaphore.Release();
                if (_rateSemaphore.CurrentCount != _limit || Stopwatch.Elapsed.Seconds < SecondsLimit) return t;
                Stopwatch.Restart();
                --_concurrentRequests;
                return t;
            });
            switch (verb)
            {
                case HttpVerb.GET:
                    task = await HttpClient.GetAsync(endpoint)
                        .ContinueWith(taskFunction);
                    break;
                case HttpVerb.POST:
                    task = await HttpClient.PostAsync(endpoint, content)
                        .ContinueWith(taskFunction);
                    break;
                case HttpVerb.DELETE:
                    task = await HttpClient.DeleteAsync(endpoint)
                        .ContinueWith(taskFunction);
                    break;
                case HttpVerb.PUT:
                    task = await HttpClient.PutAsync(endpoint, content)
                        .ContinueWith(taskFunction);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(verb), verb, null);
            }

            try
            {
                return await task;
            }
            catch (Exception ex)
            {
                var message = ex.Message+"\r\n";
                if (ex.InnerException != null)
                    message += ex.InnerException.Message;

                throw new Exception(message);
            }
        }
    }
}