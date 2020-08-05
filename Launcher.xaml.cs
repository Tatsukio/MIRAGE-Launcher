using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Linq;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
//using System.Security.Cryptography;

using static MIRAGE_Launcher.ModManager;

namespace MIRAGE_Launcher
{
    public partial class MainWindow : Window
    {
        static MediaPlayer MediaPlayer = new MediaPlayer();
        static XmlDocument Localization = new XmlDocument();

        static bool PlayMusic = true;
        static bool IsFirstLaunch = false;

        static string LauncherExeDir = AppDomain.CurrentDomain.BaseDirectory;
        static string ParaworldDir = LauncherExeDir + "../../";
        static string ParaworldBinDir = ParaworldDir + "/bin";
        static string ToolsDir = ParaworldDir + "/Tools";
        static string LauncherDBPath = ToolsDir + "/Launcher/launcher_db.xml";
        public static string InfoDir = ParaworldDir + "/Data/Info/";

        static string CacheDir = Path.GetTempPath() + "/SpieleEntwicklungsKombinat/Paraworld";
        static string AppDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        static string SettingsDir = Path.GetFullPath(AppDataDir + "/SpieleEntwicklungsKombinat/Paraworld");
        static string SettingsPath = Path.GetFullPath(SettingsDir + "/settings.cfg");
        static string SettingsBackupPath = Path.GetFullPath(SettingsDir + "/Settings_SSSS_backup.cfg");

        static string TurnMuscOff = "Turn music off";
        static string TurnMuscOn = "Turn music on";
        static string OpenPWTool = "Open PWTool";
        static string ClosePWTool = "Close PWTool";
        static string Warning = "Warning";
        static string FileNotFound = "File not found";
        static string BackupMissing = "Backup file not found";
        static string OverwriteBackup = "Backup file already exists. Overwrite backup file?";
        static string PWIsAlreadyRunning = "ParaWorld is already running. Start PWKiller?";
        static string LauncherIsAlreadyRunning = "This programm is already running. Please close the running version first!";
        static string BackupCreated = "Created Settings_SSSS_backup.cfg from settings.cfg. This one will be used when settings.cfg becomes corrupt.";
        static string ResetSettingsSuccess = "Replaced settings.cfg with Settings_SSSS_backup.cfg. Some options might have been reset to an old state!";
        static string ResetSettings = "This will reset your settings.cfg file, and some saved data (like last IP addresses) will be lost. Do you really want to continue?";
        static string NoCacheFound = "No cache files found.";
        static string CacheDeleted = "The following cache files have been deleted successfully:";
        static string PWFontsMissing = "Trebuchet MS fonts not found.";
        static string BPMissing = "BoosterPack is not installed, please install it first. You can download it from Para-Welt.com or ParaWorld ModDB.";
        static string SettingsMissing = "Settings.cfg not found. If you have never run ParaWorld on this system before, you must run it first to create the necessary files.";
        static string SwitchSSSError = "Failed to switch server side scripts.";
        static string AskBackup = "Do you want to try to use the backup file?";

        static string InitError = "";
        static string FirstLaunchError = "";
        static string WhatsNew = "";
        public static string EnabledMods = "";

