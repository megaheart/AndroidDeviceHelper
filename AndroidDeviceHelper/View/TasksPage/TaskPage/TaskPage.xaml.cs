using AndroidDeviceHelper.Models;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for TaskPage.xaml
    /// </summary>
    public partial class TaskPage : UserControl, IWindowPage
    {
        private string? _curDeviceName = null;
        public TaskPage()
        {
            InitializeComponent();
            var devices = AppSettings.Current.DeviceProfiles;
            ConnectionOptions_Combobox.ItemsSource = devices;
        }

        public void ResetPage()
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
                if (string.IsNullOrEmpty(_curDeviceName))
                {
                    _curDeviceName = devices[ConnectionOptions_Combobox.SelectedIndex].Name;
                }
            }
        }
    }
}
