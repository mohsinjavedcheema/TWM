using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Twm.Core.Classes;
using Twm.Core.Controllers;
using Twm.Core.Enums;
using Twm.Core.Helpers;
using Twm.Core.UI.Windows.Presets;
using Twm.Core.ViewModels.Instruments;
using Twm.Core.ViewModels.Presets;
using Twm.DB.DAL.Repositories.Instruments;
using Twm.DB.DAL.Repositories.Presets;
using Twm.Model.Model;

namespace Twm.Core.ViewModels.DataSeries
{
    public class ChartParamsViewModel:ViewModelBase
    {

        public event EventHandler ConnectionAndTypeChanged;       


        public ObservableCollection<InstrumentListViewModel> InstrumentLists { get; set; }

        public TrulyObservableCollection<DataSeriesParamsViewModel> ChartParams{get; set;}
        public ICollectionView ChartParamsView { get; set; }

        private DataSeriesParamsViewModel _selectedChartParams;

        public DataSeriesParamsViewModel SelectedChartParams
        {
            get { return _selectedChartParams; }
            set
            {
                if (_selectedChartParams != value)
                {
                    _selectedChartParams = value;
                    OnPropertyChanged();
                    OnPropertyChanged("IsPresetSaveEnable");
                }
            } 

        }


        private bool _isAddChartParamEnabled;
        public bool IsAddChartParamEnabled
        {
            get { return _isAddChartParamEnabled; }
            set
            {
                if (_isAddChartParamEnabled != value)
                {
                    _isAddChartParamEnabled = value;
                    OnPropertyChanged();                    
                }
            }
        }

        public bool IsPresetSaveEnable
        {
            get { return _selectedChartParams != null; }
        }

        public ICommand AddChartParamCommand { get; set; }

        public ICommand RemoveChartParamCommand { get; set; }

        public ICommand OKCommand { get; set; }

        public OperationCommand ChartParamsPresetLoadCommand { get; set; }
        public OperationCommand ChartParamsPresetSaveCommand { get; set; }
        public OperationCommand ChartParamsPresetExportCommand { get; set; }

        public ChartParamsViewModel()
        {
            InstrumentLists = new ObservableCollection<InstrumentListViewModel>();

            ChartParams = new TrulyObservableCollection<DataSeriesParamsViewModel>();
            ChartParamsView = CollectionViewSource.GetDefaultView(ChartParams);

            AddChartParamCommand = new OperationCommand(AddChartParam);
            RemoveChartParamCommand = new OperationCommand(RemoveChartParam);
            OKCommand = new OperationCommand(Cancel, ValidateOperationParams);

            ChartParamsPresetLoadCommand = new OperationCommand(ChartParamsPresetLoad);
            ChartParamsPresetSaveCommand = new OperationCommand(ChartParamsPresetSave);
            ChartParamsPresetExportCommand = new OperationCommand(ChartParamsPresetExport);

            IsAddChartParamEnabled = !ChartParams.Any();
        }

        private bool ValidateOperationParams(object arg)
        {
            return true;
        }

        private void Cancel(object obj)
        {
            
        }

        private void RemoveChartParam(object obj)
        {
            if (SelectedChartParams != null)
            {
                SelectedChartParams.PropertyChanged -= ChartParamViewModel_PropertyChanged;
                ChartParams.Remove(SelectedChartParams);                
                SelectedChartParams = ChartParams.FirstOrDefault();
                IsAddChartParamEnabled = !ChartParams.Any();
            }
        }

        private void AddChartParam(object obj)
        {
            var chartParamViewModel = new DataSeriesParamsViewModel();
            ChartParams.Add(chartParamViewModel);
            chartParamViewModel.PropertyChanged += ChartParamViewModel_PropertyChanged;
            SelectedChartParams = chartParamViewModel;
            IsAddChartParamEnabled = !ChartParams.Any();
        }

        private void ChartParamViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedConnection" || e.PropertyName == "SelectedType")
            {
                if (sender is DataSeriesParamsViewModel dsp)
                {
                    ConnectionAndTypeChanged?.Invoke(sender, new EventArgs());
                }
            }
        }

        public override void FetchData()
        {
            using (var context = Session.Instance.DbContextFactory.GetContext())
            {
                var repository = new InstrumentListRepository(context);
                var instrumentLists = repository.GetAll().Result;

                foreach (var instrumentList in instrumentLists)
                {
                    InstrumentLists.Add(new InstrumentListViewModel(instrumentList));
                }

            }
            IsAddChartParamEnabled = !ChartParams.Any();
        }


