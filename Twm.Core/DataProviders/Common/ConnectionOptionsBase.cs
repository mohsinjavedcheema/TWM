using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using Twm.Core.ViewModels;

namespace Twm.Core.DataProviders.Common
{
    public class ConnectionOptionsBase
    {
        [Display(Name = "Name", GroupName = "Main", Order = 1)]
        public virtual string Name { get; set; }


        [Display(Name = "DataProvider", GroupName = "Main", Order = 0)]
        [ReadOnly(true)]
        public virtual string DataProvider { get; set; }

        
        [Browsable(false)]
        public virtual int DataProviderId { get; set; }
    }
}