using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Twm.Core.ViewModels;

namespace Twm.ViewModels.Options
{
    public class CategoryViewModel:ViewModelBase
    {

        public ObservableCollection<GroupViewModel> Groups { get; set; }

        public string Name { get; set; }

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

        public CategoryViewModel()
        {
            Groups = new ObservableCollection<GroupViewModel>();
        }
    }
}