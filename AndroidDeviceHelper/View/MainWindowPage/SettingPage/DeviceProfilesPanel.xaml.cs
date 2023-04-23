using AndroidDeviceHelper.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace AndroidDeviceHelper.View.MainWindowPage.SettingPage
{
    /// <summary>
    /// Interaction logic for DeviceProfilesPanel.xaml
    /// </summary>
    public partial class DeviceProfilesPanel : UserControl
    {
        private ObservableCollection<DeviceProfile> _profiles;
        public bool IsEdited { get; set; } = false;
        public DeviceProfilesPanel()
        {
            InitializeComponent();
            //_profiles = new ObservableCollection<DeviceProfile>(new DeviceProfile[]
            //{
            //    new DeviceProfile()
            //    {
            //        Name = "Default",
            //        Address = "https 1554",
            //        Type = "wifi"
            //    },
            //    new DeviceProfile()
            //    {
            //        Name = "X1 abc dshdhj dfsds sds ",
            //        Address = "hello",
            //        Type = "wifi"
            //    },
            //    new DeviceProfile()
            //    {
            //        Name = "X222 FFFF JHBJH HBHB ",
            //        Address = "hvbsadhjabsd",
            //        Type = "wifi"
            //    },
            //});
            //ProfileOptionsPanel.ItemsSource = _profiles;
            ProfileEditingPanel.Visibility = Visibility.Collapsed;
            ProfileNameInput.TextChanged += Input_TextChanged;
            NetworkAddressInput.TextChanged += Input_TextChanged;
        }
        bool switchingProfile = false;
        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (switchingProfile) { return; }
            IsEdited = true;
        }
        public IList<DeviceProfile> DeviceProfiles
        {
            get => _profiles.Select(p => new DeviceProfile() { Name = p.Name, Type = p.Type, Address = p.Address }).ToList();
            set
            {
                _profiles = null;
                _profiles = new ObservableCollection<DeviceProfile>(value.Select(
                    p => new DeviceProfile()
                    {
                        Name = p.Name,
                        Type = p.Type,
                        Address = p.Address
                    }));
                ProfileOptionsPanel.ItemsSource = _profiles;
                IsEdited = false;
            }
        }
        private void Add_Profile(object sender, RoutedEventArgs e)
        {
            var profile = new DeviceProfile() { Name = "Unknown", Type = "wifi", Address = "" };
            _profiles.Add(profile);
            ProfileOptionsPanel.SelectedIndex = _profiles.Count - 1;
            IsEdited = true;
        }
        private void Remove_Profile(object sender, RoutedEventArgs e)
        {
            Button btn = e.Source as Button;
            var device = btn.DataContext as DeviceProfile;
            _profiles.Remove(device);
            IsEdited = true;
            e.Handled = true;
        }
        private DeviceProfile _curDevice;
        private void SwitchToProfile(DeviceProfile device)
        {
            switchingProfile = true;


            ProfileEditingPanel.Visibility = Visibility.Visible;
            BindingOperations.ClearBinding(ProfileNameInput, TextBox.TextProperty);

            if (_curDevice != null)
            {
                RemoveProfileTypeInput(_curDevice.Type);
            }
            _curDevice = device;
            if (device == null)
            {
                ProfileEditingPanel.Visibility = Visibility.Collapsed;
                return;
            }
            ProfileNameInput.SetBinding(TextBox.TextProperty, new Binding("Name") { Source = device });
            var _conn_types_idx = Array.IndexOf(_conn_types, device.Type);
            if (_conn_types_idx != ConnectionOptionInput.SelectedIndex)
            {
                ConnectionOptionInput.SelectedIndex = _conn_types_idx;
            }
            else
            {
                AddProfileTypeInput(device);
            }

            switchingProfile = false;
        }
        private void ProfileOptionsPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SwitchToProfile(ProfileOptionsPanel.SelectedItem as DeviceProfile);
        }
        //private void Profile_Choosed(object sender, RoutedEventArgs e)
        //{
        //    RadioButton btn = e.Source as RadioButton;

        //    var device = btn.DataContext as DeviceProfile;

        //    SwitchToProfile(device);
        //}
        string[] _conn_types = new string[]
        {
            "wifi"
        };
        private void RemoveProfileTypeInput(string type)
        {
            switch (type)
            {
                case "wifi":
                    BindingOperations.ClearBinding(NetworkAddressInput, TextBox.TextProperty);
                    //NetworkAddressInput_Binding = null;
                    NetworkAddressInput.Text = "";
                    break;
            }
        }
        private void AddProfileTypeInput(DeviceProfile profile)
        {
            switch (profile.Type)
            {
                case "wifi":
                    //NetworkAddressInput_Binding = new Binding("Address") { Source = profile };
                    NetworkAddressInput.SetBinding(TextBox.TextProperty, new Binding("Address") { Source = profile });
                    break;
            }
        }
        private void ConnectionOptionInput_Selected(object sender, SelectionChangedEventArgs e)
        {
            if (_curDevice is null)
            {
                return;
            }
            RemoveProfileTypeInput(_curDevice.Type);
            _curDevice.Type = _conn_types[ConnectionOptionInput.SelectedIndex];
            AddProfileTypeInput(_curDevice);
        }

        private void ProfileOptionsGroup_Click(object sender, RoutedEventArgs e)
        {
            var radioBtn = e.Source as RadioButton;
            var grid = VisualTreeHelper.GetParent(radioBtn) as Grid;
            var listBoxItem = VisualTreeHelper.GetParent(grid) as ListBoxItem;
            listBoxItem.IsSelected = true;

        }
    }
}
