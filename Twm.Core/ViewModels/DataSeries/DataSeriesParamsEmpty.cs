using System;
using System.Runtime.Serialization;
using Twm.Core.Classes;
using Twm.Core.Enums;
using Twm.Core.Interfaces;
using Twm.Model.Model;

namespace Twm.Core.ViewModels.DataSeries
{
    [DataContract]
    public class DataSeriesParamsEmpty : DataSeriesParams
    {

        public override Instrument Instrument
        {
            get { return base.Instrument; }
            set
            {
                if (base.Instrument != value)
                {
                    base.Instrument = value;

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
                }
            }
        }


        [DataMember]
        public string Symbol
        {
            get { return Instrument == null ? "" : Instrument.Symbol; }
            set
            {
                if (Instrument != null)
                {
                    if (Instrument.Symbol == value)
                        return;
                }

               
            }
        }


        [DataMember]
        public override int DataSeriesValue
        {
            get { return base.DataSeriesValue; }
            set
            {
                if (value != base.DataSeriesValue )
                {
                    base.DataSeriesValue  = value;
                }
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
                }
            }
        }

       

        
       


        

        public DataSeriesParamsEmpty()
        {
            base.DataSeriesType = DataSeriesType.Minute;

            var playback = Session.Instance.Playback;

            if (playback!= null && playback.IsConnected)
            {
                base.PeriodEnd = playback.PeriodStart;
            }
            else
                base.PeriodEnd = DateTime.Now;


            base.DataSeriesValue = 1;
        }
    }
}