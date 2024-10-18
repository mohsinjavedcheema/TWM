using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Data;
using System.Windows.Input;
using Twm.Chart.Interfaces;
using Twm.Core.Classes;
using Twm.Core.Controllers;
using Twm.Core.DataProviders.Common;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.Enums;
using Twm.Core.Interfaces;
using Twm.Core.UI.Windows;
using Twm.Core.ViewModels;
using Twm.Model.Model;

namespace Twm.Core.DataProviders.Playback
{
    [DataContract]
    public class Playback : ConnectionBase, IDataProvider
    {
        private readonly Dictionary<int, int> _speedDictionary = new Dictionary<int, int>()
        {
            {0, 1},
            {1, 2},
            {2, 3},
            {3, 4},
            {4, 6},
            {5, 8},
            {6, 10},
            {7, 15},
            {8, 20},
            {9, 30},
            {10, 50},
            {11, 100},
            {12, 500},
            {13, 1000}
        };



        private IDataProviderClient _client;

        public override IDataProviderClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new PlaybackClient();
                }

                return _client;
            }
        }

        private DateTime _periodStart;
        [DataMember]
        public DateTime PeriodStart
        {
            get { return _periodStart; }
            set
            {
                if (_periodStart != value)
                {
                    _periodStart = value;

                    PeriodStartTick = _periodStart.Ticks;
                    CurrentTick = PeriodStartTick;
                    OnPropertyChanged();
                }
            }
        }


        private double _periodStartTick;
        public double PeriodStartTick
        {
            get { return _periodStartTick; }
            set
            {
                if (_periodStartTick != value)
                {
                    _periodStartTick = value;
                    OnPropertyChanged();
                }
            }
        }


        private DateTime _periodEnd;
        [DataMember]
        public DateTime PeriodEnd
        {
            get { return _periodEnd; }
            set
            {
                if (_periodEnd != value)
                {
                    _periodEnd = value;
                    PeriodEndTick = _periodEnd.Ticks;
                    OnPropertyChanged();
                }
            }
        }


        private double _periodEndTick;
        public double PeriodEndTick
        {
            get { return _periodEndTick; }
            set
            {
                if (_periodEndTick != value)
                {
                    _periodEndTick = value;
                    OnPropertyChanged();
                }
            }
        }


        private DateTime _currentDate;
        public DateTime CurrentDate
        {
            get { return _currentDate; }
            set
            {
                if (_currentDate != value)
                {
                    if (value < PeriodStart)
                    {
                        _currentDate = PeriodStart;
                    }
                    else if (value > PeriodEnd)
                    {
                        _currentDate = PeriodEnd;
                    }
                    else
                        _currentDate = value;
                    OnPropertyChanged();
                }
            }
        }



        private double _currentTick;
        public double CurrentTick
        {
            get { return _currentTick; }
            set
            {
                if (_currentTick != value)
                {
                    _currentTick = value;
                    CurrentDate = new DateTime((long)value);
                    /*if (!IsPlay)
                    {
                        _prevDate = DateTime.MinValue;
                    }*/
                    
                    OnPropertyChanged();
                }
            }
        }




        private int _currentSpeedIndex;
        public int CurrentSpeedIndex
        {
            get { return _currentSpeedIndex; }
            set
            {
                if (_currentSpeedIndex != value && value >= 0 && value <= 13)
                {
                    _currentSpeedIndex = value;
                    CurrentSpeed = _speedDictionary[_currentSpeedIndex];
                    OnPropertyChanged();
                }
            }
        }

        private int _currentSpeed;

        public int CurrentSpeed
        {
            get { return _currentSpeed; }
            set
            {
                if (_currentSpeed != value)
                {
                    _currentSpeed = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _isPlay;

        public bool IsPlay
        {
            get { return _isPlay; }
            set
            {
                if (_isPlay != value)
                {
                    _isPlay = value;
                    OnPropertyChanged();
                }
            }
        }

        public override bool IsReadonly
        {
            get { return true; }
        }

       


        private readonly Timer _timer;

        public ICommand PlayStopCommand { get; set; }
        public ICommand IncrementSpeedCommand { get; set; }
        public ICommand DecrementSpeedCommand { get; set; }
        public ICommand ResetCommand { get; set; }


        public ICommand GoToDateCommand { get; set; }
        



        private readonly object _tickLockObject = new object();


        private DateTime _prevDate;


        public event EventHandler<PlaybackEventArgs> OnTimerElapsed;

        public event EventHandler<PlaybackEventArgs> OnReset;


        public Dictionary<string,List<ICandle>> Ticks { get; set; }

        

        public string Symbol { get; set; }

        private PlaybackWindow _playBackWindow;

        public Playback()
        {
            Id = -1;
            Ticks = new Dictionary<string, List<ICandle>>();
            
            PeriodEnd = DateTime.Today;
            PeriodStart = PeriodEnd.AddDays(-7);
            PlayStopCommand = new OperationCommand(PlayStop);
            IncrementSpeedCommand = new OperationCommand(IncrementSpeed);
            DecrementSpeedCommand = new OperationCommand(DecrementSpeed);
            ResetCommand = new OperationCommand(Reset);

            GoToDateCommand = new OperationCommand(GoToDate);

            

            CurrentSpeedIndex = 0;
            CurrentSpeed = 1;
            CurrentTick = PeriodStartTick;
            _timer = new Timer {Interval = 1000, AutoReset = true};
            _timer.Elapsed += _timer_Elapsed;
            _prevDate = DateTime.MinValue;
        }

        private void GoToDate(object obj)
        {
            var goToDateWindow = new GoToDateWindow(this);
            if (goToDateWindow.ShowDialog() == true)
            {
                _currentTick = CurrentDate.Ticks;
                OnPropertyChanged("CurrentTick");
                Reset(null);
            }

        }

        public void Reset(object obj)
        {
            if (!IsPlay)
            {
                _prevDate = DateTime.MinValue;

                if (!Ticks.Any())
                    return;

                var ticks = GetCurrentTicks(Symbol);
                
                OnReset?.Invoke(this, new PlaybackEventArgs() { Ticks = ticks, Symbol = Symbol });

            }
        }

        public IEnumerable<ICandle> GetCurrentTicks(string symbol)
        {
            List<ICandle> ticks = new List<ICandle>();
            if (Ticks.TryGetValue(symbol, out var candles))
            {
                ticks = candles.Where(x => x.ct <= CurrentDate).ToList();
            }
            return ticks;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CurrentTick += TimeSpan.FromMilliseconds(CurrentSpeed * 1000).Ticks;

            lock (_tickLockObject)
            {
                
                foreach (var extraTick in Ticks)
                {
                    var ticks = extraTick.Value.Where(x => x.ct <= CurrentDate && x.ct > _prevDate).ToList();
                    if (ticks.Any())
                    {
                        OnTimerElapsed?.Invoke(this, new PlaybackEventArgs() { Ticks = ticks, Symbol = extraTick.Key });
                    }
                }
                _prevDate = CurrentDate;
            }
        }

        private void DecrementSpeed(object obj)
        {
            CurrentSpeedIndex--;
        }

        private void IncrementSpeed(object obj)
        {
            CurrentSpeedIndex++;
        }

        private void PlayStop(object parameter)
        {
            if (parameter is bool value)
            {
                IsPlay = value;
            }
            else
                IsPlay = !IsPlay;

            if (IsPlay)
            {
                _timer.Start();
            }
            else
            {
                _timer.Stop();
            }
        }


        public override void Connect()
        {
            try
            {
                Session.Instance.ConnectionStatusChanged(ConnectionStatus.Disconnected,
                    ConnectionStatus.Connecting, Id);

                IsConnected = true;
                
                LogController.Print("Playback successfully connected!");

                _playBackWindow = new PlaybackWindow(this);
                _playBackWindow.Show();
            }
            catch (Exception ex)
            {
                Session.Instance.ConnectionStatusChanged(ConnectionStatus.Connecting,
                    ConnectionStatus.Disconnected, Id);

                OnPropertyChanged("IsConnected");
                LogController.Print($"Playback not connected: {ex.Message}");
            }
        }

        public override void Disconnect()
        {
            Session.Instance.SaveSettingObject("Playback", this);

            IsConnected = false;
            IsPlay = false;
            Ticks.Clear();
            _playBackWindow?.Close();
          
            
        }

        public override IInstrumentManager CreateInstrumentManager()
        {
            return null;
        }
    

    
        public Task<IEnumerable<object>> GetInstruments()
        {
            return Task.FromResult(Enumerable.Empty<object>());
        }

        public Task<IEnumerable<object>> FindInstruments(IRequest request)
        {
            return Task.FromResult(Enumerable.Empty<object>());
        }

        public Task<IEnumerable<IHistoricalCandle>> GetHistoricalData(IRequest request)
        {
            return Task.FromResult(Enumerable.Empty<IHistoricalCandle>());
        }



        public DataSeriesParams GetParams(DataSeriesParams dataSeriesParams)
        {
            var playbackParams = dataSeriesParams;
            playbackParams.DataSeriesType = DataSeriesType.Tick;
            playbackParams.DataSeriesValue = 1;
            playbackParams.PeriodEnd = PeriodEnd;
            playbackParams.PeriodStart = PeriodStart;

            return playbackParams;
        }

        public Task<IEnumerable<object>> GetTickers()
        {
            throw new NotImplementedException();
        }

        public override List<DataSeriesValue> GetDataFormats()
        {
            throw new NotImplementedException();
        }
    }

    public class PlaybackEventArgs : EventArgs
    {
        public string Symbol { get; set; }
        public IEnumerable<ICandle> Ticks { get; set; }
    }
}