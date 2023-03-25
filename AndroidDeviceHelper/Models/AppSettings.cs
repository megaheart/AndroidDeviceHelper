using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidDeviceHelper.Models
{
    public class AppSettings
    {
        private static AppSettings? _current = null;
        public static AppSettings? Current { 
            set => _current = value;
            get => _current;
        }
        public string AdbPath { get; set; } = null;
        public DeviceProfile[] DeviceProfiles { get; set; } = new DeviceProfile[0]; 
    }
    public class DeviceProfile
    {
        public string Name { get; set; } = null;
        public string Address { get; set; } = null;
    }
}
