using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using AndroidDeviceHelper.Models;
using AndroidDeviceHelper.View.MainWindowPage.TaskPage.TransferingFiles;
using AndroidDeviceHelper.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace AndroidDeviceHelper.View.MainWindowPage.TaskPage
{
    /// <summary>
    /// Interaction logic for TransferingFilesPanel.xaml
    /// </summary>
    public partial class TransferingFilesPanel : UserControl, IConnectionStateDependency
    {
        private ObservableCollection<string> _errors = new ObservableCollection<string>();
        //private ObservableCollection<FileModel> _files = new ObservableCollection<FileModel>();
        public TransferingFilesPanel()
        {
            InitializeComponent();
            ErrorMsgViewer.ItemsSource = _errors;
            //_FileViewer.Files = _files;

            AddressBar.KeyUp += AddressBar_KeyUp;

        }

        private void AddressBar_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                Keyboard.ClearFocus();
                LoadFileList(AddressBar.Text);
            }
        }

        //public static readonly DependencyProperty ConnectionStateProperty = DependencyProperty.Register("ConnectionState", typeof(ConnectionState), typeof(TransferingFilesPanel), new UIPropertyMetadata(null));
        //public ConnectionState ConnectionState
        //{
        //    set => SetValue(ConnectionStateProperty, value);
        //    get => (ConnectionState)GetValue(ConnectionStateProperty);
        //}
        public ConnectionState ConnectionState { get; set; }

        private string _currentPath = "/";

        public void ResetPage()
        {
            _currentPath = "/";
            LoadFileList("/");
            AddressBar.Text = _currentPath;
        }
        private void ReloadFileList(object sender, RoutedEventArgs e)
        {

        }
        private void LoadFileList(string addr)
        {
            _FileViewer.IsEnabled = false;
            LoadFileListAsync(addr).ConfigureAwait(false);
        }
        private async Task LoadFileListAsync(string addr)
        {
            var adbPath = AppSettings.Current.AdbPath;
            var connState = ConnectionState;

            AdbServer server = new AdbServer();
            var server_result = server.StartServer($"{adbPath}\\adb.exe", false);
            var client = new AdbClient();
            var device = connState.DeviceData;
            var outputReceiver = new ConsoleOutputReceiver(); // create an output receiver object
            try
            {
                client.ExecuteRemoteCommand($"ls -mhsFcll {addr}", device, outputReceiver); // execute the command
            }
            catch (Exception ex)
            {
                if(ex is AdvancedSharpAdbClient.Exceptions.AdbException exception)
                {
                    if(exception.AdbError == "device offline")
                    {
                        var result = await DeviceManager.Instance.NotifyDeviceLostConnection(connState);

                        if (result.IsSuccess)
                        {
                            ConnectionState = result.ConnectionState;
                            connState = result.ConnectionState;
                            device = connState.DeviceData;

                            outputReceiver = new ConsoleOutputReceiver();
                            client.ExecuteRemoteCommand($"ls -mhsFcll {addr}", device, outputReceiver); // execute the command
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        var msg = "Navigation have been stoped because Unknown Errors occurs. Please contact developers to get more information!"
                                  + $" Error message: \"{ex.Message}\"";
                        MessageBox.Show("Error(s)", msg, MessageBoxButton.Ok, MessageBoxIcon.Error);
                        return;
                    }
                }
            }

            var fileList = DeserializeFileModel(outputReceiver.ToString(), out _);


            Dispatcher.Invoke(new Action(() =>
            {
                _FileViewer.Files = fileList;
                _FileViewer.IsEnabled = true;
                
            }));

        }
        private List<FileModel> DeserializeFileModel(string s, out string totalSize)
        {
            List<FileModel> result = new List<FileModel>();
            string[] lines = s.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            while (i < lines.Length && lines[i].IndexOf("total") == -1)
            {
                i++;
            }
            totalSize = lines[i].Substring(6);
            Console.WriteLine(totalSize);
            i++;
            while (i < lines.Length)
            {
                FileModel model = new FileModel();
                StringBuilder sb = new StringBuilder();
                int j = 0;
                var line = lines[i];
                while (j < line.Length && line[j] == ' ') j++;
                while (j < line.Length && line[j] != ' ')
                {
                    sb.Append(line[j]);
                    j++;
                }
                model.FileSize = sb.ToString();
                sb.Clear();
                while (j < line.Length && line[j] == ' ') j++;
                while (j < line.Length && line[j] != ' ') j++;
                while (j < line.Length && line[j] == ' ') j++;
                while (j < line.Length && line[j] != ' ') j++;
                while (j < line.Length && line[j] == ' ') j++;
                while (j < line.Length && line[j] != ' ') j++;
                while (j < line.Length && line[j] == ' ') j++;
                while (j < line.Length && line[j] != ' ') j++;
                while (j < line.Length && line[j] == ' ') j++;
                while (j < line.Length && line[j] != ' ') j++;
                while (j < line.Length && line[j] == ' ') j++;
                DateTime time;
                if (line[j] == '?')
                {
                    time = DateTime.MinValue;
                    j++;
                }
                else
                {
                    time = DateTime.Parse(line.Substring(j, 35)).ToLocalTime();
                    j += 35;
                }

                model.Time = time;

                while (j < line.Length && line[j] == ' ') j++;
                model.FileType = line.Last().ToString();
                if (model.FileType != "@")
                {
                    while (j < line.Length - 1)
                    {
                        if (line[j] == '\\')
                        {
                            sb.Append(line[++j]);
                        }
                        else sb.Append(line[j]);
                        j++;
                    }
                }
                else
                {
                    while (j < line.Length - 1 && line[j] != ' ')
                    {
                        if (line[j] == '\\')
                        {
                            sb.Append(line[++j]);
                        }
                        else sb.Append(line[j]);
                        j++;
                    }
                }
                model.FileName = sb.ToString();
                sb.Clear();
                model.FileExtension = model.FileName.Substring(model.FileName.LastIndexOf(".") + 1);
                result.Add(model);
                i++;
            }
            return result;
        }
        private void FileViewer_FileOpened(object sender, TransferingFiles.FileOpenedEventArgs e)
        {
            MessageBox.Show(e.FileModel.FileName);
            //if (!PreviewWindow.Preview(e.FileModel, null))
            //{

            //}
        }

        private void OpenFilesTranferPanel(object sender, RoutedEventArgs e)
        {
            FilesTranferPanel.Visibility = Visibility.Visible;
        }
    }
}
