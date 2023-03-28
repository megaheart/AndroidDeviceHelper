using AndroidDeviceHelper.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
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
        public void ResetPage()
        {
            DeviceProfilesViewer.DeviceProfiles = AppSettings.Current.DeviceProfiles;
            AdbPathInput.Text = AppSettings.Current.AdbPath;
            BackgroundPathInput.Text = "";
            _errors.Clear();
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
        private void SaveSettings(object sender, RoutedEventArgs e)
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
                        _errors.Add("Adb Path doesn't exist.");
                    }
                    else
                    {
                        isEdited = true;
                        AppSettings.Current.AdbPath = adbPath;
                    }
                }
            }
            
            if(DeviceProfilesViewer.IsEdited)
            {
                var deviceProfiles = DeviceProfilesViewer.DeviceProfiles;
                //Check Profile Name Distinct
                bool isProfileNameDistinct = false;
                for (int i = 0; i < deviceProfiles.Count; i++)
                {
                    for (int j = i + 1; j < deviceProfiles.Count; j++)
                    {
                        if (deviceProfiles[i].Name == deviceProfiles[j].Name)
                        {
                            isProfileNameDistinct = true;
                            _errors.Add($"Device Profiles '{deviceProfiles[i].Name}' are duplicated.");
                        }
                    }
                }

                if (!isProfileNameDistinct)
                {
                    AppSettings.Current.DeviceProfiles.Clear();
                    foreach (var profile in deviceProfiles)
                    {
                        AppSettings.Current.DeviceProfiles.Add(profile);
                    }
                    isEdited = true;
                }
            }

            //Save Settings File
            if (isEdited)
            {
                using (var fileStream = File.Open("settings.json", FileMode.Create))
                {
                    JsonSerializer.SerializeAsync<AppSettings>(fileStream, AppSettings.Current).ConfigureAwait(false);
                }
            }

        }
    }
}
