using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Twm.Chart.Interfaces;
using Twm.Core.Classes;
using Twm.Core.Controllers;
using Twm.Core.DataCalc;
using Twm.Core.Enums;
using Twm.Core.Helpers;
using Twm.Core.Interfaces;
using Twm.Core.Managers;
using Twm.Core.UI.Windows;
using Twm.Core.UI.Windows.Presets;
using Twm.Core.ViewModels;
using Twm.Core.ViewModels.DataSeries;
using Twm.Core.ViewModels.Presets;
using Twm.Core.ViewModels.ScriptObjects;
using Twm.DB.DAL.Repositories.Presets;
using Twm.Model.Model;
using Twm.ViewModels.Presets;
using Twm.ViewModels.ScriptObjects;
using Twm.Windows.Strategies;


namespace Twm.ViewModels.Strategies.Validator
{
    public class ValidatorViewModel : ViewModelBase
    {
        public ObservableCollection<ValidatorItemViewModel> InstrumentLists { get; set; }


        private ValidatorItemViewModel _selectedValidatorItem;

        public ValidatorItemViewModel SelectedValidatorItem
        {
            get { return _selectedValidatorItem; }
            set
            {
                if (value != _selectedValidatorItem)
                {


                    _selectedValidatorItem = value;
                    _selectedValidatorItem.ParentViewModel = this;
                    _selectedValidatorItem.IsSelected = true;

                    if (CurrentSection != null)
                    {
                        if (_selectedValidatorItem.Performance != null)
                        {
                            _selectedValidatorItem.Performance.CurrentSection = CurrentSection;
                        }
                    }

                    OnPropertyChanged();
                    OnPropertyChanged("StrategyName");
                    OnPropertyChanged("Strategy");

                    OnPropertyChanged("PerformanceVisibility");
                    OnPropertyChanged("IsStrategyEnable");
                    OnPropertyChanged("IsRunEnable");
                    OnPropertyChanged("IsStrategySelectEnable");
                    OnPropertyChanged("IsAddInstrumentEnable");
                    OnPropertyChanged("IsRemoveEnable");
                    OnPropertyChanged("IsPresetSaveEnable");
                    OnPropertyChanged("IsEditEnable");
                    if (_selectedValidatorItem.Strategy != null)
                    {


                        _selectedValidatorItem.Strategy.UpdateProperty("Instrument");
                        _selectedValidatorItem.Strategy.UpdateProperty("TickSize");
                        _selectedValidatorItem.Strategy.UpdateProperty("Multiplier");
                    }
                }
            }
        }

