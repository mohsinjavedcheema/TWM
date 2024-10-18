using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Twm.Chart.Interfaces;
using Twm.Core.Controllers;
using Twm.Core.Enums;
using Twm.Core.Interfaces;
using Twm.Core.Managers;
using Twm.Core.UI.Windows;
using Twm.Core.ViewModels;
using Twm.Core.ViewModels.DataSeries;
using Twm.Model.Model;
using PdfSharp.Pdf.IO;
using Twm.Core.DataProviders.Common;


namespace Twm.Core.Classes
{
    public class HistoricalData : ViewModelBase
    {
        private bool _isExpanded;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }


        public bool IsProvider { get; set; }

        public int DataProviderId { get; set; }

        public string ProviderName { get; set; }

        public bool IsNet { get; set; }

        public string NetName { get; set; }

        public bool IsType { get; set; }

        public string TypeName { get; set; }

        public bool IsSymbol { get; set; }

        public string Symbol { get; set; }

        public HistoricalData Parent { get; private set; }
        public ObservableCollection<HistoricalData> Children { get; set; }

        public ObservableCollection<HistoricalMetaData> Data { get; set; }


        public int DataSeriesValue { get; set; }
        public DataSeriesType DataSeriesType { get; set; }

        public string DataSeriesTypeStr { get; set; }

        private Action _removeFromParent;

        public OperationCommand AddDataCommand { get; set; }
        public OperationCommand RemoveDataCommand { get; set; }

        public HistoricalData()
        {
            AddDataCommand = new OperationCommand(AddData);
            RemoveDataCommand = new OperationCommand(RemoveData);
        }

        private async void AddData(object parameter)
        {
            Instrument ins = null;
            var connectionCode = ProviderName + NetName;
            ConnectionBase selectedConnection = null;

            var connectionPair = Session.Connections.FirstOrDefault(x => x.Value.Code == connectionCode);
            selectedConnection = connectionPair.Value;

            if (IsSymbol)
            {

                ins = await Session.Instance.GetInstrument(Symbol, TypeName, connectionCode);
            }

            var instrument = new InstrumentDataParamsViewModel
            {
                InstrumentSeriesParams =
                {
                    Instrument = ins,
                    SelectedType = TypeName,
                    SelectedConnection = selectedConnection,
                    DataSeriesValue = (DataSeriesValue == 0) ? 1 : DataSeriesValue,
                    DataSeriesType = DataSeriesType
                }
            };

            if (Children == null)
            {
                var metaData = Data.LastOrDefault();
                if (metaData != null)
                {
                    instrument.InstrumentSeriesParams.PeriodStart = metaData.PeriodEnd;
                    instrument.InstrumentSeriesParams.PeriodEnd = DateTime.Today;
                }
            }

            var instrumentParamWindow = new InstrumentParamWindow(instrument);
            if (instrumentParamWindow.ShowDialog() == true)
            {
                var connection =
                    Session.Instance.GetConnection(instrument.InstrumentSeriesParams.Instrument.ConnectionId);

                var dataProvider =
                    SystemOptions.Instance.Providers.FirstOrDefault(x => x.DataProviderId == connection.DataProviderId);

                if (dataProvider == null)
                {
                    return;

                }

                var netName = (connection as ConnectionBase).IsTest ? "Testnet" : "Mainnet";
                var net = dataProvider.Children.FirstOrDefault(x => x.NetName == netName);
                if (net == null)
                {
                    net = new HistoricalData
                    {
                        NetName = netName,
                        Symbol = netName,
                        DataProviderId = connection.DataProviderId,
                        IsNet = true,
                        Parent = dataProvider,
                        Children = new ObservableCollection<HistoricalData>()

                    };
                    dataProvider.Children.Add(net);
                }
                net.IsExpanded = true;

                var typeName = instrument.InstrumentSeriesParams.Instrument.Type.ToUpper();
                var type = net.Children.FirstOrDefault(x => x.TypeName == typeName);

                if (type == null)
                {
                    type = new HistoricalData
                    {
                        NetName = netName,
                        Symbol = netName,
                        TypeName = typeName,
                        DataProviderId = connection.DataProviderId,
                        IsType = true,
                        Parent = net,
                        Children = new ObservableCollection<HistoricalData>()

                    };
                    net.Children.Add(type);
                }
                type.IsExpanded = true;

                var symbol = type.Children.FirstOrDefault(x =>
                x.Symbol == instrument.InstrumentSeriesParams.Instrument.Symbol);

                if (symbol == null)
                {
                    symbol = new HistoricalData
                    {
                        Symbol = instrument.InstrumentSeriesParams.Instrument.Symbol,
                        NetName = netName,
                        TypeName = typeName,
                        IsSymbol = true,
                        DataProviderId = connection.DataProviderId,
                        Children = new ObservableCollection<HistoricalData>()
                    };


                    type.Children.Add(symbol);
                    type.IsExpanded = true;
                    symbol.IsExpanded = true;
                }

                var child = symbol.Children.FirstOrDefault(x =>
                    x.DataSeriesType == instrument.InstrumentSeriesParams.DataSeriesType && x.DataSeriesValue ==
                    instrument.InstrumentSeriesParams.DataSeriesValue);
                if (child == null)
                {
                    child = new HistoricalData
                    {
                        Symbol = symbol.Symbol,
                        Parent = symbol,
                        NetName = netName,
                        TypeName = typeName,
                        DataSeriesValue = instrument.InstrumentSeriesParams.DataSeriesValue,
                        DataSeriesTypeStr = instrument.InstrumentSeriesParams.DataSeriesType.ToString(),
                        DataSeriesType = instrument.InstrumentSeriesParams.DataSeriesType,
                        DataProviderId = connection.DataProviderId
                    };
                    symbol.Children.Add(child);
                    child._removeFromParent = () => symbol.Children.Remove(child);
                    child.Data = new ObservableCollection<HistoricalMetaData>();
                }

                child.IsExpanded = true;
                child.IsSelected = true;

                var historicalData = child;
                historicalData.IsBusy = true;
                _ctsFetchData = new CancellationTokenSource();

                historicalData.IsBusy = true;
                SystemOptions.Instance.SelectedSymbol = historicalData;

                await Task.Run(() => LoadData(historicalData, instrument.InstrumentSeriesParams, _ctsFetchData.Token),
                    _ctsFetchData.Token).ContinueWith(
                    t => OnDataLoaded(historicalData, _ctsFetchData.Token),
                    TaskContinuationOptions.OnlyOnRanToCompletion);



            }
        }


