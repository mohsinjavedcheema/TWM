using System;
using System.Collections.ObjectModel;
using System.Linq;
using Twm.Core.Classes;
using Twm.Core.Controllers;
using Twm.Core.Managers;
using Twm.Core.ViewModels;
using Twm.DB.DAL.Repositories.Options;

namespace Twm.ViewModels.Options
{
    public class SystemOptionsViewModel:ViewModelBase
    {
        private const string HistoricalData = "\\Twm\\DB\\HistoricalData";

        public ObservableCollection<CategoryViewModel> Categories { get; set; }


        private SystemOptions _systemOptions;
        public SystemOptions SystemOptions
        {
            get
            {
                return _systemOptions;
            }

            set
            {
                if (_systemOptions != value)
                {
                    _systemOptions = value;
                    OnPropertyChanged();
                }
            }
        }
    

        public SystemOptionsViewModel()
        {
            SystemOptions = SystemOptions.Instance;
            SystemOptions.RemoveAllCommand = new OperationCommand((RemoveAllHistoricalData));
            /*SystemOptions.RemoveCommand = new OperationCommand(RemoveData);*/

            FillCategoryAndGroups();
            FetchOptions();

            var category = Categories.FirstOrDefault();
            if (category != null)
            {
                category.IsSelected = true;
                category.IsExpanded = true;
            }
            //SelectedCategoryAndGroup = CategoryAndGroups.FirstOrDefault();
          
        }

        /*private void RemoveData(object obj)
        {
            CacheData data = (CacheData)obj;
            var historicalDataManager = new HistoricalDataManager();
            historicalDataManager.DeleteCachedData(data);
        }*/

        private void RemoveAllHistoricalData(object obj)
        {
            var historicalDataManager = new HistoricalDataManager();
            historicalDataManager.ClearAll();
            SystemOptions.Providers = new ObservableCollection<HistoricalData>();
        }


        private void FetchOptions()
        {
            
           // List<SystemOption> systemOptions = SystemOptions.Options;

            foreach (var category in Categories)
            {
                foreach (var group in category.Groups)
                {
                    var options = SystemOptions.GetOptions(category.Name, group.Name);
                    foreach (var option in options)
                    {
                        group.SystemOptions.Add(new SystemOptionViewModel(option));
                    }
                }
                
            }
        }


        private void FillCategoryAndGroups()
        {
            Categories = new ObservableCollection<CategoryViewModel>();
            var categoryVm = new CategoryViewModel() {Name = "General"};
            Categories.Add(categoryVm);
            categoryVm.Groups.Add(new GroupViewModel(){Name = "Project"});
            categoryVm.Groups.Add(new GroupViewModel() { Name = "Preferences" });
            categoryVm.Groups.Add(new GroupViewModel() { Name = "Display" });
            categoryVm.Groups.Add(new GroupViewModel() { Name = "Email" });            
            categoryVm.Groups.Add(new GroupViewModel() { Name = "HistoricalData" });

            categoryVm = new CategoryViewModel() { Name = "Calculation" };
            Categories.Add(categoryVm);
            categoryVm.Groups.Add(new GroupViewModel() { Name = "Optimizer" });


        }

        public async void SaveOptions()
        {
             var changedOptions = SystemOptions.SyncOptions().ToList();

            using (var context = Session.Instance.DbContextFactory.GetContext())
            {
                var systemOptionRepository = new SystemOptionRepository(context);

                
                foreach (var systemOption in SystemOptions.GetOptions())
                {
                    if (changedOptions.Contains(systemOption.Code))
                    {
                        await systemOptionRepository.Update(systemOption);
                    }
                }
                
            }
        }

        public void ResetOptions()
        {
            SystemOptions.ResetOptions();
        }
    }
}