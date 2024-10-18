using System.Windows;
using System.Windows.Input;
using Twm.Core.Classes;
using Twm.Core.DataProviders.Playback;



namespace Twm.Core.UI.Windows
{
    /// <summary>
    /// Логика взаимодействия для PresetNameWindow.xaml
    /// </summary>
    public partial class PlaybackWindow : Window
    {
        private readonly Playback _playback;

        public PlaybackWindow(Playback playback)
        {
            InitializeComponent();
            _playback = playback;
            DataContext = playback;
            Closed += PlaybackWindow_Closed;
        }

        private void PlaybackWindow_Closed(object sender, System.EventArgs e)
        {
            Session.Instance.Playback.Disconnect();
        }

      
        

        private void SliderOnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _playback.PlayStopCommand.Execute(false);
        }

        private void SliderOnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _playback.ResetCommand.Execute(false);

        }
    }
}
