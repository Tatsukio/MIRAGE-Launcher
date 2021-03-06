﻿using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using MIRAGE_Launcher.Models;
using MIRAGE_Launcher.ViewModels;

namespace MIRAGE_Launcher.ViewModel
{
    public class CLauncherViewModel : CViewModelBase
    {
        static bool _firstLaunch = false;
        static string _enabledMods = "";

        static readonly MediaPlayer _mediaPlayer = new MediaPlayer();
        static readonly XmlDocument _launcherDB = new XmlDocument();

        static string _launcherExeDir = AppDomain.CurrentDomain.BaseDirectory;
        static string _paraworldDir = _launcherExeDir + "../../";
        static string _paraworldBinDir = _paraworldDir + "/bin";
        static string _toolsDir = _paraworldDir + "/Tools";
        static string _launcherDBPath = _toolsDir + "/Launcher/LauncherDB.xml";
        static string _infoDir = _paraworldDir + "/Data/Info/";
        static string[] _pwProcesses = { "Paraworld", "PWClient", "PWServer" };

        static readonly string _cacheDir = Path.GetFullPath(Path.GetTempPath() + "/SpieleEntwicklungsKombinat/Paraworld");
        static readonly string _appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        static readonly string _settingsDir = Path.GetFullPath(_appDataDir + "/SpieleEntwicklungsKombinat/Paraworld");
        static readonly string _settingsPath = Path.GetFullPath(_settingsDir + "/settings.cfg");
        static readonly string _settingsBackupPath = Path.GetFullPath(_settingsDir + "/Settings_SSSS_backup.cfg");

