using Twm.Model.Model;

namespace Twm.Core.ViewModels.Presets
{
    public class PresetViewModel : ViewModelBase
    {
        public Preset DataModel { get; set; }


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


        public string Guid
        {
            get { return DataModel?.Guid; }
            set
            {
                if (DataModel.Guid != value)
                {
                    DataModel.Guid = value;
                    OnDataPropertyChanged();
                    OnPropertyChanged();
                }
            }
        }

        public string Data
        {
            get { return DataModel?.Data; }
            set
            {
                if (DataModel.Data != value)
                {
                    DataModel.Data = value;
                    OnDataPropertyChanged();
                    OnPropertyChanged();
                }
            }
        }

        public PresetViewModel(Preset preset)
        {
            DataModel = preset;
        }
    }
}