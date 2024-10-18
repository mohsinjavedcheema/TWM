using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Twm.Core.Controllers;
using Twm.Core.DataProviders.Binance.Enums;
using Twm.Core.DataProviders.Binance.Models;
using Twm.Core.DataProviders.Common;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.Helpers;
using Twm.Model.Model;

namespace Twm.Core.DataProviders.Binance
{
    public class BinanceInstrumentManager : BaseInstrumentManager<BinanceInstrumentViewModel, BinanceInstrument>
    {
        private readonly Binance _connection;

        public static readonly Dictionary<MarketType, string> TypeConvertTable = new Dictionary<MarketType, string>()
        {
            { MarketType.Spot, "SPOT"},
            { MarketType.UsdM, "FUTURE"},
            { MarketType.CoinM, "FUTURE"},
        };

        private List<string> _defaultInstruments = new List<string>() { "BTCUSDT", "ETHUSDT", "BNBUSDT", "ATOMUSDT", "DASHUSDT", "SOLUSDT", "LYCUSDT", "TRXUSDT", "DOGEUSDT" };

        public BinanceInstrumentManager(Binance connection) : base(connection)
        {
            _connection = connection;
            InstrumentColumnCustomization = SystemOptions.Instance.ViewOptions["BinanceInstruments"];
        }


        public Instrument BinanceInstrumentToInstrument(BinanceInstrumentViewModel providerInstrumentVm)
        {
            return ProviderInstrumentToInstrument(providerInstrumentVm);
        }

        protected override Instrument ProviderInstrumentToInstrument(BinanceInstrumentViewModel providerInstrumentVm)
        {


            var instrument = new Instrument()
            {
                Symbol = providerInstrumentVm.Symbol,
                TradingHours = "",
                Description = "",
                Multiplier = 1,
                Base = providerInstrumentVm.Base,
                Quote = providerInstrumentVm.Quote,
                PriceIncrements = providerInstrumentVm.TickSize.ToString(),
                MinLotSize = providerInstrumentVm.MinLotSize,
                Notional = providerInstrumentVm.Notional
            };



            instrument.Type = TypeConvertTable.ContainsKey(providerInstrumentVm.Type) ? TypeConvertTable[providerInstrumentVm.Type] : providerInstrumentVm.Type.Description();
            instrument.PriceIncrements = providerInstrumentVm.TickSize.ToString();
            instrument.ConnectionId = _connection.Id;

            instrument.ProviderData = Serialize(providerInstrumentVm.DataModel);


            return instrument;

        }

        protected override void FillTypes()
        {
            TypesItems = new ObservableCollection<string>()
            {                
                "FUTURE",                
                "SPOT",
            };
            _selectedType = "FUTURE";
        }


        protected override bool FilterInstrument(object item)
        {
            var instrument = item as BinanceInstrumentViewModel;
            if (instrument == null)
                return false;

            

            

            if (TypeConvertTable[instrument.Type].ToUpper() !=  SelectedType.ToUpper())
            {
                return false;
            }

            if (string.IsNullOrEmpty(Instrument) || _findHint == null ||
                Instrument == _findHint)
                return true;


            if (
                (instrument.Symbol ?? "").ToUpper().Contains(Instrument.ToUpper()))
            {
                return true;
            }

            return false;
        }

        public override List<string> GetDefaultInstruments()
        {
            return _defaultInstruments;
        }

        public override List<Instrument> ConvertToInstruments(List<object> insCollection, string type, List<string> symbolList = null)
        {
            MarketType markerType = MarketType.UsdM;
            if (type.ToUpper() == "FUTURE")
                markerType = MarketType.UsdM;
            if (type.ToUpper() == "SPOT")
                markerType = MarketType.Spot;

            if (symbolList == null)
                symbolList = _defaultInstruments;

            var binanceInstruments = insCollection.ConvertAll(x => (BinanceInstrument)x);
            var binanceDefInstruments = binanceInstruments.Where(x => x.Status.ToUpper() == "TRADING" && x.Type == markerType && symbolList.Contains(x.Symbol)).ToList();

            var instruments = new List<Instrument>();
            foreach (var instrument in binanceDefInstruments)
            {
                instruments.Add(BinanceInstrumentToInstrument(new BinanceInstrumentViewModel(instrument)));
            }
            return instruments;
        }


        

        public override async Task FetchInstruments()
        {
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IsBusy = true;
                    IsAddEnable = false;
                    IsAddAllEnable = false;
                });
               
                if (_connection is IDataProvider dataProvider)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        Message = "Get instruments from provider...";
                    });



                    var insCollection = (await dataProvider.GetInstruments()).ToList();
                    var instruments = insCollection.ConvertAll(x => (BinanceInstrument)x);
                    var instrumentList = instruments.Where(x => x.Status == "TRADING").ToList();
                    Instruments.Clear();
                    MaxCount = instrumentList.Count;
                    var orderedInstruments = instrumentList.OrderBy(x => x.Symbol);
                    foreach (var instrument in orderedInstruments)
                    {
                        Instruments.Add(new BinanceInstrumentViewModel(instrument) { ConnectionName = _connection.Name });
                    }

                    var tickers = (await dataProvider.GetTickers()).ToList();
                    var insTickers = tickers.ConvertAll(x => (InstrumentTicker)x);

                    foreach (var instrument in Instruments)
                    {
                        var ticker = insTickers.FirstOrDefault(x => x.Symbol == instrument.Symbol);

                        if (ticker != null)
                        {
                            instrument.LastPrice = ticker.LastPrice;
                            instrument.Vol24 = ticker.Volume;
                        }
                    }



                    await Application.Current.Dispatcher.InvokeAsync(() =>
                        SelectedInstrument = Instruments.FirstOrDefault());


                    IsAddEnable = true;
                    IsAddAllEnable = true;
                }
            }
            finally
            {
                IsBusy = false;
            
            }
        }
    }
}