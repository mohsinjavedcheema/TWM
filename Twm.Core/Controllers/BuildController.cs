using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Twm.Core.Attributes;
using Twm.Core.Classes;
using Twm.Core.Controllers.CodeGenerators;
using Twm.Core.DataCalc;
using Twm.Core.DataCalc.Commissions;
using Twm.Core.DataCalc.MoneyManagement;
using Twm.Core.DataCalc.Optimization;
using Twm.Core.Enums;
using Twm.Core.ViewModels.ScriptObjects;
using Project = Microsoft.Build.Evaluation.Project;


namespace Twm.Core.Controllers
{
    public class BuildController
    {

        public static  string IndicatorDefaultNamespace = "Twm.Custom.Indicators";
        public static string StrategiesDefaultNamespace = "Twm.Custom.Strategies";

        public static string IndicatorBaseTypeName = "Indicator";
        public static string StrategyBaseTypeName = "Strategy";

        private const string DebugName = "Debug";

        public Assembly CustomAssembly { get; set; }

        private static BuildController _mInstance;

        public static BuildController Instance
        {
            get { return _mInstance ?? (_mInstance = new BuildController()); }
        }


        public event EventHandler OnCompile;

        private readonly string _pathToProject;

        private readonly string _projectObjPath;

        private readonly string _pdbFileName;

        private readonly string _indicatorTypeName;

        private readonly string _tvmPropertyName;

        private readonly Type _indicatorType = typeof(IndicatorBase);

        private readonly Type _strategyType = typeof(StrategyBase);

        private readonly Type _optimizerType = typeof(Optimizer);

        private readonly Type _moneyManagementType = typeof(MoneyManagement);

        private readonly Type _commissionType = typeof(Commission);

        private readonly Type _optimizationFitnessType = typeof(OptimizationFitness);

        private List<Type> _availableIndicatorTypes;
        private List<Type> _availableStrategyTypes;

        public List<ScriptObjectItemViewModel> OptimizerStrategyTypes { get; set; }

        public List<ScriptObjectItemViewModel> OptimizationFitnessTypes { get; set; }

        public List<ScriptObjectItemViewModel> MoneyManagementTypes { get; set; }

        public List<ScriptObjectItemViewModel> CommissionTypes { get; set; }

        public BuildController()
        {
            _pathToProject = Path.Combine(SystemOptions.Instance.PathToProject, "Twm.Custom.csproj");
            _projectObjPath = Path.Combine(SystemOptions.Instance.PathToProject, $"obj\\{DebugName}");
            _pdbFileName = Path.Combine(_projectObjPath, "Twm.Custom.pdb");
            _indicatorTypeName = "Indicator";
            _tvmPropertyName = (typeof(TwmPropertyAttribute).Name).Replace("Attribute", "");
        }


        private void RemovePdb()
        {
            if (File.Exists(_pdbFileName))
            {
                var newPdbFileName = _projectObjPath + Guid.NewGuid() + "_" + "Twm.Custom.pdb";
                File.Move(_pdbFileName, newPdbFileName);
            }
        }

