using IniParser;
using System.IO;
using System;
using System.Windows.Forms;
using IniParser.Model;
using System.Diagnostics;
using System.Net;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;
using RuFramework;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace PlaySync
{
    public partial class Main : Form
    {
        string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PlaySync");
        FileIniDataParser parser = new FileIniDataParser();
        IniData config = null;
        string gamefolder = "C:\\";
        bool pluggedin = false;
        bool syncing = false;
        bool cansync = false;
        string version = "0";
        string oldtext = "";
        string deviceinfo = "";
        string[] games;
        string title = "PlaySync";
        string tempfolder = "";

        public static bool isDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                    return false;
#endif
            }
        }

        public Main()
        {
            InitializeComponent();
            oldtext = richTextBox1.Text;
            config = parser.ReadFile(Path.Combine(appDataFolder, "config.ini"));
            gamefolder = config["paths"]["gamefolder"];
            label5.Text = "Game folder: " + gamefolder + " (click to open)";
            readGames(gamefolder);
            if (!gamefolder.Contains("PlaydateSDK"))
            {
                button7.Enabled = false;
            }
            if (isDebug)
            {
                debugToolStripMenuItem.Visible = true;
            }
            this.Text = title;
            if (isDebug)
            {
                toolStripMenuItem2.Visible = false;
            }
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (rk.GetValue("PlaySync") != null)
            {
                toolStripMenuItem2.Checked = true;
            }
            else
            {
                toolStripMenuItem2.Checked = false;
            }
            string[] args = Environment.GetCommandLineArgs();
            foreach (string arg in args)
            {
                if (arg == "--autostart")
                {
                    this.ShowInTaskbar = false;
                    this.WindowState = FormWindowState.Minimized;
                }
            }
        }

        private void readGames(string readfolder)
        {
            listBox1.Items.Clear();
            string[] folders = Directory.GetDirectories(readfolder);
            games = folders;
            foreach (string folder in folders)
            {
                try
                {
                    string[] pdxinfo = File.ReadAllLines(Path.Combine(gamefolder, folder, "pdxinfo"));
                    string gamename = "";
                    foreach (string line in pdxinfo)
                    {
                        if (line.StartsWith("name="))
                        {
                            gamename = line.Replace(" ", "").Replace("name=", "");
                        }
                    }
                    listBox1.Items.Add(gamename);
                }
                catch { }
            }
        }

        private string sendSerial(string port, string command)
        {
            /*
             * Serial function for the Playdate
             * Documented further at https://github.com/cranksters/playdate-reverse-engineering/blob/main/usb/usb.md
            */
            SerialPort serialPort = new SerialPort();
            serialPort.PortName = port;
            serialPort.BaudRate = 115200;
            serialPort.Parity = Parity.None;
            serialPort.StopBits = StopBits.One;
            serialPort.DataBits = 8;
            serialPort.ReadTimeout = 20;
            try
            {
                serialPort.Open();
                serialPort.WriteLine(command); // Might have to wait a little for output, but we're gonna try it out when the actual hardware arrived
                string output = "";
                while (true)
                {
                    try
                    {
                        output += serialPort.ReadLine() + "\n";
                    }
                    catch
                    {
                        return output;
                    }
                }

            }
            catch
            {
                return "";
            }
            finally
            {
                if (serialPort.IsOpen)
                {
                    try
                    {
                        serialPort.Close();
                    }
                    catch { }
                }
            }
            /*
             * The following commands are available:

                Telnet commands:
                 help        Displays all available commands or individual help on each command

                CPU Control:
                 serialread  Print the device serial number
                 trace       trace_<delay>. (trace 10)
                 stoptrace   stoptrace
                 bootdisk    reboot into recovery segment USB disk
                 datadisk    reboot into data segment USB disk
                 factoryreset factory reset
                 formatdata  format data disk
                 settime     sets the RTC. format is ISO8601 plus weekday (1=mon) e.g.: 2018-03-20T19:58:29Z 2
                 gettime     reads the RTC
                 vbat        get battery voltage
                 rawvbat     get raw battery adc value
                 batpct      get battery percentage
                 temp        get estimated ambient temperature
                 dcache      dcache <on/off>: turn dcache on or off
                 icache      icache <on/off>: turn icache on or off

                Runtime control:
                 echo        echo (on|off): turn console echo on or off
                 buttons     Test buttons & crank
                 tunebuttons tunebuttons <debounce> <holdoff>
                 btn         btn <btn>: simulate a button press. +a/-a/a for down/up/both
                 changecrank changecrank +-<degrees>
                 dockcrank   simulates crank docking
                 enablecrank Reenables crank updates
                 disablecrank Disables crank updates
                 accel       simulate accelerometer change
                 screen      Dump framebuffer data (400x240 bits)
                 bitmap      Send bitmap to screen (followed by 400x240 bits)
                 controller  start or stop controller mode
                 eval        execute a compiled Lua function
                 run         run <path to pdx>: Run the named program
                 luatrace    Get a Lua stack trace
                 stats       Display runtime stats
                 autolock    autolock <always|onBattery>
                 version     Display build target and SDK version
                 memstats    memstats
                 hibernate   hibernate

                Stream:
                 stream      stream <enable|disable|poke>

                ESP functions:
                 espreset    reset the ESP chip
                 espoff      turn off the ESP
                 espbootlog  get the ESP startup log
                 espfile     espfile <path> <address> <md5> <uncompressed size>: add the given file to the upload list. If <uncompressed size> is added then the file is assumed to be compressed.
                 espflash    espflash <baud> [0|1] send the files listed with the espfile command to the ESP flash.
                 espbaud     espbaud <speed> [cts]
                 esp         esp <cmd>: Forward a command to the ESP firmware, read until keypress

                Firmware Update:
                 fwup        fwup [bundle_path]
            */
        }

        private void label5_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", gamefolder);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            readGames(gamefolder);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            string savesFolder = config["paths"]["gamefolder"].Replace("\\Games", "\\Data");
            foreach (string folder in Directory.GetDirectories(savesFolder))
            {
                listBox1.Items.Add(folder.Replace(savesFolder + "\\", ""));
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            launchUrl("https://shop.play.date");
        }

        private void showPluggedUI()
        {
            if (pluggedin)
            {
                label1.Text = "PlaySync";
                pictureBox3.Visible = true;
                pictureBox1.Visible = false;
                button4.Enabled = true;
                //button5.Enabled = true;
                //button2.Enabled = true;
                //button8.Enabled = true;
                //readDeviceToolStripMenuItem.Enabled = true;
                //readDeviceToolStripMenuItem1.Enabled = true;
                toDeviceToolStripMenuItem.Enabled = true;
                //fromDeviceToolStripMenuItem.Enabled = true;
            }
            else
            {
                label1.Text = "Waiting for device...";
                pictureBox3.Visible = false;
                pictureBox1.Visible = true;
                button4.Enabled = false;
                button5.Enabled = false;
                button2.Enabled = false;
                button8.Enabled = false;
                readDeviceToolStripMenuItem.Enabled = false;
                readDeviceToolStripMenuItem1.Enabled = false;
                toDeviceToolStripMenuItem.Enabled = false;
                fromDeviceToolStripMenuItem.Enabled = false;
            }
            if (syncing)
            {
                label1.Text = "Syncing...";
            }
        }

        private void updateTextBox(string serial, string battery, string osver)
        {
            richTextBox1.Text = $"Playdate\nSerial: {serial}\nBattery: {battery}%\nOS: Playdate OS {osver}";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            pluggedin = !pluggedin;
            if (pluggedin)
            {
                updateTextBox("PD-A123456", "100", "2.1.1");
            }
            else
            {
                richTextBox1.Text = oldtext;
            }
            showPluggedUI();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Quit();
        }

        private void readLocalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            readGames(gamefolder);
        }

        private void readLocalToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            string savesFolder = config["paths"]["gamefolder"].Replace("\\Games", "\\Data");
            foreach (string folder in Directory.GetDirectories(savesFolder))
            {
                listBox1.Items.Add(folder.Replace(savesFolder + "\\", ""));
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("PlaySync v. 1.0.0\n(C) koyu.space 2023", "About PlaySync", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void launchUrl(string url)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cmd.StartInfo.Arguments = $"/c start {url}";
            cmd.Start();
        }

        private void visitKoyuspaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            launchUrl("https://koyu.space");
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string pubv = new WebClient().DownloadString("https://updates.koyu.space/playsync/latest");
                if (pubv.Split('\n')[0] != version)
                {
                    DialogResult result = MessageBox.Show("New update available. Download now?", "New update available", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        launchUrl("https://updates.koyu.space/playsync/playsync.zip");
                    }
                }
                else
                {
                    MessageBox.Show("PlaySync is up to date!", "Up to date", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch
            {
                MessageBox.Show("Your computer is not connected to the internet. Checking for updates failed.", "Checking for updates failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void getSerialPortsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            string sports = "";
            foreach (string port in ports)
            {
                if (port != "COM1" && port != "COM3")
                {
                    sports += port + "\n";
                }
            }
            if (sports != "")
            {
                MessageBox.Show(sports);
            }
            else
            {
                MessageBox.Show("No devices connected");
            }
        }

        private string getPlaydatePort()
        {
            try
            {
                string port = "";
                string VID = "1331";
                string PID = "5740";
                String pattern = String.Format("^VID_{0}.PID_{1}", VID, PID);
                Regex _rx = new Regex(pattern, RegexOptions.IgnoreCase);
                RegistryKey rk1 = Registry.LocalMachine;
                RegistryKey rk2 = rk1.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum");
                foreach (String s3 in rk2.GetSubKeyNames())
                {
                    RegistryKey rk3 = rk2.OpenSubKey(s3);
                    foreach (String s in rk3.GetSubKeyNames())
                    {
                        if (_rx.Match(s).Success)
                        {
                            RegistryKey rk4 = rk3.OpenSubKey(s);
                            foreach (String s2 in rk4.GetSubKeyNames())
                            {
                                RegistryKey rk5 = rk4.OpenSubKey(s2);
                                RegistryKey rk6 = rk5.OpenSubKey("Device Parameters");
                                if (rk6.GetValue("PortName") != null)
                                {
                                    port = rk6.GetValue("PortName").ToString();
                                }
                            }
                        }
                    }
                }
                string[] ports = SerialPort.GetPortNames();
                bool isavailable = false;
                foreach (string sport in ports)
                {
                    if (port == sport)
                    {
                        isavailable = true;
                    }
                }
                if (isavailable)
                {
                    return port;
                }
                else
                {
                    return "";
                }
            }
            catch
            {
                return "";
            }
        }

        private void simulateSyncToolStripMenuItem_Click(object sender, EventArgs e)
        {
            syncing = !syncing;
            pluggedin = true;
            showPluggedUI();
        }

        private void changePlug(string serial = "", string battery = "", string version = "")
        {
            if (pluggedin)
            {
                updateTextBox(serial, battery, version);
                if (serial != "")
                {
                    deviceinfo = richTextBox1.Text;
                }
            }
            else
            {
                richTextBox1.Text = oldtext;
            }
            showPluggedUI();
        }

        private void syncToDevice(object status)
        {
            try
            {
                IProgressCallback callback = status as IProgressCallback;
                int i = 0;
                if (games.Length == 0)
                {
                    callback.End();
                    ejectPlaydate();
                    BeginInvoke(new Action(() =>
                    {
                        this.Text = title;
                    }));
                }
                if (cansync)
                {
                    callback.Begin(0, games.Length);
                    string gamefolder = config["paths"]["gamefolder"];
                    string playdatedrive = "";
                    foreach (var item in System.IO.DriveInfo.GetDrives())
                    {
                        if (item.VolumeLabel == "PLAYDATE")
                        {
                            playdatedrive = item.Name;
                        }
                    }
                    if (playdatedrive != "")
                    {
                        // The .pdx folder from the game array and on the device are treated like files/apps on macOS. Recursively sync all .pdx folders and subfolders to the device, but only if they are not already there and check if the game is newer than the one on the device. The pdxinfo is irrelevant!! All games on the device reside in D:\Games, but D: isn't always the drive letter. We have to check for the label.
                        foreach (string game in games)
                        {
                            try
                            {
                                string gameName = game.Replace(gamefolder + "\\", "");
                                if (!Directory.Exists(Path.Combine(playdatedrive, "Games", gameName)))
                                {
                                    Directory.CreateDirectory(Path.Combine(playdatedrive, "Games", gameName));
                                }
                                foreach (string file in Directory.GetFiles(game))
                                {
                                    string fileName = file.Replace(game + "\\", "");
                                    if (!File.Exists(Path.Combine(playdatedrive, "Games", gameName, fileName)))
                                    {
                                        File.Copy(file, Path.Combine(playdatedrive, "Games", gameName, fileName));
                                    }
                                    else
                                    {
                                        if (File.GetLastWriteTime(file) > File.GetLastWriteTime(Path.Combine(playdatedrive, "Games", gameName, fileName)))
                                        {
                                            File.Copy(file, Path.Combine(playdatedrive, "Games", gameName, fileName), true);
                                        }
                                    }
                                }
                                foreach (string folder in Directory.GetDirectories(game))
                                {
                                    string folderName = folder.Replace(game + "\\", "");
                                    if (!Directory.Exists(Path.Combine(playdatedrive, "Games", gameName, folderName)))
                                    {
                                        Directory.CreateDirectory(Path.Combine(playdatedrive, "Games", gameName, folderName));
                                    }
                                    foreach (string file in Directory.GetFiles(folder))
                                    {
                                        string fileName = file.Replace(folder + "\\", "");
                                        if (!File.Exists(Path.Combine(playdatedrive, "Games", gameName, folderName, fileName)))
                                        {
                                            File.Copy(file, Path.Combine(playdatedrive, "Games", gameName, folderName, fileName));
                                        }
                                        else
                                        {
                                            if (File.GetLastWriteTime(file) > File.GetLastWriteTime(Path.Combine(playdatedrive, "Games", gameName, folderName, fileName)))
                                            {
                                                File.Copy(file, Path.Combine(playdatedrive, "Games", gameName, folderName, fileName), true);
                                            }
                                        }
                                    }
                                    foreach (string subfolder in Directory.GetDirectories(folder))
                                    {
                                        string subfolderName = subfolder.Replace(folder + "\\", "");
                                        if (!Directory.Exists(Path.Combine(playdatedrive, "Games", gameName, folderName, subfolderName)))
                                        {
                                            Directory.CreateDirectory(Path.Combine(playdatedrive, "Games", gameName, folderName, subfolderName));
                                        }
                                        foreach (string file in Directory.GetFiles(subfolder))
                                        {
                                            string fileName = file.Replace(subfolder + "\\", "");
                                            if (!File.Exists(Path.Combine(playdatedrive, "Games", gameName, folderName, subfolderName, fileName)))
                                            {
                                                File.Copy(file, Path.Combine(playdatedrive, "Games", gameName, folderName, subfolderName, fileName));
                                            }
                                            else
                                            {
                                                if (File.GetLastWriteTime(file) > File.GetLastWriteTime(Path.Combine(playdatedrive, "Games", gameName, folderName, subfolderName, fileName)))
                                                {
                                                    File.Copy(file, Path.Combine(playdatedrive, "Games", gameName, folderName, subfolderName, fileName), true);
                                                }
                                            }
                                        }
                                        foreach (string subsubfolder in Directory.GetDirectories(subfolder))
                                        {
                                            string subsubfolderName = subsubfolder.Replace(subfolder + "\\", "");
                                            if (!Directory.Exists(Path.Combine(playdatedrive, "Games", gameName, folderName, subfolderName, subsubfolderName)))
                                            {
                                                Directory.CreateDirectory(Path.Combine(playdatedrive, "Games", gameName, folderName, subfolderName, subsubfolderName));
                                            }
                                            foreach (string file in Directory.GetFiles(subsubfolder))
                                            {
                                                string fileName = file.Replace(subsubfolder + "\\", "");
                                                if (!File.Exists(Path.Combine(playdatedrive, "Games", gameName, folderName, subfolderName, subsubfolderName, fileName)))
                                                {
                                                    File.Copy(file, Path.Combine(playdatedrive, "Games", gameName, folderName, subfolderName, subsubfolderName, fileName));
                                                }
                                                else
                                                {
                                                    if (File.GetLastWriteTime(file) > File.GetLastWriteTime(Path.Combine(playdatedrive, "Games", gameName, folderName, subfolderName, subsubfolderName, fileName)))
                                                    {
                                                        File.Copy(file, Path.Combine(playdatedrive, "Games", gameName, folderName, subfolderName, subsubfolderName, fileName), true);
                                                    }
                                                }
                                            }
                                            foreach (string subsubsubfolder in Directory.GetDirectories(subsubfolder))
                                            {
                                                string subsubsubfolderName = subsubsubfolder.Replace(subsubfolder + "\\", "");
                                                if (!Directory.Exists(Path.Combine(playdatedrive, "Games", gameName, folderName, subfolderName, subsubfolderName, subsubsubfolderName)))
                                                {
                                                    Directory.CreateDirectory(Path.Combine(playdatedrive, "Games", gameName, folderName, subfolderName, subsubfolderName, subsubsubfolderName));
                                                }
                                                foreach (string file in Directory.GetFiles(subsubsubfolder))
                                                {
                                                    string fileName = file.Replace(subsubsubfolder + "\\", "");
                                                    if (!File.Exists(Path.Combine(playdatedrive, "Games", gameName, folderName, subfolderName, subsubfolderName, subsubsubfolderName, fileName)))
                                                    {
                                                        File.Copy(file, Path.Combine(playdatedrive, "Games", gameName, folderName, subfolderName, subsubfolderName, subsubsubfolderName, fileName));
                                                    }
                                                    else
                                                    {
                                                        if (File.GetLastWriteTime(file) > File.GetLastWriteTime(Path.Combine(playdatedrive, "Games", gameName, folderName, subfolderName, subsubfolderName, subsubsubfolderName, fileName)))
                                                        {
                                                            File.Copy(file, Path.Combine(playdatedrive, "Games", gameName, folderName, subfolderName, subsubfolderName, subsubsubfolderName, fileName), true);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch { }
                            i++;
                            callback.StepTo(i);
                        }
                    }
                }
                else
                {
                    callback.End();
                    ejectPlaydate();
                    BeginInvoke(new Action(() =>
                    {
                        this.Text = title;
                    }));
                }
                callback.StepTo(games.Length);
                callback.End();
                ejectPlaydate();
                BeginInvoke(new Action(() =>
                {
                    this.Text = title;
                }));
            }
            catch
            {
                ejectPlaydate();
                BeginInvoke(new Action(() =>
                {
                    this.Text = title;
                }));
            }
        }

        private void syncFromDevice(object callback)
        {
            try
            {
                IProgressCallback progress = callback as IProgressCallback;
                if (cansync)
                {
                    string gamefolder = config["paths"]["gamefolder"];
                    string playdatedrive = "";
                    foreach (var item in System.IO.DriveInfo.GetDrives())
                    {
                        if (item.VolumeLabel == "PLAYDATE")
                        {
                            playdatedrive = item.Name;
                        }
                    }
                    if (playdatedrive != "")
                    {
                        string[] folders = Directory.GetDirectories(playdatedrive);
                        foreach (string folder in folders)
                        {
                            if (folder.Contains("User") || folder.Contains("Seasons") || folder.Contains("Purchased"))
                            {
                                try
                                {
                                    string folderName = folder.Replace(playdatedrive, "");
                                    if (!Directory.Exists(Path.Combine(gamefolder, folderName)))
                                    {
                                        Directory.CreateDirectory(Path.Combine(gamefolder, folderName));
                                    }
                                    foreach (string file in Directory.GetFiles(folder))
                                    {
                                        string fileName = file.Replace(folder + "\\", "");
                                        if (!File.Exists(Path.Combine(gamefolder, folderName, fileName)))
                                        {
                                            File.Copy(file, Path.Combine(gamefolder, folderName, fileName));
                                        }
                                        else
                                        {
                                            if (File.GetLastWriteTime(file) > File.GetLastWriteTime(Path.Combine(gamefolder, folderName, fileName)))
                                            {
                                                File.Copy(file, Path.Combine(gamefolder, folderName, fileName), true);
                                            }
                                        }
                                    }
                                    foreach (string subfolder in Directory.GetDirectories(folder))
                                    {
                                        string subfolderName = subfolder.Replace(folder + "\\", "");
                                        if (!Directory.Exists(Path.Combine(gamefolder, folderName, subfolderName)))
                                        {
                                            Directory.CreateDirectory(Path.Combine(gamefolder, folderName, subfolderName));
                                        }
                                        foreach (string file in Directory.GetFiles(subfolder))
                                        {
                                            string fileName = file.Replace(subfolder + "\\", "");
                                            if (!File.Exists(Path.Combine(gamefolder, folderName, subfolderName, fileName)))
                                            {
                                                File.Copy(file, Path.Combine(gamefolder, folderName, subfolderName, fileName));
                                            }
                                            else
                                            {
                                                if (File.GetLastWriteTime(file) > File.GetLastWriteTime(Path.Combine(gamefolder, folderName, subfolderName, fileName)))
                                                {
                                                    File.Copy(file, Path.Combine(gamefolder, folderName, subfolder));
                                                }
                                            }
                                        }
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }
                progress.End();
                ejectPlaydate();
                BeginInvoke(new Action(() =>
                {
                    this.Text = title;
                }));
            }
            catch
            {
                ejectPlaydate();
                BeginInvoke(new Action(() =>
                {
                    this.Text = title;
                }));
            }
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFile(
         string lpFileName,
         uint dwDesiredAccess,
         uint dwShareMode,
         IntPtr SecurityAttributes,
         uint dwCreationDisposition,
         uint dwFlagsAndAttributes,
         IntPtr hTemplateFile
        );

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped
        );

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            byte[] lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        private IntPtr handle = IntPtr.Zero;

        const uint GENERIC_READ = 0x80000000;
        const uint GENERIC_WRITE = 0x40000000;
        const int FILE_SHARE_READ = 0x1;
        const int FILE_SHARE_WRITE = 0x2;
        const int FSCTL_LOCK_VOLUME = 0x00090018;
        const int FSCTL_DISMOUNT_VOLUME = 0x00090020;
        const int IOCTL_STORAGE_EJECT_MEDIA = 0x2D4808;
        const int IOCTL_STORAGE_MEDIA_REMOVAL = 0x002D4804;

        /// <summary>
        /// Constructor for the USBEject class
        /// </summary>
        /// <param name="driveLetter">This should be the drive letter. Format: F:/, C:/..</param>

        public IntPtr USBEject(string driveLetter)
        {
            string filename = @"\\.\" + driveLetter[0] + ":";
            return CreateFile(filename, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, 0x3, 0, IntPtr.Zero);
        }

        public bool Eject(IntPtr handle)
        {
            bool result = false;

            if (DismountVolume(handle))
            {
                PreventRemovalOfVolume(handle, false);
                result = AutoEjectVolume(handle);
            }
            CloseHandle(handle);
            return result;
        }

        private bool PreventRemovalOfVolume(IntPtr handle, bool prevent)
        {
            byte[] buf = new byte[1];
            uint retVal;

            buf[0] = (prevent) ? (byte)1 : (byte)0;
            return DeviceIoControl(handle, IOCTL_STORAGE_MEDIA_REMOVAL, buf, 1, IntPtr.Zero, 0, out retVal, IntPtr.Zero);
        }

        private bool DismountVolume(IntPtr handle)
        {
            uint byteReturned;
            return DeviceIoControl(handle, FSCTL_DISMOUNT_VOLUME, IntPtr.Zero, 0, IntPtr.Zero, 0, out byteReturned, IntPtr.Zero);
        }

        private bool AutoEjectVolume(IntPtr handle)
        {
            uint byteReturned;
            return DeviceIoControl(handle, IOCTL_STORAGE_EJECT_MEDIA, IntPtr.Zero, 0, IntPtr.Zero, 0, out byteReturned, IntPtr.Zero);
        }

        private bool CloseVolume(IntPtr handle)
        {
            return CloseHandle(handle);
        }

        private void ejectPlaydate()
        {
            try
            {
                if (cansync)
                {
                    string driveletter = "";
                    foreach (var item in System.IO.DriveInfo.GetDrives())
                    {
                        if (item.VolumeLabel == "PLAYDATE")
                        {
                            driveletter = item.Name;
                        }
                    }
                    handle = USBEject(driveletter);
                    Eject(handle);
                    syncing = false;
                    showPluggedUI();
                }
            }
            catch { }
        }

        private void Quit()
        {
            if (!syncing)
            {
                ejectPlaydate();
                try
                {
                    Directory.Delete(tempfolder, true);
                }
                catch { }
                Environment.Exit(0);
            }
            else
            {
                MessageBox.Show("Cannot close window. Device is syncing.", "Device is syncing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!cansync)
            {
                if (getPlaydatePort() != "" && pluggedin == false)
                {
                    this.Show();
                    string versioninfo = sendSerial(getPlaydatePort(), "version");
                    string battery = sendSerial(getPlaydatePort(), "batpct");
                    string version = "";
                    string serial = "";
                    foreach (string line in versioninfo.Split('\n'))
                    {
                        if (line.StartsWith("SDK="))
                        {
                            version = line.Replace("SDK=", "");
                        }
                        if (line.StartsWith("serial#="))
                        {
                            serial += " " + line.Replace("serial#=", "");
                        }
                    }
                    foreach (string line in battery.Split('\n'))
                    {
                        if (line.StartsWith("PCT="))
                        {
                            battery = line.Split('=')[1];
                        }
                    }
                    battery = battery.Split('.')[0];
                    pluggedin = true;
                    changePlug(serial, battery, version);
                }
                else if (getPlaydatePort() == "" && pluggedin == true)
                {
                    pluggedin = false;
                    changePlug();
                }
                else
                {
                    foreach (var item in System.IO.DriveInfo.GetDrives())
                    {
                        if (item.VolumeLabel == "PLAYDATE" && pluggedin == false)
                        {
                            pluggedin = true;
                            changePlug();
                            richTextBox1.Text = deviceinfo;
                            cansync = true;
                        }
                    }
                }
            }
            else
            {
                try
                {
                    bool playdateexists = false;
                    foreach (var item in System.IO.DriveInfo.GetDrives())
                    {
                        if (item.VolumeLabel == "PLAYDATE")
                        {
                            playdateexists = true;
                        }
                    }
                    if (!playdateexists)
                    {
                        cansync = false;
                        pluggedin = false;
                    }
                }
                catch { }
            }
        }

        private void dataDiskMode()
        {
            try
            {
                sendSerial(getPlaydatePort(), "datadisk");
            }
            catch { }
        }

        private void beforeAction()
        {
            ejectPlaydate();
            syncing = true;
            showPluggedUI();
            dataDiskMode();
            Thread.Sleep(1000);
            cansync = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            beforeAction();
            RuProgressBar ruProgressBar = new RuProgressBar(Text = "Syncing...");
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(syncToDevice), ruProgressBar);
            ruProgressBar.ShowDialog();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            beforeAction();
            RuProgressBar ruProgressBar = new RuProgressBar(Text = "Syncing...");
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(syncFromDevice), ruProgressBar);
            ruProgressBar.ShowDialog();
        }

        private void readGamesFromDevice(object callback)
        {
            // Read games from device, but also read User, Seasons and Purchased. Then read the game names to the listbox.
            try
            {
                IProgressCallback progress = callback as IProgressCallback;
                List<string> devicegames = new List<string>();
                if (cansync)
                {
                    string gamefolder = config["paths"]["gamefolder"];
                    string playdatedrive = "";
                    foreach (var item in System.IO.DriveInfo.GetDrives())
                    {
                        if (item.VolumeLabel == "PLAYDATE")
                        {
                            playdatedrive = item.Name;
                        }
                    }
                    if (playdatedrive != "")
                    {
                        // This is not a file operation
                        string[] folders = Directory.GetDirectories(playdatedrive);
                        foreach (string folder in folders)
                        {
                            string usergames = Path.Combine(playdatedrive, "Games\\User");
                            string seasonsgames = Path.Combine(playdatedrive, "Games\\Seasons");
                            string purchasedgames = Path.Combine(playdatedrive, "Games\\Purchased");
                            string[] folders2 = Directory.GetDirectories(usergames);
                            foreach (string folder2 in folders2)
                            {
                                // Get game name from pdxinfo
                                string[] pdxinfo = File.ReadAllLines(Path.Combine(folder2, "pdxinfo"));
                                string gamename = "";
                                foreach (string line in pdxinfo)
                                {
                                    if (line.StartsWith("name="))
                                    {
                                        gamename = line.Replace(" ", "").Replace("name=", "");
                                    }
                                }
                            }
                            string[] folders3 = Directory.GetDirectories(seasonsgames);
                            foreach (string folder3 in folders3)
                            {
                                // Get game name from pdxinfo
                                string[] pdxinfo = File.ReadAllLines(Path.Combine(folder3, "pdxinfo"));
                                string gamename = "";
                                foreach (string line in pdxinfo)
                                {
                                    if (line.StartsWith("name="))
                                    {
                                        gamename = line.Replace(" ", "").Replace("name=", "");
                                    }
                                }
                                devicegames.Add(gamename);
                            }
                            string[] folders4 = Directory.GetDirectories(purchasedgames);
                            foreach (string folder4 in folders4)
                            {
                                // Get game name from pdxinfo
                                string[] pdxinfo = File.ReadAllLines(Path.Combine(folder4, "pdxinfo"));
                                string gamename = "";
                                foreach (string line in pdxinfo)
                                {
                                    if (line.StartsWith("name="))
                                    {
                                        gamename = line.Replace(" ", "").Replace("name=", "");
                                    }
                                }
                                devicegames.Add(gamename);
                            }
                            string[] folders5 = Directory.GetDirectories(playdatedrive + "\\Games");
                            foreach (string folder5 in folders5)
                            {
                                if (folder5 != "User" && folder5 != "Seasons" && folder5 != "Purchased")
                                {
                                    // Get game name from pdxinfo
                                    string[] pdxinfo = File.ReadAllLines(Path.Combine(folder5, "pdxinfo"));
                                    string gamename = "";
                                    foreach (string line in pdxinfo)
                                    {
                                        if (line.StartsWith("name="))
                                        {
                                            gamename = line.Replace(" ", "").Replace("name=", "");
                                        }
                                    }
                                    devicegames.Add(gamename);
                                }
                            }
                        }
                    }
                }
                progress.End();
                ejectPlaydate();
                string[] arrdevicegames = devicegames.ToArray();
                BeginInvoke(new Action(() =>
                {
                    this.Text = title;
                    listBox1.Items.Clear();
                    foreach (string game in arrdevicegames)
                    {
                        listBox1.Items.Add(game);
                    }
                }));
            }
            catch
            {
                ejectPlaydate();
                BeginInvoke(new Action(() =>
                {
                    this.Text = title;
                }));
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            beforeAction();
            RuProgressBar ruProgressBar = new RuProgressBar(Text = "Syncing...");
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(readGamesFromDevice), ruProgressBar);
            ruProgressBar.ShowDialog();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            try
            {
                beforeAction();
                string drivename = "";
                foreach (var item in System.IO.DriveInfo.GetDrives())
                {
                    if (item.VolumeLabel == "PLAYDATE")
                    {
                        drivename = item.Name;
                    }
                }
                string screenshotfolder = Path.Combine(drivename, "Screenshots");
                tempfolder = Path.Combine(Path.GetTempPath(), "PlaySync");
                if (!Directory.Exists(tempfolder))
                {
                    Directory.CreateDirectory(tempfolder);
                }
                foreach (string file in Directory.GetFiles(screenshotfolder))
                {
                    string fileName = file.Replace(screenshotfolder + "\\", "");
                    if (!File.Exists(Path.Combine(tempfolder, fileName)))
                    {
                        File.Copy(file, Path.Combine(tempfolder, fileName));
                    }
                    else
                    {
                        if (File.GetLastWriteTime(file) > File.GetLastWriteTime(Path.Combine(tempfolder, fileName)))
                        {
                            File.Copy(file, Path.Combine(tempfolder, fileName), true);
                        }
                    }
                }
                launchUrl("explorer.exe " + tempfolder);
                ejectPlaydate();
            }
            catch { }
        }

        private void diskModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string drivename = "";
            foreach (var item in System.IO.DriveInfo.GetDrives())
            {
                if (item.VolumeLabel == "PLAYDATE")
                {
                    drivename = item.Name;
                }
            }
            if (drivename == "")
            {
                dataDiskMode();
            }
            else
            {
                ejectPlaydate();
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
            this.Show();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Quit();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            Process[] pname = Process.GetProcessesByName("PlaySync");
            if (pname.Length > 1)
            {
                Environment.Exit(0);
            }
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
        }

        private void toDeviceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button4_Click(sender, e);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            try
            {
                if (toolStripMenuItem2.Checked)
                {
                    RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    rk.DeleteValue("PlaySync", false);
                    toolStripMenuItem2.Checked = false;
                }
                else
                {
                    RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    rk.SetValue("PlaySync", "\"" + Application.ExecutablePath + "\" --autostart");
                    toolStripMenuItem2.Checked = true;
                }
            }
            catch { }
        }
    }
}
