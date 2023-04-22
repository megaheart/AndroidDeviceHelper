using AdvancedSharpAdbClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidDeviceHelper.Models
{
    public class ConnectionState
    {
        public bool IsConnected { get; set; }
        public DeviceData? DeviceData { get; set; }
        public string DeviceConnectionHash { get; set; }
        public DeviceProfile DeviceProfile { get; set; }
    }
}
