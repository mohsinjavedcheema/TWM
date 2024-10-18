using System;
using System.Collections.Generic;
using System.Windows;
using Twm.Core.ViewModels;

namespace Twm.ViewModels.Charts
{
    public class PriceLevelViewModel : ViewModelBase
    {
        private double _level;

        public double Level
        {
            get { return _level; }
            set
            {
                if (_level != value)
                {
                    _level = value;
                    OnPropertyChanged();
                }
            }
        }


        private double? _ask;

        public double? Ask
        {
            get { return _ask; }
            set
            {
                if (_ask != value)
                {
                    _ask = value;
                    _bid = null;
                    IsAsk = value != null;
                    if (IsAsk)
                        Volume = value;
                    OnPropertyChanged();
                }
            }
        }


        private double? _bid;

        public double? Bid
        {
            get { return _bid; }
            set
            {
                if (_bid != value)
                {
                    _ask = null;
                    _bid = value;
                    IsAsk = value == null;
                    if (!IsAsk)
                        Volume = value;
                    OnPropertyChanged();
                }
            }
        }


        private double? _volume;

        public double? Volume
        {
            get { return _volume; }
            set
            {
                if (_volume != value)
                {
                    _volume = value;
                    if (_volume == null)
                    {
                        _bid = null;
                        _ask = null;
                    }

                    OnPropertyChanged();
                    OnPropertyChanged("Bid");
                    OnPropertyChanged("Ask");
                }
            }
        }

        private double? _maxVolume;

        public double? MaxVolume
        {
            get { return _maxVolume; }
            set
            {
                if (_maxVolume != value)
                {
                    _maxVolume = value;
                    OnPropertyChanged();
                }
            }
        }


        private double _imbalance;

        public double Imbalance
        {
            get { return _imbalance; }
            set
            {
                if (Math.Round(_imbalance, 2) != Math.Round(value, 2))
                {
                    _imbalance = value;
                    OnPropertyChanged();
                }
            }
        }


        private double _cumSize;
        public double CumSize
        {
            get { return _cumSize; }
            set
            {
                if (Math.Round(_cumSize, 2) != Math.Round(value, 2))
                {
                    _cumSize = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _maxCumSize;
        public double MaxCumSize
        {
            get { return _maxCumSize; }
            set
            {
                if (Math.Round(_maxCumSize, 2) != Math.Round(value, 2))
                {
                    _maxCumSize = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _isAsk;

        public bool IsAsk
        {
            get { return _isAsk; }
            set
            {
                if (_isAsk != value)
                {
                    _isAsk = value;
                    OnPropertyChanged();
                    OnPropertyChanged("IsBid");
                    OnPropertyChanged("AskVisibility");
                    OnPropertyChanged("BidVisibility");
                }
            }
        }

        public bool IsBid
        {
            get { return !_isAsk; }
        }


        private bool _isBest;

        public bool IsBest
        {
            get { return _isBest; }
            set
            {
                if (_isBest != value)
                {
                    _isBest = value;
                    OnPropertyChanged();
                }
            }
        }


        private short _localIndex;

        public short LocalIndex
        {
            get { return _localIndex; }
            set
            {
                if (_localIndex != value)
                {
                    _localIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        private ushort _globalIndex;

        public ushort GlobalIndex
        {
            get { return _globalIndex; }
            set
            {
                if (_globalIndex != value)
                {
                    _globalIndex = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _isVisible;

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChanged();
                    Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }


        private System.Windows.Visibility _visibility;

        public Visibility Visibility
        {
            get { return _visibility; }
            set
            {
                if (_visibility != value)
                {
                    _visibility = value;
                    OnPropertyChanged();
                    OnPropertyChanged("AskVisibility");
                    OnPropertyChanged("BidVisibility");
                }
            }
        }


        public Visibility AskVisibility
        {
            get
            {
                if (Visibility == Visibility.Collapsed || Visibility == Visibility.Hidden)
                    return Visibility;

                if (IsAsk)
                    return Visibility.Visible;

                return Visibility.Collapsed;
            }
        }


        public Visibility BidVisibility
        {
            get
            {
                if (Visibility == Visibility.Collapsed || Visibility == Visibility.Hidden)
                    return Visibility;

                if (IsBid)
                    return Visibility.Visible;

                return Visibility.Collapsed;
            }
        }



       

        public int LastTickIndex { get; set; }

      

        public PriceLevelViewModel()
        {
            Visibility = Visibility.Visible;           
            LastTickIndex = -1;
        }



    }
}
