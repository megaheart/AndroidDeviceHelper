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
    public class DeviceProfile: NotifiableObject
    {
        private string _name = null;
        public string Name { 
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            } 
        }
        private string _type;
        public string Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        public string Address { get; set; } = null;
    }
}
