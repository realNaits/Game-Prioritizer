﻿using Microsoft.Win32;
using System;
using System.Activities;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Game_Prioritizer
{
    public partial class Form1 : Form
    {
        public System.Drawing.Point lastLoc;
        public System.Drawing.Size lastSize;

        Settings settings;
        Updater updater;
        Run run;
        Log log;

        //Directories
        public static string APPDATA;
        public static string EXEC_PATH;
        public static string WORK_DIR;

        //ArrayList for class Run
        public ArrayList games = new ArrayList();

        //Localtimer
        Timer upCheck = new Timer();

        public Form1()
        {
            //Normal Initialize
            InitializeComponent();

            //THIS
            run = new Run(this);
            settings = new Settings(this);
            log = new Log(this);
            updater = new Updater(this);

            //Creating directory
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string specificFolder = Path.Combine(folder, "GamePrioritizer");
            Directory.CreateDirectory(specificFolder);
            if (!System.IO.File.Exists(specificFolder + "\\data.xml"))
            {
                System.IO.File.Create(specificFolder + "\\data.xml").Dispose();
            }
            APPDATA = specificFolder.ToString();

            //EXEC PATH
            EXEC_PATH = System.Reflection.Assembly.GetEntryAssembly().Location;
            //WORK APPDATA
            WORK_DIR = Path.GetDirectoryName(Application.ExecutablePath);

            //Getting version.
            toolLabelVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            //Initialize external timers
            run.initTimer();
            log.initTimer();

            //Checking for update
            pictureUpdate.Visible = false;
            checkForUpdate();    

            //ListBox settings
            gameList.FormattingEnabled = true;
            gameList.HorizontalScrollbar = true;
            gameList.ScrollAlwaysVisible = true;

            //Loading and so on..
            loadGameList();
            settings.loadSettings();

            //AutoStart
            autoStart();

            //Init localtimer
            upCheck.Interval = 1800000;
            upCheck.Start();
            upCheck.Tick += UpCheck_Tick;
        }

        private void UpCheck_Tick(object sender, EventArgs e)
        {
            checkForUpdate();
        }

        public void checkForUpdate()
        {
            if (updater.checkUpdate())
            {
                sendLogData(1, "Found new update.");
                pictureUpdate.Visible = true;
            }
        }

        public static Version getVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        public Boolean reEnableAdd
        {
            set { this.buttonAdd.Enabled = true; }
        }

        public Boolean reEnableEdit
        {
            set { this.buttonEdit.Enabled = true; }
        }

        public void SetGameText(string name, Color color)
        {
            labelGameRunning.ForeColor = color;
            labelGameRunning.Text = name;
        }

        public string getGameText()
        {
            return labelGameRunning.Text;
        }

        public void addGame(String name, String path, ProcessPriorityClass priority)
        {
            if (name == null || path == null)
            {
                MessageBox.Show("Something wrong happened while adding the game... Please try again. " +
                    "If the problem repeats itself, please contact the creator of this application.",
                    "Something went wrong...", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            gameList.Items.AddRange(new object[] {
                name + ", " + priority.ToString() + ", " + path,
            });
            sendLogData(1, "Added: " + name + " to game list.");
        }

        public void updateChecker()
        {
            if (gameList.Items.Count > 0)
            {
                for (int i = 0; i < gameList.Items.Count; i++)
                {
                    games.Add(gameList.Items[i].ToString());
                }
            }
        }

        public void saveGameList()
        {
            string sPath = APPDATA + "\\gameList.txt";

            try
            {
                StreamWriter SaveFile = new StreamWriter(sPath);
                foreach (var item in gameList.Items)
                {
                    SaveFile.WriteLine(item.ToString());
                }

                SaveFile.Close();
                sendLogData(1, "Game list saved.");
            }
            catch (Exception e)
            {
                sendLogData(3, "Message: " + e);
            }
        }

        public void loadGameList()
        {
            try
            {
                string lPath = APPDATA + "\\gameList.txt";
                string line;
                StreamReader LoadFile = new StreamReader(lPath);
                while ((line = LoadFile.ReadLine()) != null)
                {
                    String[] split = line.Split(',');

                    String name = split[0];
                    String path = split[2].TrimStart(' ');

                    ProcessPriorityClass pri = ProcessPriorityClass.Normal;

                    String priority = split[1].TrimStart(' ');
                    if (priority == "High" || priority == "high")
                    {
                        pri = ProcessPriorityClass.High;
                    }
                    if (priority == "AboveNormal" || priority == "abovenormal")
                    {
                        pri = ProcessPriorityClass.AboveNormal;
                    }
                    if (priority == "Normal" || priority == "normal")
                    {
                        pri = ProcessPriorityClass.Normal;
                    }

                    addGame(name, path, pri);
                }

                foreach (string game in gameList.Items)
                {
                    string[] split = game.Split(' ');
                    string name = split[0];
                }

                LoadFile.Close();
                sendLogData(1, "Game list loaded.");
            }
            catch(Exception e)
            {
                sendLogData(3, "Message: " + e);
            }
        }

        public void removeGame()
        {
            if (gameList.SelectedItems == null)
            {
                return;
            }

            ListBox.SelectedObjectCollection selectedItems = new ListBox.SelectedObjectCollection(gameList);
            selectedItems = gameList.SelectedItems;

            for (int i = selectedItems.Count - 1; i >= 0; i--)
                gameList.Items.Remove(selectedItems[i]);

            sendLogData(1, "Removed: " + gameList.SelectedItem.ToString() + " from game list.");
        }

        public void removeGameAt(int row)
        {
            gameList.Items.RemoveAt(row);
            sendLogData(1, "Removed: " + gameList.SelectedItem.ToString() + " from game list.");
        }

        public void openWebsite()
        {
            System.Diagnostics.Process.Start("https://realnaits.com/");
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            buttonAdd.Enabled = false;
            Add addForm = new Add();
            addForm.Show();
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            if (gameList.SelectedItem == null)
            {
                MessageBox.Show("Something wrong happened while editing the game... Please try again. " +
                    "If the problem repeats itself, please contact the creator of this application.",
                    "Something went wrong...", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            String selected = gameList.SelectedItem.ToString();
            String[] split = selected.Split(',');

            String name = split[0];
            String path = split[2];
            int row = gameList.SelectedIndex;
            ProcessPriorityClass pri = ProcessPriorityClass.Normal;

            String priority = split[1].TrimStart(' ');
            if (priority == "High" || priority == "high")
            {
                pri = ProcessPriorityClass.High;
            }
            if (priority == "AboveNormal" || priority == "abovenormal")
            {
                pri = ProcessPriorityClass.AboveNormal;
            }
            if (priority == "Normal" || priority == "normal")
            {
                pri = ProcessPriorityClass.Normal;
            }

            buttonEdit.Enabled = false;
            Edit edit = new Edit(name.TrimStart(' '), path.TrimStart(' '), pri, row);
            edit.Show();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (gameList.SelectedItem == null)
            {
                MessageBox.Show("Something wrong happened while deleting the game... Please try again. " +
                    "If the problem repeats itself, please contact the creator of this application.",
                    "Something went wrong...", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult dr = MessageBox.Show("You sure you want to remove " + gameList.SelectedItem.ToString().Split(',')[0], "Confirm", MessageBoxButtons.YesNo);

            if (dr == DialogResult.Yes)
            {
                removeGame();
            }
        }

        private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            openWebsite();
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListBox.SelectedObjectCollection selectedItems = new ListBox.SelectedObjectCollection(gameList);
            selectedItems = gameList.SelectedItems;

            for (int i = selectedItems.Count - 1; i >= 0; i--)
                gameList.Items.Remove(selectedItems[i]);
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            openWebsite();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            lastSize = this.Size;
            lastLoc = this.Location;
            saveGameList();
            settings.saveSettings();
            sendLogData(1, "Exited");
            log.saveToFile();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            start();
        }
        public void start()
        {
            if (gameList.Items.Count < 1)
            {
                MessageBox.Show("You must add a game before you can start.", "Error!", MessageBoxButtons.OK);
                return;
            }

            updateChecker();

            run.startTimers();
            pictureStatus.BackColor = Color.Green;
            buttonStart.Enabled = false;
            buttonStop.Enabled = true;
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            stop();
        }

        public void stop()
        {
            run.stopTimers();
            pictureStatus.BackColor = Color.DarkRed;
            buttonStart.Enabled = true;
            buttonStop.Enabled = false;
        }

        private void pictureUpdate_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Do you want to update?", "Update?", MessageBoxButtons.YesNo);

            if (dr == DialogResult.Yes)
            {
                Process.Start(WORK_DIR + "\\Game Prioritizer Updater.exe");
                this.Close();
            }
        }

        private void pictureStatus_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();

            if (pictureStatus.BackColor == Color.Green)
            {
                tt.SetToolTip(this.pictureStatus, "Status: Running");
            }
            else
            {
                tt.SetToolTip(this.pictureStatus, "Status: Stopped");
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.Size = lastSize;
            this.Location = lastLoc;

            if (checkMini.Checked == true && checkStartup.Checked == true)
            {
                WindowState = FormWindowState.Minimized;
                Hide();
            }
        }

        public void autoStart()
        {
            if (checkAuto.Checked)
            {
                start();
            }
        }

        private void checkStartup_CheckedChanged(object sender, EventArgs e)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (checkStartup.Checked)
            {
                rk.SetValue("GamePrioritizer", EXEC_PATH);
            }
            else
            {
                rk.DeleteValue("GamePrioritizer", false);
            }
        }

        /// <summary>
        /// Sending data to log.
        /// </summary>
        /// <param name="type">Info=1|Warning=2|Error=3</param>
        /// <param name="msg">String message</param>
        public void sendLogData(int type, string msg)
        {
            log.printLog(type, msg);
        }

        private void textLog_VisibleChanged(object sender, EventArgs e)
        {
            if (textLog.Visible)
            {
                textLog.SelectionStart = textLog.TextLength;
                textLog.ScrollToCaret();
            }
        }

        private void buttonOpenDataDir_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", APPDATA);
        }

        private void buttonOpenLog_Click(object sender, EventArgs e)
        {
            Process.Start(APPDATA + "\\log.txt");
        }

        private void buttonClearLog_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Do you really want to clear the log file?", "Clear log file", MessageBoxButtons.YesNo);

            if (dr == DialogResult.Yes)
            {
                System.IO.File.WriteAllText(APPDATA + "\\log.txt", String.Empty);
                textLog.Clear();
            }
        }

        private void notifMenuOpen_Click(object sender, EventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void notifMenuCheckUpdate_Click(object sender, EventArgs e)
        {
            checkForUpdate();
        }

        private void notifMenuExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
