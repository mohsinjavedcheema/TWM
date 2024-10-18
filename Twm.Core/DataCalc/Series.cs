using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Twm.Chart.Interfaces;
using Twm.Core.Attributes;
using Twm.Core.Enums;


namespace Twm.Core.DataCalc
{
    public class Series<T> : DataCalcObject, ISeries<T>, ICloneable
    {
        [Browsable(false)] public string Name { get; set; }

        [Browsable(false)]
        public string DisplayName
        {
            get { return ToString(); }
        }

        private readonly object _suspendLock = new object();

        [Browsable(false)] public bool IsExtraSeries { get; set; }
        [Browsable(false)] public int ExtraSeriesIndex { get; set; }


        protected CandleProperty _candleProperty;

        private PriceType _priceType;

       

        [Display(Name = "Price type", GroupName = "Parameters", Order = 1)]
        [ExcludePriceType(PriceType.Unset)]
        public PriceType PriceType
        {
            get { return _priceType; }

            set
            {
                if (_priceType != value)
                {
                    switch (value)
                    {
                        case PriceType.Open:
                            _candleProperty = CandleProperty.Open;
                            break;

                        case PriceType.High:
                            _candleProperty = CandleProperty.High;
                            break;

                        case PriceType.Low:
                            _candleProperty = CandleProperty.Low;
                            break;

                        case PriceType.Close:
                            _candleProperty = CandleProperty.Close;
                            break;
                    }

                    _priceType = value;
                    OnPropertyChanged("DisplayName");
                }
            }
        }


        [Browsable(false)]
        public int Count
        {
            get
            {
                if (_data != null)
                    return _data.Length;

                return 0;
            }
        }


        private T[] _data;
        private bool[] _validation;
        private readonly int _resizeCount = 100;

        private int _lastValidPointIndex = -1;

        private readonly SemaphoreSlim _suspendSemaphore = new SemaphoreSlim(1, 1);

        public Series()
        {
            _data = new T[0];
            _validation = new bool[0];
            _candleProperty = CandleProperty.Unset;
        }

        public Series(DataCalcContext dataCalcContext, CandleProperty candleProperty,
            int extraSeriesIndex = -1) : this()
        {
            DataCalcContext = dataCalcContext;
            _candleProperty = candleProperty;
            if (!Enum.TryParse(_candleProperty.ToString(), out _priceType))
            {
                _priceType = PriceType.Close;
            }


            if (extraSeriesIndex == -1)
            {
                if (DataCalcContext?.Candles != null)
                    _data = new T[DataCalcContext.Candles.Count];
            }
            else
            {
                if (DataCalcContext.ExtraDataSeries[extraSeriesIndex]?.Candles != null)
                    _data = new T[DataCalcContext.ExtraDataSeries[extraSeriesIndex].Candles.Count];
            }
        }

        public T GetValueAt(int index)
        {
            try
            {
                return _data[index];
            }
            catch
            {
                return default;
            }
        }

        public T[] GetRange(int startIndex, int count)
        {
            return _data.Skip(startIndex).Take(count).Select(x => x == null ? default : x).ToArray();
        }


        public virtual T this[int key]
        {
            get
            {
                var index = DataCalcContext.CurrentBar - 1 - key;
                
                ResizeArrays(index);

                if (!IsValidPoint(index))
                {
                    if (_lastValidPointIndex == index - 1)
                    {
                        //if (ParentIndicator.LastBarIndex != CurrentBar)
                        {
                            ParentIndicator?.OnBarUpdate();
                            ParentIndicator?.OnAfterBarUpdate();
                        }
                    }
                    else
                    {
                        if (ParentIndicator != null)
                            DataCalcContext.CalculateObject(ParentIndicator, _lastValidPointIndex + 2);
                    }
                    //WaitContext(index);
                }

                return _data[index];
            }
            set
            {
                var index = DataCalcContext.CurrentBar - 1 - key;

                ResizeArrays(index);

                _data[index] = value;
                _validation[index] = true;
                _lastValidPointIndex = index;
                //SetValid(key);
            }
        }

        public void Clear()
        {
            _data = null;
            _validation = null;
            Parent = null;
            DataCalcContext = null;
        }

        public bool IsValidPoint(int index)
        {
            try
            {
                return _validation[index];
            }
            catch
            {
                return false;
            }
        }

