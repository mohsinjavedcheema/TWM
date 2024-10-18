using Twm.Core.DataCalc;

namespace Twm.Interfaces
{
    public interface IDomViewModel
    {

      
        DataCalcContext DataCalcContext { get; set; }

        void DoAutoCenter();

        bool AutoCentering { get; set; }

      

        //    string SubscriberCode { get; set; }

        void Subscribe();

        void Unsubscribe();

       // string SelectedInstrument { get; set; }

       // Window Window { get; set; }
    }
}
