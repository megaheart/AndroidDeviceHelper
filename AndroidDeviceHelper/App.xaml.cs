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
using System.Text;

namespace AndroidDeviceHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
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
            //MainWindow = new MainWindow();
            //try
            //{
            //    var bitmap = new BitmapImage(new Uri(Path.GetFullPath(AppSettings.Current.BackgroundPath)));
            //    bitmap.Freeze();
            //    var brush = new ImageBrush() { Stretch = Stretch.UniformToFill };
            //    brush.Freeze();
            //    MainWindow.Background = brush;
            //}
            //catch (Exception _) { }
            //MainWindow.Show();

            var msg = "Navigation have been stoped because Unknown Errors occurs. Please contact developers to get more information!";
            MessageBox.Show("Error(s)", msg, MessageBoxButton.Ok, MessageBoxIcon.Error);

        }
    }
}
