using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Twm.Core.Classes;
using Twm.Core.DataProviders.Common;
using Twm.Core.ViewModels;
using Twm.Core.ViewModels.DataSeries;
using Twm.Core.ViewModels.Instruments;
using Twm.DB.DAL.Repositories.Instruments;

namespace Twm.Core.CustomProperties.Controls
{
    /// <summary>
    /// Логика взаимодействия для DataSeriesControl.xaml
    /// </summary>
    public partial class DataSeriesControl : UserControl
    {
        private DataSeriesParamsViewModel _dataSeriesParamsViewModel;

        public DataSeriesControl()
        {
            InitializeComponent();
            DataContextChanged += DataSeriesControl_DataContextChanged;
        }

        private void DataSeriesControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is DataSeriesParamsViewModel dataSeriesParamViewModel)
            {
                _dataSeriesParamsViewModel = dataSeriesParamViewModel;
                _dataSeriesParamsViewModel.PropertyChanged += _dataSeriesParamsViewModel_PropertyChanged;
                CreateMenu(_dataSeriesParamsViewModel);


            }
        }

        private void _dataSeriesParamsViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is DataSeriesParamsViewModel dsp)
            {
                if (e.PropertyName == "IsInstrumentEnabled")
                {
                    if  (dsp.IsInstrumentEnabled)
                        CreateMenu(dsp);
                }
            }
        }

      

        private void CreateMenu(DataSeriesParamsViewModel dsp)
        {
            List<InstrumentListViewModel> instrumentListsVm = new List<InstrumentListViewModel>();

            using (var context = Session.Instance.DbContextFactory.GetContext())
            {
                var repository = new InstrumentListRepository(context);
                var instrumentLists = repository.GetAll().Result;

                foreach (var instrumentList in instrumentLists)
                {
                    instrumentListsVm.Add(new InstrumentListViewModel(instrumentList));
                }
            }
            CreateInstrumentMenu(instrumentListsVm, dsp.SelectedConnection, dsp.SelectedType);
        }


        public void CreateInstrumentMenu(IEnumerable<InstrumentListViewModel> instrumentLists, ConnectionBase connection, string type)
        {
            var instrumnetLists = instrumentLists.Where(x => x.ConnectionId == connection.Id && x.Type == type);
            menu.Items.Clear();


            foreach (var instrumentList in instrumnetLists)
            {
                var miInstrumentList = new MenuItem { Header = instrumentList.Name };

                foreach (var instrument in instrumentList.Instruments)
                {
                    var miInstrument = new MenuItem { Header = instrument.Symbol, Tag = instrument };
                    miInstrument.Click += MiInstrument_Click;
                    miInstrumentList.Items.Add(miInstrument);
                }

                if (miInstrumentList.Items.Count == 0)
                {
                    miInstrumentList.IsEnabled = false;
                }

                menu.Items.Add(miInstrumentList);
            }

        }


        private void MiInstrument_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem miInstrument)
            {
                _dataSeriesParamsViewModel?.SelectSymbol((InstrumentViewModel)miInstrument.Tag);
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            menu.IsOpen = true;
        }
    }
}
