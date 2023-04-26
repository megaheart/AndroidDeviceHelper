using AdvancedSharpAdbClient;
using AndroidDeviceHelper.Models;
using AndroidDeviceHelper.View.MainWindowPage.TaskPage;
using AndroidDeviceHelper.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AndroidDeviceHelper.View.MainWindowPage.TaskPage
{
    /// <summary>
    /// Interaction logic for TaskPage.xaml
    /// </summary>
    public partial class TaskPage : UserControl, IWindowPage
    {
        private string? _curDeviceName = null;
        //private string? _curDeviceConnectionHash = null;
        //private bool isConnecting = false;
        public TaskPage()
        {
            InitializeComponent();
            var devices = AppSettings.Current.DeviceProfiles;
            ConnectionOptions_Combobox.ItemsSource = devices;
            MainTasksPanel.Visibility = Visibility.Collapsed;
            CurrentDeviceProfileViewer.Visibility = Visibility.Collapsed;

            DeviceManager.Instance.DeviceLostConnection += TaskPage_DeviceLostConnection;

            Loaded += TaskPage_Loaded;

        }

        private void TaskPage_DeviceLostConnection(Models.ConnectionState state)
        {
            ConnectingBtn.IsChecked = false;

            AlertIcon.Foreground = AlertMessage.Foreground = FindResource("Red_Foreground") as SolidColorBrush;
            AlertIcon.Text = "warning";
            AlertMessage.Text = $"Connection to device with profile name '{state.DeviceProfile.Name}' is interrupted.";
            MainTasksPanel.Visibility = Visibility.Collapsed;

            ConnectionOptions_Combobox.Visibility = Visibility.Visible;
            CurrentDeviceProfileViewer.Content = null;
            CurrentDeviceProfileViewer.Visibility = Visibility.Collapsed;

            _InstallingApkPanel.ConnectionState = null;
            _TransferingFilesPanel.ConnectionState = null;

            Application.Current.MainWindow.Topmost = true;
            Application.Current.MainWindow.Topmost = false;

        }

        public void PageClosed()
        {
            
        }

        public void PageOpened()
        {
            var devices = AppSettings.Current.DeviceProfiles;
            if (devices.Count > 0)
            {
                if (!string.IsNullOrEmpty(_curDeviceName))
                {
                    ConnectionOptions_Combobox.SelectedItem = devices.FirstOrDefault(d => d.Name == _curDeviceName);
                }

                if (ConnectionOptions_Combobox.SelectedIndex == -1)
                {
                    ConnectionOptions_Combobox.SelectedIndex = 0;
                }
                
            }
            
        }
        public async void ConnectDevice(ToggleButton sender, RoutedEventArgs e)
        {
            var devices = AppSettings.Current.DeviceProfiles;
            var curDevice = devices[ConnectionOptions_Combobox.SelectedIndex];

            if(curDevice.Type == "wifi")
            {
                sender.IsEnabled = false;
                DoubleAnimation doubleAnimation = new DoubleAnimation(360, 0, TimeSpan.FromSeconds(5), FillBehavior.Stop);
                doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
                ConnectingBtn_Icon_Tfm.BeginAnimation(RotateTransform.AngleProperty, doubleAnimation);
                var result = await DeviceManager.Instance.Connect(curDevice).ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        _InstallingApkPanel.ConnectionState = result.ConnectionState;
                        _TransferingFilesPanel.ConnectionState = result.ConnectionState;

                        _InstallingApkPanel.ResetPage();
                        _TransferingFilesPanel.ResetPage();

                        AlertIcon.Foreground = AlertMessage.Foreground = FindResource("Green_Foreground") as SolidColorBrush;
                        AlertIcon.Text = "check_circle";
                        AlertMessage.Text = "Device is connected.";
                        MainTasksPanel.Visibility = Visibility.Visible;

                        ConnectionOptions_Combobox.Visibility = Visibility.Collapsed;
                        CurrentDeviceProfileViewer.Visibility = Visibility.Visible;
                        CurrentDeviceProfileViewer.Content = result.ConnectionState.DeviceProfile;



                        //isConnecting = false;
                    }));
                }
                else
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        AlertIcon.Foreground = AlertMessage.Foreground = FindResource("Red_Foreground") as SolidColorBrush;
                        AlertIcon.Text = "warning";
                        AlertMessage.Text = result.Error;
                        MainTasksPanel.Visibility = Visibility.Collapsed;
                        sender.IsChecked = false;

                        ConnectionOptions_Combobox.Visibility = Visibility.Visible;
                        CurrentDeviceProfileViewer.Content = null;
                        CurrentDeviceProfileViewer.Visibility = Visibility.Collapsed;


                        //isConnecting = true;
                    }));
                }


                ConnectingBtn_Icon_Tfm.Dispatcher.Invoke(new Action(() =>
                {
                    ConnectingBtn_Icon_Tfm.BeginAnimation(RotateTransform.AngleProperty, null);
                }));
                sender.Dispatcher.Invoke(new Action(() =>
                {
                    sender.IsEnabled = true;
                }));
            }

            e.Handled = true;
        }
        
        public void DisconnectDevice(ToggleButton sender, RoutedEventArgs e)
        {
            AlertIcon.Foreground = AlertMessage.Foreground = FindResource("Yellow_Foreground") as SolidColorBrush;
            AlertIcon.Text = "info";
            AlertMessage.Text = "Device is NOT connected. Please click <Connect> button";
            MainTasksPanel.Visibility = Visibility.Collapsed;

            ConnectionOptions_Combobox.Visibility = Visibility.Visible;
            CurrentDeviceProfileViewer.Content = null;
            CurrentDeviceProfileViewer.Visibility = Visibility.Collapsed;

            _InstallingApkPanel.ConnectionState = null;
            _TransferingFilesPanel.ConnectionState = null;

            e.Handled = true;
        }

        private void ConnectingBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton)sender;
            var _checked = btn.IsChecked;
            if(_checked == true)
            {
                ConnectDevice(btn, e);
            }
            else
            {
                DisconnectDevice(btn, e);
            }
        }

        private void TaskPage_Loaded(object sender, RoutedEventArgs e)
        {
            ConnectionOptions_Combobox.SelectionChanged += ConnectionOptions_Combobox_SelectionChanged;
        }

        private void ConnectionOptions_Combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //var devices = AppSettings.Current.DeviceProfiles;

            if (ConnectionOptions_Combobox.SelectedIndex > -1)
            {
                //var curDevice = devices[ConnectionOptions_Combobox.SelectedIndex];
                //_curDeviceName = curDevice.Name;
                AlertIcon.Foreground = AlertMessage.Foreground = FindResource("Yellow_Foreground") as SolidColorBrush;
                AlertIcon.Text = "info";
                AlertMessage.Text = "Device is NOT connected. Please click <Connect> button";
                MainTasksPanel.Visibility = Visibility.Collapsed;
            }
        }
    }
}
