using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using WixSharp;
using WixSharp.CommonTasks;
using WixSharp.UI.WPF;
using InstallDirDialog = WixSharp.UI.WPF.InstallDirDialog;

namespace Twm.Setup
{
    class Script
    {
        
         static string product = "Twm";
        static void Main()
        {
            string lang = "en-US";

            AutoElements.DisableAutoKeyPath = true;
            var fileName = Environment.CurrentDirectory + $"\\Assets\\lang.{lang}.xaml";
            if (!System.IO.File.Exists(fileName))
            {
                Environment.CurrentDirectory = @"..\..\";
                fileName = Environment.CurrentDirectory + $"\\Assets\\lang.{lang}.xaml";
            }
            
            var resDict = new ResourceDictionary();

            

            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {                
                resDict = (ResourceDictionary)XamlReader.Load(fs);                
            }

            //resDict.Source = new Uri($"/Twm.Setup;component/Assets/lang.{lang}.xaml", UriKind.RelativeOrAbsolute);


            
            var project = new ManagedProject("Twm",
                new Dir(new Id("BINARIES_DIR"),  "BINARIES_DIR",
                    new Files(@"..\Twm\bin\ObfuscatedRelease\*.*", f => !f.Contains("*.pdb"))                    
                ),
                new Dir(new Id("DATA_DIR"), "DATA_DIR",
                    new Dir("bin", new Dir("Custom", new Files(@"..\Twm.Custom\*.*", f =>
                    f.Contains("bin") ||
                    f.Contains("Commissions")||
                    f.Contains("Indicators") ||                    
                    f.Contains("OptimizationFitnesses") ||
                    f.Contains("Optimizers") ||
                    f.Contains("Properties") ||
                    f.Contains("Strategies") ||
                    f.Contains("app.config") ||
                    f.Contains("packages.config") ||
                    f.Contains("Twm.Custom.csproj")
                    ), new Dir("lib", new Files(@"..\Twm\bin\ObfuscatedRelease\*.*", f=> f.Contains("Twm.Chart.dll")||f.Contains("Twm.Lib.dll")))
                    
                    ))
                )
            );

            project.AddProperty(new Property("NEW_INSTALL", "1") { IsDeferred = true});
            project.AddProperty(new Property("VersionAlreadyInstalled", resDict["VersionAlreadyInstalled"].ToString()) );
            project.AddProperty(new Property("ERROR1_1", resDict["Error1_1"].ToString()));
            project.AddProperty(new Property("ERROR1_2", resDict["Error1_2"].ToString()));

            project.AddProperty(new Property("UpdateDlgCaptionLabel", resDict["UpdateDlgCaptionLabel"].ToString()));
            project.AddProperty(new Property("UpdateDlgFromVersionLabel", resDict["UpdateDlgFromVersionLabel"].ToString()));
            project.AddProperty(new Property("UpdateDlgToVersion", resDict["UpdateDlgToVersion"].ToString()));


            project.AddRegValue(new RegValue(new Id("REG_BINARIES_DIR"),
                    WixSharp.RegistryHive.LocalMachine,
                    $"Software\\{product}\\{product}",
                    "Path", "[BINARIES_DIR]"));


            project.LicenceFile = @"";
            project.Language = lang;
            project.OutDir = @"" + lang;
            project.GUID = new Guid("75359C66-639A-4A8F-90C4-88496C43ADD8");
            project.LocalizationFile = "WixUI_" + lang + ".wxl";
            project.Language = lang;
            project.ResolveWildCards();

            project.ControlPanelInfo.NoRepair = false;
            //project.ControlPanelInfo.ProductIcon = "Icon.ico";
            project.ControlPanelInfo.NoModify = false;
            project.ControlPanelInfo.Manufacturer = product;
            project.Version = new Version(ReadVersion("..\\CommonAssemblyInfo.cs"));



            project.UpgradeCode = new Guid("20B81887-EF7B-42B4-9CB7-78036093997B");

            project.MajorUpgrade = new MajorUpgrade
            {
                AllowDowngrades = false,
                MigrateFeatures = true,
                DowngradeErrorMessage =
                    "A later version of [ProductName] is already installed. Setup will now exit."
            };


            
            project.Language = lang;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
            project.ManagedUI = new ManagedUI();

            //removing all entry dialogs and install dir
            project.ManagedUI.InstallDialogs.Add<WelcomeDialog>()
                .Add<InstallDirDialog>()
                .Add<UpdateDialog>()
                .Add<ProgressDialog>()
                .Add<ExitDialog>();

      
            project.UIInitialized += Project_UIInitialized;
            project.UILoaded += Project_UILoaded;

            project.OutFileName = "twm";

            project.FindFile(f => f.Name.EndsWith("Twm.exe"))
                .First()
                .Shortcuts = new[]
            {
                new FileShortcut("Twm", "BINARIES_DIR"),
                new FileShortcut("Twm", "%Desktop%")/*,
                new FileShortcut("Twm", @"%ProgramMenu%\Twm")*/
            };

            Compiler.BuildMsi(project);

        }


