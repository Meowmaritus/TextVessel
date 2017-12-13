using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using MeowDSIO;
using MeowDSIO.DataFiles;
using MeowDSIO.DataTypes.PARAM;
using MeowDSIO.DataTypes.PARAMDEF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using TextVessel.Credits;

namespace TextVessel
{
    public class MainDataContext : INotifyPropertyChanged
    {
        private ICSharpCode.AvalonEdit.TextEditorOptions _avalonEditorOptions { get; set; } = null;

        public ICSharpCode.AvalonEdit.TextEditorOptions AvalonEditorOptions
        {
            get => _avalonEditorOptions;
            set
            {
                _avalonEditorOptions = value;
                RaisePropertyChanged();
            }
        }

        private FMGRef _selectedFMG = null;
        public FMGRef SelectedFMG
        {
            get => _selectedFMG;
            set
            {
                MainListViewVisibility = Visibility.Hidden;
                _selectedFMG = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _mainListViewVisibility = Visibility.Hidden;
        public Visibility MainListViewVisibility
        {
            get => _mainListViewVisibility;
            set
            {
                _mainListViewVisibility = value;
                RaisePropertyChanged();
            }
        }

        private bool _isModified = false;
        public bool IsModified
        {
            get => _isModified;
            set
            {
                if (_isModified != value)
                {
                    _isModified = value;
                    RaisePropertyChanged();
                }

            }
        }

        private ZoomManager _zoom = new ZoomManager();
        public ZoomManager Zoom
        {
            get => _zoom;
            set
            {
                _zoom = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<LanguageDef> DataLanguageList { get; set; } = new ObservableCollection<LanguageDef>
        {
            new LanguageDef("ENGLISH", "English", "English"),
            new LanguageDef("FRENCH", "French", "Français"),
            new LanguageDef("GERMAN", "German", "Deutsch"),
            new LanguageDef("ITALIAN", "Italian", "Italiano"),
            new LanguageDef("JAPANESE", "Japanese", "日本語"),
            new LanguageDef("KOREAN", "Korean", "조선말"),
            new LanguageDef("POLISH", "Polish", "Polski"),
            new LanguageDef("RUSSIAN", "Russian", "Русский"),
            new LanguageDef("SPANISH", "Spanish", "Español"),
            new LanguageDef("TCHINESE", "T. Chinese", "繁體字"),
        };

        public ObservableCollection<LanguageDef> FmgListLanguageList { get; set; } = new ObservableCollection<LanguageDef>
        {
            new LanguageDef("ENGLISH", "English", "English"),
            new LanguageDef("JAPANESE", "Japanese", "日本語"),
        };

        public LanguageDef FmgListLanguage
        {
            get => FmgListLanguageList.Where(x => x.IDString == Config.FmgListLanguageName).First();
            set
            {
                Config.FmgListLanguageName = value.IDString;
                RaisePropertyChanged();
            }
        }

        public LanguageDef CurrentlyLoadedLanguage { get; set; } = null;

        public LanguageDef DataLanguage
        {
            get => DataLanguageList.Where(x => x.IDString == Config.DataLanguageName).First();
            set
            {
                Config.DataLanguageName = value.IDString;
                RaisePropertyChanged();
            }
        }

        public static double MaxWaitBlurRadius = 8;

        private bool _isWait = false;
        public bool IsWait
        {
            get => _isWait;
            set
            {
                _isWait = value;
                RaisePropertyChanged();
            }
        }

        public bool IsAnimWait
        {
            get => _waitAnimProgress < 1;
        }

        private bool _waitAnimSecondHalf = false;
        public bool WaitAnimSecondHalf
        {
            get => _waitAnimSecondHalf;
            set
            {
                _waitAnimSecondHalf = value;
                RaisePropertyChanged();
            }
        }

        private string _waitText = "";
        public string WaitText
        {
            get => _waitText;
            set
            {
                _waitText = value;
                RaisePropertyChanged();
            }
        }

        private double _waitAnimProgress = 1;
        public double WaitAnimProgress
        {
            get => _waitAnimProgress;
            set
            {
                _waitAnimProgress = value;
                RaisePropertyChanged();

                WaitOverlayOpacity = (1.0 - Math.Abs((value * 2) - 1));
                WaitBlurRadius = WaitOverlayOpacity * MaxWaitBlurRadius;

                bool waitAnimSecondHalf_NewVal = Math.Abs(WaitAnimProgress - 0.5) <= 0.001;

                if (WaitAnimSecondHalf != waitAnimSecondHalf_NewVal)
                {
                    WaitAnimSecondHalf = waitAnimSecondHalf_NewVal;
                }

                RaisePropertyChanged(nameof(IsWait));
            }
        }

        private double _waitBlurRadius = 0;
        public double WaitBlurRadius
        {
            get => _waitBlurRadius;
            set
            {
                _waitBlurRadius = value;
                RaisePropertyChanged();
            }
        }

        private double _waitOverlayOpacity = 0;
        public double WaitOverlayOpacity
        {
            get => _waitOverlayOpacity;
            set
            {
                _waitOverlayOpacity = value;
                RaisePropertyChanged();
            }
        }

        private IHighlightingDefinition _highlightdef = null;
        public IHighlightingDefinition HighlightDef
        {
            get { return _highlightdef; }
            set
            {
                if (_highlightdef != value)
                {
                    _highlightdef = value;
                    RaisePropertyChanged();
                }
            }
        }

        private UserConfig _config = new UserConfig();
        public UserConfig Config
        {
            get => _config;
            set
            {
                _config = value;
                RaisePropertyChanged();
            }
        }

        public void LoadConfig()
        {
            if (File.Exists(UserConfigPath))
            {
                string cfgJson = File.ReadAllText(UserConfigPath);
                Config = Newtonsoft.Json.JsonConvert.DeserializeObject<UserConfig>(cfgJson);
            }
            else
            {
                Config = new UserConfig();
                SaveConfig();
            }
        }

        public void SaveConfig()
        {
            if (File.Exists(UserConfigPath))
            {
                File.Delete(UserConfigPath);
            }
            string cfgJson = Newtonsoft.Json.JsonConvert.SerializeObject(Config, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(UserConfigPath, cfgJson);
        }

        public string UserConfigPath => IOHelper.Frankenpath(Environment.CurrentDirectory, CONFIG_FILE);

        public const string CONFIG_FILE = "TextVessel_UserConfig.json";

        public const string EXT_FMG = ".fmg";

        public List<BND3> MSGBNDs = new List<BND3>();

        private ObservableCollection<FMGRef> _fmgs = new ObservableCollection<FMGRef>();

        public ObservableCollection<FMGRef> FMGs
        {
            get => _fmgs;
            set
            {
                _fmgs = value;
                RaisePropertyChanged();
            }
        }

        public async Task LoadFmgsInOtherThread(Func<string, Task> setWait)
        {
            await Task.Run(async () =>
            {
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    await setWait?.Invoke("Loading all files...");
                });

                LoadAllFMGs();

                //Application.Current.Dispatcher.Invoke(() =>
                //{
                //    Mouse.OverrideCursor = null;
                //    setIsLoading?.Invoke(false);
                //});
            });
        }

        private string LoadEmbeddedJson(string path)
        {
            string name = $"{nameof(TextVessel)}.{path}";
            var a = typeof(MainDataContext).Assembly;

            var resNames = a.GetManifestResourceNames();

            if (!resNames.Contains(name))
            {
                return "{ }";
            }

            using (var strm = a.GetManifestResourceStream(name))
            {
                using (var strmReader = new StreamReader(strm))
                {
                    return strmReader.ReadToEnd();
                }
            }
        }

        private void Translate(ObservableCollection<FMGRef> Fmgs)
        {
            var fmgNames = Newtonsoft.Json.JsonConvert
                .DeserializeObject<Dictionary<string, string>>(
                LoadEmbeddedJson($"EmbeddedResources.FmgListLanguage.{Config.FmgListLanguageName}.json"));

            foreach (var f in Fmgs)
            {
                if (fmgNames.ContainsKey(f.Key))
                {
                    f.TranslatedName = fmgNames[f.Key];
                }
                else
                {
                    f.TranslatedName = f.Key;
                }
            }
        }

        private void LoadAllFMGs()
        {
            if (Config?.InterrootPath == null)
                return;

            var prog = new Progress<(int, int)>((p) => 
            {

            });

            MSGBNDs.Clear();
            var UPCOMING_FMGs = new ObservableCollection<FMGRef>();

            var msgBndFolder = Config.GetMsgFolderForSelectedLanguage();

            MSGBNDs = Directory.GetFiles(msgBndFolder, "*.msgbnd")
                .Select(p => DataFile.LoadFromFile<BND3>(p, new Progress<(int, int)>((pr) =>
                {

                }))).ToList();

            for (int i = 0; i < MSGBNDs.Count; i++)
            {
                foreach (var fmg in MSGBNDs[i])
                {
                    var newFmg = fmg.ReadDataAs<FMG>(new Progress<(int, int)>((p) =>
                    {

                    }));

                    var newFmgName = IOHelper.RemoveExtension(new FileInfo(fmg.Name).Name, EXT_FMG);

                    string newParamBndName = new FileInfo(MSGBNDs[i].FilePath).Name;

                    newParamBndName = newParamBndName.Substring(0, newParamBndName.LastIndexOf('.'));

                    UPCOMING_FMGs.Add(new FMGRef(newFmgName, newFmg, newParamBndName));
                }
            }

            Translate(UPCOMING_FMGs);

            UPCOMING_FMGs = new ObservableCollection<FMGRef>(UPCOMING_FMGs.OrderBy(x => x.TranslatedName));

            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var oldFmg in FMGs)
                {
                    oldFmg.Value.IsModifiedChanged -= Fmg_IsModifiedChanged;
                }

                FMGs = UPCOMING_FMGs;

                CurrentlyLoadedLanguage = DataLanguage;

                foreach (var newFmg in FMGs)
                {
                    newFmg.Value.IsModifiedChanged += Fmg_IsModifiedChanged;
                }

                IsModified = false;

                SelectedFMG = FMGs[0];
            });
        }

