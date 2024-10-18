
using Twm.Core.ViewModels;
using Twm.Model.Model;

namespace Twm.ViewModels.Options
{
    public class SystemOptionViewModel :ViewModelBase
    {
        public SystemOption DataModel { get; set; }


        public int Id
        {
            get
            {
                if (DataModel == null)
                    return 0;
                return DataModel.Id;
            }
            set
            {
                if (DataModel.Id != value)
                {
                    DataModel.Id = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get { return DataModel?.Name; }
            set
            {
                if (DataModel.Name != value)
                {
                    DataModel.Name = value;
                    OnDataPropertyChanged();
                    OnPropertyChanged();
                }
            }
        }


        public string Code
        {
            get { return DataModel?.Code; }
            set
            {
                if (DataModel.Code != value)
                {
                    DataModel.Code = value;
                    OnDataPropertyChanged();
                    OnPropertyChanged();
                }
            }
        }


        public string Category
        {
            get { return DataModel?.Category; }
            set
            {
                if (DataModel.Category != value)
                {
                    DataModel.Category = value;
                    OnDataPropertyChanged();
                    OnPropertyChanged();
                }
            }
        }


        public string Group
        {
            get { return DataModel?.Group; }
            set
            {
                if (DataModel.Group != value)
                {
                    DataModel.Group = value;
                    OnDataPropertyChanged();
                    OnPropertyChanged();
                }
            }
        }


        public string ValueType
        {
            get { return DataModel?.ValueType; }
            set
            {
                if (DataModel.ValueType != value)
                {
                    DataModel.ValueType = value;
                    OnDataPropertyChanged();
                    OnPropertyChanged();
                }
            }
        }


        public object Value
        {
            get
            {
                if (DataModel == null)
                    return null;

                if (DataModel.ValueType == "bool")
                    return DataModel.ValueBool;

                return DataModel.Value;

            }
            set
            {
                
                if (DataModel.ValueType == "bool" && DataModel.ValueBool != (bool)value)
                {
                    DataModel.ValueBool = (bool)value;
                    OnDataPropertyChanged();
                    OnPropertyChanged();
                }

                if (DataModel.Value != value.ToString())
                {
                    DataModel.Value = value.ToString();
                    OnDataPropertyChanged();
                    OnPropertyChanged();
                }
            }
        }

       

        public SystemOptionViewModel(SystemOption systemOption)
        {
            DataModel = systemOption;
        }

    }
}