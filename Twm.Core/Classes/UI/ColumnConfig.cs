using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Twm.Core.Classes.UI
{
    public class ColumnConfig
    {
        public Dictionary<int, int> Widths{ get; set; }

        public Dictionary<int, string> Headers { get; set; }

        public ColumnConfig()
        {
            Headers = new Dictionary<int, string>();
            Widths = new Dictionary<int, int>();
        }

        public void Clear()
        {
            Headers.Clear();
            Widths.Clear();
            for (int i = 0; i < 30; i++)
            {
                Headers.Add(i, "");
                Widths.Add(i, 0);
            }
        }
    }
}