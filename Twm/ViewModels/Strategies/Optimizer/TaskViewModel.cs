using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Twm.Core.Classes;
using Twm.Core.ViewModels;
using Twm.Windows.Strategies;

namespace Twm.ViewModels.Strategies.Optimizer
{
    public class TaskViewModel:ViewModelBase
    {
        public int TaskId { get; set; }
        public ObservableCollection<Task> Tasks { get; set; }

        public Exception TaskException { get; set; }

        public string StackTrace { get; set; }

        public OptimizerPeriodViewModel OptimizerPeriod { get; set; }

        public Core.DataCalc.Optimization.Optimizer Optimizer { get; set; }

        public string ErrorMessage
        {
            get { return StackTrace.ToString(); }

        }


        public Dictionary<string, object> LastStrategyValues
        {
            get
            {
                if (Optimizer == null)
                    return null;
                return Optimizer.LastStrategy?.GetTwmPropertyValues();
            }
        }

        private string _status;
        public string Status
        {
            get
            {
                return _status;
            }

            set
            {
                if (_status != value)

                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
        }


        private string _action;
        public string Action
        {
            get
            {
                return _action;
            }

            set
            {
                if (_action != value)

                {
                    _action = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _actionInfo;
        public string ActionInfo
        {
            get
            {
                return _actionInfo;
            }

            set
            {
                if (_actionInfo != value)

                {
                    _actionInfo = value;
                    OnPropertyChanged();
                }
            }
        }


        private string _name;
        public string Name
        {
            get { return _name;}

            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }


        private long _allTasks;
        public long AllTasks
        {
            get { return _allTasks; }

            set
            {
                if (_allTasks != value)
                {
                    _allTasks = value;
                    OnPropertyChanged();
                }
            }
        }


        private int _endTasks;
        public int EndTasks
        {
            get { return _endTasks; }

            set
            {
                if (_endTasks != value)
                {
                    _endTasks = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _remainingTime;
        public string RemainingTime
        {
            get { return _remainingTime; }

            set
            {
                if (_remainingTime != value)
                {
                    _remainingTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public OperationCommand ShowCommand { get; set; }

        private bool _isOptimizationEnd;

        public void Subscribe()
        {
            OptimizerPeriod.OnCalculating += OptimizerPeriod_OnCalculating;
        }

        public void UnSubscribe()
        {
            OptimizerPeriod.OnCalculating -= OptimizerPeriod_OnCalculating;
            if (Tasks != null)
                Tasks.CollectionChanged -= Tasks_CollectionChanged;
            if (Optimizer != null)
                Optimizer.OnTaskCompleted -= Optimizer_OnTaskCompleted;
        }

        private void OptimizerPeriod_OnCalculating(object sender, Core.Messages.MessageEventArgs e)
        {
            if (e.Message == "Optimization")
            {
                if (OptimizerPeriod.PeriodStrategy.Optimizer.OptimizationTasks != null)
                {
                    Tasks = OptimizerPeriod.PeriodStrategy.Optimizer.OptimizationTasks;
                    Optimizer = OptimizerPeriod.PeriodStrategy.Optimizer;
                    AllTasks = Optimizer.CombinationCount;
                    Tasks.CollectionChanged += Tasks_CollectionChanged;
                    Optimizer.OnTaskCompleted += Optimizer_OnTaskCompleted;
                }
            }

            if (e.Message == "Optimization end")
            {
                _isOptimizationEnd = true;
                RemainingTime = "";
            }

           


            if (e.Status == "Canceled" || e.Status == "Error")
            {
                UnSubscribe();
                RemainingTime = "";
                ActionInfo = "";
            }


            /*var period = "";
            if (e.Tag == "DataLoad")
            {
                period = "IS: ";
                if (_isOptimizationEnd)
                    period = "OS: ";
            }*/

            if (Status != "Completed")
                Status = e.Status;

            Action = e.Message;
            ActionInfo = e.SubMessage;

            if (e.Status == "Completed")
            {
                UnSubscribe();
                ActionInfo = $"Optimized {AllTasks} combinations";
                RemainingTime = "";
            }
        }

        private void Optimizer_OnTaskCompleted(object sender, System.EventArgs e)
        {
            EndTasks++;
            ActionInfo = $"Completed tasks: {EndTasks}/{AllTasks}";
            RemainingTime = TimeSpan.FromMilliseconds((AllTasks - EndTasks) * Optimizer.AvgTaskTime).ToString(@"hh\:mm\:ss");
        }

        private void Tasks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // AllTasks = Tasks.Count;
            AllTasks = Optimizer.CombinationCount;
            ActionInfo = $"Completed tasks: {EndTasks}/{AllTasks}";

        }

        public TaskViewModel()
        {
            Status = "Not started";
            RemainingTime = "";

            ShowCommand = new OperationCommand(Show);
        }

        private void Show(object obj)
        {
            if (TaskException != null)
            {
                var errorWindow = new OptimizerErrorWindow(this);
                errorWindow.ShowDialog();
            }
        }
    }
}