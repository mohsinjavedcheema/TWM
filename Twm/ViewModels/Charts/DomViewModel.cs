using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Twm.Chart.Classes;
using Twm.Chart.Interfaces;
using Twm.Core.DataCalc;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.ViewModels;
using Twm.Core.ViewModels.DataSeries;
using Twm.Interfaces;
using Twm.Model.Model;

namespace Twm.ViewModels.Charts
{
    public class DomViewModel : ViewModelBase, IDomViewModel
    {

        public event EventHandler ConnectionAndTypeChanged;

        private static readonly object SyncLock = new object();

        public Dictionary<string, Instrument> _dictSymbols;

        public ObservableCollection<string> Symbols { get; set; }
        public ICollectionView SymbolsView { get; set; }

        public DataCalcContext DataCalcContext { get; set; }

        public Dispatcher Dispatcher { get; set; }

        public event EventHandler OnAutoCentering;

        public string Header
        {
            get
            {

                var header = "";
                if (DataSeriesParams.SelectedConnection != null)
                    header = DataSeriesParams.SelectedConnection.Name;

                if (DataSeriesParams.SelectedType != null)
                    header += " " + DataSeriesParams.SelectedType;

                if (DataSeriesParams.Instrument != null)
                    header += " " + DataSeriesParams.Instrument.Symbol;


                return header;
            }
        }


        public DataSeriesParamsViewModel DataSeriesParams { get; set; }

        private IRequest _currentLiveRequest;
        private IRequest _currentDepthRequest;


        /* private string _currentSubscriber;*/

        private Instrument _selectedInstrument;

        public Instrument SelectedInstrument
        {
            get { return _selectedInstrument; }
            set
            {
                if (_selectedInstrument != value)
                {
                    _selectedInstrument = value;
                    OnPropertyChanged();
                }
            }
        }

        // public Window Window { get; set; }


        private double? _last;

