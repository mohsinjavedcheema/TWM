using System;
using System.Diagnostics.Tracing;
using System.Windows;
using AlgoDesk.Core.Classes;
using AlgoDesk.Core.ConnectionOptions;
using AlgoDesk.Core.Interfaces;


namespace AlgoDesk.Core.Connections
{
    public class DxFeed : ConnectionBase, IConnectionBase
    {


        private DxFeedConnectionOptions _options;

        //private static void DisconnectHandler(IDxConnection con)
        //{
        //    Console.WriteLine("Disconnected");
        //}

        public bool IsConnected { get; set; }
        public void Connect()
        {

            _options = (DxFeedConnectionOptions) Options;

            IsConnected = true;

            /*
            DateTime? dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var address = "";
            var token = "";
            var exchange = CandleSymbolAttributes.Exchange.NewExchange(' ');
            var period = CandleSymbolAttributes.Period.NewPeriod(periodValue, CandleType.MINUTE);

            var symbol = CandleSymbol.ValueOf("", exchange, );

            using (var con = new NativeConnection(address, token, DisconnectHandler))
            {
                using (var s = con.CreateSubscription(dateTime, new EventPrinter() ))
                {
                    s.AddSymbol(symbol);

                    
                }
            }


           
*/


            MessageBox.Show("Connect " + _options.CustomOption);
        }

        public void Disconnect()
        {
            IsConnected = false;

            MessageBox.Show("Disconnect");
        }


        public DxFeed()
        {
           
        }
    }
}