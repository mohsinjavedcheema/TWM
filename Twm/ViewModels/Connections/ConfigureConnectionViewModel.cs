using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using Twm.Core.Attributes;
using Twm.Core.Classes;
using Twm.Core.DataProviders.Binance;
using Twm.Core.DataProviders.Bybit;
using Twm.Core.DataProviders.Common;
using Twm.Core.DataProviders.Enums;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.ViewModels;
using Twm.DB.DAL.Repositories.Connections;
using Twm.Model.Model;

namespace Twm.ViewModels.Connections
{
    public class ConfigureConnectionViewModel : ViewModelBase
    {

        public ObservableCollection<Type> AvailableConnections { get; set; }

        public ObservableCollection<ConnectionBase> ConfiguredConnections { get; set; }


        private Type _selectedConnectionType;

        public Type SelectedConnectionType
        {
            get
            {
                return _selectedConnectionType;
            }
            set
            {
                if (value != _selectedConnectionType)
                {
                    _selectedConnectionType = value;
                    if (_selectedConnectionType != null)
                        SelectedConnection = null;
                    OnPropertyChanged();
                }
            }

        }


        private ConnectionBase _selectedConnection;
        public ConnectionBase SelectedConnection
        {
            get
            {
                return _selectedConnection;
            }
            set
            {
                if (value != _selectedConnection)
                {
                    _selectedConnection = value;
                    if (_selectedConnection != null)
                        SelectedConnectionType = null;
                    OnPropertyChanged();
                }
            }
        }

        public OperationCommand ConfigureConnectionCommand { get; set; }

        public OperationCommand RemoveConnectionCommand { get; set; }




        public ConfigureConnectionViewModel()
        {
            AvailableConnections = new ObservableCollection<Type>();

            ConfiguredConnections = new ObservableCollection<ConnectionBase>();

            SelectedConnection = null;

            ConfigureConnectionCommand = new OperationCommand(ConfigureConnection);
            RemoveConnectionCommand = new OperationCommand(RemoveConnection);

        }

        private void RemoveConnection(object obj)
        {
            if (SelectedConnection != null)
            {
                


                var connectionsForDelete = ConfiguredConnections.Where(x => x.DataProviderId == SelectedConnection.DataProviderId).ToList();
                Type type = null;
                foreach (var connection in connectionsForDelete)
                {
                    ConfiguredConnections.Remove(connection);
                    type = connection.GetType();
                }

                AvailableConnections.Add(type);
                SelectedConnection = ConfiguredConnections.FirstOrDefault();
            }
        }

        private void ConfigureConnection(object obj)
        {
            if (SelectedConnectionType != null)
            {
                var dataProvider = Session.Instance.GetDataProvider(SelectedConnectionType.Name);
                if (dataProvider == null)
                {
                    MessageBox.Show($"Data provider for type {SelectedConnectionType.Name} not found");
                    return;
                }

                var connectionReal = CreteConnection(dataProvider, ConnectionType.Real);
                ConfiguredConnections.Add(connectionReal);                
                var connectionPaper = CreteConnection(dataProvider, ConnectionType.Paper);
                ConfiguredConnections.Add(connectionPaper);

                AvailableConnections.Remove(SelectedConnectionType);

                SelectedConnection = connectionReal;

              
            }
        }

        private ConnectionBase CreteConnection(DataProvider dataProvider, ConnectionType connectionType)
        {
            dynamic connection = Activator.CreateInstance(SelectedConnectionType);
            var con = (ConnectionBase)connection;
            con.Options = (ConnectionOptionsBase)Session.GetInstance(Session.GetConnectionOptionsFullyQualifiedName(SelectedConnectionType.Name));

            con.DataProviderId = dataProvider.Id;
            con.DataProvider = dataProvider.Code;
            con.Options.DataProvider = dataProvider.Code;
            con.Options.DataProviderId = dataProvider.Id;
            

            

            if (con.Options is BinanceConnectionOptions binanceConnectionOptions)
            {
                binanceConnectionOptions.SetConnectionType(connectionType);
                con.Options.Name = "Binance Mainnet";
                con.Code = "BinanceMainnet";

                if (connectionType == ConnectionType.Paper)
                {
                    con.Options.Name = "Binance Testnet";
                    con.Code = "BinanceTestnet";
                }
            }

            if (con.Options is BybitConnectionOptions bybitConnectionOptions)
            {
                bybitConnectionOptions.SetConnectionType(connectionType);
                con.Options.Name = "Bybit Mainnet";
                con.Code = "BybitMainnet";

                if (connectionType == ConnectionType.Paper)
                {
                    con.Options.Name = "Bybit Testnet";
                    con.Code = "BybitTestnet";
                }
            }


            return connection;
        }

