using System.Collections.Generic;
using Twm.Model.Model;

namespace Twm.Core.Classes
{
    public class Theme
    {
        public string Name { get; set; }
        public List<SystemOption> Options { get; set; }
        public bool IsSelected { get; set; }

        public bool IsDark { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
