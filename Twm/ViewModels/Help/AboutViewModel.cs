using System;
using System.Reflection;
using Twm.Core.ViewModels;

namespace Twm.ViewModels.Charts
{
    public class AboutViewModel : ViewModelBase
    {
        
        public string Version { get; set; }
        

        public AboutViewModel()
        {

            Version = GetVersion().ToString();




        }


        private Version GetVersion()
        {
            Assembly runningAssembly = Assembly.GetEntryAssembly();
            if (runningAssembly == null)
            {
                runningAssembly = Assembly.GetExecutingAssembly();
            }
            return runningAssembly.GetName().Version;
        }


    }
}