        public double? Last
        {
            get { return _last; }
            set
            {
                if (_last != value)
                {
                    _last = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime? _lastDateTime;

        public DateTime? LastDateTime
        {
            get { return _lastDateTime; }
            set
            {
                if (_lastDateTime != value)
                {
                    _lastDateTime = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _spread;

        public string Spread
        {
            get { return _spread; }
            set
            {
                if (_spread != value)
                {
                    _spread = value;
                    OnPropertyChanged();
                }
            }
        }

        private double? _changePercent;

        public double? ChangePercent
        {
            get { return _changePercent; }
            set
            {
                if (_changePercent != value)
                {
                    _changePercent = value;
                    OnPropertyChanged();
                }
            }
        }

        private double? _open;

        public double? Open
        {
            get { return _open; }
            set
            {
                if (_open != value)
                {
                    _open = value;
                    OnPropertyChanged();
                }
            }
        }


        private double? _high;

        public double? High
        {
            get { return _high; }
            set
            {
                if (_high != value)
                {
                    _high = value;
                    OnPropertyChanged();
                }
            }
        }


        private double? _low;

        public double? Low
        {
            get { return _low; }
            set
            {
                if (_low != value)
                {
                    _low = value;
                    OnPropertyChanged();
                }
            }
        }


        private double? _close;

        public double? Close
        {
            get { return _close; }
            set
            {
                if (_close != value)
                {
                    _close = value;
                    OnPropertyChanged();
                }
            }
        }


        private double? _prevClose;

        public double? PrevClose
        {
            get { return _prevClose; }
            set
            {
                if (_prevClose != value)
                {
                    _prevClose = value;
                    OnPropertyChanged();
                }
            }
        }


        private long? _volume;
        public long? Volume
        {
            get { return _volume; }
            set
            {
                if (_volume != value)
                {
                    _volume = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _autoCentering;
        public bool AutoCentering
        {
            get { return _autoCentering; }
            set
            {
                if (_autoCentering != value)
                {
                    _autoCentering = value;
                    OnPropertyChanged();
                    if (value)
                        OnAutoCentering?.Invoke(this, new EventArgs());
                    ApplySettings();
                }
            }
        }


        private double _startIndex;
        public double StartIndex
        {
            get { return _startIndex; }
            set
            {
                if (_startIndex != value)
                {
                    _startIndex = value;
                    OnPropertyChanged();
                }
            }
        }


        private double _itemsCount;
        public double ItemsCount
        {
            get { return _itemsCount; }
            set
            {
                if (_itemsCount != value)
                {
                    _itemsCount = value;
                    OnPropertyChanged();
                }
            }
        }


        public bool IsConnected
        {
            get { return _connection != null; }
        }


        private IConnection _connection;
        public IConnection Connection
        {
            get { return _connection; }
            set
            {
                if (_connection != value)
                {
                    _connection = value;
                    OnPropertyChanged();
                    OnPropertyChanged("IsConnected");
                }
            }
        }

        public string SubscriberCode { get; set; }

        public PricesViewModel PricesViewModel { get; set; }



        public ICommand ShowSettingsCommand { get; set; }
        public int Levels { get; set; }

        //  public DomSettings Settings { get; set; }

        private string _settingsFilePath;

        public DomViewModel()
        {
            DataCalcContext = new DataCalcContext();

            Symbols = new ObservableCollection<string>();
            SymbolsView = CollectionViewSource.GetDefaultView(Symbols);
            BindingOperations.EnableCollectionSynchronization(Symbols, SyncLock);
            AutoCentering = false;
         
            PricesViewModel = new PricesViewModel(this);

            SubscriberCode = Guid.NewGuid().ToString();
            ShowSettingsCommand = new RelayCommand(ShowSetting);
            DataSeriesParams = new DataSeriesParamsViewModel();
            DataSeriesParams.PropertyChanged += DataSeriesParams_PropertyChanged;

            /*#if DEBUG
                        _settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NinjaTrader 8", "bin", "Custom",
                            "dxFeedDomSettings.json");
            #else
                        _settingsFilePath = Path.Combine(NinjaTrader.Core.Globals.UserDataDir, "bin", "Custom", "dxFeedDomSettings.json");
            #endif

                        if (File.Exists(_settingsFilePath))
                        {
                            try
                            {
                                var settingsJson = File.ReadAllText(_settingsFilePath);
                                Settings = JsonConvert.DeserializeObject<DomSettings>(settingsJson);
                            }
                            catch (Exception ex)
                            {
                                Settings = new DomSettings();
                            }

                        }
                        else
                        {
                            Settings = new DomSettings();
                        }*/

            AutoCentering = true;// Settings.AutoCentering;


            ApplySettings();

            _dictSymbols = new Dictionary<string, Instrument>();

        }

        private void DataSeriesParams_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Instrument")
            {
                Connection = DataSeriesParams.SelectedConnection;
                DataCalcContext.Connection = Connection;

                Subscribe();
            }
            if (e.PropertyName == "SelectedConnection" || e.PropertyName == "SelectedType")
            {
                if (sender is DataSeriesParamsViewModel dsp)
                {
                    ConnectionAndTypeChanged?.Invoke(sender, new EventArgs());
                }
            }
            OnPropertyChanged("Header");
        }

        private void ApplySettings()
        {
            //if (PricesViewModel.LineNumber != Settings.LevelNumbers)
            {

                PricesViewModel.LineNumber = 20;//Settings.LevelNumbers;
                /*  if (PricesViewModel.LineNumber > Connector.MaxDomLevels)
                      PricesViewModel.LineNumber = Connector.MaxDomLevels;*/

               // PricesViewModel.Refresh(StartIndex, ItemsCount);
            }

            /*   if (AutoCentering != Settings.AutoCentering)
               {
                   Settings.AutoCentering = AutoCentering;
               }

               var json = JsonConvert.SerializeObject(Settings);

               try
               {
                   if (!File.Exists(_settingsFilePath))
                   {
                       using (File.Create(_settingsFilePath))
                       {

                       }
                   }

                   File.WriteAllText(_settingsFilePath, json);
               }
               catch (Exception ex)
               {
                   MessageBox.Show("Can`t save setting to file: " + ex.Message);
               }*/

        }

        private void ShowSetting(object obj)
        {
            /* var settings = new DomSettings { LevelNumbers = Settings.LevelNumbers };


             var settingsWindow = new DomSettingsWindow(settings);
             if (settingsWindow.ShowDialog() == true)
             {
                 Settings.LevelNumbers = settings.LevelNumbers;


                 ApplySettings();
             }*/

        }

        public async void Subscribe()
        {

            if (DataSeriesParams.Instrument == null || DataSeriesParams.SelectedConnection == null)
                return;

            if ((DataSeriesParams.Instrument != SelectedInstrument))
            {
                Unsubscribe();

                PricesViewModel.Levels = DataSeriesParams.SelectedConnection.Levels ;// Settings.LevelNumbers;
                /*if (PricesViewModel.LineNumber > Connector.MaxDomLevels)
                    PricesViewModel.LineNumber = Connector.MaxDomLevels;*/

                var depthRequest = DataCalcContext.GetDepthRequest(DataSeriesParams);
                
                PricesViewModel.SubscribeMarketDepth(depthRequest);
                _currentDepthRequest = depthRequest;

                SelectedInstrument = DataSeriesParams.Instrument;

            }
        }


        private void UpdateLiveByTicks(string symbol, IEnumerable<ICandle> ticks)
        {

        }

      /*  public string CurrentSubscriber()
        {
            return SelectedInstrument;
        }
*/

        public void Unsubscribe()
        {
            if (SelectedInstrument != null)
            {
                StopSubscribe();

                PricesViewModel.Clear();
                

            }
        }

        public async void StopSubscribe()
        {
            if (_currentLiveRequest != null)
            {
                Connection.Client.UnSubscribeFromLive(_currentLiveRequest, UpdateLiveByTicks);
            }

            if (_currentDepthRequest != null)
            {

                PricesViewModel.UnsubscribeMarketDepth(_currentDepthRequest);
            }
            
        }

        public void ChangeViewIndex(double verticalOffset, double viewportHeight)
        {
            StartIndex = verticalOffset;
            ItemsCount = viewportHeight;
           
        }

        public void DoAutoCenter()
        {
            OnAutoCentering?.Invoke(this, new EventArgs());
        }
    }
}