        private void ResizeArrays(int index)
        {
            if (index >= _data.Length - _resizeCount)
            {
                //_suspendSemaphore.Wait(DataCalcContext.CancellationTokenSourceCalc.Token);
                try
                {
                  //  if (index >= _data.Length - _resizeCount)
                    {
                        Array.Resize(ref _data, _data.Length + _resizeCount);
                        Array.Resize(ref _validation, _data.Length + _resizeCount);
                    }
                }
                finally
                {
                  //  _suspendSemaphore.Release(1);
                }
            }
        }

        public void SetValid(int key)
        {
            var index = DataCalcContext.CurrentBar - 1 - key;
            ResizeArrays(index);


            /*if (!_validation[index])
            {
                lock (_validation)
                {
                    _validation[index] = true;
                    Monitor.PulseAll(_validation);
                }
            }*/
            _validation[index] = true;

        }


        private void WaitContext(int index)
        {
            if (DataCalcContext.IsCalculatingState() && DataCalcContext.CalcMode == CalcMode.Async)
            {
                //Значение еще не валидно, но обращение к серии идет из другого индикатора
                if (Parent.ExecutionTaskId != Task.CurrentId)
                {
                    if (!DataCalcContext.IsCancelCalcRequest && !IsValidPoint(index))
                    {
                        /*lock (_validation)
                            while (!_validation[index])
                            {
                                Monitor.Wait(_validation);
                            }*/

                        SpinWait.SpinUntil(() => { return _validation[index]; });
                    }
                }
            }
        }

        public void Reset()
        {
            if (DataCalcContext.Candles == null)
                return;
            _data = new T[DataCalcContext.Candles.Count];
            _validation = new bool[DataCalcContext.Candles.Count];
        }

        public override void SetDataCalcContext(DataCalcContext dataCalcContext)
        {
            DataCalcContext = dataCalcContext;
            Reset();
            State = DataCalcContext.State;
        }


        public override string ToString()
        {
            if (_candleProperty == CandleProperty.Unset)
                return base.ToString();

            if (DataCalcContext == null || string.IsNullOrEmpty(DataCalcContext.Name))
                return _candleProperty.ToString();

            if (!IsExtraSeries)
            {
                return DataCalcContext.Name + " " + _candleProperty;
            }
            else
            {
                return DataCalcContext.ExtraDataSeries[ExtraSeriesIndex].DataSeriesName + " " + _candleProperty;
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        protected int GetExtraSeriesCandleIndex(int index)
        {
            var curTime = DataCalcContext.Candles[DataCalcContext.CurrentBar - index - 1].ct;
            var candleIndex = DataCalcContext.ExtraDataSeries[ExtraSeriesIndex].LastIndex;

            var i = candleIndex;
            if (i == -1)
                i = 0;

            bool isExist = false;
            var length = DataCalcContext.ExtraDataSeries[ExtraSeriesIndex].Candles.Count;

            var isLess = true;
            while (DataCalcContext.ExtraDataSeries[ExtraSeriesIndex].Candles[i].ct <= curTime)
            {
                if (DataCalcContext.ExtraDataSeries[ExtraSeriesIndex].Candles[i].ct.ToUniversalTime().Date ==
                    curTime.ToUniversalTime().Date)
                {
                    isLess = false;
                }

                i++;
                isExist = true;

                if (i >= length)
                    break;
            }


            if (isExist)
            {
                candleIndex = i - 1;
                if (candleIndex < 0)
                {
                    return -1;
                    //throw new IndexOutOfRangeException("Extra data series index: " + candleIndex);
                }

                DataCalcContext.ExtraDataSeries[ExtraSeriesIndex].LastIndex = candleIndex;
            }
            else
            {
                return -1;
                //throw new IndexOutOfRangeException("Extra data series index: " + candleIndex);
            }

            if (DataCalcContext.ExtraDataSeries[ExtraSeriesIndex].DataSeriesType == DataSeriesType.Day ||
                DataCalcContext.ExtraDataSeries[ExtraSeriesIndex].DataSeriesType == DataSeriesType.Month)
            {
                if (!isLess)
                {
                    if (candleIndex - 1 >= 0)
                    {
                        candleIndex--;
                    }
                    else
                    {
                        return -1;
                        //throw new IndexOutOfRangeException("Extra data series index: " + candleIndex);
                    }
                }
            }

            return candleIndex;
        }
    }
}