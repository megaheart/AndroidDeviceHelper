using AndroidDeviceHelper.Models;
using AndroidDeviceHelper.Native;
using AndroidDeviceHelper.View;
using AndroidDeviceHelper.View.TasksPage.SettingPage;
using AndroidDeviceHelper.View.TasksPage.TaskPage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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

namespace AndroidDeviceHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WindowResizer = new WindowResizer(this);
            
        }
        private IWindowPage _taskPage = null;
        private void MoveToTasksPage(object sender, RoutedEventArgs e)
        {
            if (_taskPage == null) _taskPage = new TaskPage();
            _taskPage.ResetPage();
            Main.Content = _taskPage;
        }
        private IWindowPage _settingPage = null;
        private void MoveToSettingPage(object sender, RoutedEventArgs e)
        {
            if(_settingPage == null) _settingPage = new SettingPage();
            _settingPage.ResetPage();
            Main.Content = _settingPage;
        }
        private void MoveToBlankPage(object sender, RoutedEventArgs e)
        {
            Main.Content = null;
        }
        #region Window resizer
        WindowResizer WindowResizer;
        // for each rectangle, assign the following method to its PreviewMouseDown event.
        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
            e.Handled = true;
        }
        private void Resize(object sender, MouseButtonEventArgs e)
        {
            WindowResizer.resizeWindow((System.Windows.Controls.Primitives.Thumb)sender);
            e.Handled = true;
        }
        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }
        }
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}
