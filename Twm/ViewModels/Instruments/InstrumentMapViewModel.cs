using System.ComponentModel;
using System.Runtime.CompilerServices;
using Twm.Chart.Annotations;
using Twm.Core.ViewModels.Instruments;
using Twm.Model.Model;

namespace Twm.ViewModels.Instruments
{
    public class InstrumentMapViewModel:INotifyPropertyChanged
    {
        public InstrumentMap DataModel { get; set; }


        public int Id
        {
            get { return DataModel.Id; }
            set
            {
                if (DataModel.Id != value)
                {
                    DataModel.Id = value;
                    OnPropertyChanged();
                }
            }
        }

        public InstrumentViewModel FirstInstrument { get; set; }

        public InstrumentViewModel SecondInstrument { get; set; }



        public InstrumentMapViewModel(InstrumentMap instrumentMap)
        {
            DataModel = instrumentMap;

            FirstInstrument = new InstrumentViewModel(instrumentMap.FirstInstrument);
            SecondInstrument = new InstrumentViewModel(instrumentMap.SecondInstrument);
        }







        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        }
    }
}
