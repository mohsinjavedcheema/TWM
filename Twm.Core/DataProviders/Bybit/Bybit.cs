using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twm.Core.Classes;
using Twm.Core.Controllers;
using Twm.Core.DataProviders.Bybit.Models;
using Twm.Core.DataProviders.Bybit.Models.Response;
using Twm.Core.DataProviders.Common;
using Twm.Core.DataProviders.Common.Response;
using Twm.Core.DataProviders.Enums;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.Enums;
using Twm.Core.Interfaces;

namespace Twm.Core.DataProviders.Bybit
{
    public class Bybit : ConnectionBase, IDataProvider
    {
        private string _apiKey;
        private string _secretKey;

        private string _reference = "Us000514";

        private IDataProviderClient _client;
        public override IDataProviderClient Client
        {
            get
            {
                if (_client == null)
                {
                    var isPaper = false;

                    if (_options == null && Options is BybitConnectionOptions bybitConnectionOptions)
                        _options = bybitConnectionOptions;


                    if (_options != null)
                    {
                        isPaper = _options.ConnectionType == ConnectionType.Paper;
                        _apiKey = _options.APIKey;
                        _secretKey = _options.APISecret;
                    }

                    _client = new BybitClient(new ClientConfiguration()
                    {
                        UserName = "",
                        Password = "",
                        ApiKey = _apiKey,
                        SecretKey = _secretKey,
                        ConnectionId = Id,
                        Reference = _reference
                    }, null, isPaper);
                }

                return _client;
            }
        }

        public override int Levels
        {
            get { return 50; }
        }

        public override bool IsLocalPaperSupported
        {
            get { return false; }
        }


        private bool _isServerPaperSupported;
        public override bool IsServerPaperSupported
        {
            get { return _isServerPaperSupported; }
        }


        private bool _isBrokerSupported;
        public override bool IsBrokerSupported
        {
            get { return _isBrokerSupported; }
        }

        private BybitConnectionOptions _options;

        public Bybit()
        {
            InstrumentType = typeof(BybitInstrument);
        }

        public override List<DataSeriesValue> GetDataFormats()
        {
            return new List<DataSeriesValue>()
            {
                new DataSeriesValue() {Value = 1, Type = DataSeriesType.Minute, Name = "1m" },
                new DataSeriesValue() {Value = 3, Type = DataSeriesType.Minute, Name = "3m" },
                new DataSeriesValue() {Value = 5, Type = DataSeriesType.Minute, Name = "5m" },
                new DataSeriesValue() {Value = 15, Type = DataSeriesType.Minute, Name = "15m" },
                new DataSeriesValue() {Value = 30, Type = DataSeriesType.Minute, Name = "30m" },
                new DataSeriesValue() {Value = 1, Type = DataSeriesType.Hour, Name = "1h" },
                new DataSeriesValue() {Value = 2, Type = DataSeriesType.Hour, Name = "2h" },
                new DataSeriesValue() {Value = 4, Type = DataSeriesType.Hour, Name = "4h" },
                new DataSeriesValue() {Value = 6, Type = DataSeriesType.Hour, Name = "6h" },
                new DataSeriesValue() {Value = 12, Type = DataSeriesType.Hour, Name = "12h" },
                new DataSeriesValue() {Value = 1, Type = DataSeriesType.Day, Name = "1d" },
                new DataSeriesValue() {Value = 1, Type = DataSeriesType.Week, Name = "1w" },
                new DataSeriesValue() {Value = 1, Type = DataSeriesType.Month, Name = "1M" }
            };
        }

        public override async void Connect()
        {
            if (Options is BybitConnectionOptions options)
            {
                _options = options;
                _apiKey = _options.APIKey;
                


                _secretKey = _options.APISecret;
                /*_userName = _options.UserName;
                _password = _options.Password;*/
                _isServerPaperSupported = _options.ConnectionType == ConnectionType.Paper;
                _isBrokerSupported = _options.ConnectionType == ConnectionType.Real;
            }
            try
            {

                Session.Instance.ConnectionStatusChanged(ConnectionStatus.Disconnected,
                    ConnectionStatus.Connecting, Id);

                if (await Task.Run(() => Client.Connect(null)))
                {
                    var result = await CheckDefaultInstruments();
                    IsConnected = true;                    
                    LogController.Print("Bybit successfully connected!");
                }
                else
                {

                    Session.Instance.ConnectionStatusChanged(ConnectionStatus.Connecting,
                        ConnectionStatus.Disconnected, Id);

                    OnPropertyChanged("IsConnected");
                    LogController.Print($"Bybit not connected: {Client.Message}");

                }

            }
            catch (Exception ex)
            {
                Session.Instance.ConnectionStatusChanged(ConnectionStatus.Connecting,
                    ConnectionStatus.Disconnected, Id);

                OnPropertyChanged("IsConnected");
                LogController.Print($"Bybit not connected: {ex.Message}");
            }



        }

        public async Task<IEnumerable<object>> GetInstruments()
        {
           
            var response = await Client.GetInstruments(null);
            if (response is InstrumentsResponse instrumentsResponse)
            {
                return instrumentsResponse.Instruments;
            }
            return Enumerable.Empty<BybitInstrument>();


        }

        public async Task<IEnumerable<object>> GetTickers()
        {
            var response = await Client.GetTickers(null);
            if (response is InstrumentTickersResponse instrumentTickersResponse)
            {
                return instrumentTickersResponse.InstrumentTickers;
            }
            return Enumerable.Empty<InstrumentTicker>();


        }




        public async Task<IEnumerable<object>> FindInstruments(IRequest request)
        {

            return Enumerable.Empty<BybitInstrument>();
        }

        public async Task<IEnumerable<IHistoricalCandle>> GetHistoricalData(IRequest request)
        {
            var response = await Client.GetHistoricalData(request);
            if (response is HistoricalDataResponse historicalDataResponse)
            {
                return historicalDataResponse.Candles;

            }

            return Enumerable.Empty<IHistoricalCandle>();
        }


        public override void Disconnect()
        {
            if (IsConnected)
            {
                IsConnected = false;

                Client?.Disconnect();
                _client = null;
                var account = Session.Instance.Accounts.FirstOrDefault(x => x.Connection == this);
                if (account != null)
                {
                    account.UnsubscribeServer(this);
                }


            }
        }

        public override IInstrumentManager CreateInstrumentManager()
        {
            return new BybitInstrumentManager(this);
        }
    }
}