using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Twm.Core.Classes;

namespace Twm.Core.DataProviders.Interfaces
{
    public interface IConnection
    {
        int Id { get; set; }

        string Name { get; set; }

        string DataProvider { get; set; }

        string Code { get; set; }

        int DataProviderId { get; set; }

        bool IsConnected { get; set; }

        bool IsLocalPaperSupported { get; }

        bool IsServerPaperSupported { get; }

        bool IsBrokerSupported { get; }

        List<DataSeriesValue> GetDataFormats();

        void Connect();

        void Disconnect();

        IDataProviderClient Client { get; set; }
    }
}