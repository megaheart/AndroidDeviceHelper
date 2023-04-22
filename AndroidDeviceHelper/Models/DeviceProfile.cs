using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidDeviceHelper.Models
{
    public class DeviceProfile : NotifiableObject
    {
        private string _name = null;
        public string Name
        {
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
        public string DeviceConnectionHash 
        {
            get
            {
                if (_type == "wifi")
                {
                    return $"wifi{Address}";
                }
                return "";
            }
        }
        public DeviceProfile Clone() { 
            DeviceProfile profile = new DeviceProfile();
            profile._name = _name;
            profile._type = _type;
            if(_type == "wifi")
            {
                profile.Address = Address;
            }
            return profile;
        }
    }
}
