using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Twm.Chart.Annotations;
using Twm.Core.Enums;
using Twm.Model.Model;

namespace Twm.Core.Market
{
    public class Order :ICloneable, INotifyPropertyChanged
    {
        public Instrument Instrument { get; set; }
        public string EnabledStrategyGui { get; set; }
        public long Id { get; set; }
        public string Sid { get; set; }
        public string Guid { get; set; }

        public string RealGuid { get; set; }

        public long RealId { get; set; }

        public DateTime OrderInitDate { get; set; }
        public int OrderInitBarNumber { get; set; }

        private DateTime? _orderFillDate;
        public DateTime? OrderFillDate
        {
            get { return _orderFillDate; }

            set
            {
                if (_orderFillDate != value)
                {
                    _orderFillDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public int OrderFillBarNumber { get; set; }

        public string Name { get; set; }

        public string Comment { get; set; }

        public string Oco { get; set; }


        private double _limitPrice;
        public double LimitPrice
        {
            get { return _limitPrice; }

            set
            {
                if (_limitPrice != value)
                {
                    _limitPrice = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _stopPrice;
        public double StopPrice
        {
            get { return _stopPrice; }

            set
            {
                if (_stopPrice != value)
                {
                    _stopPrice = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _triggerPrice;
        public double TriggerPrice
        {
            get { return _triggerPrice; }

            set
            {
                if (_triggerPrice != value)
                {
                    _triggerPrice = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _fillPrice;
        public double FillPrice
        {
            get { return _fillPrice; }

            set
            {
                if (_fillPrice != value)
                {
                    _fillPrice = value;
                    OnPropertyChanged();
                }
            }
        }


        private double _quantity;
        public double Quantity
        {
            get { return _quantity; }

            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                }
            }
        }

        public OrderAction OrderAction { get; set; }


        private OrderState _orderState;
        public OrderState OrderState
        {
            get { return _orderState; }

            set
            {
                if (_orderState != value)
                {
                    _orderState = value;
                    OnPropertyChanged();
                }
            }
        }

        public OrderType OrderType { get; set; }
        public HistoricalOrderDirection HistoricalOrderDirection { get; set; }
        public OrderEnvironment OrderEnvironment { get; set; }


        public bool IsWorking
        {
            get { return _orderState == OrderState.Working; }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}