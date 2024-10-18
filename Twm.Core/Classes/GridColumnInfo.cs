using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows;
using Twm.Chart.Annotations;

namespace Twm.Core.Classes
{
    [DataContract]
    public class GridColumnInfo : INotifyPropertyChanged, ICloneable
    {
        private string _name;
        [DataMember]
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

        private double _width;
        [DataMember]
        public double Width
        {
            get { return _width; }
            set
            {
                if (_width != value)
                {
                    _width = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _caption;

        public string Caption
        {
            get { return _caption; }
            set
            {
                if (_caption != value)
                {
                    _caption = value;
                    OnPropertyChanged();
                }
            }
        }


        private Visibility _visibility;

        [DataMember]
        public Visibility Visibility
        {
            get { return _visibility; }
            set
            {
                if (_visibility != value)
                {
                    _visibility = value;
                    OnPropertyChanged();
                }
            }
        }

        public GridColumnInfo()
        {
        }

        public GridColumnInfo(string name, string caption, Visibility visibility = Visibility.Visible, double width = 100)
        {
            Name = name;
            Caption = caption;
            Visibility = visibility;
            Width = width;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}