        private async void ChartParamsPresetSave(object obj)
        {
            if (SelectedChartParams != null)
            {
                var presetsViewModel = new PresetsViewModel(PresetType.Chart);
                presetsViewModel.FetchData();
                presetsViewModel.PresetName = SelectedChartParams.DisplayName;

                var presetsWindow = new PresetsWindow(presetsViewModel);
                if (presetsWindow.ShowDialog() == true)
                {
                    Preset preset = null;
                    if (presetsViewModel.SelectedPreset != null)
                        preset = presetsViewModel.SelectedPreset.DataModel;
                    await SaveChartParamPreset(SelectedChartParams, preset, presetsViewModel.PresetName);
                }
                
            }
        }


        private async Task SaveChartParamPreset(DataSeriesParamsViewModel dataSeriesParams, Preset preset, string name)
        {
            using (var context = Session.Instance.DbContextFactory.GetContext())
            {
                var repository = new PresetRepository(context);

                if (preset == null)
                {
                    preset = new Preset()
                    {
                        Name = name,
                        Type = (int)PresetType.Chart,
                        Data = JsonHelper.ToJson(new PresetObject<DataSeriesParamsViewModel>()
                            { Object = dataSeriesParams, PresetType = PresetType.Chart })
                    };
                    await repository.Add(preset);
                }
                else
                {
                    preset.Data = JsonHelper.ToJson(new PresetObject<DataSeriesParamsViewModel>()
                        { Object = dataSeriesParams, PresetType = PresetType.Chart });
                    await repository.Update(preset);

                }
                await repository.CompleteAsync();
               
            }
        }

        private void ChartParamsPresetLoad(object obj)
        {
            var presetsViewModel = new PresetsViewModel(PresetType.Chart, PresetFormMode.Load);

            presetsViewModel.FetchData();

           
            presetsViewModel.SelectedPreset = presetsViewModel.Presets.FirstOrDefault();
            

            var presetsWindow = new PresetsWindow(presetsViewModel);
            if (presetsWindow.ShowDialog() == true)
            {
                if (presetsViewModel.SelectedPreset != null &&
                    !string.IsNullOrEmpty(presetsViewModel.SelectedPreset.Data))
                {
                    ChartParams.Clear();

                    DataSeriesParamsViewModel dataSeriesParams = CreateDataSeriesParamByPreset(presetsViewModel.SelectedPreset.Data, presetsViewModel.IsLivePreset);

                    if (dataSeriesParams != null)
                    {
                        ChartParams.Add(dataSeriesParams);
                        SelectedChartParams = dataSeriesParams;
                        OnPropertyChanged("ChartParams");

                    }
                }
            }
        }

        private DataSeriesParamsViewModel CreateDataSeriesParamByPreset(string json, bool isSync)
        {
            DataSeriesParamsViewModel dataSeriesParams = null;
            try
            {
                var presetObject =
                    JsonHelper.ToObject<PresetObject<DataSeriesParamsEmpty>>(json);

                var playback = Session.Instance.Playback;

                if (playback != null && playback.IsConnected)
                {
                    presetObject.Object.PeriodEnd = playback.PeriodStart;
                }
                else
                {
                    if (isSync)
                        presetObject.Object.PeriodEnd = DateTime.Now;
                }



                dataSeriesParams = new DataSeriesParamsViewModel();
                dataSeriesParams.SelectedTimeFrameBase = presetObject.Object.SelectedTimeFrameBase;
                dataSeriesParams.PeriodEnd = presetObject.Object.PeriodEnd;
                dataSeriesParams.DaysToLoad = presetObject.Object.DaysToLoad;
                dataSeriesParams.Instrument = presetObject.Object.Instrument;
                dataSeriesParams.DataSeriesType = presetObject.Object.DataSeriesType;
                dataSeriesParams.DataSeriesValue = presetObject.Object.DataSeriesValue;
                




            }
            catch (Exception ex)
            {
                MessageBox.Show("Can`t load preset: " + ex.Message);
                LogController.Print("Can`t load preset: " + ex.Message + "\r\n" + ex.StackTrace);
            }

            return dataSeriesParams;
        }

        private void ChartParamsPresetExport(object obj)
        {
            var presetsViewModel = new PresetsViewModel(PresetType.Chart, PresetFormMode.Export);
            presetsViewModel.FetchData();
            presetsViewModel.SelectedPreset = presetsViewModel.Presets.FirstOrDefault();

            var presetsWindow = new PresetsWindow(presetsViewModel);
            if (presetsWindow.ShowDialog() == true)
            {
                if (presetsViewModel.SelectedPreset != null &&
                    !string.IsNullOrEmpty(presetsViewModel.SelectedPreset.Data))
                {
                    presetsViewModel.ExportCommand.Execute(null);
                }
            }
        }


    }
}