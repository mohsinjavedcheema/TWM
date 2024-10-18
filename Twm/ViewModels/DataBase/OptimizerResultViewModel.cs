using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Twm.Core.Controllers;
using Twm.Core.DataCalc;
using Twm.Core.DataCalc.Performance;
using Twm.Core.Enums;
using Twm.Core.Helpers;
using Twm.Core.Market;
using Twm.Core.ViewModels;
using Twm.Core.ViewModels.DataSeries;
using Twm.Model.Model;
using Twm.ViewModels.Presets;
using Twm.ViewModels.Strategies;
using Twm.ViewModels.Strategies.Optimizer;
using AutoMapper.Internal;

namespace Twm.ViewModels.DataBase
{
    public class OptimizerResultViewModel : ViewModelBase
    {
        public OptimizerResult DataModel { get; set; }

        public int Id
        {
            get
            {
                if (DataModel == null)
                    return 0;
                return DataModel.Id;
            }
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
            get { return DataModel?.Name; }
            set
            {
                if (DataModel.Name != value)
                {
                    DataModel.Name = value;
                    OnDataPropertyChanged();
                    OnPropertyChanged();
                }
            }
        }


        public string Guid
        {
            get { return DataModel?.Guid; }
            set
            {
                if (DataModel.Guid != value)
                {
                    DataModel.Guid = value;
                    OnDataPropertyChanged();
                    OnPropertyChanged();
                }
            }
        }


        public string StrategyGuid
        {
            get { return DataModel?.StrategyGuid; }
            set
            {
                if (DataModel.StrategyGuid != value)
                {
                    DataModel.StrategyGuid = value;
                    OnDataPropertyChanged();
                    OnPropertyChanged();
                }
            }
        }


        private string _strategyName;

        public string StrategyName
        {
            get { return _strategyName; }
            set
            {
                if (_strategyName != value)
                {
                    _strategyName = value;
                    OnPropertyChanged();
                }
            }
        }


        public string Version
        {
            get { return DataModel?.StrategyVersion; }
            set
            {
                if (DataModel.StrategyVersion != value)
                {
                    DataModel.StrategyVersion = value;
                    OnDataPropertyChanged();
                    OnPropertyChanged();
                }
            }
        }


        public string Symbol
        {
            get { return DataModel?.Symbol; }
            set
            {
                if (DataModel.Symbol != value)
                {
                    DataModel.Symbol = value;
                    OnDataPropertyChanged();
                    OnPropertyChanged();
                }
            }
        }


        private string _timeFrame;

        public string TimeFrame
        {
            get { return _timeFrame; }
            set
            {
                if (_timeFrame != value)
                {
                    _timeFrame = value;
                    OnPropertyChanged();
                }
            }
        }


        public DateTime Date
        {
            get { return DataModel.DateCreated; }
            set
            {
                if (DataModel.DateCreated != value)
                {
                    DataModel.DateCreated = value;
                    OnDataPropertyChanged();
                    OnPropertyChanged();
                }
            }
        }


        private string _pathToFiles;

        public OptimizerResultViewModel(OptimizerResult optimizerResult)
        {
            DataModel = optimizerResult;
            StrategyName = BuildController.Instance.GetStrategyNameByGuid(optimizerResult.StrategyGuid);
            TimeFrame = optimizerResult.DataSeriesValue + " " + optimizerResult.DataSeriesType;

            var pathToDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Twm" +
                            "\\OptimizerResults";
            _pathToFiles = Path.Combine(pathToDir, Guid);
        }


        public OptimizerTestViewModel CreateTest()
        {
            var pathToTest = Path.Combine(_pathToFiles, "test.json");
            var testJson = File.ReadAllText(pathToTest);
            OptimizerTestViewModel test = JsonHelper.ToObject<OptimizerTestViewModel>(testJson);
            if (test != null)
            {
                foreach (var period in test.Periods)
                {
                    period.Test = test;
                }

                test.IsExpanded = true;
            }

            return test;
        }

