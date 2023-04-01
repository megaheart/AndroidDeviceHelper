using AndroidDeviceHelper.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AndroidDeviceHelper.View.TasksPage.TaskPage
{
    /// <summary>
    /// Interaction logic for InstallingApkPanel.xaml
    /// </summary>
    public partial class InstallingApkPanel : UserControl, IDeviceProfileDependency
    {
        private ObservableCollection<string> _errors = new ObservableCollection<string>();
        PowershellIO powershellIO = null;
        public InstallingApkPanel()
        {
            InitializeComponent();
            ErrorMsgViewer.ItemsSource = _errors;
            powershellIO = new PowershellIO();
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
            if (dotPos == -1 || apkPath.Substring(dotPos + 1) != "apk")
            {
                _errors.Add("File input is not .apk file.");
                return;
            }
            if (!File.Exists(apkPath))
            {
                _errors.Add("APK File path doesn't exist.");
                return;
            }
            if (deviceProfile.Type != "wifi") { throw new Exception("Not support other connection type except 'wifi'."); }

            var btn = e.Source as Button;
            btn.IsEnabled = false;
            APKLoadingBar.Visibility = Visibility.Visible;

            List<string> errors = new List<string>();


            powershellIO.Reset();
            powershellIO.AddCommand($"cd \"{adbPath}\"", e =>
            {
                e.PrintToConsole();
                if (e.Error.Length > 0 && e.Error.First().Contains("Cannot find path"))
                {
                    lock (errors)
                    {
                        errors.Add($"ADB Path \"{adbPath}\" is not exist in storage. Please go to setting panel and change ADB Path.");
                    }
                }
            });
            powershellIO.AddCommand($"./adb.exe connect {deviceProfile.Address}", e =>
            {
                e.PrintToConsole();
                if (e.Output.Length == 0) return;
                var lastline = e.Output.Last();
                if (e.Error.Length > 0 && e.Error.First().Contains("The term './adb.exe' is not recognized"))
                {
                    lock (errors)
                    {
                        errors.Add($"ADB Path \"{adbPath}\" doesn't contain \"adb.exe\". Please go to setting panel and change ADB Path.");
                    }
                }
                else if (lastline.Contains("(10061)"))
                {
                    lock (errors)
                    {
                        errors.Add($"{lastline}\nPlease check your device whether it is active, check the connection, or check the IP Address of device profile at settings tab.");
                    }
                }

            });
            powershellIO.AddCommand($"./adb.exe install \"{apkPath}\"", e =>
            {
                e.PrintToConsole();
                if (e.Error.Length > 0 && e.Error.Last().Contains("INSTALL_PARSE_FAILED_NOT_APK"))
                {
                    lock (errors)
                    {
                        errors.Add($"Failed to parse apk file. This file is invalid apk format.");
                    }
                }
                if (e.Output.Last().Contains("Success"))
                {
                    isSuccess = true;
                }
            });
            powershellIO.ExecuteAsync().ContinueWith(t =>
            {
                btn.Dispatcher.Invoke(new Action(() =>
                {
                    btn.IsEnabled = true;
                }));
                APKLoadingBar.Dispatcher.Invoke(new Action(() =>
                {
                    APKLoadingBar.Visibility = Visibility.Collapsed;
                }));
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
                    Task.Delay(4000).ContinueWith(t =>
                    {
                        InstallAPK_Successfully.Dispatcher.Invoke(new Action(() =>
                        {
                            InstallAPK_Successfully.Visibility = Visibility.Collapsed;
                        }));
                    }).Wait();
                }
                else if (errors.Count == 0)
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
