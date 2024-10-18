using System;
using Twm.Core.Helpers;
using Twm.Core.Market;
using Twm.Core.ViewModels;
using System.Linq;
using Twm.Core.Enums;
using System.Globalization;
using Twm.Chart;
using Twm.Core.Controllers;


namespace Twm.ViewModels.Orders
{
    public class OrderViewModel : ViewModelBase
    {
        private readonly Order _order;

        private string[] _updatedProperties = { "OrderAction", "OrderState", "OrderType", "LimitPrice", "StopPrice", "FillPrice", "Quantity", "OrderInitDate", "OrderFillDate", "Id", "Name" };

        public Order Order
        {
            get { return _order; }
        }


        public string Instrument
        {
            get { return _order.Instrument.Symbol; }
        }


        public string ConnectionName
        {
            get
            {

                if (_order.Instrument == null)
                    return "";

                var connectionId = _order.Instrument.ConnectionId;

                var connection = Session.GetConnection(connectionId);
                if (connection != null)
                {
                    return connection.Name;
                }
                return "";
            }
        }

        public string InstrumentType
        {
            get { return _order.Instrument.Type; }
        }

        public OrderAction OrderAction
        {
            get { return _order.OrderAction; }

        }

        public OrderState OrderState
        {
            //
            get { return _order.OrderState; }

        }

        public OrderType OrderType
        {
            get { return _order.OrderType; }

        }

        public double LimitPrice
        {

            get { return _order.LimitPrice; }

        }

        public double StopPrice
        {
            get { return _order.StopPrice; }

        }

        public double FillPrice
        {
            //
            get { return _order.FillPrice; }

        }

        public double Quantity
        {
            //
            get { return _order.Quantity; }

        }

        public string Name
        {
            get { return _order.Name; }

        }

        public long Id
        {
            get { return _order.Id; }

        }

        public string Guid
        {
            get { return _order.Guid; }

        }

        public DateTime OrderInitDate
        {
            get { return _order.OrderInitDate; }


        }

        public DateTime? OrderFillDate
        {
            //
            get
            {
                if (_order.OrderFillDate == null)
                    return null;

                return TimeZoneInfo.ConvertTimeFromUtc(_order.OrderFillDate ?? DateTime.MaxValue, SystemOptions.Instance.TimeZone);



            }


        }

        public string ValueFormat
        {
            get; set;

        }

        public int ConnectionId
        {
            get; set;

        }

        public OrderViewModel(Order order)
        {
            _order = order;
            if (double.TryParse(_order.Instrument.PriceIncrements.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture,
                              out var tickSize))
            {
                ValueFormat = "{}{0:0." + "".PadRight(tickSize.GetDecimalCount(), '0') + "}";
            }
            _order.PropertyChanged += _order_PropertyChanged;
        }

        private void _order_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_updatedProperties.Contains(e.PropertyName))
            {
                OnPropertyChanged(e.PropertyName);
            }
        }
    }
}