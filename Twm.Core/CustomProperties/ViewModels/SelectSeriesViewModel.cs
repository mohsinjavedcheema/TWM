using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Twm.Core.Annotations;
using Twm.Core.Attributes;
using Twm.Core.Classes;
using Twm.Core.Controllers;
using Twm.Core.DataCalc;
using Twm.Core.Enums;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace Twm.Core.CustomProperties.ViewModels
{
    public class SelectSeriesViewModel : INotifyPropertyChanged
    {
        public PropertyDefinitionCollection PropertyDefinitionCollection { get; set; }

        public ObservableCollection<object> SeriesCollection { get; set; }

        private readonly DataCalcContext _dataCalcContext;

        private object _selectedSeries;
        public object SelectedSeries
        {
            get { return _selectedSeries; }
            set
            {
                if (value != _selectedSeries)
                {
                    if (value is TempObjectType tempIndicatorType)
                    {
                        _selectedSeries = CreateIndicator(tempIndicatorType);
                    }
                    else
                    {
                        _selectedSeries = value;
                    }

                    
                    OnPropertyChanged();
                }
            }
        }

       


        public SelectSeriesViewModel(DataCalcContext dataCalcContext)
        {
            _dataCalcContext = dataCalcContext;
            SeriesCollection = new ObservableCollection<object>();
            PropertyDefinitionCollection = new PropertyDefinitionCollection();
            //TODO: Input will enabled in the future
           // PropertyDefinitionCollection.Add(new PropertyDefinition(){TargetProperties = { "Input" } });
            PropertyDefinitionCollection.Add(new PropertyDefinition() { TargetProperties = { "PriceType" } });
        }


        public void Init(object value)
        {
            SeriesCollection.Clear();

            bool isValueIndicator = false;
            Type valueType = value.GetType();

            if (value is DoubleSeries ds)
                SeriesCollection.Add(ds.Clone());
            else
            {
                isValueIndicator = true;
                SeriesCollection.Add(new DoubleSeries(_dataCalcContext, CandleProperty.Close));
            }


            var indicatorTypeBase = typeof(IndicatorBase);

            //TODO:Cache types
            var indicatorTypes = BuildController.Instance.CustomAssembly.GetTypes()
                .Where(x => x.IsSubclassOf(indicatorTypeBase));

            IEnumerable<string> propertyCollection = Enumerable.Empty<string>();
            foreach (var indicatorType in indicatorTypes)
            {
                var propertyNames = indicatorType.GetProperties().Where(x => Attribute.IsDefined(x, typeof(TwmPropertyAttribute)))
                    .Select(x => x.Name);
                propertyCollection = propertyCollection.Union(propertyNames);

                
                if (isValueIndicator && indicatorType == valueType)
                {
                    var indicator = (IndicatorBase)(value as ICloneable).Clone();
                    SeriesCollection.Add(indicator);
                }
                else
                {

                    SeriesCollection.Add(new TempObjectType(indicatorType));
                    //indicator = _dataCalcContext.CreateIndicator(indicatorType, null, true);
                }
                
            }

            foreach (var property in propertyCollection)
            {
                PropertyDefinitionCollection.Add(new PropertyDefinition() { TargetProperties = { property } });
            }

        }

        private object CreateIndicator(TempObjectType tempIndicatorType)
        {
            var index =SeriesCollection.IndexOf(tempIndicatorType);
            var indicator = _dataCalcContext.CreateIndicator(tempIndicatorType.Type, null, true);

            SeriesCollection[index] = indicator;

            return indicator;
        }


        public void Select(object value)
        {
            if (value is IndicatorBase)
            {
                SelectedSeries = SeriesCollection.FirstOrDefault(x => x.GetType() == value.GetType());
                return;
            }

            SelectedSeries = SeriesCollection.FirstOrDefault();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

       
    }
}