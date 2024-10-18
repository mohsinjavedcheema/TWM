using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Twm.Chart.Annotations;
using Twm.Core.Enums;
using Twm.Model.Model;

namespace Twm.Core.Market
{
    public class Position : INotifyPropertyChanged
    {

        public Instrument Instrument { get; set; }

        public string EnabledStrategyGui { get; set; }


        private MarketPosition _marketPosition;
        public MarketPosition MarketPosition
        {
            get { return _marketPosition; }
            set
            {
                if (_marketPosition != value)
                {
                    _marketPosition = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Name));
                }
            }

        }

        private double _quantity;
        public double Quantity
        {
            get { return _quantity;}
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Name));
                }
            } 

        }

        public double Price { get; set; }

        public int SubPositionCount { get; set; }

        public DateTime EntryDate { get; set; }
        public int EntryBarNumber { get; set; }
        public DateTime ExitDate { get; set; }
        public int ExitBarNumber { get; set; }
        public int PositionNumber { get; set; }
        public bool IsRealTime { get; set; }
        public List<Trade> Trades { get; set; }

        public string Name
        {
            get { return ToString(); }
        }


        private double _averageEntryPrice;
        public double AverageEntryPrice
        {
            get { return _averageEntryPrice; }

            set
            {
                if (_averageEntryPrice != value)
                {
                    _averageEntryPrice = value;
                    OnPropertyChanged();
                }
            }
        }


        private double? _unrealized;
        public double? Unrealized
        {
            get { return _unrealized; }

            set
            {
                if (_unrealized != value)
                {
                    _unrealized = value;
                    OnPropertyChanged();
                }
            }
        }

        public Position()
        {
            MarketPosition = MarketPosition.Flat;
        }

        public override string ToString()
        {
            var marketPosition = "";
            switch (MarketPosition)
            {
                case MarketPosition.Short:
                    marketPosition = "S";
                    break;
                case MarketPosition.Flat:
                    if (Quantity == 0)
                        return "";
                    marketPosition = "F";
                    break;
                case MarketPosition.Long:
                    marketPosition = "L";
                    break;
            }

            return $"{Quantity}{marketPosition}";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}