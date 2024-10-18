using System.Collections.Generic;
using System.Windows;
using Twm.Core.Classes;

namespace Twm.Core.UI.Windows
{
    /// <summary>
    /// Логика взаимодействия для ViewSettingsWindow.xaml
    /// </summary>
    public partial class ViewSettingsWindow : Window
    {
        

        public ViewSettingsWindow(List<GridColumnInfo> columnInfos)
        {
            InitializeComponent();
            DataContext = columnInfos;
        }

     

        
        private void ButtonOKClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
