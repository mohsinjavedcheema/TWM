using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Twm.Core.Classes;
using Twm.Core.Classes.Exporters;
using Twm.Core.Controllers;
using Twm.Core.DataCalc;
using Twm.Core.DataCalc.Performance;
using Twm.Core.Enums;
using Twm.Core.Helpers;
using Twm.Core.Interfaces;
using Twm.Core.UI.Windows.Presets;
using Twm.Core.ViewModels;
using Twm.Core.ViewModels.DataSeries;
using Twm.Core.ViewModels.Presets;
using Twm.Core.ViewModels.ScriptObjects;
using Twm.DB.DAL.Repositories.OptimizerResults;
using Twm.DB.DAL.Repositories.Presets;
using Twm.Model.Model;
using Twm.ViewModels.DataBase;
using Twm.ViewModels.Presets;
using Twm.ViewModels.ScriptObjects;
using Twm.Views.Strategies.Performance;
using Twm.Windows.Strategies;
using Microsoft.Win32;
using OxyPlot.Wpf;
using DataObject = Twm.Core.Classes.Exporters.DataObject;
using OptimizerTestWindow = Twm.Windows.Strategies.OptimizerTestWindow;
using PresetNameWindow = Twm.Core.UI.Windows.Presets.PresetNameWindow;
using Twm.Model.Model.Interfaces;


namespace Twm.ViewModels.Strategies.Optimizer
{
    public class OptimizerViewModel : ViewModelBase
    {
        public ObservableCollection<OptimizerTestViewModel> Tests { get; set; }


        private ViewModelBase _selectedOptimizerItem;

        public ViewModelBase SelectedOptimizerItem
        {
            get { return _selectedOptimizerItem; }
            set
            {
                if (value != _selectedOptimizerItem)
                {
                    if (_selectedOptimizerItem is OptimizerPeriodViewModel prevPeriod)
                    {
                        SelectedTabIndex = prevPeriod.SelectedTabIndex;
                    }

                    _selectedOptimizerItem = value;
                    if (_selectedOptimizerItem != null)
                    {
                        _selectedOptimizerItem.ParentViewModel = this;
                        if (_selectedOptimizerItem is IOptimizerItem optimizerItem)
                        {
                            optimizerItem.IsSelected = true;

                            if (CurrentSection != null)
                            {
                                if (optimizerItem is OptimizerPeriodViewModel period)
                                {
                                    period.CurrentSection = CurrentSection;
                                }

                                if (optimizerItem is OptimizerTestViewModel test)
                                {
                                    test.SelectedStrategy?.TotalPerformance?.SelectSection(CurrentSection);
                                }
                            }
                        }

                        if (_selectedOptimizerItem is OptimizerPeriodViewModel currentPeriod)
                        {
                            currentPeriod.SelectedTabIndex = SelectedTabIndex;
                        }
                    }

                    OnPropertyChanged();
                    OnPropertyChanged("StrategyName");
                    OnPropertyChanged("PerformanceVisibility");
                    OnPropertyChanged("IsStrategyEnable");
                    OnPropertyChanged("IsRunEnable");
                    OnPropertyChanged("IsStrategySelectEnable");
                    OnPropertyChanged("IsAddPeriodEnable");
                    OnPropertyChanged("IsRemoveEnable");
                    OnPropertyChanged("IsEditEnable");
                    OnPropertyChanged("IsPresetSaveEnable");
                }
            }
        }

        public StrategyPerformanceSection? CurrentSection { get; set; }

        public int SelectedTabIndex { get; set; }


        public string StrategyName
        {
            get
            {
                if (SelectedOptimizerItem == null)
                    return string.Empty;

                if (SelectedOptimizerItem is OptimizerTestViewModel test)
                {
                    if (test.Strategy != null)
                    {
                        return test.Strategy.DisplayName;
                    }
                }
                else if (SelectedOptimizerItem is OptimizerPeriodViewModel period)
                {
                    if (period.Test.Strategy != null)
                    {
                        return period.Test.Strategy.DisplayName;
                    }
                }

                return string.Empty;
            }
        }

        private bool _applyRiskLevels;

        public bool ApplyRiskLevels
        {
            get { return _applyRiskLevels; }
            set
            {
                if (_applyRiskLevels != value)
                {
                    _applyRiskLevels = value;
                    OnPropertyChanged();
                    OnPropertyChanged("IsStrategyEnable");
                    OnPropertyChanged("CanRun");
                }
            }
        }

        public bool IsOptimizerViewer { get; set; }


        public bool IsStrategyEnable
        {
            get
            {
                /*if (IsOptimizerViewer)
                    return false;*/

                if (ApplyRiskLevels)
                    return false;

                return SelectedOptimizerItem != null;
            }
        }

        public bool IsRunEnable
        {
            get { return true; }
        }

        public bool IsStopEnable
        {
            get { return true; }
        }


        public bool IsAddPeriodEnable
        {
            get { return SelectedOptimizerItem != null; }
        }


        public Visibility RunVisibility
        {
            get
            {
                if (!IsOptimizeRunning)
                    return Visibility.Visible;

                return Visibility.Collapsed;
            }
        }

        public Visibility OptimizerTestVisibility { get; set; }

        public Visibility StopVisibility
        {
            get
            {
                if (IsOptimizeRunning)
                    return Visibility.Visible;

                return Visibility.Collapsed;
            }
        }


        public bool IsStrategySelectEnable
        {
            get
            {
                if (SelectedOptimizerItem == null)
                {
                    return false;
                }

                return true;
            }
        }


        public bool IsRemoveEnable
        {
            get { return SelectedOptimizerItem != null; }
        }


        public bool IsEditEnable
        {
            get { return SelectedOptimizerItem != null; }
        }


        public StrategyBase Strategy
        {
            get
            {
                if (SelectedOptimizerItem is IOptimizerItem optimizerItem)
                    return optimizerItem.Strategy;

                return null;
            }
        }


        private bool _isOptimizeRunning;

        public bool IsOptimizeRunning
        {
            get { return _isOptimizeRunning; }
            set
            {
                if (_isOptimizeRunning != value)
                {
                    _isOptimizeRunning = value;
                    OnPropertyChanged();
                    OnPropertyChanged("RunVisibility");
                    OnPropertyChanged("CanRun");
                    OnPropertyChanged("StopVisibility");
                }
            }
        }

        public string TotalTimeStr
        {
            get
            {
                if (TotalTime == null)
                    return "";


                return "Total time:" + "\r\n" + TotalTime;
            }
        }


        private TimeSpan? _totalTime;

        public TimeSpan? TotalTime
        {
            get { return _totalTime; }
            set
            {
                if (_totalTime != value)
                {
                    _totalTime = value;
                    OnPropertyChanged();
                    OnPropertyChanged("TotalTimeStr");
                }
            }
        }

        private bool _canRun;

        public bool CanRun
        {
            get
            {
                return Tests.Count > 0
                       && _canRun
                       && !ApplyRiskLevels
                       && Tests.FirstOrDefault()?.Periods?.Count > 0
                       && Strategy?.Instrument != null
                       && Strategy?.KeepBestNumber != 0
                       && (Strategy?.Optimizer?.OptimizerParameters.Where(op => op.IsChecked)).Any();
            }
        }

        public bool CanAddTest
        {
            get => _canAddTest;
            set
            {
                _canAddTest = value;
                OnPropertyChanged(nameof(CanAddTest));
            }
        }


        public bool IsPresetSaveEnable
        {
            get { return SelectedOptimizerItem != null; }
        }


        public OperationCommand ExportPDFCommand { get; set; }

