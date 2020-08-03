using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using static MIRAGE_Launcher.MainWindow;

namespace MIRAGE_Launcher
{
    class ModManager
    {
        public static ObservableCollection<ModInfo> ModList = new ObservableCollection<ModInfo>();
        public class ModInfo : INotifyPropertyChanged
        {
            private bool modEnabled;
            public bool ModEnabled
            {
                get { return modEnabled; }
                set { modEnabled = value; OnPropertyChanged(); }
            }

            private bool modCheckBoxEnabled;
            public bool ModCheckBoxEnabled
            {
                get { return modCheckBoxEnabled; }
                set { modCheckBoxEnabled = value; OnPropertyChanged(); }
            }

            public string ModName { get; set; }

            public string ModVersion { get; set; }

            public List<string> ModRequires { get; set; }


            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public static void GetMods()
        {
            if (Directory.Exists(InfoDir))
            {
                foreach (string InfoName in Directory.EnumerateFiles(InfoDir, "*.info").Select(Path.GetFileName).Where(s => s != "BaseLocale.info" && s != "LevelEd.info" && !s.Contains("Locale_")))
                {
                    using (StreamReader ReadInfo = new StreamReader(InfoDir + InfoName))
                    {
                        string ModName = "";
                        string ModVersion = "";
                        List<string> ModRequires = new List<string>();

                        while (!ReadInfo.EndOfStream)
                        {
                            string Line = ReadInfo.ReadLine();

                            if (Line.StartsWith("id"))
                            {
                                ModName = Line.Split().Skip(1).FirstOrDefault();
                            }
                            else if (Line.StartsWith("version"))
                            {
                                ModVersion = Line.Split().Skip(1).FirstOrDefault();
                            }
                            else if (Line.StartsWith("requires"))
                            {
                                foreach (string Temp in Line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Skip(1))
                                {
                                    if (Temp == "BaseData")
                                    {
                                        continue;
                                    }
                                    ModRequires.Add(Temp);
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(ModName))
                        {
                            ModList.Add(new ModInfo() { ModEnabled = true, ModCheckBoxEnabled = true, ModName = ModName, ModVersion = ModVersion, ModRequires = ModRequires });
                        }
                        else
                        {
                            MessageBox.Show($"Error reading {InfoName} file. Mod id can't be null.", null, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        static HashSet<string> EnabledMods = new HashSet<string>();
        static List<string> Requires = new List<string>();

        public static void GetEnabledMods()
        {
            EnabledMods.Clear();
            Requires.Clear();
            foreach (ModInfo Mod in ModList)
            {
                if (Mod.ModEnabled)
                {
                    EnabledMods.Add(Mod.ModName);
                    Requires = Requires.Union(Mod.ModRequires).ToList();
                }
            }
            if (Requires.Except(EnabledMods).Any())
            {
                EnableRequires();
            }
        }

        public static void EnableRequires()
        {
            foreach (string Require in Requires)
            {
                foreach (ModInfo Mod in ModList)
                {
                    if (Mod.ModName == Require)
                    {
                        Mod.ModEnabled = true;
                    }
                }
            }
            GetEnabledMods();
        }

        public static string PrepareCommandLine()
        {
            if (EnabledMods.Any())
            {
                string MissingMods = string.Join(", ", Requires.Except(EnabledMods));

                if (string.IsNullOrEmpty(MissingMods))
                {
                    string Mods = " -enable " + string.Join(" -enable ", EnabledMods.Except(Requires));
                    return Mods;
                }
                else
                {
                    MessageBox.Show($"Required mods ({MissingMods}) not found or disabled. All mods disabled.", null, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return null;
        }
    }
}
