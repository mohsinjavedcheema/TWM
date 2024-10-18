using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Twm.Chart.Interfaces;
using Twm.Core.Annotations;
using Twm.Core.Controllers;
using Twm.Core.Enums;

namespace Twm.Core.DataCalc
{
    public abstract class DataCalcObject : INotifyPropertyChanged
    {
        protected DataCalcContext DataCalcContext { get; set; }

        [Browsable(false)] public bool IsSuspend;

        [Browsable(false)] public bool IsOptimization { get; set; }

        private State[] _calculatingStates = { State.Historical, State.RealTime, State.Playback };

        [Browsable(false)]
        public int LastBarIndex { get; set; }

        private State _state;
        [Browsable(false)]
        public State State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged();
                }
            }

        }

        [Browsable(false)]
        public DataCalcObject Parent { get; set; }

        [Browsable(false)]
        public IndicatorBase ParentIndicator { get; set; }

        [Browsable(false)]
        public int? ExecutionTaskId { get; set; }


        [Browsable(false)]
        public int CurrentBar
        {
            get { return DataCalcContext.CurrentBar; }
        }

       

        /// <summary>
        /// Set new state
        /// </summary>
        public void SetState(State newState)
        {
            if (State != newState)
            {
                if (newState == State.Configured)
                {
                    BeforeConfigured();
                    State = State.Configured;
                    OnStateChanged();
                    AfterConfigured();
                }
                else if (_calculatingStates.Contains(newState))
                {
                    if (State == State.SetDefaults)
                    {
                        BeforeConfigured();
                        State = State.Configured;
                        OnStateChanged();
                        AfterConfigured();
                        State = newState;
                        DataCalcContext.CalculateObject(this);
                    }
                    else
                    {
                        State = newState;
                    }

                }
                else if (newState == State.Finalized)
                {
                    DataCalcContext = null;
                }
                else
                {
                    State = newState;
                    OnStateChanged();
                }
            }
        }

        /// <summary>
        /// Set state from parent DataCalcContext
        /// </summary>
        public void ChangeState()
        {
            if (DataCalcContext != null)
                SetState(DataCalcContext.State);
        }


        /// <summary>
        /// Clear all debug messages
        /// </summary>
        public virtual void ClearDebug()
        {
            DebugController.Clear();
        }


        /// <summary>
        /// Print message to debug
        /// </summary>
        /// <param name="message"></param>
        public virtual void Print(object message)
        {
            DebugController.Print(message.ToString());
        }


        /// <summary>
        /// Print message to log
        /// </summary>
        /// <param name="message"></param>
        public virtual void Log(object message)
        {
            LogController.Print(message.ToString());
        }


        public void SetParent(ScriptBase scriptBase)
        {
            Parent = scriptBase;
            if (scriptBase is IndicatorBase ib)
                ParentIndicator = ib;
        }

        public virtual void SetDataCalcContext(DataCalcContext dataCalcContext) { }

        public virtual void OnStateChanged() { }

        public virtual void OnBarUpdate() { }

        public virtual void OnTickUpdate(ICandle candle, ICandle tick) { }

        public virtual void OnAfterBarUpdate() { }

        public virtual void BeforeConfigured() { }

        public virtual void AfterConfigured() { }

        public virtual void OnConnectionStatusChanged(ConnectionStatus oldStatus, ConnectionStatus newStatus, string connectionName) { }

        public void UpdateProperty(string name)
        {
            OnPropertyChanged(name);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}