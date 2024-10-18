using System;
using System.Collections.ObjectModel;
using System.Linq;
using Twm.Core.Classes;
using Twm.Core.ViewModels;

namespace Twm.ViewModels.Strategies.Optimizer
{
 
    public class OptimizerPeriodsEditViewModel : ViewModelBase
    {
        public ObservableCollection<OptimizerPeriodEditViewModel> Periods { get; set; }

        
        private OptimizerPeriodEditViewModel _selectedPeriod;

        public OptimizerPeriodEditViewModel SelectedPeriod
        {
            get { return _selectedPeriod; }
            set
            {
                if (value != _selectedPeriod)
                {
                    _selectedPeriod = value;
                    OnPropertyChanged();
                }
            }
        }


        public bool IsPeriodDeleteEnabled
        {
            get { return Periods.Count > 1; }
        }

        private int _windowHeight;
        public int WindowHeight
        {
            get
            {
                return _windowHeight;
            }

            set
            {
                if (_windowHeight != value)
                {
                    _windowHeight = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isLastPeriodLive;
        public bool IsLastPeriodLive
        {
            get
            {
                return _isLastPeriodLive;
            }

            set
            {
                if (_isLastPeriodLive != value)
                {
                    _isLastPeriodLive = value;
                    OnPropertyChanged();
                }
            }
        }

        public OperationCommand AddPeriodCommand { get; set; }
        public OperationCommand DeletePeriodCommand { get; set; }

        public OperationCommand SetPeriodLiveCommand { get; set; }

        public OptimizerPeriodsEditViewModel()
        {
            Periods = new ObservableCollection<OptimizerPeriodEditViewModel>();

            AddPeriodCommand = new OperationCommand(AddPeriod);
            DeletePeriodCommand = new OperationCommand(DeletePeriod);
            SetPeriodLiveCommand = new OperationCommand(SetPeriodLive);
        }

        private void SetPeriodLive(object obj)
        {
            var lastPeriod = Periods.LastOrDefault();
            if (IsLastPeriodLive)
            {
                var osPercent = lastPeriod.OSPercent;
                var daysShift = lastPeriod.OSDays;
                lastPeriod.OSEndDate = lastPeriod.OSEndDate.AddDays(daysShift);
                lastPeriod.ISStartDate = lastPeriod.ISStartDate.AddDays(daysShift);
                lastPeriod.OSStartDate = DateTime.Today;
                lastPeriod.OSPercent = osPercent;
                lastPeriod.IsLive = true;


                var periods = Periods.Where(x => x != lastPeriod).OrderByDescending(x => x.Number);

                foreach (var period in periods)
                {
                    period.OSEndDate = period.NextPeriod.ISEndDate;
                    period.TotalDays = period.NextPeriod.TotalDays;
                    period.OSPercent = period.NextPeriod.OSPercent;
                }

            }
            else
            {
                var totalDays = lastPeriod.TotalDays;
                var osPercent = lastPeriod.OSPercent;
                var daysShift = lastPeriod.OSDays;
                lastPeriod.OSEndDate = lastPeriod.OSStartDate;
                lastPeriod.OSStartDate = lastPeriod.OSEndDate.AddDays(-(daysShift));
                lastPeriod.ISStartDate = lastPeriod.OSEndDate.AddDays(-(totalDays+1));
                lastPeriod.OSPercent = osPercent;
                lastPeriod.IsLive = false;

                var periods = Periods.Where(x => x != lastPeriod).OrderByDescending(x => x.Number);

                foreach (var period in periods)
                {
                    period.OSEndDate = period.NextPeriod.ISEndDate;
                    period.TotalDays = period.NextPeriod.TotalDays;
                    period.OSPercent = period.NextPeriod.OSPercent;
                }

            }
        }


        public void UpdateWindowHeight()
        {
            /*var windowHeight = (int)(Periods.Count * 74) + 100;

            if (windowHeight > 800)
                windowHeight = 800;

            if (WindowHeight < windowHeight)
                WindowHeight = windowHeight;
*/



        }
        

        private void AddPeriod(object obj)
        {

            var nextPeriod = Periods.FirstOrDefault();

            var newPeriod = new OptimizerPeriodViewModel();
            if (nextPeriod != null)
            {
                newPeriod.OSEndDate = nextPeriod.ISEndDate;
                newPeriod.TotalDays = nextPeriod.TotalDays;
                newPeriod.OSPercent = nextPeriod.OSPercent;
            }
            else
            {

                var dateNow = DateTime.Now;
                newPeriod.OSEndDate = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day);
                newPeriod.ISStartDate = newPeriod.OSEndDate.AddMonths(-12);
                newPeriod.OSPercent = 30;
            }
            newPeriod.Number = 1;

            var newPeriodEdit = new OptimizerPeriodEditViewModel(newPeriod);

            newPeriodEdit.NextPeriod = nextPeriod;
            if (nextPeriod != null)
                nextPeriod.PrevPeriod = newPeriodEdit;

            foreach (var period in Periods)
            {
                period.Number++;
            }

            Periods.Insert(0, newPeriodEdit);

            UpdateWindowHeight();

            OnPropertyChanged("IsPeriodDeleteEnabled");
        }

        private void DeletePeriod(object obj)
        {
            if (Periods.Count > 1)
            {
                var firstPeriod = Periods.FirstOrDefault();

                if (firstPeriod != null)
                    firstPeriod.NextPeriod.PrevPeriod = null;

                Periods.RemoveAt(0);

                foreach (var period in Periods)
                {
                    period.Number--;
                }

                UpdateWindowHeight();
            }
            OnPropertyChanged("IsPeriodDeleteEnabled");
        }
    }
}