        public MainWindow()
        {
            if (Process.GetProcessesByName("MIRAGE Launcher").Count() == 1)
            {
                InitializeComponent();
                LoadDB();
                LoadUI();
                if (!string.IsNullOrEmpty(InitError))
                {
                    MessageBox.Show(InitError.TrimEnd('\n'), null, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                if (IsFirstLaunch)
                {
                    OnFirstLaunch();
                }

                GetMods();
                Task TGetMyPublicIp = new Task(GetMyPublicIp);
                TGetMyPublicIp.Start();
            }
            else
            {
                InitializeComponent();
                LoadDB();
                MessageBox.Show(LauncherIsAlreadyRunning, Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                Application.Current.Shutdown();
            }
        }

        private void DragMove(object sender, MouseButtonEventArgs e)
        {
            DragMove();
            if (e.ClickCount == 2) WindowState = WindowState.Minimized;
        }

        private void LoadDB()
        {
            if (FileFound(LauncherDBPath))
            {
                Localization.Load(LauncherDBPath);
                string Version = Translate("/mod_version");
                Task TVersionCheck = new Task(() => VersionCheck(Version));
                TVersionCheck.Start();

                MainLabel.Text                      = Translate("/main_label");
                StartMirage.Content                 = Translate("/start_mod");
                StartSdk.Content                    = Translate("/start_sdk");
                StartServer.Content                 = Translate("/start_server");
                OpenTavern.Content                  = Translate("/open_tavern");
                TurnMuscOff                         = Translate("/turn_music_off");
                TurnMuscOn                          = Translate("/turn_music_on");
                ClearCacheButton.Content            = Translate("/clear_cache");
                OpenSettings.Content                = Translate("/open_settings");
                KillProcesses.Content               = Translate("/kill_processes");
                OpenPWTool                          = Translate("/open_pwtool");
                ClosePWTool                         = Translate("/close_pwtool");
                Uninstall.Content                   = Translate("/uninstall");
                Exit.Content                        = Translate("/exit");
                SSSOnButton.Content                 = Translate("/sss_on");
                SSSOffButton.Content                = Translate("/sss_off");
                RestoreSettingsButton.Content       = Translate("/restore_settings");
                CreateSettingsBackupButton.Content  = Translate("/create_settings_backup");
                UpdateLabel.Text                    = Translate("/update_label");
                SwitchMusicButton.Content           = TurnMuscOff;
                SwitchPWTool.Content                = OpenPWTool;

                Warning = Translate("/warning");

                string ErrorCode = "Code#"; //Line num in mirage_db.xml

                PWIsAlreadyRunning          = ErrorCode + "26\n" + Translate("/pw_is_already_running");
                LauncherIsAlreadyRunning    = ErrorCode + "27\n" + Translate("/launcher_is_already_running");
                BackupMissing               = ErrorCode + "28\n" + Translate("/backup_missing");
                ResetSettings               = ErrorCode + "29\n" + Translate("/reset_settings");
                ResetSettingsSuccess        = ErrorCode + "30\n" + Translate("/reset_settings_success");
                OverwriteBackup             = ErrorCode + "31\n" + Translate("/overwrite_backup");
                BackupCreated               = ErrorCode + "32\n" + Translate("/backup_created");
                NoCacheFound                = ErrorCode + "33\n" + Translate("/no_cache_found");
                CacheDeleted                = ErrorCode + "34\n" + Translate("/cache_deleted");
                BPMissing                   = ErrorCode + "35\n" + Translate("/bp_missing");
                SettingsMissing             = ErrorCode + "36\n" + Translate("/settings_missing");
                SwitchSSSError              = ErrorCode + "37\n" + Translate("/switch_sss_error");
                AskBackup                   = ErrorCode + "38\n" + Translate("/ask_backup");

                IsFirstLaunch = Convert.ToBoolean(Localization.SelectSingleNode("/launcher_db/launcher_misc/is_first_launch").InnerText);

                EnabledMods = Localization.SelectSingleNode("/launcher_db/launcher_misc/enabled_mods").InnerText;
            }
        }

        public string Translate(string Text)
        {
            return Localization.DocumentElement.SelectSingleNode("/launcher_db/launcher_localization" + Text).InnerText;
        }

        public void SaveToDB(string Name, string Value)
        {
            Localization.DocumentElement.SelectSingleNode("/launcher_db/launcher_misc" + Name).InnerText = Value;
            Localization.Save(LauncherDBPath);
        }

        public void VersionCheck(string Version)
        {
            Version MirageVersion = new Version(Version);
            using (WebClient VersionPage = new WebClient())
            {
                VersionPage.Proxy = new WebProxy();
                //string FullSiteVersion = VersionPage.DownloadString("https://para-welt.com/mirage/version.txt");
                string FullSiteVersion = VersionPage.DownloadString("https://raw.githubusercontent.com/Tatsukio/MIRAGE-Launcher/master/Res/updateinfo.txt");
                //versioncheck	MIRAGE 2.6.2	14	0
                string[] Info = FullSiteVersion.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (Info[0] == "versioncheck" && Info != null)
                {
                    string[] SiteModVersion = Info[1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    Version MirageSiteVersion = new Version(SiteModVersion[1]);
                    switch (MirageVersion.CompareTo(MirageSiteVersion))
                    {
                        case 0:
                            //MirageVersion == MirageSiteVersion
                            break;
                        case 1:
                            //MirageVersion > MirageSiteVersion
                            break;
                        case -1:
                            WhatsNew = Info[4];
                            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => ShowUpdateWindow()));
                            break;
                    }
                }
            }
        }

        private void LoadUI()
        {
            Random Random = new Random();
            int BGIndex = Random.Next(1, 85);
            string MusicDir = Path.GetFullPath(ParaworldDir + "/Data/Base/Audio/Music/");
            string BGDir = Path.GetFullPath(ToolsDir + "/Launcher/Backgrounds/");

            if (!Directory.Exists(BGDir) || !File.Exists(BGDir + "background_" + BGIndex + ".jpg"))
            {
                InitError += "Folder " + BGDir + " not found or empty.\n\n";
            }
            else
            {
                LauncherBG.Source = new BitmapImage(new Uri(BGDir + "background_" + BGIndex + ".jpg"));
            }

            MusicDir += GetMusicName(BGIndex) + ".mp3";

            if (!File.Exists(MusicDir))
            {
                InitError += MusicDir + " not found.\n\n";
            }
            else
            {
                MediaPlayer.MediaEnded += (sender, eventArgs) => LoadUI();
                MediaPlayer.Open(new Uri(MusicDir, UriKind.Relative));
                MediaPlayer.Play();
            }
        }

        private string GetMusicName(int MusicIndex)
        {
            switch (MusicIndex)
            {
                case 1:
                    return "01_maintheme";
                case 2:
                case 41:
                case 43:
                    return "13_plain_icewaste";
                case 3:
                case 65:
                case 67:
                    return "11_plain_jungle_1";
                case 4:
                case 48:
                    return "23_location_arena";
                case 5:
                case 53:
                case 74:
                    return "36_combat_aje_1";
                case 6:
                case 82:
                    return "43_combat_ninigi_2";
                case 7:
                    return "10_plain_heroes";
                case 8:
                case 45:
                case 54:
                case 83:
                    return "35_combat_hu_1";
                case 9:
                    return "04_plain_northland_1";
                case 10:
                case 60:
                    return "41_combat_hu_2";
                case 11:
                    return "16_darkzone_3";
                case 12:
                case 56:
                case 75:
                    return "42_combat_aje_2";
                case 13:
                case 36:
                case 38:
                    return "48_var_combat_aje_1";
                case 14:
                    return "15_darkzone_2";
                case 15:
                case 73:
                case 85:
                    return "17_maintheme_hu";
                case 16:
                case 50:
                case 81:
                    return "47_var_combat_hu_2";
                case 17:
                case 61:
                case 71:
                    return "50_var_combat_seas_1";
                case 18:
                case 66:
                case 78:
                    return "12_plain_jungle_2";
                case 19:
                case 79:
                    return "51_location_seas_temple";
                case 20:
                    return "30_location_scientist_hut";
                case 21:
                case 35:
                case 55:
                case 57:
                    return "39_combat_dinos";
                case 22:
                case 27:
                case 80:
                    return "28_location_walhalla";
                case 23:
                case 24:
                    return "32_location_holycity";
                case 25:
                    return "26_location_water_temple";
                case 26:
                case 29:
                    return "27_location_entry_to_walhalla";
                case 28:
                    return "29_location_aeroplane";
                case 30:
                    return "25_location_pirates";
                case 31:
                    return "20_location_druids";
                case 32:
                    return "06_plain_savannah_1";
                case 33:
                    return "21_location_amazons";
                case 34:
                case 47:
                    return "18_maintheme_aje";
                case 64:
                    return "45_var_plain_savannah_1";
                case 37:
                    return "05_plain_northland_2";
                case 52:
                case 76:
                    return "07_plain_savannah_2";
                case 39:
                    return "40_combat_heroes";
                case 40:
                    return "08_plain_savannah_3";
                case 42:
                    return "14_darkzone_1";
                case 44:
                    return "34_location_prison_island";
                case 46:
                    return "49_var_combat_ninigi_1";
                case 49:
                    return "09_plain_savannah_4";
                case 51:
                    return "37_combat_ninigi_1";
                case 58:
                case 70:
                case 72:
                    return "38_combat_seas_1";
                case 59:
                    return "19_maintheme_ninigi";
                case 62:
                case 63:
                    return "44_var_plain_northland_1";
                case 68:
                    return "33_location_holycity_walls";
                case 69:
                    return "24_location_temple";
                case 77:
                    return "46_var_plain_heroes";
                case 84:
                    return "22_location_the_gate";
            }
            return null;
        }

        private void GetMyPublicIp()
        {
            using (WebClient GetIP = new WebClient())
            {
                string MyIP = GetIP.DownloadString(new Uri("https://ipinfo.io/ip")).Trim();
                string IPPath = CacheDir + "/paraworld_ip.txt";
                bool NewIP = true;
                if (File.Exists(IPPath))
                {
                    using (StreamReader ReadIP = new StreamReader(IPPath))
                    {
                        string PreviousIP = ReadIP.ReadLine();
                        if (MyIP == PreviousIP)
                        {
                            NewIP = false;
                        }
                        else
                        {
                            NewIP = true;
                        }
                    }
                }
                if (NewIP == true)
                {
                    using (StreamWriter WriteIP = new StreamWriter(IPPath, false, System.Text.Encoding.Default))
                    {
                        WriteIP.WriteLine(MyIP);
                    }
                }
            }
        }

        private void OnFirstLaunch()
        {
            bool PWFontsCheck = false;
            bool BPCheck = false;
            bool SettingsCheck = false;


            /*
            //Check for Tages drivers
            string TagesDir = Environment.SystemDirectory;
            if (!File.Exists(TagesDir + "/atksgt.sys") || !File.Exists(TagesDir + "/lirsgt.sys"))
            {
                ErrorSum += "Tages drivers not found in\n" + TagesDir + "\n\n";
            }
            //End of Tages drivers check

            //Check for Win7Fix
            if (File.Exists(ParaworldBinDir + "/Paraworld.exe"))
            {
                using (MD5 MD5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(ParaworldBinDir + "/Paraworld.exe"))
                    {
                        byte[] Win7FixExeMD5 = { 08, 41, 82, 123, 147, 26, 93, 185, 136, 237, 71, 119, 102, 252, 145, 01 };
                        byte[] ExeMD5 = MD5.ComputeHash(stream);
                        if (Win7FixExeMD5.SequenceEqual(ExeMD5) == false)
                        {
                            FirstLaunchError += "Install Win7Fix\n\n";
                        }
                    }
                }
            }
            else
            {

            }
            //End of Win7Fix check
            */

            //Check for PW fonts
            string FontsDir = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            if (File.Exists($"{FontsDir}/trebuc.ttf") && File.Exists($"{FontsDir}/trebucbd.ttf") && File.Exists($"{FontsDir}/trebucbi.ttf") && File.Exists($"{FontsDir}/trebucit.ttf"))
            {
                PWFontsCheck = true;
            }
            else
            {
                FirstLaunchError += PWFontsMissing + "\n\n";
            }
            //End of PW fonts check

            //Check for BP
            if (Directory.Exists(ParaworldDir))
            {
                if (Directory.Exists(ParaworldDir + "/Data/BoosterPack1"))
                {
                    BPCheck = true;
                }
                else
                {
                    FirstLaunchError += BPMissing + "\n\n";
                }
            }
            //End of BP check

            //Check for settings.cfg
            if (Directory.Exists(SettingsDir) && File.Exists(SettingsPath))
            {
                SettingsCheck = true;
            }
            else
            {
                FirstLaunchError += SettingsMissing + "\n\n";
            }
            //End of settings.cfg check

            if (PWFontsCheck && BPCheck && SettingsCheck)
            {
                SaveToDB("/is_first_launch", false.ToString());
            }
        }

        private bool EnableSSS(bool Enable)
        {
            if (FileFound(ToolsDir + "/mod_conf.exe"))
            {
                Process mod_conf = new Process();
                mod_conf.StartInfo.FileName = ToolsDir + "/mod_conf.exe";
                if (Enable)
                {
                    mod_conf.StartInfo.Arguments = "SSSOn " + AppDataDir;
                }
                else
                {
                    mod_conf.StartInfo.Arguments = "SSSOff " + AppDataDir;
                }
                mod_conf.StartInfo.CreateNoWindow = true;
                mod_conf.StartInfo.UseShellExecute = false;
                mod_conf.Start();
                mod_conf.WaitForExit();
                if (mod_conf.ExitCode != 0)
                {
                    if (MessageBox.Show(SwitchSSSError + "\n" + AskBackup, null, MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                    {
                        RestoreSettings();
                    }
                    return false;
                }
                return true;
            }
            return false;
        }

        private bool ReadyToStart()
        {
            if (!string.IsNullOrEmpty(FirstLaunchError))
            {
                MessageBox.Show(FirstLaunchError.TrimEnd('\n'), null, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (!FileFound(ParaworldBinDir + "/Paraworld.exe") || !FileFound(ParaworldBinDir + "/PWClient.exe"))
            {
                return false;
            }
            if (Process.GetProcessesByName("Paraworld").Any() || Process.GetProcessesByName("PWClient").Any() || Process.GetProcessesByName("PWClient2").Any() || Process.GetProcessesByName("PWServer").Any())
            {
                if (MessageBox.Show(PWIsAlreadyRunning, null, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    StartPWKiller(false);
                }
                return false;
            }
            if (PlayMusic == true)
            {
                SwitchMusic();
            }
            if (!EnableSSS(true))
            {
                return false;
            }
            EnabledMods = GetEnabledMods();
            if (EnabledMods == null)
            {
                return false;
            }
            SaveToDB("/enabled_mods", string.Join(",", Mods));
            ClearCache();
            StartPWKiller(true);
            Directory.SetCurrentDirectory(ParaworldBinDir);
            return true;
        }

        private void StartMirage_Click(object sender, RoutedEventArgs e)
        {
            if (ReadyToStart())
            {
                Process.Start(ParaworldBinDir + "/Paraworld.exe", GetCommandLine(EnabledMods));
            }
        }

        private void StartSDK_Click(object sender, RoutedEventArgs e)
        {
            if (ReadyToStart())
            {
                Process.Start(ParaworldBinDir + "/PWClient.exe", " -leveled" + GetCommandLine(EnabledMods));
            }
        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            if (ReadyToStart())
            {
                Process.Start(ParaworldBinDir + "/Paraworld.exe", " -dedicated" + GetCommandLine(EnabledMods));
            }
        }

        private void OpenTavern_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://para-welt.com/tavern/");
        }

        private void SwitchMusic_Click(object sender, RoutedEventArgs e)
        {
            SwitchMusic();
        }

        private void SwitchMusic()
        {
            if (PlayMusic == true)
            {
                PlayMusic = false;
                MediaPlayer.Pause();
                SwitchMusicButton.Content = TurnMuscOn;
            }
            else
            {
                PlayMusic = true;
                MediaPlayer.Play();
                SwitchMusicButton.Content = TurnMuscOff;
            }
        }

        private void ClearCache_Click(object sender, RoutedEventArgs e)
        {
            string AllCacheFileNames = ClearCache();

            if (string.IsNullOrEmpty(AllCacheFileNames))
            {
                MessageBox.Show(NoCacheFound, "ClearPWCache", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(CacheDeleted + "\n" + AllCacheFileNames, "ClearPWCache", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private string ClearCache()
        {
            string[] CacheExts = { "bin", "ubc", "swd" };
            List<string> CacheFileNames = new List<string>();
            if (Directory.Exists(CacheDir))
            {
                IEnumerable<string> AllCacheFiles = Directory.EnumerateFiles(CacheDir, "*.*").Where(file => CacheExts.Any(x => file.EndsWith(x, StringComparison.OrdinalIgnoreCase)));
                foreach (string CacheFile in AllCacheFiles)
                {
                    CacheFileNames.Add(Path.GetFullPath(CacheFile));
                    try
                    {
                        File.Delete(CacheFile);
                    }
                    catch (IOException)
                    {

                    }
                }
                string AllCacheFileNames = string.Join("\n", CacheFileNames);
                return AllCacheFileNames;
            }
            return null;
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(SettingsDir))
            {
                MessageBox.Show("Folder\n" + SettingsDir + "\nnot found", FileNotFound, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Process.Start("explorer", SettingsDir);
        }

        private void KillProcesses_Click(object sender, RoutedEventArgs e)
        {
            StartPWKiller(false);
        }

        private void StartPWKiller(bool Minimized)
        {
            if (FileFound(ToolsDir + "/PWKiller.exe"))
            {
                ProcessStartInfo PWKiller_start = new ProcessStartInfo(ToolsDir + "/PWKiller.exe");
                if (Minimized)
                {
                    Process.Start(ToolsDir + "/PWKiller.exe", "-SSSOffAfterPWExit");
                    PWKiller_start.WindowStyle = ProcessWindowStyle.Minimized;
                }
                Process.Start(PWKiller_start);
            }
        }

        private void SwitchPWTool_Click(object sender, RoutedEventArgs e)
        {
            if (PWTool.Visibility == Visibility.Hidden)
            {
                ModListView.ItemsSource = ModList;
                PWTool.Visibility = Visibility.Visible;
                Menu.Visibility = Visibility.Hidden;
                SwitchPWTool.Content = ClosePWTool;
            }
            else
            {
                PWTool.Visibility = Visibility.Hidden;
                Menu.Visibility = Visibility.Visible;
                SwitchPWTool.Content = OpenPWTool;
            }
        }

        private void Uninstall_Click(object sender, RoutedEventArgs e)
        {
            if (FileFound(ParaworldDir + "/Uninstall MIRAGE.exe"))
            {
                Process.Start(ParaworldDir + "/Uninstall MIRAGE.exe");
                Application.Current.Shutdown();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        //PWTool start

        private void SSSOn_Click(object sender, RoutedEventArgs e)
        {
            EnableSSS(true);
        }

        private void SSSOff_Click(object sender, RoutedEventArgs e)
        {
            EnableSSS(false);
        }

        private void RestoreSettings_Click(object sender, RoutedEventArgs e)
        {
            RestoreSettings();
        }

        private void RestoreSettings()
        {
            FileInfo SettingsBackup = new FileInfo(SettingsBackupPath);
            if (!SettingsBackup.Exists)
            {
                MessageBox.Show(BackupMissing, FileNotFound, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (MessageBox.Show(ResetSettings, Warning, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                SettingsBackup.CopyTo(SettingsPath, true);
                MessageBox.Show(ResetSettingsSuccess, Warning, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CreateSettingsBackup_Click(object sender, RoutedEventArgs e)
        {
            CreateSettingsBackup();
        }

        private void CreateSettingsBackup()
        {
            FileInfo Settings = new FileInfo(SettingsPath);
            FileInfo SettingsBackup = new FileInfo(SettingsBackupPath);
            if (Settings.Exists)
            {
                if (SettingsBackup.Exists)
                    if (MessageBox.Show(OverwriteBackup, Warning, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        Settings.CopyTo(SettingsBackupPath, true);
                        MessageBox.Show(BackupCreated, Warning, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {

                    }
                else
                {
                    Settings.CopyTo(SettingsBackupPath, true);
                    MessageBox.Show(BackupCreated, Warning, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show(SettingsMissing, FileNotFound, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //PWTool end

        private void ShowUpdateWindow()
        {
            UpdateLog.Text = "● " + WhatsNew.Replace(";", "\n● ");
            SocialBG.Visibility = Visibility.Hidden;
            Update.Visibility = Visibility.Visible;
        }

        private void OpenUpdatePage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://para-welt.com/mirage/?version=15");
        }

        private void OpenModdb_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.moddb.com/mods/paraworld-mirage");
        }

        private void OpenDiscord_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://discord.com/invite/Vz6dzx2");
        }

        private void OpenPatreon_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.patreon.com/parawelt");
        }

        private bool FileFound(string Filename)
        {
            if (File.Exists(Filename))
            {
                return true;
            }
            MessageBox.Show(Path.GetFullPath(Filename) + " not found.", FileNotFound, MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }
}
