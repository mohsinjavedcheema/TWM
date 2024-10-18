using System.ComponentModel;
using System.Runtime.CompilerServices;
using Twm.Chart.Annotations;

namespace Twm.Chart.DrawObjects
{
    public abstract class BaseDraw : INotifyPropertyChanged
    {

        public string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        [Browsable(false)]
        public string Tag { get; set; }

        [Browsable(false)]
        public bool IsDrawingObject { get; set; }

        [Browsable(false)]
        public bool IsSelected { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
