using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using Twm.Core.Classes;
using Twm.Core.ViewModels;
using Twm.Core.ViewModels.Instruments;
using Twm.DB.DAL.Repositories.Instruments;
using Twm.Model.Model;
using Twm.Windows.Tools;

namespace Twm.ViewModels.Instruments
{
    public class InstrumentListsViewModel : ViewModelBase
    {
        public ObservableCollection<InstrumentListViewModel> InstrumentLists { get; set; }
        public ICollectionView InstrumentListsView { get; set; }

        private readonly object _instrumentListLock = new object();

        private InstrumentListViewModel _selectedInstrumentList;

        public InstrumentListViewModel SelectedInstrumentList
        {
            get { return _selectedInstrumentList; }
            set
            {
                if (_selectedInstrumentList != value)
                {
                    _selectedInstrumentList = value;
                    
                    if (_selectedInstrumentList != null)
                        Instruments.FetchData(_selectedInstrumentList.DataModel.Connection, _selectedInstrumentList.DataModel.Type);
                    OnPropertyChanged();
                }
            }
        }

        public InstrumentsViewModel Instruments { get; set; }

        public ICommand AddCommand { get; set; }
        public ICommand RemoveCommand { get; set; }

        public ICommand EditCommand { get; set; }
        public ICommand OKCommand { get; set; }
        public ICommand AddInstrumentToListCommand { get; set; }
        public ICommand RemoveInstrumentFromListCommand { get; set; }

        private CancellationTokenSource _ctsFetchData;

        public InstrumentListsViewModel()
        {
            InstrumentLists = new ObservableCollection<InstrumentListViewModel>();
            InstrumentListsView = CollectionViewSource.GetDefaultView(InstrumentLists);
            Instruments = new InstrumentsViewModel();
            BindingOperations.EnableCollectionSynchronization(InstrumentLists, _instrumentListLock);

            OKCommand = new OperationCommand(Close);
            AddCommand = new OperationCommand(AddInstrumentList);
            RemoveCommand = new OperationCommand(RemoveInstrumentList);
            EditCommand = new OperationCommand(EditInstrumentList);
            AddInstrumentToListCommand = new OperationCommand(AddInstrumentToList);
            RemoveInstrumentFromListCommand = new OperationCommand(RemoveInstrumentFromList);
        }

        private void AddInstrumentToList(object obj)
        {
            if (SelectedInstrumentList != null && Instruments.SelectedInstrument != null)
            {
                using (var context = Session.Instance.DbContextFactory.GetContext())
                {
                    var repository = new InstrumentInstrumentListRepository(context);

                    var instrumentViewModel = new InstrumentViewModel(Instruments.SelectedInstrument.DataModel);

                    var instrumentInstrumentList = new InstrumentInstrumentList();
                    instrumentInstrumentList.InstrumentId = instrumentViewModel.Id;
                    instrumentInstrumentList.InstrumentListId = SelectedInstrumentList.Id;
                    repository.Add(instrumentInstrumentList);
                    repository.CompleteAsync();

                    SelectedInstrumentList.AddInstrument(instrumentViewModel);
                    SelectedInstrumentList.SelectedInstrument = instrumentViewModel;
                }
            }
        }


        private void RemoveInstrumentFromList(object obj)
        {
            if (SelectedInstrumentList?.SelectedInstrument != null)
            {
                using (var context = Session.Instance.DbContextFactory.GetContext())
                {
                    var repository = new InstrumentInstrumentListRepository(context);

                    var link = repository.FindBy(x =>
                        x.InstrumentId == SelectedInstrumentList.SelectedInstrument.Id &&
                                         x.InstrumentListId == SelectedInstrumentList.Id).FirstOrDefault();

                    repository.Remove(link);
                    repository.CompleteAsync();


                    SelectedInstrumentList.Instruments.Remove(SelectedInstrumentList?.SelectedInstrument);
                    SelectedInstrumentList.SelectedInstrument = SelectedInstrumentList.Instruments.FirstOrDefault();
                }
            }
        }

        private void EditInstrumentList(object obj)
        {
            if (SelectedInstrumentList != null)
            {
                var instrumentListViewModel = (InstrumentListViewModel) SelectedInstrumentList.Clone();

                instrumentListViewModel.ConnectionEnabled = false;
                var instrumentListWindow = new InstrumentListWindow(instrumentListViewModel)
                    {Title = "Edit Instrument List"};
                

                if (instrumentListWindow.ShowDialog() == true)
                {
                    SelectedInstrumentList.CopyFrom(instrumentListViewModel);

                    using (var context = Session.Instance.DbContextFactory.GetContext())
                    {
                        var repository = new InstrumentListRepository(context);
                        repository.Update(SelectedInstrumentList.DataModel);
                    }

                    var configuredConnection = Session.Instance.ConfiguredConnections.FirstOrDefault(x => x.Id == instrumentListViewModel.ConnectionId);
                    if (configuredConnection != null)
                    {
                        instrumentListViewModel.DataModel.Connection = configuredConnection.DataModel;
                    }
                }
            }
        }

        private void AddInstrumentList(object obj)
        {
            var instrumentListViewModel = new InstrumentListViewModel();
            var instrumentListWindow = new InstrumentListWindow(instrumentListViewModel)
                {Title = "Add Instrument List"};
            if (instrumentListWindow.ShowDialog() == true)
            {
                using (var context = Session.Instance.DbContextFactory.GetContext())
                {
                    var repository = new InstrumentListRepository(context);

                    repository.Add(instrumentListViewModel.DataModel);
                    repository.CompleteAsync();
                    InstrumentLists.Add(instrumentListViewModel);
                    var configuredConnection = Session.Instance.ConfiguredConnections.FirstOrDefault(x => x.Id == instrumentListViewModel.ConnectionId);
                    if (configuredConnection != null)
                    {
                        instrumentListViewModel.DataModel.Connection = configuredConnection.DataModel;
                    }
                    
                    SelectedInstrumentList = instrumentListViewModel;
                }
            }
        }

        private void RemoveInstrumentList(object obj)
        {
            if (SelectedInstrumentList != null)
            {
                if (SelectedInstrumentList.DataModel.IsDefault)
                {
                    MessageBox.Show("Сan't delete the default list", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                    return;
                }

                using (var context = Session.Instance.DbContextFactory.GetContext())
                {
                    var repository = new InstrumentListRepository(context);

                    repository.Remove(SelectedInstrumentList.DataModel);
                    repository.CompleteAsync();
                    InstrumentLists.Remove(SelectedInstrumentList);
                    SelectedInstrumentList = InstrumentLists.FirstOrDefault();
                }
            }
        }

        private void Close(object obj)
        {
        }


        public async void FetchData()
        {
            _ctsFetchData = new CancellationTokenSource();
            await  FetchInstrumentLists(_ctsFetchData.Token);
            
        }

        private async Task FetchInstrumentLists(CancellationToken token)
        {
            InstrumentLists.Clear();

            using (var context = Session.Instance.DbContextFactory.GetContext())
            {
                var repository = new InstrumentListRepository(context);

                var instrumentLists =  repository.GetAll().Result.OrderBy(x=>x.Id);

                foreach (var instrumentList in instrumentLists)
                {
                    var instrumentListViewModel = new InstrumentListViewModel(instrumentList);

                    InstrumentLists.Add(instrumentListViewModel);
                }

                SelectedInstrumentList = InstrumentLists.FirstOrDefault();
            }
        }
    }
}