using System;
using System.Collections.Generic;
using Twm.Core.Classes;
using Twm.Core.DataProviders.Common;
using Twm.Core.DataProviders.Interfaces;


namespace Twm.Core.DataProviders.All
{
    public class All : ConnectionBase
    {

        public override string Name { get; set; }

        public override bool IsLocalPaperSupported
        {
            get { return false; }
        }


        private bool _isServerPaperSupported;
        public override bool IsServerPaperSupported
        {
            get { return _isServerPaperSupported; }
        }


        private bool _isBrokerSupported;
        public override bool IsBrokerSupported
        {
            get { return _isBrokerSupported; }
        }



        public All()
        {          
            Name = "ALL";
        }

       


        public override async void Connect()
        {
            



        }



        

      


        public override void Disconnect()
        {
      
        }

        public override List<DataSeriesValue> GetDataFormats()
        {
            throw new NotImplementedException();
        }

        public override IInstrumentManager CreateInstrumentManager()
        {
            throw new NotImplementedException();
        }
    }
}