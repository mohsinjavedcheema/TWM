using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Twm.Core.DataProviders.Common.OrderBooks;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.ViewModels;
using Twm.Interfaces;


namespace Twm.ViewModels.Charts
{
    public class PricesViewModel : ViewModelBase
    {
        private static readonly object SyncLock = new object();

        /// <summary>
        /// Collection current price levels info
        /// </summary>
        public ObservableCollection<PriceLevelViewModel> Prices { get; set; }

        public ICollectionView PricesView { get; set; }

        /// <summary>
        /// Collection for fast seek PriceLevel by price
        /// </summary>
        public Dictionary<double, PriceLevelViewModel> DictPrices;


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
                }
            }
        }


        private double _maxAskVolumeValue;
        public double MaxAskVolumeValue
        {
            get { return _maxAskVolumeValue; }
            set
            {
                if (_maxAskVolumeValue != value)
                {
                    _maxAskVolumeValue = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _maxBidVolumeValue;
        public double MaxBidVolumeValue
        {
            get { return _maxBidVolumeValue; }
            set
            {
                if (_maxBidVolumeValue != value)
                {
                    _maxBidVolumeValue = value;
                    OnPropertyChanged();
                }
            }
        }



        public int LineNumber { get; set; } = 10;
        public bool ShowAllLevels { get; set; } = false;

        private double _maxVolumeValue;
        public double MaxVolumeValue
        {
            get { return _maxVolumeValue; }
            set
            {
                if (_maxVolumeValue != value)
                {
                    _maxVolumeValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public int AskIndex { get; set; }
        public int BidIndex { get; set; }


        private int _levels;
        public int Levels 
        {
            get { return _levels; }

            set
            {
                _levels = value;
                AskIndex = value - 1;
            }
        }
        public string CurrentPriceIncrement { get; set; }

        private readonly IDomViewModel _domViewModel;



        private readonly System.Timers.Timer _timer;


        private double _maxBid = double.MinValue;
        private double _minAsk = double.MaxValue;

        private int _maxBidIndex = -1;
        private int _minAskIndex = -1;

        private bool _isFirstRefresh;

        private int secCount = 0;


        private bool isfirst = false;

        public PricesViewModel(IDomViewModel domViewModel)
        {
            _domViewModel = domViewModel;
            Prices = new ObservableCollection<PriceLevelViewModel>();
            PricesView = CollectionViewSource.GetDefaultView(Prices);
            BindingOperations.EnableCollectionSynchronization(Prices, SyncLock);

            DictPrices = new Dictionary<double, PriceLevelViewModel>();

            MaxVolumeValue = 0;
            _domViewModel = domViewModel;
           
        }







       


        public async void SubscribeMarketDepth(IRequest request)
        {
            _domViewModel.DataCalcContext.Connection.Client.SubscribeToDepth(request, UpdateDepth);
        }

        public async void UnsubscribeMarketDepth(IRequest request)
        {


            _domViewModel.DataCalcContext.Connection.Client.UnSubscribeFromDepth(request, UpdateDepth);

        }

        private void UpdateDepth(string symbol, object obj)
        {
            lock (SyncLock)
            {
                try
                {
                    if (obj is OrderBookResult priceLevels)
                    {
                        if (priceLevels.IsNewBook)
                        {
                            isfirst = false;
                            DateTime dateTime = DateTime.MinValue;
                            if (priceLevels.Snapshot.Asks != null)
                            {
                                var asks = priceLevels.Snapshot.Asks.OrderByDescending(x => x.Price).ToList();
                                foreach (var ask in asks)
                                {
                                    InsertPriceLevel(ask, true);
                                }
                            }

                            if (priceLevels.Snapshot.Bids != null)
                            {
                                var bids = priceLevels.Snapshot.Bids.OrderByDescending(x => x.Price).ToList();
                                foreach (var bid in bids)
                                {
                                    InsertPriceLevel(bid, false);
                                }
                            }

                            _domViewModel.DoAutoCenter();
                        }
                        else if (priceLevels.IsIncrement)
                        {

                            var maxVolume = 0.0;
                            if (priceLevels.Snapshot.Asks != null)
                            {

                                var asks = priceLevels.Snapshot.Asks.OrderBy(x => x.Price).ToList();
                                var i = Levels - 1;
                                var cumSize = 0.0;
                                foreach (var ask in asks)
                                {
                                    Prices[i].Level = ask.Price;
                                    Prices[i].Ask = ask.Size;
                                   
                                    if (ask.Size > maxVolume)
                                        maxVolume = ask.Size;

                                    cumSize += ask.Size;
                                    Prices[i].CumSize = cumSize;
                                    i--;
                                }
                            }

                            if (priceLevels.Snapshot.Bids != null)
                            {
                                var bids = priceLevels.Snapshot.Bids.OrderByDescending(x => x.Price).ToList();
                                var i = Levels;
                                var cumSize = 0.0;
                                foreach (var bid in bids)
                                {
                                    Prices[i].Level = bid.Price;
                                    Prices[i].Bid = bid.Size;
                                    
                                    if (bid.Size > maxVolume)
                                        maxVolume = bid.Size;

                                    cumSize += bid.Size;
                                    Prices[i].CumSize = cumSize;
                                    i++;
                                }
                            }
                            MaxVolumeValue = maxVolume;


                        }


                    }
                }
                catch (Exception ex)
                {

                }
            }
        }


        private void InsertPriceLevel(PriceLevel priceLevel, bool isAsk)
        {
            if (DictPrices.ContainsKey(priceLevel.Price))
            {
                var priceLevelVm = DictPrices[priceLevel.Price];
                if (isAsk)
                {
                    //   Debug.WriteLine("Insert1: " + dxPriceLevel.Price + "; isAsk");
                    priceLevelVm.Ask = priceLevel.Size;

                    if (priceLevelVm.Level < _minAsk)
                    {
                        _minAsk = priceLevelVm.Level;
                        Prices[_minAskIndex].IsBest = false;
                        _minAskIndex = Prices.IndexOf(priceLevelVm);
                        priceLevelVm.IsBest = true;
                    }
                }
                else
                {
                    //  Debug.WriteLine("Insert1: " + dxPriceLevel.Price + "; isBid");
                    priceLevelVm.Bid = priceLevel.Size;

                    if (priceLevelVm.Level > _maxBid)
                    {
                        _maxBid = priceLevelVm.Level;
                        Prices[_maxBidIndex].IsBest = false;
                        _maxBidIndex = Prices.IndexOf(priceLevelVm);
                        priceLevelVm.IsBest = true;
                    }
                }
            }
            else
            {
                var priceLevelVm = new PriceLevelViewModel { Level = priceLevel.Price };

                if (isAsk)
                {
                    priceLevelVm.Ask = priceLevel.Size;
                    // Debug.WriteLine("Insert: " + dxPriceLevel.Price + "; isAsk");
                }
                else
                {
                    priceLevelVm.Bid = priceLevel.Size;
                    //  Debug.WriteLine("Insert: " + dxPriceLevel.Price + "; isBid");
                }


                var price = Prices.FirstOrDefault(x => x.Level < priceLevel.Price);
                int index;
                if (price == null)
                {
                    Prices.Add(priceLevelVm);
                    index = Prices.Count - 1;
                }
                else
                {
                    index = Prices.IndexOf(price);
                    Prices.Insert(index, priceLevelVm);
                }

                if (isAsk)
                {
                    if (priceLevelVm.Level < _minAsk)
                    {
                        _minAsk = priceLevelVm.Level;
                        if (_minAskIndex >= 0)
                            Prices[_minAskIndex].IsBest = false;
                        _minAskIndex = index;
                        Prices[_minAskIndex].IsBest = true;
                    }
                    else
                    {
                        _minAskIndex++;
                    }

                    _maxBidIndex++;
                }
                else
                {
                    if (priceLevelVm.Level > _maxBid && _maxBid == double.MinValue)
                    {
                        _maxBid = priceLevelVm.Level;
                        _maxBidIndex = index;
                        Prices[_maxBidIndex].IsBest = true;
                    }
                }

                DictPrices.Add(priceLevel.Price, priceLevelVm);
            }
        }



        public void Clear()
        {



            Prices.Clear();
            DictPrices.Clear();
            MaxVolumeValue = 0;
            _maxBid = double.MinValue;
            _minAsk = double.MaxValue;
            _maxBidIndex = -1;
            _minAskIndex = -1;

          
            _isFirstRefresh = false;


        }

        





    }
}