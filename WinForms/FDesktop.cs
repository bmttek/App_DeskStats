using DLL_Support;
using DLL_Support.Config;
using DLL_Support.ILS;
using DLL_Support.Operation;
using DLL_Support.Stats;
using DLL_Support.Stats.API;
using DLL_Support.Util;
using DLL_Support.WinForms;
using DLL_Support.Worker;
using Ninject.Parameters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace APP_DeskStats.WinForms
{
    public partial class FDesktop : FormBase
    {
        #region Dependencies
        internal int timerHide;
        internal StatUser statUser;
        internal List<Location> locations;
        internal string dirCommon;
        private readonly IConfigManager configManager;
        private readonly NotificationManager notify;
        private readonly CultureInitializer cultureInitializer;
        private readonly IOperationFactory operationFactory;
        private readonly IWorkerServiceFactory workerServiceFactory;
        private readonly IIlsProvider ilsProvider;
        private readonly IStatProvider statProvider;
        private readonly IFormFactory formFactory;
        private readonly ChangeTracker changeTracker;
        private readonly IOperationProgress operationProgress;
        private readonly IErrorOutput errorOutput;
        private readonly ApiStatsWrapper apiStatWrapper;
        internal System.Windows.Forms.Timer timer;
        #endregion
        #region State
        private bool closed = false;
        private LayoutManager layoutManager;
        #endregion
        #region Initialization
        public FDesktop(IFormFactory formFactory, IStatProvider statProvider, IErrorOutput errorOutput, IIlsProvider ilsProvider, IConfigManager configManager, NotificationManager notify, IOperationProgress operationProgress, ChangeTracker changeTracker)
        {
            this.configManager = configManager;
            InitializeComponent();
            this.notify = notify;
            this.operationProgress = operationProgress;
            this.statProvider = statProvider;
            this.changeTracker = changeTracker;
            notify.ParentForm = this;
            this.formFactory = formFactory;
            this.errorOutput = errorOutput;
            this.ilsProvider = ilsProvider;
            statUser = new StatUser();
            locations = new List<Location>();
            apiStatWrapper = new ApiStatsWrapper(errorOutput,configManager,formFactory);
            FormClosing += FDesktop_FormClosing;
            Closed += FDesktop_Closed;
        }
        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            PostInitializeComponent();
        }
        private void PostInitializeComponent()
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
                img = (Image)new Bitmap(img, 25, 52);
                pbHide.Image = img;
            }
            else
            {
                pbHide.Tag = "Hide";
                pbHide.Text = "Hide";
            }
            statUser.user_name = Environment.UserName;
            statUser.user_domain = Environment.UserDomainName;
            statUser = apiStatWrapper.GetUser(statUser);
            locations = apiStatWrapper.GetLocations();
            lbLocation.Text = locIDtoString(locations,statUser.id_loc);
            setFormButtons(locations,statUser.id_loc);
            timer1.Interval = 1000;
            timer1.Start();
        }
        private void FDesktop_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (closed) return;

            if (operationProgress.ActiveOperations.Any())
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    if (operationProgress.ActiveOperations.Any(x => !x.SkipExitPrompt))
                    {
                        var result = MessageBox.Show($"Do you want to exit with active operations?", $"Active Operations",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                        if (result != DialogResult.Yes)
                        {
                            e.Cancel = true;
                        }
                    }
                }
                else
                {
                }
            }
            else if (changeTracker.HasUnsavedChanges)
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    var result = MessageBox.Show($"Do you want to exit with unsaved changes?", $"Unsaved changes",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.Yes)
                    {
                        changeTracker.Clear();
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }
            }

            if (!e.Cancel && operationProgress.ActiveOperations.Any())
            {
                operationProgress.ActiveOperations.ForEach(op => op.Cancel());
                e.Cancel = true;
                Hide();
                ShowInTaskbar = false;
                Task.Factory.StartNew(() =>
                {
                    var timeoutCts = new CancellationTokenSource();
                    timeoutCts.CancelAfter(TimeSpan.FromSeconds(60));
                    try
                    {
                        operationProgress.ActiveOperations.ForEach(op => op.Wait(timeoutCts.Token));
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    closed = true;
                    SafeInvoke(Close);
                });
            }
        }

        private void FDesktop_Closed(object sender, EventArgs e)
        {
            //SaveToolStripLocation();
            Pipes.KillServer();
            closed = true;
        }
        #endregion
        #region Functions
        public string locIDtoString(List<Location> lMl, int id)
        {
            foreach (Location mL in lMl)
            {
                if (mL.id == id) { return mL.loc_name; }
            }
            return "NONE";
        }
        public List<string> getButtonList(List<Location> lMl, int id)
        {
            List<string> lStr = new List<string>();
            foreach (Location mL in lMl)
            {
                if (mL.id == id)
                {
                    lStr = mL.loc_properties;
                }
            }
            return lStr;
        }
        private void DynamicButton_Click(object sender, EventArgs e, Form passedForm)
        {
            //Form1 frmMain = (Form1)Application.OpenForms["Form1"];
            timerHide = 0;
            StatMessage statMessage = new StatMessage()
            {
                cpu_name = Environment.MachineName,
                stat_user = statUser,
                created_by = statUser.user_name,
                stat_date = DateTime.Now,
                stat_type = (sender as Button).Text.Split('(')[0].Trim(),
                stat_comment = ""
            };
            if ((string)(sender as Button).Tag == "time")
            {
                FComment fComment = FormFactory.Create<FComment>("number", "Amount of time spent:");
                if (fComment.ShowDialog() == DialogResult.OK)
                {
                    statMessage.stat_comment = fComment.textBox1.Text;
                }
            }
            if ((string)(sender as Button).Tag == "string")
            {
                FComment fComment = FormFactory.Create<FComment>("", "Enter Comment:");
                if (fComment.ShowDialog() == DialogResult.OK)
                {
                    statMessage.stat_comment = fComment.textBox1.Text;
                }
            }
            apiStatWrapper.PostStat(statMessage);
            if (BackColor == Color.Blue) { BackColor = Color.White; }
            else if (BackColor == Color.White) { BackColor = Color.Blue; }
            string strCount = (sender as Button).Text.Split('(')[1].Replace(")", string.Empty);
            int countButton = 0;
            if (int.TryParse(strCount, out countButton))
            {
                countButton++;
            }
            (sender as Button).Text = $"{statMessage.stat_type}({countButton.ToString()})";
        }
        public void setFormButtons(List<Location> lMl, int id)
        {
            Width = 325;
            Height = 68;
            Controls.Remove(Controls["tlMain"]);
            TableLayoutPanel tlMain = new TableLayoutPanel();
            tlMain.Location = new Point(30, 29);
            tlMain.Size = new Size(135, 34);
            tlMain.AutoSize = true;
            tlMain.Name = "tlMain";
            tlMain.ColumnCount = 0;
            tlMain.RowCount = 1;
            tlMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlMain.GrowStyle = TableLayoutPanelGrowStyle.AddColumns;

            List<string> lStr = getButtonList(lMl, id);
            int counter = 0;
            int widthForm = 30;
            int widthTable = 20;
            foreach (string str in lStr)
            {

                Button b1 = new Button();
                if (str.ToLower().Contains("(time)")) { b1.Tag = "time"; b1.Text = str.Replace("(time)", string.Empty); }
                else if (str.ToLower().Contains("(string)")) { b1.Tag = "string"; b1.Text = str.Replace("(string)", string.Empty); }
                else { b1.Text = str; }
                b1.Text = $"{b1.Text}(0)";
                b1.AutoSize = true;
                b1.Height = 30;
                b1.BackColor = Color.LightGray;
                b1.Font = new System.Drawing.Font("Arial", 13, FontStyle.Regular);
                b1.Click += delegate (object sender, EventArgs e) { DynamicButton_Click(sender, e, this); };
                tlMain.Controls.Add(b1, counter, 0);
                widthForm += b1.Width += 9;
                widthTable += b1.Width += 9;
                if (Width < widthForm) { Width = widthForm; }
                if (tlMain.Width < widthTable) { tlMain.Width = widthTable; }
                counter++;
            }
            Controls.Add(tlMain);
            Screen rightmost = Screen.AllScreens[0];
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.WorkingArea.Right > rightmost.WorkingArea.Right)
                    rightmost = screen;
            }

            Left = rightmost.WorkingArea.Right - Width;
            Top = rightmost.WorkingArea.Bottom - Height;

        }
        #endregion


        public FDesktop()
        {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void PbHide_Click(object sender, EventArgs e)
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
        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (statUser.autohide_enable == 1)
            {
                if (timerHide > statUser.autohide_timeout)
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
        private void FDesktop_MouseHover(object sender, EventArgs e)
        {
            timerHide = 0;
        }

        private void BtChangeLoc_Click(object sender, EventArgs e)
        {
            FSettings fSettings = FormFactory.Create<FSettings>(locations,statUser);
            if (fSettings.ShowDialog() == DialogResult.OK)
            {
                statUser = fSettings.statUser;
                lbLocation.Text = fSettings.location;
            }
        }

        private void BtnInputStats_Click(object sender, EventArgs e)
        {
            FInputStats fInputStats = FormFactory.Create<FInputStats>(locations, statUser);
            fInputStats.ShowDialog();
        }

        private void btnGetStats_Click(object sender, EventArgs e)
        {
            FGetStats fGetStats = FormFactory.Create<FGetStats>(locations, statUser);
            fGetStats.ShowDialog();
        }
    }
}
