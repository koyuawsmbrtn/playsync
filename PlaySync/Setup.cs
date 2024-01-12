using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace PlaySync
{
    public partial class Setup : Form
    {
        string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PlaySync");
        static string sdkFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PlaydateSDK");
        string gamesFolder = Path.Combine(sdkFolder, "Disk\\Games");
        int setupState = 0;
        public Setup()
        {
            InitializeComponent();
            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
            }
            if (File.Exists(Path.Combine(appDataFolder, "config.ini")))
            {
                setupState = 2;
            } else
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
            }
            if (!Directory.Exists(sdkFolder))
            {
                button1.Enabled = false;
                label2.Text = "The Playdate SDK isn't installed on your system. Please choose your games folder.";
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void label3_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Maximized;
            } else
            {
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (setupState == 0)
            {
                folderBrowserDialog1.ShowDialog();
                gamesFolder = folderBrowserDialog1.SelectedPath;
                setupState = 1;
            } else if (setupState == 1)
            {
                setupState = 2;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (setupState == 0)
            {
                setupState = 1;
            } else if (setupState == 1)
            {
                setupState = 2;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (setupState == 1)
            {
                label4.Text = "Step 2: Done";
                label2.Text = "Configuration finished. Enjoy PlaySync!";
                button1.Text = "Back";
                button1.Enabled = false;
                button2.Text = "Continue";
                File.WriteAllText(Path.Combine(appDataFolder, "config.ini"), "[paths]\ngamefolder="+gamesFolder, Encoding.UTF8);
            }
            if (setupState == 2)
            {
                Main mainWindow = new Main();
                mainWindow.Show();
                setupState = 3;
            }
            if (setupState == 3)
            {
                this.Hide();
            }
        }
    }
}
