using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;

namespace PWKiller
{
    public partial class PWKillerMain : Form
    {
        public PWKillerMain(string[] args)
        {
            InitializeComponent();
            if (File.Exists(_toolsDir + "/../bin/Paraworld2.exe"))
            {
                _processes[0] = "Paraworld2";
            }
            if (File.Exists(_toolsDir + "/../bin/PWClient2.exe"))
            {
                _processes[1] = "PWClient2";
            }
            if (args.Contains("-SSSOffAfterPWExit"))
            {
                CheckPW();
            }
            else
            {
                if (LoadLocale())
                {
                    PWKillerTitleText.Text = Translate("/PWKillerTitleText");
                    PWKillerButtonText.Text = Translate("/PWKillerButtonText");
                }
            }
        }

        static readonly XmlDocument _locale = new XmlDocument();

        static readonly string _toolsDir = AppDomain.CurrentDomain.BaseDirectory;
        static readonly string[] _processes = { "Paraworld", "PWClient", "PWServer" };

        void MainButton_Click(object sender, EventArgs e)
        {
            KillPW();
            Process me = Process.GetCurrentProcess();
            me.Kill();
        }

        public static bool LoadLocale()
        {
            string _launcherDBPath = Path.GetFullPath(_toolsDir + "/Launcher/LauncherDB.xml");
            if (File.Exists(_launcherDBPath))
            {
                _locale.Load(_launcherDBPath);
                return true;
            }
            return false;
        }
        public static string Translate(string text)
        {
            string textPath = "/LauncherDB/PWKillerLocalization";
            return _locale.DocumentElement.SelectSingleNode(textPath + text).InnerText;
        }
        public static void CheckPW()
        {
            System.Threading.Thread.Sleep(1000);
            while (true)
            {
                if (!Process.GetProcessesByName(_processes[0]).Any())
                {
                    if (!Process.GetProcessesByName(_processes[1]).Any())
                    {
                        break;
                    }
                }
                System.Threading.Thread.Sleep(1000);
            }
            ParaWorldExited();
        }
        public static void ParaWorldExited()
        {
            if (File.Exists(_toolsDir + "/mod_conf.exe"))
            {
                string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                ProcessStartInfo modConf = new ProcessStartInfo(_toolsDir + "/mod_conf.exe", "SSSOff " + appDataDir)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process.Start(modConf);
            }
            KillPW();
            KillPWKiller();
        }
        public static void KillPW()
        {
            for (int i = 0; i < 3; i++)
            {
                Process[] paraworldProcesses = Process.GetProcessesByName(_processes[i]);
                foreach (Process paraworldProcess in paraworldProcesses)
                {
                    paraworldProcess.Kill();
                }
            }
        }
        public static void KillPWKiller()
        {
            Process me = Process.GetCurrentProcess();
            Process[] AllPWKiller = Process.GetProcessesByName("PWKiller");
            foreach (Process PWKiller in AllPWKiller)
            {
                if (PWKiller.Id != me.Id)
                {
                    PWKiller.Kill();
                }
            }
            me.Kill();
        }
    }
}