        private static string ReadVersion(string path)
        {
        
            if (System.IO.File.Exists(path))
            {
                // Open the file to read from.
                string[] readText = System.IO.File.ReadAllLines(path);
                var versionInfoLines = readText.Where(t => t.Contains("[assembly: AssemblyVersion"));
                foreach (string item in versionInfoLines)
                {
                    string version = item.Substring(item.IndexOf('(') + 2, item.LastIndexOf(')') - item.IndexOf('(') - 3);
                    //Console.WriteLine(Regex.Replace(version, @"\P{S}", string.Empty));
                    return version;
                }

            }
            return "";
        }

        private static void Project_UIInitialized(SetupEventArgs e)
        {
            e.Session["WixSharp_UI_INSTALLDIR"] = "BINARIES_DIR";
            

            Version installedVersion = e.Session.LookupInstalledVersion();
            Version thisVersion = e.Session.QueryProductVersion();

            if (thisVersion <= installedVersion)
            {
                MessageBox.Show(e.Session["VersionAlreadyInstalled"] +" "+ installedVersion);

                e.ManagedUI.Shell.ErrorDetected = true;
                e.Result = ActionResult.UserExit;
            }


            e.Session["DATA_DIR"] = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"\\{product}";
            if (string.IsNullOrEmpty("" + installedVersion))
            {
                //New install
                e.Session["BINARIES_DIR"] = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + $"\\{product}";
                e.Session["NEW_INSTALL"] = "1";
            }
            else
            {
                var installDir = "";
                try
                {
                    var subKey = Registry.LocalMachine.OpenSubKey($"Software\\{product}\\{product}");
                    if (subKey != null)
                    {
                        installDir = subKey.GetValue("Path").ToString();
                    }
                }
                catch
                {
                    MessageBox.Show(e.Session["ERROR1_1"] + installedVersion + e.Session["ERROR1_2"] + thisVersion);

                    e.ManagedUI.Shell.ErrorDetected = true;
                    e.Result = ActionResult.UserExit;
                }

                //Update
                e.Session["CURRENT_VERSION"] = "" + installedVersion;
                e.Session["NEW_VERSION"] = "" + thisVersion;
                e.Session["BINARIES_DIR"] = installDir;
                e.Session["NEW_INSTALL"] = "0";
            }
        }

        private static void Project_UILoaded(SetupEventArgs e)
        {
            if (e.Session["NEW_INSTALL"] == "1")
            {
                //New Install
                e.ManagedUI.Shell.Dialogs.Remove(typeof(UpdateDialog));
            }
            else
            {
                //Update Install
                //e.ManagedUI.Shell.Dialogs.Remove(typeof(LicenceDialog));
                e.ManagedUI.Shell.Dialogs.Remove(typeof(InstallDirDialog));
            }
        }
    }
}
