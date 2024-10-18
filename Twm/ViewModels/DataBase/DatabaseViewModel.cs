using System.Collections.ObjectModel;
using System.Linq;
using Twm.Core.ViewModels;

namespace Twm.ViewModels.DataBase
{
    public class DatabaseViewModel:ViewModelBase
    {
        public ObservableCollection<ViewModelBase> Items { get; set; }


        private double? _value;
        public double? Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }

        }

        private ViewModelBase _selectedModel;
        public ViewModelBase SelectedModel
        {
            get { return _selectedModel; }
            set
            {
                if (_selectedModel != value)
                {
                    _selectedModel = value;
                    _selectedModel.FetchData();
                }
            }

        }

        public DatabaseViewModel()
        {
            Items = new ObservableCollection<ViewModelBase>();
            Items.Add(new OptimizerResultsViewModel());
            Value = null;
            SelectedModel = Items.FirstOrDefault();
        }
    }
}