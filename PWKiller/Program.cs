using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace PWKiller
{
    static class Program
    {
        public static XmlDocument Locale = new XmlDocument();

        public static string ToolsDir = AppDomain.CurrentDomain.BaseDirectory;
        public static string[] Processes = { "Paraworld", "PWClient", "PWServer" };

        static void Main(string[] args)
        {
            if (File.Exists(ToolsDir + "/../bin/PWClient2.exe"))
            {
                Processes[1] = "PWClient2";
            }
            if (args.Contains("-SSSOffAfterPWExit"))
            {
                CheckPW();
            }
            else
            {
                LoadLocale();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new PWKiller());
            }
        }
        public static bool LoadLocale()
        {
            string MirageDBPath = Path.GetFullPath(ToolsDir + "/../Data/MIRAGE/Texts/mirage_db.xml");
            if (File.Exists(MirageDBPath))
            {
                Locale.Load(MirageDBPath);
                return true;
            }
            return false;
        }
        public static string Translate(string Text)
        {
            string TextPath = "/mirage_db/pwkiller_localization";
            return Locale.DocumentElement.SelectSingleNode(TextPath + Text).InnerText;
        }
        public static void CheckPW()
        {
            while (true)
            {
                if (!Process.GetProcessesByName(Processes[0]).Any())
                {
                    if (!Process.GetProcessesByName(Processes[1]).Any())
                    {
                        break;
                        //if (!Process.GetProcessesByName(Processes[2]).Any())
                        //{
                        //    break;
                        //}
                    }
                }
                System.Threading.Thread.Sleep(1000);
            }
            ParaWorldExited();
        }
        public static void ParaWorldExited()
        {
            if (File.Exists(ToolsDir + "/mod_conf.exe"))
            {
                string AppDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                ProcessStartInfo mod_conf_start = new ProcessStartInfo(ToolsDir + "/mod_conf.exe", "SSSOff " + AppDataDir)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process.Start(mod_conf_start);
            }
            KillPW();
        }
        public static void KillPW()
        {
            for (int i = 0; i < 3; i++)
            {
                Process[] AllParaworld = Process.GetProcessesByName(Processes[i]);
                foreach (Process Paraworld in AllParaworld)
                {
                    Paraworld.Kill();
                }
            }

            Process Me = Process.GetCurrentProcess();
            Process[] AllPWKiller = Process.GetProcessesByName("PWKiller");
            foreach (Process PWKiller in AllPWKiller)
            {
                if (PWKiller.Id != Me.Id)
                {
                    PWKiller.Kill();
                }
            }
            Me.Kill();
        }
    }
}
