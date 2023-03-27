using AndroidDeviceHelper.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace AndroidDeviceHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                using (var fileStream = File.OpenRead("settings.json"))
                {
                    var x = JsonSerializer.DeserializeAsync<AppSettings>(fileStream).Result;
                    if (x == null) throw new Exception("Error while read settings.json file");
                    AppSettings.Current = x;
                }
                
            }
            catch(Exception _) { }
            MainWindow = new MainWindow();
            try
            {
                var brush = new ImageBrush(new BitmapImage(new Uri(Path.GetFullPath(AppSettings.Current.BackgroundPath)))) { Stretch = Stretch.UniformToFill };
                MainWindow.Background = brush;
            }
            catch (Exception _) { }
            MainWindow.Show();
        }
    }
}
