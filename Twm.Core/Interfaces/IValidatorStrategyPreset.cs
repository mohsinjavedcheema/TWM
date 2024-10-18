using System.Collections.Generic;

namespace Twm.Core.Interfaces
{
    public interface IValidatorStrategyPreset
    {
         string Guid { get; set; }

         Dictionary<string, object> Parameters { get; set; }

    }
}