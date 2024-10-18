using System;
using System.Collections.Generic;
using Twm.Model.Model;

namespace Twm.Core.DataProviders.Interfaces
{
    public interface IInstrumentManager
    {
        event InstrumentsAdded InstrumentsAddedEvent;

        bool IsAddEnable { get; set; }

        bool IsAddAllEnable { get; set; }

        bool IsFindEnable { get; set; }

        List<Instrument> ConvertToInstruments(List<object> insCollection, string type, List<string> symbolList = null);

        List<string> GetDefaultInstruments();

    }

    public delegate void InstrumentsAdded(List<Instrument> instruments);
}