using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextVessel
{
    public class UserConfig : INotifyPropertyChanged
    {
        [JsonIgnore]
        private string _interrootPath = null;

        [JsonIgnore]
        private int _lastMsgIndex = -1;

        [JsonIgnore]
        private string _language = "ENGLISH";

        public string InterrootPath
        {
            get => _interrootPath;
            set
            {
                _interrootPath = value;
                NotifyPropertyChanged(nameof(InterrootPath));
            }
        }

        public int LastFmgIndex
        {
            get => _lastMsgIndex;
            set
            {
                _lastMsgIndex = value;
                NotifyPropertyChanged(nameof(LastFmgIndex));
            }
        }

        public string Language
        {
            get => _language;
            set
            {
                _language = value;
                NotifyPropertyChanged(nameof(Language));
            }
        }

        public string GetMsgFolderForSelectedLanguage()
        {
            return IOHelper.Frankenpath(InterrootPath, "msg", Language);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
