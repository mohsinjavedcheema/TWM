using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Twm.Core.Classes;
using Twm.Core.ViewModels;
using Twm.Core.ViewModels.Instruments;
using Twm.DB.DAL.Repositories.Instruments;
using Twm.Model.Model;
using Microsoft.Build.Framework;


namespace Twm.ViewModels.Instruments
{
    public class InstrumentsMapViewModel : ViewModelBase
    {
        private CancellationTokenSource _ctsFetchData;

        private readonly object _logLock = new object();

        public List<InstrumentMapViewModel> InstrumentMaps { get; set; }

        private ICollectionView _instrumentMapsView;

        public ICollectionView InstrumentMapsView
        {
            get { return _instrumentMapsView; }
            set
            {
                if (_instrumentMapsView != value)
                {
                    _instrumentMapsView = value;
                    OnPropertyChanged();
                }
            }
        }

        private InstrumentMapViewModel _selectedInstrumentMap;

        public InstrumentMapViewModel SelectedInstrumentMap
        {
            get { return _selectedInstrumentMap; }
            set
            {
                if (_selectedInstrumentMap != value)
                {
                    _selectedInstrumentMap = value;
                    InstrumentMapsView.MoveCurrentTo(_selectedInstrumentMap);
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

        private bool _isMapEnable;

        public bool IsMapEnable
        {
            get { return _isMapEnable; }
            set
            {
                if (_isMapEnable != value)
                {
                    _isMapEnable = value;
                    OnPropertyChanged();
                }
            }
        }


        public InstrumentsViewModel LeftInstruments { get; set; }
        public InstrumentsViewModel RightInstruments { get; set; }

        public OperationCommand CancelFetchDataCommand { get; set; }

        public OperationCommand MapCommand { get; set; }
        public OperationCommand UnmapCommand { get; set; }

        public InstrumentsMapViewModel()
        {
            InstrumentMaps = new List<InstrumentMapViewModel>();
            InstrumentMapsView = CollectionViewSource.GetDefaultView(InstrumentMaps);
            BindingOperations.EnableCollectionSynchronization(InstrumentMaps, _logLock);
            InstrumentMapsView.Filter += InstrumentMapsViewFilter;

            var leftConnection = Session.Instance.ConfiguredConnections.FirstOrDefault();
            var rightConnection = Session.Instance.ConfiguredConnections.FirstOrDefault(x => x != leftConnection);

            LeftInstruments = new InstrumentsViewModel() {SelectedConnection = leftConnection};
            LeftInstruments.PropertyChanged += Instruments_PropertyChanged;

            RightInstruments = new InstrumentsViewModel() {SelectedConnection = rightConnection};
            RightInstruments.PropertyChanged += Instruments_PropertyChanged;

            CancelFetchDataCommand = new OperationCommand(CancelFetchData);
            MapCommand = new OperationCommand(MapInstruments);
            UnmapCommand = new OperationCommand(UnmapInstruments);

            IsRemoveEnable = false;
            IsRemoveAllEnable = false;
        }

        private void Instruments_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedConnection" || e.PropertyName == "SelectedInstrument")
            {
                if (e.PropertyName == "SelectedConnection")
                    InstrumentMapsView.Refresh();

                IsMapEnable = (LeftInstruments.SelectedConnection != null &&
                               RightInstruments.SelectedConnection != null &&
                               LeftInstruments.SelectedConnection.Id != RightInstruments.SelectedConnection.Id &&
                               LeftInstruments.SelectedInstrument != null &&
                               RightInstruments.SelectedInstrument != null
                    );
            }
        }

        private bool InstrumentMapsViewFilter(object item)
        {
            var instrumentMap = item as InstrumentMapViewModel;
            if (instrumentMap == null)
                return false;

            if (LeftInstruments.SelectedConnection == null || RightInstruments.SelectedConnection == null)
            {
                return false;
            }

            if (instrumentMap.FirstInstrument.ConnectionId == LeftInstruments.SelectedConnection.Id &&
                instrumentMap.SecondInstrument.ConnectionId == RightInstruments.SelectedConnection.Id)

            {
                return true;
            }


            if (instrumentMap.FirstInstrument.ConnectionId == RightInstruments.SelectedConnection.Id &&
                instrumentMap.SecondInstrument.ConnectionId == LeftInstruments.SelectedConnection.Id)
            {
                return true;
            }

            return false;
        }

        private async void MapInstruments(object obj)
        {
            if (LeftInstruments.SelectedInstrument != null && RightInstruments.SelectedInstrument != null)
            {
                var instrumentMapVm = InstrumentMaps.FirstOrDefault(x =>
                    (x.FirstInstrument.Id == LeftInstruments.SelectedInstrument.Id &&
                     x.SecondInstrument.Id == RightInstruments.SelectedInstrument.Id) ||
                    (x.FirstInstrument.Id == RightInstruments.SelectedInstrument.Id &&
                     x.SecondInstrument.Id == LeftInstruments.SelectedInstrument.Id)
                );

                if (instrumentMapVm != null)
                {
                    SelectedInstrumentMap = instrumentMapVm;
                    MessageBox.Show("This map already exist");
                    return;
                }

                instrumentMapVm = InstrumentMaps.FirstOrDefault(x =>
                    (x.FirstInstrument.Id == LeftInstruments.SelectedInstrument.Id &&
                     x.SecondInstrument.ConnectionId == RightInstruments.SelectedInstrument.ConnectionId) ||
                    (x.FirstInstrument.Id == RightInstruments.SelectedInstrument.Id &&
                     x.SecondInstrument.ConnectionId == LeftInstruments.SelectedInstrument.ConnectionId) ||
                    (x.SecondInstrument.Id == LeftInstruments.SelectedInstrument.Id &&
                     x.FirstInstrument.ConnectionId == RightInstruments.SelectedInstrument.ConnectionId) ||
                    (x.SecondInstrument.Id == RightInstruments.SelectedInstrument.Id &&
                     x.FirstInstrument.ConnectionId == LeftInstruments.SelectedInstrument.ConnectionId) 
                );

                if (instrumentMapVm != null)
                {
                    SelectedInstrumentMap = instrumentMapVm;
                    MessageBox.Show("One of the instruments is already mapped to instrument from another connection");
                    return;
                }

                var instrumentMap = new InstrumentMap()
                {
                    FirstInstrumentId = LeftInstruments.SelectedInstrument.Id,
                    SecondInstrumentId = RightInstruments.SelectedInstrument.Id,
                };

                await Session.DbSemaphoreSlim.WaitAsync(new CancellationTokenSource().Token);
                try
                {
                    using (var context = Session.Instance.DbContextFactory.GetContext())
                    {
                        var instrumentMapRepository = new InstrumentMapRepository(context);
                        await instrumentMapRepository.Add(instrumentMap);
                        await instrumentMapRepository.CompleteAsync();
                    }
                }
                finally
                {
                    Session.DbSemaphoreSlim.Release(1);
                }

                instrumentMap.FirstInstrument = LeftInstruments.SelectedInstrument.DataModel;
                instrumentMap.SecondInstrument = RightInstruments.SelectedInstrument.DataModel;

                instrumentMapVm = new InstrumentMapViewModel(instrumentMap);
                InstrumentMaps.Add(instrumentMapVm);
                InstrumentMapsView.Refresh();
                SelectedInstrumentMap = instrumentMapVm;
            }
        }


        private async void UnmapInstruments(object obj)
        {
            if (SelectedInstrumentMap != null)
            {
                await Session.DbSemaphoreSlim.WaitAsync(new CancellationTokenSource().Token);
                try
                {
                    using (var context = Session.Instance.DbContextFactory.GetContext())
                    {
                        var instrumentMapRepository = new InstrumentMapRepository(context);

                        instrumentMapRepository.Remove(SelectedInstrumentMap.DataModel);
                        await instrumentMapRepository.CompleteAsync();
                    }
                }
                finally
                {
                    Session.DbSemaphoreSlim.Release(1);
                }

                InstrumentMaps.Remove(SelectedInstrumentMap);
                InstrumentMapsView.Refresh();
                InstrumentMapsView.MoveCurrentToFirst();
                if (InstrumentMapsView.CurrentItem != null)
                    SelectedInstrumentMap = (InstrumentMapViewModel)InstrumentMapsView.CurrentItem;
            }

        }

        private void CancelFetchData(object obj)
        {
            LeftInstruments.CancelFetchDataCommand.Execute(null);
            RightInstruments.CancelFetchDataCommand.Execute(null);
        }

        public override void FetchData()
        {
            LeftInstruments.FetchData();
            RightInstruments.FetchData();
            _ctsFetchData = new CancellationTokenSource();
            Task.Run(() => FetchInstrumentMaps(_ctsFetchData.Token), _ctsFetchData.Token);
        }

    

        private async Task FetchInstrumentMaps(CancellationToken token)
        {
            InstrumentMaps.Clear();

            await Session.DbSemaphoreSlim.WaitAsync(token);
            try
            {
                using (var context = Session.Instance.DbContextFactory.GetContext())
                {
                    var pageItems = 10000;
                    var page = 0;
                   
                    var instrumentMapRepository = new InstrumentMapRepository(context);


                    while (true)
                    {
                        if (token.IsCancellationRequested)
                            return;


                        var instrumentMaps = (await instrumentMapRepository.GetPage(page, pageItems)).ToList();

                        lock (_logLock)
                        {
                            InstrumentMaps.AddRange(instrumentMaps.Select(x => new InstrumentMapViewModel(x)));
                        }


                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            var selectedInstrument = SelectedInstrumentMap;
                            InstrumentMapsView.Refresh();
                            if (selectedInstrument != null)
                                SelectedInstrumentMap = selectedInstrument;
                            if (SelectedInstrumentMap != null)
                            {
                                IsRemoveEnable = true;
                                IsRemoveAllEnable = true;
                            }
                        });

                        if (page == 0)
                            await Application.Current.Dispatcher.InvokeAsync(() =>
                            {
                                InstrumentMapsView.MoveCurrentToFirst();
                            });

                        if (instrumentMaps.Count < pageItems)
                            break;

                        page++;
                    }


                    IsLoaded = true;
                }
            }
            finally
            {
                Session.DbSemaphoreSlim.Release(1);
            }
        }
    }
}