        private void RemoveData(object parameter)
        {
            var historicalDataManager = new HistoricalDataManager();
            historicalDataManager.DeleteHistoricalData(this);

            if (IsProvider)
            {
                SystemOptions.Instance.Providers.Remove(this);
            }
            else
            {
                var dataProvider =
                    SystemOptions.Instance.Providers.FirstOrDefault(x => x.DataProviderId == DataProviderId);

                if (IsNet)
                {
                    dataProvider.Children.Remove(this);
                }
                else
                {

                    if (IsType)
                    {
                        var net = dataProvider.Children.FirstOrDefault(x => x == Parent);
                        net.Children.Remove(this);

                    }
                    else
                    {
                        HistoricalData symbol = null;
                        var parent = this.Parent;
                        while (symbol == null && parent == null)
                        {
                            if (parent.IsSymbol)
                            {
                                symbol = this.Parent;
                                break;
                            }
                            parent = this.Parent;
                        }

                        if (symbol != null)
                        {
                            if (IsSymbol || Parent.Children.Count == 1)
                            {
                                symbol.Children.Clear();
                                dataProvider.Children.Remove(symbol);
                            }
                            else
                            {
                                symbol.Children.Remove(this);
                            }
                        }
                    }
                }



            }
        }

        private async Task OnDataLoaded(HistoricalData historicalData, CancellationToken token)
        {
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                await Session.DbSemaphoreSlim.WaitAsync(token);
                IEnumerable<HistoricalMetaData> historicalMetaDatas;
                try
                {
                    using (var context = Session.Instance.DbContextFactory.GetContext())
                    {
                        historicalMetaDatas = context.HistoricalMetaDatas.Where(
                            x => x.Symbol == historicalData.Symbol &&
                                 x.DataType == "Candle" &&
                                 x.IsTest == (historicalData.NetName == "Testnet") &&
                                 x.InstrumentType == (historicalData.TypeName) &&
                                 x.DataProviderId == historicalData.DataProviderId &&
                                 x.DataSeriesValue == historicalData.DataSeriesValue &&
                                 x.DataSeriesType == historicalData.DataSeriesType.ToString()).ToList();
                    }
                }
                finally
                {
                    Session.DbSemaphoreSlim.Release(1);
                }

                if (historicalData.Data != null)
                {
                    historicalData.Data.Clear();
                    foreach (var historicalMetaData in historicalMetaDatas)
                    {
                        historicalData.Data.Add(historicalMetaData);
                    }
                }


                historicalData.IsBusy = false;
            });
        }

        public static ObservableCollection<HistoricalData> InitFromHistoricalDatas(
            IEnumerable<HistoricalMetaData> historicalMetaDatas)
        {
            var historicalData = new ObservableCollection<HistoricalData>();

            var symbolProviderMetaDatas = historicalMetaDatas.GroupBy(x => x.DataProviderId).OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Select(y => y));

            foreach (var symbolProviderMetaData in symbolProviderMetaDatas)
            {
                var dataProvider = Session.Instance.GetDataProvider(symbolProviderMetaData.Key);
                var providerData = new HistoricalData
                {
                    IsProvider = true,
                    Symbol = dataProvider.Code,
                    ProviderName = dataProvider.Code,
                    DataProviderId = symbolProviderMetaData.Key,
                    Children = new ObservableCollection<HistoricalData>()
                };
                historicalData.Add(providerData);
                providerData.IsExpanded = true;

                var symbolTestMetaDatas = symbolProviderMetaData.Value.GroupBy(x => x.IsTest).OrderBy(x => x.Key)
                    .ToDictionary(x => x.Key, x => x.Select(y => y));

                foreach (var symbolTestMetaData in symbolTestMetaDatas)
                {
                    bool isTest = symbolTestMetaData.Key;

                    var netData = new HistoricalData
                    {
                        NetName = isTest ? "Testnet" : "Mainnet",
                        Symbol = isTest ? "Testnet" : "Mainnet",
                        DataProviderId = dataProvider.Id,
                        ProviderName = dataProvider.Name,
                        IsNet = true,
                        Parent = providerData,
                        Children = new ObservableCollection<HistoricalData>()

                    };
                    providerData.Children.Add(netData);


                    var typeMetaDatas = symbolTestMetaData.Value.GroupBy(x => x.InstrumentType).OrderBy(x => x.Key)
                   .ToDictionary(x => x.Key, x => x.Select(y => y));

                    foreach (var typeMetaData in typeMetaDatas)
                    {
                        var typeData = new HistoricalData
                        {
                            DataProviderId = dataProvider.Id,
                            ProviderName = dataProvider.Name,
                            NetName = isTest ? "Testnet" : "Mainnet",
                            Symbol = typeMetaData.Key,
                            TypeName = typeMetaData.Key,
                            IsType = true,
                            Parent = providerData,
                            Children = new ObservableCollection<HistoricalData>()

                        };
                        netData.Children.Add(typeData);


                        var symbolMetaDatas = typeMetaData.Value.GroupBy(x => x.Symbol).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Select(y => y));

                        foreach (var symbolMetaData in symbolMetaDatas)
                        {
                            var symbolData = new HistoricalData
                            {
                                Symbol = symbolMetaData.Key,
                                ProviderName = dataProvider.Name,
                                DataProviderId = dataProvider.Id,
                                NetName = isTest ? "Testnet" : "Mainnet",
                                TypeName = typeMetaData.Key,
                                IsSymbol = true,
                                Parent = typeData,
                                Children = new ObservableCollection<HistoricalData>()
                            };

                            var symbolMetaDataSeries = symbolMetaData.Value
                                .GroupBy(x => new { x.DataSeriesValue, x.DataSeriesType })
                                .ToDictionary(x => x.Key, x => x.Select(y => y));

                            /*symbolData._removeFromParent = () => cache.Remove(symbolData);*/
                            foreach (var symbolRecord in symbolMetaDataSeries)
                            {
                                var symbolRecordData = new HistoricalData
                                {
                                    Symbol = symbolMetaData.Key,
                                    Parent = symbolData,
                                    ProviderName = dataProvider.Name,
                                    DataProviderId = dataProvider.Id,
                                    NetName = isTest ? "Testnet" : "Mainnet",
                                    TypeName = typeMetaData.Key,
                                    DataSeriesValue = symbolRecord.Key.DataSeriesValue,
                                    DataSeriesTypeStr = symbolRecord.Key.DataSeriesType
                                };

                                if (Enum.TryParse(symbolRecord.Key.DataSeriesType, out DataSeriesType dataSeriesValue))
                                {
                                    symbolRecordData.DataSeriesType = dataSeriesValue;
                                }

                                symbolRecordData.Data = new ObservableCollection<HistoricalMetaData>();

                                var orderedRecords = symbolRecord.Value.OrderBy(x => x.PeriodStart);

                                foreach (var record in orderedRecords)
                                {
                                    symbolRecordData.Data.Add(record);
                                }

                                symbolData.Children.Add(symbolRecordData);
                            }

                            typeData.Children.Add(symbolData);
                        }
                    }
                }
            }

            return historicalData;
        }


        public override string ToString()
        {
            return Data == null ? Symbol : $"({DataSeriesValue} {DataSeriesTypeStr})";
        }


        private CancellationTokenSource _ctsFetchData;

        private async Task LoadData(HistoricalData historicalData, DataSeriesParams dataSeriesParams, CancellationToken token)
        {
            var historicalDataManager = new HistoricalDataManager();
            try
            {
                historicalDataManager.SenderObject = historicalData;
                historicalDataManager.RaiseMessageEvent += HistoricalDataManagerRaiseMessageEvent;

                await historicalDataManager.GetData(dataSeriesParams, token, false);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                historicalDataManager.RaiseMessageEvent -= HistoricalDataManagerRaiseMessageEvent;
            }
        }

        private void HistoricalDataManagerRaiseMessageEvent(object sender, Messages.MessageEventArgs e)
        {
            var dataSeries = "";
            HistoricalData historicalData = null;
            if (sender is HistoricalDataManager hdm)
            {
                dataSeries = hdm.DataSeriesParams.DataSeriesName;
                historicalData = (HistoricalData)hdm.SenderObject;
            }

            if (historicalData != null)
            {
                historicalData.Message = dataSeries + " " + e.Message;
                historicalData.SubMessage = e.SubMessage;
            }
        }
    }
}