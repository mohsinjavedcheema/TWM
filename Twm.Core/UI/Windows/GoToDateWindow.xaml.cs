using System;
using System.Windows;
using Twm.Core.DataProviders.Playback;


namespace Twm.Core.UI.Windows
{
    /// <summary>
    /// Логика взаимодействия для GoToDateWindow.xaml
    /// </summary>
    public partial class GoToDateWindow : Window
    {
        private readonly Playback _playback;

        private DateTime _currentDate;
        public DateTime CurrentDate
        {
            get { return _currentDate; }
            set
            {
                if (_currentDate != value)
                {

                    _currentDate = value;
                    
                }
            }
        }

        private DateTime _currentTime;
        public DateTime CurrentTime
        {
            get { return _currentTime; }
            set
            {
                if (_currentTime != value)
                {

                    _currentTime = value;

                }
            }
        }

        public GoToDateWindow(Playback playback)
        {
            InitializeComponent();
            _playback = playback;
            CurrentDate = playback.CurrentDate;
            CurrentTime = playback.CurrentDate;
            DataContext = this;
        }


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            _playback.CurrentDate = new DateTime(CurrentDate.Year, CurrentDate.Month, CurrentDate.Day,CurrentTime.Hour, CurrentTime.Minute, CurrentTime.Second);
            DialogResult = true;
        }
    }
}
