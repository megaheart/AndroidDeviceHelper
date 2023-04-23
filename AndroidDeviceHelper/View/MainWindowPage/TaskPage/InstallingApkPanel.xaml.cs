using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using AndroidDeviceHelper.Models;
using AndroidDeviceHelper.ViewModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AndroidDeviceHelper.View.MainWindowPage.TaskPage
{
    /// <summary>
    /// Interaction logic for InstallingApkPanel.xaml
    /// </summary>
    public partial class InstallingApkPanel : UserControl, IConnectionStateDependency
    {
        private ObservableCollection<string> _errors = new ObservableCollection<string>();
        PowershellIO powershellIO = null;
        public InstallingApkPanel()
        {
            InitializeComponent();
            ErrorMsgViewer.ItemsSource = _errors;
            powershellIO = new PowershellIO();
        }
        //public static readonly DependencyProperty ConnectionStateProperty = DependencyProperty.Register("ConnectionState", typeof(ConnectionState), typeof(InstallingApkPanel), new UIPropertyMetadata(null));
        //public ConnectionState ConnectionState
        //{
        //    set => SetValue(ConnectionStateProperty, value);
        //    get => (ConnectionState)GetValue(ConnectionStateProperty);
        //}

        public ConnectionState ConnectionState { get; set; }

        public void ResetPage()
        {
            _errors.Clear();
            ApkPathInput.Text = "";
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
        private async void StartInstallingApkFile(object sender, RoutedEventArgs e)
        {
            var apkPath = ApkPathInput.Text.Trim();
            if (string.IsNullOrEmpty(apkPath)) return;
            _errors.Clear();

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

            var btn = e.Source as Button;
            btn.IsEnabled = false;
            ApkPathInput.IsEnabled = false;
            BrowseApkBtn.IsEnabled = false;
            APKLoadingBar.Visibility = Visibility.Visible;

            var adbPath = AppSettings.Current.AdbPath;
            var connState = ConnectionState;
            //var isSuccess = false;
            ConcurrentBag<string> error = new ConcurrentBag<string>();

            var isSuccess = await InstallApkFileAsync(adbPath, apkPath, connState, error).ConfigureAwait(false);

            Dispatcher.Invoke(new Action(() =>
            {
                btn.IsEnabled = true;
                ApkPathInput.IsEnabled = true;
                BrowseApkBtn.IsEnabled = true;
                APKLoadingBar.Visibility = Visibility.Collapsed;

                if (isSuccess && error.Count == 0)
                {
                    InstallAPK_Successfully.Visibility = Visibility.Visible;
                }
                else
                {
                    while(error.TryTake(out var err))
                    {
                        _errors.Add(err);
                    }
                }
            }));

            await Task.Delay(4000).ConfigureAwait(false);

            InstallAPK_Successfully.Dispatcher.Invoke(new Action(() =>
            {
                InstallAPK_Successfully.Visibility = Visibility.Collapsed;
            }));
        }

        private async Task<bool> InstallApkFileAsync(string adbPath, string apkPath, ConnectionState connState, ConcurrentBag<string> error)
        {
            var device = connState.DeviceData;

            AdbServer server = new AdbServer();

            try
            {
                var result = server.StartServer($"{adbPath}\\adb.exe", false);
            }
            catch (Exception ex)
            {
                error.Add("ADB Path \"{adbPath}\" doesn't exist in storage or doesn't contain \"adb.exe\". Please check ADB Path or go to setting panel and change it.");
                return false;
            }

            var client = new AdbClient();

            PackageManager manager = null;

            try
            {
                manager = new PackageManager(client, device);
            }
            catch (Exception e)
            {
                var result = await DeviceManager.Instance.NotifyDeviceLostConnection(connState);

                if (result.IsSuccess)
                {
                    ConnectionState = result.ConnectionState;
                    connState = result.ConnectionState;
                    device = connState.DeviceData;

                    manager = new PackageManager(client, device);
                }
                else
                {
                    return false;
                }
            }

            try
            {
                manager.InstallPackage(apkPath, reinstall: true);
            }
            catch (Exception ex)
            {
                error.Add("Installation fail. This apk file may be invalid format or fake. If you ensure that apk file is valid, please contact developers to get more information.");
                return false;
            }
            return true;
        }

        private void InstallApkFile_Legacy(DeviceProfile deviceProfile)
        {
            var apkPath = ApkPathInput.Text.Trim();
            if (string.IsNullOrEmpty(apkPath)) return;


            var isSuccess = false;
            _errors.Clear();

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

            //var btn = e.Source as Button;
            //btn.IsEnabled = false;
            //APKLoadingBar.Visibility = Visibility.Visible;

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
                //btn.Dispatcher.Invoke(new Action(() =>
                //{
                //    btn.IsEnabled = true;
                //}));
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
