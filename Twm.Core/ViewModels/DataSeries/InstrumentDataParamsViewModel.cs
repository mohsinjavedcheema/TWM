namespace Twm.Core.ViewModels.DataSeries
{
    public class InstrumentDataParamsViewModel:ViewModelBase
    {
        public bool IsInstrumentParamValid
        {
            get { return string.IsNullOrEmpty(InstrumentSeriesParams.Error); }
        }


        private DataSeriesParamsViewModel _instrumentSeriesParams;

        public DataSeriesParamsViewModel InstrumentSeriesParams
        {
            get { return _instrumentSeriesParams; }

            set
            {
                if (value != _instrumentSeriesParams)
                {
                    _instrumentSeriesParams = value;
                    OnPropertyChanged();
                }
            }

        }

        public InstrumentDataParamsViewModel()
        {
            InstrumentSeriesParams = new DataSeriesParamsViewModel();
            InstrumentSeriesParams.PropertyChanged += InstrumentParam_PropertyChanged;
        }

        private void InstrumentParam_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged("IsInstrumentParamValid");
        }
    }
}