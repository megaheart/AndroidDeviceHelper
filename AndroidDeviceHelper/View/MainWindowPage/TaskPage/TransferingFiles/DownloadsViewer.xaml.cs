using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AndroidDeviceHelper.Models;

namespace AndroidDeviceHelper.View.MainWindowPage.TaskPage.TransferingFiles
{
    /// <summary>
    /// Interaction logic for DownloadsViewer.xaml
    /// </summary>
    public partial class DownloadsViewer : UserControl
    {
        ObservableCollection<FileTranferProgress> transfers;
        public DownloadsViewer()
        {
            InitializeComponent();
            transfers = new ObservableCollection<FileTranferProgress>();
            FileTranferProgressViewer.ItemsSource = transfers;
        }

        private void CloseViewer(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void OpenDownloadedFile(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;
            FileTranferProgress progress = toggleButton.DataContext as FileTranferProgress;
            if(progress.IsCompleted && progress.IsPulling)
            {

            }
        }
        private void OpenFileLocation(object sender, RoutedEventArgs e)
        {
            Button toggleButton = sender as Button;
            FileTranferProgress progress = toggleButton.DataContext as FileTranferProgress;
            if (progress.IsCompleted && progress.IsPulling)
            {

            }
        }
    }
}