        private void Fmg_IsModifiedChanged(object sender, EventArgs e)
        {
            if (((DataFile)sender).IsModified)
            {
                IsModified = true;
            }
        }

        public void UpdateLanguage()
        {
            Translate(FMGs);

            FMGs = new ObservableCollection<FMGRef>(FMGs.OrderBy(x => x.TranslatedName));
        }


        public async Task SaveInOtherThread(Func<string, Task> setWait)
        {
            await Task.Run(async () =>
            {
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    await setWait?.Invoke("Saving all files...");
                });

                var backupsCreated = new List<string>();
                SaveAllFMGs(backupsCreated);

                if (backupsCreated.Count > 0)
                {
                    var sb = new StringBuilder();

                    sb.AppendLine("The following MSGBND file backup(s) did not exist and had to be created before saving:");

                    foreach (var b in backupsCreated)
                    {
                        sb.AppendLine($"\t'{b.Replace(Config.InterrootPath, ".")}'");
                    }

                    sb.AppendLine();

                    sb.AppendLine("Note: previously-created backups are NEVER overridden by this application. " +
                        "Subsequent file save operations will not display a messagebox if a backup of every file already exists.");

                    MessageBox.Show(sb.ToString(), "Backups Created", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                SaveConfig();

                IsModified = false;

                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    await setWait?.Invoke(null);
                });
            });
        }

        public void LoadSyntaxHighlighting()
        {
            using (var xshdStream = typeof(MainDataContext).Assembly.GetManifestResourceStream($"{nameof(TextVessel)}.EmbeddedResources.DarkXML.xshd"))
            {
                using (var xml = XmlReader.Create(xshdStream))
                {
                    var xshd = HighlightingLoader.LoadXshd(xml);
                    HighlightDef = HighlightingLoader.Load(xshd, HighlightingManager.Instance);
                }
            }
        }

        //public void DEBUG_RestoreBackupsLoadResave()
        //{
        //    LoadConfig();

        //    var gameparamBnds = Directory.GetFiles(Config.GameParamFolder, "*.parambnd")
        //        .Select(p => DataFile.LoadFromFile<BND3>(p, new Progress<(int, int)>((pr) =>
        //        {

        //        })));

        //    var drawparamBnds = Directory.GetFiles(Config.DrawParamFolder, "*.parambnd")
        //        .Select(p => DataFile.LoadFromFile<BND3>(p, new Progress<(int, int)>((pr) =>
        //        {

        //        })));

        //    PARAMBNDs = gameparamBnds.Concat(drawparamBnds).ToList();

        //    foreach (var bnd in PARAMBNDs)
        //    {
        //        bnd.RestoreBackup();
        //    }

        //    LoadAllPARAMs();

        //    var asdf = new List<string>();
        //    SaveAllPARAMs(asdf);

        //    Application.Current.Shutdown();
        //}

        public async Task RestoreBackupsInOtherThread(Func<string, Task> setWait)
        {
            await Task.Run(async () =>
            {
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    await setWait?.Invoke("Restoring all backups...");
                });

                foreach (var bnd in MSGBNDs)
                {
                    if (bnd.RestoreBackup() != true)
                    {
                        throw new Exception();
                    }

                    DataFile.Reload(bnd, new Progress<(int, int)>((p) =>
                    {

                    }));
                }

                LoadAllFMGs();

                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    await setWait?.Invoke(null);
                });
            });
        }

        private void SaveAllFMGs(List<string> backupsCreated)
        {
            foreach (var msgBnd in MSGBNDs)
            {
                if(msgBnd.CreateBackup(overwriteExisting: false) == true)
                {
                    backupsCreated.Add(msgBnd.FileBackupPath);
                }

                foreach (var fmg in msgBnd)
                {
                    var matchingFmg = FMGs.Where(x => x.Key == IOHelper.RemoveExtension(new FileInfo(fmg.Name).Name, EXT_FMG)).First();

                    fmg.ReplaceData(matchingFmg.Value, 
                        new Progress<(int, int)>((p) =>
                    {

                    }));
                }

                DataFile.Resave(msgBnd, new Progress<(int, int)>((p) =>
                {

                }));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
