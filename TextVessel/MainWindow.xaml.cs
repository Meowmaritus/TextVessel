using MahApps.Metro.Controls;
using MeowDSIO;
using MeowDSIO.DataFiles;
using MeowDSIO.DataTypes.PARAM;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TextVessel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            //DEBUG
            //MAINDATA.DEBUG_RestoreBackupsLoadResave();
        }

        private void SetLoadingMode(bool isLoading)
        {
            MainGrid.Opacity = isLoading ? 0.25 : 1;
            MainGrid.IsEnabled = !isLoading;

            LoadingTextBox.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;

            Mouse.OverrideCursor = isLoading ? Cursors.Wait : null;
        }

        private void SetSavingMode(bool isLoading)
        {
            MainGrid.Opacity = isLoading ? 0.25 : 1;
            MainGrid.IsEnabled = !isLoading;

            SavingTextBox.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;

            Mouse.OverrideCursor = isLoading ? Cursors.Wait : null;
        }

        private async Task BrowseForInterrootDialog(Action<bool> setIsLoading)
        {
            var browseDialog = new OpenFileDialog()
            {
                AddExtension = false,
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
                FileName = "DARKSOULS.exe",
                Filter = "Dark Souls EXEs (Usually DARKSOULS.exe)|*.exe",
                ShowReadOnly = false,
                Title = "Choose your DARKSOULS.exe file...",
                ValidateNames = true
            };

            if ((browseDialog.ShowDialog() ?? false) == true)
            {
                var interrootDir = new FileInfo(browseDialog.FileName).DirectoryName;
                if (CheckInterrotDirValid(interrootDir))
                {
                    MAINDATA.Config.InterrootPath = interrootDir;
                    MAINDATA.SaveConfig();
                    await MAINDATA.LoadFmgsInOtherThread(setIsLoading);
                }
                else
                {
                    var sb = new StringBuilder();

                    sb.AppendLine(@"Directory of EXE chosen did not include the following directories/files which are required:");
                    sb.AppendLine(@" - '.\param\DrawParam\'");
                    sb.AppendLine(@" - '.\param\GameParam\'");
                    sb.AppendLine(@" - '.\param\GameParam\GameParam.parambnd'");
                    sb.AppendLine(@" - '.\paramdef\paramdef.paramdefbnd'");

                    if (CheckIfUdsfmIsProbablyNotInstalled(interrootDir))
                    {
                        sb.AppendLine();
                        sb.AppendLine();
                        sb.AppendLine("This installation does not appear to be unpacked with " +
                            "UnpackDarkSoulsForModding because it meets one or more of the " +
                            "criteria below (please note that it is only a suggestion and not " +
                            "required for this tool to function; only the above criteria is " +
                            "required to be met in order to use this tool).");

                        sb.AppendLine(@" - '.\unpackDS-backup' does not exist.");
                        sb.AppendLine(@" - '.\dvdbnd0.bdt' exists.");
                        sb.AppendLine(@" - '.\dvdbnd1.bdt' exists.");
                        sb.AppendLine(@" - '.\dvdbnd2.bdt' exists.");
                        sb.AppendLine(@" - '.\dvdbnd3.bdt' exists.");
                        sb.AppendLine(@" - '.\dvdbnd0.bhd5' exists.");
                        sb.AppendLine(@" - '.\dvdbnd1.bhd5' exists.");
                        sb.AppendLine(@" - '.\dvdbnd2.bhd5' exists.");
                        sb.AppendLine(@" - '.\dvdbnd3.bhd5' exists.");
                    }

                    MessageBox.Show(
                        sb.ToString(), 
                        "Invalid Directory", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);

                }
            }
        }

        private bool CheckIfUdsfmIsProbablyNotInstalled(string dir)
        {
            return !Directory.Exists(IOHelper.Frankenpath(dir, @"unpackDS-backup")) ||
                File.Exists(IOHelper.Frankenpath(dir, @"dvdbnd0.bdt")) ||
                File.Exists(IOHelper.Frankenpath(dir, @"dvdbnd1.bdt")) ||
                File.Exists(IOHelper.Frankenpath(dir, @"dvdbnd2.bdt")) ||
                File.Exists(IOHelper.Frankenpath(dir, @"dvdbnd3.bdt")) ||
                File.Exists(IOHelper.Frankenpath(dir, @"dvdbnd0.bhd5")) ||
                File.Exists(IOHelper.Frankenpath(dir, @"dvdbnd1.bhd5")) ||
                File.Exists(IOHelper.Frankenpath(dir, @"dvdbnd2.bhd5")) ||
                File.Exists(IOHelper.Frankenpath(dir, @"dvdbnd3.bhd5"));
        }

        private bool CheckInterrotDirValid(string dir)
        {
            return
                Directory.Exists(IOHelper.Frankenpath(dir, @"param\DrawParam")) &&
                Directory.Exists(IOHelper.Frankenpath(dir, @"param\GameParam")) &&
                Directory.Exists(IOHelper.Frankenpath(dir, @"paramdef")) &&
                File.Exists(IOHelper.Frankenpath(dir, @"param\GameParam\GameParam.parambnd")) &&
                File.Exists(IOHelper.Frankenpath(dir, @"paramdef\paramdef.paramdefbnd"));
        }

        //private void RANDOM_DEBUG_TESTING()
        //{
        //    //var uniqueInternalDataTypes = new List<string>();
        //    //foreach (var p in MAINDATA.ParamDefs)
        //    //{
        //    //    string testDir = IOHelper.Frankenpath(Environment.CurrentDirectory, "VERBOSE_DUMP");

        //    //    if (!Directory.Exists(testDir))
        //    //        Directory.CreateDirectory(testDir);

        //    //    string testFileName = IOHelper.Frankenpath(testDir, p.Key + ".txt");

        //    //    var sb = new StringBuilder();

        //    //    foreach (var e in p.Value.Entries)
        //    //    {
        //    //        sb.AppendLine($"{e.Name}:");
        //    //        sb.AppendLine($"\tID: {e.ID}");
        //    //        sb.AppendLine($"\tInternal Value Type: {e.InternalValueType}");
        //    //        sb.AppendLine($"\tDefault: {e.DefaultValue}");
        //    //        sb.AppendLine($"\t[PARAM MAN] Display Name: {e.DisplayName}:");
        //    //        sb.AppendLine($"\t[PARAM MAN] Description: {e.Description}");
        //    //        sb.AppendLine($"\t[PARAM MAN] Min: {e.Min}");
        //    //        sb.AppendLine($"\t[PARAM MAN] Max: {e.Max}");
        //    //        sb.AppendLine($"\t[PARAM MAN] Incrementation: {e.Increment}");
        //    //        sb.AppendLine($"\t[PARAM MAN] GUI Value Format: \"{e.GuiValueStringFormat}\"");
        //    //        sb.AppendLine($"\t[PARAM MAN] GUI Value Type: {e.GuiValueType}");
        //    //        sb.AppendLine($"\t[PARAM MAN] GUI Value Mode: {e.GuiValueDisplayMode}");
        //    //        sb.AppendLine($"\t[PARAM MAN] GUI Value Size: {e.GuiValueByteCount}");
        //    //        sb.AppendLine();
        //    //    }

        //    //    File.WriteAllText(testFileName, sb.ToString());
        //    //}

        //    var sb = new StringBuilder();

        //    foreach (var p in MAINDATA.Params)
        //    {
        //        int defSize = p.Value.AppliedPARAMDEF.CalculateEntrySize();

        //        if (p.Value._debug_calculatedEntrySize != defSize)
        //        {
        //            sb.AppendLine($"Entry size check fail - {p.BNDName} - {p.Value.Name} - {p.Value._debug_calculatedEntrySize} (Def: {defSize})");

        //            //foreach (var e in p.Value.AppliedPARAMDEF.Entries)
        //            //{

        //            //    sb.AppendLine($"\tDef Entry size check fail ({e.Name}) - BitCount = {e.ValueBitCount}, DispVarBytes*8 = {(e.GuiValueByteCount * 8)}");
        //            //}
        //        }
        //    }

        //    Console.WriteLine(sb.ToString());

        //    Console.WriteLine();
        //}

        public FMG SelectedFmg
        {
            get
            {
                if (MainTabs.SelectedValue != null)
                    return (MainTabs.SelectedItem as FMGRef).Value;
                else
                    return null;
            }
        }

        private async void MenuSelectDarkSoulsDirectory_Click(object sender, RoutedEventArgs e)
        {
            await BrowseForInterrootDialog(SetLoadingMode);
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MAINDATA.LoadConfig();

            if (!string.IsNullOrWhiteSpace(MAINDATA.Config?.InterrootPath))
            {
                await MAINDATA.LoadFmgsInOtherThread(SetLoadingMode);
            }
            else
            {
                if (MessageBox.Show("Note: A Dark Souls installation unpacked by the mod " +
                    "'UnpackDarkSoulsForModding' by HotPocketRemix is >>>REQUIRED<<<." +
                    "\n" +
                    @"Please navigate to your '.\DATA\DARKSOULS.exe' file." +
                    "Once the inital setup is performed, the path will be saved." +
                    "\nYou may press cancel to continue without selecting the path but the GUI will " +
                    "be blank until you go to 'File -> Select Dark Souls Directory...'",
                    "Initial Setup", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                {
                    await BrowseForInterrootDialog(SetLoadingMode);
                }
            }

            MainTabs.Items.Refresh();

            //RANDOM_DEBUG_TESTING();
        }

        private void CmdSave_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private async void CmdSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            await MAINDATA.SaveInOtherThread(SetSavingMode);
        }

        private void SaveParamRowIndex()
        {
            var selectedParam = (MainTabs.SelectedItem as FMGRef);

            if (selectedParam == null)
                return;

            //TODO: SaveParamRowIndex
        }

        private void LoadParamRowIndex()
        {
            var selectedParam = (MainTabs.SelectedItem as FMGRef);

            if (selectedParam == null)
                return;

            //TODO: LoadParamRowIndex
        }

        private void ParamEntryList_CurrentCellChanged(object sender, EventArgs e)
        {
            SaveParamRowIndex();
        }

        private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainTabs == null)
                return;

            MAINDATA.Config.LastFmgIndex = MainTabs.SelectedIndex;

            LoadParamRowIndex();
        }

        private void ParamEntryList_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            LoadParamRowIndex();

            //SetLoadingMode(false);
        }

        private void ParamEntryList_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            SaveParamRowIndex();
        }

        private void ParamEntryList_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //TODO: Use e.Cancel, asking user if they wanna save changes and all that

            //Even if the user decides not to save the params, always save the config:
            MAINDATA.SaveConfig();
        }

        private void MainTabs_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            SetLoadingMode(false);
            MainTabs.SelectedIndex = MAINDATA.Config.LastFmgIndex;
        }

        private async void MenuRestoreBackups_Click(object sender, RoutedEventArgs e)
        {
            await MAINDATA.RestoreBackupsInOtherThread(SetLoadingMode);
        }

        private void ParamManStyleDataGrid_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            var about = new About();
            about.Owner = this;
            about.ShowDialog();
        }

    }
}