        public void Init()
        {
            ConfiguredConnections.Clear();

            foreach (var connection in Session.Instance.ConfiguredConnections)
            {
                ConfiguredConnections.Add(connection);
            }

            if (ConfiguredConnections.FirstOrDefault(x => x.GetType() == typeof(Binance)) == null)
            {
                AvailableConnections.Add(typeof(Binance));
            }

            if (ConfiguredConnections.FirstOrDefault(x => x.GetType() == typeof(Bybit)) == null)
            {
                AvailableConnections.Add(typeof(Bybit));
            }
            SelectedConnectionType = AvailableConnections.FirstOrDefault();
        }


        public override async void Save()
        {

            //Save or update Connection
            foreach (var configuredConnection in ConfiguredConnections)
            {
                if (configuredConnection.IsReadonly)
                    continue;

                using (var context = Session.Instance.DbContextFactory.GetContext())
                {
                    var connectionRepository = new ConnectionRepository(context);

                    if (configuredConnection.Id == 0)
                    {
                        configuredConnection.DataModel.Name = configuredConnection.Name;
                        configuredConnection.DataModel.DataProviderId = configuredConnection.DataProviderId;
                        await connectionRepository.Add(configuredConnection.DataModel);
                        await connectionRepository.CompleteAsync();
                        Session.Instance.ConfiguredConnections.Add(configuredConnection);
                        Session.Instance.Connections.Add(configuredConnection.Id, configuredConnection);
                    }
                    else
                    {
                        var connection = connectionRepository.FindBy(x => x.Id == configuredConnection.Id).FirstOrDefault();
                        connection.Name = configuredConnection.Name;
                        connection.DataProviderId = configuredConnection.DataProviderId;
                        await connectionRepository.Update(connection);
                    }


                    //Save or update Options
                    var connectionOptionRepository = new ConnectionOptionRepository(context);

                    var connectionOptionType = configuredConnection.Options.GetType();
                    var properties = connectionOptionType.GetProperties();

                    foreach (var propertyInfo in properties)
                    {
                        var attribute = propertyInfo.GetCustomAttributes(typeof(CustomConnectionOptionAttribute), true);

                        if (attribute.Length > 0)
                        {
                            var name = propertyInfo.Name;
                            var value = propertyInfo.GetValue(configuredConnection.Options);

                            var connectionOption = connectionOptionRepository.FindBy(x => x.ConnectionId == configuredConnection.Id && x.Name == name).FirstOrDefault();

                            if (connectionOption == null)
                            {
                                await connectionOptionRepository.Add(new ConnectionOption() { Name = name, Value = value == null ? "" : value.ToString(), ConnectionId = configuredConnection.Id });
                                await connectionOptionRepository.CompleteAsync();
                            }
                            else
                            {
                                connectionOption.Value = value == null ? "" : value.ToString();
                                await connectionOptionRepository.Update(connectionOption);
                            }

                        }
                    }
                }

            }

            var existingConnectionsIds = ConfiguredConnections.Select(x => x.Id).ToList();

            var notExistingConnectionsIds = Session.Instance.ConfiguredConnections.Select(x => x.Id).Where(y => !existingConnectionsIds.Contains(y)).ToList();

            foreach (var id in notExistingConnectionsIds)
            {
                var connectionToRemove = Session.Instance.ConfiguredConnections.FirstOrDefault(x => x.Id == id);
                Session.Instance.ConfiguredConnections.Remove(connectionToRemove);
                using (var context = Session.Instance.DbContextFactory.GetContext())
                {

                    var connectionRepository = new ConnectionRepository(context);
                    connectionRepository.Remove(connectionToRemove.DataModel);
                    await connectionRepository.CompleteAsync();

                    Session.Instance.Connections.Remove(connectionToRemove.Id);
                }
            }

            Session.Instance.ConnectionsUpdated();

        }
    }
}