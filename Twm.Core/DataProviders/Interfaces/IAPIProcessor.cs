using System;
using System.Net.Http;
using System.Threading.Tasks;
using Twm.Core.DataProviders.Common;

namespace Twm.Core.DataProviders.Interfaces
{
    public interface IAPIProcessor
    {
        /// <summary>
        /// Set the cache expiry time
        /// </summary>
        /// <param name="time"></param>
        void SetCacheTime(TimeSpan time);

        /// <summary>
        /// Processes a GET request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        Task<T> ProcessGetRequest<T>(EndpointData endpoint, int receiveWindow = 5000, string responseType = "json" ) where T : class;

        /// <summary>
        /// Processes a DELETE request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        Task<T> ProcessDeleteRequest<T>(EndpointData endpoint, int receiveWindow = 5000) where T : class;

        /// <summary>
        /// Processes a POST request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="receiveWindow"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        Task<T> ProcessPostRequest<T>(EndpointData endpoint, int receiveWindow = 5000, HttpContent content= null) where T : class;

        /// <summary>
        /// Processes a PUT request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        Task<T> ProcessPutRequest<T>(EndpointData endpoint, int receiveWindow = 5000) where T : class;

        RequestClient RequestClient { get; set; }
    }
}