using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;
using Twm.Chart.Interfaces;
using Twm.Core.Annotations;
using Twm.Core.Attributes;
using Twm.Core.Controllers;
using Twm.Core.DataCalc;
using Twm.Core.DataProviders.Common;
using Twm.Core.DataProviders.ExchangeMockUp;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.DataProviders.Playback;
using Twm.Core.Enums;
using Twm.Core.Helpers;
using Twm.Core.Interfaces;
using Twm.Core.Market;
using Twm.Core.ViewModels.Instruments;
using Twm.DB.DAL;
using Twm.DB.DAL.Repositories.Connections;
using Twm.DB.DAL.Repositories.Instruments;
using Twm.DB.DAL.Repositories.Settings;
using Twm.Model.Model;
using AutoMapper;
using Twm.Model.Model.Interfaces;



namespace Twm.Core.Classes
{
    public class Session : INotifyPropertyChanged
    {
        private static Session _mInstance;


        public static Session Instance
        {
            get { return _mInstance ?? (_mInstance = new Session()); }
        }

        public Dispatcher Dispatcher { get; set; }

        public ObservableCollection<DataCalcContext> DataCalcContexts { get; set; }

        public ObservableCollection<ConnectionBase> ConfiguredConnections { get; set; }

        public DataCalcContext CurrentDataCalcContext { get; set; }

        public ConcurrentDictionary<string, ConcurrentDictionary<string, Instrument>> InstrumentSpots { get; set; }

        public ConcurrentDictionary<string, ConcurrentDictionary<string, Instrument>> InstrumentFutures { get; set; }

        public ConcurrentDictionary<int, Instrument> InstrumentCache { get; set; }

        public ConcurrentDictionary<int, ConcurrentDictionary<int, int>> InstrumentMapCache { get; set; }

        public ObservableCollection<StrategyBase> Strategies { get; set; }

        public ObservableCollection<Market.Account> Accounts { get; set; }

        public ICollectionView ActiveAccounts { get; set; }

        public static readonly SemaphoreSlim DbSemaphoreSlim = new SemaphoreSlim(1, 1);

        public DbContextFactory DbContextFactory { get; set; }

        public Dictionary<string, string> Settings { get; set; }

        public event EventHandler<ConnectionStatusChangeEventArgs> OnConnectionStatusChanged;

        public event EventHandler<EventArgs> OnConnectionsUpdated;

        public Playback Playback { get; set; }

        public Dictionary<int, ExchangeMockUp> ExchangeMockUps { get; set; }

        public Dictionary<int, ConnectionBase> Connections { get; set; }

        public Dictionary<int, DataProvider> DataProviders { get; set; }


        public bool IsConnected
        {
            get { return Connections.Any(x => x.Value.IsConnected); }
        }

        public bool IsPlayback
        {
            get
            {
                if (Playback != null)
                {
                    return Playback.IsConnected;
                }

                return false;
            }
        }


        private CompileState _compileState;
        public CompileState CompileState
        {
            get { return _compileState; }
            set
            {
                if (_compileState != value)
                {
                    _compileState = value;
                    OnPropertyChanged();
                }
            }
        }

        private readonly IMapper _mapper;
        public IMapper Mapper
        {
            get { return _mapper; }
        }


        public SynchronizationContext UiContext { get; set; }







        protected Session()
        {

            DbContextFactory = new DbContextFactory();
            DbContextFactory.Init();

            // ConfiguredIndicators = new Dictionary<Chart.Classes.Chart, ObservableCollection<IndicatorBase>>();
            DataCalcContexts = new ObservableCollection<DataCalcContext>();
            ConfiguredConnections = new ObservableCollection<ConnectionBase>();
            Strategies = new ObservableCollection<StrategyBase>();
            Accounts = new ObservableCollection<Account>();
            ActiveAccounts = CollectionViewSource.GetDefaultView(Accounts);
            ActiveAccounts.Filter += ActiveAccountsViewFilter;

            InstrumentFutures = new ConcurrentDictionary<string, ConcurrentDictionary<string, Instrument>>();
            InstrumentSpots = new ConcurrentDictionary<string, ConcurrentDictionary<string, Instrument>>();
            InstrumentCache = new ConcurrentDictionary<int, Instrument>();
            InstrumentMapCache = new ConcurrentDictionary<int, ConcurrentDictionary<int, int>>();
            ExchangeMockUps = new Dictionary<int, ExchangeMockUp>();
            Connections = new Dictionary<int, ConnectionBase>();

            var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<IHistoricalCandle, ICandle>()
                        .ConstructUsing(x => new Chart.Classes.Candle())
                        .ForMember(dest => dest.t, opt => opt.MapFrom(src => src.Time))
                        .ForMember(dest => dest.ct, opt => opt.MapFrom(src => src.CloseTime))
                        .ForMember(dest => dest.V, opt => opt.MapFrom(src => src.Volume ?? 0))
                        .ForMember(dest => dest.O, opt => opt.MapFrom(src => src.Open ?? 0))
                        .ForMember(dest => dest.H, opt => opt.MapFrom(src => src.High ?? 0))
                        .ForMember(dest => dest.L, opt => opt.MapFrom(src => src.Low ?? 0))
                        .ForMember(dest => dest.C, opt => opt.MapFrom(src => src.Close ?? 0)).As<ICandle>();



                }
            );

