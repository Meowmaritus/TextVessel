using MahApps.Metro.Controls;
using MeowDSIO;
using MeowDSIO.DataFiles;
using MeowDSIO.DataTypes.FMG;
using MeowDSIO.DataTypes.PARAM;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private object __ctrlSync = new object();
        private bool _ctrl = false;

        public bool Ctrl
        {
            get
            {
                bool res = false;
                lock (__ctrlSync)
                {
                    res = _ctrl;
                }
                return res;
            }

            set
            {
                lock (__ctrlSync)
                {
                    _ctrl = value;
                }
            }
        }

        private object __needsScrollSync = new object();
        private bool _needsScroll = false;

        public bool NeedsScroll
        {
            get
            {
                bool res = false;
                lock (__needsScrollSync)
                {
                    res = _needsScroll;
                }
                return res;
            }

            set
            {
                lock (__needsScrollSync)
                {
                    _needsScroll = value;
                }
            }
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            context.MainListViewVisibility = Visibility.Hidden;

            Border fmgListBorder = (Border)VisualTreeHelper.GetChild(MainListView, 0);
            FmgScrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(fmgListBorder, 0);

            context.LoadConfig();

            if (!string.IsNullOrWhiteSpace(context.Config?.InterrootPath))
            {
                await context.LoadFmgsInOtherThread(SetWait);
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
                    await BrowseForInterrootDialog(SetWait);
                }
            }

            context.PropertyChanged += Context_PropertyChanged;

            DataLanguageListBox.SelectedItem = context.DataLanguage;
        }

        private void Context_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(context.SelectedFMG))
            {
                NeedsScroll = true;
            }
        }

        public static readonly DependencyProperty WaitAnimProgressProperty =
            DependencyProperty.Register(nameof(WaitAnimProgress), typeof(double), typeof(MainWindow),
                new PropertyMetadata(WaitAnimProgressCallback));

        public double WaitAnimProgress
        {
            get => (double)GetValue(WaitAnimProgressProperty);
            set => SetValue(WaitAnimProgressProperty, value);
        }

        static void WaitAnimProgressCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var contextInstance = (d.GetValue(DataContextProperty) as MainDataContext);
            if (contextInstance != null)
            {
                if (e.Property == WaitAnimProgressProperty)
                    contextInstance.WaitAnimProgress = (double)e.NewValue;
            }
        }

        public ScrollViewer FmgScrollViewer = null;

        public MainWindow()
        {
            InitializeComponent();

            //DEBUG
            //context.DEBUG_RestoreBackupsLoadResave();

            context.LoadSyntaxHighlighting();
        }

        public async Task SetWait(string waitingActionDescription)
        {
            if (waitingActionDescription != null)
            {
                while (context.IsAnimWait)
                {
                    await Task.Delay(100);
                }

                context.WaitText = waitingActionDescription;
                context.WaitAnimProgress = 0;
            }

            context.IsWait = waitingActionDescription != null;
            MainGrid.IsHitTestVisible = !context.IsWait;

            Mouse.OverrideCursor = context.IsWait ? Cursors.Wait : null;
        }

        private async Task BrowseForInterrootDialog(Func<string, Task> setWait)
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
                    context.Config.InterrootPath = interrootDir;
                    context.SaveConfig();
                    await context.LoadFmgsInOtherThread(setWait);
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

        private async void MenuSelectDarkSoulsDirectory_Click(object sender, RoutedEventArgs e)
        {
            await BrowseForInterrootDialog(SetWait);
        }

        private void CmdSave_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (!context.IsWait && context.IsModified);
        }

        private async void CmdSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            await context.SaveInOtherThread(SetWait);
        }

        private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            context.Config.LastFmgIndex = MainTabs.SelectedIndex;
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //TODO: Use e.Cancel, asking user if they wanna save changes and all that

            //Even if the user decides not to save the params, always save the config:
            context.SaveConfig();
        }

        private void MainTabs_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            MainTabs.SelectedIndex = -1;
            MainTabs.SelectedIndex = context.Config.LastFmgIndex;
        }

        private async void MenuRestoreBackups_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Are you sure you want to restore the backups for " +
                $"all {context.DataLanguage.EngName} text files, losing any custom edits done?",
                "Restore Backups?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                await context.RestoreBackupsInOtherThread(SetWait);
            }
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            var about = new About();
            about.Owner = this;
            about.ShowDialog();
        }

        private void CodeEditor_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var ce = (sender as CodeEditor);

            ce.TextArea.ClearSelection();

            MainListView.SelectedIndex = -1;
        }

        private void CodeEditor_LostFocus(object sender, RoutedEventArgs e)
        {
            var ce = (sender as CodeEditor);

            ce.TextArea.ClearSelection();
        }

        private void SaveCurrentFmgScrollOffset()
        {
            if (MainListView == null)
                return;

            if (MainTabs.SelectedItem is FMGRef fmg)
            {
                if (context.Config.LastFmgScrollOffsets.ContainsKey(fmg.Key))
                    context.Config.LastFmgScrollOffsets[fmg.Key] = FmgScrollViewer.VerticalOffset;
                else
                    context.Config.LastFmgScrollOffsets.Add(fmg.Key, FmgScrollViewer.VerticalOffset);
            }
        }

        private void LoadCurrentFmgScrollOffset()
        {
            if (MainListView == null)
                return;

            if (MainTabs.SelectedItem is FMGRef fmg)
            {
                if (context.Config.LastFmgScrollOffsets.ContainsKey(fmg.Key))
                    FmgScrollViewer?.ScrollToVerticalOffset(context.Config.LastFmgScrollOffsets[fmg.Key]);
            }
        }

        private async void MainListView_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (e.Property.Name == nameof(MainListView.ItemsSource))
            {
                await SetWait(null);
            }
        }

        private void MainTabs_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SaveCurrentFmgScrollOffset();
        }

        private async void DataLanguageListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (context.DataLanguage != context.CurrentlyLoadedLanguage)
            {
                if (context.IsModified)
                {
                    var msgResult = MessageBox.Show($"Would you like to save all unsaved " +
                        $"changes to the FMG files for the " +
                        $"current language, {context.CurrentlyLoadedLanguage.EngName}, " +
                        $"before switching to the selected " +
                        $"language, {context.DataLanguage.EngName}?",
                        "Keep Unsaved Changes?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                    if (msgResult == MessageBoxResult.Cancel)
                    {
                        await Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle,
                            new Action(() =>
                            {
                                DataLanguageListBox.SelectionChanged -= DataLanguageListBox_SelectionChanged;
                                context.DataLanguage = context.CurrentlyLoadedLanguage;
                                DataLanguageListBox.SelectionChanged += DataLanguageListBox_SelectionChanged;
                            }));
                        
                        return;
                    }
                    else
                    {
                        if (msgResult == MessageBoxResult.Yes)
                        {
                            await context.SaveInOtherThread(SetWait);
                        }

                        await context.LoadFmgsInOtherThread(SetWait);
                    }
                }
                else
                {
                    await context.LoadFmgsInOtherThread(SetWait);
                }

            }
        }

        private void MetroWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (IsKeyboardFocusWithin && e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                Ctrl = true;
            }
        }

        private void MetroWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (IsKeyboardFocusWithin && e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                Ctrl = false;
            }
        }

        private void MainListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Ctrl && IsKeyboardFocusWithin)
            {
                if (e.Delta > 0)
                    context.Zoom.ZoomLevel++;
                else
                    context.Zoom.ZoomLevel--;

                e.Handled = true;
            }
        }

        private void CmdZoomIn_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            context.Zoom.ZoomLevel++;
        }

        private void CmdZoomOut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            context.Zoom.ZoomLevel--;
        }

        private void CmdZoomReset_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            context.Zoom.ZoomLevel = 0;
        }

        private void MainListView_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (NeedsScroll)
            {
                NeedsScroll = false;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    LoadCurrentFmgScrollOffset();
                }), System.Windows.Threading.DispatcherPriority.ContextIdle);
            }
            else if (!NeedsScroll && context.MainListViewVisibility != Visibility.Visible)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    context.MainListViewVisibility = Visibility.Visible;
                }), System.Windows.Threading.DispatcherPriority.ContextIdle);
            }
        }

        private void MetroWindow_LostFocus(object sender, RoutedEventArgs e)
        {
            Ctrl = false;
        }

        private void ListViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (string.IsNullOrEmpty(context.Config.Filter))
            {
                e.Accepted = true;
                return;
            }

            var entry = e.Item as FMGEntryRef;

            e.Accepted = entry.Value.IndexOf(context.Config.Filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void ButtonApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            context.Config.Filter = FilterTextBox.Text;

            CollectionViewSource.GetDefaultView(MainListView.ItemsSource).Refresh();
        }

        private void ButtonClearFilter_Click(object sender, RoutedEventArgs e)
        {
            context.Config.Filter = string.Empty;

            CollectionViewSource.GetDefaultView(MainListView.ItemsSource).Refresh();
        }

        private void FilterTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && FilterTextBox.IsFocused)
            {
                context.Config.Filter = FilterTextBox.Text;

                CollectionViewSource.GetDefaultView(MainListView.ItemsSource).Refresh();
            }
        }

        private void CmdGoto_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var gotoWind = new GotoWindow();
            gotoWind.Owner = this;
            gotoWind.ShowDialog();
            var gotoVal = gotoWind.Goto;
            if (gotoVal >= 0)
            {
                var matches = MainListView.Items.Cast<FMGEntryRef>().Where(x => x.ID == gotoVal);

                if (matches.Any())
                {
                    var match = matches.First();
                    MainListView.ScrollIntoView(match);
                    MainListView.SelectedItem = match;
                }
                else
                {
                    MessageBox.Show($"No entry found with ID {gotoVal}.");
                }
            }
        }

        private void CmdGoto_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!CollectionViewSource.GetDefaultView(MainListView.ItemsSource).IsEmpty)
            {
                e.CanExecute = true;
            }
        }

        private void MetroWindow_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsKeyboardFocusWithin)
            {
                Ctrl = false;
            }
        }
    }
}
