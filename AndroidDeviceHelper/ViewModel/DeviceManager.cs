using AdvancedSharpAdbClient;
using AndroidDeviceHelper.Models;
using AndroidDeviceHelper.View.MainWindowPage.TaskPage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AndroidDeviceHelper.ViewModel
{
    
    public class DeviceManager
    {
        public static readonly DeviceManager Instance = new DeviceManager();
        public DeviceManager() { }
        public IReadOnlyDictionary<string, ConnectionState> ConnectionStates { get => _connectionStates; }
        private Dictionary<string, ConnectionState> _connectionStates = new Dictionary<string, ConnectionState>();

        public ConnectionState? CurrentConnection { get; private set; }

        public bool TryGetConnectionState(DeviceProfile deviceProfile, out ConnectionState connectionState)
        {
            var deviceConnectionHash = deviceProfile.DeviceConnectionHash;
            return _connectionStates.TryGetValue(deviceConnectionHash, out connectionState);
        }

        //public int ConnectedDeviceCount { get; private set; }

        /// <summary>
        /// Connect with a device with connection method in device profile
        /// </summary>
        /// <param name="deviceProfile"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<ConnectionResult> Connect(DeviceProfile deviceProfile)
        {
            CurrentConnection = null;

            if (deviceProfile == null) { throw new ArgumentNullException(nameof(deviceProfile)); }
            //if (deviceData == null) { throw new ArgumentNullException(nameof(deviceData)); }

            var adbPath = AppSettings.Current.AdbPath;
            var deviceConnectionHash = deviceProfile.DeviceConnectionHash;
            string e = $"Not support connection type '{deviceProfile.Type}'";

            if (deviceProfile.Type == "wifi")
                e = await ConnectViaWifi(deviceProfile).ConfigureAwait(false);

            if (e == "SUCC")
            {
                if (!_connectionStates.TryGetValue(deviceConnectionHash, out ConnectionState state))
                {
                    state = new ConnectionState();
                    state.DeviceConnectionHash = deviceConnectionHash;
                    state.DeviceProfile = deviceProfile.Clone();
                    //state.DeviceProfile.Name = null;
                    _connectionStates.Add(deviceConnectionHash, state);
                }
                lock (state)
                {
                    state.DeviceData = null;
                }

                AdbServer server = new AdbServer();
                var result = server.StartServer($"{adbPath}\\adb.exe", false);
                var client = new AdbClient();
                //client.Connect("127.0.0.1:58526");
                var devices = client.GetDevices();

                int idx = -1;

                if (deviceProfile.Type == "wifi")
                {
                    idx = devices.FindIndex(d => d.Serial == deviceProfile.Address);
                }

                if (idx != -1)
                {
                    lock (state)
                    {
                        state.DeviceData = devices[idx];
                        state.IsConnected = true;
                    }
                    return new() { IsSuccess = true, ConnectionState = state };
                }
                return new() { IsSuccess = false, Error = "No device found. Unknown Error(s) occur. Please contact developers to get more information." };
            } 
            else
            {
                return new() { IsSuccess = false, Error = e ?? "Unknown Error(s) occur. Please contact developers to get more information." };
            }
        }


        /// <summary>
        /// Connect with device via wifi
        /// </summary>
        /// <param name="curDevice">Device connection info</param>
        /// <returns>SUCC if connect successful, otherwise return error message.</returns>
        private async Task<string> ConnectViaWifi(DeviceProfile curDevice)
        {
            var deviceAddr = curDevice.Address;
            var deviceType = curDevice.Type;
            var adbPath = AppSettings.Current.AdbPath;

            var e = await ProcessIO.ExecuteCommand($"{adbPath}\\adb.exe", $"connect {deviceAddr}", TimeSpan.FromSeconds(10));

            if (e.Error.Any(err => err.Contains("ProcessTimeOut")))
            {
                return "\"adb.exe\" (in Adb Path) is running too long. Please check whether \"adb.exe\" is fake Android Debug Bridge execution file.";
            }
            else if (e.Error.Any(err => err.Contains("The system cannot find the file specified.")))
            {
                return "Adb Path doesn't exist or doesn't contain \"adb.exe\".";
            }
            else if (e.Error.Any(err => err.Contains("The specified executable is not a valid application for this OS platform.")))
            {
                return "The specified executable (\"adb.exe\") is not a valid application for this OS platform.";
            }
            else if (e.Output.Any(err => err.Contains("(10061)")))
            {
                return "Device is off or its address is invalid. Please turn device on or change in setting page.";
            }
            else if (e.Error.Any(err => err.Contains("usage: adb connect HOST[:PORT]")))
            {
                return "Device address is invalid (format \"HOST[:PORT]\"). Please change in setting page.";
            }
            else if (e.Error.Length != 0)
            {
                return "Unknown Error(s) occur. Please contact developers to get more information.";
            }
            else
            {
                
                return "SUCC";

            }
        }

        public async Task<string> Disconnect(ConnectionState connectionState)
        {
            var deviceProfile = connectionState.DeviceProfile;

            var adbPath = AppSettings.Current.AdbPath;
            var deviceConnectionHash = deviceProfile.DeviceConnectionHash;
            string e = $"Not support connection type '{deviceProfile.Type}'";

            if (deviceProfile.Type == "wifi")
                e = await DisconnectViaWifi(deviceProfile).ConfigureAwait(false);

            if (e == "SUCC")
            {
                if (!_connectionStates.TryGetValue(deviceConnectionHash, out ConnectionState state))
                {
                    state = new ConnectionState();
                    _connectionStates.Add(deviceConnectionHash, state);
                }

                lock (state)
                {
                    state.DeviceData = null;
                    state.IsConnected = false;
                }

                return "";
            }
            else
            {
                return e ?? "Unknown Error(s) occur. Please contact developers to get more information.";
            }
        }

        /// <summary>
        /// Connect with device via wifi
        /// </summary>
        /// <param name="curDevice">Device connection info</param>
        /// <returns>SUCC if connect successful, otherwise return error message.</returns>
        private async Task<string> DisconnectViaWifi(DeviceProfile curDevice)
        {
            var deviceAddr = curDevice.Address;
            var deviceType = curDevice.Type;
            var adbPath = AppSettings.Current.AdbPath;

            var e = await ProcessIO.ExecuteCommand($"{adbPath}\\adb.exe", $"disconnect {deviceAddr}", TimeSpan.FromSeconds(10));

            if (e.Error.Any(err => err.Contains("ProcessTimeOut")))
            {
                return "\"adb.exe\" (in Adb Path) is running too long. Please check whether \"adb.exe\" is fake Android Debug Bridge execution file.";
            }
            else if (e.Error.Any(err => err.Contains("The system cannot find the file specified.")))
            {
                return "Adb Path doesn't exist or doesn't contain \"adb.exe\".";
            }
            else if (e.Error.Any(err => err.Contains("The specified executable is not a valid application for this OS platform.")))
            {
                return "The specified executable (\"adb.exe\") is not a valid application for this OS platform.";
            }
            else if (e.Output.Any(err => err.Contains("(10061)")))
            {
                return "Device is off or its address is invalid. Please turn device on or change in setting page.";
            }
            else if (e.Error.Length != 0)
            {
                return "Unknown Error(s) occur. Please contact developers to get more information.";
            }
            else
            {

                return "SUCC";

            }
        }

        public event Action<ConnectionState> DeviceLostConnection;

        public async Task<ConnectionResult> NotifyDeviceLostConnection(ConnectionState state)
        {
            var result = await Connect(state.DeviceProfile);
            if(!result.IsSuccess)
            {
                DeviceLostConnection?.Invoke(state);
            }
            return result;
        }


    }
}
