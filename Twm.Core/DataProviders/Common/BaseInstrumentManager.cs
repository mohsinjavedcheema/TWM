using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Xml;
using System.Xml.Serialization;
using Twm.Core.Classes;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.UI.Windows;
using Twm.Core.ViewModels;
using Twm.Model.Model;

namespace Twm.Core.DataProviders.Common
{
    public abstract class BaseInstrumentManager<TViewModel, TProviderInstrument> : ViewModelBase, IInstrumentManager where TViewModel : class where TProviderInstrument : class
    {
        private readonly ConnectionBase _connection;

        public event InstrumentsAdded InstrumentsAddedEvent;
        public ObservableCollection<TViewModel> Instruments { get; set; }
        public ICollectionView InstrumentsView { get; set; }

        private readonly object _logLock = new object();

        public GridColumnCustomization InstrumentColumnCustomization { get; set; }


        private TViewModel _selectedInstrument;
        public TViewModel SelectedInstrument
        {
            get { return _selectedInstrument; }
            set
            {
                if (_selectedInstrument == null && value == null)
                    return;

                if ((_selectedInstrument == null && value != null) || !_selectedInstrument.Equals(value))
                {
                    _selectedInstrument = value;
                    InstrumentsView.MoveCurrentTo(_selectedInstrument);
                    OnPropertyChanged();
                }
            }
        }


        private bool _isAddAllEnable;

        public bool IsAddAllEnable
        {
            get { return _isAddAllEnable; }
            set
            {
                if (_isAddAllEnable != value)
                {
                    _isAddAllEnable = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _isFindEnable;

        public bool IsFindEnable
        {
            get { return _isFindEnable; }
            set
            {
                if (_isFindEnable != value)
                {
                    _isFindEnable = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _isAddEnable;

        public bool IsAddEnable
        {
            get { return _isAddEnable; }
            set
            {
                if (_isAddEnable != value)
                {
                    _isAddEnable = value;
                    OnPropertyChanged();
                }
            }
        }


        public ObservableCollection<string> TypesItems { get; set; }

        protected string _selectedType;

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


        private string _instrument;

        public string Instrument
        {
            get { return _instrument; }
            set
            {
                if (!_instrument.Equals(value))
                {
                    bool needRefresh = (_instrument != _findHint && value != _findHint);
                    _instrument = value;
                    OnPropertyChanged();
                    if (needRefresh)
                        InstrumentsView.Refresh();
                }
            }
        }


        protected string _findHint;

        private CancellationTokenSource _ctsReloadData;

        public OperationCommand FindDataCommand { get; set; }

        public OperationCommand AddCommand { get; set; }

        public OperationCommand AddAllCommand { get; set; }

        public OperationCommand InstrumentSettingsCommand { get; set; }


        protected BaseInstrumentManager(ConnectionBase connection)
        {
            _connection = connection;
            _instrument = "";
            _findHint = "Find";
            Instruments = new ObservableCollection<TViewModel>();
            InstrumentsView = CollectionViewSource.GetDefaultView(Instruments);
            BindingOperations.EnableCollectionSynchronization(Instruments, _logLock);
            InstrumentsView.Filter += InstrumentsViewFilter;
            InstrumentsView.CurrentChanged += InstrumentsView_CurrentChanged;
            FillTypes();
            FindDataCommand = new OperationCommand(Find);
            AddCommand = new OperationCommand(AddInstrument);
            AddAllCommand = new OperationCommand(AddAllInstruments);
            InstrumentSettingsCommand = new OperationCommand(InstrumentSettings);
            IsAddEnable = false;
            IsAddAllEnable = false;
            IsFindEnable = true;

        }



        protected string Serialize(TProviderInstrument instrument)
        {
            if (instrument == null)
            {
                return string.Empty;
            }
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(TProviderInstrument));
                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    var emptyNs = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
                    xmlSerializer.Serialize(writer, instrument, emptyNs);
                    return stringWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred", ex);
            }
        }

        private void InstrumentSettings(object obj)
        {
            var columns = InstrumentColumnCustomization.Columns.ToList();

            var gridColumnInfos = new List<GridColumnInfo>();
            foreach (var column in columns)
            {
                gridColumnInfos.Add((GridColumnInfo)column.Clone());
            }

            var viewSettingsWindow = new ViewSettingsWindow(gridColumnInfos);
            if (viewSettingsWindow.ShowDialog() == true)
            {
                foreach (var column in InstrumentColumnCustomization.Columns)
                {
                    var gridColumnInfo = gridColumnInfos.Find(x => x.Name == column.Name);
                    column.Visibility = gridColumnInfo.Visibility;
                }

                InstrumentColumnCustomization.ReloadColumns();
                InstrumentColumnCustomization.UpdateProperty("Columns");
                InstrumentColumnCustomization.SaveLayout();
            }
        }

        protected abstract Instrument ProviderInstrumentToInstrument(TViewModel providerInstrumentVm);

        public abstract List<Instrument> ConvertToInstruments(List<object> insCollection, string type, List<string> symbolList = null);

        public abstract List<string> GetDefaultInstruments();


        private void AddAllInstruments(object obj)
        {
            if (SelectedInstrument != null)
            {
                var list = new List<Instrument>();
                bool hasErrors = false;
                foreach (var providerInstruemnt in Instruments)
                {
                    var instrument = ProviderInstrumentToInstrument(providerInstruemnt);
                    if (instrument != null)
                        list.Add(instrument);
                    else
                        hasErrors = true;
                }
                if (list.Any())
                    AddInstruments(list);

                if (hasErrors)
                    MessageBox.Show("Some instruments not found!");

            }
        }

        private void AddInstrument(object obj)
        {
            if (SelectedInstrument != null)
            {
                var instrument = ProviderInstrumentToInstrument(SelectedInstrument);

                if (instrument != null)
                    AddInstruments(new List<Instrument>() { instrument });
            }
        }

        public void AddInstruments(List<Instrument> instruments)
        {
            IsAddEnable = false;
            IsAddAllEnable = false;
            InstrumentsAddedEvent?.BeginInvoke(instruments, null, null);
        }


        private async void Find(object obj)
        {
            _ctsReloadData = new CancellationTokenSource();
            await Task.Run(FetchInstruments, _ctsReloadData.Token);
        }

        protected abstract void FillTypes();

        public abstract Task FetchInstruments();


        protected abstract bool FilterInstrument(object item);

        private bool InstrumentsViewFilter(object item)
        {
            return FilterInstrument(item);
        }

       


        private void InstrumentsView_CurrentChanged(object sender, EventArgs e)
        {
            var lcv = (ListCollectionView)InstrumentsView;
            if (lcv.Count > 0)
            {
                if (lcv.CurrentPosition != -1)
                {
                    SelectedInstrument = (TViewModel)lcv.CurrentItem;
                    CurrentPosition = lcv.CurrentPosition + 1;
                }
                else
                {
                    SelectedInstrument = (TViewModel)lcv.GetItemAt(0);
                    CurrentPosition = 1;
                }
            }
            else
            {
                SelectedInstrument = default;
                CurrentPosition = 0;
            }
        }


    }
}