using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Twm.Core.Annotations;

namespace Twm.Core.Classes
{
    public class LogMessage: INotifyPropertyChanged
    {
        public DateTime DateTime { get; set; }

        public string Message { get; set; }


        public override string ToString()
        {
            return DateTime.ToString("yyyy-MM-dd HH:mm:ss") + "    " + Message;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}