using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Twm.Chart.DrawObjects;
using Twm.Core.Classes;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.Enums;
using Twm.Core.Market;
using Twm.Core.ViewModels;
using Twm.Model.Model;
using Twm.Model.Model.Interfaces;

namespace Twm.ViewModels.Charts
{
    public class ChartTraderViewModel : ViewModelBase
    {
        private IConnection _connection;

        public IConnection Connection
        {
            get { return _connection; }
            set
            {
                if (_connection != value)
                {
                    _connection = value;
                    RefreshAccounts();
                    Account = Session.GetAccount(_connection);
                    OnPropertyChanged();
                }
            }
        }



        private Chart.Classes.Chart _chart;
        public Chart.Classes.Chart Chart
        {
            get { return _chart; }
            set
            {
                if (_chart != value)
                {
                    _chart = value;
                    OnPropertyChanged();
                }
            }
        }


        public ObservableCollection<Account> Accounts { get; set; }




        private Account _account;
        public Account Account
        {
            get { return _account; }
            set
            {
                if (_account != value)
                {
                    _account = value;
                    AccountInit();
                    OnPropertyChanged();
                }
            }
        }

        private Instrument _instrument;

        public Instrument Instrument
        {
            get { return _instrument; }
            set
            {
                if (_instrument != value)
                {
                    _instrument = value;
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

        private double _size;

        public double Size
        {
            get { return _size; }

            set
            {
                if (_size != value)
                {
                    _size = value;
                    OnPropertyChanged();
                }
            }
        }

        public OperationCommand BuyCommand { get; set; }
        public OperationCommand BuyStopMarketCommand { get; set; }
        public OperationCommand BuyLimitCommand { get; set; }

        public OperationCommand BuyStopLimitCommand { get; set; }

        public OperationCommand SellCommand { get; set; }
        public OperationCommand SellStopMarketCommand { get; set; }
        public OperationCommand SellLimitCommand { get; set; }
        public OperationCommand SellStopLimitCommand { get; set; }
        public OperationCommand CloseCommand { get; set; }


        

        public ChartTraderViewModel()
        {
            

            BuyCommand = new OperationCommand(Buy);
            BuyStopMarketCommand = new OperationCommand(BuyStopMarket);
            BuyLimitCommand = new OperationCommand(BuyLimit);
            BuyStopLimitCommand = new OperationCommand(BuyStopLimit);

            SellCommand = new OperationCommand(Sell);
            SellStopMarketCommand = new OperationCommand(SellStopMarket);
            SellLimitCommand = new OperationCommand(SellLimit);
            SellStopLimitCommand = new OperationCommand(SellStopLimit);

            CloseCommand = new OperationCommand(Close);
            Size = 0.001;

            Session.ActiveAccounts.CollectionChanged += ActiveAccounts_CollectionChanged;

            Accounts = new ObservableCollection<Account>();


        }


        public void Close()
        {
            Session.ActiveAccounts.CollectionChanged -= ActiveAccounts_CollectionChanged;

        }




        private void RefreshAccounts()
        {
            var selectedAccount = Account;
            Accounts.Clear();

            foreach (var account in Session.Instance.Accounts)
            {
                if (!account.IsActive || account.IsPlayback)
                    continue;

                if (account.Connection.Id == Connection.Id || account.AccountType != AccountType.LocalPaper)
                {
                    Accounts.Add(account);
                }
            }

            Account = selectedAccount;

            if (!Accounts.Contains(Account))
                Account = Accounts.FirstOrDefault();
        }

        private void ActiveAccounts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RefreshAccounts();
        }

        public void AccountInit()
        {
            AddWorkingOrders();
            AddPositions();
            Chart.RefreshMainPane();
        }

        private async void AddWorkingOrders()
        {
            Chart.OrderDraws.Clear();


            if (Account == null)
                return;
            foreach (var accountOrder in Account.Orders)
            {
                if (accountOrder.OrderState != OrderState.Working)
                    continue;

                if (accountOrder.Instrument.Type ==Instrument.Type && accountOrder.Instrument.Symbol == Instrument.Symbol)

                    if (accountOrder.OrderType != OrderType.Market)
                    {
                        Chart.OrderDraws.Add(accountOrder.Guid, new OrderDraw()
                        {
                            Guid = accountOrder.Guid,
                            OrderType = accountOrder.OrderType.ToString(),
                            OrderAction = accountOrder.OrderAction.ToString(),
                            Qnt = accountOrder.Quantity,
                            LimitPrice = accountOrder.LimitPrice,
                            StopPrice = accountOrder.StopPrice
                        });
                    }

            }

        }

        private async void AddPositions()
        {
            Chart.PositionDraw = null;

            if (Account == null)
                return;

            foreach (var accountPosition in Account.Positions)
            {
                if (accountPosition.Instrument.Type == Instrument.Type && accountPosition.Instrument.Symbol == Instrument.Symbol)
                    Chart.PositionDraw = new PositionDraw()
                    {
                        Price = accountPosition.AverageEntryPrice,
                        Qnt = accountPosition.Quantity
                    };

            }


        }


        private void Close(object parameter)
        {
            if (Account != null)
            {
                try
                {
                    Account.Close(Instrument);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("You are not connected to data provider");
            }
        }

        private void Sell(object parameter)
        {
            if (Connection.IsConnected && Account != null)
            {
                try
                {
                    Account.Sell(Size, Instrument);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("You are not connected to data provider");
            }
        }

        private void SellStopMarket(object parameter)
        {
            if (Connection.IsConnected && Account != null)
            {
                try
                {
                    if (parameter is double val)
                    {
                        TriggerPrice = val;
                        Account.SellStopMarket(val, TriggerPrice, Size, Instrument);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("You are not connected to data provider");
            }
        }

        private void SellLimit(object parameter)
        {
            if (Connection.IsConnected && Account != null)
            {
                try
                {
                    if (parameter is double val)
                    {
                        TriggerPrice = val;
                        Account.SellLimit(val, TriggerPrice,Size, Instrument);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("You are not connected to data provider");
            }
        }


        private void SellStopLimit(object parameter)
        {
            if (Connection.IsConnected && Account != null)
            {
                try
                {
                    if (parameter is double val)
                    {
                        TriggerPrice = val;
                        Account.SellStopLimit(val, TriggerPrice,Size, Instrument);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("You are not connected to data provider");
            }
        }

        private void Buy(object parameter)
        {
            if (Connection.IsConnected && Account != null)
            {
                try
                {

                    Account.Buy(Size, Instrument);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("You are not connected to data provider");
            }
        }

        private void BuyStopMarket(object parameter)
        {
            if (Connection.IsConnected && Account != null)
            {
                try
                {
                    if (parameter is double val)
                    {
                        TriggerPrice = val;
                        Account.BuyStopMarket(val, TriggerPrice, Size, Instrument);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("You are not connected to data provider");
            }
        }


        private void BuyLimit(object parameter)
        {
            if (Connection.IsConnected && Account != null)
            {
                try
                {
                    if (parameter is double val)
                    {
                        TriggerPrice = val;
                        Account.BuyLimit(val, TriggerPrice,Size, Instrument);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("You are not connected to data provider");
            }
        }

        private void BuyStopLimit(object parameter)
        {
            if (Connection.IsConnected && Account != null)
            {
                try
                {
                    if (parameter is double val)
                    {
                        TriggerPrice = val;
                        Account.BuyStopLimit(val, TriggerPrice,Size, Instrument);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("You are not connected to data provider");
            }
        }


    }
}