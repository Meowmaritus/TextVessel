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
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace TextVessel
{
    public class MainDataContext : INotifyPropertyChanged
    {
        private UserConfig _config = new UserConfig();
        public UserConfig Config
        {
            get => _config;
            set
            {
                _config = value;
                NotifyPropertyChanged(nameof(Config));
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
                NotifyPropertyChanged(nameof(FMGs));
            }
        }

        public async Task LoadFmgsInOtherThread(Action<bool> setIsLoading)
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    setIsLoading?.Invoke(true);
                });

                LoadAllFMGs();

                //Application.Current.Dispatcher.Invoke(() =>
                //{
                //    Mouse.OverrideCursor = null;
                //    setIsLoading?.Invoke(false);
                //});
            });
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

            MSGBNDs = Directory.GetFiles(Config.GetMsgFolderForSelectedLanguage(), "*.msgbnd")
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

            Application.Current.Dispatcher.Invoke(() =>
            {
                FMGs = UPCOMING_FMGs;
            });
        }


        public async Task SaveInOtherThread(Action<bool> setIsLoading)
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    setIsLoading?.Invoke(true);
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

                Application.Current.Dispatcher.Invoke(() =>
                {
                    setIsLoading?.Invoke(false);
                });
            });
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

        public async Task RestoreBackupsInOtherThread(Action<bool> setIsLoading)
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    setIsLoading?.Invoke(true);
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

                Application.Current.Dispatcher.Invoke(() =>
                {
                    setIsLoading?.Invoke(false);
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

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
