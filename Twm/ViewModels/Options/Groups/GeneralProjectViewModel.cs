using System.Linq;
using Twm.Core.ViewModels;

namespace Twm.ViewModels.Options.Groups
{
    public class GeneralProjectViewModel : ViewModelBase
    {
        private string _pathToProject;
        public string PathToProject
        {
            get { return _pathToProject; }
            set
            {
                if (_pathToProject != value)
                {
                    _pathToProject = value;
                    OnPropertyChanged();
                    OnDataPropertyChanged();
                }
            }
        }


        private string _pathToProjectDll;
        public string PathToProjectDll
        {
            get { return _pathToProjectDll; }
            set
            {
                if (_pathToProjectDll != value)
                {
                    _pathToProjectDll = value;
                    OnPropertyChanged();
                    OnDataPropertyChanged();
                }
            }
        }


        private bool _compileDebug;
        public bool CompileDebug
        {
            get { return _compileDebug; }
            set
            {
                if (_compileDebug != value)
                {
                    _compileDebug = value;
                    OnPropertyChanged();
                    OnDataPropertyChanged();
                }
            }
        }

        private bool _reloadOnRecompile;
        public bool ReloadOnRecompile
        {
            get { return _reloadOnRecompile; }
            set
            {
                if (_reloadOnRecompile != value)
                {
                    _reloadOnRecompile = value;
                    OnPropertyChanged();
                    OnDataPropertyChanged();
                }
            }
        }

        public GeneralProjectViewModel()
        {

        }

        public void FetchData(GroupViewModel groupViewModel)
        {
            var option = groupViewModel.SystemOptions.FirstOrDefault(x => x.Code == "CstPrjPath");
            _pathToProject = option?.Value.ToString();

            option = groupViewModel.SystemOptions.FirstOrDefault(x => x.Code == "CstPrjDllPath");
            _pathToProjectDll = option?.Value.ToString();

            option = groupViewModel.SystemOptions.FirstOrDefault(x => x.Code == "CompileDebug");
            _compileDebug = (bool)option?.Value;

            option = groupViewModel.SystemOptions.FirstOrDefault(x => x.Code == "ReloadOnRecompile");
            _reloadOnRecompile = (bool)option?.Value;

            OnPropertyChanged("PathToProject");
            OnPropertyChanged("PathToProjectDll");
            OnPropertyChanged("CompileDebug");
            OnPropertyChanged("ReloadOnRecompile");
            IsLoaded = true;
        }
    }
}