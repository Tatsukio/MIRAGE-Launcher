using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using static MIRAGE_Launcher.MainWindow;

namespace MIRAGE_Launcher
{
    class ModManager
    {
        public static ObservableCollection<ModInfo> ModList = new ObservableCollection<ModInfo>();

        public class ModInfo
        {
            public bool ModEnabled { get; set; }

            public string ModName { get; set; }

            public string ModVersion { get; set; }

            public List<string> ModRequires { get; set; }
        }

        public static void GetMods()
        {
            ModList.Clear();
            if (Directory.Exists(InfoDir))
            {
                foreach (string InfoName in Directory.EnumerateFiles(InfoDir, "*.info").Select(Path.GetFileName).Where(s => s != "BaseLocale.info" && s != "LevelEd.info" && !s.Contains("Locale_")))
                {
                    using (StreamReader ReadInfo = new StreamReader(InfoDir + InfoName))
                    {
                        bool ModEnabled = false;
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
                                    if(Temp == "BaseData")
                                    {
                                        continue;
                                    }
                                    ModRequires.Add(Temp);
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(ModName))
                        {
                            if (Array.Exists(EnabledMods.Split(','), s => s == ModName))
                            {
                                ModEnabled = true;
                            }
                            ModList.Add(new ModInfo() { ModEnabled = ModEnabled, ModName = ModName, ModVersion = ModVersion, ModRequires = ModRequires });
                        }
                        else
                        {
                            MessageBox.Show($"Error reading {InfoName} file. Mod id can't be null.", null, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        public static List<string> Mods = new List<string>();

        public static string GetEnabledMods()
        {
            Mods.Clear();
            List<string> Requires = new List<string>();

            foreach (ModInfo Mod in ModList)
            {
                if (Mod.ModEnabled)
                {
                    Mods.Add(Mod.ModName);
                    Requires = Requires.Union(Mod.ModRequires).ToList();
                }
            }
            if (Mods.Any())
            {
                string MissingMods = string.Join(", ", Requires.Except(Mods));

                if (string.IsNullOrEmpty(MissingMods))
                {
                    string EnabledMods = string.Join(",", Mods.Except(Requires));
                    return EnabledMods;
                }
                else
                {
                    MessageBox.Show($"Required mods ({MissingMods}) not found or disabled.", null, MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
            return string.Empty;
        }

        public static string GetCommandLine(string EnabledMods)
        {
            string CommandLine = " -enable " + string.Join(" -enable ", EnabledMods.Split(','));
            return CommandLine;
        }
    }
}
