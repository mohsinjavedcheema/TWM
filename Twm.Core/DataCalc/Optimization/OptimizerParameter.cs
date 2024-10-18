using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Twm.Chart.Annotations;
using Twm.Core.Converters;
using Newtonsoft.Json;

namespace Twm.Core.DataCalc.Optimization
{
    [DataContract]
    [JsonConverter(typeof(OptimizerParameterJsonConverter))]
    public abstract class OptimizerParameter : INotifyPropertyChanged, ICloneable
    {
        public Type Type { get; set; }

        [DataMember]
        public int TypeNum { get; set; }


        [DataMember]
        public string TypeName
        {
            get
            {
                if (Type == null)
                    return "";
                
                return Type.Name;
            } 

        }

        private bool _isChecked;
        [DataMember]
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged();
                    OnPropertyChanged("CombinationCount");
                }
            }
        }

      

        private string _name;
        [DataMember]
        public string Name
        {
            get { return _name;}
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                if (_displayName != value)
                {
                    _displayName = value;
                    OnPropertyChanged();
                }
            }
        }

        public abstract object CurrentValue
        {
            get;
            set;
        }


        public abstract object DefaultValue
        {
            get;
        }


        public abstract long CombinationCount
        {
            get;
        }


        protected OptimizerParameter(string name, Type type)
        {
            Name = name;
            Type = type;
        }



        protected OptimizerParameter(string name)
        {
            Name = name;
        }



        public void UpdateProperty(string name)
        {
            OnPropertyChanged(name);
        }

        public abstract void ResetValue();

        public abstract bool NextVal();

     

        public abstract void CopyPreset(OptimizerParameter parameter);


        public object Clone()
        {
            return MemberwiseClone();
        }


        public static Type GetType(string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(typeName);
                if (type == null)
                    continue;
                if (type.IsEnum)
                    return type;
            }
            return null;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


      
    }
}