        public StrategyBase CreateStrategy(OptimizerTestViewModel test)
        {
            StrategyBase strategy = null;
            var pathToStrategy = Path.Combine(_pathToFiles, "strategy.json");
            var strategyJson = File.ReadAllText(pathToStrategy);

            if (test != null)
            {
                var optimizerStrategyPreset =
                    JsonHelper.ToObject<OptimizerStrategyPreset>(strategyJson);


                var strategyType = BuildController.Instance.GetStrategyType(optimizerStrategyPreset.StrategyType);
                var scriptObject =
                    test.DataCalcContext.CreateObject(strategyType, null, true);

                strategy = (StrategyBase) scriptObject;

                strategy.DataSeriesSeriesParams = new DataSeriesParamsViewModel()
                {
                    DataSeriesType = optimizerStrategyPreset.DataSeriesType,
                    DataSeriesValue = optimizerStrategyPreset.DataSeriesValue,
                    Instrument = Session.GetInstrument(optimizerStrategyPreset.Symbol, optimizerStrategyPreset.Type, optimizerStrategyPreset.ConnectionCode).Result
                };

                strategy.OptimizationFitnessType = BuildController.Instance.OptimizationFitnessTypes.FirstOrDefault(
                    x =>
                        x.ObjectType.Name == optimizerStrategyPreset.OptimizationFitnessType);

                test.Strategy = strategy;

                var presetOptimizerType = BuildController.Instance.OptimizerStrategyTypes.FirstOrDefault(
                    x =>
                        x.ObjectType.Name == optimizerStrategyPreset.OptimizerType);

                if (presetOptimizerType != null && presetOptimizerType.ObjectType != null &&
                    strategy.OptimizerType.ObjectType != presetOptimizerType?.ObjectType)
                {
                    strategy.OptimizerType = presetOptimizerType;
                    strategy.CreateOptimizer();

                    var propertyInfos = optimizerStrategyPreset.Optimizer.GetType().GetProperties()
                        .Where(x => Attribute.IsDefined(x, typeof(DataMemberAttribute)));


                    var propertyInfos2 = strategy.Optimizer.GetType().GetProperties()
                        .Where(x => Attribute.IsDefined(x, typeof(DataMemberAttribute))).ToList();


                    foreach (var pi in propertyInfos)
                    {
                        if (pi.SetMethod != null)
                        {
                            var value = pi.GetValue(optimizerStrategyPreset.Optimizer);

                            var pi2 = propertyInfos2.FirstOrDefault(x => x.Name == pi.Name);
                            if (pi2 != null)
                            {
                                pi2.SetValue(strategy.Optimizer, value);
                            }
                        }
                    }
                }


                if (test.Strategy.Optimizer != null)
                {
                    test.Strategy.Optimizer.MinTrades = optimizerStrategyPreset.Optimizer.MinTrades;
                    test.Strategy.Optimizer.KeepBestNumber = optimizerStrategyPreset.Optimizer.KeepBestNumber;
                    test.Strategy.Optimizer.DrawDownLevel = optimizerStrategyPreset.Optimizer.DrawDownLevel;
                    test.Strategy.Optimizer.TaskBatchSize = optimizerStrategyPreset.Optimizer.TaskBatchSize;
                    test.Strategy.Optimizer.CreateParams();

                    foreach (var parameter in test.Strategy.Optimizer.OptimizerParameters)
                    {
                        var presetParameter =
                            optimizerStrategyPreset.Parameters.FirstOrDefault(x => x.Name == parameter.Name);
                        if (presetParameter != null)
                        {
                            presetParameter.CopyPreset(parameter);
                            parameter.IsChecked = presetParameter.IsChecked;
                        }
                    }
                }
            }

            return strategy;
        }