        private void GenerateCode(Project project)
        {
            foreach (var item in project.AllEvaluatedItems)
            {
                if (item.ItemType == "Compile")
                {
                    var filePath = Path.Combine(SystemOptions.Instance.PathToProject, item.EvaluatedInclude);
                    if (File.Exists(filePath))
                    {
                        string text = File.ReadAllText(filePath);
                        SyntaxTree tree = CSharpSyntaxTree.ParseText(text);
                        CompilationUnitSyntax root = tree.GetCompilationUnitRoot();


                        var namespaceDeclarations = from namespaceDeclaration
                                in root.DescendantNodes().OfType<NamespaceDeclarationSyntax>()
                            select namespaceDeclaration;

                        var nameSpace = namespaceDeclarations.FirstOrDefault();

                        if (nameSpace == null)
                            continue;

                        if (nameSpace.FullSpan.End < text.Length)
                            text = text.Remove(nameSpace.FullSpan.End);
                        
                        var indicatorDeclarations = from classDeclaration
                                in root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                            where classDeclaration.BaseList != null &&
                                  classDeclaration.BaseList.Types.Any(x => x.Type.ToString() == _indicatorTypeName)
                            select classDeclaration;

                        var indicatorDeclaration = indicatorDeclarations.FirstOrDefault();

                        if (indicatorDeclaration != null)
                        {
                            var className = indicatorDeclaration.Identifier.ToString();

                            var tvmProperties = from propertyDeclaration
                                    in indicatorDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>()
                                where propertyDeclaration.AttributeLists.Any(x =>
                                    x.Attributes.Any(y => y.Name.ToString() == _tvmPropertyName))
                                select propertyDeclaration;


                            var indicatorGenerator = new IndicatorCodeGenerator(className, tvmProperties.ToList());
                            indicatorGenerator.Build();
                            var generatedCode = indicatorGenerator.Generate();

                            var index = text.LastIndexOf('}');
                            if (index > 0 && index+1 < text.Length)
                            {
                                text = text.Remove(index+1);
                            }

                            text += "\r\n\r\n";
                            text += generatedCode;
                            File.WriteAllText(filePath, text);

                            
                        }
                    }
                }
            }
            
        }