            _mapper = config.CreateMapper();


        }


        private bool ActiveAccountsViewFilter(object item)
        {
            var account = item as Account;
            if (account == null)
                return false;



            if (!account.IsActive)
            {
                return false;
            }

            return true;
        }

        public void ConnectionStatusChanged(ConnectionStatus oldStatus, ConnectionStatus newStatus, int connectionId)
        {

            var connection = GetConnection(connectionId);

            LogController.Print(connection.Name + ": " + "Old status: " + oldStatus.Description() + "; New status: " + newStatus.Description());
            OnConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangeEventArgs(oldStatus, newStatus, connectionId, connection.Name));
            OnPropertyChanged("IsConnected");

        }

        public void ConnectionsUpdated()
        {
            OnConnectionsUpdated.Invoke(this, new EventArgs() );
        }

        public void Init(IEnumerable<Model.Model.Connection> connections)
        {
            var i = 0;
            var orderedConnections = connections.OrderBy(x => x.Id);
            foreach (var connection in orderedConnections)
            {
                var connectionType = GetConnectionFullyQualifiedName(connection.DataProvider.Code);
                var connectionOptionType = GetConnectionOptionsFullyQualifiedName(connection.DataProvider.Code);

                var configuredConnection = (ConnectionBase)GetInstance(connectionType);
                configuredConnection.DataModel = connection;
                configuredConnection.Options = (ConnectionOptionsBase)GetInstance(connectionOptionType);

                var connectionOptions = configuredConnection.Options;

                connectionOptions.Name = connection.Name;
                connectionOptions.DataProvider = connection.DataProvider.Code;

                var properties = configuredConnection.Options.GetType().GetProperties();

                foreach (var propertyInfo in properties)
                {
                    var attribute = propertyInfo.GetCustomAttributes(typeof(CustomConnectionOptionAttribute), true);

                    if (attribute.Length > 0)
                    {

                        var connectionOption = connection.Options.FirstOrDefault(x => x.Name == propertyInfo.Name);
                        if (connectionOption != null)
                        {
                            if (propertyInfo.PropertyType.IsEnum)
                            {
                                var enumValue = Enum.Parse(propertyInfo.PropertyType, connectionOption.Value);
                                propertyInfo.SetValue(configuredConnection.Options, enumValue);
                            }
                            else
                                propertyInfo.SetValue(configuredConnection.Options, connectionOption.Value);
                        }


                    }
                }
                configuredConnection.Order = i;
                Instance.ConfiguredConnections.Add(configuredConnection);
                Connections.Add(configuredConnection.Id, configuredConnection);
            }

            /* Playback = GetSettingObject<Playback>("Playback") ?? new Playback();
               Playback.Options = new PlaybackConnectionOptions();
               Playback.Order = 999;
               Playback.Name = "Playback connection";
               Instance.ConfiguredConnections.Add(Playback);
               Connections.Add(Playback.Id, Playback);*/


            using (var context = DbContextFactory.GetContext())
            {
                DataProviders = context.DataProviders.ToDictionary(x => x.Id, y => y);
            }
        }

        public static string GetConnectionFullyQualifiedName(string connectionName)
        {
            return "Twm.Core.DataProviders." + connectionName + "." + connectionName;
        }


        public static string GetConnectionOptionsFullyQualifiedName(string connectionName)
        {
            return "Twm.Core.DataProviders." + connectionName + "." + connectionName + "ConnectionOptions";
        }

        public static object GetInstance(string strFullyQualifiedName)
        {
            Type type = Type.GetType(strFullyQualifiedName);
            if (type != null)
                return Activator.CreateInstance(type);
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(strFullyQualifiedName);
                if (type != null)
                    return Activator.CreateInstance(type);
            }
            return null;
        }

        public static Type GetTypeByName(string strFullyQualifiedName)
        {
            Type type = Type.GetType(strFullyQualifiedName);
            if (type != null)
                return type;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(strFullyQualifiedName);
                if (type != null)
                    return type;
            }
            return null;
        }


        public IEnumerable<InstrumentListViewModel> GetInstrumentLists()
        {
            var result = new List<InstrumentListViewModel>();
            using (var context = DbContextFactory.GetContext())
            {
                var repository = new InstrumentListRepository(context);
                var instrumentLists = repository.GetAll().Result;

                foreach (var instrumentList in instrumentLists)
                {
                    result.Add(new InstrumentListViewModel(instrumentList));
                }
            }

            return result;
        }


        public async Task<Instrument> GetInstrument(string symbol, string type, string connectionCode)
        {
            ConcurrentDictionary<string, ConcurrentDictionary<string, Instrument>> instrumentsByType;
            ConcurrentDictionary<string, Instrument> connectionInstruments;
            Instrument instrument;

            if (type.ToUpper() == "FUTURE")
            {
                instrumentsByType = InstrumentFutures;
            }
            else
            {
                //SPOT
                instrumentsByType = InstrumentSpots;
            }

            if (instrumentsByType.TryGetValue(connectionCode, out connectionInstruments))
            {
                if (connectionInstruments != null)
                {
                    if (connectionInstruments.TryGetValue(symbol, out instrument))
                    {
                        return instrument;
                    }
                }
            }
            else
            {
                connectionInstruments = new ConcurrentDictionary<string, Instrument>();
                instrumentsByType.TryAdd(connectionCode, connectionInstruments);
            }

            var activeConnection = Connections.FirstOrDefault(x => x.Value.Code == connectionCode);
            instrument = await GetConnectionInstrument(activeConnection.Value.Id, symbol, type);

            if (instrument != null)
            {
                connectionInstruments.TryAdd(symbol, instrument);
            }
            return instrument;
        }


        public async Task<Instrument> GetConnectionInstrument(int connectionId, string symbol, string type)
        {
            await DbSemaphoreSlim.WaitAsync(new CancellationToken());
            try
            {

                using (var context = Instance.DbContextFactory.GetContext())
                {
                    var repository = new InstrumentRepository(context);
                    var instrument = repository
                        .FindBy(x => x.ConnectionId == connectionId && x.Symbol == symbol && x.Type == type)
                        .FirstOrDefault();

                    if (instrument == null)
                        LogController.Print(
                            $"Retrieve position information error. Can`t find native instrument {symbol} for connection {Session.Instance.GetConnection(connectionId).Name}");


                    return instrument;
                }


            }
            finally
            {
                DbSemaphoreSlim.Release(1);
            }
        }


        public async Task<Instrument> GetInstrument(int id)
        {
            if (InstrumentCache.TryGetValue(id, out var instrument))
            {
                if (instrument != null)
                    return instrument;
            }

            await DbSemaphoreSlim.WaitAsync(new CancellationToken());
            try
            {
                using (var context = DbContextFactory.GetContext())
                {
                    var repository = new InstrumentRepository(context);
                    instrument = repository.FindBy(x => x.Id == id).FirstOrDefault();

                    InstrumentCache.TryAdd(id, instrument);

                    return instrument;
                }
            }
            finally
            {
                DbSemaphoreSlim.Release(1);
            }
        }

        public void SetSettings(List<Setting> settings)
        {
            Settings = settings.ToDictionary(x => x.Code, y => y.Data);
        }

        public T GetSettingObject<T>(string code)
        {
            if (Settings.TryGetValue(code, out var data))
            {
                return JsonHelper.ToObject<T>(data);
            }

            return default(T);
        }


        public async void SaveSettingObject(string code, object obj)
        {
            using (var context = Session.Instance.DbContextFactory.GetContext())
            {
                var repository = new SettingRepository(context);
                var setting = await repository.GetByCode(code);

                var data = JsonHelper.ToJson(obj);
                if (Settings.ContainsKey(code))
                {
                    Settings[code] = data;
                    setting.Data = data;
                    await repository.Update(setting);

                }
                else
                {
                    Settings.Add(code, data);
                    setting = new Setting { Code = code, Data = data };
                    await repository.Add(setting);
                }
                await repository.CompleteAsync();
            }


        }





        public bool IsConnectionActive(int connectionId)
        {
            Connections.TryGetValue(connectionId, out var connection);
            if (connection != null)
                return connection.IsConnected;

            return false;
        }


        public IConnection GetConnection(int connectionId)
        {
            Connections.TryGetValue(connectionId, out var connection);
            return connection;
        }

        public IConnection GetConnection(string connectionCode)
        {
            
            return ConfiguredConnections.FirstOrDefault(x => x.Code == connectionCode);
        }

        public ExchangeMockUp GetMockup(IConnection connection)
        {
            if (!ExchangeMockUps.TryGetValue(connection.Id, out var mockup))
            {
                mockup = new ExchangeMockUp();
                ExchangeMockUps.Add(connection.Id, mockup);
            }

            return mockup;
        }

        /*public void AddConnection(IConnection connection)
        {
            Connections.Add(connection.Id, connection);
        }

        public void RemoveConnection(IConnection connection)
        {
            Connections.Remove(connection.Id);
        }*/
        public Account GetAccount(IConnection connection)
        {
            return Accounts.FirstOrDefault(x => x.Connection == connection);
        }


        public Account GetAccount(int connectionId)
        {
            if (Connections.TryGetValue(connectionId, out var connection))
            {
                return Accounts.FirstOrDefault(x => x.Connection == connection);
            }
            return null;
        }

        public DataProvider GetDataProvider(int dataDataProviderId)
        {
            if (DataProviders.TryGetValue(dataDataProviderId, out var dataProvider))
            {
                return dataProvider;
            }
            return null;
        }

        public DataProvider GetDataProvider(string dataDataProviderCode)
        {
            var dataProvider = DataProviders.FirstOrDefault(x => x.Value.Code == dataDataProviderCode);

            return dataProvider.Value;
        }

        public async Task<Instrument> GetMappedInstrument(Instrument inst, int connectionId)
        {

            if (InstrumentMapCache.ContainsKey(inst.Id) && InstrumentMapCache[inst.Id].ContainsKey(connectionId))
            {
                if (InstrumentCache.ContainsKey(InstrumentMapCache[inst.Id][connectionId]))
                {
                    return InstrumentCache[InstrumentMapCache[inst.Id][connectionId]];
                }
            }

            IEnumerable<InstrumentMap> instrumentMaps;
            await DbSemaphoreSlim.WaitAsync(new CancellationToken());
            try
            {
                using (var context = DbContextFactory.GetContext())
                {
                    var repository = new InstrumentMapRepository(context);
                    instrumentMaps = (await repository.GetInstrumentMapByInstrument(inst.Id)).ToList();
                }
            }
            finally
            {
                DbSemaphoreSlim.Release(1);
            }

            Instrument mappedInstrument = null;
            foreach (var instrumentMap in instrumentMaps)
            {
                if (instrumentMap.FirstInstrument.ConnectionId == connectionId)
                {
                    mappedInstrument = instrumentMap.FirstInstrument;
                }
                else if (instrumentMap.SecondInstrument.ConnectionId == connectionId)
                {
                    mappedInstrument = instrumentMap.SecondInstrument;
                }

                if (mappedInstrument != null)
                {
                    if (!InstrumentMapCache.ContainsKey(inst.Id))
                        InstrumentMapCache.TryAdd(inst.Id,
                            new ConcurrentDictionary<int, int>(connectionId, mappedInstrument.Id));
                    else
                    {
                        InstrumentMapCache[inst.Id].TryAdd(connectionId, mappedInstrument.Id);
                    }

                    if (!InstrumentCache.ContainsKey(mappedInstrument.Id))
                    {
                        InstrumentCache.TryAdd(mappedInstrument.Id, mappedInstrument);
                    }
                }
            }

            return mappedInstrument;



        }


        public async Task<bool> IsInstrumentEqual(Instrument ins1, Instrument ins2)
        {

            /* if (ins1.ConnectionId == ins2.ConnectionId)
             {
                 if (ins1.Id != ins2.Id)
                 {
                     return false;
                 }
             }
             else
             {
                 var inst = await GetMappedInstrument(ins1, ins2.ConnectionId);
                 if (inst.Id != ins2.Id)
                 {
                     return false;
                 }
             }*/

            return true;
        }



        public void InitTestEnvironment()
        {
            IEnumerable<Model.Model.Connection> connections;
            List<Setting> settings;
            using (var context = DbContextFactory.GetContext())
            {
                settings = context.Settings.ToList();
                var connectionRepository = new ConnectionRepository(context);
                connections = connectionRepository.GetAll().Result.ToList();
            }
            SetSettings(settings);
            Init(connections);
        }



        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));



        }
    }

    public class ConnectionStatusChangeEventArgs : EventArgs
    {
        public ConnectionStatus OldStatus { get; set; }
        public ConnectionStatus NewStatus { get; set; }

        public int ConnectionId { get; set; }
        public string ConnectionName { get; set; }

        public ConnectionStatusChangeEventArgs(ConnectionStatus oldStatus, ConnectionStatus newStatus, int connectionId, string connectionName)
        {
            OldStatus = oldStatus;
            NewStatus = newStatus;
            ConnectionId = connectionId;
            ConnectionName = connectionName;
        }
    }
}