using System.Collections.Generic;
using System.Runtime.Serialization;
using Twm.Core.DataCalc;
using Twm.Core.Interfaces;

namespace Twm.ViewModels.Presets
{
    [DataContract]
    public class ValidatorStrategyPreset :IValidatorStrategyPreset
    {
        [DataMember]
        public string Guid { get; set; }

        [DataMember]
        public Dictionary<string, object> Parameters { get; set; }

        public ValidatorStrategyPreset()
        {
            
        }

        public ValidatorStrategyPreset(StrategyBase strategyBase)
        {
            Guid = strategyBase.Guid.ToString();
            Parameters = strategyBase.GetTwmPropertyValues();
        }

    }
}