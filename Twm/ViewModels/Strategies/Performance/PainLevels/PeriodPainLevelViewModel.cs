using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Twm.Core.Controllers;
using Twm.Core.DataCalc.Performance;
using Twm.Core.Enums;
using Twm.Core.Interfaces;
using Twm.Core.ViewModels;

namespace Twm.ViewModels.Strategies.Performance.RiskLevels
{
    public class PeriodRiskLevelViewModel : ViewModelBase
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }


        private int? _osTrades;

        public int? OSTrades
        {
            get { return _osTrades; }
            set
            {
                if (value != _osTrades)
                {
                    _osTrades = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _simTrades;

        public int? SimTrades
        {
            get { return _simTrades; }
            set
            {
                if (value != _simTrades)
                {
                    _simTrades = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _painTrades;

        public int PainTrades
        {
            get { return _painTrades; }
            set
            {
                if (value != _painTrades)
                {
                    _painTrades = value;
                    OnPropertyChanged();
                }
            }
        }


        private int? _globalPainTrades;

        public int? GlobalPainTrades
        {
            get { return _globalPainTrades; }
            set
            {
                if (value != _globalPainTrades)
                {
                    _globalPainTrades = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _isCheck;

        public bool IsCheck
        {
            get { return _isCheck; }
            set
            {
                if (value != _isCheck)
                {
                    _isCheck = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _isGlobalCheck;

        public bool IsGlobalCheck
        {
            get { return _isGlobalCheck; }
            set
            {
                if (value != _isGlobalCheck)
                {
                    _isGlobalCheck = value;
                    OnPropertyChanged();
                }
            }
        }


        public Dictionary<AnalyticalFeature, double?> ISValues { get; set; }

        public ConcurrentDictionary<AnalyticalFeature, double?> OSValues { get; set; }

        public ConcurrentDictionary<AnalyticalFeature, double?> SimValues { get; set; }

        public ConcurrentDictionary<AnalyticalFeature, bool?> IsOSChecks { get; set; }

        public ConcurrentDictionary<AnalyticalFeature, bool?> IsSimChecks { get; set; }

        public Dictionary<AnalyticalFeature, IRiskLevel> _osRiskLevels;
        public Dictionary<AnalyticalFeature, IRiskLevel> _simRiskLevels;


        public PeriodRiskLevelViewModel()
        {
            ISValues = new Dictionary<AnalyticalFeature, double?>();
            OSValues = new ConcurrentDictionary<AnalyticalFeature, double?>();
            SimValues = new ConcurrentDictionary<AnalyticalFeature, double?>();
            IsOSChecks = new ConcurrentDictionary<AnalyticalFeature, bool?>();
            IsSimChecks = new ConcurrentDictionary<AnalyticalFeature, bool?>();
        }


        public void InitValues(List<IRiskLevel> allRiskLevels, List<IRiskLevel> osRiskLevels,
            List<IRiskLevel> simRiskLevels, Summary summary)
        {
            _osRiskLevels = allRiskLevels.ToDictionary(x => x.Feature, y => y);
            _simRiskLevels = allRiskLevels.ToDictionary(x => x.Feature, y => y);

            foreach (var painLevel in allRiskLevels)
            {
                if (painLevel.IsChecked)
                {
                    ISValues.Add(painLevel.Feature, summary.GetValue(painLevel.Feature));
                    _osRiskLevels[painLevel.Feature] = osRiskLevels.Find(x => x.Feature == painLevel.Feature);
                    _simRiskLevels[painLevel.Feature] = simRiskLevels.Find(x => x.Feature == painLevel.Feature);
                }
                else
                    ISValues.Add(painLevel.Feature, null);

                OSValues.TryAdd(painLevel.Feature, null);
                SimValues.TryAdd(painLevel.Feature, null);
                IsOSChecks.TryAdd(painLevel.Feature, null);
                IsSimChecks.TryAdd(painLevel.Feature, null);
            }
        }

        public void SetOSValues(int osTradeCount, Summary summary)
        {
            OSTrades = osTradeCount;
            PainTrades = summary.GetValue<int>(AnalyticalFeature.Trades);
            if (!SystemOptions.Instance.CalculateSimulation)
                IsCheck = OSTrades == PainTrades;

            var features = OSValues.Keys.ToList();

            foreach (var feature in features)
            {
                if (ISValues[feature] != null)
                {
                    OSValues[feature] = summary.GetValue(feature);
                    IsOSChecks[feature] = !_osRiskLevels[feature].IsFalse;
                }
            }

            OnPropertyChanged("OSValues");
            OnPropertyChanged("IsOSChecks");
        }

        public void SetSimValues(int simTradeCount, Summary summary)
        {
            SimTrades = simTradeCount;
            PainTrades = summary.GetValue<int>(AnalyticalFeature.Trades);
            IsCheck = SimTrades == PainTrades;

            var features = SimValues.Keys.ToList();

            foreach (var feature in features)
            {
                if (ISValues[feature] != null)
                {
                    SimValues[feature] = summary.GetValue(feature);
                    IsSimChecks[feature] = !_simRiskLevels[feature].IsFalse;
                }
            }

            OnPropertyChanged("SimValues");
            OnPropertyChanged("IsSimChecks");
        }


        public void SetValues(int osTradeCount, Summary osSummary, int simTradeCount, Summary simSummary)
        {
            var features = OSValues.Keys.ToList();
            foreach (var feature in features)
            {
                if (ISValues[feature] != null)
                {
                    OSValues[feature] = osSummary.GetValue(feature);
                    IsOSChecks[feature] = !_osRiskLevels[feature].IsFalse;

                    if ((IsOSChecks[feature]??false) && SystemOptions.Instance.CalculateSimulation)
                    {
                        SimValues[feature] = simSummary.GetValue(feature);
                        IsSimChecks[feature] = !_simRiskLevels[feature].IsFalse;
                    }

                }
            }

            OSTrades = osTradeCount;
            PainTrades = osSummary.GetValue<int>(AnalyticalFeature.Trades);
            IsCheck = !IsOSChecks.Values.Contains(false);

            if (SystemOptions.Instance.CalculateSimulation)
            {
                if (IsCheck)
                {
                    SimTrades = simTradeCount;
                    PainTrades = simSummary.GetValue<int>(AnalyticalFeature.Trades);
                    IsCheck = !IsSimChecks.Values.Contains(false);
                }
            }



            OnPropertyChanged("OSValues");
            OnPropertyChanged("IsOSChecks");

            OnPropertyChanged("SimValues");
            OnPropertyChanged("IsSimChecks");
        }
    }
}