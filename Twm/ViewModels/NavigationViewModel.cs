using System.Linq;
using System.Windows.Input;
using Twm.Core.Classes;
using Twm.Core.Controllers;
using Twm.Core.ViewModels;
using Twm.Core.ViewModels.DataSeries;
using Twm.ViewModels.Charts;
using Twm.ViewModels.Connections;
using Twm.ViewModels.Instruments;
using Twm.ViewModels.Options;
using Twm.ViewModels.Strategies.Optimizer;
using Twm.ViewModels.Strategies.Validator;
using Twm.Windows.Charts;
using Twm.Windows.Connections;
using Twm.Windows.Database;
using Twm.Windows.Help;
using Twm.Windows.Strategies;
using Twm.Windows.Tools;
using DebugWindow = Twm.Windows.DebugWindow;
using InstrumentsWindow = Twm.Windows.Tools.InstrumentsWindow;

namespace Twm.ViewModels
{
    public class NavigationViewModel : ViewModelBase
    {
        private ViewModelBase _selectedViewModel;

        public ViewModelBase SelectedViewModel
        {
            get { return _selectedViewModel; }

            set
            {
                _selectedViewModel = value;
                OnPropertyChanged();
            }
        }


        public ICommand AboutCommand { get; set; }

        public ICommand NewChartCommand { get; set; }

        public ICommand NewOrderBookCommand { get; set; }
        //public ICommand NewChart2Command { get; set; }
        public ICommand NewDebugCommand { get; set; }
        public ICommand CompileProjectCommand { get; set; }
        public ICommand ConfigureConnectionCommand { get; set; }
       
        public ICommand OptionsCommand { get; set; }
        public ICommand InstrumentsCommand { get; set; }
        public ICommand InstrumentsMapCommand { get; set; }
        public ICommand InstrumentListsCommand { get; set; }
        public ICommand ValidatorCommand { get; set; }
        public ICommand OptimizerCommand { get; set; }
        public ICommand DatabaseCommand { get; set; }

        public ICommand NewStrategyCommand { get; set; }

        public NavigationViewModel()
        {
            NewChartCommand = new OperationCommand(NewChart);

            NewOrderBookCommand = new OperationCommand(NewOrderBook);
            //NewChart2Command = new OperationCommand(NewChart2);
            NewDebugCommand = new OperationCommand(NewDebug);
            ConfigureConnectionCommand = new OperationCommand(ConfigureConnection);
            CompileProjectCommand = new OperationCommand(CompileProject);
            OptionsCommand = new OperationCommand(Options);
            InstrumentsCommand = new OperationCommand(Instruments);
            InstrumentListsCommand = new OperationCommand(InstrumentLists);
            ValidatorCommand = new OperationCommand(NewValidator);
            OptimizerCommand = new OperationCommand(NewOptimizer);
            DatabaseCommand = new OperationCommand(ViewDatabase);
            AboutCommand = new OperationCommand(About);
        }



        private void About(object obj)
        {
            var aboutViewModel = new AboutViewModel();
            var aboutWindow = new AboutWindow(aboutViewModel);
            aboutWindow.ShowDialog();
         }

        private void NewValidator(object obj)
        {
            var validatorViewModel = new ValidatorViewModel();
            var validatorWindow = new ValidatorWindow(validatorViewModel);
            validatorWindow.Show();
        }


        private void NewOptimizer(object obj)
        {
            var optimizerViewModel = new OptimizerViewModel();
            var optimizerWindow = new OptimizerWindow(optimizerViewModel);
            optimizerWindow.Show();
        }

        private void Options(object obj)
        {
            var systemOptionsViewModel = new SystemOptionsViewModel();
            using (var context = Session.Instance.DbContextFactory.GetContext())
            {
                SystemOptions.Instance.InitHistoricalData(context.HistoricalMetaDatas.ToList());
            }

            var systemOptionsWindow = new SystemOptionsWindow(systemOptionsViewModel);
            if (systemOptionsWindow.ShowDialog() == true)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    systemOptionsViewModel.SaveOptions();
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
            else
            {
                systemOptionsViewModel.ResetOptions();
            }
        }

        private async void Instruments(object obj)
        {
            var instrumentsViewModel = new InstrumentsViewModel();
            instrumentsViewModel.FetchData();
            var instrumentsWindow = new InstrumentsWindow(instrumentsViewModel);
            instrumentsWindow.ShowDialog();
        }

       

        private async void InstrumentLists(object obj)
        {
            var instrumentListsViewModel = new InstrumentListsViewModel();
            instrumentListsViewModel.FetchData();
            var instrumentListsWindow = new InstrumentListsWindow(instrumentListsViewModel);
            instrumentListsWindow.ShowDialog();
        }

        private void CompileProject(object obj)
        {
            BuildController.Instance.CompileProject();
        }

        private void ConfigureConnection(object obj)
        {
            var configureConnectionViewModel = new ConfigureConnectionViewModel();
            configureConnectionViewModel.Init();
            var connectionWindow = new ConnectionWindow(configureConnectionViewModel);
            if (connectionWindow.ShowDialog() == true)
            {
                configureConnectionViewModel.Save();
                App.CreateConnectionsMenu();
            }
        }


        /*private void StartStopConnection(object obj)
        {
            if (obj != null)
            {
                var connectionName = obj.ToString();

                var configuredConnection = Session.Instance.ConfiguredConnections.FirstOrDefault(x => x.Name == connectionName);
                {
                    if (configuredConnection != null)
                    {
                        if (!configuredConnection.IsConnected)
                        {
                            configuredConnection.Connect();
                        }
                        else
                        {
                            configuredConnection.Disconnect();
                        }
                    }
                }
            }
        }*/

        private void NewChart(object obj)
        {

            var chartParamsViewModel = new ChartParamsViewModel();
            chartParamsViewModel.FetchData();
            var chartParamsWindow = new ChartParamsWindow(chartParamsViewModel);

            if (chartParamsWindow.ShowDialog() == true)
            {
                var charTwm = new ChartViewModel(chartParamsViewModel);
                var chartWindow = new ChartWindow(charTwm);
                chartWindow.Show();
            }
        }

        private void NewOrderBook(object obj)
        {
            var domViewModel = new DomViewModel();        
            var orderBookWindow = new OrderBookWindow(domViewModel);
            orderBookWindow.Show();
         
        }






        private void NewDebug(object obj)
        {
            
            var debugWindow = new DebugWindow();
            debugWindow.Show();

        }


        private void ViewDatabase(object obj)
        {
            var databaseWindow = new DatabaseWindow();
            databaseWindow.Show();

        }
    }
}