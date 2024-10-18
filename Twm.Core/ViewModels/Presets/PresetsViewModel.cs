using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Twm.Core.Classes;
using Twm.Core.Enums;
using Twm.Core.Helpers;
using Twm.Core.Interfaces;
using Twm.Core.UI.Windows.Presets;
using Twm.DB.DAL.Repositories.Presets;
using Twm.Model.Model;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Twm.Core.ViewModels.Presets
{
    public class PresetsViewModel : ViewModelBase
    {
        public ObservableCollection<PresetViewModel> Presets { get; set; }


        private PresetViewModel _selectedPreset;

        public PresetViewModel SelectedPreset
        {
            get { return _selectedPreset; }
            set
            {
                if (_selectedPreset != value)
                {
                    _selectedPreset = value;
                    if (_selectedPreset != null)
                        PresetName = _selectedPreset.Name;
                    OnPropertyChanged();
                }
            }
        }


        private Visibility _saveVisibility;

        public Visibility SaveVisibility
        {
            get { return _saveVisibility; }
            set
            {
                if (_saveVisibility != value)
                {
                    _saveVisibility = value;
                    OnPropertyChanged();
                }
            }
        }


        private Visibility _loadVisibility;

        public Visibility LoadVisibility
        {
            get { return _loadVisibility; }
            set
            {
                if (_loadVisibility != value)
                {
                    _loadVisibility = value;
                    OnPropertyChanged();
                }
            }
        }


        private Visibility _exportVisibility;

        public Visibility ExportVisibility
        {
            get { return _exportVisibility; }
            set
            {
                if (_exportVisibility != value)
                {
                    _exportVisibility = value;
                    OnPropertyChanged();
                }
            }
        }


        private string _presetName;
        public string PresetName
        {
            get { return _presetName; }
            set
            {
                if (_presetName != value)
                {
                    _presetName = value;
                    OnPropertyChanged();
                    OnPropertyChanged("IsSaveEnabled");
                }
            }
        }


        private string _presetModeName;
        public string PresetModeName
        {
            get { return _presetModeName; }
            set
            {
                if (_presetModeName != value)
                {
                    _presetModeName = value;
                    OnPropertyChanged();
                    
                }
            }
        }
        


        private bool _isLoadEnabled;

        public bool IsLoadEnabled
        {
            get { return _isLoadEnabled; }
            set
            {
                if (_isLoadEnabled != value)
                {
                    _isLoadEnabled = value;
                    OnPropertyChanged();
                }
            }
        }



        private bool _isLivePreset;

        public bool IsLivePreset
        {
            get { return _isLivePreset; }
            set
            {
                if (_isLivePreset != value)
                {
                    _isLivePreset = value;
                    OnPropertyChanged();
                }
            }
        }


        public bool IsSaveEnabled
        {
            get { return !string.IsNullOrEmpty(PresetName); }
        }


        public PresetType PresetType{ get; private set; }

        public PresetFormMode PresetFormMode { get; private set; }


        public ICommand RemovePresetCommand { get; set; }
        public ICommand RenamePresetCommand { get; set; }

        public ICommand ImportCommand { get; set; }
        public ICommand ExportCommand { get; set; }

        public PresetsViewModel(PresetType presetType, PresetFormMode presetFormMode = PresetFormMode.Save)
        {
            PresetType = presetType;
            PresetFormMode = presetFormMode;
            Presets = new ObservableCollection<PresetViewModel>();
            IsLivePreset = true;

            switch (presetFormMode)
            {
                case PresetFormMode.Save:
                    PresetModeName = "Save preset";
                    LoadVisibility = Visibility.Collapsed;
                    ExportVisibility = Visibility.Collapsed;
                    SaveVisibility = Visibility.Visible;
                    break;

                case PresetFormMode.Load:
                    PresetModeName = "Load preset";
                    SaveVisibility = Visibility.Collapsed;
                    ExportVisibility = Visibility.Collapsed;
                    LoadVisibility = Visibility.Visible;
                    break;

                case PresetFormMode.Export:
                    PresetModeName = "Export preset";
                    LoadVisibility = Visibility.Collapsed;
                    SaveVisibility = Visibility.Collapsed;
                    ExportVisibility = Visibility.Visible;
                    break;
            }
            RemovePresetCommand = new OperationCommand(RemovePreset);
            RenamePresetCommand = new OperationCommand(RenamePreset);

            
            
            ImportCommand = new OperationCommand(ImportPreset);
            ExportCommand = new OperationCommand(ExportPreset);
        }

        private async void RemovePreset(object obj)
        {
            if (SelectedPreset != null)
            {

                using (var context = Session.Instance.DbContextFactory.GetContext())
                {
                    var repository = new PresetRepository(context);
                    repository.Remove(SelectedPreset.DataModel);
                    await repository.CompleteAsync();
                    Presets.Remove(SelectedPreset);
                }
            }
        }

        private async void RenamePreset(object obj)
        {
            if (SelectedPreset != null)
            {
                var name = SelectedPreset.Name;

                var presetNameWindow = new PresetNameWindow(SelectedPreset);

                if (presetNameWindow.ShowDialog() == true)
                {
                    using (var context = Session.Instance.DbContextFactory.GetContext())
                    {
                        var repository = new PresetRepository(context);
                        await repository.Update(SelectedPreset.DataModel);
                    }
                }
                else
                {
                    SelectedPreset.Name = name;
                }
            }

        }

        private void ExportPreset(object obj)
        {
            var pathToPresets = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Twm" +
                                "\\Presets";
            if (!Directory.Exists(pathToPresets))
            {
                Directory.CreateDirectory(pathToPresets);
            }

            var pathToPresetType = pathToPresets;

            switch (PresetType)
            {
                case PresetType.Chart:
                    pathToPresetType += "\\Chart";
                    break;
                case PresetType.ValidatorInstrumentList:
                    pathToPresetType += "\\ValidatorInstrumentList";
                    break;
                case PresetType.OptimizerStrategy:
                    pathToPresetType += "\\OptimizerStrategy";
                    break;
                case PresetType.OptimizerTest:
                    pathToPresetType += "\\OptimizerTest";
                    break;
            }

            if (!Directory.Exists(pathToPresetType))
            {
                Directory.CreateDirectory(pathToPresetType);
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                DefaultExt = "json", Filter = "json files (*.json)|*.json", FileName = SelectedPreset.Name,
                InitialDirectory = pathToPresetType
            };

            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, SelectedPreset.Data);
            }

            MessageBox.Show("Preset successfully exported");
        }

        private async void ImportPreset(object obj)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            OpenFileDialog ofd = new OpenFileDialog()
            {
                DefaultExt = "json",
                Filter = "json files (*.json)|*.json",
                InitialDirectory = path
            };

            if (ofd.ShowDialog() == true)
            {
                var json = File.ReadAllText(ofd.FileName);
                var guid = "";
                int presetType;
                if (PresetType == PresetType.ValidatorInstrumentList)
                {
                    var presetObject = JsonHelper.ToObject<PresetObject<JObject[]>>(json);
                    presetType = (int) presetObject.PresetType;
                }
                else
                {
                    var presetObject = JsonHelper.ToObject<PresetObject<JObject>>(json);
                    presetType = (int)presetObject.PresetType;
                    if (presetObject.PresetType == PresetType.OptimizerStrategy ||
                        presetObject.PresetType == PresetType.Strategy)
                    {
                        guid = presetObject.Object["Guid"].Value<string>();
                    }
                }


                var preset = new Preset
                {
                    Data = json,
                    Guid = guid,
                    Type = presetType,
                    Name = Path.GetFileNameWithoutExtension(ofd.FileName)
                };

                using (var context = Session.Instance.DbContextFactory.GetContext())
                {
                    var repository = new PresetRepository(context);
                    await repository.Add(preset);
                    await repository.CompleteAsync();
                }

                if ((int)PresetType == presetType)
                {
                    var presetVm = new PresetViewModel(preset);
                    Presets.Add(presetVm);
                    SelectedPreset = presetVm;
                }
                else
                {
                    MessageBox.Show("Preset successfully imported," + "\r\n" + "but it's a different type ");
                }
            }
        }

        

        


        public void FetchData(string guid = null)
        {
            Presets.Clear();

            using (var context = Session.Instance.DbContextFactory.GetContext())
            {
                var repository = new PresetRepository(context);

                IEnumerable<Preset> presets;
                if (string.IsNullOrEmpty(guid))
                {
                    presets = repository.FindBy(x => x.Type == (int)PresetType);
                }
                else
                {
                    presets = repository.FindBy(x => x.Type == (int)PresetType && x.Guid.ToLower() == guid.ToLower());
                }


                foreach (var preset in presets)
                {
                    var presetViewModel = new PresetViewModel(preset);

                    Presets.Add(presetViewModel);
                }

            }
        }

        public bool CheckName()
        {
            var preset = Presets.FirstOrDefault(x => x.Name == PresetName);

            if (preset == null)
            {
                SelectedPreset = null;
                return true;
            }

            if (MessageBox.Show("A preset with this name already exists.\r\n Do you want to rewrite it?",
                    "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                SelectedPreset = preset;
                return true;
            }

            return false;
        }
    }
}