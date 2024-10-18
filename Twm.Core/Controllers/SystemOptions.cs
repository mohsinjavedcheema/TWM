using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Media;
using Twm.Core.Annotations;
using Twm.Core.Classes;
using Twm.Core.Enums;
using Twm.Core.Helpers;
using Twm.Model.Model;


namespace Twm.Core.Controllers
{
    public class SystemOptions : INotifyPropertyChanged
    {
        #region Project options

        private SystemOption _pathToProjectSystemOption;
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
                }
            }
        }


        private SystemOption _pathToProjectDllSystemOption;
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
                }
            }
        }


        private SystemOption _compileDebugSystemOption;
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
                }
            }
        }


        private SystemOption __reloadOnRecompileOption;
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
                }
            }
        }

        #endregion

        #region Display options

        private SystemOption _tradeBuyColorSystemOption;
        private Color _tradeBuyColor;

        public Color TradeBuyColor
        {
            get { return _tradeBuyColor; }
            set
            {
                if (_tradeBuyColor != value)
                {
                    _tradeBuyColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private SystemOption _markerTextColorSystemOption;
        private Color _markerTextColor;

        public Color MarkerTextColor
        {
            get { return _markerTextColor; }
            set
            {
                if (_markerTextColor != value)
                {
                    _markerTextColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private SystemOption _tradeSellColorSystemOption;
        private Color _tradeSellColor;

        public Color TradeSellColor
        {
            get { return _tradeSellColor; }
            set
            {
                if (_tradeSellColor != value)
                {
                    _tradeSellColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private SystemOption _plotExecutionsSystemOption;
        private PlotExecutions _plotExecutions;

        public PlotExecutions PlotExecutions
        {
            get { return _plotExecutions; }
            set
            {
                if (_plotExecutions != value)
                {
                    _plotExecutions = value;
                    OnPropertyChanged();
                }
            }
        }


        private SystemOption _downBarColorSystemOption;
        private Color _downBarColor;

        public Color DownBarColor
        {
            get { return _downBarColor; }
            set
            {
                if (_downBarColor != value)
                {
                    _downBarColor = value;
                    OnPropertyChanged();
                }
            }
        }


        private SystemOption _upBarColorSystemOption;
        private Color _upBarColor;

        public Color UpBarColor
        {
            get { return _upBarColor; }
            set
            {
                if (_upBarColor != value)
                {
                    _upBarColor = value;
                    OnPropertyChanged();
                }
            }
        }


        private SystemOption _candleOutlineColorSystemOption;
        private Color _candleOutlineColor;

        public Color CandleOutlineColor
        {
            get { return _candleOutlineColor; }
            set
            {
                if (_candleOutlineColor != value)
                {
                    _candleOutlineColor = value;
                    OnPropertyChanged();
                }
            }
        }


        private SystemOption _candleWickColorSystemOption;
        private Color _candleWickColor;

        public Color CandleWickColor
        {
            get { return _candleWickColor; }
            set
            {
                if (_candleWickColor != value)
                {
                    _candleWickColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private SystemOption _chartBackgroundColorSystemOption;
        private Color _chartBackgroundColor;

        public Color ChartBackgroundColor
        {
            get { return _chartBackgroundColor; }
            set
            {
                if (_chartBackgroundColor != value)
                {
                    _chartBackgroundColor = value;
                    OnPropertyChanged();
                }
            }
        }


        private SystemOption _chartVGridColorSystemOption;
        private Color _chartVGridColor;

        public Color ChartVGridColor
        {
            get { return _chartVGridColor; }
            set
            {
                if (_chartVGridColor != value)
                {
                    _chartVGridColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private SystemOption _chartHGridColorSystemOption;
        private Color _chartHGridColor;

        public Color ChartHGridColor
        {
            get { return _chartHGridColor; }
            set
            {
                if (_chartHGridColor != value)
                {
                    _chartHGridColor = value;
                    OnPropertyChanged();
                }
            }
        }


        private SystemOption _textColorSystemOption;
        private Color _textColor;

        public Color TextColor
        {
            get { return _textColor; }
            set
            {
                if (_textColor != value)
                {
                    _textColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private SystemOption _indicatorSeparatorColorSystemOption;
        private Color _indicatorSeparatorColor;

        public Color IndicatorSeparatorColor
        {
            get { return _indicatorSeparatorColor; }
            set
            {
                if (_indicatorSeparatorColor != value)
                {
                    _indicatorSeparatorColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private SystemOption _indicatorSeparatorWidthSystemOption;
        private int _indicatorSeparatorWidth;

        public int IndicatorSeparatorWidth
        {
            get { return _indicatorSeparatorWidth; }
            set
            {
                if (_indicatorSeparatorWidth != value)
                {
                    _indicatorSeparatorWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Data options

        private HistoricalData _selectedSymbol;

        public HistoricalData SelectedSymbol
        {
            get { return _selectedSymbol; }
            set
            {
                if (_selectedSymbol != value)
                {
                    _selectedSymbol = value;
                    OnPropertyChanged();
                }
            }
        }


        private ObservableCollection<HistoricalData> _providers = new ObservableCollection<HistoricalData>();

        public ObservableCollection<HistoricalData> Providers
        {
            get => _providers;
            set
            {
                if (Equals(value, _providers)) return;
                _providers = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Preferences

        private SystemOption _timeZoneSystemOption;
        private TimeZoneInfo _timeZone;

        public TimeZoneInfo TimeZone
        {
            get { return _timeZone; }
            set
            {
                if (_timeZone != value)
                {
                    _timeZone = value;
                    OnPropertyChanged();
                }
            }
        }


        private SystemOption _logInFileSystemOption;
        private bool _logInFile;

        public bool LogInFile
        {
            get { return _logInFile; }
            set
            {
                if (_logInFile != value)
                {
                    _logInFile = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Calculation

        private SystemOption _calculateSimulationSystemOption;
        private bool _calculateSimulation;

        public bool CalculateSimulation
        {
            get { return _calculateSimulation; }
            set
            {
                if (_calculateSimulation != value)
                {
                    _calculateSimulation = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Email

        private SystemOption _emailHostSystemOption;
        private string _emailHost;

        public string EmailHost
        {
            get { return _emailHost; }
            set
            {
                if (_emailHost != value)
                {
                    _emailHost = value;
                    OnPropertyChanged();
                }
            }
        }

        private SystemOption _emailPortSystemOption;
        private int _emailPort;

        public int EmailPort
        {
            get { return _emailPort; }
            set
            {
                if (_emailPort != value)
                {
                    _emailPort = value;
                    OnPropertyChanged();
                }
            }
        }


        private SystemOption _emailUsernameSystemOption;
        private string _emailUsername;

        public string EmailUsername
        {
            get { return _emailUsername; }
            set
            {
                if (_emailUsername != value)
                {
                    _emailUsername = value;
                    OnPropertyChanged();
                }
            }
        }

        private SystemOption _emailPasswordSystemOption;
        private string _emailPassword;

        public string EmailPassword
        {
            get { return _emailPassword; }
            set
            {
                if (_emailPassword != value)
                {
                    _emailPassword = value;
                    OnPropertyChanged();
                }
            }
        }


        private string _testEmailTo;

        public string TestEmailTo
        {
            get { return _testEmailTo; }
            set
            {
                if (_testEmailTo != value)
                {
                    _testEmailTo = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region API

        private SystemOption _apiKeyOption;
        private string _apiKey;

        public string ApiKey
        {
            get { return _apiKey; }
            set
            {
                if (_apiKey != value)
                {
                    _apiKey = value;
                    OnPropertyChanged();
                }
            }
        }


        private SystemOption _apiUserNameOption;
        private string _apiUserName;

        public string ApiUserName
        {
            get { return _apiUserName; }
            set
            {
                if (_apiUserName != value)
                {
                    _apiUserName = value;
                    OnPropertyChanged();
                }
            }
        }


        private SystemOption _apiPasswordOption;
        private string _apiPassword;

        public string ApiPassword
        {
            get { return _apiPassword; }
            set
            {
                if (_apiPassword != value)
                {
                    _apiPassword = value;
                    OnPropertyChanged();
                }
            }
        }


        private SystemOption _apiServerUrlOption;
        private string _apiServerUrl;

        public string ApiServerUrl
        {
            get { return _apiServerUrl; }
            set
            {
                if (_apiServerUrl != value)
                {
                    _apiServerUrl = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        private string _selectedCategory;

        public string SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                if (value != _selectedCategory)
                {
                    _selectedCategory = value;
                    OnPropertyChanged();
                }
            }
        }


        private string _selectedGroup;
        private ObservableCollection<Theme> _themes;

        public string SelectedGroup
        {
            get { return _selectedGroup; }
            set
            {
                if (value != _selectedGroup)
                {
                    _selectedGroup = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<Theme> Themes
        {
            get => _themes;
            set
            {
                if (Equals(value, _themes)) return;
                _themes = value;
                OnPropertyChanged();
            }
        }

        public Theme SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (Equals(value, _selectedTheme)) return;
                _selectedTheme = value;
                if (_saveTheme != null && _selectedTheme != null)
                {
                    _saveTheme.Invoke(_selectedTheme);

                    Init();
                }

                OnPropertyChanged();
            }
        }

        public OperationCommand RemoveAllCommand { get; set; }
        public OperationCommand RemoveCommand { get; set; }
        public OperationCommand TestMailCommand { get; set; }
        public OperationCommand GetTokenCommand { get; set; }


        public ReadOnlyCollection<TimeZoneInfo> TimeZones { get; set; }


        private static SystemOptions _mInstance;
        private Action<Theme> _saveTheme;
        private Theme _selectedTheme;


        public static SystemOptions Instance
        {
            get { return _mInstance ?? (_mInstance = new SystemOptions()); }
        }

        public List<SystemOption> Options { get; set; }


        public bool IsInit { get; set; }


        public Dictionary<string, GridColumnCustomization> ViewOptions { get; set; }

        public OperationCommand SelectProjectPathCommand { get; set; }

        public OperationCommand SelectProjectPathDllCommand { get; set; }

        public SystemOptions()
        {
            Options = new List<SystemOption>();
            SelectProjectPathCommand = new OperationCommand(SelectProjectPath);
            SelectProjectPathDllCommand = new OperationCommand(SelectProjectPathDll);
            TestMailCommand = new OperationCommand(TestMail);
           
            TimeZones = TimeZoneInfo.GetSystemTimeZones();
            ViewOptions = new Dictionary<string, GridColumnCustomization>();
            IsInit = false;
        }

        private async void TestMail(object obj)
        {
            if (!string.IsNullOrEmpty(TestEmailTo))
            {
                try
                {
                    await EmailService.Instance.SendEmail(TestEmailTo, "Twm test", "Test message");
                    MessageBox.Show("Test successful!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Test unsuccessfull: " + ex.Message);
                }
            }
        }

        

        private void RemoveSymbol(object obj)
        {
        }

        private void SelectProjectPath(object obj)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = PathToProject;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    PathToProject = dialog.SelectedPath;
                }
            }
        }

        private void SelectProjectPathDll(object obj)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = PathToProjectDll;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    PathToProjectDll = dialog.SelectedPath;
                }
            }
        }


        public void InitTestOptions()
        {
            List<SystemOption> systemOptions;
            using (var context = Session.Instance.DbContextFactory.GetContext())
            {
                systemOptions = context.SystemOptions.ToList();
            }
            var themes = new List<Theme>() { new Theme() { Name = "Custom", Options = systemOptions } };

            Themes = new ObservableCollection<Theme>(themes);
            Options = systemOptions;
            SelectedTheme = themes.FirstOrDefault();
            Init();
        }


        public void Init()
        {
            var customTheme =
                Themes.FirstOrDefault(theme => theme.Name.Equals(@"custom", StringComparison.OrdinalIgnoreCase));

            if (SelectedTheme == null)
            {
                _selectedTheme = customTheme;
            }

            if (SelectedTheme != null && SelectedTheme != customTheme)
            {
                foreach (var option in Options)
                {
                    var themeOption = SelectedTheme.Options.FirstOrDefault(x => x.Code == option.Code);
                    if (themeOption != null)
                        option.Value = themeOption.Value;
                }
            }

            #region Project options

            _pathToProjectSystemOption = OptionByCodeWithCustom(customTheme, "CstPrjPath");

            if (_pathToProjectSystemOption != null)
            {
                PathToProject = _pathToProjectSystemOption.Value;
            }

            if (string.IsNullOrEmpty(PathToProject))
            {
                PathToProject = SystemDefaultValues.PathToProject;
                Directory.CreateDirectory(PathToProject);
            }

            _pathToProjectDllSystemOption = OptionByCodeWithCustom(customTheme, "CstPrjDllPath");

            if (_pathToProjectDllSystemOption != null)
            {
                PathToProjectDll = _pathToProjectDllSystemOption.Value;
            }

            if (string.IsNullOrEmpty(PathToProjectDll))
            {
                PathToProjectDll = SystemDefaultValues.PathToProjectDll;
                Directory.CreateDirectory(PathToProjectDll);
            }

            _compileDebugSystemOption = OptionByCodeWithCustom(customTheme, "CompileDebug");

            if (_compileDebugSystemOption != null)
            {
                CompileDebug = _compileDebugSystemOption.ValueBool;
            }

            __reloadOnRecompileOption = OptionByCodeWithCustom(customTheme, "ReloadOnRecompile");

            if (__reloadOnRecompileOption != null)
            {
                ReloadOnRecompile = __reloadOnRecompileOption.ValueBool;
            }

            #endregion

            #region Display settings

            _tradeBuyColorSystemOption = OptionByCodeWithCustom(customTheme, "TradeBuyColor");
            if (_tradeBuyColorSystemOption != null)
            {
                TradeBuyColor = StringToColor(_tradeBuyColorSystemOption.Value, "TradeBuyColor",
                    SystemDefaultValues.TradeBuyColor);
            }

            _markerTextColorSystemOption = OptionByCodeWithCustom(customTheme, "MarkerTextColor");
            if (_markerTextColorSystemOption != null)
            {
                MarkerTextColor = StringToColor(_markerTextColorSystemOption.Value, "MarkerTextColor",
                    SystemDefaultValues.MarkerTextColor);
            }


            _tradeSellColorSystemOption = OptionByCodeWithCustom(customTheme, "TradeSellColor");
            if (_tradeSellColorSystemOption != null)
            {
                TradeSellColor = StringToColor(_tradeSellColorSystemOption.Value, "TradeSellColor",
                    SystemDefaultValues.TradeSellColor);
            }

            _plotExecutionsSystemOption = OptionByCodeWithCustom(customTheme, "PlotExecutions");
            if (_plotExecutionsSystemOption != null)
            {
                PlotExecutions = (PlotExecutions) StringToInt(_plotExecutionsSystemOption.Value, "PlotExecutions",
                    (int) SystemDefaultValues.PlotExecutions);
            }

            _downBarColorSystemOption = OptionByCodeWithCustom(customTheme, "DownBarColor");
            if (_downBarColorSystemOption != null)
            {
                DownBarColor = StringToColor(_downBarColorSystemOption.Value, "DownBarColor",
                    SystemDefaultValues.DownBarColor);
            }

            _indicatorSeparatorColorSystemOption = OptionByCodeWithCustom(customTheme, "IndicatorSeparatorColor");
            if (_indicatorSeparatorColorSystemOption != null)
            {
                IndicatorSeparatorColor = StringToColor(_indicatorSeparatorColorSystemOption.Value,
                    "IndicatorSeparatorColor", SystemDefaultValues.IndicatorSeparatorColor);
            }

            _indicatorSeparatorWidthSystemOption = OptionByCodeWithCustom(customTheme, "IndicatorSeparatorWidth");
            if (_indicatorSeparatorWidthSystemOption != null)
            {
                IndicatorSeparatorWidth = StringToInt(_indicatorSeparatorWidthSystemOption.Value,
                    "IndicatorSeparatorWidth", SystemDefaultValues.IndicatorSeparatorWidth);
            }

            _upBarColorSystemOption = OptionByCodeWithCustom(customTheme, "UpBarColor");
            if (_upBarColorSystemOption != null)
            {
                UpBarColor = StringToColor(_upBarColorSystemOption.Value, "UpBarColor", SystemDefaultValues.UpBarColor);
            }

            _candleOutlineColorSystemOption = OptionByCodeWithCustom(customTheme, "CandleOutlineColor");
            if (_candleOutlineColorSystemOption != null)
            {
                CandleOutlineColor = StringToColor(_candleOutlineColorSystemOption.Value, "CandleOutlineColor",
                    SystemDefaultValues.CandleOutlineColor);
            }

            _candleWickColorSystemOption = OptionByCodeWithCustom(customTheme, "CandleWickColor");
            if (_candleWickColorSystemOption != null)
            {
                CandleWickColor = StringToColor(_candleWickColorSystemOption.Value, "CandleWickColor",
                    SystemDefaultValues.CandleWickColor);
            }

            _textColorSystemOption = OptionByCodeWithCustom(customTheme, "TextColor");
            if (_textColorSystemOption != null)
            {
                TextColor = StringToColor(_textColorSystemOption.Value, "TextColor", SystemDefaultValues.TextColor);
            }

            _chartBackgroundColorSystemOption = OptionByCodeWithCustom(customTheme, "ChartBackgroundColor");
            if (_chartBackgroundColorSystemOption != null)
            {
                ChartBackgroundColor = StringToColor(_chartBackgroundColorSystemOption.Value, "ChartBackgroundColor",
                    SystemDefaultValues.ChartBackgroundColor);
            }

            _chartVGridColorSystemOption = OptionByCodeWithCustom(customTheme, "ChartVGridColor");
            if (_chartVGridColorSystemOption != null)
            {
                ChartVGridColor = StringToColor(_chartVGridColorSystemOption.Value, "ChartVGridColor",
                    SystemDefaultValues.ChartVGridColor);
            }

            _chartHGridColorSystemOption = OptionByCodeWithCustom(customTheme, "ChartHGridColor");
            if (_chartHGridColorSystemOption != null)
            {
                ChartHGridColor = StringToColor(_chartHGridColorSystemOption.Value, "ChartHGridColor",
                    SystemDefaultValues.ChartHGridColor);
            }

            #endregion

            #region Preferences

            _timeZoneSystemOption = OptionByCodeWithCustom(customTheme, "TimeZone");

            if (_timeZoneSystemOption != null && !string.IsNullOrEmpty(_timeZoneSystemOption.Value))
            {
                try
                {
                    TimeZone = TimeZoneInfo.FindSystemTimeZoneById(_timeZoneSystemOption.Value);
                }
                catch
                {
                }
            }

            if (TimeZone == null)
            {
                TimeZone = TimeZoneInfo.Local;
            }

            _logInFileSystemOption = OptionByCodeWithCustom(customTheme, "LogInFile");

            if (_logInFileSystemOption != null)
            {
                LogInFile = _logInFileSystemOption.ValueBool;
            }

            #endregion

            #region Calculation optimizer

            _calculateSimulationSystemOption = OptionByCodeWithCustom(customTheme, "CalculateSimulation");

            if (_calculateSimulationSystemOption != null)
            {
                CalculateSimulation = _calculateSimulationSystemOption.ValueBool;
            }

            #endregion

            #region Email

            _emailHostSystemOption = OptionByCodeWithCustom(customTheme, "EmailHost");

            if (_emailHostSystemOption != null)
            {
                EmailHost = _emailHostSystemOption.Value;
            }

            _emailPortSystemOption = OptionByCodeWithCustom(customTheme, "EmailPort");
            if (_emailPortSystemOption != null)
            {
                EmailPort = _emailPortSystemOption.ValueInt;
            }

            _emailUsernameSystemOption = OptionByCodeWithCustom(customTheme, "EmailUsername");
            if (_emailUsernameSystemOption != null)
            {
                EmailUsername = _emailUsernameSystemOption.Value;
            }


            _emailPasswordSystemOption = OptionByCodeWithCustom(customTheme, "EmailPassword");
            if (_emailPasswordSystemOption != null)
            {
                EmailPassword = _emailPasswordSystemOption.Value;
            }

            #endregion

            #region ServerApi

            _apiServerUrlOption = OptionByCodeWithCustom(customTheme, "ApiServerUrl");

            if (_apiServerUrlOption != null)
            {
                ApiServerUrl = _apiServerUrlOption.Value;
            }

            _apiUserNameOption = OptionByCodeWithCustom(customTheme, "ApiUserName");
            if (_apiUserNameOption != null)
            {
                ApiUserName = _apiUserNameOption.Value;
            }

            _apiPasswordOption = OptionByCodeWithCustom(customTheme, "ApiPassword");
            if (_apiPasswordOption != null)
            {
                ApiPassword = _apiPasswordOption.Value;
            }

            #endregion

            IsInit = true;
        }

        public void InitHistoricalData(List<HistoricalMetaData> historicalMetaDatas)
        {
            Providers = HistoricalData.InitFromHistoricalDatas(historicalMetaDatas);
        }

        private SystemOption OptionByCodeWithCustom(Theme custom, string code)
        {
            var option = Options.FirstOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase)) ??
                         custom.Options.FirstOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            return option;
        }


        private Color StringToColor(string value, string settingName, Color defaultValue)
        {
            try
            {
                var obj = ColorConverter.ConvertFromString(value);

                if (obj != null)
                    return (Color) obj;

                LogController.Print($"Invalid or empty system setting {settingName}");
                return defaultValue;
            }
            catch
            {
                LogController.Print($"Invalid system setting {settingName}");
                return defaultValue;
            }
        }

        private int StringToInt(string value, string settingName, int defaultValue)
        {
            try
            {
                var obj = int.Parse(value);

                return obj;
            }
            catch
            {
                LogController.Print($"Invalid system setting {settingName}");
                return defaultValue;
            }
        }


        public IEnumerable<string> SyncOptions()
        {
            List<string> changedOptions = new List<string>();

            CheckOptionChange(_pathToProjectSystemOption, PathToProject, changedOptions);
            CheckOptionChange(_pathToProjectDllSystemOption, PathToProjectDll, changedOptions);
            CheckOptionChange(_compileDebugSystemOption, CompileDebug, changedOptions);
            CheckOptionChange(__reloadOnRecompileOption, ReloadOnRecompile, changedOptions);
            CheckOptionChange(_plotExecutionsSystemOption, ((int) PlotExecutions).ToString(), changedOptions);

            CheckOptionChange(_timeZoneSystemOption, TimeZone, changedOptions);
            CheckOptionChange(_logInFileSystemOption, LogInFile, changedOptions);
            CheckOptionChange(_emailHostSystemOption, EmailHost, changedOptions);
            CheckOptionChange(_emailPortSystemOption, EmailPort, changedOptions);
            CheckOptionChange(_emailUsernameSystemOption, EmailUsername, changedOptions);
            CheckOptionChange(_emailPasswordSystemOption, EmailPassword, changedOptions);

            CheckOptionChange(_apiServerUrlOption, ApiServerUrl, changedOptions);
            CheckOptionChange(_apiUserNameOption, ApiUserName, changedOptions);
            CheckOptionChange(_apiPasswordOption, ApiPassword, changedOptions);

            if (SelectedTheme != null && SelectedTheme.Name.Equals("custom", StringComparison.OrdinalIgnoreCase))
            {
                CheckOptionChange(_tradeSellColorSystemOption, TradeSellColor.ToString(), changedOptions);
                CheckOptionChange(_tradeBuyColorSystemOption, TradeBuyColor.ToString(), changedOptions);
                CheckOptionChange(_markerTextColorSystemOption, MarkerTextColor.ToString(), changedOptions);

                CheckOptionChange(_downBarColorSystemOption, DownBarColor.ToString(), changedOptions);
                CheckOptionChange(_upBarColorSystemOption, UpBarColor.ToString(), changedOptions);
                CheckOptionChange(_candleOutlineColorSystemOption, CandleOutlineColor.ToString(), changedOptions);
                CheckOptionChange(_candleWickColorSystemOption, CandleWickColor.ToString(), changedOptions);

                CheckOptionChange(_textColorSystemOption, TextColor.ToString(), changedOptions);
                CheckOptionChange(_indicatorSeparatorColorSystemOption, IndicatorSeparatorColor.ToString(),
                    changedOptions);
                CheckOptionChange(_indicatorSeparatorWidthSystemOption, IndicatorSeparatorWidth.ToString(),
                    changedOptions);

                CheckOptionChange(_chartBackgroundColorSystemOption, ChartBackgroundColor.ToString(), changedOptions);
                CheckOptionChange(_chartVGridColorSystemOption, ChartVGridColor.ToString(), changedOptions);
                CheckOptionChange(_chartHGridColorSystemOption, ChartHGridColor.ToString(), changedOptions);
            }

            CheckOptionChange(_calculateSimulationSystemOption, CalculateSimulation, changedOptions);


            if (changedOptions.Contains("PathToProject") || changedOptions.Contains("PathToProjectDll"))
                BuildController.Instance.CompileProject();


         


            return changedOptions;
        }

        private void CheckOptionChange(SystemOption option, object value, List<string> changedOption)
        {
            if (value is string strValue)
            {
                if (option.Value != strValue)
                {
                    option.Value = strValue;
                    changedOption.Add(option.Code);
                }
            }
            else if (value is bool boolValue)
            {
                if (option.ValueBool != boolValue)
                {
                    option.ValueBool = boolValue;
                    changedOption.Add(option.Code);
                }
            }
            else if (value is int intValue)
            {
                if (option.ValueInt != intValue)
                {
                    option.ValueInt = intValue;
                    changedOption.Add(option.Code);
                }
            }
            else if (value is TimeZoneInfo timeZoneInfo)
            {
                if (option.Value != timeZoneInfo.Id)
                {
                    option.Value = timeZoneInfo.Id;
                    changedOption.Add(option.Code);
                }
            }
        }

        public void ResetOptions()
        {
            PathToProject = _pathToProjectSystemOption.Value;
            PathToProjectDll = _pathToProjectDllSystemOption.Value;
            CompileDebug = _compileDebugSystemOption.ValueBool;
            ReloadOnRecompile = __reloadOnRecompileOption.ValueBool;

            TradeBuyColor = StringToColor(_tradeBuyColorSystemOption.Value, "TradeBuyColor",
                SystemDefaultValues.TradeBuyColor);
            MarkerTextColor = StringToColor(_markerTextColorSystemOption.Value, "MarkerTextColor",
                SystemDefaultValues.MarkerTextColor);

            TradeSellColor = StringToColor(_tradeSellColorSystemOption.Value, "TradeSellColor",
                SystemDefaultValues.TradeSellColor);
            PlotExecutions = (PlotExecutions) StringToInt(_plotExecutionsSystemOption.Value, "PlotExecutions",
                (int) SystemDefaultValues.PlotExecutions);

            DownBarColor = StringToColor(_downBarColorSystemOption.Value, "DownBarColor",
                SystemDefaultValues.DownBarColor);
            UpBarColor = StringToColor(_upBarColorSystemOption.Value, "UpBarColor", SystemDefaultValues.UpBarColor);
            CandleOutlineColor = StringToColor(_candleOutlineColorSystemOption.Value, "CandleOutlineColor",
                SystemDefaultValues.CandleOutlineColor);
            CandleWickColor = StringToColor(_candleWickColorSystemOption.Value, "CandleWickColor",
                SystemDefaultValues.CandleWickColor);
            ChartBackgroundColor = StringToColor(_chartBackgroundColorSystemOption.Value, "ChartBackgroundColor",
                SystemDefaultValues.ChartBackgroundColor);

            ChartVGridColor = StringToColor(_chartVGridColorSystemOption.Value, "ChartVGridColor",
                SystemDefaultValues.ChartVGridColor);
            ChartHGridColor = StringToColor(_chartHGridColorSystemOption.Value, "ChartHGridColor",
                SystemDefaultValues.ChartHGridColor);

            IndicatorSeparatorColor = StringToColor(_indicatorSeparatorColorSystemOption.Value,
                "IndicatorSeparatorColor", SystemDefaultValues.IndicatorSeparatorColor);
            IndicatorSeparatorWidth = StringToInt(_indicatorSeparatorWidthSystemOption.Value, "IndicatorSeparatorWidth",
                SystemDefaultValues.IndicatorSeparatorWidth);
            TextColor = StringToColor(_textColorSystemOption.Value, "TextColor", SystemDefaultValues.TextColor);


            TimeZone = !string.IsNullOrEmpty(_timeZoneSystemOption.Value)
                ? TimeZoneInfo.FindSystemTimeZoneById(_timeZoneSystemOption.Value)
                : TimeZoneInfo.Local;

            LogInFile = _logInFileSystemOption.ValueBool;

            CalculateSimulation = _calculateSimulationSystemOption.ValueBool;

            EmailHost = _emailHostSystemOption.Value;
            EmailPort = _emailPortSystemOption.ValueInt;
            EmailUsername = _emailUsernameSystemOption.Value;
            EmailPassword = _emailPasswordSystemOption.Value;

            //ApiServerUrl = _apiServerUrlOption.Value;
        }


        public IEnumerable<SystemOption> GetOptions(string categoryName = null, string groupName = null)
        {
            if (string.IsNullOrEmpty(categoryName) && string.IsNullOrEmpty(groupName))
                return Options;

            if (string.IsNullOrEmpty(categoryName))
                return Options.Where(x => x.Group == groupName);

            if (string.IsNullOrEmpty(groupName))
                return Options.Where(x => x.Category == categoryName);

            return Options.Where(x =>
                x.Category == categoryName && x.Group == groupName);
        }

        public void SaveTheme(Action<Theme> saveTheme)
        {
            _saveTheme = saveTheme;
        }

        public void SetViewOptions(List<ViewOption> viewOptions)
        {
            ViewOptions.Clear();

            foreach (var defaultViewOption in SystemDefaultValues.ViewOptions)
            {
                var viewOption = viewOptions.FirstOrDefault(x => x.Code == defaultViewOption.Key);

                GridColumnCustomization gridColumnCustomization;
                if (viewOption != null)
                {
                    gridColumnCustomization = JsonHelper.ToObject<GridColumnCustomization>(viewOption.Data);

                    foreach (var gridColumnInfo in defaultViewOption.Value)
                    {
                        var customColumn =
                            gridColumnCustomization.Columns.FirstOrDefault(x => x.Name == gridColumnInfo.Name);
                        if (customColumn == null)
                        {
                            gridColumnCustomization.Columns.Add(gridColumnInfo);
                        }
                        else
                        {
                            customColumn.Caption = gridColumnInfo.Caption;
                            if (customColumn.Width == 0)
                            {
                                customColumn.Width = gridColumnInfo.Width;
                            }
                        }
                    }
                }
                else
                {
                    gridColumnCustomization = new GridColumnCustomization();
                    foreach (var gridColumnInfo in defaultViewOption.Value)
                    {
                        gridColumnCustomization.Columns.Add(gridColumnInfo);
                    }
                }

                gridColumnCustomization.ViewOption = viewOption;
                gridColumnCustomization.Name = defaultViewOption.Key;

                ViewOptions.Add(defaultViewOption.Key, gridColumnCustomization);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}