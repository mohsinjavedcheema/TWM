using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Twm.Core.Controllers;
using Twm.Core.DataCalc;
using Twm.Core.DataCalc.Commissions;
using Twm.Core.DataCalc.Optimization;
using Twm.Core.Enums;
using Twm.Core.Interfaces;
using Newtonsoft.Json;
using Twm.Core.Classes;

namespace Twm.ViewModels.Presets
{
    [DataContract]
    public class OptimizerStrategyPreset : IOptimizerStrategyPreset
    {
        [DataMember]
        public string StrategyType { get; set; }

        [DataMember]
        public string Guid { get; set; }

        [DataMember]
        public string Symbol { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string ConnectionCode { get; set; }

        [DataMember]
        public int DataSeriesValue { get; set; }
        [DataMember]
        public DataSeriesType DataSeriesType { get; set; }

        [DataMember]
        public DataSeriesValue DataSeriesFormat { get; set; }

      
        [DataMember]
        public string OptimizationFitnessType { get; set; }


        [DataMember]
        public List<OptimizerParameter> Parameters { get; set; }

        [DataMember]
        public Optimizer Optimizer { get; set; }

        [DataMember]
        public string OptimizerType { get; set; }

        [DataMember]
        public string TimeZone { get; set; }

        [DataMember]
        public Commission Commission { get; set; }

        [DataMember]
        public string CommissionType { get; set; }

        [DataMember]
        public double? CommissionValue { get; set; }

        public OptimizerStrategyPreset(StrategyBase strategy)
        {
            Guid = strategy.Guid.ToString();
            StrategyType = strategy.GetType().Name;
            if (strategy.Instrument != null)
            {
                Symbol = strategy.Instrument.Symbol;
            }
            else
            {
                Symbol = "";
            }

            var connection = Session.Instance.ConfiguredConnections.FirstOrDefault(x => x.Id == strategy.DataSeriesSeriesParams.Instrument.ConnectionId);

            if (strategy.DataSeriesSeriesParams != null)
            {
                DataSeriesType = strategy.DataSeriesSeriesParams.DataSeriesType;
                DataSeriesValue = strategy.DataSeriesSeriesParams.DataSeriesValue;
                DataSeriesFormat = strategy.DataSeriesSeriesParams.DataSeriesFormat;
                ConnectionCode = connection.Code;
                Type = strategy.DataSeriesSeriesParams.Instrument.Type;
            }
            else
            {
                DataSeriesValue = 1;
            }



            TimeZone = SystemOptions.Instance.TimeZone.Id;

            OptimizationFitnessType = strategy.OptimizationFitnessType.ObjectType.Name;
            OptimizerType = strategy.OptimizerType.ObjectType.Name;
            Optimizer = strategy.Optimizer;
            Parameters = strategy.Optimizer.OptimizerParameters.ToList();

            CommissionValue = strategy.CommissionValue;
            Commission = strategy.Commission;
            CommissionType = strategy.CommissionType.ObjectType.Name;


        }

        [JsonConstructor]
        public OptimizerStrategyPreset()
        {

        }
    }
}