        public CLauncherViewModel()
        {
            #region Commands

            StartParaworldCmd = new CCommand(OnStartParaworldCmd, StartParaworldCmdEnabled);
            StartSDKCmd = new CCommand(OnStartSDKCmd, StartSDKCmdEnabled);
            StartServerCmd = new CCommand(OnStartServerCmd, StartServerCmdEnabled);
            OpenTavernCmd = new CCommand(OnOpenTavernCmd, OpenTavernCmdEnabled);
            ToggleMusicCmd = new CCommand(OnToggleMusicCmd, ToggleMusicCmdEnabled);
            PlayNextTrackCmd = new CCommand(OnPlayNextTrackCmd, PlayNextTrackCmdEnabled);
            ClearCacheCmd = new CCommand(OnClearCacheCmd, ClearCacheCmdEnabled);
            OpenSettingsFolderCmd = new CCommand(OnOpenSettingsFolderCmd, OpenSettingsFolderCmdEnabled);
            StartPWKillerCmd = new CCommand(OnStartPWKillerCmd, StartPWKillerCmdEnabled);
            TogglePWToolCmd = new CCommand(OnTogglePWToolCmd, TogglePWToolCmdEnabled);

            SSSOnCmd = new CCommand(OnSSSOnCmd, SSSOnCmdEnabled);
            SSSOffCmd = new CCommand(OnSSSOffCmd, SSSOffCmdEnabled);
            RestoreSettingsCmd = new CCommand(OnRestoreSettingsCmd, RestoreSettingsCmdEnabled);
            CreateSettingsBackupCmd = new CCommand(OnCreateSettingsBackupCmd, CreateSettingsBackupCmdEnabled);

            UninstallCmd = new CCommand(OnUninstallCmd, UninstallCmdEnabled);
            ExitCmd = new CCommand(OnExitCmd, ExitCmdEnabled);

            OpenUpdatePageCmd = new CCommand(OnOpenUpdatePageCmd, OpenUpdatePageCmdEnabled);
            OpenModdbCmd = new CCommand(OnOpenModdbCmd, OpenModdbCmdEnabled);
            OpenDiscordCmd = new CCommand(OnOpenDiscordCmd, OpenDiscordCmdEnabled);
            OpenPatreonCmd = new CCommand(OnOpenPatreonCmd, OpenPatreonCmdEnabled);

            #endregion

            Task taskGetMyPublicIp = new Task(GetMyPublicIp);

            //GameRanger support
            if (Process.GetCurrentProcess().ProcessName == _pwProcesses[0])
            {
                _paraworldBinDir = AppDomain.CurrentDomain.BaseDirectory;
                _paraworldDir = _paraworldBinDir + "../";
                _toolsDir = _paraworldDir + "/Tools";
                _launcherDBPath = _toolsDir + "/Launcher/LauncherDB.xml";
                _infoDir = _paraworldDir + "/Data/Info/";

                _pwProcesses[0] = "Paraworld2";

                Process[] AllLauncher = Process.GetProcessesByName("MIRAGE Launcher");
                foreach (Process Launcher in AllLauncher)
                {
                    Launcher.Kill();
                }

                /*
                var process = Process.Start($"{_toolsDir}/Launcher/MIRAGE Launcher.exe");
                process.WaitForExit();

                _launcherDBPath = _toolsDir + "/Launcher/LauncherDB.xml";
                _infoDir = _paraworldDir + "/Data/Info/";

                _pwProcesses[0] = "Paraworld2";
                taskGetMyPublicIp.Start();
                LoadDB();
                GetMods();

                if (ReadyToStart())
                {
                    Process.Start($"{AppDomain.CurrentDomain.BaseDirectory}/Paraworld2.exe", GetEnabledMods());
                }
                Application.Current.Shutdown();
                return;
                */
            }

            if (Process.GetProcessesByName("MIRAGE Launcher").Count() > 1)
            {
                MessageBox.Show(_launcherIsAlreadyRunning, _warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                Application.Current.Shutdown();
                return;
            }

            //Discord RichPresence support
            if (File.Exists(_paraworldBinDir + "/PWClient2.exe"))
            {
                _pwProcesses[1] = "PWClient2";
            }

            taskGetMyPublicIp.Start();
            LoadDB();
            LoadUI();
            GetMods();
            ToggleMusic();
        }

        private BitmapImage _launcherBackground;
        public BitmapImage LauncherBackground
        {
            get => _launcherBackground;
            set => Set(ref _launcherBackground, value);
        }

        private bool _showUpdateWindow;
        public bool ShowUpdateWindow
        {
            get => _showUpdateWindow;
            set => Set(ref _showUpdateWindow, value);
        }

        private string _updateLogText;
        public string UpdateLogText
        {
            get => _updateLogText;
            set => Set(ref _updateLogText, value);
        }

        #region Localization

        #region MenuButtons

        private string _updateTitleText = "New update available!";
        public string UpdateTitleText
        {
            get => _updateTitleText;
            set => Set(ref _updateTitleText, value);
        }

        private string _menuTitleText = "Launcher Menu";
        public string MenuTitleText
        {
            get => _menuTitleText;
            set => Set(ref _menuTitleText, value);
        }

        private string _startModText = "Start ParaWorld";
        public string StartModText
        {
            get => _startModText;
            set => Set(ref _startModText, value);
        }

        private string _startSDKText = "Start Mapeditor";
        public string StartSDKText
        {
            get => _startSDKText;
            set => Set(ref _startSDKText, value);
        }

        private string _startServerText = "Start Dedicated Server";
        public string StartServerText
        {
            get => _startServerText;
            set => Set(ref _startServerText, value);
        }

        private string _openTavernText = "Open Tavern";
        public string OpenTavernText
        {
            get => _openTavernText;
            set => Set(ref _openTavernText, value);
        }

        private string _toggleMusicText = "Turn Music Off";
        private string _turnMusicOnText = "Turn Music On";
        private string _turnMusicOffText = "Turn Music Off";
        public string ToggleMusicText
        {
            get => _toggleMusicText;
            set => Set(ref _toggleMusicText, value);
        }

        private string _clearCacheText = "Clear Cache";
        public string ClearCacheText
        {
            get => _clearCacheText;
            set => Set(ref _clearCacheText, value);
        }

        private string _openSettingsFolderText = "Open Settings Folder";
        public string OpenSettingsFolderText
        {
            get => _openSettingsFolderText;
            set => Set(ref _openSettingsFolderText, value);
        }

        private string _startPWKillerText = "Kill PW Processes";
        public string StartPWKillerText
        {
            get => _startPWKillerText;
            set => Set(ref _startPWKillerText, value);
        }

        private string _togglePWToolText = "Open PWTool";
        private string _openPWToolText = "Open PWTool";
        private string _closePWToolText = "Close PWTool";
        public string TogglePWToolText
        {
            get => _togglePWToolText;
            set => Set(ref _togglePWToolText, value);
        }

        private string _sssOnText = "Server Scripts On";
        public string SSSOnText
        {
            get => _sssOnText;
            set => Set(ref _sssOnText, value);
        }

        private string _sssOffText = "Server Scripts Off";
        public string SSSOffText
        {
            get => _sssOffText;
            set => Set(ref _sssOffText, value);
        }

        private string _restoreSettingsText = "Restore Game Settings";
        public string RestoreSettingsText
        {
            get => _restoreSettingsText;
            set => Set(ref _restoreSettingsText, value);
        }

        private string _createSettingsBackupText = "Create Settings Backup";
        public string CreateSettingsBackupText
        {
            get => _createSettingsBackupText;
            set => Set(ref _createSettingsBackupText, value);
        }

        private string _uninstallText = "Uninstall MIRAGE";
        public string UninstallText
        {
            get => _uninstallText;
            set => Set(ref _uninstallText, value);
        }

        private string _exitText = "Exit Launcher";
        public string ExitText
        {
            get => _exitText;
            set => Set(ref _exitText, value);
        }

        #endregion

        static string _warning = "Warning";
        static string _backupMissing = "Backup file not found";
        static string _overwriteBackup = "Backup file already exists. Overwrite backup file?";
        static string _pwIsAlreadyRunning = "ParaWorld is already running. Start PWKiller?";
        static string _launcherIsAlreadyRunning = "This programm is already running. Please close the running version first!";
        static string _backupCreated = "Created Settings_SSSS_backup.cfg from settings.cfg. This one will be used when settings.cfg becomes corrupt.";
        static string _resetSettingsSuccess = "Replaced Settings.cfg with Settings_SSSS_backup.cfg. Some options might have been reset to an old state!";
        static string _resetSettings = "This will reset your Settings.cfg file, and some saved data (like last IP addresses) will be lost. Do you really want to continue?";
        static string _noCacheFound = "No cache files found.";
        static string _cacheDeleted = "ParaWorld cache was successfully cleared.";
        static string _pwFontsMissing = "Trebuchet MS fonts not found.";
        static string _bpMissing = "BoosterPack is not installed, please install it first. You can download it from Para-Welt.com or ParaWorld ModDB.";
        static string _settingsMissing = "Settings.cfg not found. If you have never run ParaWorld on this system before, you must run it first to create the necessary files.";
        static string _switchSSSError = "Failed to switch server side scripts.";
        static string _askSettingsBackup = "Do you want to try to use the backup file?";

        #endregion

        #region Commands

        public ICommand StartParaworldCmd { get; }
        private bool StartParaworldCmdEnabled(object p) => true;
        private void OnStartParaworldCmd(object p)
        {
            if (ReadyToStart())
            {
                Process.Start($"{_paraworldBinDir}/{_pwProcesses[0]}.exe", GetEnabledMods());
            }
        }
        public ICommand StartSDKCmd { get; }
        private bool StartSDKCmdEnabled(object p) => true;
        private void OnStartSDKCmd(object p)
        {
            if (ReadyToStart())
            {
                Process.Start($"{_paraworldBinDir}/{_pwProcesses[1]}.exe", " -leveled" + GetEnabledMods());
            }
        }
        public ICommand StartServerCmd { get; }
        private bool StartServerCmdEnabled(object p) => true;
        private void OnStartServerCmd(object p)
        {
            if (ReadyToStart())
            {
                Process.Start($"{_paraworldBinDir}/{_pwProcesses[0]}.exe", " -dedicated" + GetEnabledMods());
            }
        }
        public ICommand OpenTavernCmd { get; }
        private bool OpenTavernCmdEnabled(object p) => true;
        private void OnOpenTavernCmd(object p)
        {
            Process.Start("https://para-welt.com/tavern/");
        }
        public ICommand ToggleMusicCmd { get; }

        private bool _musicPlaying;
        public bool MusicPlaying
        {
            get => _musicPlaying;
            set => Set(ref _musicPlaying, value);
        }
        private bool ToggleMusicCmdEnabled(object p) => true;
        private void OnToggleMusicCmd(object p)
        {
            ToggleMusic();
        }
        private void ToggleMusic()
        {
            if (MusicPlaying)
            {
                ToggleMusicText = _turnMusicOnText;
                _mediaPlayer.Pause();
                MusicPlaying = false;
            }
            else
            {
                ToggleMusicText = _turnMusicOffText;
                _mediaPlayer.Play();
                MusicPlaying = true;
            }
        }

        public ICommand PlayNextTrackCmd { get; }
        private bool PlayNextTrackCmdEnabled(object p) => true;
        private void OnPlayNextTrackCmd(object p)
        {
            ToggleMusic();
            LoadUI();
            ToggleMusic();
        }

        public ICommand ClearCacheCmd { get; }
        private bool ClearCacheCmdEnabled(object p) => true;
        private void OnClearCacheCmd(object p)
        {
            if(ClearCache())
            {
                MessageBox.Show(_cacheDeleted, "ClearPWCache", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(_noCacheFound, "ClearPWCache", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public ICommand OpenSettingsFolderCmd { get; }
        private bool OpenSettingsFolderCmdEnabled(object p) => true;
        private void OnOpenSettingsFolderCmd(object p)
        {
            if (!Directory.Exists(_settingsDir))
            {
                MessageBox.Show("Folder\n" + _settingsDir + "\nnot found", null, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Process.Start("explorer", _settingsDir);
        }
        public ICommand StartPWKillerCmd { get; }
        private bool StartPWKillerCmdEnabled(object p) => true;
        private void OnStartPWKillerCmd(object p)
        {
            StartPWKiller(false);
        }
        public ICommand TogglePWToolCmd { get; }

        private bool _pwToolIsOpen;
        public bool PWToolIsOpen
        {
            get => _pwToolIsOpen;
            set
            {
                if (value)
                {
                    TogglePWToolText = _closePWToolText;
                    Set(ref _pwToolIsOpen, value);
                }
                else
                {
                    TogglePWToolText = _openPWToolText;
                    Set(ref _pwToolIsOpen, value);
                }
            }
        }
        private bool TogglePWToolCmdEnabled(object p) => true;
        private void OnTogglePWToolCmd(object p)
        {
            TogglePWTool();
        }
        private void TogglePWTool()
        {
            if (PWToolIsOpen)
            {
                PWToolIsOpen = false;
            }
            else
            {
                PWToolIsOpen = true;
            }
        }

        public ICommand SSSOnCmd { get; }
        private bool SSSOnCmdEnabled(object p) => true;
        private void OnSSSOnCmd(object p)
        {
            FileInfo settings = new FileInfo(_settingsPath);
            FileInfo settingsBackup = new FileInfo(_settingsBackupPath);
            if (!settingsBackup.Exists)
            {
                settings.CopyTo(_settingsBackupPath, true);
            }
            EnableSSS(true);
        }

        public ICommand SSSOffCmd { get; }
        private bool SSSOffCmdEnabled(object p) => true;
        private void OnSSSOffCmd(object p)
        {
            EnableSSS(false);
        }

        public ICommand RestoreSettingsCmd { get; }
        private bool RestoreSettingsCmdEnabled(object p) => true;
        private void OnRestoreSettingsCmd(object p)
        {
            RestoreSettings();
        }
        private void RestoreSettings()
        {
            FileInfo settingsBackup = new FileInfo(_settingsBackupPath);
            if (!settingsBackup.Exists)
            {
                MessageBox.Show(_backupMissing, null, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (MessageBox.Show(_resetSettings, _warning, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                settingsBackup.CopyTo(_settingsPath, true);
                MessageBox.Show(_resetSettingsSuccess, _warning, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public ICommand CreateSettingsBackupCmd { get; }
        private bool CreateSettingsBackupCmdEnabled(object p) => true;
        private void OnCreateSettingsBackupCmd(object p)
        {
            FileInfo settings = new FileInfo(_settingsPath);
            FileInfo settingsBackup = new FileInfo(_settingsBackupPath);
            if (settings.Exists)
            {
                if (settingsBackup.Exists)
                    if (MessageBox.Show(_overwriteBackup, _warning, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        settings.CopyTo(_settingsBackupPath, true);
                        MessageBox.Show(_backupCreated, _warning, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {

                    }
                else
                {
                    settings.CopyTo(_settingsBackupPath, true);
                    MessageBox.Show(_backupCreated, _warning, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show(_settingsMissing, null, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public ICommand UninstallCmd { get; }
        private bool UninstallCmdEnabled(object p) => true;
        private void OnUninstallCmd(object p)
        {
            if (FileFound(_paraworldDir + "/Uninstall MIRAGE.exe"))
            {
                Process.Start(_paraworldDir + "/Uninstall MIRAGE.exe");
                Application.Current.Shutdown();
            }
        }

        public ICommand ExitCmd { get; }
        private bool ExitCmdEnabled(object p) => true;
        private void OnExitCmd(object p)
        {
            Application.Current.Shutdown();
        }

        #region Social
        public ICommand OpenUpdatePageCmd { get; }
        private bool OpenUpdatePageCmdEnabled(object p) => true;
        private void OnOpenUpdatePageCmd(object p)
        {
            Process.Start("https://para-welt.com/mirage/?version=15");
        }

        public ICommand OpenModdbCmd { get; }
        private bool OpenModdbCmdEnabled(object p) => true;
        private void OnOpenModdbCmd(object p)
        {
            Process.Start("https://www.moddb.com/mods/paraworld-mirage");
        }

        public ICommand OpenDiscordCmd { get; }
        private bool OpenDiscordCmdEnabled(object p) => true;
        private void OnOpenDiscordCmd(object p)
        {
            Process.Start("https://discord.com/invite/Vz6dzx2");
        }

        public ICommand OpenPatreonCmd { get; }
        private bool OpenPatreonCmdEnabled(object p) => true;
        private void OnOpenPatreonCmd(object p)
        {
            Process.Start("https://www.patreon.com/parawelt");
        }

        #endregion

        #endregion

        public bool ReadyToStart()
        {
            if (_firstLaunch)
            {
                string firstLaunchError = OnFirstLaunch();
                if (string.IsNullOrEmpty(firstLaunchError))
                {
                    SaveEnabledModsToDB("IsFirstLaunch", false.ToString());
                }
                else
                {
                    MessageBox.Show(firstLaunchError.TrimEnd('\n'), null, MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }

            foreach (string pwProcess in _pwProcesses)
            {
                if (!FileFound($"{_paraworldBinDir}/{pwProcess}.exe"))
                {
                    return false;
                }
            }


            foreach (string pwProcess in _pwProcesses)
            {
                if (Process.GetProcessesByName(pwProcess).Any())
                {
                    if (MessageBox.Show(_pwIsAlreadyRunning, null, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        StartPWKiller(false);
                    }
                    return false;
                }
            }

            if (!EnableSSS(true))
            {
                return false;
            }

            _enabledMods = GetEnabledMods();
            if (_enabledMods == null)
            {
                return false;
            }
            SaveEnabledModsToDB("EnabledMods", string.Join(",", ModList));

            ClearCache();

            if (MusicPlaying)
            {
                ToggleMusic();
            }

            StartPWKiller(true);

            Directory.SetCurrentDirectory(_paraworldBinDir);

            return true;
        }

        private bool ClearCache()
        {
            string[] cacheExts = { "bin", "ubc", "swd" };
            if (Directory.Exists(_cacheDir))
            {
                IEnumerable<string> CacheFiles = Directory.EnumerateFiles(_cacheDir, "*.*").Where(file => cacheExts.Any(x => file.EndsWith(x, StringComparison.OrdinalIgnoreCase)));
                if (CacheFiles.Any())
                {
                    foreach (string cacheFile in CacheFiles)
                    {
                        try
                        {
                            File.Delete(cacheFile);
                        }
                        catch (IOException)
                        {

                        }
                    }
                    return true;
                }
            }
            return false;
        }

        private bool FileFound(string filename)
        {
            if (File.Exists(filename))
            {
                return true;
            }
            MessageBox.Show(Path.GetFullPath(filename) + " not found.", null, MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        private void StartPWKiller(bool minimized)
        {
            if (FileFound(_toolsDir + "/PWKiller.exe"))
            {
                ProcessStartInfo pwKiller = new ProcessStartInfo(_toolsDir + "/PWKiller.exe");
                Process.Start(pwKiller);
                if (minimized)
                {
                    pwKiller.Arguments = "-SSSOffAfterPWExit";
                    pwKiller.WindowStyle = ProcessWindowStyle.Minimized;
                    Process.Start(pwKiller);
                }
            }
        }

        private bool EnableSSS(bool enable)
        {
            if (FileFound(_toolsDir + "/mod_conf.exe"))
            {
                Process modConf = new Process();
                modConf.StartInfo.FileName = _toolsDir + "/mod_conf.exe";
                if (enable)
                {
                    modConf.StartInfo.Arguments = "SSSOn " + _appDataDir;
                }
                else
                {
                    modConf.StartInfo.Arguments = "SSSOff " + _appDataDir;
                }
                modConf.StartInfo.CreateNoWindow = true;
                modConf.StartInfo.UseShellExecute = false;
                modConf.Start();
                modConf.WaitForExit();
                if (modConf.ExitCode != 0)
                {
                    if (MessageBox.Show(_switchSSSError + "\n" + _askSettingsBackup, null, MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                    {
                        RestoreSettings();
                    }
                    return false;
                }
                return true;
            }
            return false;
        }

        private void LoadUI()
        {
            Random random = new Random();
            int backgroundIndex = random.Next(1, 85);
            string musicDir = Path.GetFullPath(_paraworldDir + "/Data/Base/Audio/Music/");
            string backgroundDir = Path.GetFullPath(_toolsDir + "/Launcher/Backgrounds/");

            if (!Directory.Exists(backgroundDir) || !File.Exists(backgroundDir + "background_" + backgroundIndex + ".jpg"))
            {
                MessageBox.Show("Folder " + backgroundDir + " not found or empty.", null, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                LauncherBackground = new BitmapImage(new Uri(backgroundDir + "background_" + backgroundIndex + ".jpg"));
            }

            musicDir += GetMusicName(backgroundIndex) + ".mp3";

            if (!File.Exists(musicDir))
            {
                MessageBox.Show(musicDir + " not found.", null, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                _mediaPlayer.MediaEnded += (sender, eventArgs) => LoadUI();
                _mediaPlayer.Open(new Uri(musicDir, UriKind.Relative));
                _mediaPlayer.Play();
            }
        }

        private void LoadDB()
        {
            if (FileFound(_launcherDBPath))
            {
                _launcherDB.Load(_launcherDBPath);
                string ModVersion = Translate("ModVersion");
                string LatestVersionURL = _launcherDB.SelectSingleNode("/LauncherDB/LauncherMisc/VersionURL").InnerText;
                Task taskVersionCheck = new Task(() => VersionCheck(ModVersion, LatestVersionURL));
                taskVersionCheck.Start();

                UpdateTitleText = Translate("UpdateTitleText");
                MenuTitleText = Translate("MenuTitleText");
                StartModText = Translate("StartModText");
                StartSDKText = Translate("StartSDKText");
                StartServerText = Translate("StartServerText");
                OpenTavernText = Translate("OpenTavernText");
                _turnMusicOnText = Translate("TurnMusicOnText");
                _turnMusicOffText = Translate("TurnMusicOffText");
                ClearCacheText = Translate("ClearCacheText");
                OpenSettingsFolderText = Translate("OpenSettingsFolderText");
                StartPWKillerText = Translate("StartPWKillerText");
                _openPWToolText = Translate("OpenPWToolText");
                _closePWToolText = Translate("ClosePWToolText");
                SSSOnText = Translate("SSSOnText");
                SSSOffText = Translate("SSSOffText");
                RestoreSettingsText = Translate("RestoreSettingsText");
                CreateSettingsBackupText = Translate("CreateSettingsBackupText");
                UninstallText = Translate("UninstallText");
                ExitText = Translate("ExitText");
                ToggleMusicText = _turnMusicOnText;
                TogglePWToolText = _openPWToolText;

                _warning = Translate("Warning");

                string errorCode = "Code#"; //Line num in mirage_db.xml

                _pwIsAlreadyRunning = errorCode + "27\n" + Translate("PWIsAlreadyRunning");
                _launcherIsAlreadyRunning = errorCode + "28\n" + Translate("LauncherIsAlreadyRunning");
                _backupMissing = errorCode + "29\n" + Translate("BackupMissing");
                _resetSettings = errorCode + "30\n" + Translate("ResetSettings");
                _resetSettingsSuccess = errorCode + "31\n" + Translate("ResetSettingsSuccess");
                _overwriteBackup = errorCode + "32\n" + Translate("OverwriteBackup");
                _backupCreated = errorCode + "33\n" + Translate("BackupCreated");
                _noCacheFound = errorCode + "34\n" + Translate("NoCacheFound");
                _cacheDeleted = errorCode + "35\n" + Translate("CacheDeleted");
                _bpMissing = errorCode + "36\n" + Translate("BPMissing");
                _settingsMissing = errorCode + "37\n" + Translate("SettingsMissing");
                _switchSSSError = errorCode + "38\n" + Translate("SwitchSSSError");
                _askSettingsBackup = errorCode + "39\n" + Translate("AskSettingsBackup");

                bool firstLaunch;
                bool.TryParse(_launcherDB.SelectSingleNode("/LauncherDB/LauncherMisc/IsFirstLaunch").InnerText, out firstLaunch);
                _firstLaunch = firstLaunch;

                _enabledMods = _launcherDB.SelectSingleNode("/LauncherDB/LauncherMisc/EnabledMods").InnerText;
            }
        }

        private string GetMusicName(int musicIndex)
        {
            switch (musicIndex)
            {
                default:
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
        }

        private void GetMyPublicIp()
        {
            using (WebClient getIP = new WebClient())
            {
                string myIP = getIP.DownloadString(new Uri("https://ipinfo.io/ip")).Trim();
                string ipPath = _cacheDir + "/paraworld_ip.txt";
                bool newIP = true;
                if (File.Exists(ipPath))
                {
                    using (StreamReader readIP = new StreamReader(ipPath))
                    {
                        string previousIP = readIP.ReadLine();
                        if (myIP == previousIP)
                        {
                            newIP = false;
                        }
                        else
                        {
                            newIP = true;
                        }
                    }
                }
                if (newIP == true)
                {
                    using (StreamWriter writeIP = new StreamWriter(ipPath, false, Encoding.Default))
                    {
                        writeIP.WriteLine(myIP);
                    }
                }
            }
        }

        public string Translate(string text)
        {
            return _launcherDB.DocumentElement.SelectSingleNode("/LauncherDB/LauncherLocalization/" + text).InnerText;
        }

        public static void SaveEnabledModsToDB(string name, string value)
        {
            _launcherDB.DocumentElement.SelectSingleNode("/LauncherDB/LauncherMisc/" + name).InnerText = value;
            _launcherDB.Save(_launcherDBPath);
        }

        private string OnFirstLaunch()
        {
            string firstLaunchError = "";

            /*
            #region Check for Tages drivers
            string tagesDir = Environment.SystemDirectory;
            if (!File.Exists(tagesDir + "/atksgt.sys") || !File.Exists(tagesDir + "/lirsgt.sys"))
            {
                firstLaunchError += "Tages drivers not found in\n" + tagesDir + "\n\n";
            }
            #endregion

            #region Check for Win7Fix
            if (File.Exists(_paraworldBinDir + "/Paraworld.exe"))
            {
                using (MD5 md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(_paraworldBinDir + "/Paraworld.exe"))
                    {
                        byte[] win7FixExeMD5 = { 08, 41, 82, 123, 147, 26, 93, 185, 136, 237, 71, 119, 102, 252, 145, 01 };
                        byte[] exeMD5 = MD5.ComputeHash(stream);
                        if (win7FixExeMD5.SequenceEqual(exeMD5) == false)
                        {
                            firstLaunchError += "Install Win7Fix\n\n";
                        }
                    }
                }
            }
            else
            {

            }
            #endregion
            */

            #region Check for PW fonts
            string fontsDir = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            if (!File.Exists($"{fontsDir}/trebuc.ttf") && !File.Exists($"{fontsDir}/trebucbd.ttf") && !File.Exists($"{fontsDir}/trebucbi.ttf") && !File.Exists($"{fontsDir}/trebucit.ttf"))
            {
                firstLaunchError += _pwFontsMissing + "\n\n";
            }
            #endregion

            #region Check for BP
            if (Directory.Exists(_paraworldDir))
            {
                if (!Directory.Exists(_paraworldDir + "/Data/BoosterPack1"))
                {
                    firstLaunchError += _bpMissing + "\n\n";
                }
            }
            #endregion

            #region Check for settings.cfg
            if (!Directory.Exists(_settingsDir) && !File.Exists(_settingsPath))
            {
                firstLaunchError += _settingsMissing + "\n\n";
            }
            #endregion

            return firstLaunchError;
        }

        public void VersionCheck(string version, string latestVersionURL)
        {
            if (!string.IsNullOrEmpty(version) && !string.IsNullOrEmpty(latestVersionURL))
            {
                Version mirageVersion = new Version(version);
                
                using (WebClient versionPage = new WebClient())
                {
                    versionPage.Proxy = new WebProxy();
                    //string FullSiteVersion = versionPage.DownloadString("https://para-welt.com/mirage/version.txt");
                    //string siteVersionFull = versionPage.DownloadString("https://raw.githubusercontent.com/Tatsukio/MIRAGE-Launcher/master/Resources/updateinfo.txt");
                    //versioncheck	MIRAGE 2.6.2	14	0
                    string siteVersionFull = versionPage.DownloadString(latestVersionURL);
                    //MessageBox.Show(siteVersionFull, null, MessageBoxButton.OK, MessageBoxImage.Error);
                    string[] siteVersion = siteVersionFull.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    if (siteVersion[0] == "versioncheck" && siteVersion != null)
                    {
                        string[] siteModVersion = siteVersion[1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        Version mirageSiteVersion = new Version(siteModVersion[1]);
                        switch (mirageVersion.CompareTo(mirageSiteVersion))
                        {
                            case 0:
                                //MirageVersion == MirageSiteVersion
                                break;
                            case 1:
                                //MirageVersion > MirageSiteVersion
                                break;
                            case -1:
                                UpdateLogText = "● " + siteVersion[4].Replace(";", "\n● ");
                                MessageBox.Show(UpdateLogText, null, MessageBoxButton.OK, MessageBoxImage.Error);
                                ShowUpdateWindow = true;
                                break;
                        }
                    }
                }
            }
        }

        public static ObservableCollection<ModInfo> ModCollection { get; } = new ObservableCollection<ModInfo>();

        public class ModInfo
        {
            public bool ModEnabled { get; set; }

            public string ModName { get; set; }

            public string ModVersion { get; set; }

            public List<string> ModRequires { get; set; }
        }

        public static void GetMods()
        {
            ModCollection.Clear();
            if (Directory.Exists(_infoDir))
            {
                foreach (string infoName in Directory.EnumerateFiles(_infoDir, "*.info").Select(Path.GetFileName).Where(s => s != "BaseLocale.info" && s != "LevelEd.info" && !s.Contains("Locale_")))
                {
                    using (StreamReader readInfo = new StreamReader(_infoDir + infoName))
                    {
                        bool modEnabled = false;
                        string modName = "";
                        string modVersion = "";
                        List<string> modRequires = new List<string>();

                        while (!readInfo.EndOfStream)
                        {
                            string line = readInfo.ReadLine();

                            if (line.StartsWith("id"))
                            {
                                modName = line.Split().Skip(1).FirstOrDefault();
                            }
                            else if (line.StartsWith("version"))
                            {
                                modVersion = line.Split().Skip(1).FirstOrDefault();
                            }
                            else if (line.StartsWith("requires"))
                            {
                                foreach (string temp in line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Skip(1))
                                {
                                    if (temp == "BaseData")
                                    {
                                        continue;
                                    }
                                    modRequires.Add(temp);
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(modName))
                        {
                            if (Array.Exists(_enabledMods.Split(','), s => s == modName))
                            {
                                modEnabled = true;
                            }
                            ModCollection.Add(new ModInfo() { ModEnabled = modEnabled, ModName = modName, ModVersion = modVersion, ModRequires = modRequires });
                        }
                        else
                        {
                            MessageBox.Show($"Error reading {infoName} file. Mod id can't be null.", null, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        public static List<string> ModList = new List<string>();

        public static string GetEnabledMods()
        {
            ModList.Clear();
            List<string> requiresList = new List<string>();

            foreach (ModInfo mod in ModCollection)
            {
                if (mod.ModEnabled)
                {
                    ModList.Add(mod.ModName);
                    requiresList = requiresList.Union(mod.ModRequires).ToList();
                }
            }
            if (ModList.Any())
            {
                string missingMods = string.Join(", ", requiresList.Except(ModList));

                if (string.IsNullOrEmpty(missingMods))
                {
                    string enabledMods = string.Join(",", ModList.Except(requiresList));
                    string commandLine = " -enable " + string.Join(" -enable ", enabledMods.Split(','));
                    return commandLine;
                }
                else
                {
                    MessageBox.Show($"Required mods ({missingMods}) not found or disabled.", null, MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
            return string.Empty;
        }
    }
}
