using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Twm.Core.Classes;
using Twm.Core.DataProviders.Common;
using Twm.Core.ViewModels.DataSeries;
using Twm.Core.ViewModels.Instruments;
using Twm.DB.DAL.Repositories.Instruments;

namespace Twm.Core.UI.Views
{
    /// <summary>
    /// Логика взаимодействия для ChartParamView.xaml
    /// </summary>
    public partial class ChartParamView : UserControl
    {
        private DataSeriesParamsViewModel _chartSeriesParamsViewModel;

        public static readonly DependencyProperty IsControlEnabledProperty
            = DependencyProperty.Register("IsControlEnabled", typeof(bool), typeof(ChartParamView), new UIPropertyMetadata(null));
        public bool IsControlEnabled
        {
            get { return (bool)GetValue(IsControlEnabledProperty); }
            set { SetValue(IsControlEnabledProperty, value); }
        }

        public ChartParamView()
        {
            InitializeComponent();
            DataContextChanged += ChartParamView_DataContextChanged;
        }

        private void ChartParamView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            IsControlEnabled = false;
            if (e.NewValue != null)
            {
                if (DataContext is ChartParamsViewModel chartParamsViewModel)
                {
                    chartParamsViewModel.ConnectionAndTypeChanged += ChartParamsViewModel_ConnectionAndTypeChanged;
                }
                else if (DataContext is DataSeriesParamsViewModel chartParamViewModel)
                {
                    _chartSeriesParamsViewModel = chartParamViewModel;
                    chartParamViewModel.PropertyChanged += ChartParamViewModel_PropertyChanged;
                    CreateMenu(chartParamViewModel);
        
                    IsControlEnabled = true;
                }
            }
            
        }

        private void ChartParamViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedConnection" || e.PropertyName == "SelectedType")
            {
                if (sender is DataSeriesParamsViewModel dsp)
                {
                    CreateMenu(dsp);
                }
            }
        }

        private void ChartParamsViewModel_ConnectionAndTypeChanged(object sender, System.EventArgs e)
        {
            if (sender is DataSeriesParamsViewModel dsp)
            {
                CreateMenu(dsp);
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
                _chartSeriesParamsViewModel?.SelectSymbol((InstrumentViewModel)miInstrument.Tag);
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            menu.IsOpen = true;
        }
    }
}