        private void Strategy_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is ValidatorItemViewModel validatorItemViewModel)
            {
                validatorItemViewModel.Invalid();
            }
        }

        public StrategyPerformanceSection? CurrentSection { get; set; }


        public string StrategyName
        {
            get
            {
                if (SelectedValidatorItem == null)
                    return string.Empty;


                if (SelectedValidatorItem.IsPortfolio)
                {
                    if (SelectedValidatorItem.Strategy != null)
                    {
                        return SelectedValidatorItem.Strategy.DisplayName;
                    }
                }
                else
                {
                    if (SelectedValidatorItem.InstrumentList.ApplyToAllInstruments)
                    {
                        if (SelectedValidatorItem.InstrumentList.Strategy != null)
                        {
                            return SelectedValidatorItem.InstrumentList.Strategy.DisplayName;
                        }
                    }
                    else
                    {
                        if (SelectedValidatorItem.Strategy != null)
                        {
                            return SelectedValidatorItem.Strategy.DisplayName;
                        }
                    }
                }


                return string.Empty;
            }
        }


        public bool IsStrategyEnable
        {
            get { return SelectedValidatorItem != null; }
        }

        public bool IsRunEnable
        {
            get
            {
                if (SelectedValidatorItem != null)
                {
                    if (SelectedValidatorItem.IsPortfolio)
                    {
                        if (SelectedValidatorItem.ApplyToAllInstruments)
                        {
                            return SelectedValidatorItem.Items.Any() && SelectedValidatorItem.Strategy != null;
                        }
                        else
                        {
                            return SelectedValidatorItem.Items.OfType<ValidatorItemViewModel>()
                                .Any(x => x.Strategy != null);
                        }
                    }
                    else
                    {
                        return SelectedValidatorItem.Strategy != null;
                    }
                }

                return false;
            }
        }


        public Visibility PerformanceVisibility
        {
            get
            {
                if (SelectedValidatorItem?.Performance != null && SelectedValidatorItem.IsValid)
                {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        public bool IsStrategySelectEnable
        {
            get
            {
                if (SelectedValidatorItem == null)
                {
                    return false;
                }

                if (SelectedValidatorItem.IsPortfolio)
                {
                    return true;
                }
                else
                {
                    return !SelectedValidatorItem.InstrumentList.ApplyToAllInstruments;
                }
            }
        }

        public bool IsAddInstrumentEnable
        {
            get { return SelectedValidatorItem != null; }
        }

        public bool IsRemoveEnable
        {
            get { return SelectedValidatorItem != null; }
        }

        public bool IsPresetSaveEnable
        {
            get { return SelectedValidatorItem != null; }
        }

        public bool IsEditEnable
        {
            get { return SelectedValidatorItem != null; }
        }


        private readonly object _busyLock = new object();

        public OperationCommand AddInstrumentCommand { get; set; }

        public OperationCommand AddInstrumentListCommand { get; set; }

        public OperationCommand RemoveCommand { get; set; }

        public OperationCommand EditCommand { get; set; }

        public OperationCommand RunCommand { get; set; }

        public OperationCommand SelectStrategyCommand { get; set; }
        public OperationCommand ClearStrategyCommand { get; set; }

        public OperationCommand ValidatorInstrumentListPresetsLoadCommand { get; set; }
        public OperationCommand ValidatorInstrumentListPresetsSaveCommand { get; set; }
        public OperationCommand ValidatorInstrumentListPresetsExportCommand { get; set; }


        public OperationCommand ValidatorStrategyPresetsLoadCommand { get; set; }
        public OperationCommand ValidatorStrategyPresetsSaveCommand { get; set; }
        public OperationCommand ValidatorStrategyPresetsExportCommand { get; set; }


        public ValidatorViewModel()
        {
            InstrumentLists = new ObservableCollection<ValidatorItemViewModel>();
            AddInstrumentCommand = new OperationCommand(AddInstrument);
            AddInstrumentListCommand = new OperationCommand(AddInstrumentList);
            SelectStrategyCommand = new OperationCommand(SelectStrategy);
            ClearStrategyCommand = new OperationCommand(ClearStrategy);
            RemoveCommand = new OperationCommand(Remove);
            EditCommand = new OperationCommand(Edit);
            RunCommand = new OperationCommand(Run);
            ValidatorInstrumentListPresetsSaveCommand = new OperationCommand(ValidatorInstrumentListPresetsSave);
            ValidatorInstrumentListPresetsLoadCommand = new OperationCommand(ValidatorInstrumentListPresetsLoad);
            ValidatorInstrumentListPresetsExportCommand = new OperationCommand(ValidatorInstrumentListPresetsExport);

            ValidatorStrategyPresetsSaveCommand = new OperationCommand(ValidatorStrategyPresetsSave);
            ValidatorStrategyPresetsLoadCommand = new OperationCommand(ValidatorStrategyPresetsLoad);
            ValidatorStrategyPresetsExportCommand = new OperationCommand(ValidatorStrategyPresetsExport);

            BuildController.Instance.OnCompile += Instance_OnCompile;
        }

        private void Instance_OnCompile(object sender, EventArgs e)
        {

            foreach (var il in InstrumentLists)
            {
                var ilStrategy = il.DataCalcContext.Strategies.FirstOrDefault();
                if (ilStrategy != null)
                    il.ReloadStrategy(ilStrategy);
                foreach (ValidatorItemViewModel item in il.Items)
                {
                    var itemStrategy = item.DataCalcContext.Strategies.FirstOrDefault();
                    if (itemStrategy != null)
                        item.ReloadStrategy(itemStrategy);
                }

            }

            /*if (SelectedValidatorItem != null)
            {
                var strategy = SelectedValidatorItem.DataCalcContext.Strategies.FirstOrDefault();
                SelectedValidatorItem.ReloadStrategy(strategy);
            }*/
        }

        private void AddInstrumentList(object obj)
        {
            var portfolio = new ValidatorItemViewModel { IsPortfolio = true, IsValid = false };

            var portfolioWindow = new PortfolioWindow(portfolio);
            if (portfolioWindow.ShowDialog() == true)
            {
                InstrumentLists.Add(portfolio);
                SelectedValidatorItem = portfolio;
            }
        }

        private void AddInstrument(object obj)
        {
            if (SelectedValidatorItem != null)
            {
                var instrument = new ValidatorItemViewModel { IsValid = false };

                var instrumentParamWindow = new InstrumentParamWindow(instrument);

                if (instrumentParamWindow.ShowDialog() == true)
                {
                    if (SelectedValidatorItem.IsPortfolio)
                    {
                        instrument.InstrumentList = SelectedValidatorItem;
                        SelectedValidatorItem.Items.Add(instrument);
                        SelectedValidatorItem.IsExpanded = true;
                        SelectedValidatorItem.IsValid = false;
                    }
                    else
                    {
                        if (SelectedValidatorItem.InstrumentList is ValidatorItemViewModel instrumentList)
                        {
                            instrumentList.Items.Add(instrument);
                            instrument.InstrumentList = instrumentList;
                            instrument.InstrumentList.IsValid = false;
                        }
                    }

                    instrument.DataCalcContext.SetParams(new List<DataSeriesParams>()
                        {instrument.InstrumentSeriesParams});

                    SelectedValidatorItem = instrument;

                    _ctsFetchData = new CancellationTokenSource();

                    Task.Run(() => LoadData(instrument, _ctsFetchData.Token), _ctsFetchData.Token);
                }
            }
        }


        private CancellationTokenSource _ctsFetchData;

        private async Task LoadData(ValidatorItemViewModel instrument, CancellationToken token)
        {
            lock (_busyLock)
            {
                IsBusy = true;
            }

            var historicalDataManager = new HistoricalDataManager();
            try
            {
                historicalDataManager.RaiseMessageEvent += HistoricalDataManagerRaiseMessageEvent;

                var historicalCandles = await historicalDataManager.GetData(instrument.InstrumentSeriesParams, token);

                if (token.IsCancellationRequested)
                    return;

                Application.Current.Dispatcher.Invoke(() =>
                    {
                        var candleSource = new ObservableCollection<ICandle>();

                        foreach (var historicalCandle in historicalCandles)
                        {
                            if (token.IsCancellationRequested)
                                return;

                            candleSource.Add(Session.Mapper.Map<IHistoricalCandle, ICandle>(historicalCandle));
                        }

                        instrument.DataCalcContext.Candles = candleSource;
                    }
                );
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                historicalDataManager.RaiseMessageEvent -= HistoricalDataManagerRaiseMessageEvent;
                lock (_busyLock)
                {
                    IsBusy = false;
                }
            }
        }

        private void HistoricalDataManagerRaiseMessageEvent(object sender, Core.Messages.MessageEventArgs e)
        {
            var dataSeries = "";
            if (sender is HistoricalDataManager hdm)
            {
                dataSeries = hdm.DataSeriesParams.DataSeriesName;
            }

            Message = dataSeries + " " + e.Message;
            SubMessage = e.SubMessage;
        }


        private void Remove(object obj)
        {
            if (SelectedValidatorItem != null)
            {
                if (SelectedValidatorItem.IsPortfolio)
                    InstrumentLists.Remove(SelectedValidatorItem);
                else
                {
                    if (SelectedValidatorItem.InstrumentList is ValidatorItemViewModel validatorItemViewModel)
                    {
                        validatorItemViewModel.Items.Remove(SelectedValidatorItem);
                        validatorItemViewModel.IsValid = false;
                    }
                }
            }
        }

        private void SelectStrategy(object obj)
        {
            var selectScriptObjectViewModel =
                new SelectScriptObjectViewModel(typeof(StrategyBase), SelectedValidatorItem.DataCalcContext);
            var strategySelectWindow = new StrategySelectWindow(selectScriptObjectViewModel);
            selectScriptObjectViewModel.Init();
            if (strategySelectWindow.ShowDialog() == true)
            {
                if (selectScriptObjectViewModel.SelectedObjectType is ScriptObjectItemViewModel scriptItemViewModel)
                {
                    var scriptObject =
                        SelectedValidatorItem.DataCalcContext.CreateObject(scriptItemViewModel.ObjectType, null, true);

                    SelectedValidatorItem.Strategy = (StrategyBase)scriptObject;
                }
            }
        }


        private void ClearStrategy(object obj)
        {


            SelectedValidatorItem.Strategy = null;

        }

        private void Run(object obj)
        {


            SelectedValidatorItem.Invalid();
            SelectedValidatorItem?.RunCommand.Execute(null);

        }

        private void Edit(object obj)
        {
            if (SelectedValidatorItem != null)
            {
                if (SelectedValidatorItem.IsPortfolio)
                {
                    var editInstrumentList = (ValidatorItemViewModel)SelectedValidatorItem.Clone();
                    var portfolioWindow = new PortfolioWindow(editInstrumentList);
                    if (portfolioWindow.ShowDialog() == true)
                    {
                        SelectedValidatorItem.Name = editInstrumentList.Name;
                    }
                }
                else
                {
                    var editInstrument = (ValidatorItemViewModel)SelectedValidatorItem.Clone();
                    var instrumentParamWindow = new InstrumentParamWindow(editInstrument);
                    if (instrumentParamWindow.ShowDialog() == true)
                    {
                        SelectedValidatorItem.InstrumentSeriesParams = editInstrument.InstrumentSeriesParams;
                        if (SelectedValidatorItem.Strategy != null)
                        {
                            SelectedValidatorItem.Strategy.DataSeriesSeriesParams =
                                SelectedValidatorItem.InstrumentSeriesParams;
                        }

                        Task.Run(() => LoadData(SelectedValidatorItem, _ctsFetchData.Token), _ctsFetchData.Token).ContinueWith(
                            t =>
                            {
                                if (SelectedValidatorItem.IsValid)
                                {
                                    SelectedValidatorItem.RunCommand.Execute(null);
                                }
                            }, TaskContinuationOptions.OnlyOnRanToCompletion
                            );
                    }
                }
            }
        }


        private async void ValidatorInstrumentListPresetsSave(object obj)
        {
            if (SelectedValidatorItem != null)
            {
                var portfolio = InstrumentLists.FirstOrDefault();

                var presetsViewModel = new PresetsViewModel(PresetType.ValidatorInstrumentList);
                presetsViewModel.FetchData();
                presetsViewModel.PresetName = portfolio.Name;

                var presetsWindow = new PresetsWindow(presetsViewModel);
                if (presetsWindow.ShowDialog() == true)
                {
                    Preset preset = null;
                    if (presetsViewModel.SelectedPreset != null)
                        preset = presetsViewModel.SelectedPreset.DataModel;
                    await SaveInstrumentListPreset(InstrumentLists, preset, presetsViewModel.PresetName);
                }
            }
        }


        private async Task SaveInstrumentListPreset(ObservableCollection<ValidatorItemViewModel> instrumentLists, Preset preset, string name)
        {
            using (var context = Session.Instance.DbContextFactory.GetContext())
            {
                var repository = new PresetRepository(context);
                List<InstrumentListPreset> list = new List<InstrumentListPreset>();
                foreach (var instrumentList in instrumentLists)
                {
                    var instrumentListPreset = new InstrumentListPreset(instrumentList);
                    list.Add(instrumentListPreset);
                }

                if (preset == null)
                {
                    preset = new Preset()
                    {
                        Name = name,
                        Type = (int)PresetType.ValidatorInstrumentList,
                        Data = JsonHelper.ToJson(new PresetObject<InstrumentListPreset[]>()
                        { Object = list.ToArray(), PresetType = PresetType.ValidatorInstrumentList })
                    };
                    await repository.Add(preset);
                }
                else
                {
                    preset.Data = JsonHelper.ToJson(new PresetObject<InstrumentListPreset[]>()
                    { Object = list.ToArray(), PresetType = PresetType.ValidatorInstrumentList });
                    await repository.Update(preset);
                }

                await repository.CompleteAsync();
            }
        }


        private async void ValidatorInstrumentListPresetsLoad(object obj)
        {
            var presetsViewModel = new PresetsViewModel(PresetType.ValidatorInstrumentList, PresetFormMode.Load);
            presetsViewModel.FetchData();
            presetsViewModel.SelectedPreset = presetsViewModel.Presets.FirstOrDefault();


            var presetsWindow = new PresetsWindow(presetsViewModel);
            if (presetsWindow.ShowDialog() == true)
            {
                if (presetsViewModel.SelectedPreset != null &&
                    !string.IsNullOrEmpty(presetsViewModel.SelectedPreset.Data))
                {
                    InstrumentLists.Clear();
                    var instrumentLists = await CreateInstrumentListByPreset(presetsViewModel.SelectedPreset.Data, presetsViewModel.IsLivePreset);
                    SelectedValidatorItem = instrumentLists.FirstOrDefault();
                    foreach (var instrumentList in instrumentLists)
                    {
                        InstrumentLists.Add(instrumentList);
                    }


                    OnPropertyChanged(nameof(IsRunEnable));
                }
            }
        }


        private async Task<List<ValidatorItemViewModel>> CreateInstrumentListByPreset(string json, bool isSync)
        {
            List<ValidatorItemViewModel> instrumentLists = new List<ValidatorItemViewModel>();
            try
            {
                var presetObject = JsonHelper.ToObject<PresetObject<InstrumentListPreset[]>>(json);

                foreach (var instrumentListPreset in presetObject.Object)
                {
                    var instrumentList = new ValidatorItemViewModel
                    {
                        IsPortfolio = true,
                        Name = instrumentListPreset.Name,
                        ApplyToAllInstruments = instrumentListPreset.ApplyToAll,
                        IsExpanded = true
                    };

                    if (!string.IsNullOrEmpty((instrumentListPreset.Strategy.Guid)))
                    {
                        var strategyType =
                            BuildController.Instance.GetStrategyTypeByGuid(instrumentListPreset.Strategy.Guid);
                        var scriptObject =
                            instrumentList.DataCalcContext.CreateObject(strategyType, null, true);

                        var strategy = (StrategyBase)scriptObject;


                        if (instrumentListPreset.Strategy.Parameters != null &&
                            instrumentListPreset.Strategy.Parameters.Any())
                        {
                            foreach (var property in strategy.TwmPropertyNames)
                            {
                                if (instrumentListPreset.Strategy.Parameters.TryGetValue(property, out var value))
                                {
                                    strategy.SetTwmPropertyValue(property, value);
                                }
                            }
                        }

                        instrumentList.Strategy = strategy;
                    }

                    if (instrumentListPreset.Instruments != null && instrumentListPreset.Instruments.Any())
                    {
                        _ctsFetchData = new CancellationTokenSource();
                        foreach (var instrumentPreset in instrumentListPreset.Instruments)
                        {
                            var endDate = instrumentPreset.PeriodEnd.Date;
                            if (isSync)
                                endDate = DateTime.Now;

                            var instrument = new ValidatorItemViewModel
                            {
                                InstrumentList = instrumentList,
                                InstrumentSeriesParams = new DataSeriesParamsViewModel()
                                {
                                    SelectedConnection = Session.ConfiguredConnections.FirstOrDefault(x => x.Code == instrumentPreset.ConnectionCode),
                                    SelectedType = instrumentPreset.Type,
                                    Instrument = Session.Instance.GetInstrument(instrumentPreset.Symbol, instrumentPreset.Type, instrumentPreset.ConnectionCode).Result,
                                    DataSeriesType = instrumentPreset.DataSeriesType,
                                    DataSeriesValue = instrumentPreset.DataSeriesValue,
                                    DataSeriesFormat = instrumentPreset.DataSeriesFormat,
                                    DaysToLoad = instrumentPreset.DaysToLoad,
                                    SelectedTimeFrameBase = instrumentPreset.TimeFrameBase,
                                    PeriodEnd = endDate,
                                }
                            };
                            instrument.InstrumentSeriesParams.DataSeriesFormat = instrument.InstrumentSeriesParams.DataSeriesFormats.FirstOrDefault(x => x.Type == instrumentPreset.DataSeriesFormat.Type && x.Value == instrumentPreset.DataSeriesFormat.Value);


                            if (!string.IsNullOrEmpty(instrumentPreset.Strategy.Guid))
                            {
                                var strategyType =
                                    BuildController.Instance.GetStrategyTypeByGuid(instrumentPreset.Strategy.Guid);
                                var scriptObject = instrument.DataCalcContext.CreateObject(strategyType, null, true);

                                var strategy = (StrategyBase)scriptObject;

                                strategy.DataSeriesSeriesParams = instrument.InstrumentSeriesParams;

                                if (instrumentPreset.Strategy.Parameters != null &&
                                    instrumentPreset.Strategy.Parameters.Any())
                                {
                                    foreach (var property in strategy.TwmPropertyNames)
                                    {
                                        if (instrumentPreset.Strategy.Parameters.TryGetValue(property, out var value))
                                        {
                                            strategy.SetTwmPropertyValue(property, value);
                                        }
                                    }
                                }

                                instrument.Strategy = strategy;
                            }

                            instrument.DataCalcContext.SetParams(new List<DataSeriesParams>()
                                {instrument.InstrumentSeriesParams});
                            var task = Task.Run(() => LoadData(instrument, _ctsFetchData.Token), _ctsFetchData.Token);
                            await Task.WhenAll(task);


                            instrumentList.Items.Add(instrument);
                        }
                    }
                    instrumentLists.Add(instrumentList);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can`t load preset: " + ex.Message);
                LogController.Print("Can`t load preset: " + ex.Message + "\r\n" + ex.StackTrace);
            }

            return instrumentLists;
        }


        private void ValidatorInstrumentListPresetsExport(object obj)
        {
            var presetsViewModel = new PresetsViewModel(PresetType.ValidatorInstrumentList, PresetFormMode.Export);
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


        private async void ValidatorStrategyPresetsSave(object obj)
        {
            if (SelectedValidatorItem?.Strategy != null)
            {
                var strategy = SelectedValidatorItem.Strategy;

                var presetsViewModel = new PresetsViewModel(PresetType.Strategy);
                presetsViewModel.FetchData(strategy.Guid.ToString());
                presetsViewModel.PresetName = strategy.Name;

                var presetsWindow = new PresetsWindow(presetsViewModel);
                if (presetsWindow.ShowDialog() == true)
                {
                    Preset preset = null;
                    if (presetsViewModel.SelectedPreset != null)
                        preset = presetsViewModel.SelectedPreset.DataModel;
                    await SaveStrategyPreset(strategy, preset, presetsViewModel.PresetName);
                }
            }
        }

        private async Task SaveStrategyPreset(StrategyBase strategy, Preset preset, string name)
        {
            using (var context = Session.Instance.DbContextFactory.GetContext())
            {
                var repository = new PresetRepository(context);
                var validatorStrategyPreset = new ValidatorStrategyPreset(strategy);

                if (preset == null)
                {
                    preset = new Preset()
                    {
                        Name = name,
                        Guid = strategy.Guid.ToString(),
                        Type = (int)PresetType.Strategy,
                        Data = JsonHelper.ToJson(new PresetObject<ValidatorStrategyPreset>()
                        { Object = validatorStrategyPreset, PresetType = PresetType.Strategy })
                    };
                    await repository.Add(preset);
                }
                else
                {
                    preset.Data = JsonHelper.ToJson(new PresetObject<ValidatorStrategyPreset>()
                    { Object = validatorStrategyPreset, PresetType = PresetType.Strategy });
                    await repository.Update(preset);
                }

                await repository.CompleteAsync();
            }
        }


        private void ValidatorStrategyPresetsLoad(object obj)
        {
            var presetsViewModel = new PresetsViewModel(PresetType.Strategy, PresetFormMode.Load);
            if (SelectedValidatorItem.Strategy == null)
                presetsViewModel.FetchData();
            else
                presetsViewModel.FetchData(SelectedValidatorItem.Strategy.Guid.ToString());

            presetsViewModel.SelectedPreset = presetsViewModel.Presets.FirstOrDefault();

            var presetsWindow = new PresetsWindow(presetsViewModel);
            if (presetsWindow.ShowDialog() == true)
            {
                if (presetsViewModel.SelectedPreset != null &&
                    !string.IsNullOrEmpty(presetsViewModel.SelectedPreset.Data))
                {
                    SelectedValidatorItem.Strategy = CreateStrategyByPreset(presetsViewModel.SelectedPreset.Data);
                    SelectedValidatorItem.Strategy.DataSeriesSeriesParams =
                        SelectedValidatorItem.InstrumentSeriesParams;
                    OnPropertyChanged(nameof(IsRunEnable));
                }
            }
        }


        private StrategyBase CreateStrategyByPreset(string json)
        {
            StrategyBase strategy = null;
            try
            {
                var presetObject =
                    JsonHelper.ToObject<PresetObject<ValidatorStrategyPreset>>(json);

                var strategyType = BuildController.Instance.GetStrategyTypeByGuid(presetObject.Object.Guid);
                var scriptObject =
                    SelectedValidatorItem.DataCalcContext.CreateObject(strategyType, null, true);


                strategy = (StrategyBase)scriptObject;


                if (presetObject.Object.Parameters != null &&
                    presetObject.Object.Parameters.Any())
                {
                    foreach (var property in strategy.TwmPropertyNames)
                    {
                        if (presetObject.Object.Parameters.TryGetValue(property, out var value))
                        {
                            strategy.SetTwmPropertyValue(property, value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can`t load preset: " + ex.Message);
                LogController.Print("Can`t load preset: " + ex.Message + "\r\n" + ex.StackTrace);
            }

            return strategy;
        }


        private void ValidatorStrategyPresetsExport(object obj)
        {
            var presetsViewModel = new PresetsViewModel(PresetType.Strategy, PresetFormMode.Export);
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

        public void Clear()
        {
            foreach (var il in InstrumentLists)
            {
                foreach (var period in il.Items.OfType<ValidatorItemViewModel>())
                {
                    Session.Instance.DataCalcContexts.Remove(period.DataCalcContext);
                }

                Session.Instance.DataCalcContexts.Remove(il.DataCalcContext);
            }

            BuildController.Instance.OnCompile -= Instance_OnCompile;
        }

        public async Task LoadByStrategy(string name, StrategyViewModel strategyViewMode, DataSeriesParams dataSeriesParams, DateTime startDate, DateTime endDate)
        {
            ValidatorItemViewModel instrumentList = null;
            try
            {
                instrumentList = new ValidatorItemViewModel
                {
                    IsPortfolio = true,
                    Name = name,
                    ApplyToAllInstruments = false,
                    IsExpanded = true
                };

                var strategyGuid = strategyViewMode.Strategy.Guid.ToString();

                _ctsFetchData = new CancellationTokenSource();

                var instrument = new ValidatorItemViewModel
                {
                    InstrumentList = instrumentList,
                    InstrumentSeriesParams = new DataSeriesParamsViewModel()
                    {
                        SelectedType = dataSeriesParams.Instrument.Type,
                        SelectedConnection =  Session.Instance.ConfiguredConnections.FirstOrDefault(x=>x.Id == dataSeriesParams.Instrument.ConnectionId),
                        Instrument = dataSeriesParams.Instrument,
                        DataSeriesFormat = dataSeriesParams.DataSeriesFormat,
                        SelectedTimeFrameBase = TimeFrameBase.CustomRange,
                        PeriodStart = startDate,
                        PeriodEnd = endDate,
                    }
                };
                instrument.InstrumentSeriesParams.DataSeriesFormat = instrument.InstrumentSeriesParams.DataSeriesFormats.FirstOrDefault(x => x.Type == dataSeriesParams.DataSeriesFormat.Type && x.Value == dataSeriesParams.DataSeriesFormat.Value);

                if (!string.IsNullOrEmpty(strategyGuid))
                {
                    var strategyType =
                        BuildController.Instance.GetStrategyTypeByGuid(strategyGuid);

                    if (strategyType == null)
                    {
                        MessageBox.Show("Can`t open strategy in validator.  " + Environment.NewLine + "Can`t find strategy with GUID: " + strategyGuid);
                        LogController.Print("Can`t open strategy in validator.  " + Environment.NewLine + "Can`t find strategy with GUID: " + strategyGuid);
                        return;
                    }

                    var scriptObject = instrument.DataCalcContext.CreateObject(strategyType, null, true);

                    var strategy = (StrategyBase)scriptObject;

                    strategy.DataSeriesSeriesParams = instrument.InstrumentSeriesParams;

                    var properties = strategyViewMode.Strategy.GetTwmPropertyValues();

                    if (properties.Any())
                    {
                        foreach (var property in strategy.TwmPropertyNames)
                        {
                            if (properties.TryGetValue(property, out var value))
                            {
                                strategy.SetTwmPropertyValue(property, value);
                            }
                        }
                    }

                    instrument.Strategy = strategy;
                }

                instrument.DataCalcContext.SetParams(new List<DataSeriesParams>() { instrument.InstrumentSeriesParams });
                var task = Task.Run(() => LoadData(instrument, _ctsFetchData.Token), _ctsFetchData.Token);
                await Task.WhenAll(task);


                instrumentList.Items.Add(instrument);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can`t open strategy in validator: " + ex.Message);
                LogController.Print("Can`t open strategy in validator: " + ex.Message + "\r\n" + ex.StackTrace);
            }

            SelectedValidatorItem = instrumentList;
            InstrumentLists.Add(SelectedValidatorItem);

            OnPropertyChanged(nameof(IsRunEnable));
        }
    }
}