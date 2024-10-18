using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using Twm.Core.Classes;
using Twm.Core.Enums;
using Twm.Core.Helpers;
using Twm.Model.Model;

namespace Twm.Core.Interfaces
{
    public abstract class DataSeriesParams
    {
        public virtual Instrument Instrument { get; set; }
        public virtual int DataSeriesValue { get; set; }

        public virtual DataSeriesValue DataSeriesFormat { get; set; }
        public virtual DataSeriesType DataSeriesType { get; set; }
        public virtual DateTime PeriodStart { get; set; }
        public virtual DateTime PeriodEnd { get; set; }

        public virtual ObservableCollection<DataSeriesValue> DataSeriesFormats { get; set; }


        public int DataSeriesLength { get; set; }

        public int GetLengthInMinutes()
        {
            switch (DataSeriesType)
            {
                case DataSeriesType.Minute:
                    return DataSeriesValue;
                case DataSeriesType.Hour:
                    return DataSeriesValue * 60;
                case DataSeriesType.Day:
                    return DataSeriesValue * 60 * 24;
                case DataSeriesType.Week:
                    return DataSeriesValue * 60 * 24 * 7;
                case DataSeriesType.Month:
                    return DataSeriesValue * 60 * 24 * 30;
            }

            return 0;
        }

        public string DataSeries
        {
            get { return (DataSeriesValue == 1 ? "" : DataSeriesValue.ToString()) + DataSeriesType.ToAbbr(); }
        }

        public string SymbolDataSeries
        {
            get
            {
                if (Instrument == null)
                    return "";

                return Instrument.Symbol + "{=" + DataSeries + "}";
            }
        }


        


        public string DataSeriesName
        {
            get
            {
                if (Instrument == null)
                    return "";

                return Instrument.Symbol + "(" + DataSeriesValue + " " + DataSeriesType + ")" +"("+Instrument.Type+")";
            }
        }


        public string DataSeriesCode
        {
            get
            {
                if (Instrument == null)
                    return "";

                return Instrument.Symbol + "(" + DataSeriesValue + " " + DataSeriesType + ")"+PeriodStart.ToString("ddMMyyyy")+"-"+ PeriodEnd.ToString("ddMMyyyy");
            }
        }

    }
}