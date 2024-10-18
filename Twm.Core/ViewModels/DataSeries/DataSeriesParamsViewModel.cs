using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Twm.Chart.Annotations;
using Twm.Core.Classes;
using Twm.Core.DataProviders.Common;
using Twm.Core.Enums;
using Twm.Core.Interfaces;
using Twm.Core.ViewModels.Instruments;
using Twm.Model.Model;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Twm.Core.ViewModels.DataSeries
{
    [DataContract]
    public class DataSeriesParamsViewModel : DataSeriesParams, INotifyPropertyChanged, IDataErrorInfo, ICloneable
    {

        private readonly string[] _propertiesForValidation = { "Instrument", "DataSeriesFormat", /*"DataSeriesType", "DataSeriesValue",*/ "SelectedTimeFrameBase", "DaysToLoad", "PeriodStart", "PeriodEnd" };

      


        public override Instrument Instrument
        {
            get { return base.Instrument; }
            set
            {
                if (base.Instrument != value)
                {
                    base.Instrument = value;
                    OnPropertyChanged();
                    OnPropertyChanged("DisplayName");                    
                }
            }
        }

       /* public string Connection
        {
            get
            {
                if (Instrument == null || Instrument.ConnectionId == 0)
                    return "";

                Session.Instance.GetConnection(Instrument.ConnectionId);

                return Session.Instance.GetConnection(Instrument.ConnectionId).Name;
            }
        }*/

       


        [DataMember]
        public override DataSeriesValue DataSeriesFormat
        {
            get { return base.DataSeriesFormat; }
            set
            {
                if (value != base.DataSeriesFormat)
                {
                    base.DataSeriesFormat = value;
                    OnPropertyChanged();

                    if (value != null)
                    {
                        DataSeriesType = value.Type;
                        DataSeriesValue = value.Value;
                    }

                    OnPropertyChanged("DisplayName");

                }
            }
        }


        [DataMember]
        public override DataSeriesType DataSeriesType
        {
            get { return base.DataSeriesType; }
            set
            {
                if (value != base.DataSeriesType)
                {
                    base.DataSeriesType = value;
                    OnPropertyChanged();
                    OnPropertyChanged("DisplayName");
                    DataSeriesLength = GetLengthInMinutes();
                }
            }
        }


        [DataMember]
        public string Symbol
        {
            get { return Instrument == null ? "" : Instrument.Symbol; }
            set
            {
               /* if (Instrument != null)
                {
                    if (Instrument.Symbol == value)
                        return;
                }*/

                Instrument = Session.Instance.GetInstrument(value, SelectedType, SelectedConnection.Code).Result;

               
                
                OnPropertyChanged();
                OnPropertyChanged("Instrument");
            }
        }


        public string DisplayName
        {
            get
            {
                return string.Format("{0}({1}{2})", Instrument == null ? "" : Instrument.Symbol, DataSeriesValue,
                    DataSeriesType);
            }
        }


        [DataMember]
        public override int DataSeriesValue
        {
            get { return base.DataSeriesValue; }
            set
            {
                if (value != base.DataSeriesValue)
                {
                    base.DataSeriesValue = value;
                    OnPropertyChanged();
                    OnPropertyChanged("DisplayName");
                    DataSeriesLength = GetLengthInMinutes();
                }
            }
        }

        public bool HasErrors
        {
            get => _hasErrors;
            set
            {
                _hasErrors = value;
                OnPropertyChanged(nameof(HasErrors));
            }
        }

        private TimeFrameBase _selectedTimeFrameBase;
        [DataMember]
        public TimeFrameBase SelectedTimeFrameBase
        {
            get { return _selectedTimeFrameBase; }
            set
            {
                if (value != _selectedTimeFrameBase)
                {
                    _selectedTimeFrameBase = value;
                    OnPropertyChanged();
                }
            }
        }


        private int _daysToLoad;
        [DataMember]
        public int DaysToLoad
        {
            get { return _daysToLoad; }
            set
            {
                if (value != _daysToLoad)
                {
                    _daysToLoad = value;
                    OnPropertyChanged();
                    base.PeriodStart = PeriodEnd.AddDays(-_daysToLoad);
                    OnPropertyChanged("PeriodStart");
                }
            }
        }



        [DataMember]
        public override DateTime PeriodStart
        {
            get { return base.PeriodStart; }
            set
            {
                if (value != base.PeriodStart)
                {
                    base.PeriodStart = value;
                    OnPropertyChanged();
                    _daysToLoad = (int)(PeriodEnd - PeriodStart).TotalDays;
                    OnPropertyChanged("DaysToLoad");
                    OnPropertyChanged(nameof(PeriodEnd));
                }
            }
        }



        [DataMember]
        public override DateTime PeriodEnd
        {
            get { return base.PeriodEnd; }
            set
            {
                if (value != base.PeriodEnd)
                {
                    base.PeriodEnd = value;
                    OnPropertyChanged();

                    if (SelectedTimeFrameBase == TimeFrameBase.Days)
                    {
                        base.PeriodStart = PeriodEnd.AddDays(-_daysToLoad);
                        OnPropertyChanged("PeriodStart");
                    }
                    else
                    {
                        _daysToLoad = (int)(PeriodEnd - PeriodStart).TotalDays;
                        OnPropertyChanged("DaysToLoad");
                    }
                }
            }
        }



        public override ObservableCollection<DataSeriesValue> DataSeriesFormats
        {
            get { return base.DataSeriesFormats; }
            set
            {
                if (value != base.DataSeriesFormats)
                {
                    base.DataSeriesFormats = value;
                    OnPropertyChanged();

                }
            }
        }


        public ObservableCollection<ConnectionBase> Connections { get; set; }

        private ConnectionBase _selectedConnection;

        public ConnectionBase SelectedConnection
        {
            get { return _selectedConnection; }
            set
            {
                if (_selectedConnection != value)
                {
                    _selectedConnection = value;

                    if (_selectedConnection != null)
                    {
                        DataSeriesFormats.Clear();
                        var list = _selectedConnection.GetDataFormats();
                        foreach (var dataFormat in list)
                        {
                            DataSeriesFormats.Add(dataFormat);
                        }
                        DataSeriesFormat = DataSeriesFormats.FirstOrDefault();

                        if (Instrument != null && SelectedType != "")
                        {
                            Instrument = Session.Instance.GetConnectionInstrument(_selectedConnection.Id, Instrument.Symbol, SelectedType).Result;
                        }
                    }

                    OnPropertyChanged();
                    IsInstrumentEnabled = SelectedConnection != null && !string.IsNullOrEmpty(SelectedType);

                   

                    OnPropertyChanged("IsInstrumentEnabled");
                }
            }
        }


        public ObservableCollection<string> TypesItems { get; set; }



        private string _selectedType;
        public string SelectedType
        {
            get { return _selectedType; }
            set
            {
                if (_selectedType != value)
                {
                    _selectedType = value;

                    if (Instrument != null && _selectedType != "" && _selectedConnection != null)
                    {
                        Instrument = Session.Instance.GetConnectionInstrument(_selectedConnection.Id, Instrument.Symbol, _selectedType).Result;
                    }

                    OnPropertyChanged();
                    IsInstrumentEnabled = SelectedConnection != null && !string.IsNullOrEmpty(SelectedType);
                    OnPropertyChanged("IsInstrumentEnabled");
                }
            }
        }


        private bool _isInstrumentEnabled;
        public bool IsInstrumentEnabled
        {
            get { return _isInstrumentEnabled; }
            set
            {
                if (_isInstrumentEnabled != value)
                {
                    _isInstrumentEnabled = value;
                    OnPropertyChanged();
                }
            }

        }


        private Dictionary<string, Func<string>> _validators = new Dictionary<string, Func<string>>();
        private bool _hasErrors;


        public bool IsParamsValid
        {
            get { return string.IsNullOrEmpty(Error); }
        }

        public DataSeriesParamsViewModel()
        {
            base.DataSeriesFormats = new ObservableCollection<DataSeriesValue>();
            base.DataSeriesType = DataSeriesType.Minute;

            var playback = Session.Instance.Playback;

            if (playback != null && playback.IsConnected)
            {
                base.PeriodEnd = playback.PeriodStart;
            }
            else
                base.PeriodEnd = DateTime.Now;


            base.PeriodStart = PeriodEnd.AddDays(-5);

            DaysToLoad = 5;
            base.DataSeriesValue = 1;

            DataSeriesLength = GetLengthInMinutes();
            InitValidators();
            PropertyChanged += DataSeriesParamViewModel_PropertyChanged;
            IsInstrumentEnabled = false;
            FillConnections();
            TypesItems = new ObservableCollection<string>() { "FUTURE", "SPOT" };
            SelectedType = "FUTURE";

        }


        private void FillConnections()
        {
            var connections = Session.Instance.ConfiguredConnections;
            Connections = new ObservableCollection<ConnectionBase>();
            var orderedConnections = connections.Where(x => x.Id > 0).OrderBy(x => x.Id);
            foreach (var connection in orderedConnections)
            {
                Connections.Add(connection);
            }

            if (SelectedConnection != null)
            {
                SelectedConnection = Connections.FirstOrDefault(x => x.Id == SelectedConnection.Id);

            }
            else
            {
                var currentConnection = Session.Instance.Connections.FirstOrDefault(x => x.Value.IsConnected);
                if (currentConnection.Value != null)
                {
                    SelectedConnection = Connections.FirstOrDefault(x => x.Id == currentConnection.Key);
                }
                else
                {
                    SelectedConnection = Connections.FirstOrDefault();
                }

                //ConnectionId = SelectedConnection.Id;

            }

        }

        private void DataSeriesParamViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_propertiesForValidation.Contains(e.PropertyName))
            {
                OnPropertyChanged("IsParamsValid");
            }
        }

        private void InitValidators()
        {
            _validators = new Dictionary<string, Func<string>>();
            _validators.Add("DaysToLoad", () => ValidatePositiveInteger(DaysToLoad));

            _validators.Add("Instrument.Symbol", () =>
            {
                if (Instrument == null || string.IsNullOrEmpty(Instrument.Symbol))
                {
                    return "Should select a instrument";
                }

                return null;
            });

            _validators.Add("DataSeriesValue", () => ValidatePositiveInteger(DataSeriesValue));
            _validators.Add("PeriodEnd", ValidateCustomRange);
            _validators.Add("PeriodStart", ValidateCustomRange);
        }

        private string ValidateCustomRange()
        {
            if (SelectedTimeFrameBase != TimeFrameBase.CustomRange)
            {
                return null;
            }
            Debug.WriteLine(PeriodEnd.CompareTo(PeriodStart));
            return PeriodEnd.CompareTo(PeriodStart) <= 0 ? "The end date must be later than the start date" : null;
        }

        private string ValidatePositiveInteger(object value)
        {
            if (value == null)
            {
                return "Should be greater then zero";
            }
            var strValue = value.ToString();
            int intValue;
            if (int.TryParse(strValue, NumberStyles.Integer, CultureInfo.CurrentUICulture, out intValue))
            {
                if (intValue > 0)
                {
                    return null;
                }

                return "Should be the positive integer";
            }

            return "Should be greater then zero";
        }


        public void SelectSymbol(InstrumentViewModel instrument)
        {
            Instrument = instrument.DataModel;
        }

        public string Error
        {
            get
            {
                return string.Join("", _validators.Select(d => d.Value.Invoke()).Where(str => str != null));
            }
        }

        public string this[string name] => !_validators.ContainsKey(name) ? null : _validators[name].Invoke();
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}