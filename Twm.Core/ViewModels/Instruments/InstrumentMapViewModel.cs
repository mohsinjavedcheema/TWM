using System.ComponentModel;
using System.Runtime.CompilerServices;
using Twm.Chart.Annotations;

namespace Twm.Core.ViewModels.Instruments
{
    public class InstrumentMapViewModel:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
