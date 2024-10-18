using System;
using System.Windows;
using Twm.ViewModels.Charts;

namespace Twm.Windows.Help
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow() => InitializeComponent();

        public AboutWindow(AboutViewModel aboutViewModel) : this() =>
            DataContext = aboutViewModel;

        
    }
}