        public OperationCommand AddOptimizerPeriodCommand { get; set; }

        public OperationCommand AddOptimizerTestCommand { get; set; }

        public OperationCommand EditCommand { get; set; }

        public OperationCommand EditPeriodsCommand { get; set; }

        public OperationCommand RemoveCommand { get; set; }

        public OperationCommand RunCommand { get; set; }

        public OperationCommand StopCommand { get; set; }

        public OperationCommand SelectStrategyCommand { get; set; }

        public OperationCommand OptimizerTestPresetsLoadCommand { get; set; }
        public OperationCommand OptimizerTestPresetsSaveCommand { get; set; }

        public OperationCommand OptimizerTestPresetsExportCommand { get; set; }

        public OperationCommand OptimizerStrategyPresetLoadCommand { get; set; }
        public OperationCommand OptimizerStrategyPresetSaveCommand { get; set; }
        public OperationCommand OptimizerStrategyPresetExportCommand { get; set; }

        public OperationCommand SaveOptimizerResultsCommand { get; set; }

        public OperationCommand SendOptimizerResultsCommand { get; set; }

        public OperationCommand SendPeriodToServerCommand { get; set; }

        private readonly List<Task> _optimizationTasks;

        public OptimizerViewModel()
        {
            _canRun = true;

            Tests = new ObservableCollection<OptimizerTestViewModel>();
            CanAddTest = true;
            Tests.CollectionChanged += (sender, args) => { CanAddTest = Tests == null || Tests.Count == 0; };
            AddOptimizerPeriodCommand = new OperationCommand(AddOptimizerPeriod);
            AddOptimizerTestCommand =
                new OperationCommand(AddOptimizerTest, o => { return Tests == null || Tests.Count == 0; });

            ExportPDFCommand = new OperationCommand(ExportPDF);
            SelectStrategyCommand = new OperationCommand(SelectStrategy);
            EditCommand = new OperationCommand(Edit);
            RemoveCommand = new OperationCommand(Remove);
            RunCommand = new OperationCommand(Run);
            StopCommand = new OperationCommand(Stop);
            EditPeriodsCommand = new OperationCommand(EditPeriods);

            OptimizerTestPresetsLoadCommand = new OperationCommand(OptimizerTestPresetLoad);
            OptimizerTestPresetsSaveCommand = new OperationCommand(OptimizerTestPresetSave);
            OptimizerTestPresetsExportCommand = new OperationCommand(OptimizerTestPresetExport);

            OptimizerStrategyPresetLoadCommand = new OperationCommand(OptimizerStrategyPresetLoad);
            OptimizerStrategyPresetSaveCommand = new OperationCommand(OptimizerStrategyPresetSave);
            OptimizerStrategyPresetExportCommand = new OperationCommand(OptimizerStrategyPresetExport);

            SaveOptimizerResultsCommand = new OperationCommand(SaveOptimizerResults);            
            _optimizationTasks = new List<Task>();
            IsOptimizerViewer = false;
            OptimizerTestVisibility = Visibility.Visible;

            BuildController.Instance.OnCompile += Instance_OnCompile;
        }

        private void Instance_OnCompile(object sender, EventArgs e)
        {
            foreach (var test in Tests)
            {
                var strategy = test.DataCalcContext.Strategies.FirstOrDefault();
                if (strategy != null && test.Strategy != strategy)
                {
                    //If recompile
                    test.SetStrategy(strategy, false);
                }
            }



            OnPropertyChanged("Strategy");
        }

        private void ExportPDF(object obj)
        {
            var sfd = new SaveFileDialog
            {
                DefaultExt = "pdf",
                Filter = "PDF Files (*.pdf)|*.pdf"
            };
            if (sfd.ShowDialog() != true)
            {
                return;
            }

            var file = sfd.FileName;

            OptimizerTestViewModel test = null;

            if (SelectedOptimizerItem is OptimizerPeriodViewModel optimizerPeriodViewModel)
            {
                test = optimizerPeriodViewModel.Test;
            }
            else if (SelectedOptimizerItem is OptimizerTestViewModel optimizerTestViewModel)
            {
                test = optimizerTestViewModel;
            }

            if (test == null)
                return;

            var sampleTableData = new List<List<string>>();
            for (int i = 0; i < test.Periods.Count; i++)
            {
                var list = new List<string>();
                list.Add((i + 1).ToString());
                list.Add(
                    $"IS{test.Periods[i].Number}:   {test.Periods[i].ISStartDate:dd.MM.yyyy} - {test.Periods[i].ISEndDate:dd.MM.yyyy}");
                if (!test.Periods[i].IsLive)
                {
                    list.Add(
                        $"OS{test.Periods[i].Number}: {test.Periods[i].OSStartDate:dd.MM.yyyy} - {test.Periods[i].OSEndDate:dd.MM.yyyy}");
                }
                sampleTableData.Add(list);
            }


            var tables = new Dictionary<string, Table>()
            {
                {
                    "main",
                    new Table(
                        new List<string>() {"Period #", "Date Start/End In Sample", "Date Start/End Out of Sample"},
                        sampleTableData)
                }
            };
            tables.Add($"Сombined Out of Sample Results", GenerateTable(test.Performance));
            var images = new List<KeyValuePair<string, Func<byte[]>>>();

            if (test.Performance != null)
            {
                foreach (var boxItem in test.Performance.Analysis.GraphTypes)
                {
                    images.Add(new KeyValuePair<string, Func<byte[]>>($"Сombined Out of Sample Results: {boxItem.Tag}",
                        () =>
                        {
                            AnalysisView anal = null;
                            PlotView plot = null;
                            var filePath = "";

                            filePath = Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid()}.png");
                            var pngExporter = new PngExporter { Width = 600, Height = 400 };
                            test.Performance.Analysis.SelectedGraphType = boxItem;
                            if (test.Performance.Analysis.SelectedGraphViewModel.Model == null)
                            {
                                test.Performance.Analysis.SelectedGraphViewModel.CreatePlotModel();
                            }

                            pngExporter.ExportToFile(test.Performance.Analysis.SelectedGraphViewModel.Model, filePath);
                            return string.IsNullOrEmpty(filePath) ? new byte[] { 0 } : File.ReadAllBytes(filePath);
                        }));
                }
            }

            foreach (var period in test.Periods)
            {
                if (period.IsPerformance != null)
                {
                    tables.Add($"{period.DisplayName}_IS", GenerateTable(period.IsPerformance));
                }


                if (period.OsPerformance != null)
                {
                    tables.Add($"{period.DisplayName}_OS", GenerateTable(period.OsPerformance));
                }
                else
                {
                    foreach (var boxItem in period.IsPerformance.Analysis.GraphTypes)
                    {
                        images.Add(new KeyValuePair<string, Func<byte[]>>($"{period.DisplayName}", () =>
                        {
                            var filePath = "";

                            filePath = Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid()}.png");
                            var ar = 1.5;
                            var heigiht = 800;
                            var pngExporter = new PngExporter { Width = (int)(heigiht * ar), Height = heigiht };
                            period.IsPerformance.Analysis.SelectedGraphType = boxItem;
                            if (period.IsPerformance.Analysis.SelectedGraphViewModel.Model == null)
                            {
                                period.IsPerformance.Analysis.SelectedGraphViewModel.CreatePlotModel();
                            }

                            pngExporter.ExportToFile(period.IsPerformance.Analysis.SelectedGraphViewModel.Model,
                                filePath);
                            return string.IsNullOrEmpty(filePath) ? new byte[] { 0 } : File.ReadAllBytes(filePath);
                        }));
                    }
                }

                if (period.TotalPerformance == null)
                {
                    continue;
                }

