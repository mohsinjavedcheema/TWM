using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twm.Chart.Interfaces;
using Twm.Core.DataProviders.Common.OrderBooks;
using Twm.Core.Interfaces;

namespace Twm.Core.DataProviders.Interfaces
{
    public interface IDataProviderClient
    {
        
        TimeSpan TimestampOffset { get; set; }

        string ServerName { get; set; }

        string Message { get; set; }

        bool IsTestMode { get; set; }

        bool IsPrivate { get;  }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IResponse> TestConnection(IRequest request);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<bool> Connect(IRequest request);



        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<bool> SubscribeToOrderRouting(IRequest request);



        /// <summary>
        /// /
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IResponse> GetInstruments(IRequest request);


        /// <summary>
        /// /
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IResponse> GetTickers(IRequest request);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<object> GetInstrument(IRequest request);


        /// <summary>
        /// /
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IResponse> FindInstruments(IRequest request);

        /// <summary>
        /// Gets the historical ticks file for the provided request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IResponse> GetHistoricalData(IRequest request);


        void SubscribeToLive(IRequest request, Action<string, IEnumerable<ICandle>> callback);

        void UnSubscribeFromLive(IRequest request, Action<string, IEnumerable<ICandle>> callback);

        void SubscribeToDepth(IRequest request, Action<string, OrderBookResult> callback);

        void UnSubscribeFromDepth(IRequest request, Action<string, OrderBookResult> callback);

        void InitAccount();


        /// <summary>
        /// /
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        void SubmitOrder(IRequest request);


        void CancelOrder(IRequest request);


        void ChangeOrder(IRequest request);


        IRequest GetHistoricalDataRequest(DataSeriesParams dataSeriesParams, DateTime startDate, DateTime endDate);
        IRequest GetLiveDataRequest(DataSeriesParams dataSeriesParams);

        IRequest GetDepthDataRequest(DataSeriesParams dataSeriesParams);

        EventHandler<IResponse> OrderStatusChanged { get; set; }

        EventHandler<IResponse> PositionChanged { get; set; }

        EventHandler<IResponse> AccountChanged { get; set; }

        EventHandler<IResponse> AssetChanged { get; set; }

        void Disconnect();
        
    }
}