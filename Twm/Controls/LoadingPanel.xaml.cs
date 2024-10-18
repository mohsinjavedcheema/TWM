using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Twm.Core.Annotations;

namespace Twm.Controls
{
    /// <summary>
    /// Interaction logic for LoadingPanel.xaml
    /// </summary>
    public partial class LoadingPanel: INotifyPropertyChanged
    {

        public static readonly DependencyProperty IsLoadingProperty
            = DependencyProperty.Register("IsLoading", typeof(bool), typeof(LoadingPanel), new UIPropertyMetadata(false, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is LoadingPanel loadingPanel)
            {

                if ((bool) e.NewValue)
                    loadingPanel.Visibility = Visibility.Visible;
                else
                    loadingPanel.Visibility = Visibility.Hidden;
            }
        }


        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.RegisterAttached("Message", typeof(string), typeof(LoadingPanel),
                new UIPropertyMetadata("", propertyChangedCallback: null)
            );

        public static readonly DependencyProperty SubMessageProperty =
            DependencyProperty.RegisterAttached("SubMessage", typeof(string), typeof(LoadingPanel),
                new UIPropertyMetadata("", propertyChangedCallback: null)
            );

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value);}
        }

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set
            {
                SetValue(MessageProperty, value);                
            }
        }

        public string SubMessage
        {
            get { return (string)GetValue(SubMessageProperty); }
            set
            {
                SetValue(SubMessageProperty, value);
            }
        }     

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadingPanel"/> class.
        /// </summary>
        public LoadingPanel()
        {
            InitializeComponent();
            Visibility = Visibility.Hidden;
         
        }

   
       

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}