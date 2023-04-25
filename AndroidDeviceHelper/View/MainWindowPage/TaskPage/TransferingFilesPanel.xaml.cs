using AdvancedSharpAdbClient;
using AndroidDeviceHelper.Models;
using AndroidDeviceHelper.ViewModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Shapes;
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

        //public static readonly DependencyProperty ConnectionStateProperty = DependencyProperty.Register("ConnectionState", typeof(ConnectionState), typeof(TransferingFilesPanel), new UIPropertyMetadata(null));
        //public ConnectionState ConnectionState
        //{
        //    set => SetValue(ConnectionStateProperty, value);
        //    get => (ConnectionState)GetValue(ConnectionStateProperty);
        //}
        public ConnectionState ConnectionState { get; set; }
        private async Task<bool> NavigateTo(string path)
        {
            Dispatcher.Invoke(() =>
            {
                _FileViewer.IsEnabled = false;
            });
            var r = await LoadFileListAsync(path).ConfigureAwait(false);

            if (r)
            {
                var item = bookmarks.FirstOrDefault(p => p.Path == _currentPath);
                Dispatcher.Invoke(() =>
                {
                    BookmarkViewer.SelectedItem = item;
                    PinBtn.IsChecked = item != null;
                });
            }

            return r;
        }

        private string _currentPath = "/";

        private static int FileTypeIndex(string fileType) => fileType switch
        {
            "/" => 1,
            "@" => 1,
            "*" => 3,
            "|" => 4,
            _ => 100000
        };

        private readonly Comparison<FileModel> FileSortFunc = (a, b) =>
        {
            var a_fileTypeIndex = FileTypeIndex(a.FileType);
            var b_fileTypeIndex = FileTypeIndex(b.FileType);

            var d = a_fileTypeIndex - b_fileTypeIndex;

            if (d != 0) return d;

            if (a.FileExtension != b.FileExtension) return a.FileExtension.CompareTo(b.FileExtension);

            return a.FileName.CompareTo(b.FileName);
        };
        private async Task<bool> LoadFileListAsync(string path)
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
                await client.ExecuteRemoteCommandAsync($"ls -mhsFcll {path}", device, outputReceiver); // execute the command
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
                            ConnectionState = result.ConnectionState;
                            connState = result.ConnectionState;
                            device = connState.DeviceData;

                            outputReceiver = new ConsoleOutputReceiver();
                            client.ExecuteRemoteCommand($"ls -mhsFcll {path}", device, outputReceiver); // execute the command
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        var msg = "Navigation have been stoped because Unknown Errors occurs. Please contact developers to get more information!"
                                  + $" Error message: (Adb Errors)\"{ex.Message}\"";
                        MessageBox.Show("Error(s)", msg, MessageBoxButton.Ok, MessageBoxIcon.Error);
                        return false;
                    }
                }
                else
                {
                    var msg = "Navigation have been stoped because Unknown Errors occurs. Please contact developers to get more information!"
                              + $" Error message: \"{ex.Message}\"";
                    MessageBox.Show("Error(s)", msg, MessageBoxButton.Ok, MessageBoxIcon.Error);
                    return false;
                }

            }
            var output = outputReceiver.ToString();

            if (!output.Contains("total"))
            {
                if (output.Contains("No such file or directory"))
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        MessageBox.Show("Warning", $"No such directory \"{path}\" in \"{connState.DeviceProfile.Name}\" device. Navigation have been stoped.", MessageBoxButton.Ok, MessageBoxIcon.Warning);
                        AddressBar.Text = _currentPath;
                        _FileViewer.IsEnabled = true;
                    }));

                    return false;
                }
                else if (output.Contains("Permission denied"))
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        MessageBox.Show("Permission denied", $"\"{connState.DeviceProfile.Name}\" device doesn't allow you to open this directory. Navigation have been stoped.", MessageBoxButton.Ok, MessageBoxIcon.Error);
                        AddressBar.Text = _currentPath;
                        _FileViewer.IsEnabled = true;
                    }));

                    return false;
                }
                else
                {
                    var msg = "Navigation have been stoped because Unknown Errors occurs. Please contact developers to get more information!";
                    MessageBox.Show("Error(s)", msg, MessageBoxButton.Ok, MessageBoxIcon.Error);
                    return false;
                }
            }
            else
            {
                var fileList = DeserializeFileModel(output, out _);

                fileList.Sort(FileSortFunc);

                _currentPath = path;
                Dispatcher.Invoke(new Action(() =>
                {
                    _FileViewer.Files = fileList;
                    AddressBar.Text = _currentPath;
                    _FileViewer.IsEnabled = true;
                }));
                return true;
            }


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

                    if (model.FileType != "/" && model.FileType != "*" && model.FileType != "|")
                    {
                        sb.Append(model.FileType);
                        model.FileType = "";
                    }

                    model.FileName = sb.ToString();
                    sb.Clear();
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
                    model.FileName = sb.ToString();
                    sb.Clear();

                    j += 4;
                    while (j < line.Length - 1)
                    {
                        if (line[j] == '\\')
                        {
                            sb.Append(line[++j]);
                        }
                        else sb.Append(line[j]);
                        j++;
                    }
                    model.ShortcutLink = sb.ToString();
                    sb.Clear();
                }

                model.FileExtension = model.FileName.Substring(model.FileName.LastIndexOf(".") + 1);
                result.Add(model);
                i++;
            }
            return result;
        }

        private readonly ConcurrentStack<string> previousVisitedDir = new ConcurrentStack<string>();
        private readonly ConcurrentStack<string> forwardVisitedDir = new ConcurrentStack<string>();

        public async void ResetPage()
        {
            PreviousDirectoryBtn.IsEnabled = false;
            ForwardDirectoryBtn.IsEnabled = false;
            await LoadBookmark().ConfigureAwait(false);
            await NavigateTo("/").ConfigureAwait(false);
            previousVisitedDir.Clear();
            forwardVisitedDir.Clear();
        }
        private async void AddressBar_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                Keyboard.ClearFocus();
                PreviousDirectoryBtn.IsEnabled = false;
                ForwardDirectoryBtn.IsEnabled = false;

                var _curPath = _currentPath;

                var path = LinuxPath.NormalizeDirectoryName(AddressBar.Text.Trim());

                var result = await NavigateTo(path);
                if (result)
                {
                    previousVisitedDir.Push(_curPath);
                    forwardVisitedDir.Clear();
                }

                if (previousVisitedDir.Count != 0)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        PreviousDirectoryBtn.IsEnabled = true;
                    }));
                }

                if (forwardVisitedDir.Count != 0)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        ForwardDirectoryBtn.IsEnabled = true;
                    }));
                }
            }
        }
        private void RefreshDirectory(object sender, RoutedEventArgs e)
        {
            NavigateTo(_currentPath);
        }
        private async void NavigateToParentDirectory(object sender, RoutedEventArgs e)
        {
            var parentDir = LinuxPath.GetDirectoryName(_currentPath);
            if (parentDir != null)
            {
                PreviousDirectoryBtn.IsEnabled = false;
                ForwardDirectoryBtn.IsEnabled = false;

                var _curPath = _currentPath;
                var result = await NavigateTo(parentDir);
                if (result)
                {
                    previousVisitedDir.Push(_curPath);
                    forwardVisitedDir.Clear();
                }

                if (previousVisitedDir.Count != 0)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        PreviousDirectoryBtn.IsEnabled = true;
                    }));
                }

                if (forwardVisitedDir.Count != 0)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        ForwardDirectoryBtn.IsEnabled = true;
                    }));
                }
            }

        }
        private async void NavigateToPreviousDirectory(object sender, RoutedEventArgs e)
        {
            if (previousVisitedDir.TryPeek(out var path))
            {
                PreviousDirectoryBtn.IsEnabled = false;
                ForwardDirectoryBtn.IsEnabled = false;

                var _curPath = _currentPath;
                var result = await NavigateTo(path);
                if (result)
                {
                    previousVisitedDir.TryPop(out _);
                    forwardVisitedDir.Push(_curPath);
                }

                if (previousVisitedDir.Count != 0)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        PreviousDirectoryBtn.IsEnabled = true;
                    }));
                }

                if (forwardVisitedDir.Count != 0)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        ForwardDirectoryBtn.IsEnabled = true;
                    }));
                }
            }
        }
        private async void NavigateToForwardDirectory(object sender, RoutedEventArgs e)
        {
            if (forwardVisitedDir.TryPeek(out var path))
            {
                PreviousDirectoryBtn.IsEnabled = false;
                ForwardDirectoryBtn.IsEnabled = false;

                var _curPath = _currentPath;
                var result = await NavigateTo(path);
                if (result)
                {
                    forwardVisitedDir.TryPop(out _);
                    previousVisitedDir.Push(_curPath);
                }

                if (previousVisitedDir.Count != 0)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        PreviousDirectoryBtn.IsEnabled = true;
                    }));
                }

                if (forwardVisitedDir.Count != 0)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        ForwardDirectoryBtn.IsEnabled = true;
                    }));
                }
            }
        }
        private async void FileViewer_FileOpened(object sender, TransferingFiles.FileOpenedEventArgs e)
        {
            if (e.FileModel.FileType == "/")
            {
                PreviousDirectoryBtn.IsEnabled = false;
                ForwardDirectoryBtn.IsEnabled = false;

                var _curPath = _currentPath;
                var result = await NavigateTo(LinuxPath.Combine(_currentPath, e.FileModel.FileName));
                if (result)
                {
                    previousVisitedDir.Push(_curPath);
                    forwardVisitedDir.Clear();
                }

                if (previousVisitedDir.Count != 0)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        PreviousDirectoryBtn.IsEnabled = true;
                    }));
                }

                if (forwardVisitedDir.Count != 0)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        ForwardDirectoryBtn.IsEnabled = true;
                    }));
                }
            }
            else if (e.FileModel.FileType == "@")
            {
                PreviousDirectoryBtn.IsEnabled = false;
                ForwardDirectoryBtn.IsEnabled = false;

                var _curPath = _currentPath;
                var result = await NavigateTo(e.FileModel.ShortcutLink);
                if (result)
                {
                    previousVisitedDir.Push(_curPath);
                    forwardVisitedDir.Clear();
                }

                if (previousVisitedDir.Count != 0)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        PreviousDirectoryBtn.IsEnabled = true;
                    }));
                }

                if (forwardVisitedDir.Count != 0)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        ForwardDirectoryBtn.IsEnabled = true;
                    }));
                }
            }
            else if (e.FileModel.FileType == "*")
            {
                //if (!PreviewWindow.Preview(e.FileModel, null))
                //{

                //}
            }

        }

        private void OpenFilesTranferPanel(object sender, RoutedEventArgs e)
        {
            FilesTranferPanel.Visibility = Visibility.Visible;
        }

        private void Click_PushFile(object sender, RoutedEventArgs e)
        {
            
        }

        private void Click_PullFile(object sender, RoutedEventArgs e)
        {
            
        }

        private ObservableCollection<FileTransferBookmark> bookmarks;

        private async Task LoadBookmark()
        {
            var deviceSerial = ConnectionState.DeviceData?.Serial ?? "";
            if (File.Exists($"Pinned Directory\\{deviceSerial}.json"))
            {
                using (var fileStream = File.OpenRead($"Pinned Directory\\{deviceSerial}.json"))
                {
                    var x = await JsonSerializer.DeserializeAsync<IEnumerable<FileTransferBookmark>>(fileStream);
                    if (x == null) throw new Exception($"Error while read 'Pinned Directory\\{deviceSerial}.json' file");
                    bookmarks = new ObservableCollection<FileTransferBookmark>(x);
                }
            }
            else
            {
                bookmarks = new ObservableCollection<FileTransferBookmark>();
            }

            Dispatcher.Invoke(new Action(() =>
            {
                BookmarkViewer.ItemsSource = bookmarks;
            }));
        }
        private async Task SaveBookmark()
        {
            if (!Directory.Exists("Pinned Directory"))
            {
                Directory.CreateDirectory("Pinned Directory");
            }
            var deviceSerial = ConnectionState.DeviceData?.Serial ?? "";
            using (var fileStream = File.Open($"Pinned Directory\\{deviceSerial}.json", FileMode.Create))
            {
                await JsonSerializer.SerializeAsync(fileStream, bookmarks);
            }
        }
        private void Click_RemoveBookmark(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            FileTransferBookmark bookmark = btn.DataContext as FileTransferBookmark;
            bookmarks.Remove(bookmark);
            SaveBookmark().ConfigureAwait(false);
        }
        private void Click_TogglePin(object sender, RoutedEventArgs e)
        {
            ToggleButton btn = sender as ToggleButton;
            if(btn.IsChecked == true)
            {
                FileTransferBookmark bookmark = new FileTransferBookmark()
                {
                    Path = _currentPath,
                    Name = LinuxPath.GetFilename(_currentPath),
                };
                bookmarks.Add(bookmark);
                BookmarkViewer.SelectedItem = bookmark;
                SaveBookmark().ConfigureAwait(false);
            }
            else
            {
                if(bookmarks.Remove(bookmarks.FirstOrDefault(p => p.Path == _currentPath)))
                {
                    SaveBookmark().ConfigureAwait(false);
                }
            }
        }

        private async void BookmarkViewer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var bookmark = BookmarkViewer.SelectedItem as FileTransferBookmark;
            if (bookmark != null)
            {
                var result = await NavigateTo(bookmark.Path).ConfigureAwait(false);

                if (!result)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var msg = "The pinned directory doesn't exist in device. Do you want to unpin it?";
                        var r = MessageBox.Show("No such file or directory", msg, MessageBoxButton.OkCancel, MessageBoxIcon.Warning);
                        if (r)
                        {
                            bookmarks.Remove(bookmark);
                            SaveBookmark().ConfigureAwait(false);
                        }
                    });
                }
            }
        }
    }
}
