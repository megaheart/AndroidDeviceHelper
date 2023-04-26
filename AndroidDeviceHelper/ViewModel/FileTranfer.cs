using AdvancedSharpAdbClient;
using AndroidDeviceHelper.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Interop;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AndroidDeviceHelper.ViewModel
{
    public class FileTranfer
    {
        private ConnectionState connState;
        public FileTranfer(ConnectionState connState)
        {
            FileTranferProgresses = new ObservableCollection<FileTranferProgress>();
            this.connState = connState;
        }
        public ObservableCollection<FileTranferProgress> FileTranferProgresses { get; private set; }

        /// <summary>
        /// Start a tranfer process
        /// </summary>
        /// <param name="pullOrPush">true if pulling, false if pushing</param>
        /// <param name="fileModel"></param>
        /// <param name="filePath">Path (or uri) of file pulled or pushed</param>
        /// <param name="savePath">Path (or uri) where file will be copied to</param>
        /// <returns>tranfer progress</returns>
        public FileTranferProgress CreateWorker(bool pullOrPush, FileModel fileModel, string filePath, string savePath)
        {
            var adbPath = AppSettings.Current.AdbPath;
            FileTranferProgress progress = new FileTranferProgress() { 
                IsPulling = pullOrPush,
                File = fileModel,
                FilePath = filePath,
                SavePath = savePath
            };
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            ConcurrentStack<string> _errorStack = new ConcurrentStack<string>();

            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.RunWorkerCompleted += (o, e) =>
            {
                if (_errorStack.TryPeek(out var error))
                {
                    progress.Error = error;
                    progress.IsError = true;
                }
                else
                {
                    progress.IsCompleted = true;
                }
            };

            if(pullOrPush)
            {
                backgroundWorker.DoWork += GetPullProcessHandler(connState, adbPath, filePath, savePath, _errorStack);
            }
            else
            {
                backgroundWorker.DoWork += GetPushProcessHandler(connState, adbPath, filePath, savePath, _errorStack);
            }
            backgroundWorker.RunWorkerAsync();

            FileTranferProgresses.Add(progress);

            return progress;
        }

        public static DoWorkEventHandler GetPullProcessHandler(ConnectionState connState, string adbPath, 
            string filePath, string savePath, ConcurrentStack<string> _errorStack) { 
        
            return (object? sender, DoWorkEventArgs e) => {
                DeviceData device = connState.DeviceData;
                AdbServer server = new AdbServer();
                var server_result = server.StartServer($"{adbPath}\\adb.exe", false);

                var client = new AdbClient();

                Stream stream = File.Open(savePath, FileMode.Create);
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
                            var result = DeviceManager.Instance.NotifyDeviceLostConnection(connState).Result;

                            if (result.IsSuccess)
                            {
                                connState = result.ConnectionState;
                                device = connState.DeviceData;

                                service = new SyncService(client, device);
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            var msg = "Cannot transfer file because Unknown Errors occurs. Please contact developers to get more information!"
                                      + $" Error message: (Adb Errors)\"{ex.Message}\"";
                            _errorStack.Push(msg);
                            return;
                        }
                    }
                    else
                    {
                        var msg = "Cannot transfer file because Unknown Errors occurs. Please contact developers to get more information!"
                                  + $" Error message: \"{ex.Message}\"";
                        _errorStack.Push(msg);
                        return;
                    }
                }

                try
                {
                    service.Pull(filePath, stream, null, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _errorStack.Push("Connection with device is interrupted. Please try again.");
                    return;
                }

                service.Dispose();
                stream.Dispose();
            };
        }


        public static DoWorkEventHandler GetPushProcessHandler(ConnectionState connState, string adbPath,
            string filePath, string savePath, ConcurrentStack<string> _errorStack)
        {

            return (object? sender, DoWorkEventArgs e) => {
                DeviceData device = connState.DeviceData;
                AdbServer server = new AdbServer();
                var server_result = server.StartServer($"{adbPath}\\adb.exe", false);

                var client = new AdbClient();

                Stream stream = File.OpenRead(filePath);
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
                            var result = DeviceManager.Instance.NotifyDeviceLostConnection(connState).Result;

                            if (result.IsSuccess)
                            {
                                connState = result.ConnectionState;
                                device = connState.DeviceData;

                                service = new SyncService(client, device);
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            var msg = "Cannot transfer file because Unknown Errors occurs. Please contact developers to get more information!"
                                      + $" Error message: (Adb Errors)\"{ex.Message}\"";
                            _errorStack.Push(msg);
                            return;
                        }
                    }
                    else
                    {
                        var msg = "Cannot transfer file because Unknown Errors occurs. Please contact developers to get more information!"
                                  + $" Error message: \"{ex.Message}\"";
                        _errorStack.Push(msg);
                        return;
                    }
                }
                stream.Position = 0;
                stream.Seek(0, SeekOrigin.Begin);
                try
                {
                    service.Push(stream, savePath, 777, DateTimeOffset.Now, null, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _errorStack.Push("Connection with device is interrupted. Please try again.");
                    return;
                }

                service.Dispose();
                stream.Dispose();
            };
        }

    }
}
