using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Data;
using Twm.Chart.Annotations;
using Twm.Core.Classes;
using Twm.Core.DataProviders.Common;
using Twm.Model.Model;

namespace Twm.Core.ViewModels.Instruments
{
    public class InstrumentListViewModel:INotifyPropertyChanged, ICloneable
    {
        public InstrumentList DataModel { get; set; }


        public int Id
        {
            get { return DataModel.Id; }
            set
            {
                if (DataModel.Id != value)
                {
                    DataModel.Id = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get { return DataModel.Name; }
            set
            {
                if (DataModel.Name != value)
                {
                    DataModel.Name = value;
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


        public int ConnectionId
        {
            get { return DataModel.ConnectionId; }
            set
            {
                if (DataModel.ConnectionId != value)
                {
                    DataModel.ConnectionId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ConnectionName
        {
            get {
                if (DataModel.Connection == null)
                    return "";
                
                return DataModel.Connection.Name; }
            
        }

        public readonly object _instrumentLock = new object();

        public ObservableCollection<InstrumentViewModel> Instruments { get; set; }

        public ICollectionView InstrumentsView { get; set; }

        private InstrumentViewModel _selectedInstrument;

        public bool IsInstrument
        {
            get { return false; }

        }

        public InstrumentViewModel SelectedInstrument
        {
            get { return _selectedInstrument; }
            set
            {
                if (_selectedInstrument != value)
                {
                    _selectedInstrument = value;
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
                    ConnectionId = value.Id;
                    OnPropertyChanged();                    
                }
            }
        }


        public ObservableCollection<string> TypesItems { get; set; }

     

        public string Type
        {
            get { return DataModel.Type; }
            set
            {
                if (DataModel.Type != value)
                {
                    DataModel.Type = value;
                    OnPropertyChanged();
                }
            }
        }

        public InstrumentListViewModel(InstrumentList dataModel)
        {
            DataModel = dataModel;
            Instruments = new ObservableCollection<InstrumentViewModel>();
            InstrumentsView = CollectionViewSource.GetDefaultView(Instruments);

            InstrumentsView.CurrentChanged += InstrumentsView_CurrentChanged;
            ConnectionEnabled = true;
            if (dataModel.InstrumentInstrumentLists != null)
            {
                foreach (var InstrumentInstrumentList in dataModel.InstrumentInstrumentLists)
                {
                    //lock (_instrumentLock)
                    {
                        Instruments.Add(new InstrumentViewModel(InstrumentInstrumentList.Instrument));
                    }
                }
            }
            FillConnections();

            TypesItems = new ObservableCollection<string>() { "FUTURE", "SPOT" };
          
        }

        public InstrumentListViewModel() : this(new InstrumentList() { Type = "FUTURE"})
        {
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

            if (ConnectionId > 0)
            {
                SelectedConnection = Connections.FirstOrDefault(x => x.Id == ConnectionId);
              
            }
            else
            {
                var currentConnection = Session.Instance.Connections.FirstOrDefault(x => x.Value.IsConnected);
                if (currentConnection.Value != null)
                {
                    SelectedConnection = Connections.FirstOrDefault(x => x.Id == currentConnection.Key);
                }
                else
                {
                    SelectedConnection = Connections.FirstOrDefault();
                }

                ConnectionId = SelectedConnection.Id;
                
            }
           
        }

        private void InstrumentsView_CurrentChanged(object sender, System.EventArgs e)
        {
            var lcv = (ListCollectionView)InstrumentsView;
            if (lcv.Count > 0)
            {
                if (lcv.CurrentPosition != -1)
                {
                    SelectedInstrument = (InstrumentViewModel)lcv.CurrentItem;
                }
                else
                {
                    SelectedInstrument = (InstrumentViewModel)lcv.GetItemAt(0);
                }

            }
            else
            {
                SelectedInstrument = null;
            }
        }

        public object Clone()
        {
            var dataModel = (InstrumentList)DataModel.Clone();
            return new InstrumentListViewModel(dataModel);
        }

        public void CopyFrom(InstrumentListViewModel instrumentListViewModel)
        {
            Name = instrumentListViewModel.Name;
        }

        public void AddInstrument(InstrumentViewModel instrumentViewModel)
        {
            lock (_instrumentLock)
            {
                Instruments.Add(instrumentViewModel);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}