        public async Task CreatePeriod(OptimizerPeriodViewModel period, StrategyBase strategy, CancellationTokenSource cancellationTokenSource)
        {
            period.PeriodStrategy = (StrategyBase)strategy.Clone();
            period.PeriodStrategy.Optimizer = (Core.DataCalc.Optimization.Optimizer)strategy.Optimizer.Clone();
            period.PeriodStrategy.Optimizer.Reset();
            period.PeriodStrategy.Optimizer.CancellationTokenSource = cancellationTokenSource;
            period.PeriodStrategy.SetTwmProperties();

            var pathToPeriod = Path.Combine(_pathToFiles, period.Number.ToString());
            var i = 1;
            var pathToPeriodStrategy = Path.Combine(pathToPeriod, i.ToString());
            if (Directory.Exists(pathToPeriodStrategy))
            {
                var dataCalcContext = new DataCalcContext(period.PeriodStrategy.GetDataCalcContext().Connection)
                {
                    
                    Candles = period.PeriodStrategy.GetDataCalcContext().Candles,
                    Instruments = period.PeriodStrategy.GetDataCalcContext().Instruments,
                    IsOptimization = true
                };


                var optimizedStrategy = (StrategyBase)strategy.Clone();
                optimizedStrategy.Init();
                optimizedStrategy.SetTwmProperties();

                optimizedStrategy.SetDataCalcContext(dataCalcContext);

                var pathToStrategy = Path.Combine(pathToPeriodStrategy, "strategy.json");
                var strategyJson = File.ReadAllText(pathToStrategy);
                var strategyObject = JsonHelper.ToObject<ValidatorStrategyPreset>(strategyJson);

                if (strategyObject.Parameters != null &&
                    strategyObject.Parameters.Any())
                {
                    foreach (var property in optimizedStrategy.TwmPropertyNames)
                    {
                        if (strategyObject.Parameters.TryGetValue(property, out var value))
                        {
                            optimizedStrategy.SetTwmPropertyValue(property, value);
                        }
                    }
                }

                var strategyVm = new StrategyViewModel(optimizedStrategy);
                period.StrategyViewModels.Add(strategyVm);
                await period.SetViewSource(period.StrategyViewModels);


                var totalTrades = new List<Trade>();

                var pathToPeriodStrategyIsTrades = Path.Combine(pathToPeriodStrategy, "isTrades.json");
                var isTradesJson = File.ReadAllText(pathToPeriodStrategyIsTrades);
                var isTrades = JsonHelper.ToObject<ObservableCollection<Trade>>(isTradesJson);

                var po = new PerformanceOptions { IsPortfolio = false ,
                    ParentViewModel = period,
                    ExcludeSections = new List<object>(){StrategyPerformanceSection.Chart, StrategyPerformanceSection.Orders} };
                isTrades.ForAll(x => x.Profit += +x.Commission);
                strategyVm.IsPerformance = new StrategyPerformanceViewModel(null, isTrades.ToArray(), po);

                await strategyVm.IsPerformance.Calculate(cancellationTokenSource.Token);
                optimizedStrategy.SystemPerformance = strategyVm.IsPerformance; ;
                period.IsPerformance = strategyVm.IsPerformance;
                period.IsPerformance.SelectSection(period.CurrentSection);
                optimizedStrategy.OptimizationFitness.OnCalculatePerformanceValue(optimizedStrategy);
                
                totalTrades.AddRange(isTrades);

                var pathToPeriodStrategyOsTrades = Path.Combine(pathToPeriodStrategy, "osTrades.json");
                if (File.Exists(pathToPeriodStrategyOsTrades))
                {
                    var osTradesJson = File.ReadAllText(pathToPeriodStrategyOsTrades);
                    var osTrades = JsonHelper.ToObject<ObservableCollection<Trade>>(osTradesJson);

                    strategyVm.OsPerformance = new StrategyPerformanceViewModel(null, osTrades.ToArray(), po)
                    {
                        ParentViewModel = period
                    };
                    await strategyVm.OsPerformance.Calculate(cancellationTokenSource.Token);
                    period.OsPerformance = strategyVm.OsPerformance;
                    period.OsPerformance.SelectSection(period.CurrentSection);
                    osTrades.ForAll(x => x.Profit += +x.Commission);
                    totalTrades.AddRange(osTrades);


                    strategyVm.TotalPerformance = new StrategyPerformanceViewModel(null, totalTrades.ToArray(), po);
                    await strategyVm.TotalPerformance.Calculate(cancellationTokenSource.Token);
                    period.TotalPerformance = strategyVm.TotalPerformance;
                    period.TotalPerformance.SelectSection(period.CurrentSection);
                }






                var pathToPeriodStrategySimTrades = Path.Combine(pathToPeriodStrategy, "simTrades.json");

                if (File.Exists(pathToPeriodStrategySimTrades))
                {
                    period.SimPerformanceVisibility = Visibility.Visible;
                    var simTradesJson = File.ReadAllText(pathToPeriodStrategySimTrades);
                    var simTrades = JsonHelper.ToObject<ObservableCollection<Trade>>(simTradesJson);

                    strategyVm.SimPerformance = new StrategyPerformanceViewModel(null, simTrades.ToArray(), po)
                    {
                        ParentViewModel = period
                    };
                    await strategyVm.SimPerformance.Calculate(cancellationTokenSource.Token);
                    period.SimPerformance = strategyVm.SimPerformance;
                    period.SimPerformance.SelectSection(period.CurrentSection);
                    simTrades.ForAll(x => x.Profit += +x.Commission);
                    totalTrades.AddRange(simTrades);

                    strategyVm.TotalPerformance = new StrategyPerformanceViewModel(null, totalTrades.ToArray(), po);
                    await strategyVm.TotalPerformance.Calculate(cancellationTokenSource.Token);
                    period.TotalPerformance = strategyVm.TotalPerformance;
                    period.TotalPerformance.SelectSection(period.CurrentSection);

                }
                else
                {
                    period.SimPerformanceVisibility = Visibility.Collapsed;
                }


                period.StrategiesView.MoveCurrentToFirst();
                period.SelectedStrategy = (StrategyViewModel)period.StrategiesView.CurrentItem;

            }

            var taskVm = new TaskViewModel
            {
                Name = period.PeriodName,
                OptimizerPeriod = period,
                Status = "Completed",
                AllTasks = period.PeriodStrategy.Optimizer.CombinationCount
            };
            taskVm.ActionInfo = $"Optimized {taskVm.AllTasks} combinations";

            period.Test.TaskViewModels.Add(taskVm);
        }


        public string GetTestJson()
        {
            var pathToTest = Path.Combine(_pathToFiles, "test.json");
            if (File.Exists(pathToTest))
            {
                var testJson = File.ReadAllText(pathToTest);
                return testJson;
            }

            return string.Empty;
        }
    

        public string GetStrategyJson()
        {
            var pathToTest = Path.Combine(_pathToFiles, "strategy.json");

            if (File.Exists(pathToTest))
            {
                var testJson = File.ReadAllText(pathToTest);
                return testJson;
            }

            return string.Empty;
        }


       

    }
}