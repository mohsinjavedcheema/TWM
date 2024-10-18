using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Twm.Core.Classes;
using Twm.Core.Controllers;
using Twm.DB.DAL;
using Twm.DB.DAL.Repositories.Connections;
using Twm.Model.Model;
using Twm.Properties;
using Twm.ViewModels;
using Twm.ViewModels.Accounts;
using Twm.ViewModels.Charts;
using Twm.ViewModels.Orders;
using Twm.ViewModels.Positions;
using Twm.ViewModels.Strategies;
using Twm.Windows;
using Newtonsoft.Json;
using Application = System.Windows.Application;
using Binding = System.Windows.Data.Binding;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.MessageBox;
using Twm.ViewModels.Assets;


namespace Twm
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static DbContextFactory DbContextFactory
        {
            get { return Session.Instance.DbContextFactory; }
        }

        public static NavigationViewModel Navigation { get; set; }


        public static StrategiesViewModel Strategies { get; set; }

        public static AccountsViewModel Accounts { get; set; }

        public static PositionsViewModel Positions { get; set; }

        public static AssetsViewModel Assets { get; set; }

        public static List<ChartViewModel> Charts { get; set; }

        public static OrdersViewModel Orders { get; set; }

        public App()
        {
            InitializeComponent();
            LogController.Init();

            Language = Settings.Default.DefaultLanguage;
            Navigation = new NavigationViewModel();
            Strategies = new StrategiesViewModel();
            Accounts = new AccountsViewModel();
            Positions = new PositionsViewModel();
            Assets = new AssetsViewModel();
            Orders = new OrdersViewModel();
            Charts = new List<ChartViewModel>();

            //CultureInfo ci = CultureInfo.CreateSpecificCulture(CultureInfo.CurrentCulture.Name);
            CultureInfo ci = CultureInfo.CreateSpecificCulture("ru-RU");
            ci.DateTimeFormat.ShortDatePattern = "dd.MM.yyyy";
            ci.NumberFormat.CurrencyNegativePattern = 1;
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
                LogUnhandledException((Exception) ex.ExceptionObject,
                    "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, ex) =>
            {
                LogUnhandledException(ex.Exception,
                    "Application.Current.DispatcherUnhandledException");
                ex.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, ex) =>
            {
                LogUnhandledException(ex.Exception,
                    "TaskScheduler.UnobservedTaskException");
                ex.SetObserved();
            };

            
            var splashScreen = new SplashScreenWindow();
            this.MainWindow = splashScreen;
            splashScreen.Show();


            IEnumerable<Connection> connections;
            SystemOptions.Instance.SaveTheme(theme =>
            {
                Settings.Default.SelectedTheme = theme.Name;
                Settings.Default.Save();
            });

            List<SystemOption> systemOptions;
            List<ViewOption> viewOptions;
            List<Setting> settings;
            List<InstrumentList> instrumentLists;

            using (var context = DbContextFactory.GetContext())
            {
                context.Migrate();
                systemOptions = context.SystemOptions.ToList();
                viewOptions = context.ViewOptions.ToList();
                settings = context.Settings.ToList();
                instrumentLists = context.InstrumentLists.ToList();
                var connectionRepository = new ConnectionRepository(context);
                connections = connectionRepository.GetAll().Result.ToList();
            }

            var themes = LoadThemes();

            themes.Add(new Theme() {Name = "Custom", Options = systemOptions});

            SystemOptions.Instance.Themes = new ObservableCollection<Theme>(themes);

            if (string.IsNullOrEmpty(Settings.Default.SelectedTheme))
            {
                Settings.Default.SelectedTheme = "Custom";
                Settings.Default.Save();
            }

            SystemOptions.Instance.Options = systemOptions;
            SystemOptions.Instance.SetViewOptions(viewOptions);


            SystemOptions.Instance.SelectedTheme =
                themes.FirstOrDefault(theme => theme.Name.Equals(Settings.Default.SelectedTheme));

            if (!SystemOptions.Instance.IsInit)
                SystemOptions.Instance.Init();

            Session.Instance.SetSettings(settings);
            Session.Instance.Init(connections);

            Orders.FillConnections();
            Positions.FillConnections();
            Assets.FillConnections();


            BuildController.Instance.LoadCustomAssembly();

            var mainWindow = new MainWindow();
            this.MainWindow = mainWindow;
            mainWindow.Show();
            splashScreen.Close();
            LogController.Print("Twm session start");
        }

        private List<Theme> LoadThemes()
        {
            var executingFolder = AppDomain.CurrentDomain.BaseDirectory;
            var themes = Directory.GetFiles(executingFolder, $"*.{SystemDefaultValues.ThemeExtension}");
            return themes.Select(file => JsonConvert.DeserializeObject<Theme>(File.ReadAllText(file))).ToList();
        }


        private void LogUnhandledException(Exception e, string @event)
        {
            string errorMessage = string.Format("An unhandled exception occurred: {0}", e.Message);
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            if (e.StackTrace != null)
            {
                errorMessage = errorMessage + "\r\n" + e.StackTrace;
            }

            if (e.InnerException != null)
            {
                errorMessage = errorMessage + "\r\n" + e.InnerException.Message;
                if (e.InnerException.StackTrace != null)
                {
                    errorMessage = errorMessage + "\r\n" + e.InnerException.StackTrace;
                }
            }

            LogController.Print(errorMessage);
        }


        public static void CreateConnectionsMenu(Window window = null)
        {
            if (window == null)
                window = App.Current.MainWindow;

            var menuItem = (MenuItem) window.FindName("miConnections");
            if (menuItem != null)
            {
                menuItem.Items.Clear();
                var toggleStyle = App.Current.FindResource("MenuItemToggleStyle");


                var maxConnectionNameWidth = 0.0;
                var orderedConnections = Session.Instance.ConfiguredConnections.OrderBy(x => x.Order);
                foreach (var connection in orderedConnections)
                {
                    var newItem = new MenuItem {Header = connection, Style = (Style)toggleStyle };
                    //newItem.SetBinding(MenuItem.CommandProperty, new Binding("StartStopConnectionCommand"));
                    newItem.CommandParameter = connection.Name;
                    menuItem.Items.Add(newItem);


                    var formattedText = new FormattedText(
                        connection.Name,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface(App.Current.MainWindow.FontFamily, App.Current.MainWindow.FontStyle, App.Current.MainWindow.FontWeight, App.Current.MainWindow.FontStretch),
                        App.Current.MainWindow.FontSize,
                        Brushes.Black,
                        new NumberSubstitution(),
                        1);


                    maxConnectionNameWidth = formattedText.Width > maxConnectionNameWidth
                        ? maxConnectionNameWidth = formattedText.Width
                        : maxConnectionNameWidth;
                }

                var configureItem = new MenuItem {Header = "Configure..."};
                configureItem.SetBinding(MenuItem.CommandProperty, new Binding("ConfigureConnectionCommand"));
                menuItem.Items.Add(configureItem);

                foreach (var connection in orderedConnections)
                {
                    connection.MaxConnectionNameWidth = new GridLength(maxConnectionNameWidth);
                }


            }
        }


        public static event EventHandler LanguageChanged;

        public static CultureInfo Language
        {
            get { return System.Threading.Thread.CurrentThread.CurrentUICulture; }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                //if (Equals(value, System.Threading.Thread.CurrentThread.CurrentUICulture)) return;

                //1. Меняем язык приложения:
                System.Threading.Thread.CurrentThread.CurrentUICulture = value;
                System.Threading.Thread.CurrentThread.CurrentCulture = value;

                //2. Создаём ResourceDictionary для новой культуры
                ResourceDictionary dict = new ResourceDictionary();
                switch (value.Name)
                {
                    case "ru-RU":
                        dict.Source = new Uri($"Resources/lang.{value.Name}.xaml", UriKind.Relative);
                        break;
                    case "en-US":
                        dict.Source = new Uri($"Resources/lang.{value.Name}.xaml", UriKind.Relative);
                        break;
                    default:
                        dict.Source = new Uri("Resources/lang.en-US.xaml", UriKind.Relative);
                        break;
                }

                //3. Находим старую ResourceDictionary и удаляем его и добавляем новую ResourceDictionary
                ResourceDictionary oldDict = null;
                if (Current.Resources.MergedDictionaries.Count > 0)
                    oldDict = (from d in Current.Resources.MergedDictionaries
                        where d.Source != null && d.Source.OriginalString.StartsWith("Resources/lang.")
                        select d).First();
                if (oldDict != null)
                {
                    int ind = Current.Resources.MergedDictionaries.IndexOf(oldDict);
                    Current.Resources.MergedDictionaries.Remove(oldDict);
                    Current.Resources.MergedDictionaries.Insert(ind, dict);
                }
                else
                {
                    Current.Resources.MergedDictionaries.Add(dict);
                }

                //4. Вызываем евент для оповещения всех окон.
                LanguageChanged?.Invoke(Current, new EventArgs());
            }
        }


       
    }
}