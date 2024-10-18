using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using System.Xml.Serialization;
using Twm.Chart.Annotations;
using Twm.Core.Classes;
using Twm.Core.CustomProperties;
using Twm.Core.DataProviders.Common;
using Twm.Core.UI.Windows;
using Twm.DB.DAL.Repositories.Instruments;
using Twm.Model.Model;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Twm.Core.ViewModels.Instruments
{
    public class InstrumentViewModel:INotifyPropertyChanged
    {
        [Browsable(false)]
        public Instrument DataModel { get; set; }

        [Browsable(false)]
        public object ViewObject { get; set; }

        [Browsable(false)]
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

        [Category("Main")]
        public string Symbol
        {
            get { return DataModel.Symbol; }
            set
            {
                if (DataModel.Symbol != value)
                {
                    DataModel.Symbol = value;
                    OnPropertyChanged();
                }
            }
        }

        [Category("Main")]
        public string Base
        {
            get { return DataModel.Base; }
            set
            {
                if (DataModel.Base != value)
                {
                    DataModel.Base = value;
                    OnPropertyChanged();
                }
            }
        }

        [Category("Main")]
        public string Quote
        {
            get { return DataModel.Quote; }
            set
            {
                if (DataModel.Quote != value)
                {
                    DataModel.Quote = value;
                    OnPropertyChanged();
                }
            }
        }


        [Category("Main")]
        public double? MinLotSize
        {
            get { return DataModel.MinLotSize; }
            set
            {
                if (DataModel.MinLotSize != value)
                {
                    DataModel.MinLotSize = value;
                    OnPropertyChanged();
                }
            }
        }

        [Category("Main")]
        public double? Notional
        {
            get { return DataModel.Notional; }
            set
            {
                if (DataModel.Notional != value)
                {
                    DataModel.Notional = value;
                    OnPropertyChanged();
                }
            }
        }




        [Browsable(false)]
        public string ConnectionName
        {
            get
            {
                var connection = Session.Instance.GetConnection(DataModel.ConnectionId);
                if (connection != null)
                    return connection.Name;
                return string.Empty;
            }

        }

        [Category("Main")]
        public string Type
        {
            get { return DataModel.Type; }
            set
            {
                if (DataModel.Type != value)
                {
                    DataModel.Type = value;
                    OnPropertyChanged();
                }
            }
        }

      

       





        [Category("Main")]
        public double? Multiplier
        {
            get { return DataModel.Multiplier; }
            set
            {
                if (DataModel.Multiplier != value)
                {
                    DataModel.Multiplier = value;
                    OnPropertyChanged();
                }
            }
        }

       



        [Category("Main")]
        public string PriceIncrements
        {
            get { return DataModel.PriceIncrements; }
            set
            {
                if (DataModel.PriceIncrements != value)
                {
                    DataModel.PriceIncrements = value;
                    OnPropertyChanged();
                }
            }
        }

        [Browsable(false)]
        public string TradingHours
        {
            get { return DataModel.TradingHours; }
            set
            {
                if (DataModel.TradingHours != value)
                {
                    DataModel.TradingHours = value;
                    OnPropertyChanged();
                }
            }
        }



        [Browsable(false)]
        public int ConnectionId
        {
            get { return DataModel.ConnectionId; }
            set
            {
                if (DataModel.ConnectionId != value)
                {
                    DataModel.ConnectionId = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _isReadOnly;
        [Browsable(false)]
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                if (_isReadOnly != value)
                {
                    _isReadOnly = value;
                    OnPropertyChanged();
                }
            }
        }


        [Browsable(false)]
        public ICommand ViewCommand { get; set; }


       
        [ExpandableObject]
        [Display(Name = "Fields")]
        [Category("Provider specific")]
        [ReadOnly(true)]
        public object ProviderData { get; set; }


        public InstrumentViewModel(Instrument dataModel)
        {
            DataModel = dataModel;
            ViewCommand = new OperationCommand(View);
            ViewObject = this;
            
        }

        [Browsable(false)]
        public bool IsInstrument
        {
            get { return true; }

        }

        public InstrumentViewModel() : this(new Instrument())
        {
        }


        private object _instrument;

        protected object Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }
            try
            {
                if (Session.Instance.GetConnection(ConnectionId) is ConnectionBase connection)
                {
                    if (connection.InstrumentType == null)
                    {
                        MessageBox.Show("Set instrument type for this provider!");
                        return null;
                    }

                    var xmlSerializer = new XmlSerializer(connection.InstrumentType);
                    var stringReader = new StringReader(data);
                    using (var reader = XmlReader.Create(stringReader))
                    {
                        _instrument = xmlSerializer.Deserialize(reader);

                        var bo = new BusinessObject();
                        var properties = connection.InstrumentType.GetProperties().Where(x => Attribute.IsDefined(x, typeof(CategoryAttribute)));
                        var descriptors = TypeDescriptor.GetProperties(_instrument.GetType());
                        foreach (var property in properties)
                        {
                            PropertyDescriptor theDescriptor = descriptors[property.Name];
                            CategoryAttribute categoryAttribute = (CategoryAttribute)theDescriptor.Attributes[typeof(CategoryAttribute)];

                            if (categoryAttribute.Category != "Main")
                            {
                                bo.AddMember(property.Name, property.PropertyType, property.GetValue(_instrument));
                            }
                        }

                        return bo;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Deserialize error", ex);
            }
        }

        private string Serialize(object obj)
        {
            if (obj == null)
            {
                return null;
            }
            try
            {
                if (Session.Instance.GetConnection(ConnectionId) is ConnectionBase connection)
                {
                    if (obj is BusinessObject bo)
                    {
                        
                        var properties = connection.InstrumentType.GetProperties()
                            .Where(x => Attribute.IsDefined(x, typeof(CategoryAttribute)));
                        
                        foreach (var property in properties)
                        {
                            if (bo.TryGetMemberValue(property.Name, out var value))
                            {
                                property.SetValue(_instrument, value);
                            }
                        }

                        var xmlSerializer = new XmlSerializer(connection.InstrumentType);
                        var stringWriter = new StringWriter();
                        using (var writer = XmlWriter.Create(stringWriter))
                        {
                            var emptyNs = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
                            xmlSerializer.Serialize(writer, _instrument, emptyNs);
                            return stringWriter.ToString();
                        }
                    }
                }


                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Serialize error", ex);
            }
        }


        private async void View(object obj)
        {
            
            var instrument = (Instrument)DataModel.Clone();
            var instrumentVm = new InstrumentViewModel(instrument);
            instrumentVm.DeserializeProviderData();
            
            var instrumentWindow = new InstrumentWindow(instrumentVm);
            if (instrumentWindow.ShowDialog() == true)
            {
                instrumentVm.SerializeProviderData();

                

                DataModel = instrument;

                using (var context =  Session.Instance.DbContextFactory.GetContext())
                {
                    var instrumentRepository = new InstrumentRepository(context);
                    await instrumentRepository.Update(DataModel);
                    await instrumentRepository.CompleteAsync();

                }
            }
        }

        private void DeserializeProviderData()
        {
            ProviderData = Deserialize(DataModel.ProviderData);
        }

        private void SerializeProviderData()
        {
            DataModel.ProviderData = Serialize(ProviderData);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}