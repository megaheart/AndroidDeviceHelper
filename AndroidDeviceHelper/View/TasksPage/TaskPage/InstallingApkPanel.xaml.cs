using AndroidDeviceHelper.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AndroidDeviceHelper.View.TasksPage.TaskPage
{
    /// <summary>
    /// Interaction logic for InstallingApkPanel.xaml
    /// </summary>
    public partial class InstallingApkPanel : UserControl, IDeviceProfileDependency
    {
        private ObservableCollection<string> _errors = new ObservableCollection<string>();
        public InstallingApkPanel()
        {
            InitializeComponent();
            ErrorMsgViewer.ItemsSource = _errors;
        }
        public static readonly DependencyProperty DeviceProfileProperty = DependencyProperty.Register("DeviceProfile", typeof(DeviceProfile), typeof(InstallingApkPanel), new UIPropertyMetadata(null));
        public DeviceProfile DeviceProfile
        {
            set => SetValue(DeviceProfileProperty, value);
            get => (DeviceProfile)GetValue(DeviceProfileProperty);
        }
        private void BrowseApkFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            //openFileDialog.Multiselect = true;
            openFileDialog.Filter = "APK file|*.apk";
            //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() == true)
            {
                ApkPathInput.Text = openFileDialog.FileName;
            }
        }
        private void InstallApkFile(object sender, RoutedEventArgs e)
        {
            var apkPath = ApkPathInput.Text.Trim();
            if (string.IsNullOrEmpty(apkPath)) return;
            _errors.Clear();
            var deviceProfile = DeviceProfile;
            var adbPath = AppSettings.Current.AdbPath;
            var dotPos = apkPath.LastIndexOf(".");
            if(dotPos == -1 || apkPath.Substring(dotPos + 1) != "apk")
            {
                _errors.Add("File input is not .apk file.");
                return;
            }
            if(!File.Exists(apkPath))
            {
                _errors.Add("APK File path doesn't exist.");
                return;
            }

            Process p = new Process();
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd.exe";
            info.RedirectStandardInput = true;
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.StandardErrorEncoding = Encoding.UTF8;
            info.StandardInputEncoding = Encoding.UTF8;
            info.StandardOutputEncoding = Encoding.UTF8;

            p.StartInfo = info;
            p.Start();
            p.OutputDataReceived += (sender, e) => { Console.WriteLine(e.Data); };
            p.ErrorDataReceived += (sender, e) => { Console.WriteLine(e.Data); };
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            if (deviceProfile.Type != "wifi") { throw new Exception("Not support other connection type except 'wifi'."); }
            using (StreamWriter sw = new StreamWriter(p.StandardInput.BaseStream, Encoding.UTF8))
            {
                if (sw.BaseStream.CanWrite)
                {
                    sw.WriteLine();
                    Console.WriteLine($"cd \"{adbPath}\"");
                    sw.WriteLine($"cd \"{adbPath}\"");

                    Console.WriteLine($"adb.exe connect {deviceProfile.Address}"); 
                    sw.WriteLine($"adb.exe connect {deviceProfile.Address}");

                    Console.WriteLine($"adb.exe install \"{apkPath}\"");
                    sw.WriteLine($"adb.exe install \"{apkPath}\"");


                }
                else
                {
                    _errors.Add("File input is not .apk file.");
                    return;
                }
            }
            p.WaitForExit();
        }
    }
}
