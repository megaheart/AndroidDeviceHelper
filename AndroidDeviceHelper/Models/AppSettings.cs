using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidDeviceHelper.Models
{
    public class AppSettings
    {
        private static AppSettings _current = new AppSettings();
        public static AppSettings Current { 
            set => _current = value;
            get => _current;
        }
        public string? BackgroundPath { get; set; } = null;
        public string? AdbPath { get; set; } = null;
        public ObservableCollection<DeviceProfile> DeviceProfiles { get; set; } = new ObservableCollection<DeviceProfile>(); 
    }
    
}
