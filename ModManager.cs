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

            public bool ModCheckBoxEnabled { get; set; }

            public string ModName { get; set; }

            public string ModVersion { get; set; }

            public List<string> ModRequires { get; set; }
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

                        bool ModNameTouched = false;
                        bool ModVersionTouched = false;
                        bool ModRequiresTouched = false;

                        while (!ReadInfo.EndOfStream && (!ModNameTouched || !ModVersionTouched || !ModRequiresTouched))
                        {
                            string Line = ReadInfo.ReadLine();

                            if (Line.StartsWith("#") || Line.Length == 0)
                            {
                                continue;
                            }
                            if (Line.StartsWith("id "))
                            {
                                ModName = Line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Last();
                                ModNameTouched = true;
                            }
                            else if (Line.StartsWith("version "))
                            {
                                ModVersion = Line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Last();
                                ModVersionTouched = true;
                            }
                            else if (Line.StartsWith("requires"))
                            {
                                foreach (string Temp in Line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    if (Temp == "requires" || Temp == "BaseData")
                                    {
                                        continue;
                                    }
                                    ModRequires.Add(Temp);
                                }
                                ModRequiresTouched = true;
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

        public static string GetEnabledMods()
        {
            List<string> Mods = new List<string>();
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
                    string EnabledMods = " -enable " + string.Join(" -enable ", Mods.Except(Requires));
                    return EnabledMods;
                }
                else
                {
                    MessageBox.Show("The following mods were not found or should be enabled: " + MissingMods, null, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return null;
        }
    }
}
