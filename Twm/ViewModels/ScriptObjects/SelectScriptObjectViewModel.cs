using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Twm.Core.Classes;
using Twm.Core.Controllers;
using Twm.Core.DataCalc;
using Twm.Core.Enums;
using Twm.Core.Helpers;
using Twm.Core.UI.Windows.Presets;
using Twm.Core.ViewModels;
using Twm.Core.ViewModels.Presets;
using Twm.Core.ViewModels.ScriptObjects;
using Twm.DB.DAL.Repositories.Presets;
using Twm.Model.Model;
using Twm.ViewModels.Presets;
using static System.String;


namespace Twm.ViewModels.ScriptObjects
{
    public class SelectScriptObjectViewModel : ViewModelBase
    {
        public DataCalcContext DataCalcContext { get; set; }

        public ObservableCollection<object> AvailableObjects { get; set; }

        public ObservableCollection<ScriptBase> ConfiguredObjects { get; set; }


        public string AvailableObjectsHeader { get; set; }

        public string ConfiguredObjectsHeader { get; set; }


        private Type  _objectType;

        private object _selectedObjectType;


        public object SelectedObjectType
        {
            get { return _selectedObjectType; }
            set
            {
                if (value != _selectedObjectType)
                {
                    _selectedObjectType = value;
                    if (_selectedObjectType != null)
                        SelectedObject = null;
                    OnPropertyChanged();
                }
            }
        }


        private ScriptBase _selectedObject;

        public ScriptBase SelectedObject
        {
            get { return _selectedObject; }
            set
            {
                if (value != _selectedObject)
                {
                    _selectedObject = value;
                    if (_selectedObject != null)
                        SelectedObjectType = null;
                    OnPropertyChanged();
                    OnPropertyChanged("IsStrategyPresetSaveEnable");
                }
            }
        }

        public bool IsStrategyPresetSaveEnable
        {
            get { return SelectedObject != null; }
        }


        public OperationCommand ConfigureObjectCommand { get; set; }
        public OperationCommand RemoveObjectCommand { get; set; }
        public OperationCommand ApplyCommand { get; set; }

        public OperationCommand StrategyPresetsLoadCommand { get; set; }
        public OperationCommand StrategyPresetsSaveCommand { get; set; }
        public OperationCommand StrategyPresetsExportCommand { get; set; }



        public SelectScriptObjectViewModel(Type objectType, DataCalcContext dataCalcContext)
        {
            _objectType = objectType;

            DataCalcContext = dataCalcContext;
            
            AvailableObjects = new ObservableCollection<object>();
            ConfiguredObjects = new ObservableCollection<ScriptBase>();

            SelectedObject = null;

            ConfigureObjectCommand = new OperationCommand(ConfigureObject);
            RemoveObjectCommand = new OperationCommand(RemoveObject);
            StrategyPresetsLoadCommand = new OperationCommand(StrategyPresetsLoad);
            StrategyPresetsSaveCommand = new OperationCommand(StrategyPresetsSave);
            StrategyPresetsExportCommand = new OperationCommand(StrategyPresetsExport);
        }


        private void RemoveObject(object obj)
        {
            if (SelectedObject != null)
            {
                SelectedObject.Clear();
                ConfiguredObjects.Remove(SelectedObject);
                SelectedObject = ConfiguredObjects.FirstOrDefault();
            }
        }

        private void ConfigureObject(object obj)
        {
            if (SelectedObjectType != null)
            {
                if (SelectedObjectType is ScriptObjectItemViewModel scriptItemViewModel && !scriptItemViewModel.IsFolder)
                {
                    var scriptObject = DataCalcContext.CreateObject(scriptItemViewModel.ObjectType, null, true);
                    ConfiguredObjects.Add(scriptObject);
                    SelectedObject = scriptObject;
                }

                
            }
        }

        public void Init()
        {
            var objectTypes = BuildController.Instance.CustomAssembly.GetTypes()
                .Where(x => x.IsSubclassOf(_objectType)).OrderBy(x=>x.Name);

            foreach (var objectType in objectTypes)
            {
                if (objectType.Name == BuildController.IndicatorBaseTypeName || objectType.Name == BuildController.StrategyBaseTypeName)
                    continue;
               
                var fields = objectType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance
                                                                                   | BindingFlags.Public | BindingFlags.Static);
                var name = fields.FirstOrDefault(info =>
                    info.Name.Equals("StrategyName", StringComparison.OrdinalIgnoreCase));

                if (name == null)
                    name = fields.FirstOrDefault(info =>
                        info.Name.Equals("IndicatorName", StringComparison.OrdinalIgnoreCase));

                var item = new ScriptObjectItemViewModel
                {
                    IsFolder = false, Name = (string)(name != null && !IsNullOrEmpty(name.GetValue(null).ToString()) ? name.GetValue(null) : objectType.Name), ObjectType = objectType
                };
                if (objectType.Namespace == BuildController.IndicatorDefaultNamespace ||
                    objectType.Namespace == BuildController.StrategiesDefaultNamespace)
                {
                    AvailableObjects.Add(item);
                }
                else
                {

                    var foldersString = objectType.Namespace?.Replace(BuildController.IndicatorDefaultNamespace + ".", "").
                        Replace(BuildController.StrategiesDefaultNamespace + ".", "");
                    var folderNames = foldersString?.Split('.');

                    ObservableCollection<object> currentFolder = AvailableObjects;
                    if (folderNames != null)
                    {
                        foreach (var folderName in folderNames)
                        {
                            var folder = currentFolder.OfType<ScriptObjectItemViewModel>()
                                .FirstOrDefault(x => x.IsFolder && x.Name == folderName);
                            if (folder == null)
                            {
                                folder = new ScriptObjectItemViewModel
                                {
                                    IsFolder = true, Name = folderName, Items = new ObservableCollection<object>(),
                                };

                                var prevFolder = currentFolder.OfType<ScriptObjectItemViewModel>()
                                    .LastOrDefault(x =>
                                        x.IsFolder && Compare(x.Name, folder.Name, StringComparison.Ordinal) > 0);

                                if (prevFolder == null)
                                {
                                    currentFolder.Insert(0, folder);
                                }
                                else
                                {
                                    var index = currentFolder.IndexOf(prevFolder);
                                    currentFolder.Insert(index + 1, folder);
                                }
                            }

                            currentFolder = folder.Items;
                        }
                    }

                    currentFolder.Add(item);
                }
            }

