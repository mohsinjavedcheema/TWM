using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;

namespace Twm.Chart.Interfaces
{
    public interface ISeries<T>
    {

        string Name { get; set; }
        
        int Count { get;}

        T GetValueAt(int index);

        //  List<ISeriesValue<T>> Data { get; set; }

        T[] GetRange(int startIndex, int count);

        T this[int key] { get; set; }

        bool IsValidPoint(int index);

        void SetValid(int index);

        void Clear();

        void Reset();

    }
}