using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TextVessel
{
    public class UserConfig : INotifyPropertyChanged
    {
        [JsonIgnore]
        private string _filter = "";

        public string Filter
        {
            get => _filter;
            set
            {
                _filter = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(FilterDispStr));
            }
        }

        [JsonIgnore]
        public string FilterDispStr => !string.IsNullOrWhiteSpace(Filter) ? $"\"{Filter}\"" : "(None)";

        [JsonIgnore]
        public bool InterrootPathSelected
        {
            get => InterrootPath != null;
        }

        [JsonIgnore]
        private string _interrootPath = null;
        public string InterrootPath
        {
            get => _interrootPath;
            set
            {
                _interrootPath = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(InterrootPathSelected));
            }
        }

        [JsonIgnore]
        private int _lastFmgIndex = -1;
        public int LastFmgIndex
        {
            get => _lastFmgIndex;
            set
            {
                _lastFmgIndex = value;
                RaisePropertyChanged();
            }
        }

        [JsonIgnore]
        private Dictionary<string, double> _lastFmgScrollOffsets = new Dictionary<string, double>();
        public Dictionary<string, double> LastFmgScrollOffsets
        {
            get => _lastFmgScrollOffsets;
            set
            {
                _lastFmgScrollOffsets = value;
                RaisePropertyChanged();
            }
        }

        [JsonIgnore]
        private string _dataLanguageName = "ENGLISH";
        public string DataLanguageName
        {
            get => _dataLanguageName;
            set
            {
                if (ValidLanguageValues.Contains(value))
                    _dataLanguageName = value;
                else
                    _dataLanguageName = "ENGLISH";

                RaisePropertyChanged();
            }
        }

        [JsonIgnore]
        private string _fmgListLanguageName = "ENGLISH";
        public string FmgListLanguageName
        {
            get => _fmgListLanguageName;
            set
            {
                if (ValidLanguageValues.Contains(value))
                    _fmgListLanguageName = value;
                else
                    _fmgListLanguageName = "ENGLISH";

                RaisePropertyChanged();
            }
        }

        private static readonly string[] ValidLanguageValues = new string[]
        {
            "ENGLISH",
            "FRENCH",
            "GERMAN",
            "ITALIAN",
            "JAPANESE",
            "KOREAN",
            "POLISH",
            "RUSSIAN",
            "SPANISH",
            "TCHINESE"
        };

        public string GetMsgFolderForSelectedLanguage()
        {
            return IOHelper.Frankenpath(InterrootPath, "msg", DataLanguageName);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
