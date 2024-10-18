using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml.Serialization;
using Twm.Chart.Classes;
using Twm.Chart.Controls;
using Twm.Chart.Enums;
using Twm.Core.Attributes;
using Twm.Core.DataCalc;
using Twm.Core.Enums;
using Twm.Core.Market;
using Twm.Core.Tests.Enums;
using Twm.Custom.Indicators;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Twm.Custom.Strategies.TestStrategies
{
    [CategoryOrder(Global, 5)]
    [CategoryOrder(Entry, 6)]
    [CategoryOrder(Filters, 7)]
    [CategoryOrder(Exits, 8)]
    public class IndicatorValStrat : Strategy
    {
        private const string SystemVersion = " V1.0";
        private const string StrategyName = "IndicatorValStrat";
        private const string StrategyGuid = "72E9A605-62AD-4BF6-BB01-5FA47FCC8050";

        private const string Global = "Global";
        private const string Entry = "Entry";
        private const string Filters = "Filter";
        private const string Exits = "Exits";

        #region Parameters

        
        #endregion

        #region Init

        private Indicator _ema;
        private Indicator _sma;
        private Indicator _wma;
        private Indicator _tema;
        private Indicator _tma;
        private Indicator _hma;
        private Series<double> _macdDiff;

        List<double> _emaValues = new List<double>();
        List<double> _smaValues = new List<double>();
        List<double> _wmaValues = new List<double>();
        List<double> _temaValues = new List<double>();
        List<double> _tmaValues = new List<double>();
        List<double> _hmaValues = new List<double>();
        List<double> _macdDiffValues = new List<double>();

        List<List<double>> _values = new List<List<double>>();

        private int _barCount;

        public override void OnStateChanged()
        {
            if (State == State.SetDefaults)
            {
                SetDefaults();
                SetParameters();

            }
            else if (State == State.Configured)
            {
                _barCount = Close.Count - 1;

                _values.Add(_emaValues);
                _values.Add(_smaValues);
                _values.Add(_wmaValues);
                _values.Add(_temaValues);
                _values.Add(_tmaValues);
                _values.Add(_hmaValues);
                _values.Add(_macdDiffValues);
                

                InitializeMas();
            }
        }

        private void SetDefaults()
        {
            Name = StrategyName + SystemVersion;
            Guid = new Guid(StrategyGuid);
            Version = SystemVersion;
        }

        private void SetParameters()
        {

        }

        private void InitializeMas()
        {
            _sma = SMA(14);
            _ema = EMA(14);
            _wma = WMA(14);
            _tema = TEMA(14);
            _tma = TMA(14);
            _hma = HMA(14);

            var macd = MACD(12, 24, 3);
            _macdDiff = macd.Diff;

        }


        #endregion

        #region Business


        public override void OnBarUpdate()
        {
            _emaValues.Add(_ema[0]);
            //Tag = _emaValues;

            _smaValues.Add(_sma[0]);
            //Tag = _smaValues;

            _wmaValues.Add(_wma[0]);
            //Tag = _wmaValues;

            _temaValues.Add(_tema[0]);
            //Tag = _temaValues;

            _tmaValues.Add(_tma[0]);
            //Tag = _tmaValues;

            _hmaValues.Add(_hma[0]);
            //Tag = _hmaValues;

            _macdDiffValues.Add(_macdDiff[0]);
            //Tag = _macdDiffValues;

            if (CurrentBar == _barCount)
            {
                Tag = _values;
            }
        }

        #endregion

    }
}
