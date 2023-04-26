using AdvancedSharpAdbClient;
using AndroidDeviceHelper.Models;
using AndroidDeviceHelper.Native;
using AndroidDeviceHelper.View.Converters;
using AndroidDeviceHelper.View.PreviewWindowPage;
using AndroidDeviceHelper.ViewModel;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AndroidDeviceHelper
{
    /// <summary>
    /// Interaction logic for PreviewWindow.xaml
    /// </summary>
    public partial class PreviewWindow : Window
    {
        public PreviewWindow()
        {
            InitializeComponent();
            WindowResizer = new WindowResizer(this);
            Closed += PreviewWindow_Closed;
        }

        private void PreviewWindow_Closed(object? sender, EventArgs e)
        {
            Stream?.Dispose();
        }

        public static bool ImageSupport(string fileExtension)
        {
            return fileExtension == "png" || fileExtension == "jpg"
                || fileExtension == "jpeg" || fileExtension == "bmp";
        }
        private Stream Stream = null;
        private string fileName = string.Empty;
        private string fileExtension = string.Empty;
        public static async Task<Stream> LoadStreamFromDeviceAsync(string adbPath, ConnectionState connState, string filePath)
        {
            DeviceData device = connState.DeviceData;
            AdbServer server = new AdbServer();
            var server_result = server.StartServer($"{adbPath}\\adb.exe", false);

            var client = new AdbClient();

            Stream stream = new MemoryStream();
            SyncService service = null;
            try
            {
                service = new SyncService(client, device);
            }
            catch (Exception ex)
            {
                if (ex is AdvancedSharpAdbClient.Exceptions.AdbException exception)
                {
                    if (exception.AdbError == "device offline")
                    {
                        var result = await DeviceManager.Instance.NotifyDeviceLostConnection(connState);

                        if (result.IsSuccess)
                        {
                            connState = result.ConnectionState;
                            device = connState.DeviceData;

                            service = new SyncService(client, device);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        var msg = "Cannot preview file because Unknown Errors occurs. Please contact developers to get more information!"
                                  + $" Error message: (Adb Errors)\"{ex.Message}\"";
                        MessageBox.Show("Error(s)", msg, MessageBoxButton.Ok, MessageBoxIcon.Error);
                        return null;
                    }
                }
                else
                {
                    var msg = "Cannot preview file because Unknown Errors occurs. Please contact developers to get more information!"
                              + $" Error message: \"{ex.Message}\"";
                    MessageBox.Show("Error(s)", msg, MessageBoxButton.Ok, MessageBoxIcon.Error);
                    return null;
                }
            }

            try
            {
                service.Pull(filePath, stream, null, CancellationToken.None);
            }
            catch (Exception ex)
            {
                var msg = "Connection with device is interrupted. Please try again.";
                MessageBox.Show("Error(s)", msg, MessageBoxButton.Ok, MessageBoxIcon.Error);
                return null;
            }

            service.Dispose();

            return stream;
        }
        public async Task LoadFileStreamAsync<T>(ConnectionState state, T control, FileModel file, string filePath, IList<FileModel> DirFiles)
            where T : FrameworkElement, IPreviewingControl
        {
            var adbPath = AppSettings.Current.AdbPath;
            var stream = await LoadStreamFromDeviceAsync(adbPath, state, filePath);

            if(stream == null)
            {
                Close(); 
                return;
            }

            Stream = Stream.Synchronized(stream);
            //await Task.Delay(5000);
            Dispatcher.Invoke(() =>
            {
                control.SetStream(Stream);
                control.Visibility = Visibility.Visible;
                DownloadBtn.IsEnabled = true;
            });
        }
        public static bool Preview(ConnectionState state, FileModel file, string filePath, IList<FileModel> DirFiles)
        {
            if (ImageSupport(file.FileExtension))
            {
                PreviewWindow previewWindow = new PreviewWindow();
                previewWindow.fileName = file.FileName;
                previewWindow.fileExtension = file.FileExtension;

                previewWindow.DownloadBtn.IsEnabled = false;

                ImagePreviewer previewer = new ImagePreviewer();
                previewWindow.Main.Content = previewer;

                previewer.Visibility = Visibility.Collapsed;

                previewWindow.LoadFileStreamAsync(state, previewer, file, filePath, DirFiles).ConfigureAwait(false);
                previewWindow.LoadingBar.SetBinding(FrameworkElement.VisibilityProperty, new Binding()
                {
                    Source = previewer,
                    Path = new PropertyPath("Visibility"),
                    Converter = new InverseVisibility()
                });

                previewWindow.Show();
                return true;
            }

            return false;
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

        private async void Click_Download(object sender, RoutedEventArgs e)
        {
            VistaSaveFileDialog dialog = new VistaSaveFileDialog
            {
                Filter = $"{fileExtension}|*.{fileExtension}",
                AddExtension = true,
                OverwritePrompt = true,
                DefaultExt = $"{fileExtension}",
                FileName = $"{fileName}"
            };
            dialog.ShowDialog();
            if (!string.IsNullOrEmpty(dialog.FileName))
            {
                DownloadBtn.IsEnabled = false;
                Download_Content.Text = "Downloading";
                var path = dialog.FileName;
                await Task.Run(() => {
                    using (var fileStream = File.Open(dialog.FileName, FileMode.Create))
                    {
                        fileStream.Position = 0;
                        fileStream.Seek(0, SeekOrigin.Begin);
                        Stream.Position = 0;
                        Stream.Seek(0, SeekOrigin.Begin);
                        Stream.CopyTo(fileStream);
                    }

                    Dispatcher.Invoke(() =>
                    {
                        DownloadBtn.IsEnabled = true;
                        Download_Content.Text = "Download";
                    });

                }).ConfigureAwait(false);
            }
        }
    }
}
