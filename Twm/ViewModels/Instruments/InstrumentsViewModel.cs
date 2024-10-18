using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Twm.Core.Classes;
using Twm.Core.DataProviders.Binance;
using Twm.Core.DataProviders.Bybit;
using Twm.Core.DataProviders.Common;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.ViewModels;
using Twm.Core.ViewModels.Instruments;
using Twm.DB.DAL.Repositories.Instruments;
using Twm.Model.Model;



namespace Twm.ViewModels.Instruments
{
    public class InstrumentsViewModel : ViewModelBase
    {
        public List<InstrumentViewModel> Instruments { get; set; }

        private ICollectionView _instrumentsView;

        public ICollectionView InstrumentsView
        {
            get { return _instrumentsView; }
            set
            {
                if (_instrumentsView != value)
                {
                    _instrumentsView = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _connectionEnabled;
        public bool ConnectionEnabled
        {
            get { return _connectionEnabled; }
            set
            {
                if (_connectionEnabled != value)
                {
                    _connectionEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _typeEnabled;
        public bool TypeEnabled
        {
            get { return _typeEnabled; }
            set
            {
                if (_typeEnabled != value)
                {
                    _typeEnabled = value;
                    OnPropertyChanged();
                }
            }
        }




        private readonly object _logLock = new object();


        private readonly Dictionary<ConnectionBase, IInstrumentManager> _connectionInstrumentManagers =
            new Dictionary<ConnectionBase, IInstrumentManager>();

        private InstrumentViewModel _selectedInstrument;

        public InstrumentViewModel SelectedInstrument
        {
            get { return _selectedInstrument; }
            set
            {
                if (_selectedInstrument != value)
                {
                    _selectedInstrument = value;
                    InstrumentsView.MoveCurrentTo(_selectedInstrument);
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<ConnectionBase> Connections { get; set; }

        private ConnectionBase _selectedConnection;

        public ConnectionBase SelectedConnection
        {
            get { return _selectedConnection; }
            set
            {
                if (_selectedConnection != value)
                {
                    _selectedConnection = value;
                    OnPropertyChanged();
                    InstrumentsView.Refresh();
                    RefreshTypes();
                    if (!_connectionInstrumentManagers.ContainsKey(value))
                    {
                        var viewModel = _selectedConnection.CreateInstrumentManager();
                        _connectionInstrumentManagers.Add(value, viewModel);
                    }


                    if (!_connectionMaxCounts.ContainsKey(value.Id))
                    {
                        _connectionMaxCounts.Add(value.Id, 0);
                    }

                    MaxCount = _connectionMaxCounts[_selectedConnection.Id];
                    IsRemoveEnable = _connectionMaxCounts[_selectedConnection.Id] != 0;
                    IsRemoveAllEnable = _connectionMaxCounts[_selectedConnection.Id] != 0;


                    if (SelectedConnectionInstrumentManager != null)
                        SelectedConnectionInstrumentManager.InstrumentsAddedEvent -=
                            SelectedConnectionInstrumentManager_InstrumentsAddedEvent;
                    SelectedConnectionInstrumentManager = _connectionInstrumentManagers[value];
                    if (SelectedConnectionInstrumentManager != null)
                        SelectedConnectionInstrumentManager.InstrumentsAddedEvent +=
                        SelectedConnectionInstrumentManager_InstrumentsAddedEvent;

                    if (_selectedConnection.IsConnected)
                    {
                        _ctsFetchProviderData = new CancellationTokenSource();
                        Task.Run(() => FetchProviderInstruments(_ctsFetchProviderData.Token), _ctsFetchProviderData.Token);
                    }
                }
            }
        }


        private IInstrumentManager _selectedConnectionInstrumentManager;

        public IInstrumentManager SelectedConnectionInstrumentManager
        {
            get { return _selectedConnectionInstrumentManager; }
            set
            {
                if (_selectedConnectionInstrumentManager != value)
                {
                    _selectedConnectionInstrumentManager = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _isRemoveEnable;

        public bool IsRemoveEnable
        {
            get { return _isRemoveEnable; }
            set
            {
                if (_isRemoveEnable != value)
                {
                    _isRemoveEnable = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _isRemoveAllEnable;

        public bool IsRemoveAllEnable
        {
            get { return _isRemoveAllEnable; }
            set
            {
                if (_isRemoveAllEnable != value)
                {
                    _isRemoveAllEnable = value;
                    OnPropertyChanged();
                }
            }
        }


        public ObservableCollection<string> TypesItems { get; set; }

        private string _selectedType;
        public string SelectedType
        {
            get { return _selectedType; }
            set
            {
                if (_selectedType != value)
                {
                    _selectedType = value;
                    OnPropertyChanged();
                    InstrumentsView.Refresh();
                }
            }
        }


        private int _currentPosition;

        public int CurrentPosition
        {
            get { return _currentPosition; }
            set
            {
                if (_currentPosition != value)
                {
                    _currentPosition = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _maxCount;

        public int MaxCount
        {
            get { return _maxCount; }
            set
            {
                if (_maxCount != value)
                {
                    _maxCount = value;
                    OnPropertyChanged();
                }
            }
        }


        private string _instrumentFilterString;

        public string InstrumentFilterString
        {
            get { return _instrumentFilterString; }
            set
            {
                if (!_instrumentFilterString.Equals(value))
                {
                    bool needRefresh = (_instrumentFilterString != _findHint && value != _findHint);
                    _instrumentFilterString = value;
                    OnPropertyChanged();
                    if (needRefresh)
                        InstrumentsView.Refresh();
                }
            }
        }

        private readonly string _findHint;


        public OperationCommand CancelFetchDataCommand { get; set; }

        //public OperationCommand ReloadDataCommand { get; set; }

        public OperationCommand RemoveCommand { get; set; }

        public OperationCommand RemoveAllCommand { get; set; }


        private CancellationTokenSource _ctsFetchLocalData;

        private CancellationTokenSource _ctsFetchProviderData;

        //private CancellationTokenSource _ctsReloadData;

        //private CancellationTokenSource _ctsReloadFetchData;

        public InstrumentsViewModel()
        {
            ConnectionEnabled = true;

            Instruments = new List<InstrumentViewModel>();
            InstrumentsView = CollectionViewSource.GetDefaultView(Instruments);

            _instrumentFilterString = "";
            InstrumentsView.Filter += InstrumentsViewFilter;

            BindingOperations.EnableCollectionSynchronization(Instruments, _logLock);

            _findHint = "Find";
            InstrumentsView.CurrentChanged += InstrumentsView_CurrentChanged;

            TypesItems = new ObservableCollection<string>() { "FUTURE", "SPOT" };

            _selectedType = "FUTURE";
            _connectionMaxCounts = new Dictionary<int, int>();
            FillConnections();

            CancelFetchDataCommand = new OperationCommand(CancelFetchData);
            //ReloadDataCommand = new OperationCommand(ReloadData);

            RemoveCommand = new OperationCommand(RemoveInstrument);
            RemoveAllCommand = new OperationCommand(RemoveAllInstruments);
            IsRemoveEnable = false;
            IsRemoveAllEnable = false;
        }

        private void FillConnections()
        {
            var connections = Session.Instance.ConfiguredConnections;
            Connections = new ObservableCollection<ConnectionBase>();
            var orderedConnections = connections.Where(x => x.Id > 0).OrderBy(x => x.Id);
            foreach (var connection in orderedConnections)
            {
                Connections.Add(connection);
            }

            var currentConnection = Session.Connections.FirstOrDefault(x => x.Value.IsConnected);
            if (currentConnection.Value != null)
            {
                SelectedConnection = Connections.FirstOrDefault(x => x.Id == currentConnection.Key);
            }
            else
            {
                SelectedConnection = Connections.FirstOrDefault();
            }
        }

        private async void RemoveAllInstruments(object obj)
        {
            if (SelectedInstrument != null)
            {
                await Task.Run(async () =>
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        Message = "Removing all instruments...";

                        IsBusy = true;
                        IsRemoveEnable = false;
                        IsRemoveAllEnable = false;
                    });
                    try
                    {
                        var connection = Session.Connections[SelectedConnection.Id];

                        var manager = connection.CreateInstrumentManager();

                        using (var context = Session.Instance.DbContextFactory.GetContext())
                        {
                            var instrumentRepository = new InstrumentRepository(context);
                            instrumentRepository.RemoveAll(SelectedConnection.Id, manager.GetDefaultInstruments());
                            await instrumentRepository.CompleteAsync();
                        }

                        Instruments =
                            new List<InstrumentViewModel>(
                                Instruments.Where(x => x.ConnectionId != SelectedConnection.Id || manager.GetDefaultInstruments().Contains(x.Symbol)));


                        _connectionMaxCounts[_selectedConnection.Id] = Instruments.Count;
                        MaxCount = _connectionMaxCounts[_selectedConnection.Id];
                    }
                    finally
                    {
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            InstrumentsView = CollectionViewSource.GetDefaultView(Instruments);
                            InstrumentsView.Filter += InstrumentsViewFilter;
                            BindingOperations.EnableCollectionSynchronization(Instruments, _logLock);
                            InstrumentsView.CurrentChanged += InstrumentsView_CurrentChanged;
                            InstrumentsView.Refresh();
                            RefreshTypes();
                            IsBusy = false;
                            IsRemoveEnable = _connectionMaxCounts[_selectedConnection.Id] != 0;
                            IsRemoveAllEnable = _connectionMaxCounts[_selectedConnection.Id] != 0;
                        });
                    }
                });
            }
        }

        private void RefreshTypes()
        {

            /*while (TypesItems.Count != 1)
            {
                TypesItems.RemoveAt(1);
            }
           
            var view = InstrumentsView.Cast<InstrumentViewModel>();
            var types = view.GroupBy(x => x.Type).Select(x => x.Key).ToList();
            foreach (var type in types)
            {
                TypesItems.Add(type);
            }*/

        }

        private async void RemoveInstrument(object obj)
        {
            if (SelectedInstrument != null)
            {
                var connection = Session.Connections[SelectedInstrument.ConnectionId];
                var instrumentManager = connection.CreateInstrumentManager();

                if (instrumentManager.GetDefaultInstruments().Contains(SelectedInstrument.Symbol))
                {
                    MessageBox.Show("Сan't delete the default instrument", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                await Task.Run(async () =>
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        IsBusy = true;
                        IsRemoveEnable = false;
                        IsRemoveAllEnable = false;
                        Message = $"Remove {SelectedInstrument.Symbol}";
                    });

                    try
                    {
                        using (var context = Session.Instance.DbContextFactory.GetContext())
                        {
                            var instrumentRepository = new InstrumentRepository(context);
                            instrumentRepository.Remove(SelectedInstrument.DataModel);
                            await instrumentRepository.CompleteAsync();
                        }
                        Instruments.Remove(SelectedInstrument);
                        _connectionMaxCounts[_selectedConnection.Id]--;
                        MaxCount = _connectionMaxCounts[_selectedConnection.Id];
                    }
                    finally
                    {
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            SelectedInstrument = Instruments.FirstOrDefault();
                            InstrumentsView.Refresh();
                            RefreshTypes();
                            IsBusy = false;
                            IsRemoveEnable = _connectionMaxCounts[_selectedConnection.Id] != 0;
                            IsRemoveAllEnable = _connectionMaxCounts[_selectedConnection.Id] != 0;
                        });
                    }
                });
            }
        }

        private async void SelectedConnectionInstrumentManager_InstrumentsAddedEvent(List<Instrument> instruments)
        {
            await Task.Run(async () =>
            {
                Debug.WriteLine(instruments.Count);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        IsBusy = true;
                        SelectedConnectionInstrumentManager.IsAddEnable = false;
                        SelectedConnectionInstrumentManager.IsAddAllEnable = false;
                        SelectedConnectionInstrumentManager.IsFindEnable = false;
                        IsRemoveEnable = false;
                        IsRemoveAllEnable = false;
                    }
                );
                try
                {
                    var instrumentsCount = instruments.Count;
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            Message = $"Checking {instrumentsCount} instruments...";
                        }
                    );

                    var symbols = Instruments.Where(x => x.ConnectionId == SelectedConnection.Id).Select(x => $"{x.Symbol};{x.Type}")
                        .ToArray();

                    instruments.RemoveAll(x => symbols.Contains($"{x.Symbol};{x.Type}"));

                    var instrumentsToAdd = instruments.ToArray();

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            Message = $"Adding {instrumentsToAdd.Length} instruments...";
                        }
                    );

                    if (instrumentsToAdd.Any())
                    {
                        foreach (var instrument in instrumentsToAdd)
                        {
                            if (string.IsNullOrEmpty(instrument.PriceIncrements) && instrument.Type == "INDEX")
                            {
                                instrument.PriceIncrements = "1";
                            }
                        }


                        var length = instrumentsToAdd.Length;

                        var pageCount = (length / 10000) + 1;

                        using (var context = Session.Instance.DbContextFactory.GetContext())
                        {
                            var instrumentRepository = new InstrumentRepository(context);
                            var total = 0;
                            for (var i = 0; i < pageCount; i++)
                            {
                                var page = instrumentsToAdd.Skip(i * 10000).Take(10000).ToArray();

                                await instrumentRepository.Add(page);
                                await instrumentRepository.CompleteAsync();
                                total += page.Length;
                                await Application.Current.Dispatcher.InvokeAsync(() =>
                                    {
                                        SubMessage = $"{total} added";
                                    }
                                );
                            }
                        }


                        await Application.Current.Dispatcher.InvokeAsync(() => { Message = "Updating..."; }
                        );


                        var instrumentList = Instruments.ToList();
                        foreach (var instrumentToAdd in instrumentsToAdd)
                        {
                            instrumentList.Add(new InstrumentViewModel(instrumentToAdd));
                        }

                        var sortedInstrumentList = instrumentList.OrderBy(x => x.Symbol);

                        Instruments = new List<InstrumentViewModel>(sortedInstrumentList);


                        _connectionMaxCounts[_selectedConnection.Id] =
                            Instruments.Count(x => x.ConnectionId == _selectedConnection.Id);

                        MaxCount = _connectionMaxCounts[_selectedConnection.Id];


                    }
                }
                finally
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            if (SelectedInstrument == null)
                                SelectedInstrument = Instruments.FirstOrDefault();

                            InstrumentsView = CollectionViewSource.GetDefaultView(Instruments);
                            InstrumentsView.Filter += InstrumentsViewFilter;
                            BindingOperations.EnableCollectionSynchronization(Instruments, _logLock);
                            InstrumentsView.CurrentChanged += InstrumentsView_CurrentChanged;
                            InstrumentsView.Refresh();
                            RefreshTypes();
                            IsBusy = false;
                            SelectedConnectionInstrumentManager.IsAddEnable = true;
                            SelectedConnectionInstrumentManager.IsAddAllEnable = true;
                            SelectedConnectionInstrumentManager.IsFindEnable = true;
                            if (SelectedInstrument != null)
                            {
                                IsRemoveEnable = _connectionMaxCounts[_selectedConnection.Id] != 0;
                                IsRemoveAllEnable = _connectionMaxCounts[_selectedConnection.Id] != 0;
                            }

                            SubMessage = "";
                        }
                    );
                }
            }, CancellationToken.None);
        }







        private void CancelFetchData(object obj)
        {
            _ctsFetchLocalData?.Cancel();
            _ctsFetchProviderData?.Cancel();
        }

        private void InstrumentsView_CurrentChanged(object sender, System.EventArgs e)
        {
            var lcv = (ListCollectionView)InstrumentsView;
            if (lcv.Count > 0)
            {
                if (lcv.CurrentPosition != -1)
                {
                    SelectedInstrument = (InstrumentViewModel)lcv.CurrentItem;
                    CurrentPosition = lcv.CurrentPosition + 1;
                }
                else
                {
                    SelectedInstrument = (InstrumentViewModel)lcv.GetItemAt(0);
                    CurrentPosition = 1;
                }
            }
            else
            {
                SelectedInstrument = null;
                CurrentPosition = 0;
            }
        }

        private bool InstrumentsViewFilter(object item)
        {
            var instrument = item as InstrumentViewModel;
            if (instrument == null)
                return false;



            if ((instrument.Type.ToUpper() != SelectedType.ToUpper()))
            {
                return false;
            }

            if (SelectedConnection == null || (instrument.ConnectionId != SelectedConnection.Id))
            {
                return false;
            }

            if (string.IsNullOrEmpty(InstrumentFilterString) || _findHint == null ||
                InstrumentFilterString == _findHint)
                return true;


            if (
                (instrument.Symbol ?? "").ToUpper().Contains(InstrumentFilterString.ToUpper()))
            {
                return true;
            }

            return false;
        }


        public override void FetchData()
        {
            _ctsFetchLocalData = new CancellationTokenSource();
            Task.Run(() => FetchLocalInstruments(_ctsFetchLocalData.Token), _ctsFetchLocalData.Token);

            /*  _ctsFetchProviderData = new CancellationTokenSource();
              Task.Run(() => FetchProviderInstruments(_ctsFetchProviderData.Token), _ctsFetchProviderData.Token);*/
        }

        public void FetchData(Connection connection, string type)
        {
            _ctsFetchLocalData = new CancellationTokenSource();
            Task.Run(() => FetchLocalInstruments(_ctsFetchLocalData.Token, connection, type), _ctsFetchLocalData.Token);

            /*  _ctsFetchProviderData = new CancellationTokenSource();
              Task.Run(() => FetchProviderInstruments(_ctsFetchProviderData.Token), _ctsFetchProviderData.Token);*/
        }

        private Dictionary<int, int> _connectionMaxCounts;

        private async Task FetchLocalInstruments(CancellationToken token, Connection connection = null, string type = "")
        {
            Instruments.Clear();



            await Session.DbSemaphoreSlim.WaitAsync(token);
            try
            {
                using (var context = Session.Instance.DbContextFactory.GetContext())
                {
                    var pageItems = 10000;
                    var page = 0;
                    _connectionMaxCounts = context.Instruments.GroupBy(x => x.ConnectionId)
                        .ToDictionary(x => x.Key, y => y.Count());
                    int connectionId = SelectedConnection?.Id ?? 0;


                    if (_connectionMaxCounts.ContainsKey(connectionId))
                        MaxCount = _connectionMaxCounts[connectionId];


                    while (true)
                    {
                        if (token.IsCancellationRequested)
                            return;

                        List<Instrument> instruments;
                        if (connection == null)
                        {
                            if (string.IsNullOrEmpty(type))
                            {
                                instruments = context.Instruments.OrderBy(x => x.Symbol).Skip(page * pageItems)
                                   .Take(pageItems).ToList();
                            }
                            else
                                instruments = context.Instruments.Where(x => x.Type == type).OrderBy(x => x.Symbol).Skip(page * pageItems)
                                .Take(pageItems).ToList();
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(type))
                            {
                                instruments = context.Instruments.Where(x => x.ConnectionId == connection.Id).OrderBy(x => x.Symbol).Skip(page * pageItems)
                               .Take(pageItems).ToList();
                            }
                            else
                                instruments = context.Instruments.Where(x => x.ConnectionId == connection.Id && x.Type == type).OrderBy(x => x.Symbol).Skip(page * pageItems)
                               .Take(pageItems).ToList();

                        }

                        lock (_logLock)
                        {
                            Instruments.AddRange(instruments.Select(x => new InstrumentViewModel(x)));
                        }


                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            var selectedInstrument = SelectedInstrument;
                            InstrumentsView.Refresh();
                            RefreshTypes();
                            if (selectedInstrument != null)
                                SelectedInstrument = selectedInstrument;
                            if (SelectedInstrument != null)
                            {
                                IsRemoveEnable = true;
                                IsRemoveAllEnable = true;
                            }
                        });

                        if (page == 0)
                            await Application.Current.Dispatcher.InvokeAsync(() =>
                            {
                                InstrumentsView.MoveCurrentToFirst();
                            });

                        if (instruments.Count < pageItems)
                            break;

                        page++;
                    }


                    IsLoaded = true;
                }

                if (connection != null)
                {

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        _selectedConnection = Connections.FirstOrDefault(x => x.Id == connection.Id);
                        OnPropertyChanged("SelectedConnection");
                        if (!string.IsNullOrEmpty(type))
                            _selectedType = type;
                        OnPropertyChanged("SelectedType");
                        InstrumentsView.Refresh();
                    });


                }
                ConnectionEnabled = (connection == null);
                TypeEnabled = (connection == null);

            }
            catch (Exception ex)
            {

            }
            finally
            {
                Session.DbSemaphoreSlim.Release(1);
            }
        }


        private async Task FetchProviderInstruments(CancellationToken token)
        {
            try
            {

                if (_connectionInstrumentManagers[SelectedConnection] is BinanceInstrumentManager binanceInstrumentManager)
                {
                    await binanceInstrumentManager.FetchInstruments();
                }

                if (_connectionInstrumentManagers[SelectedConnection] is BybitInstrumentManager bybitInstrumentManager)
                {
                    await bybitInstrumentManager.FetchInstruments();
                }


            }
            catch (Exception ex)
            {

            }
            finally
            {

            }
        }
    }
}