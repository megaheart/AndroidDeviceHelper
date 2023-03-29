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
            var isSuccess = false;
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

            var btn = e.Source as Button;
            btn.IsEnabled = false;
            APKLoadingBar.Visibility = Visibility.Visible;

            Process p = new Process();
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "powershell.exe";
            info.RedirectStandardInput = true;
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.StandardErrorEncoding = Encoding.UTF8;
            info.StandardInputEncoding = Encoding.UTF8;
            info.StandardOutputEncoding = Encoding.UTF8;

            p.StartInfo = info;
            p.Start();
            List<string> errors = new List<string>();
            p.OutputDataReceived += (sender, e) => { 
                string data = e.Data;
                if(data != null )
                {
                    Console.WriteLine(data);
                    if (data.Contains("(10061)"))
                    {
                        var lastline = data.Substring(e.Data.LastIndexOf("\n") + 1);
                        lock (errors)
                        {
                            errors.Add($"{lastline}\nPlease check your device whether it is active, check the connection, or check the IP Address of device profile at settings tab.");
                        }
                    }
                    else if (data.Contains("Success"))
                    {
                        isSuccess = true;
                    }
                }
                
            };
            p.ErrorDataReceived += (sender, e) => {
                string data = e.Data;
                if (data != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Data);
                    Console.ForegroundColor = ConsoleColor.White;
                    if (data.Contains("INSTALL_PARSE_FAILED_NOT_APK"))
                    {
                        lock (errors)
                        {
                            errors.Add($"Failed to parse apk file. This file is invalid apk format.");
                        }
                    }
                }
                
            };
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            if (deviceProfile.Type != "wifi") { throw new Exception("Not support other connection type except 'wifi'."); }
            using (StreamWriter sw = p.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    //sw.WriteLine();
                    Console.WriteLine($"cd \"{adbPath}\"");
                    sw.WriteLine($"cd \"{adbPath}\"");

                    Console.WriteLine($"./adb.exe connect {deviceProfile.Address}"); 
                    sw.WriteLine($"./adb.exe connect {deviceProfile.Address}");

                    Console.WriteLine($"./adb.exe install \"{apkPath}\"");
                    sw.WriteLine($"./adb.exe install \"{apkPath}\"");


                }
                else
                {
                    _errors.Add("Shell input stream cannot be written.");
                    return;
                }
            }

            p.WaitForExitAsync().ContinueWith(t=> {
                btn.Dispatcher.Invoke(new Action(() =>
                {
                    btn.IsEnabled = true;
                }));
                APKLoadingBar.Dispatcher.Invoke(new Action(() =>
                {
                    APKLoadingBar.Visibility = Visibility.Collapsed;
                }));
                p.Dispose();
                lock (errors)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        errors.ForEach(errors => _errors.Add(errors));
                    }));
                    
                }
                if (isSuccess)
                {
                    InstallAPK_Successfully.Dispatcher.Invoke(new Action(() =>
                    {
                        InstallAPK_Successfully.Visibility = Visibility.Visible;
                    }));
                }
                else if(errors.Count == 0)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        _errors.Add("Unknown Error(s) occur.");
                    }));
                }
            });

            
        }
    }
}
