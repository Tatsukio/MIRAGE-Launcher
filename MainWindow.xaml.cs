using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace MIRAGE_Launcher
{
    public partial class MainWindow : Window
    {
        static MediaPlayer MediaPlayer = new MediaPlayer();

        static bool PlayMusic = true;
        static bool IsFirstLaunch = false;
        static string MirageExeCurrentDir = AppDomain.CurrentDomain.BaseDirectory;
        static string ParaworldBinDir = Path.GetFullPath(MirageExeCurrentDir + "..\\..\\..\\Paraworld\\bin");
        static string AppDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        static string SettingsDir = Path.GetFullPath(AppDataDir + "\\SpieleEntwicklungsKombinat\\Paraworld");
        static string SettingsPath = Path.GetFullPath(SettingsDir + "\\settings.cfg");
        static string SettingsBackupPath = Path.GetFullPath(SettingsDir + "\\Settings_SSSS_backup.cfg");
        static string ToolsDir = Path.GetFullPath(ParaworldBinDir + "\\..\\Tools");
        static string MirageDBPath = Path.GetFullPath(MirageExeCurrentDir + "\\Texts\\mirage_db.xml");

        static string ModName = "MIRAGE";
        static string VersionTmp = "1.0.0"; //Use localization file to change the version
        static string TurnMuscOff = "Turn music off";
        static string TurnMuscOn = "Turn music on";
        static string Warning = "Warning";
        static string FileNotFound = "File not found";
        static string BackupNotFound = "Backup file not found";
        static string OverwriteBackup = "Backup file already exists. Overwrite backup file?";
        static string PWIsAlreadyRunning = "ParaWorld is already running. Start PWKiller?";
        static string LauncherIsAlreadyRunning = "This programm is already running. Please close the running version first!";
        static string BackupCreated = "Created Settings_SSSS_backup.cfg from settings.cfg.This one will be used when settings.cfg becomes corrupt.";
        static string ResetSettingsSuccess = "Replaced settings.cfg with Settings_SSSS_backup.cfg.Some options might have been reset to an old state!";
        static string ResetSettings = "This will reset your settings.cfg file, and some saved data (like last IP addresses) will be lost. Do you really want to continue?";
        static string NoCacheFound = "No cache files found.";
        static string CacheDeleted = "The following cache files have been deleted successfully:";
        static string WhatsNew = "";

        public MainWindow()
        {
            if (Process.GetProcessesByName("MIRAGE Launcher").Count() == 1)
            {
                InitializeComponent();
                LoadLocalization();
                if (IsFirstLaunch == true)
                {
                    OnFirstLaunch();
                }
                LoadUI();
                Task TGetMyPublicIp = new Task(GetMyPublicIp);
                TGetMyPublicIp.Start();
            }
            else
            {
                InitializeComponent();
                LoadLocalization();
                MessageBox.Show(LauncherIsAlreadyRunning, Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                Application.Current.Shutdown();
            }
        }

        private void LoadLocalization()
        {
            if (!File.Exists(MirageDBPath))
            {
                MessageBox.Show("mirage_db.xml not found in\n" + Path.GetFullPath(MirageExeCurrentDir + "\\Texts\\"), FileNotFound, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                XmlDocument Localization = new XmlDocument();
                Localization.Load(MirageDBPath);
                VersionTmp = Localization.SelectSingleNode("/mirage_db/launcher_localization/mirage_version").InnerText;
                Task TVersionCheck = new Task(VersionCheck);
                TVersionCheck.Start();

                IsFirstLaunch = Convert.ToBoolean(Localization.SelectSingleNode("/mirage_db/launcher_misc/is_first_launch").InnerText);

                MainLabel.Content = Localization.SelectSingleNode("/mirage_db/launcher_localization/main_label").InnerText;
                StartMirage.Content = Localization.SelectSingleNode("/mirage_db/launcher_localization/start_mirage").InnerText;
                StartSdk.Content = Localization.SelectSingleNode("/mirage_db/launcher_localization/start_sdk").InnerText;
                StartServer.Content = Localization.SelectSingleNode("/mirage_db/launcher_localization/start_server").InnerText;
                OpenTavern.Content = Localization.SelectSingleNode("/mirage_db/launcher_localization/open_tavern").InnerText;
                TurnMuscOff = Localization.SelectSingleNode("/mirage_db/launcher_localization/turn_music_off").InnerText;
                TurnMuscOn = Localization.SelectSingleNode("/mirage_db/launcher_localization/turn_music_on").InnerText;
                ClearCacheButton.Content = Localization.SelectSingleNode("/mirage_db/launcher_localization/clear_cache").InnerText;
                OpenSettings.Content = Localization.SelectSingleNode("/mirage_db/launcher_localization/open_settings").InnerText;
                KillProcesses.Content = Localization.SelectSingleNode("/mirage_db/launcher_localization/kill_processes").InnerText;
                StartPWtool.Content = Localization.SelectSingleNode("/mirage_db/launcher_localization/start_pwtool").InnerText;
                Uninstall.Content = Localization.SelectSingleNode("/mirage_db/launcher_localization/uninstall").InnerText;
                Exit.Content = Localization.SelectSingleNode("/mirage_db/launcher_localization/exit").InnerText;
                SSSOnButton.Content = Localization.SelectSingleNode("/mirage_db/launcher_localization/sss_on").InnerText;
                SSSOffButton.Content = Localization.SelectSingleNode("/mirage_db/launcher_localization/sss_off").InnerText;
                RestoreSettings.Content = Localization.SelectSingleNode("/mirage_db/launcher_localization/restore_settings").InnerText;
                CreateSettingsBackup.Content = Localization.SelectSingleNode("/mirage_db/launcher_localization/create_settings_backup").InnerText;
                ModNameLabel.Content = Localization.SelectSingleNode("/mirage_db/launcher_localization/mod_name_label").InnerText;
                UpdateLabel.Content = Localization.SelectSingleNode("/mirage_db/launcher_localization/update_label").InnerText;
                SwitchMusicButton.Content = TurnMuscOff;

                PWIsAlreadyRunning = Localization.SelectSingleNode("/mirage_db/launcher_localization/pw_is_already_running").InnerText;
                LauncherIsAlreadyRunning = Localization.SelectSingleNode("/mirage_db/launcher_localization/launcher_is_already_running").InnerText;
                Warning = Localization.SelectSingleNode("/mirage_db/launcher_localization/warning").InnerText;
                FileNotFound = Localization.SelectSingleNode("/mirage_db/launcher_localization/file_not_found").InnerText;
                BackupNotFound = Localization.SelectSingleNode("/mirage_db/launcher_localization/backup_not_found").InnerText;
                ResetSettings = Localization.SelectSingleNode("/mirage_db/launcher_localization/reset_settings").InnerText;
                ResetSettingsSuccess = Localization.SelectSingleNode("/mirage_db/launcher_localization/reset_settings_success").InnerText;
                OverwriteBackup = Localization.SelectSingleNode("/mirage_db/launcher_localization/overwrite_backup").InnerText;
                BackupCreated = Localization.SelectSingleNode("/mirage_db/launcher_localization/backup_created").InnerText;
                NoCacheFound = Localization.SelectSingleNode("/mirage_db/launcher_localization/no_cache_found").InnerText;
                CacheDeleted = Localization.SelectSingleNode("/mirage_db/launcher_localization/cache_deleted").InnerText;
            }
        }

        public void VersionCheck()
        {
            Version MirageVersion = new Version(VersionTmp);
            WebClient VersionPage = new WebClient();
            VersionPage.Proxy = new WebProxy();
            string FullSiteVersion = VersionPage.DownloadString("https://para-welt.com/mirage/version.txt");
            //versioncheck	MIRAGE 2.6.2	14	0
            string[] vars = FullSiteVersion.Split(new char[] { '	' }, StringSplitOptions.RemoveEmptyEntries);
            if (vars[0] == "versioncheck" && vars != null)
            {
                string[] SiteModVersion = vars[1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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
                        WhatsNew = vars[2];
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => ShowUpdateWindow()));
                        break;
                }
            }
        }

        private void LoadUI()
        {
            Random Random = new Random();
            int BGIndex = Random.Next(1, 85);
            string MusicDir = Path.GetFullPath(ParaworldBinDir + "\\..\\Data\\Base\\Audio\\Music\\");
            string BGDir = Path.GetFullPath(ParaworldBinDir + "\\..\\Data\\MIRAGE\\launcher_misc\\backgrounds\\");

            if (!Directory.Exists(BGDir) || !File.Exists(BGDir + "background_" + BGIndex + ".jpg"))
            {
                MessageBox.Show("Folder\n" + BGDir + "\nnot found or empty.", FileNotFound, MessageBoxButton.OK, MessageBoxImage.Error);
                //LauncherBG.Source = new BitmapImage(new Uri(ParaworldBinDir + "\\..\\Data\\Base\\UI\\menue\\decoration\\loadbg_static.jpg"));
                LauncherBG.Source = new BitmapImage(new Uri("pack://application:,,,/Res/mirage.ico"));
                LauncherBG.HorizontalAlignment = HorizontalAlignment.Center;
                LauncherBG.VerticalAlignment = VerticalAlignment.Center;
                LauncherBG.Stretch = Stretch.None;
            }
            else
            {
                LauncherBG.Source = new BitmapImage(new Uri(BGDir + "background_" + BGIndex + ".jpg"));
            }
            switch (BGIndex)
            {
                case 1:
                    MusicDir += "01_maintheme.mp3";
                    break;
                case 2:
                case 41:
                case 43:
                    MusicDir += "13_plain_icewaste.mp3";
                    break;
                case 3:
                case 65:
                case 67:
                    MusicDir += "11_plain_jungle_1.mp3";
                    break;
                case 4:
                case 48:
                    MusicDir += "23_location_arena.mp3";
                    break;
                case 5:
                case 53:
                case 74:
                    MusicDir += "36_combat_aje_1.mp3";
                    break;
                case 6:
                case 82:
                    MusicDir += "43_combat_ninigi_2.mp3";
                    break;
                case 7:
                    MusicDir += "10_plain_heroes.mp3";
                    break;
                case 8:
                case 45:
                case 54:
                case 83:
                    MusicDir += "35_combat_hu_1.mp3";
                    break;
                case 9:
                    MusicDir += "04_plain_northland_1.mp3";
                    break;
                case 10:
                case 60:
                    MusicDir += "41_combat_hu_2.mp3";
                    break;
                case 11:
                    MusicDir += "16_darkzone_3.mp3";
                    break;
                case 12:
                case 56:
                case 75:
                    MusicDir += "42_combat_aje_2.mp3";
                    break;
                case 13:
                case 36:
                case 38:
                    MusicDir += "48_var_combat_aje_1.mp3";
                    break;
                case 14:
                    MusicDir += "15_darkzone_2.mp3";
                    break;
                case 15:
                case 73:
                case 85:
                    MusicDir += "17_maintheme_hu.mp3";
                    break;
                case 16:
                case 50:
                case 81:
                    MusicDir += "47_var_combat_hu_2.mp3";
                    break;
                case 17:
                case 61:
                case 71:
                    MusicDir += "50_var_combat_seas_1.mp3";
                    break;
                case 18:
                case 66:
                case 78:
                    MusicDir += "12_plain_jungle_2.mp3";
                    break;
                case 19:
                case 79:
                    MusicDir += "51_location_seas_temple.mp3";
                    break;
                case 20:
                    MusicDir += "30_location_scientist_hut.mp3";
                    break;
                case 21:
                case 35:
                case 55:
                case 57:
                    MusicDir += "39_combat_dinos.mp3";
                    break;
                case 22:
                case 27:
                case 80:
                    MusicDir += "28_location_walhalla.mp3";
                    break;
                case 23:
                case 24:
                    MusicDir += "32_location_holycity.mp3";
                    break;
                case 25:
                    MusicDir += "26_location_water_temple.mp3";
                    break;
                case 26:
                case 29:
                    MusicDir += "27_location_entry_to_walhalla.mp3";
                    break;
                case 28:
                    MusicDir += "29_location_aeroplane.mp3";
                    break;
                case 30:
                    MusicDir += "25_location_pirates.mp3";
                    break;
                case 31:
                    MusicDir += "20_location_druids.mp3";
                    break;
                case 32:
                    MusicDir += "06_plain_savannah_1.mp3";
                    break;
                case 33:
                    MusicDir += "21_location_amazons.mp3";
                    break;
                case 34:
                case 47:
                    MusicDir += "18_maintheme_aje.mp3";
                    break;
                case 64:
                    MusicDir += "45_var_plain_savannah_1.mp3";
                    break;
                case 37:
                    MusicDir += "05_plain_northland_2.mp3";
                    break;
                case 52:
                case 76:
                    MusicDir += "07_plain_savannah_2.mp3";
                    break;
                case 39:
                    MusicDir += "40_combat_heroes.mp3";
                    break;
                case 40:
                    MusicDir += "08_plain_savannah_3.mp3";
                    break;
                case 42:
                    MusicDir += "14_darkzone_1.mp3";
                    break;
                case 44:
                    MusicDir += "34_location_prison_island.mp3";
                    break;
                case 46:
                    MusicDir += "49_var_combat_ninigi_1.mp3";
                    break;
                case 49:
                    MusicDir += "09_plain_savannah_4.mp3";
                    break;
                case 51:
                    MusicDir += "37_combat_ninigi_1.mp3";
                    break;
                case 58:
                case 70:
                case 72:
                    MusicDir += "38_combat_seas_1.mp3";
                    break;
                case 59:
                    MusicDir += "19_maintheme_ninigi.mp3";
                    break;
                case 62:
                case 63:
                    MusicDir += "44_var_plain_northland_1.mp3";
                    break;
                case 68:
                    MusicDir += "33_location_holycity_walls.mp3";
                    break;
                case 69:
                    MusicDir += "24_location_temple.mp3";
                    break;
                case 77:
                    MusicDir += "46_var_plain_heroes.mp3";
                    break;
                case 84:
                    MusicDir += "22_location_the_gate.mp3";
                    break;
            }
            if (!File.Exists(MusicDir))
            {
                MessageBox.Show(MusicDir + "\nnot found.", FileNotFound, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MediaPlayer.MediaEnded += (sender, eventArgs) => LoadUI();
                MediaPlayer.Open(new Uri(MusicDir, UriKind.Relative));
                MediaPlayer.Play();
            }
        }

        private void GetMyPublicIp()
        {
            WebClient web = new WebClient();
            string MyIP = web.DownloadString(new Uri("https://ipinfo.io/ip")).Trim();
            string IPPath = AppDataDir + "\\..\\Local\\Temp\\paraworld_ip.txt";
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

        private void OnFirstLaunch()
        {
            //Check for PW fonts
            string FontsDir = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            if (!File.Exists($"{FontsDir}\\trebuc.ttf") || !File.Exists($"{FontsDir}\\trebucbd.ttf") || !File.Exists($"{FontsDir}\\trebucbi.ttf") || !File.Exists($"{FontsDir}\\trebucit.ttf"))
            {
                MessageBox.Show("Trebuchet MS fonts not found in\n" + FontsDir, FileNotFound, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            //End of PW fonts check

            //Check for BP
            if (Directory.Exists(ParaworldBinDir))
            {
                if (!Directory.Exists(ParaworldBinDir + "\\..\\Data\\BoosterPack1"))
                {
                    MessageBox.Show("BoosterPack is not installed, please install it first.\nYou can download it from Para-Welt.com or ParaWorld ModDB.", null, MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                }
            }
            //End of BP check

            //Check for Win7Fix
            if (File.Exists(ParaworldBinDir + "\\123\\bin\\Paraworld.exe"))
            {
                using (MD5 MD5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(ParaworldBinDir + "\\123\\bin\\Paraworld.exe"))
                    {
                        byte[] Win7FixExeMD5 = { 08, 41, 82, 123, 147, 26, 93, 185, 136, 237, 71, 119, 102, 252, 145, 01 };
                        byte[] ExeMD5 = MD5.ComputeHash(stream);
                        if (Win7FixExeMD5.SequenceEqual(ExeMD5) == false)
                        {
                            MessageBox.Show("Install Win7Fix", Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            else
            {

            }
            //End of Win7Fix check

            //Check for settings.cfg
            if (!Directory.Exists(SettingsDir) || !File.Exists(SettingsPath))
            {
                MessageBox.Show("If you have never run ParaWorld on this system before, you must run it first to create the necessary files", null, MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            //End of settings.cfg check
        }

        private void SSSOn()
        {
            if (!File.Exists(ToolsDir + "\\mod_conf.exe"))
            {
                MessageBox.Show("Failed to switch server side scripts.\nmod_conf.exe not found in\n" + ToolsDir, FileNotFound, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ProcessStartInfo mod_conf_start = new ProcessStartInfo(ToolsDir + "\\mod_conf.exe", "SSSOn " + AppDataDir);
            mod_conf_start.CreateNoWindow = true;
            mod_conf_start.UseShellExecute = false;
            Process.Start(mod_conf_start);
        }

        private void SSSOff()
        {
            if (!File.Exists(ToolsDir + "\\mod_conf.exe"))
            {
                MessageBox.Show("Failed to switch server side scripts.\nmod_conf.exe not found in\n" + ToolsDir, FileNotFound, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ProcessStartInfo mod_conf_start = new ProcessStartInfo(ToolsDir + "\\mod_conf.exe", "SSSOff " + AppDataDir);
            mod_conf_start.CreateNoWindow = true;
            mod_conf_start.UseShellExecute = false;
            Process.Start(mod_conf_start);
        }

        private void DragMove(object sender, MouseButtonEventArgs e)
        {
            DragMove();
            if (e.ClickCount == 2) WindowState = WindowState.Minimized;
        }

        private bool PWRunning()
        {
            if (Process.GetProcessesByName("Paraworld").Any() || Process.GetProcessesByName("PWClient").Any() || Process.GetProcessesByName("PWServer").Any())
            {
                if (MessageBox.Show(PWIsAlreadyRunning, Warning, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    StartPWKiller();
                }
                return true;
            }
            return false;
        }

        private void StartMirage_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(ParaworldBinDir + "\\Paraworld.exe"))
            {
                MessageBox.Show("Paraworld.exe not found in\n" + ParaworldBinDir, FileNotFound, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!PWRunning())
            {
                if (PlayMusic == true) { SwitchMusic(); }
                SSSOn();
                ClearCache();
                StartPWKiller();
                Process.Start(ParaworldBinDir + "\\Paraworld.exe", "-enable boosterpack1 -enable " + ModName);
            }
        }

        private void StartSDK_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(ParaworldBinDir + "\\Paraworld.exe") || !File.Exists(ParaworldBinDir + "\\PWClient.exe"))
            {
                MessageBox.Show("Paraworld.exe or PWClient.exe not found in\n" + ParaworldBinDir, FileNotFound, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!PWRunning())
            {
                if (PlayMusic == true) { SwitchMusic(); }
                SSSOn();
                ClearCache();
                StartPWKiller();
                Process.Start(ParaworldBinDir + "\\PWClient.exe", "-leveled -enable boosterpack1 -enable " + ModName);
                Process.Start(ParaworldBinDir + "\\Paraworld.exe", "-enable boosterpack1 -enable " + ModName);
            }
        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(ParaworldBinDir + "\\Paraworld.exe"))
            {
                MessageBox.Show("Paraworld.exe not found in\n" + ParaworldBinDir, FileNotFound, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!PWRunning())
            {
                if (PlayMusic == true) { SwitchMusic(); }
                SSSOn();
                ClearCache();
                StartPWKiller();
                Process.Start(ParaworldBinDir + "\\Paraworld.exe", "-enable boosterpack1 -dedicated");
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
            string CacheDir = AppDataDir + "\\..\\Local\\Temp\\SpieleEntwicklungsKombinat\\Paraworld";
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
            StartPWKiller();
        }

        private void StartPWKiller()
        {
            Process[] AllPWKiller = Process.GetProcessesByName("PWKiller");
            foreach (Process PWKiller in AllPWKiller) { PWKiller.Kill(); }
            if (!File.Exists(ToolsDir + "\\PWKiller.exe"))
            {
                MessageBox.Show("PWKiller.exe not found in\n" + ToolsDir, FileNotFound, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Process.Start(ToolsDir + "\\PWKiller.exe");
        }

        private void StartPWTool_Click(object sender, RoutedEventArgs e)
        {
            if (PWTool.Visibility == Visibility.Hidden)
            {
                PWTool.Visibility = Visibility.Visible;
            }
            else
            {
                PWTool.Visibility = Visibility.Hidden;
            }
        }

        private void Uninstall_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(ParaworldBinDir + "\\..\\Uninstall MIRAGE.exe"))
            {
                MessageBox.Show("Uninstall MIRAGE.exe not found in " + Path.GetFullPath(ParaworldBinDir + "\\..\\"), FileNotFound, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Process.Start(ParaworldBinDir + "\\..\\Uninstall MIRAGE.exe");
            Application.Current.Shutdown();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        //PWTool start
        private void ModNameTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ModName = ModNameTextbox.Text;
        }

        private void SSSOn_Click(object sender, RoutedEventArgs e)
        {
            SSSOn();
        }

        private void SSSOff_Click(object sender, RoutedEventArgs e)
        {
            SSSOff();
        }

        private void RestoreSettings_Click(object sender, RoutedEventArgs e)
        {
            FileInfo SettingsBackup = new FileInfo(SettingsBackupPath);
            if (!SettingsBackup.Exists)
            {
                MessageBox.Show(BackupNotFound, FileNotFound, MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("settings.cfg not found in\n" + SettingsDir, FileNotFound, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //PWTool end

        private void ShowUpdateWindow()
        {
            UpdateLog.Text = WhatsNew.Replace(" ●", "\n●");
            SocialBG.Visibility = Visibility.Hidden;
            Update.Visibility = Visibility.Visible;
        }

        private void OpenUpdatePage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://para-welt.com/mirage/?version=14");
        }

        private void OpenModdb_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.moddb.com/mods/paraworld-mirage");
        }

        private void OpenDiscord_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://discordapp.com/invite/vKVxUfT");
        }

        private void OpenPatreon_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.patreon.com/");
        }
    }
}