        public async void CompileProject()
        {
            var configuration = SystemOptions.Instance.CompileDebug ? DebugName : CurrentConfiguration();

            await Session.Instance.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (System.Action) delegate() { Session.Instance.CompileState = CompileState.Compiling; }
            );
            try
            {
                CustomAssembly = null;
                RemovePdb();
                var pc = new ProjectCollection() {DefaultToolsVersion = "4.0"};
                var project = new Project(_pathToProject, null, "4.0", pc);
                project.SetGlobalProperty("Configuration", configuration);

                var logger = new BuildLogger();
                var loggers = new List<ILogger>() {logger};

                GenerateCode(project);

                var buildSucceed = project.Build(loggers);

                LogController.Print(logger.Source.ToArray());

                if (buildSucceed)
                {
                    LoadCustomAssembly();

                    SoundPlayer sound = new SoundPlayer {Stream = Properties.Resource.CompiledSuccessfully};
                    sound.Play();

                    await RecreateSessionObjects();
                    await Session.Instance.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (System.Action) delegate() { Session.Instance.CompileState = CompileState.Compiled; }
                    );
                    OnCompile?.Invoke(this, new EventArgs());

                }
                else
                {
                    SystemSounds.Beep.Play();
                    await Session.Instance.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (System.Action) delegate() { Session.Instance.CompileState = CompileState.Failed; }
                    );
                }

                
            }
            catch (Exception ex)
            {
                LogController.Print("Critical build error: " + ex.Message);
                await Session.Instance.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (System.Action) delegate() { Session.Instance.CompileState = CompileState.Failed; }
                );
            }
        }

        private string CurrentConfiguration()
        {
#if DEBUG
            return DebugName;
#else
            return "Release";
#endif
        }

        public void LoadCustomAssembly()
        {
            try
            {
                var pathToProjectDll = SystemOptions.Instance.PathToProjectDll;

                var configurationName = CurrentConfiguration();
                if (SystemOptions.Instance.CompileDebug)
                    configurationName = DebugName;
                pathToProjectDll = pathToProjectDll.Replace("$(ConfigurationName)", configurationName);

                var path = Path.Combine(pathToProjectDll, "Twm.Custom.dll");
                CustomAssembly = Assembly.Load(File.ReadAllBytes(path));
                Session.Instance.CompileState = CompileState.Compiled;
                FillObjectTypeCollections();

                LogController.Print("Twm.Custom.dll successfully loaded");
            }
            catch (Exception ex)
            {
                LogController.Print("Custom project load error: " + ex.Message);
                Session.Instance.CompileState = CompileState.Failed;
            }
        }

        private void FillObjectTypeCollections()
        {
            OptimizerStrategyTypes = new List<ScriptObjectItemViewModel>();
            OptimizationFitnessTypes = new List<ScriptObjectItemViewModel>();
            MoneyManagementTypes = new List<ScriptObjectItemViewModel>();
            CommissionTypes = new List<ScriptObjectItemViewModel>();

            var objectTypes = CustomAssembly.GetTypes();

            foreach (var objectType in objectTypes)
            {
                if (objectType.IsSubclassOf(_optimizerType))
                {
                    OptimizerStrategyTypes.Add(CreateItemByName(objectType, "OptimizerName"));
                }

                if (objectType.IsSubclassOf(_optimizationFitnessType))
                {
                    OptimizationFitnessTypes.Add(CreateItemByName(objectType, "OptimizationFitnessName"));
                }

                if (objectType.IsSubclassOf(_moneyManagementType))
                {
                    MoneyManagementTypes.Add(CreateItemByName(objectType, "MoneyManagementName"));
                }

                if (objectType.IsSubclassOf(_commissionType))
                {
                    CommissionTypes.Add(CreateItemByName(objectType, "CommissionName"));
                }
            }

            GetAvailableScriptObjectTypes();
        }

        private ScriptObjectItemViewModel CreateItemByName(Type objectType, string fieldName)
        {
            var fields = objectType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance
                                                                     | BindingFlags.Public | BindingFlags.Static);
            var name = fields.FirstOrDefault(info =>
                info.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));


            var item = new ScriptObjectItemViewModel
            {
                IsFolder = false,
                Name = (string)(name != null && !string.IsNullOrEmpty(name.GetValue(null).ToString()) ? name.GetValue(null) : objectType.Name),
                ObjectType = objectType
            };
            return item;
        }


        private void GetAvailableScriptObjectTypes()
        {
            var allTypes = CustomAssembly.GetTypes();
            _availableIndicatorTypes = allTypes.Where(x => x.IsSubclassOf(_indicatorType)).ToList();
            _availableStrategyTypes = allTypes.Where(x => x.IsSubclassOf(_strategyType)).ToList();
        }


        private async Task RecreateSessionObjects()
        {
            GetAvailableScriptObjectTypes();
            foreach (var dataCalcContext in Session.Instance.DataCalcContexts)
            {
                if (dataCalcContext.IsSingleChart && !SystemOptions.Instance.ReloadOnRecompile)
                    continue;

                await RecreateIndicators(dataCalcContext);
                RecreateStrategies(dataCalcContext);
            }
        }


        public async Task RecreateIndicators(DataCalcContext dataCalcContext)
        {
            var oldIndicators = dataCalcContext.Indicators.ToList();
            dataCalcContext.Indicators.Clear();
            var indicators = new List<ScriptBase>();

            foreach (var oldIndicator in oldIndicators)
            {
                var oldIndicatorType = oldIndicator.GetType();

                var newIndicatorType = _availableIndicatorTypes.FirstOrDefault(x => x.Name == oldIndicatorType.Name);

                if (newIndicatorType != null)
                {
                    var indicator = oldIndicator.GetDataCalcContext().CreateIndicator(newIndicatorType, null, true);
                    indicator.IsTemporary = false;
                    var oldProperties = oldIndicatorType.GetProperties().ToList();
                    var newProperties = newIndicatorType.GetProperties().ToList();

                    foreach (var propertyInfo in newProperties)
                    {
                        if (propertyInfo.GetSetMethod() == null)
                            continue;

                        if (propertyInfo.GetCustomAttributes(false).Any(
                            x => (x is BrowsableAttribute ba && !ba.Browsable)))
                        {
                            continue;
                        }

                        var oldPropertyInfo = oldProperties.FirstOrDefault(x => x.Name == propertyInfo.Name);

                        if (oldPropertyInfo != null)
                        {
                            propertyInfo.SetValue(indicator, oldPropertyInfo.GetValue(oldIndicator));
                        }
                    }

                   // indicator.SetDataCalcContext(oldIndicator.GetDataCalcContext());
                    indicator.ViewModel = oldIndicator.ViewModel;
                    indicators.Add(indicator);

                    oldIndicator.Clear();
                }
            }
            dataCalcContext.SynchronizeIndicators(indicators);

            dataCalcContext.CreateToken();

            await Task.Run(() => ExecuteScript(dataCalcContext), dataCalcContext.CancellationTokenSourceCalc.Token);

        }

        private async Task ExecuteScript(DataCalcContext dataCalcContext)
        {
            var indicator = dataCalcContext.Indicators.FirstOrDefault();
            if (indicator != null)
            {
                dataCalcContext.Chart.Dispatcher.Invoke(() =>
                    {
                        indicator.ViewModel.IsBusy = true;
                        indicator.ViewModel.Message = "Calculating...";
                    }
                );

                
                try
                {
                    await dataCalcContext.Execute();
                    await dataCalcContext.Chart.Dispatcher.InvokeAsync(() =>
                        {
                            dataCalcContext.Chart.ReCalc_VisibleCandlesExtremums();
                            dataCalcContext.Chart.ChartControl?.Invalidate();
                        }
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Execute script error");
                    LogController.Print(ex.Message);
                    LogController.Print(ex.StackTrace);
                    if (ex.InnerException != null)
                    {
                        LogController.Print(ex.InnerException.StackTrace);
                    }
                }
                finally
                {
                    dataCalcContext.Chart.Dispatcher.Invoke(() =>
                        {
                            indicator.ViewModel.IsBusy = false;
                        }
                    );
                    
                }
            }
        }



        public void RecreateStrategies(DataCalcContext dataCalcContext)
        {
            var oldStrategies = dataCalcContext.Strategies.ToList();
            dataCalcContext.Strategies.Clear();

            foreach (var oldStrategy in oldStrategies)
            {
                var oldStrategyType = oldStrategy.GetType();

                var newStrategyType = _availableStrategyTypes.FirstOrDefault(x => x.Name == oldStrategyType.Name);

                if (newStrategyType != null)
                {
                    var strategy = oldStrategy.GetDataCalcContext().CreateStrategy(newStrategyType, null, oldStrategy.IsTemporary);

                    var oldProperties = oldStrategyType.GetProperties().ToList();
                    var newProperties = newStrategyType.GetProperties().ToList();

                    foreach (var propertyInfo in newProperties)
                    {
                        if (propertyInfo.GetSetMethod() == null)
                            continue;

                        if (propertyInfo.GetCustomAttributes(false).Any(
                            x => (x is BrowsableAttribute ba && !ba.Browsable)))
                        {
                            continue;
                        }

                        if (propertyInfo.Name == "Enabled")
                        {
                            continue;
                        }

                        var oldPropertyInfo = oldProperties.FirstOrDefault(x => x.Name == propertyInfo.Name);

                        if (oldPropertyInfo != null)
                        {
                            propertyInfo.SetValue(strategy, oldPropertyInfo.GetValue(oldStrategy));
                        }
                    }

                    strategy.OptimizationFitnessType = OptimizationFitnessTypes.FirstOrDefault(x => x.ObjectType.Name == oldStrategy.OptimizationFitnessType.ObjectType.Name);
                    strategy.CreateOptimizationFitness();
                    strategy.OptimizerType = OptimizerStrategyTypes.FirstOrDefault(x => x.ObjectType.Name == oldStrategy.OptimizerType.ObjectType.Name);
                    strategy.CreateOptimizer(false);

                    if (strategy.Optimizer != null && oldStrategy.Optimizer != null)
                    {
                        var propertyInfos = oldStrategy.Optimizer.GetType().GetProperties()
                            .Where(x => Attribute.IsDefined(x, typeof(DataMemberAttribute)));

                        var propertyInfos2 = strategy.Optimizer.GetType().GetProperties()
                            .Where(x => Attribute.IsDefined(x, typeof(DataMemberAttribute))).ToList();


                        foreach (var pi in propertyInfos)
                        {
                            if (pi.SetMethod != null)
                            {
                                var value = pi.GetValue(oldStrategy.Optimizer);

                                var pi2 = propertyInfos2.FirstOrDefault(x => x.Name == pi.Name);
                                if (pi2 != null)
                                {
                                    pi2.SetValue(strategy.Optimizer, value);
                                }
                            }
                        }


                        foreach (var parameter in strategy.Optimizer.OptimizerParameters)
                        {
                            var oldParameter =
                                oldStrategy.Optimizer.OptimizerParameters.FirstOrDefault(x => x.Name == parameter.Name);
                            if (oldParameter != null)
                            {
                                oldParameter.CopyPreset(parameter);
                                parameter.IsChecked = oldParameter.IsChecked;
                            }
                        }

                    }



                    strategy.IsTemporary = oldStrategy.IsTemporary;
                    strategy.ViewModel = oldStrategy.ViewModel;                  


                    if (!strategy.IsTemporary)
                        Session.Instance.Strategies.Remove(oldStrategy);
                    var isEnabled = oldStrategy.Enabled;

                    oldStrategy.Clear();
                    oldStrategy.Stop();
                    

                    dataCalcContext.Strategies.Add(strategy);
                    if (!strategy.IsTemporary)
                        Session.Instance.Strategies.Add(strategy);

                    if (isEnabled)
                    {
                        strategy.Enabled = true;
                        if (strategy.IsTemporary)
                        {

                            if (dataCalcContext.Chart != null)
                            {
                                strategy.SetChart(dataCalcContext.Chart);
                                strategy.SynchronizeChart(false);
                            }

                            strategy.Start();
                        }
                    }
                    
                }
            }
        }


        public Type GetIndicatorType(string name)
        {
            var objectTypes = Instance.CustomAssembly.GetTypes()
                .Where(x => x.IsSubclassOf(_indicatorType));

            foreach (var objectType in objectTypes)
            {
                if (objectType.Name == name)
                    return objectType;
            }

            return null;
        }

        public Type GetStrategyType(string name)
        {
            var objectTypes = Instance.CustomAssembly.GetTypes()
                .Where(x => x.IsSubclassOf(_strategyType));

            foreach (var objectType in objectTypes)
            {
                if (objectType.Name == name)
                    return objectType;
            }

            return null;
        }


        public Type GetStrategyTypeByGuid(string guid)
        {
            var objectTypes = Instance.CustomAssembly.GetTypes()
                .Where(x => x.IsSubclassOf(_strategyType));

            foreach (var objectType in objectTypes)
            {
                var fields = objectType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance
                                                                         | BindingFlags.Public | BindingFlags.Static);
                var name = fields.FirstOrDefault(info =>
                    info.Name.Equals("StrategyGuid", StringComparison.OrdinalIgnoreCase));

                if (name != null)
                {
                    var value = name.GetValue(null).ToString();

                    if (value.ToLower() == guid.ToLower())
                        return objectType;
                }
            }

            return null;
        }


        public string GetStrategyNameByGuid(string guid)
        {
            var objectTypes = Instance.CustomAssembly.GetTypes()
                .Where(x => x.IsSubclassOf(_strategyType));

            foreach (var objectType in objectTypes)
            {
                var fields = objectType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance
                                                                         | BindingFlags.Public | BindingFlags.Static);
                var guidField = fields.FirstOrDefault(info =>
                    info.Name.Equals("StrategyGuid", StringComparison.OrdinalIgnoreCase));

                var nameField = fields.FirstOrDefault(info =>
                    info.Name.Equals("StrategyName", StringComparison.OrdinalIgnoreCase));

                if (guidField != null)
                {
                    var value = guidField.GetValue(null).ToString();

                    if (value.ToLower() == guid.ToLower() && nameField!= null)
                    {
                        return nameField.GetValue(null).ToString();
                    }
                }
            }

            return null;
        }

    }
}