            if (_objectType == typeof(IndicatorBase))
            {
                foreach (var indicator in DataCalcContext.Indicators)
                {
                    ConfiguredObjects.Add(indicator);
                }
            }
            if (_objectType == typeof(StrategyBase))
            {
                foreach (var strategy in DataCalcContext.Strategies)
                {
                    strategy.IsTemporary = true;
                    ConfiguredObjects.Add(strategy);
                }
            }


            SelectedObjectType = AvailableObjects.FirstOrDefault();
        }



        private async void StrategyPresetsSave(object obj)
        {
            if (SelectedObject != null)
            {
                if (SelectedObject is StrategyBase strategy)
                {

                    var presetsViewModel = new PresetsViewModel(PresetType.Strategy);
                    presetsViewModel.FetchData(strategy.Guid.ToString());
                    presetsViewModel.PresetName = strategy.Name;

                    var presetsWindow = new PresetsWindow(presetsViewModel);
                    if (presetsWindow.ShowDialog() == true)
                    {
                        Preset preset = null;
                        if (presetsViewModel.SelectedPreset != null)
                            preset = presetsViewModel.SelectedPreset.DataModel;
                        await SaveStrategyPreset(strategy, preset, presetsViewModel.PresetName);
                    }
                }
            }
        }

        private async Task SaveStrategyPreset(StrategyBase strategy, Preset preset, string name)
        {
            using (var context = Session.Instance.DbContextFactory.GetContext())
            {
                var repository = new PresetRepository(context);
                var validatorStrategyPreset = new ValidatorStrategyPreset(strategy);

                if (preset == null)
                {
                    preset = new Preset()
                    {
                        Name = name,
                        Guid = strategy.Guid.ToString(),
                        Type = (int)PresetType.Strategy,
                        Data = JsonHelper.ToJson(new PresetObject<ValidatorStrategyPreset>()
                            { Object = validatorStrategyPreset, PresetType = PresetType.Strategy })
                    };
                    await repository.Add(preset);
                }
                else
                {
                    preset.Data = JsonHelper.ToJson(new PresetObject<ValidatorStrategyPreset>()
                        { Object = validatorStrategyPreset, PresetType = PresetType.Strategy });
                    await repository.Update(preset);

                }
                await repository.CompleteAsync();
            }
        }


        private void StrategyPresetsLoad(object obj)
        {
            var presetsViewModel = new PresetsViewModel(PresetType.Strategy, PresetFormMode.Load);
            if (!(SelectedObject is StrategyBase))
                presetsViewModel.FetchData();
            else
            {
                presetsViewModel.FetchData((SelectedObject as StrategyBase)?.Guid.ToString());
            }

            
            presetsViewModel.SelectedPreset = presetsViewModel.Presets.FirstOrDefault();
            

            var presetsWindow = new PresetsWindow(presetsViewModel);
            if (presetsWindow.ShowDialog() == true)
            {
                if (presetsViewModel.SelectedPreset != null &&
                    !string.IsNullOrEmpty(presetsViewModel.SelectedPreset.Data))
                {
                    var scriptObject = CreateStrategyByPreset(presetsViewModel.SelectedPreset.Data);
                    RemoveObject(null);
                    ConfiguredObjects.Add(scriptObject);
                   
                    SelectedObject = scriptObject;
                }
            }
        }


        private StrategyBase CreateStrategyByPreset(string json)
        {
            StrategyBase strategy = null;
            try
            {
                var presetObject =
                    JsonHelper.ToObject<PresetObject<ValidatorStrategyPreset>>(json);

                var strategyType = BuildController.Instance.GetStrategyTypeByGuid(presetObject.Object.Guid);
                var scriptObject = DataCalcContext.CreateObject(strategyType, null, true);

                strategy = (StrategyBase)scriptObject;

                if (presetObject.Object.Parameters != null &&
                    presetObject.Object.Parameters.Any())
                {
                    foreach (var property in strategy.TwmPropertyNames)
                    {
                        if (presetObject.Object.Parameters.TryGetValue(property, out var value))
                        {
                            strategy.SetTwmPropertyValue(property, value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can`t load preset: " + ex.Message);
                LogController.Print("Can`t load preset: " + ex.Message + "\r\n" + ex.StackTrace);
            }

            return strategy;
        }

        private void StrategyPresetsExport(object obj)
        {
            var presetsViewModel = new PresetsViewModel(PresetType.Strategy, PresetFormMode.Export);
            presetsViewModel.FetchData();
            presetsViewModel.SelectedPreset = presetsViewModel.Presets.FirstOrDefault();

            var presetsWindow = new PresetsWindow(presetsViewModel);
            if (presetsWindow.ShowDialog() == true)
            {
                if (presetsViewModel.SelectedPreset != null &&
                    !string.IsNullOrEmpty(presetsViewModel.SelectedPreset.Data))
                {
                    presetsViewModel.ExportCommand.Execute(null);
                }
            }
        }


    }
}