                foreach (var boxItem in period.TotalPerformance.Analysis.GraphTypes)
                {
                    images.Add(new KeyValuePair<string, Func<byte[]>>($"{period.DisplayName}", () =>
                    {
                        var filePath = "";

                        filePath = Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid()}.png");
                        var ar = 1.5;
                        var heigiht = 800;
                        var pngExporter = new PngExporter { Width = (int)(heigiht * ar), Height = heigiht };
                        period.TotalPerformance.Analysis.SelectedGraphType = boxItem;
                        if (period.TotalPerformance.Analysis.SelectedGraphViewModel.Model == null)
                        {
                            period.TotalPerformance.Analysis.SelectedGraphViewModel.CreatePlotModel();
                        }

                        pngExporter.ExportToFile(period.TotalPerformance.Analysis.SelectedGraphViewModel.Model,
                            filePath);
                        return string.IsNullOrEmpty(filePath) ? new byte[] { 0 } : File.ReadAllBytes(filePath);
                    }));
                }
            }

            var data = new DataObject(new Dictionary<string, Func<string>>()
            {
                {OptimizationPDFExporter.TestName, () => "Сombined Out of Sample Results"},
                {OptimizationPDFExporter.Symbol, () => Strategy.Instrument.Symbol},
                {OptimizationPDFExporter.Strategy, () => $"{test.Strategy.Name} {test.Strategy.Version}"},
                {
                    OptimizationPDFExporter.Optimizer, () =>
                    {
                        var objectType = test.Strategy.Optimizer.GetType();
                        var fields = objectType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance
                                                                                 | BindingFlags.Public |
                                                                                 BindingFlags.Static);
                        var optimizerName = fields.FirstOrDefault(info =>
                            info.Name.Equals("OptimizerName", StringComparison.OrdinalIgnoreCase));
                        return optimizerName.GetValue(null).ToString();
                    }
                },
                {
                    OptimizationPDFExporter.IndividualIterations,
                    () => test.TaskViewModels.FirstOrDefault().AllTasks.ToString()
                },
                {
                    OptimizationPDFExporter.OptimizationFitness, () =>
                    {
                        if (test?.Strategy?.OptimizationFitness == null)
                        {
                            return "";
                        }

                        var name = test.Strategy?.OptimizationFitness?.GetType().Name;
                        return name;
                    }
                },
                {OptimizationPDFExporter.QuantityPeriods, () => test.Periods.Count.ToString()},
                {
                    OptimizationPDFExporter.Timeframe,
                    () => test.DataCalcContext.DataSeries.Replace("(", "").Replace(")", "")
                },
                {
                    OptimizationPDFExporter.TotalIterations,
                    () => (test.TaskViewModels.FirstOrDefault().AllTasks * test.Periods.Count).ToString()
                }
            }, tables, images);

            var export = Exporter<OptimizationPDFExporter>.BeginExport(data, file, d => { Debug.WriteLine(d); });
        }


        private Table GenerateTable(StrategyPerformanceViewModel performance)
        {
            var data = new List<List<string>>();
            foreach (var summarySummaryItem in performance.Summary.SummaryItems)
            {
                var row = new List<string>();
                row.Add(summarySummaryItem.AnalyticItem.DisplayName.ToString());
                row.Add(string.IsNullOrEmpty(summarySummaryItem.AllTradesStringFormat)
                    ? summarySummaryItem.AllTrades.ToString()
                    : summarySummaryItem.AllTrades is DateTime time
                        ? time.ToString(summarySummaryItem.AllTradesStringFormat)
                        : string.Format(summarySummaryItem.AllTradesStringFormat, summarySummaryItem.AllTrades));

                row.Add(string.IsNullOrEmpty(summarySummaryItem.LongTradesStringFormat)
                    ? summarySummaryItem.LongTrades.ToString()
                    : summarySummaryItem.LongTrades is DateTime time2
                        ? time2.ToString(summarySummaryItem.LongTradesStringFormat)
                        : string.Format(summarySummaryItem.LongTradesStringFormat, summarySummaryItem.LongTrades));

                row.Add(string.IsNullOrEmpty(summarySummaryItem.ShortTradesStringFormat)
                    ? summarySummaryItem.ShortTrades.ToString()
                    : summarySummaryItem.ShortTrades is DateTime time3
                        ? time3.ToString(summarySummaryItem.ShortTradesStringFormat)
                        : string.Format(summarySummaryItem.ShortTradesStringFormat, summarySummaryItem.ShortTrades));
                data.Add(row);
            }

            var table = new Table(new List<string>() { "Perfomance", "All Trades", "Long Trades", "Short Trades" }, data);

            return table;
        }

        public OptimizerViewModel(OptimizerResultViewModel optimizerResult) : this()
        {
            OptimizerTestVisibility = Visibility.Collapsed;
            IsOptimizerViewer = true;
            LoadOptimizerResult(optimizerResult);
        }


        private async void LoadOptimizerResult(OptimizerResultViewModel optimizerResult)
        {
            var test = optimizerResult.CreateTest();
            test.ParentViewModel = this;
            Tests.Clear();
            Tests.Add(test);
            SelectedOptimizerItem = test;

            var strategy = optimizerResult.CreateStrategy(test);


            if (SelectedOptimizerItem is IOptimizerItem optimizerItem)
            {
                optimizerItem.Strategy = strategy;
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            var trades = new List<ITrade>();
            var tasks = new List<Task>();
            foreach (var period in test.Periods)
            {
                var task = optimizerResult.CreatePeriod(period, strategy, cts);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            foreach (var period in test.Periods)
            {
                if (period.OsPerformance != null)
                    trades.AddRange(period.OsPerformance.Trades);
            }


            var po = new PerformanceOptions
            {
                IsPortfolio = false,
                ParentViewModel = test,
                ExcludeSections = new List<object>
                    {StrategyPerformanceSection.Chart, StrategyPerformanceSection.Orders}
            };

            var performance = new StrategyPerformanceViewModel(null, trades.ToArray(), po);

            await performance.Calculate(cts.Token);
            test.SelectedStrategy.TotalPerformance = performance;
            test.Performance = performance;
        }


        private async void SaveOptimizerResults(object obj)
        {
            OptimizerTestViewModel test = null;

            if (SelectedOptimizerItem is OptimizerPeriodViewModel optimizerPeriodViewModel)
            {
                test = optimizerPeriodViewModel.Test;
            }
            else if (SelectedOptimizerItem is OptimizerTestViewModel optimizerTestViewModel)
            {
                test = optimizerTestViewModel;
            }

            if (test == null)
                return;


            var optimizerResult = new OptimizerResult
            {
                Name = test.Name,
                Guid = Guid.NewGuid().ToString(),
                DateCreated = DateTime.Now,
                StrategyGuid = test.Strategy.Guid.ToString(),
                Symbol = test.Strategy.Instrument.Symbol,
                DataSeriesType = test.Strategy.DataSeriesSeriesParams.DataSeriesType.ToString(),
                DataSeriesValue = test.Strategy.DataSeriesSeriesParams.DataSeriesValue,
                StrategyVersion = test.Strategy.Version
            };
            var optimizerResultViewModel = new OptimizerResultViewModel(optimizerResult);

            var optimizerResultNameWindow = new PresetNameWindow(optimizerResultViewModel)
            { Title = "Optimizer result name" };

            if (optimizerResultNameWindow.ShowDialog() == false)
            {
                return;
            }


            var pathToDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Twm" +
                            "\\OptimizerResults";
            if (!Directory.Exists(pathToDir))
            {
                Directory.CreateDirectory(pathToDir);
            }

            var pathToFiles = Path.Combine(pathToDir, optimizerResult.Guid);
            if (!Directory.Exists(pathToFiles))
            {
                Directory.CreateDirectory(pathToFiles);
            }


            var data = JsonHelper.ToJson(test);
            var pathToTest = Path.Combine(pathToFiles, "test.json");
            File.WriteAllText(pathToTest, data);

            data = JsonHelper.ToJson(new OptimizerStrategyPreset(test.Strategy));
            var pathToStrategy = Path.Combine(pathToFiles, "strategy.json");
            File.WriteAllText(pathToStrategy, data);

            foreach (var period in test.Periods)
            {
                var pathToPeriod = Path.Combine(pathToFiles, period.Number.ToString());
                if (!Directory.Exists(pathToPeriod))
                {
                    Directory.CreateDirectory(pathToPeriod);
                }

                var i = 1;
                var strategy = period.SelectedStrategy;
                //foreach (var strategy in period.StrategyViewModels)
                {
                    var pathToPeriodStrategy = Path.Combine(pathToPeriod, i.ToString());
                    if (!Directory.Exists(pathToPeriodStrategy))
                    {
                        Directory.CreateDirectory(pathToPeriodStrategy);
                    }

                    data = JsonHelper.ToJson(new ValidatorStrategyPreset(period.SelectedStrategy.Strategy));
                    pathToStrategy = Path.Combine(pathToPeriodStrategy, "strategy.json");
                    File.WriteAllText(pathToStrategy, data);

                    Directory.CreateDirectory(pathToPeriodStrategy);

                    var isTrades = JsonHelper.ToJson(strategy.IsPerformance.Trades);
                    var pathToPeriodStrategyIsTrades = Path.Combine(pathToPeriodStrategy, "isTrades.json");
                    File.WriteAllText(pathToPeriodStrategyIsTrades, isTrades);

                    if (strategy.OsPerformance != null)
                    {

                        var osTrades = JsonHelper.ToJson(strategy.OsPerformance.Trades);
                        var pathToPeriodStrategyOsTrades = Path.Combine(pathToPeriodStrategy, "osTrades.json");
                        File.WriteAllText(pathToPeriodStrategyOsTrades, osTrades);
                    }

                    if (strategy.SimPerformance != null)
                    {
                        var simTrades = JsonHelper.ToJson(strategy.SimPerformance.Trades);
                        var pathToPeriodStrategySimTrades = Path.Combine(pathToPeriodStrategy, "simTrades.json");
                        File.WriteAllText(pathToPeriodStrategySimTrades, simTrades);
                    }

                    //  i++;
                }
            }


            using (var context = Session.Instance.DbContextFactory.GetContext())
            {
                var repository = new OptimizerResultRepository(context);


                await repository.Add(optimizerResult);
                await repository.CompleteAsync();
            }

            MessageBox.Show("Optimization results successfully saved!");
        }


        


        

        private async void OptimizerTestPresetSave(object obj)
        {
            if (SelectedOptimizerItem != null)
            {
                OptimizerTestViewModel test = null;

                if (SelectedOptimizerItem is OptimizerPeriodViewModel optimizerPeriodViewModel)
                {
                    test = optimizerPeriodViewModel.Test;
                }
                else if (SelectedOptimizerItem is OptimizerTestViewModel optimizerTestViewModel)
                {
                    test = optimizerTestViewModel;
                }


                var presetsViewModel = new PresetsViewModel(PresetType.OptimizerTest);
                presetsViewModel.FetchData();
                presetsViewModel.PresetName = test.Name;

                var presetsWindow = new PresetsWindow(presetsViewModel);
                if (presetsWindow.ShowDialog() == true)
                {
                    Preset preset = null;
                    if (presetsViewModel.SelectedPreset != null)
                        preset = presetsViewModel.SelectedPreset.DataModel;
                    await SaveTestPreset(test, preset, presetsViewModel.PresetName);
                }
            }
        }


        private async Task SaveTestPreset(OptimizerTestViewModel test, Preset preset, string name)
        {
            using (var context = Session.Instance.DbContextFactory.GetContext())
            {
                var repository = new PresetRepository(context);

                if (preset == null)
                {
                    preset = new Preset()
                    {
                        Name = name,
                        Type = (int)PresetType.OptimizerTest,
                        Data = JsonHelper.ToJson(new PresetObject<OptimizerTestViewModel>()
                        { Object = test, PresetType = PresetType.OptimizerTest })
                    };
                    await repository.Add(preset);
                }
                else
                {
                    preset.Data = JsonHelper.ToJson(new PresetObject<OptimizerTestViewModel>()
                    { Object = test, PresetType = PresetType.OptimizerTest });
                    await repository.Update(preset);
                }

                await repository.CompleteAsync();
            }
        }


        private void OptimizerTestPresetLoad(object obj)
        {
            var presetsViewModel = new PresetsViewModel(PresetType.OptimizerTest, PresetFormMode.Load);
            presetsViewModel.FetchData();

            presetsViewModel.SelectedPreset = presetsViewModel.Presets.FirstOrDefault();

            var presetsWindow = new PresetsWindow(presetsViewModel);
            if (presetsWindow.ShowDialog() == true)
            {
                if (presetsViewModel.SelectedPreset != null &&
                    !string.IsNullOrEmpty(presetsViewModel.SelectedPreset.Data))
                {
                    OptimizerTestViewModel test = CreateTestByPreset(presetsViewModel.SelectedPreset.Data,
                        presetsViewModel.IsLivePreset);

                    if (test != null)
                    {
                        if (SelectedOptimizerItem is IOptimizerItem optimizerItem)
                            optimizerItem.Strategy = null;
                        Tests.Clear();
                        Tests.Add(test);
                        test.IsExpanded = true;
                        SelectedOptimizerItem = test;

                        OnPropertyChanged(nameof(CanRun));
                    }
                }
            }
        }


        private OptimizerTestViewModel CreateTestByPreset(string json, bool isLivePreset)
        {
            OptimizerTestViewModel test = null;
            try
            {
                var presetObject =
                    JsonHelper.ToObject<PresetObject<OptimizerTestViewModel>>(json);

                test = presetObject.Object;
                test.ParentViewModel = this;

                foreach (var period in test.Periods)
                {
                    period.Test = test;
                    period.ISStartDate = DateTime.SpecifyKind(period.ISStartDate, DateTimeKind.Unspecified);
                    period.ISEndDate = DateTime.SpecifyKind(period.ISEndDate, DateTimeKind.Unspecified);
                    period.OSStartDate = DateTime.SpecifyKind(period.OSStartDate, DateTimeKind.Unspecified);
                    period.OSEndDate = DateTime.SpecifyKind(period.OSEndDate, DateTimeKind.Unspecified);

                }


                if (isLivePreset)
                {
                    var lastPeriod = test.Periods.LastOrDefault();

                    var osPercent = lastPeriod.OSPercent;
                    var totalDays = lastPeriod.TotalDays;
                    var daysShift = lastPeriod.OSDays;
                    var endDate = DateTime.Today.AddDays(daysShift);
                    var startDate = DateTime.Today;

                    lastPeriod.OSStartDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, 0, 0, 0, 0, DateTimeKind.Unspecified);
                    lastPeriod.OSEndDate = new DateTime(endDate.Year, endDate.Month, endDate.Day, 0, 0, 0, 0, DateTimeKind.Unspecified);


                    lastPeriod.TotalDays = totalDays;
                    lastPeriod.OSPercent = osPercent;
                    lastPeriod.IsLive = true;



                    var periods = test.Periods.OrderByDescending(x => x.Number).ToList();

                    for (var i = 1; i < periods.Count; i++)
                    {
                        periods[i].OSEndDate = periods[i - 1].ISEndDate;
                        periods[i].TotalDays = periods[i - 1].TotalDays;
                        periods[i].OSPercent = periods[i - 1].OSPercent;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can`t load preset: " + ex.Message);
                LogController.Print("Can`t load preset: " + ex.Message + "\r\n" + ex.StackTrace);
            }

            return test;
        }


        private void OptimizerTestPresetExport(object obj)
        {
            var presetsViewModel = new PresetsViewModel(PresetType.OptimizerTest, PresetFormMode.Export);
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

        private async void OptimizerStrategyPresetSave(object obj)
        {
            if (Strategy != null)
            {
                // var preset = await SaveStrategyPreset(Strategy);


                var presetsViewModel = new PresetsViewModel(PresetType.OptimizerStrategy);
                presetsViewModel.FetchData();
                presetsViewModel.PresetName = StrategyName;

                var presetsWindow = new PresetsWindow(presetsViewModel);
                if (presetsWindow.ShowDialog() == true)
                {
                    Preset preset = null;
                    if (presetsViewModel.SelectedPreset != null)
                        preset = presetsViewModel.SelectedPreset.DataModel;
                    await SaveStrategyPreset(Strategy, preset, presetsViewModel.PresetName);
                }
            }
        }


        private async Task SaveStrategyPreset(StrategyBase strategy, Preset preset, string name)
        {
            using (var context = Session.Instance.DbContextFactory.GetContext())
            {
                var repository = new PresetRepository(context);
                var optimizerStrategyPreset = new OptimizerStrategyPreset(strategy);

                if (preset == null)
                {
                    preset = new Preset()
                    {
                        Name = name,
                        Guid = strategy.Guid.ToString(),
                        Type = (int)PresetType.OptimizerStrategy,
                        Data = JsonHelper.ToJson(new PresetObject<OptimizerStrategyPreset>()
                        { Object = optimizerStrategyPreset, PresetType = PresetType.OptimizerStrategy })
                    };
                    await repository.Add(preset);
                }
                else
                {
                    preset.Data = JsonHelper.ToJson(new PresetObject<OptimizerStrategyPreset>()
                    { Object = optimizerStrategyPreset, PresetType = PresetType.OptimizerStrategy });
                    await repository.Update(preset);
                }

                await repository.CompleteAsync();
            }
        }


        private void OptimizerStrategyPresetLoad(object obj)
        {
            var presetsViewModel = new PresetsViewModel(PresetType.OptimizerStrategy, PresetFormMode.Load);
            if (Strategy == null)
                presetsViewModel.FetchData();
            else
                presetsViewModel.FetchData(Strategy.Guid.ToString());

            presetsViewModel.SelectedPreset = presetsViewModel.Presets.FirstOrDefault();


            var presetsWindow = new PresetsWindow(presetsViewModel);
            if (presetsWindow.ShowDialog() == true)
            {
                if (presetsViewModel.SelectedPreset != null &&
                    !string.IsNullOrEmpty(presetsViewModel.SelectedPreset.Data))
                {
                    StrategyBase strategy = CreateStrategyByPreset(presetsViewModel.SelectedPreset.Data);

                    if (SelectedOptimizerItem is IOptimizerItem optimizerItem)
                    {
                        optimizerItem.Strategy = strategy;
                        OnPropertyChanged(nameof(CanRun));
                    }
                }
            }
        }


        private StrategyBase CreateStrategyByPreset(string json)
        {
            StrategyBase strategy = null;
            try
            {
                OptimizerTestViewModel test = null;

                if (SelectedOptimizerItem is OptimizerPeriodViewModel optimizerPeriodViewModel)
                {
                    test = optimizerPeriodViewModel.Test;
                }
                else if (SelectedOptimizerItem is OptimizerTestViewModel optimizerTestViewModel)
                {
                    test = optimizerTestViewModel;
                }


                if (test != null)
                {
                    var presetObject =
                        JsonHelper.ToObject<PresetObject<OptimizerStrategyPreset>>(json);


                    var strategyType = BuildController.Instance.GetStrategyType(presetObject.Object.StrategyType);
                    var scriptObject =
                        test.DataCalcContext.CreateObject(strategyType, null, true);

                    strategy = (StrategyBase)scriptObject;
                    var connection = Session.ConfiguredConnections.FirstOrDefault(x => x.Code == presetObject.Object.ConnectionCode);
                    strategy.DataSeriesSeriesParams = new DataSeriesParamsViewModel()
                    {
                        SelectedConnection = connection,
                        SelectedType = presetObject.Object.Type,
                        DataSeriesType = presetObject.Object.DataSeriesType,
                        DataSeriesValue = presetObject.Object.DataSeriesValue,
                        DataSeriesFormat = presetObject.Object.DataSeriesFormat,
                        
                        Instrument = Session.Instance.GetInstrument(presetObject.Object.Symbol, presetObject.Object.Type, presetObject.Object.ConnectionCode).Result
                    };
                    strategy.DataSeriesSeriesParams.DataSeriesFormat = strategy.DataSeriesSeriesParams.DataSeriesFormats.FirstOrDefault(x => x.Type == strategy.DataSeriesSeriesParams.DataSeriesFormat.Type && x.Value == strategy.DataSeriesSeriesParams.DataSeriesFormat.Value);
                    strategy.OptimizationFitnessType = BuildController.Instance.OptimizationFitnessTypes.FirstOrDefault(
                    x =>
                            x.ObjectType.Name == presetObject.Object.OptimizationFitnessType);

                    var presetOptimizerType = BuildController.Instance.OptimizerStrategyTypes.FirstOrDefault(
                        x =>
                            x.ObjectType.Name == presetObject.Object.OptimizerType);

                    if (presetOptimizerType != null && presetOptimizerType.ObjectType != null &&
                        strategy.OptimizerType.ObjectType != presetOptimizerType?.ObjectType)
                    {
                        strategy.OptimizerType = presetOptimizerType;
                        strategy.CreateOptimizer();

                        var propertyInfos = presetObject.Object.Optimizer.GetType().GetProperties()
                            .Where(x => Attribute.IsDefined(x, typeof(DataMemberAttribute)));


                        var propertyInfos2 = strategy.Optimizer.GetType().GetProperties()
                            .Where(x => Attribute.IsDefined(x, typeof(DataMemberAttribute))).ToList();


                        foreach (var pi in propertyInfos)
                        {
                            if (pi.SetMethod != null)
                            {
                                var value = pi.GetValue(presetObject.Object.Optimizer);

                                var pi2 = propertyInfos2.FirstOrDefault(x => x.Name == pi.Name);
                                if (pi2 != null)
                                {
                                    pi2.SetValue(strategy.Optimizer, value);
                                }
                            }
                        }
                    }

                    var presetCommisionType = BuildController.Instance.CommissionTypes.FirstOrDefault(
                       x =>
                           x.ObjectType.Name == presetObject.Object.CommissionType);

                    if (presetCommisionType != null && presetCommisionType.ObjectType != null &&
                        strategy.CommissionType.ObjectType != presetCommisionType?.ObjectType)
                    {
                        strategy.CommissionType = presetCommisionType;
                        strategy.CreateCommission();

                        var propertyInfos = presetObject.Object.Commission.GetType().GetProperties()
                            .Where(x => Attribute.IsDefined(x, typeof(DataMemberAttribute)));


                        var propertyInfos2 = strategy.Commission.GetType().GetProperties()
                            .Where(x => Attribute.IsDefined(x, typeof(DataMemberAttribute))).ToList();


                        foreach (var pi in propertyInfos)
                        {
                            if (pi.SetMethod != null)
                            {
                                var value = pi.GetValue(presetObject.Object.Commission);

                                var pi2 = propertyInfos2.FirstOrDefault(x => x.Name == pi.Name);
                                if (pi2 != null)
                                {
                                    pi2.SetValue(strategy.Commission, value);
                                }
                            }
                        }

                    }


                        if (strategy.Optimizer != null)
                    {
                        strategy.Optimizer.MinTrades = presetObject.Object.Optimizer.MinTrades;
                        strategy.Optimizer.DrawDownLevel = presetObject.Object.Optimizer.DrawDownLevel;
                        strategy.Optimizer.KeepBestNumber = presetObject.Object.Optimizer.KeepBestNumber;
                        strategy.Optimizer.TaskBatchSize = presetObject.Object.Optimizer.TaskBatchSize;
                        strategy.Optimizer.CreateParams();

                        foreach (var parameter in strategy.Optimizer.OptimizerParameters)
                        {
                            var presetParameter =
                                presetObject.Object.Parameters.FirstOrDefault(x => x.Name == parameter.Name);
                            if (presetParameter != null)
                            {
                                presetParameter.CopyPreset(parameter);
                                parameter.IsChecked = presetParameter.IsChecked;
                            }
                        }
                    }


                    if (test.Strategy != null)
                    {
                        test.Strategy.PropertyChanged -= StrategyOnPropertyChanged;
                    }


                    test.SetStrategy(strategy, false);

                    if (test.Strategy != null)
                    {
                        test.Strategy.PropertyChanged += StrategyOnPropertyChanged;
                    }


                    OnPropertyChanged("Strategy");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can`t load preset: " + ex.Message);
                LogController.Print("Can`t load preset: " + ex.Message + "\r\n" + ex.StackTrace);
            }

            return strategy;
        }

        private void OptimizerStrategyPresetExport(object obj)
        {
            var presetsViewModel = new PresetsViewModel(PresetType.OptimizerStrategy, PresetFormMode.Export);
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

        private System.Timers.Timer _timer;

        private void Run(object obj)
        {
            Destroy(false);
            if (SelectedOptimizerItem != null)
            {
                OptimizerTestViewModel test = null;
                TotalTime = new TimeSpan(0);


                _timer = new System.Timers.Timer { AutoReset = true, Interval = 1000 };
                _timer.Elapsed += _timer_Elapsed;
                _timer.Start();

                if (SelectedOptimizerItem is OptimizerPeriodViewModel optimizerPeriodViewModel)
                {
                    test = optimizerPeriodViewModel.Test;
                }
                else if (SelectedOptimizerItem is OptimizerTestViewModel optimizerTestViewModel)
                {
                    test = optimizerTestViewModel;
                }

                test.CalculateEvent = new ManualResetEventSlim(false);

                if (!Session.Instance.DataCalcContexts.Contains(test.DataCalcContext))
                    Session.Instance.DataCalcContexts.Add(test.DataCalcContext);

                if (test.Periods.Any())
                {
                    var strategy = test.DataCalcContext.Strategies.FirstOrDefault();
                    if (strategy != null && test.Strategy != strategy)
                    {
                        //If recompile
                        test.SetStrategy(strategy, false);
                    }


                    _optimizationTasks.Clear();
                    _ctsOptimize = new CancellationTokenSource();

                    test.TaskViewModels.Clear();

                    var periodCount = test.Periods.Count;
                    var lastPeriod = test.Periods.LastOrDefault();
                    DateTime isPeriodEnd;
                    DateTime periodEnd;
                    var dataSeriesParam = test.DataCalcContext.GetParams();
                    Barrier barrier = null;
                    var connection = Session.Instance.GetConnection(dataSeriesParam.Instrument.ConnectionId);
                    using (var context = App.DbContextFactory.GetContext())
                    {
                        var historicalMetaData = context.HistoricalMetaDatas.FirstOrDefault(
                            x => x.Symbol == dataSeriesParam.Instrument.Symbol &&
                                 x.DataType == "Candle" &&
                                 x.DataProviderId == connection.DataProviderId &&
                                 x.DataSeriesValue == dataSeriesParam.DataSeriesValue &&
                                 x.DataSeriesType == dataSeriesParam.DataSeriesType.ToString());

                        if (lastPeriod == null || SystemOptions.Instance.CalculateSimulation)
                        {
                            periodEnd = DateTime.UtcNow;
                            //periodEnd = TimeZoneInfo.ConvertTime(periodEnd, SystemOptions.Instance.TimeZone);

                        }
                        else
                        {
                            var dateTimeEndUnspecified = DateTime.SpecifyKind(lastPeriod.ISEndDate, DateTimeKind.Unspecified);
                            isPeriodEnd = TimeZoneInfo.ConvertTimeToUtc(dateTimeEndUnspecified, SystemOptions.Instance.TimeZone);

                            if (!lastPeriod.IsLive)
                            {
                                dateTimeEndUnspecified = DateTime.SpecifyKind(lastPeriod.OSEndDate, DateTimeKind.Unspecified);
                                periodEnd = TimeZoneInfo.ConvertTimeToUtc(dateTimeEndUnspecified, SystemOptions.Instance.TimeZone);
                            }
                            else
                            {
                                periodEnd = isPeriodEnd;
                            }

                            if (periodEnd.Date == DateTime.UtcNow.Date)
                            {
                                periodEnd = DateTime.UtcNow;

                            }
                        }

                        if (historicalMetaData == null || historicalMetaData.PeriodEnd < periodEnd)
                        {




                            //Need download from service
                            if (!Session.IsConnectionActive(dataSeriesParam.Instrument.ConnectionId))
                            {
                                var periodEndStr = periodEnd.ToString("dd.MM.yyyy HH:mm");

                                var historicalMetaDataPeriodEnd = historicalMetaData == null
                                    ? "empty"
                                    : historicalMetaData.PeriodEnd.ToString("dd.MM.yyyy HH:mm");

                                if (MessageBox.Show(
                                        "You don't have enough data to run the test and you are not connected!!!." + "\r\n" +
                                        $"Period EndDateTime: {periodEndStr} UTC" + "\r\n" +
                                        $"Meta EndDateTime: {historicalMetaDataPeriodEnd} UTC" + "\r\n" +
                                        "Press OK if you wish to start optimization anyway.", "Сonfirmation",
                                        MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                                {
                                    Stop(null);

                                    return;
                                }
                            }
                        }

                        if (historicalMetaData == null || historicalMetaData.PeriodEnd < periodEnd)
                        {
                            barrier = new Barrier(periodCount);
                        }
                    }

                    test.StartPerformanceCalc = false;

                    foreach (var optimizerPeriod in test.Periods)
                    {
                        optimizerPeriod.SelectedStrategy = null;

                        var taskVm = new TaskViewModel
                        {
                            Name = optimizerPeriod.PeriodName,
                            OptimizerPeriod = optimizerPeriod
                        };
                        taskVm.Subscribe();
                        test.TaskViewModels.Add(taskVm);

                        var task = Task.Run(async () =>
                            await optimizerPeriod.Optimize(_ctsOptimize, barrier, optimizerPeriod == lastPeriod, periodEnd));
                        taskVm.TaskId = task.Id;
                        _optimizationTasks.Add(task);
                    }

                    Task.Run(async () =>
                    {
                        bool hasErros = false;
                        try
                        {
                            await Task.WhenAll(_optimizationTasks.ToArray()).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            hasErros = true;
                            var taskError = _optimizationTasks.FirstOrDefault(task => task.Exception != null);
                            if (taskError != null)
                            {
                                var taskVmError = test.TaskViewModels.FirstOrDefault(x => x.TaskId == taskError.Id);

                                if (taskVmError != null)
                                {
                                    foreach (var task in test.TaskViewModels)
                                    {
                                        if (task != taskVmError)
                                            task.OptimizerPeriod.OnCancelOptimize();
                                    }

                                    var strategyName = "";
                                    if (taskVmError.Optimizer != null && taskVmError.Optimizer.LastStrategy != null)
                                        strategyName = taskVmError.Optimizer.LastStrategy.DisplayName;
                                    taskVmError.TaskException = ex;
                                    taskVmError.OptimizerPeriod.OnErrorOptimize(
                                        "Calculating: " + strategyName,
                                        "Error: " + ex.Message);
                                }
                            }
                        }

                        try
                        {
                            test.CalculateEvent.Wait(_ctsOptimize.Token);
                        }
                        finally
                        {
                            _ctsOptimize?.Cancel();
                            _timer?.Stop();
                            IsOptimizeRunning = false;
                            test.IsOptimizeRunning = false;

                            foreach (var period in test.Periods)
                            {
                                period.OnCanceledOptimize();
                            }

                            _ctsOptimize = new CancellationTokenSource();
                            _canRun = true;
                            OnPropertyChanged("CanRun");
                        }
                    });

                    IsOptimizeRunning = true;
                    test.IsOptimizeRunning = true;
                }
            }
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TotalTime = TotalTime?.Add(new TimeSpan(0, 0, 0, 1));
        }


        private async void Stop(object obj)
        {
            _ctsOptimize?.Cancel();

            _timer?.Stop();

            OptimizerTestViewModel test = null;

            if (SelectedOptimizerItem is OptimizerPeriodViewModel optimizerPeriodViewModel)
            {
                test = optimizerPeriodViewModel.Test;
            }
            else if (SelectedOptimizerItem is OptimizerTestViewModel optimizerTestViewModel)
            {
                test = optimizerTestViewModel;
            }


            if (test != null)
            {
                OptimizerPeriodViewModel errorPeriod = null;
                if (obj != null && obj is OptimizerPeriodViewModel optimizerPeriodVm)
                {
                    errorPeriod = optimizerPeriodVm;
                }

                test.IsOptimizeRunning = false;
                await Task.Delay(1000);
                foreach (var period in test.Periods)
                {
                    if (errorPeriod != null && errorPeriod == period)
                        continue;
                    period.OnCancelOptimize();
                }
            }
            _canRun = false;

            IsOptimizeRunning = false;



        }


        private void AddOptimizerTest(object obj)
        {
            var test = new OptimizerTestViewModel() { ParentViewModel = this };


            var optimizerTestWindow = new OptimizerTestWindow(test);
            if (optimizerTestWindow.ShowDialog() == true)
            {
                Tests.Add(test);
                SelectedOptimizerItem = test;
            }
        }

        private void AddOptimizerPeriod(object obj)
        {
            if (SelectedOptimizerItem != null)
            {
                var optimizerPeriod = new OptimizerPeriodViewModel { ParentViewModel = this };
                if (SelectedOptimizerItem is OptimizerTestViewModel test)
                {
                    optimizerPeriod.Test = test;
                }
                else if (SelectedOptimizerItem is OptimizerPeriodViewModel period)
                {
                    optimizerPeriod.Test = period.Test;
                }

                optimizerPeriod.Number = optimizerPeriod.Test.Periods.Count + 1;

                if (optimizerPeriod.Number == 1)
                {
                    var nowDate = DateTime.Now.Date;
                    optimizerPeriod.ISStartDate = new DateTime(nowDate.Year, 01, 01);
                    optimizerPeriod.ISEndDate = new DateTime(nowDate.Year, 01, 01);
                }
                else
                {
                    var prevPeriod =
                        optimizerPeriod.Test.Periods.FirstOrDefault(x => x.Number == optimizerPeriod.Number - 1);

                    if (prevPeriod != null)
                    {
                        optimizerPeriod.ISStartDate = prevPeriod.ISStartDate;
                        optimizerPeriod.ISEndDate = prevPeriod.OSEndDate;
                    }
                    else
                    {
                        var nowDate = DateTime.Now.Date;
                        optimizerPeriod.ISStartDate = new DateTime(nowDate.Year, 01, 01);
                        optimizerPeriod.ISEndDate = new DateTime(nowDate.Year, 01, 01);
                    }
                }


                var optimizerPeriodWindow = new OptimizerPeriodWindow(optimizerPeriod);
                if (optimizerPeriodWindow.ShowDialog() == true)
                {
                    optimizerPeriod.Test.Periods.Add(optimizerPeriod);
                    optimizerPeriod.Test.IsExpanded = true;
                    SelectedOptimizerItem = optimizerPeriod;
                }
            }
        }


        private CancellationTokenSource _ctsOptimize;
        private bool _canAddTest;


        private void Remove(object obj)
        {
            if (SelectedOptimizerItem != null)
            {
                if (SelectedOptimizerItem is OptimizerTestViewModel test)
                {
                    Destroy();
                    Tests.Remove(test);

                    if (Tests.Any())
                    {
                        SelectedOptimizerItem = Tests.FirstOrDefault();
                    }
                    else
                    {
                        SelectedOptimizerItem = null;
                    }
                }
                else if (SelectedOptimizerItem is OptimizerPeriodViewModel period)
                {
                    var items = period.Test.Periods.Where(x => x.Number > period.Number);

                    foreach (var item in items)
                    {
                        item.Number--;
                    }

                    period.Test.Periods.Remove(period);

                    if (period.Test.Periods.Any())
                    {
                        SelectedOptimizerItem = period.Test.Periods.FirstOrDefault();
                    }
                    else
                    {
                        SelectedOptimizerItem = period.Test;
                    }
                }
            }
        }

        private void Edit(object obj)
        {
            if (SelectedOptimizerItem != null)
            {
                if (SelectedOptimizerItem is OptimizerTestViewModel test)
                {
                    var editTest = (OptimizerTestViewModel)test.Clone();
                    var optimizerTestWindow = new OptimizerTestWindow(editTest);
                    if (optimizerTestWindow.ShowDialog() == true)
                    {
                        test.Name = editTest.Name;
                    }
                }
                else if (SelectedOptimizerItem is OptimizerPeriodViewModel period)
                {
                    var editPeriod = (OptimizerPeriodViewModel)period.Clone();
                    var optimizerPeriodWindow = new OptimizerPeriodWindow(editPeriod);
                    if (optimizerPeriodWindow.ShowDialog() == true)
                    {
                        period.ISStartDate = editPeriod.ISStartDate;
                        period.OSStartDate = editPeriod.OSStartDate;
                        period.ISEndDate = editPeriod.ISEndDate;
                        period.OSEndDate = editPeriod.OSEndDate;
                    }
                }
            }
        }


        private void EditPeriods(object obj)
        {
            if (SelectedOptimizerItem != null)
            {
                ObservableCollection<OptimizerPeriodViewModel> periods = null;
                OptimizerTestViewModel test = null;
                if (SelectedOptimizerItem is OptimizerTestViewModel optimizerTestViewModel)
                {
                    periods = optimizerTestViewModel.Periods;
                    test = optimizerTestViewModel;
                }
                else if (SelectedOptimizerItem is OptimizerPeriodViewModel period)
                {
                    periods = period.Test.Periods;
                    test = period.Test;
                }


                var optimizerPeriodsEditViewModel = new OptimizerPeriodsEditViewModel();

                OptimizerPeriodEditViewModel prevPeriod = null;
                foreach (var period in periods)
                {
                    var currentPeriod = new OptimizerPeriodEditViewModel(period);

                    if (prevPeriod != null)
                        prevPeriod.NextPeriod = currentPeriod;

                    currentPeriod.PrevPeriod = prevPeriod;
                    prevPeriod = currentPeriod;
                    optimizerPeriodsEditViewModel.Periods.Add(currentPeriod);
                }

                if (!optimizerPeriodsEditViewModel.Periods.Any())
                {
                    optimizerPeriodsEditViewModel.AddPeriodCommand.Execute(null);
                }


                optimizerPeriodsEditViewModel.IsLastPeriodLive =
                    optimizerPeriodsEditViewModel.Periods.Any(x => x.IsLive);

                var optimizerPeriodsEditWindow = new OptimizerPeriodsEditWindow(optimizerPeriodsEditViewModel);
                if (optimizerPeriodsEditWindow.ShowDialog() == true)
                {
                    if (test != null)
                    {
                        test.Periods.Clear();

                        foreach (var editPeriod in optimizerPeriodsEditViewModel.Periods)
                        {
                            editPeriod.Period.Test = test;
                            test.Periods.Add(editPeriod.Period);
                        }

                        test.IsExpanded = true;
                        SelectedOptimizerItem = test;
                    }
                }
            }
        }

        private void SelectStrategy(object obj)
        {
            OptimizerTestViewModel test = null;
            if (SelectedOptimizerItem is OptimizerTestViewModel optimizerTestViewModel)
            {
                test = optimizerTestViewModel;
            }
            else if (SelectedOptimizerItem is OptimizerPeriodViewModel period)
            {
                test = period.Test;
            }

            if (test != null)
            {
                var selectScriptObjectViewModel =
                    new SelectScriptObjectViewModel(typeof(StrategyBase), test.DataCalcContext);
                var strategySelectWindow = new StrategySelectWindow(selectScriptObjectViewModel);
                selectScriptObjectViewModel.Init();
                if (strategySelectWindow.ShowDialog() == true)
                {
                    if (selectScriptObjectViewModel.SelectedObjectType is ScriptObjectItemViewModel scriptItemViewModel)
                    {
                        var scriptObject =
                            test.DataCalcContext.CreateObject(scriptItemViewModel.ObjectType, null,
                                true);
                        if (test.Strategy != null)
                        {
                            test.Strategy.PropertyChanged -= StrategyOnPropertyChanged;
                        }

                        test.Strategy = (StrategyBase)scriptObject;
                        if (test.Strategy != null)
                        {
                            test.Strategy.PropertyChanged += StrategyOnPropertyChanged;
                        }
                    }
                }
            }
        }

        private void StrategyOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(CanRun));
        }

        public void Clear()
        {

            foreach (var test in Tests)
            {
                foreach (var taskViewModel in test.TaskViewModels)
                {
                    taskViewModel.OptimizerPeriod = null;
                    taskViewModel.Optimizer = null;
                }
                test.TaskViewModels.Clear();

                Session.Instance.DataCalcContexts.Remove(test.DataCalcContext);

                if (test.Strategy != null)
                {
                    test.Strategy.PropertyChanged -= StrategyOnPropertyChanged;
                }

                foreach (var period in test.Periods)
                {

                    foreach (var strategy in period.Strategies)
                    {
                        strategy.Reset();
                        strategy.SystemPerformance.Destroy();
                    }

                    foreach (var strategyViewModel in period.StrategyViewModels)
                    {
                        strategyViewModel.IsPerformance?.Clear();
                        strategyViewModel.OsPerformance?.Clear();
                        strategyViewModel.SimPerformance?.Clear();
                        strategyViewModel.TotalPerformance?.Clear();
                        strategyViewModel.Clear();
                    }

                    period.StrategyViewModels.Clear();
                    period.IsPerformance?.Clear();
                    period.OsPerformance?.Clear();
                    period.SimPerformance?.Clear();
                    period.TotalPerformance?.Clear();




                    period.Strategies.Clear();

                    Session.Instance.DataCalcContexts.Remove(period.IsPerformance?.ChartViewModel?.DataCalcContext);
                    Session.Instance.DataCalcContexts.Remove(period.OsPerformance?.ChartViewModel?.DataCalcContext);
                    Session.Instance.DataCalcContexts.Remove(period.SimPerformance?.ChartViewModel?.DataCalcContext);
                }
            }
        }

        public void Destroy(bool full = true)
        {
            foreach (var test in Tests)
            {
                foreach (var taskViewModel in test.TaskViewModels)
                {
                    taskViewModel.OptimizerPeriod = null;
                    taskViewModel.Optimizer = null;
                }
                test.TaskViewModels.Clear();


                Session.Instance.DataCalcContexts.Remove(test.DataCalcContext);

                if (test.Strategy != null)
                {
                    test.Strategy.PropertyChanged -= StrategyOnPropertyChanged;
                }

                if (full)
                {
                    test.DataCalcContext?.Destroy();
                    test.DataCalcContext = null;
                    test.SelectedStrategy?.Strategy.Destroy();
                }

                foreach (var period in test.Periods)
                {
                    period.IsPerformance?.ChartViewModel?.DataCalcContext?.Destroy();
                    period.OsPerformance?.ChartViewModel?.DataCalcContext?.Destroy();
                    period.SimPerformance?.ChartViewModel?.DataCalcContext?.Destroy();

                    Session.Instance.DataCalcContexts.Remove(period.IsPerformance?.ChartViewModel?.DataCalcContext);
                    Session.Instance.DataCalcContexts.Remove(period.OsPerformance?.ChartViewModel?.DataCalcContext);
                    Session.Instance.DataCalcContexts.Remove(period.SimPerformance?.ChartViewModel?.DataCalcContext);


                    foreach (var strategy in period.Strategies)
                    {
                        if (full)
                        {
                            strategy.Optimizer.Clear();
                            strategy.Optimizer.SetDataCalcContext(null);
                            strategy.Destroy();
                        }
                        else
                        {

                            strategy.Clear();
                        }

                        strategy.SystemPerformance.Destroy();
                    }

                    foreach (var strategyViewModel in period.StrategyViewModels)
                    {
                        strategyViewModel.IsPerformance?.Destroy();
                        strategyViewModel.OsPerformance?.Destroy();
                        strategyViewModel.SimPerformance?.Destroy();
                        strategyViewModel.TotalPerformance?.Destroy();
                        strategyViewModel.Clear();
                    }


                    period.StrategyViewModels.Clear();
                    period.IsPerformance?.Destroy();
                    period.OsPerformance?.Destroy();
                    period.SimPerformance?.Destroy();
                    period.TotalPerformance?.Destroy();
                    period.PeriodStrategy?.Destroy();
                    period.PeriodStrategy = null;

                    if (full)
                    {
                        period.Strategy?.Destroy();
                        period.Strategy = null;
                    }
                    else
                        period.Strategy?.Clear();

                    period.SelectedStrategy = null;
                  

                    if (full)
                        period.Test = null;

                    period.ISDataCalcContext?.Destroy();
                    if (full)
                        period.ISDataCalcContext = null;
                    period.OSDataCalcContext?.Destroy();
                    period.OSDataCalcContext = null;
                    period.SimDataCalcContext?.Destroy();
                    period.SimDataCalcContext = null;




                    period.Strategies.Clear();



                }
            }
            if (full)
                BuildController.Instance.OnCompile -= Instance_OnCompile;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}