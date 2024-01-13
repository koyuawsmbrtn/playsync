using System;
using System.IO;
using System.Windows.Forms;

namespace PlaySync
{
    public partial class ScreenshotViewer : Form
    {
        string imagefolder = "";
        public ScreenshotViewer()
        {
            InitializeComponent();
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo d in allDrives)
            {
                if (d.VolumeLabel == "PLAYDATE")
                {
                    imagefolder = Path.Combine(d.Name, "Screenshots");
                }
            }
            // Read and display all screenshots from temp folder
            if (Directory.Exists(imagefolder))
            {
                string[] files = Directory.GetFiles(imagefolder);
                foreach (string file in files)
                {
                    if (file.EndsWith(".gif"))
                    {
                        PictureBox pb = new PictureBox();
                        pb.ImageLocation = file;
                        pb.SizeMode = PictureBoxSizeMode.StretchImage;
                        pb.Width = 200;
                        pb.Height = 120;
                        pb.DoubleClick += Pb_Click;
                        pb.ContextMenuStrip = contextMenuStrip1;
                        flowLayoutPanel1.Controls.Add(pb);
                    }
                }
            }
        }

        private void Pb_Click(object sender, EventArgs e)
        {
            PictureBox pb = (PictureBox)sender;
            string path = Path.Combine(imagefolder, pb.ImageLocation);
            System.Diagnostics.Process.Start(path);
        }

        private void viewImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PictureBox pb = (PictureBox)contextMenuStrip1.SourceControl;
            string path = Path.Combine(imagefolder, pb.ImageLocation);
            System.Diagnostics.Process.Start(path);
        }

        private void deleteImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PictureBox pb = (PictureBox)contextMenuStrip1.SourceControl;
            string path = Path.Combine(imagefolder, pb.ImageLocation);
            if (MessageBox.Show("Are you sure you want to delete this image?", "Delete Image", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                File.Delete(path);
                flowLayoutPanel1.Controls.Remove(pb);
            }
        }

        private void copyImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PictureBox pb = (PictureBox)contextMenuStrip1.SourceControl;
            string path = Path.Combine(imagefolder, pb.ImageLocation);
            Clipboard.SetImage(System.Drawing.Image.FromFile(path));
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
