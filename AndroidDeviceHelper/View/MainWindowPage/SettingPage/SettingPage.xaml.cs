using AndroidDeviceHelper.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace AndroidDeviceHelper.View.TasksPage.SettingPage
{
    /// <summary>
    /// Interaction logic for SettingPage.xaml
    /// </summary>
    public partial class SettingPage : UserControl, IWindowPage
    {
        private ObservableCollection<string> _errors = new ObservableCollection<string>();
        public SettingPage()
        {
            InitializeComponent();
            ErrorMsgViewer.ItemsSource = _errors;
        }
        private void BrowseBackgroundPicturePath(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            //openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Picture (*.png, *.jpg, *.bmp)|*.png;*.jpg;*.bmp";
            //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() == true)
            {
                BackgroundPathInput.Text = openFileDialog.FileName;
            }
        }
        private void BrowseAdbPath(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == true)
            {
                AdbPathInput.Text = dialog.SelectedPath;
            }
        }

        public static readonly Regex DeviceWifiAddressRegex = new Regex("^[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}(:[0-9]{1,5})*$");

        private async void SaveSettings(object sender, RoutedEventArgs e)
        {
            bool isEdited = false;
            _errors.Clear();
            string backgroundPath = BackgroundPathInput.Text.Trim();
            //Check Background PATH
            if (!string.IsNullOrEmpty(backgroundPath))
            {
                ImageBrush? brush = null;
                try
                {
                    brush = new ImageBrush(new BitmapImage(new Uri(backgroundPath))) { Stretch = Stretch.UniformToFill };
                }
                catch (Exception _)
                {
                    _errors.Add("Background Image Path is invalid. Can't load image from that path.");
                }
                if (brush is not null)
                {
                    Window window = Window.GetWindow(this);
                    brush.Freeze();
                    window.Background = brush;
                    isEdited = true;
                    try
                    {
                        File.Delete(AppSettings.Current.BackgroundPath);
                    }
                    catch(Exception _) { }
                    //AppSettings.Current.BackgroundPath = "Resources/background_" + Guid.NewGuid() + backgroundPath.Substring(backgroundPath.LastIndexOf("."));
                    AppSettings.Current.BackgroundPath = "Resources/background" + backgroundPath.Substring(backgroundPath.LastIndexOf("."));
                    File.Copy(backgroundPath, AppSettings.Current.BackgroundPath, true);
                    BackgroundPathInput.Text = "";
                }
            }
            //Check Adb PATH
            string adbPath = AdbPathInput.Text.Trim();
            if(adbPath != AppSettings.Current.AdbPath)
            {
                if (string.IsNullOrEmpty(adbPath) || !Directory.Exists(adbPath))
                {
                    _errors.Add("Adb Path doesn't exist.");
                }
                else
                {
                    DirectoryInfo directory = new DirectoryInfo(adbPath);
                    if (!directory.EnumerateFiles().Any(f => f.Name == "adb.exe"))
                    {
                        _errors.Add("Adb Path doesn't contain \"adb.exe\".");
                    }
                    else
                    {
                        isEdited = true;
                        AppSettings.Current.AdbPath = adbPath;
                    }
                }
            }
            bool deviceProfileOccursError = false;
            if(DeviceProfilesViewer.IsEdited)
            {
                var deviceProfiles = DeviceProfilesViewer.DeviceProfiles;
                StringBuilder stringBuilder = new StringBuilder();
                //Check requirement field
                for (int i = 0; i < deviceProfiles.Count; i++)
                {
                    stringBuilder.Clear();
                    if (string.IsNullOrWhiteSpace(deviceProfiles[i].Name))
                    {
                        stringBuilder.Append("Name (not empty)");
                    }
                    if (deviceProfiles[i].Type == "wifi")
                    {
                        if (string.IsNullOrWhiteSpace(deviceProfiles[i].Address))
                        {
                            if(stringBuilder.Length != 0) { stringBuilder.Append(", "); }
                            stringBuilder.Append("Address (not empty, format: 'HOST[:PORT]' ex '1.1.1.1[:5050]')");
                        }
                        else if(!DeviceWifiAddressRegex.IsMatch(deviceProfiles[i].Address))
                        {
                            if (stringBuilder.Length != 0) { stringBuilder.Append(", "); }
                            stringBuilder.Append("Address (correct format: 'HOST[:PORT]' ex '1.1.1.1[:5050]'))");
                        }
                    }
                    if (stringBuilder.Length != 0)
                    {
                        _errors.Add($"Device Profiles {i + 1}th (Name: '{deviceProfiles[i].Name}') requires {stringBuilder}.");
                        deviceProfileOccursError = true;
                    }
                }

                if (!deviceProfileOccursError)
                {
                    //Check Profile Name Distinct
                    bool isProfileNameAndHashDistinct = false;

                    var hashList = deviceProfiles.Select(d => d.DeviceConnectionHash).ToArray();

                    for (int i = 0; i < deviceProfiles.Count; i++)
                    {
                        for (int j = i + 1; j < deviceProfiles.Count; j++)
                        {
                            if (deviceProfiles[i].Name == deviceProfiles[j].Name)
                            {
                                isProfileNameAndHashDistinct = true;
                                _errors.Add($"Device Profiles {i + 1}th and {j + 1}th mustn't have same name '{deviceProfiles[i].Name}'.");
                            }

                            if (hashList[i] == hashList[j])
                            {
                                isProfileNameAndHashDistinct = true;
                                _errors.Add($"Device Profiles {i + 1}th ('{deviceProfiles[i].Name}') and {j + 1}th ('{deviceProfiles[j].Name}') mustn't connect with the same device.");
                            }
                        }
                    }
                    if (!isProfileNameAndHashDistinct)
                    {
                        AppSettings.Current.DeviceProfiles.Clear();
                        foreach (var profile in deviceProfiles)
                        {
                            AppSettings.Current.DeviceProfiles.Add(profile);
                        }
                        isEdited = true;
                    }
                    else
                    {
                        deviceProfileOccursError = true;
                    }

                }

                

            }

            //Save Settings File
            if (isEdited)
            {
                using (var fileStream = File.Open("settings.json", FileMode.Create))
                {
                    await JsonSerializer.SerializeAsync<AppSettings>(fileStream, AppSettings.Current).ConfigureAwait(false);
                }
                if(!deviceProfileOccursError)
                    DeviceProfilesViewer.IsEdited = false;
                SaveNoti.Visibility = Visibility.Visible;
                await Task.Delay(5000);
                SaveNoti.Visibility = Visibility.Hidden;
            }

        }

        public void PageOpened()
        {
            DeviceProfilesViewer.DeviceProfiles = AppSettings.Current.DeviceProfiles;
            AdbPathInput.Text = AppSettings.Current.AdbPath;
            BackgroundPathInput.Text = "";
            _errors.Clear();
        }

        public void PageClosed()
        {
        }
    }
}
