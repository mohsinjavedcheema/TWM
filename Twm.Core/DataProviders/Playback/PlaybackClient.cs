using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twm.Chart.Interfaces;
using Twm.Core.Classes;
using Twm.Core.Controllers;
using Twm.Core.DataProviders.Common;
using Twm.Core.DataProviders.Common.OrderBooks;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.Interfaces;

namespace Twm.Core.DataProviders.Playback
{
    public class PlaybackClient : IDataProviderClient
    {
        public TimeSpan TimestampOffset
        {
            get { return _timestampOffset; }
            set
            {
                _timestampOffset = value;
                _requestClient.SetTimestampOffset(_timestampOffset);
            }
        }

        private RequestClient _requestClient;

        private TimeSpan _timestampOffset;

        public bool IsTestMode { get; set; }

        public string Message { get; set; }

        public string ServerName { get; set; }


        /// <summary>
        /// Create a new playback client
        /// </summary>
        public PlaybackClient()
        {
            IsTestMode = true;
            _symbolCallbacks = new Dictionary<string, List<Action<string, IEnumerable<ICandle>>>>();
            _requestClient = new RequestClient();
        }

        #region User Stream

        public Task<IResponse> FindInstruments(IRequest request)
        {
            return null;
        }

        public async Task<IResponse> GetHistoricalData(IRequest request)
        {
            return null;
        }


        public Task<IResponse> TestConnection(IRequest request)
        {
            return null;
        }

        public async Task<IResponse> GetInstruments(IRequest request)
        {
            return null;
        }


        public async Task<IResponse> SubmitOrder(IRequest request)
        {
            return null;
        }

        public void CancelOrder(IRequest request)
        {
            throw new NotImplementedException();
        }

        public void ChangeOrder(IRequest request)
        {
            throw new NotImplementedException();
        }

        public EventHandler<IResponse> OrderStatusChanged { get; set; }
        public EventHandler<IResponse> PositionChanged { get; set; }
        public EventHandler<IResponse> AccountChanged { get; set; }
        public bool IsPrivate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public EventHandler<IResponse> AssetChanged { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public async Task<bool> Connect(IRequest request)
        {
            return true;
        }

        private readonly Dictionary<string, List<Action<string, IEnumerable<ICandle>>>> _symbolCallbacks;

        public Task<object> GetInstrument(IRequest request)
        {
            throw new NotImplementedException();
        }

        public void SubscribeToLive(IRequest request, Action<string, IEnumerable<ICandle>> callback)
        {
            /*if (request is LiveDataRequest liveDataRequest)
            {
                
                try
                {
                    if (!_symbolCallbacks.TryGetValue(liveDataRequest.Symbol, out var callbacks))
                    {
                        callbacks = new List<Action<string, IEnumerable<ICandle>>>();
                        _symbolCallbacks.Add(liveDataRequest.Symbol, callbacks);

                        if (_symbolCallbacks.Count == 1)
                        {
                            Session.Instance.Playback.OnTimerElapsed += Playback_OnTimerElapsed;
                        }
                    }
                    callbacks.Add(callback);
                }
                catch (Exception ex)
                {
                    LogController.Print($"Subscribe to playback exception: {ex.Message}");
                }
            }*/
        }

        private void Playback_OnTimerElapsed(object sender, PlaybackEventArgs e)
        {
            if (_symbolCallbacks.TryGetValue(e.Symbol, out var callbacks))
            {

                foreach (var callback in callbacks)
                {
                    callback.BeginInvoke(e.Symbol, e.Ticks, null, null);
                }
            }
        }

        public void UnSubscribeFromLive(IRequest request, Action<string, IEnumerable<ICandle>> callback)
        {
            /*if (request is LiveDataRequest liveDataRequest)
            {
                try
                {
                    if (_symbolCallbacks.TryGetValue(liveDataRequest.Symbol, out var callbacks))
                    {
                        callbacks.Clear();
                        _symbolCallbacks.Remove(liveDataRequest.Symbol);

                        if (_symbolCallbacks.Count == 0)
                            Session.Instance.Playback.OnTimerElapsed -= Playback_OnTimerElapsed;
                    }
                }
                catch (Exception ex)
                {
                    LogController.Print($"UnSubscribe from playback exception: {ex.Message}");
                }
            }*/
        }

        void IDataProviderClient.SubmitOrder(IRequest request)
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            _symbolCallbacks.Clear();
        }

        public Task<bool> SubscribeToOrderRouting(IRequest request)
        {
            throw new NotImplementedException();
        }

        public IRequest GetHistoricalDataRequest(DataSeriesParams dataSeriesParams, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public IRequest GetLiveDataRequest(DataSeriesParams dataSeriesParams)
        {
            throw new NotImplementedException();
        }

        public void InitAccount()
        {
         
        }

        public Task<IResponse> GetTickers(IRequest request)
        {
            throw new NotImplementedException();
        }

        public void SubscribeToDepth(IRequest request, Action<string, IEnumerable<IPriceLevel>> callback)
        {
            throw new NotImplementedException();
        }

        public void UnSubscribeFromDepth(IRequest request, Action<string, IEnumerable<IPriceLevel>> callback)
        {
            throw new NotImplementedException();
        }

        public IRequest GetDepthDataRequest(DataSeriesParams dataSeriesParams)
        {
            throw new NotImplementedException();
        }

        public void SubscribeToDepth(IRequest request, Action<string, OrderBookResult> callback)
        {
            throw new NotImplementedException();
        }

        public void UnSubscribeFromDepth(IRequest request, Action<string, OrderBookResult> callback)
        {
            throw new NotImplementedException();
        }



        #endregion
    }
}