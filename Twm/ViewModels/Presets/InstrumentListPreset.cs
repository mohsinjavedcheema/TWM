using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Twm.ViewModels.Strategies.Validator;
using Newtonsoft.Json;
using Twm.Core.Classes;

namespace Twm.ViewModels.Presets
{
    [DataContract]
    public class InstrumentListPreset
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool ApplyToAll { get; set; }

        [DataMember]
        public ValidatorStrategyPreset Strategy { get; set; }


        [DataMember]
        public List<InstrumentPreset> Instruments { get; set; }

        public InstrumentListPreset(ValidatorItemViewModel validatorItem)
        {
            Name = validatorItem.Name;
            ApplyToAll = validatorItem.ApplyToAllInstruments;
            Strategy = new ValidatorStrategyPreset();
            if (validatorItem.Strategy != null)
            {
                Strategy.Guid = validatorItem.Strategy?.Guid.ToString();
                Strategy.Parameters = validatorItem.Strategy.GetTwmPropertyValues();
            }
            Instruments = new List<InstrumentPreset>();

            foreach (var instrument in validatorItem.Items.OfType<ValidatorItemViewModel>())
            {
                var instrumentPreset = new InstrumentPreset
                {
                    Symbol = instrument.InstrumentSeriesParams.Symbol,
                    Type = instrument.InstrumentSeriesParams.Instrument.Type,
                    ConnectionCode = Session.Instance.ConfiguredConnections.FirstOrDefault(x=>x.Id == instrument.InstrumentSeriesParams.Instrument.ConnectionId).Code,                    
                    DataSeriesFormat = instrument.InstrumentSeriesParams.DataSeriesFormat,
                    DaysToLoad = instrument.InstrumentSeriesParams.DaysToLoad,
                    TimeFrameBase = instrument.InstrumentSeriesParams.SelectedTimeFrameBase,
                    PeriodStart = instrument.InstrumentSeriesParams.PeriodStart,
                    PeriodEnd = instrument.InstrumentSeriesParams.PeriodEnd,
                    Strategy = new ValidatorStrategyPreset()
                    {
                        Guid = instrument.GetOriginalStrategy()?.Guid.ToString(),
                        Parameters = instrument.GetOriginalStrategy()?.GetTwmPropertyValues()
                    }
                };
                Instruments.Add(instrumentPreset);
            }



        }

        [JsonConstructor]
        public InstrumentListPreset()
        {

        }
    }
}