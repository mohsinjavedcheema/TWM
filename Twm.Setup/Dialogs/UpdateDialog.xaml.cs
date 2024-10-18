using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using WixSharp;
using WixSharp.UI.Forms;

namespace WixSharp.UI.WPF
{
    /// <summary>
    /// The standard InstallDirDialog.
    /// <para>Follows the design of the canonical Caliburn.Micro View (MVVM).</para>
    /// <para>See https://caliburnmicro.com/documentation/cheat-sheet</para>
    /// </summary>
    /// <seealso cref="WixSharp.UI.WPF.WpfDialog" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    /// <seealso cref="WixSharp.IWpfDialog" />
    public partial class UpdateDialog : WpfDialog, IWpfDialog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InstallDirDialog"/> class.
        /// </summary>
        public UpdateDialog()
        {
            InitializeComponent();
                      
        }

        /// <summary>
        /// This method is invoked by WixSHarp runtime when the custom dialog content is internally fully initialized.
        /// This is a convenient place to do further initialization activities (e.g. localization).
        /// </summary>
        public void Init()
        {
            var session = ManagedFormHost.Runtime.Session;            
            UpdateDlgCaptionLabel.Text = session?.Property("UpdateDlgCaptionLabel");
            UpdateDlgFromVersionLabel.Text = session?.Property("UpdateDlgFromVersionLabel") + ": " + session?.Property("CURRENT_VERSION");
            UpdateDlgToVersion.Text = session?.Property("UpdateDlgToVersion") + ": " + session?.Property("NEW_VERSION");

            ViewModelBinder.Bind(new UpdateDialogModel { Host = ManagedFormHost, }, this, null);
        }
    }

    /// <summary>
    /// ViewModel for standard InstallDirDialog.
    /// <para>Follows the design of the canonical Caliburn.Micro ViewModel (MVVM).</para>
    /// <para>See https://caliburnmicro.com/documentation/cheat-sheet</para>
    /// </summary>
    class UpdateDialogModel : Caliburn.Micro.Screen
    {
        public ManagedForm Host;
        ISession session => Host?.Runtime.Session;
        IManagedUIShell shell => Host?.Shell;

        public BitmapImage Banner => session?.GetResourceBitmap("WixUI_Bmp_Banner").ToImageSource();

    
        public void GoPrev()
            => shell?.GoPrev();

        public void GoNext()
            => shell?.GoNext();

        public void Cancel()
            => shell?.Cancel();
    }
}