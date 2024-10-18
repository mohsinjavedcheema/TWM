using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Twm.Core.Annotations;
using Twm.Core.Classes;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.Enums;
using Twm.Core.Market;
using Twm.Core.ViewModels;
using Twm.DB.DAL.Repositories.Instruments;
using Twm.Model.Model;

namespace Twm.Core.DataProviders.Common
{
    public abstract class ConnectionBase : INotifyPropertyChanged, IConnection
    {
        public Connection DataModel { get; set; }

        public Type InstrumentType { get; set; }

        public int Order { get; set; }

        public int Id
        {
            get { return DataModel.Id; }
            set
            {
                if (DataModel.Id != value)
                {
                    DataModel.Id = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Code
        {
            get { return DataModel.Code; }
            set
            {
                if (DataModel.Code != value)
                {
                    DataModel.Code = value;
                    OnPropertyChanged();
                }
            }
        }


        private GridLength _maxConnectionNameWidth;
        public GridLength MaxConnectionNameWidth
        {
            get { return _maxConnectionNameWidth; }
            set
            {
                if (_maxConnectionNameWidth != value)
                {
                    _maxConnectionNameWidth = value;
                    OnPropertyChanged();
                }

            }
        }

        public virtual string Name
        {
            get { return _options.Name; }
            set
            {
                if (_options.Name != value)
                {
                    _options.Name = value;
                    OnPropertyChanged();
                }

            }

        }

        public string DataProvider
        {
            get { return _options.DataProvider; }
            set
            {
                if (_options.DataProvider != value)
                {
                    _options.DataProvider = value;
                    OnPropertyChanged();
                }

            }

        }

        public int DataProviderId
        {
            get { return DataModel.DataProviderId; }
            set
            {
                if (DataModel.DataProviderId != value)
                {
                    DataModel.DataProviderId = value;
                    OnPropertyChanged();
                }

            }

        }
        
        public abstract List<DataSeriesValue> GetDataFormats();

        private ConnectionOptionsBase _options;

        public virtual ConnectionOptionsBase Options
        {
            get { return _options; }
            set
            {
                if (_options != value)
                {
                    _options = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _isConnected;
        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }
            set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;

                    if (_isConnected)
                    {
                        PlaySuccessSound();
                        if  (Client.IsPrivate)
                            CreateAccounts();

                        Session.Instance.ConnectionStatusChanged(ConnectionStatus.Connecting,
                            ConnectionStatus.Connected, Id);

                    }
                    else
                    {
                        RemoveAccount();
                        Session.Instance.ConnectionStatusChanged(ConnectionStatus.Connected,
                            ConnectionStatus.Disconnected, Id);

                    }

                    OnPropertyChanged();
                }
            }

        }

        public virtual int Levels { get; }

        public virtual bool IsLocalPaperSupported { get; }

        public virtual bool IsServerPaperSupported { get; }

        public virtual bool IsBrokerSupported { get; }

        public virtual bool IsReadonly
        {
            get { return false; }
        }

        public virtual IDataProviderClient Client { get; set; }

        public abstract void Connect();

        public abstract void Disconnect();

        public abstract IInstrumentManager CreateInstrumentManager();
        public ICommand ConnectDisconnectCommand { get; set; }

       

        public bool IsTest
        {
            get { return DataModel.Options.Exists(x => x.Name == "ConnectionType" && x.Value == "Paper"); }


        }

        protected ConnectionBase()
        {
            DataModel = new Connection();
            Options = new ConnectionOptionsBase();
            ConnectDisconnectCommand = new OperationCommand(ConnectDisconnect);
        }

        private async void ConnectDisconnect(object obj)
        {
            if (!IsConnected)
            {
                await Task.Run(() =>
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (Action)(Connect));
                });

            }
            else
            {
                Disconnect();
            }
        }


        protected void PlaySuccessSound()
        {
            SoundPlayer playSound = new SoundPlayer { Stream = Properties.Resource.Connected };
            playSound.Play();
        }



