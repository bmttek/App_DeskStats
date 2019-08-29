using APP_DeskStats.Functions;
using DLL_Support;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace APP_DeskStats
{
    public partial class Form1 : Form
    {
        internal int timerHide;
        internal IniDataCaseInsensitive dataSettings;
        controlStats cStats = new controlStats();
        controlSettings cS = new controlSettings();
        List<modelLocation> lMl;
        private modelStat _mUd;
        string dirCommon;
        public modelStat mUd
        {
            get{return _mUd; }
            set{ _mUd = value; }
        }
        private  string _error;
        public  string error
        {
            get { return _error; }
            set { _error = value; }
        }
        public string loc
        {
            get { return lbLocation.Text; }
            set { lbLocation.Text = value; }
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timerHide = 0;
            string currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            dirCommon = Path.GetFullPath(Path.Combine(currentPath, ".."));
            dirCommon = Path.GetFullPath(Path.Combine(dirCommon, "Common"));
            if (File.Exists($"{dirCommon}\\Images\\keyboard-right-arrow-button.png"))
            {
                pbHide.Text = "";
                pbHide.Tag = "Hide";
                Image img = Image.FromFile($"{dirCommon}\\Images\\keyboard-right-arrow-button.png");
                img = (Image)new Bitmap(img,25,52);
                pbHide.Image = img;
            }
            else
            {
                pbHide.Tag = "Hide";
                pbHide.Text = "Hide";
            }
                
            error = "";
            dataSettings = cS.getSettings("Library Helper Suite", "DeskStats", "false", "STATS");
            lMl = new List<modelLocation>();
            mUd = new modelStat();
            classFunctions cF = new classFunctions();
            lMl = (List<modelLocation>)cStats.statControl(dataSettings, controlStats.getLocations, "STATS", "LoadForm",this, null, "", "", false, 0);
            mUd= (modelStat)cStats.statControl(dataSettings, controlStats.getUserPreferences, "STATS", "LoadForm",this, null, "", "", true, 0);
            lbLocation.Text = cF.locIDtoString(lMl, mUd.id_loc);
            cF.setFormButtons(lMl, mUd.id_loc, this);
            //this.Hide();
            //Icon icon = new Icon("piechart.ico");
           // notifyIcon1.Icon = icon;
            timer1.Interval = 1000;
            timer1.Start();
            if (error.ToLower().Contains("error")) { MessageBox.Show("Error inside application.  Contact IT.  This application will now close"); this.Close(); }
        }

        private void btChangeLoc_Click(object sender, EventArgs e)
        {
            FormsetUserPreferences formSuP = new FormsetUserPreferences();
            formSuP.lMl = lMl;
            formSuP.dataSettings = dataSettings;
            formSuP.upload = false;
            formSuP.mU = mUd;
            timer1.Stop();
            formSuP.ShowDialog();
            classFunctions cF = new classFunctions();
            cF.setFormButtons(lMl, mUd.id_loc, this);
            timer1.Start();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            timerHide = 0;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (mUd.autohide_enable == 1)
            {
                if (timerHide > mUd.autohide_timeout)
                {
                    if ((string)pbHide.Tag == "Hide")
                    {
                        Screen rightmost = Screen.AllScreens[0];
                        foreach (Screen screen in Screen.AllScreens)
                        {
                            if (screen.WorkingArea.Right > rightmost.WorkingArea.Right)
                                rightmost = screen;
                        }
                        this.Left = rightmost.WorkingArea.Right - 30;
                        this.Top = rightmost.WorkingArea.Bottom - this.Height;
                        if (File.Exists($"{dirCommon}\\Images\\keyboard-left-arrow-button.png"))
                        {
                            Image img = Image.FromFile($"{dirCommon}\\Images\\keyboard-left-arrow-button.png");
                            img = (Image)new Bitmap(img, 25, 52);
                            pbHide.Image = img; pbHide.Text = "";
                            pbHide.Tag = "Show";
                            pbHide.Update();
                        }
                        else
                        {
                            pbHide.Text = "Show";
                            pbHide.Tag = "Show";
                        }
                    }
                }
                timerHide++;
            }
        }

        private void Form1_MouseHover(object sender, EventArgs e)
        {
            timerHide = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((string)pbHide.Tag == "Hide")
            {
                Screen rightmost = Screen.AllScreens[0];
                foreach (Screen screen in Screen.AllScreens)
                {
                    if (screen.WorkingArea.Right > rightmost.WorkingArea.Right)
                        rightmost = screen;
                }
                this.Left = rightmost.WorkingArea.Right - 30;
                this.Top = rightmost.WorkingArea.Bottom - this.Height;

                if (File.Exists($"{dirCommon}\\Images\\keyboard-left-arrow-button.png"))
                {
                    pbHide.Tag = "Show";
                    Image img = Image.FromFile($"{dirCommon}\\Images\\keyboard-left-arrow-button.png");
                    img = (Image)new Bitmap(img, 25, 52);
                    pbHide.Image = img; pbHide.Text = "";
                }
                else
                {
                    pbHide.Tag = "Show";
                    pbHide.Text = "Show";
                }
            }
            else
            {
                Screen rightmost = Screen.AllScreens[0];
                foreach (Screen screen in Screen.AllScreens)
                {
                    if (screen.WorkingArea.Right > rightmost.WorkingArea.Right)
                        rightmost = screen;
                }
                this.Left = rightmost.WorkingArea.Right - this.Width;
                this.Top = rightmost.WorkingArea.Bottom - this.Height;
                if (File.Exists($"{dirCommon}\\Images\\keyboard-right-arrow-button.png"))
                {
                    pbHide.Tag = "Hide";
                    timerHide = 0;
                    pbHide.Text = "";
                    Image img = Image.FromFile($"{dirCommon}\\Images\\keyboard-right-arrow-button.png");
                    img = (Image)new Bitmap(img, 25, 52);
                    pbHide.Image = img;
                }
                else
                {
                    pbHide.Tag = "Hide";
                    pbHide.Text = "Hide";
                    timerHide = 0;
                }
            }
        }

        private void btnInputStats_Click(object sender, EventArgs e)
        {
            frmInputStats fInputStats = new frmInputStats();
            fInputStats.dataSettings = dataSettings;
            fInputStats.mPs = mUd;
            fInputStats.ShowDialog();
        }

        private void btnGetStats_Click(object sender, EventArgs e)
        {
            frmGetStats fGetStats = new frmGetStats();
            fGetStats.dataSettings = dataSettings;
            fGetStats.mPs = mUd;
            fGetStats.ShowDialog();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
