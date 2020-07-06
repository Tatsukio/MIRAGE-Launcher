using System;
using System.Windows.Forms;

namespace PWKiller
{
    public partial class PWKiller : Form
    {
        public PWKiller()
        {
            InitializeComponent();
            if (Program.LoadLocale())
            {
                SetLocale();
            }
        }
        void SetLocale()
        {
            MainLabel.Text = Program.Translate("/pwkiller_label");
            MainButton.Text = Program.Translate("/pwkiller_button");
        }
        void MainButton_Click(object sender, EventArgs e)
        {
            Program.KillPW();
        }
    }
}