        public async Task<bool> CheckDefaultInstruments()
        {
            List<InstrumentList> instrumentLists;
            using (var context = Session.Instance.DbContextFactory.GetContext())
            {
                var instrumentListRepository = new InstrumentListRepository(context);
                instrumentLists = (await instrumentListRepository.GetAll()).ToList();
            }

            var defaultInstrumentLists = instrumentLists.Where(x => x.IsDefault && x.ConnectionId == Id).ToList();

            if (defaultInstrumentLists.Count < 2)
            {
                foreach (var type in new string[] { "Future", "Spot" })
                {
                    var defaultName = "Default";
                    var defaultInstrumentList = defaultInstrumentLists.FirstOrDefault(x => x.Name == defaultName);
                    if (defaultInstrumentList == null)
                    {
                        using (var context = Session.Instance.DbContextFactory.GetContext())
                        {
                            var instrumentListRepository = new InstrumentListRepository(context);
                            defaultInstrumentList = new InstrumentList() { Name = defaultName, Type = type.ToUpper(), IsDefault = true, ConnectionId = Id };
                            await instrumentListRepository.Add(defaultInstrumentList);
                            await instrumentListRepository.CompleteAsync();
                        }

                        if (this is IDataProvider provider)
                        {
                            var insCollection = (await provider.GetInstruments()).ToList();
                            var instrumentManager = CreateInstrumentManager();
                            var instruments = instrumentManager.ConvertToInstruments(insCollection, type);
                            var defaultInstrumentSymbols = instruments.Select(x => x.Symbol);

                            using (var context = Session.Instance.DbContextFactory.GetContext())
                            {
                                var instrumentRepository = new InstrumentRepository(context);

                                var existingSymbols = ((await instrumentRepository.GetAll()).Where(x=>x.Type == type).Select(x => x.Symbol)).ToList();

                                await instrumentRepository.Add(instruments.Where(x=> !existingSymbols.Contains(x.Symbol)).ToArray());
                                await instrumentRepository.CompleteAsync();
                                instruments = (instrumentRepository.FindBy(x => defaultInstrumentSymbols.Contains(x.Symbol) && x.Type == type.ToUpper())).ToList();


                                var InstrumentInstrumentList = instruments.Select(x => new InstrumentInstrumentList() { InstrumentId = x.Id, InstrumentListId = defaultInstrumentList.Id });
                                var instrumentInstrumentListRepository = new InstrumentInstrumentListRepository(context);
                                await instrumentInstrumentListRepository.Add(InstrumentInstrumentList.ToArray());
                                await instrumentInstrumentListRepository.CompleteAsync();
                            }
                        }
                    }
                }
            }

            return true;
        }

        public async Task<Instrument> CreateInstrument(string symbol, string type)
        {
            Instrument instrument = null;
            if (this is IDataProvider provider)
            {
                var insCollection = (await provider.GetInstruments()).ToList();
                var instrumentManager = CreateInstrumentManager();
                var instruments = instrumentManager.ConvertToInstruments(insCollection, type, new List<string>() {symbol });

                instrument = instruments.FirstOrDefault();
                if (instrument != null)
                {
                    using (var context = Session.Instance.DbContextFactory.GetContext())
                    {
                        var instrumentRepository = new InstrumentRepository(context);
                        await instrumentRepository.Add(instrument);
                        await instrumentRepository.CompleteAsync();
                    }                    
                }               
            }
            return instrument;

        }


        private void CreateAccounts()
        {
            if (this is Playback.Playback)
            {
                CreateAccount(AccountType.Playback);
            }

            if (IsLocalPaperSupported)
            {
                CreateAccount(AccountType.LocalPaper);
            }

            if (IsServerPaperSupported)
            {
                CreateAccount(AccountType.ServerPaper);
            }

            if (IsBrokerSupported)
            {
                CreateAccount(AccountType.Broker);
            }

        }

        private void CreateAccount(AccountType accountType)
        {
            var account = Session.Instance.Accounts.FirstOrDefault(x => x.Connection.Id == Id && x.AccountType == accountType);

            if (account == null)
            {
                account = new Account() { AccountType = accountType, LocalId = Guid.NewGuid().ToString() };
                Session.Instance.Accounts.Add(account);
            }
            account.InitConnection(this);

            account.IsActive = true;
        }

        private void RemoveAccount()
        {
            var accountsForRemove = Session.Instance.Accounts.Where(x => x.Connection == this).ToList();
            if (accountsForRemove.Any())
            {
                foreach (var accountForRemove in accountsForRemove)
                {
                    accountForRemove.IsActive = false;
                    accountForRemove.Disconnect();
                }

            }
        }

        public override string ToString()
        {
            return Name;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}