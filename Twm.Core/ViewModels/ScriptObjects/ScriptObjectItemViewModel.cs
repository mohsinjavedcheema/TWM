using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Twm.Chart.Annotations;

namespace Twm.Core.ViewModels.ScriptObjects
{
    public class ScriptObjectItemViewModel:INotifyPropertyChanged
    {

        public ObservableCollection<object> Items { get; set; }

        public bool IsFolder { get; set; }

        public string Name { get; set; }


        [DataMember]
        public Type ObjectType { get; set; }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}