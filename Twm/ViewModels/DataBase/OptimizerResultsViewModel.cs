using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Twm.Core.Classes;
using Twm.Core.ViewModels;
using Twm.DB.DAL.Repositories.OptimizerResults;
using Twm.ViewModels.Strategies.Optimizer;
using Twm.Windows.Strategies;
using PresetNameWindow = Twm.Core.UI.Windows.Presets.PresetNameWindow;

namespace Twm.ViewModels.DataBase
{
    public class OptimizerResultsViewModel:ViewModelBase
    {

        public ObservableCollection<OptimizerResultViewModel> OptimizerResults { get; set; }

        public ICollectionView OptimizerResultsView { get; set; }

        public OptimizerResultViewModel SelectedOptimizerResult { get; set; }


        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }


        public List<string> Symbols { get; set; }

        private string _selectedSymbol;
        public string SelectedSymbol
        {
            get { return _selectedSymbol; }
            set
            {
                if (_selectedSymbol != value)
                {
                    _selectedSymbol = value;
                    OnPropertyChanged();
                    OptimizerResultsView.Refresh();
                }
            }
        }


        public List<string> Strategies { get; set; }

        private string _selectedStrategy;
        public string SelectedStrategy
        {
            get { return _selectedStrategy; }
            set
            {
                if (_selectedStrategy != value)
                {
                    _selectedStrategy = value;
                    OnPropertyChanged();
                    OptimizerResultsView.Refresh();
                }
            }
        }


        public List<string> Versions { get; set; }

        private string _selectedVersion;
        public string SelectedVersion
        {
            get { return _selectedVersion; }
            set
            {
                if (_selectedVersion != value)
                {
                    _selectedVersion = value;
                    OnPropertyChanged();
                    OptimizerResultsView.Refresh();
                }
            }
        }


        public List<string> TimeFrames { get; set; }

        private string _selectedTimeFrame;
        public string SelectedTimeFrame
        {
            get { return _selectedTimeFrame; }
            set
            {
                if (_selectedTimeFrame != value)
                {
                    _selectedTimeFrame = value;
                    OnPropertyChanged();
                    OptimizerResultsView.Refresh();
                }
            }
        }



        public OperationCommand OpenCommand { get; set; }

        public OperationCommand RemoveCommand { get; set; }

        public OperationCommand RenameCommand { get; set; }


        public OperationCommand SendToServerCommand { get; set; }


        public OptimizerResultsViewModel()
        {
            Name = "Optimizer results";
            OptimizerResults= new ObservableCollection<OptimizerResultViewModel>();
            OptimizerResultsView = CollectionViewSource.GetDefaultView(OptimizerResults);
           

            OpenCommand = new OperationCommand(OpenOptimizerResults);
            RemoveCommand = new OperationCommand(RemoveOptimizerResults);
            RenameCommand = new OperationCommand(RenameOptimizerResults);
            
        }

        

        private async void RemoveOptimizerResults(object obj)
        {
            if (SelectedOptimizerResult != null)
            {
                var pathToDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Twm" +
                                "\\OptimizerResults";
                var pathToFiles = Path.Combine(pathToDir, SelectedOptimizerResult.Guid);

                if (Directory.Exists(pathToFiles))
                    Directory.Delete(pathToFiles, true);

                using (var context = Session.Instance.DbContextFactory.GetContext())
                {
                    var repository = new OptimizerResultRepository(context);
                    repository.Remove(SelectedOptimizerResult.DataModel); 
                    await repository.CompleteAsync();
                }

                OptimizerResults.Remove(SelectedOptimizerResult);

                SelectedOptimizerResult = OptimizerResults.FirstOrDefault();
            }
        }

        private void OpenOptimizerResults(object obj)
        {
            if (SelectedOptimizerResult != null)
            {
                var optimizerViewModel = new OptimizerViewModel(SelectedOptimizerResult);
                var optimizerWindow = new OptimizerWindow(optimizerViewModel);
                optimizerWindow.Show();
            }
        }


        private async void RenameOptimizerResults(object obj)
        {
            if (SelectedOptimizerResult != null)
            {
                var name = SelectedOptimizerResult.Name;
                var optimizerResultNameWindow = new PresetNameWindow(SelectedOptimizerResult){Title = "Optimizer result rename"};

                if (optimizerResultNameWindow.ShowDialog() == true)
                {
                    using (var context = Session.Instance.DbContextFactory.GetContext())
                    {
                        var repository = new OptimizerResultRepository(context);
                        await repository.Update(SelectedOptimizerResult.DataModel);
                        await repository.CompleteAsync();
                    }
                }
                else
                {
                    SelectedOptimizerResult.Name = name;
                }

            }
        }

        private bool OptimizerResultsFilter(object item)
        {
            var or = item as OptimizerResultViewModel;
            if (or == null)
                return false;


            if (SelectedSymbol != "All" && (or.Symbol.ToUpper() != SelectedSymbol.ToUpper()))
            {
                return false;
            }

            if (SelectedStrategy != "All" && (or.StrategyName.ToUpper() != SelectedStrategy.ToUpper()))
            {
                return false;
            }

            if (SelectedVersion != "All" && (or.Version.ToUpper() != SelectedVersion.ToUpper()))
            {
                return false;
            }


            if (SelectedTimeFrame != "All" && (or.TimeFrame.ToUpper() != SelectedTimeFrame.ToUpper()))
            {
                return false;
            }


            return true;
        }

        public override async void FetchData()
        {
            if (IsLoaded)
                return;

            OptimizerResults.Clear();

            using (var context = Session.Instance.DbContextFactory.GetContext())
            {
                var repository = new OptimizerResultRepository(context);
                var optimizerResults = repository.GetAll().Result.OrderByDescending(x=>x.DateCreated);
                foreach (var optimizerResult in optimizerResults)
                {
                    var optimizerResultViewModel = new OptimizerResultViewModel(optimizerResult);
                    OptimizerResults.Add(optimizerResultViewModel);
                }
                SelectedOptimizerResult = OptimizerResults.FirstOrDefault();
            }

            var symbolGroups = OptimizerResults.GroupBy(x => x.Symbol);
            Symbols = new List<string>() { "All" };
            Symbols.AddRange(symbolGroups.Select(x=>x.Key));
            _selectedSymbol = "All";


            var strategyGroups = OptimizerResults.GroupBy(x => x.StrategyName);
            Strategies = new List<string>() { "All" };
            Strategies.AddRange(strategyGroups.Select(x => x.Key));
            _selectedStrategy = "All";


            var versionGroups = OptimizerResults.GroupBy(x => x.Version);
            Versions = new List<string>() { "All" };
            Versions.AddRange(versionGroups.Select(x => x.Key));
            _selectedVersion = "All";

            var timeFrameGroups = OptimizerResults.GroupBy(x => x.TimeFrame);
            TimeFrames = new List<string>() { "All" };
            TimeFrames.AddRange(timeFrameGroups.Select(x => x.Key));
            _selectedTimeFrame = "All";

            OptimizerResultsView.Filter += OptimizerResultsFilter;
            IsLoaded = true;
        }
    }
}