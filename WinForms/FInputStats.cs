using DLL_Support.Config;
using DLL_Support.ILS;
using DLL_Support.Operation;
using DLL_Support.Stats;
using DLL_Support.Stats.API;
using DLL_Support.Util;
using DLL_Support.WinForms;
using DLL_Support.Worker;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace APP_DeskStats.WinForms
{
    public partial class FInputStats : FormBase
    {
        #region Dependencies
        internal StatUser statUser;
        internal List<Location> locations;
        internal DataTable dtResults;
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
        #endregion
        #region State
        private bool closed = false;
        private LayoutManager layoutManager;
        #endregion
        #region Initialization
        public FInputStats(List<Location> locations, StatUser statUser, IFormFactory formFactory, IStatProvider statProvider, IErrorOutput errorOutput, IIlsProvider ilsProvider, IConfigManager configManager, NotificationManager notify, IOperationProgress operationProgress, ChangeTracker changeTracker)
        {
            this.locations = locations;
            this.statUser = statUser;
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
            dtResults = new DataTable();
            apiStatWrapper = new ApiStatsWrapper(errorOutput, configManager, formFactory);
            FormClosing += FInputStats_FormClosing;
            Closed += FInputStats_Closed;
        }
        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            PostInitializeComponent();
        }
        private void PostInitializeComponent()
        {
            cbInputYear.Items.Add("Select Year");
            cbInputYear.Items.Add(DateTime.Now.AddYears(-2).ToString("yyyy"));
            cbInputYear.Items.Add(DateTime.Now.AddYears(-1).ToString("yyyy"));
            cbInputYear.Items.Add(DateTime.Now.ToString("yyyy"));
            cbInputYear.Items.Add(DateTime.Now.AddYears(1).ToString("yyyy"));
            cbInputYear.Items.Add(DateTime.Now.AddYears(2).ToString("yyyy"));
            cbInputYear.Items.Add(DateTime.Now.AddYears(3).ToString("yyyy"));
            cbInputYear.Items.Add(DateTime.Now.AddYears(4).ToString("yyyy"));
            cbInputYear.SelectedIndex = 0;
            cbInputMonth.Items.Add("Select Month");
            cbInputMonth.Items.Add("January");
            cbInputMonth.Items.Add("February");
            cbInputMonth.Items.Add("March");
            cbInputMonth.Items.Add("April");
            cbInputMonth.Items.Add("May");
            cbInputMonth.Items.Add("June");
            cbInputMonth.Items.Add("July");
            cbInputMonth.Items.Add("August");
            cbInputMonth.Items.Add("September");
            cbInputMonth.Items.Add("October");
            cbInputMonth.Items.Add("November");
            cbInputMonth.Items.Add("December");
            cbInputMonth.SelectedIndex = 0;
            cbLocation.Items.Add("Select Location");
            foreach (Location mLoc in locations)
            {
                if (mLoc.loc_view == 1)
                {
                    cbLocation.Items.Add(mLoc.loc_name);
                }
            }
            cbLocation.SelectedIndex = 0;
        }
        private void FInputStats_FormClosing(object sender, FormClosingEventArgs e)
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

        private void FInputStats_Closed(object sender, EventArgs e)
        {
            //SaveToolStripLocation();
            Pipes.KillServer();
            closed = true;
        }
        #endregion
        private DataTable cb_Changed()
        {
            DataTable dtResults = new DataTable();
            if (cbInputMonth.SelectedIndex != 0 && cbInputYear.SelectedIndex != 0 && cbLocation.SelectedIndex != 0)
            {
                StatMessage statMessage = new StatMessage()
                {
                    cpu_name = Environment.MachineName
                };
                foreach (Location mLoc in locations)
                {
                    if (mLoc.loc_name.ToLower().Contains(cbLocation.GetItemText(cbLocation.SelectedItem).ToLower()))
                    {
                        statMessage.id_loc = mLoc.id;
                        statMessage.location = cbLocation.GetItemText(cbLocation.SelectedItem).ToLower();
                    }
                }
                statMessage.year = cbInputYear.GetItemText(cbInputYear.SelectedItem);
                statMessage.month = cbInputMonth.GetItemText(cbInputMonth.SelectedItem).ToLower();
                statMessage.stat_type = "GetMonthlyStat";
                dtResults = apiStatWrapper.GetMonthlyStats(statMessage);
            }
            return dtResults;
        }
        private void cbLocation_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cbInputMonth.SelectedIndex != 0 && cbInputYear.SelectedIndex != 0 && cbLocation.SelectedIndex != 0)
            {
                cbLocation.BackColor = Color.White;
                cbInputMonth.BackColor = Color.White;
                cbInputYear.BackColor = Color.White;
                this.Controls.Remove(this.Controls["tlStats"]);
                TableLayoutPanel tlStats = new TableLayoutPanel();
                tlStats.Location = new Point(15, 100);
                tlStats.Size = new Size(135, 34);
                tlStats.AutoSize = true;
                tlStats.Name = "tlStats";
                tlStats.ColumnCount = 3;
                tlStats.RowCount = 1;
                tlStats.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                tlStats.GrowStyle = TableLayoutPanelGrowStyle.AddRows;
                int counter = 0;
                foreach (Location mLoc in locations)
                {
                    if (mLoc.loc_name.ToLower().Contains(cbLocation.GetItemText(cbLocation.SelectedItem).ToLower()))
                    {
                        dtResults = cb_Changed();
                        foreach (string strin in mLoc.loc_stats)
                        {
                            if (!strin.ToLower().Contains("none"))
                            {
                                string strMatch = "";
                                Label l1 = new Label();
                                TextBox t1 = new TextBox();
                                if (strin.ToLower().Contains("(time)"))
                                {
                                    t1.Name = strin;
                                    t1.Tag = "time";
                                    strMatch = strin.Replace("(time)", string.Empty);
                                    l1.Text = strin.Replace("(time)", string.Empty);
                                }
                                else if (strin.ToLower().Contains("(string)"))
                                {
                                    t1.Width = 400;
                                    t1.Name = strin;
                                    t1.Tag = "string";
                                    l1.Text = strin.Replace("(string)", string.Empty);
                                    strMatch = strin.Replace("(string)", string.Empty);
                                }
                                else { l1.Text = strin; t1.Tag = ""; t1.Name = strin; strMatch = strin; }
                                t1.Enter += delegate (object sender1, EventArgs e1) { Textbox_Enter(sender, e, this, t1); };
                                t1.Leave += delegate (object sender1, EventArgs e1) { Textbox_Leave(sender, e, this, t1); };
                                foreach (DataRow dtRow in dtResults.Rows)
                                {
                                    if (dtRow["type"].ToString().ToLower().Contains(strMatch.ToLower()))
                                    {
                                        try
                                        {
                                            if (t1.Tag.ToString().ToLower().Contains("string"))
                                            {
                                                t1.Text = dtRow["comment"].ToString();
                                                if (statUser.monthly_admin != 1)
                                                {
                                                    t1.Enabled = false;
                                                }
                                            }
                                            else
                                            {
                                                t1.Text = dtRow["count"].ToString();
                                                if (statUser.monthly_admin != 1)
                                                {
                                                    t1.Enabled = false;
                                                }
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            t1.Text = "";
                                            if (statUser.monthly_admin != 1)
                                            {
                                                t1.Enabled = false;
                                            }
                                        }
                                    }
                                }
                                if (t1.Text.Length < 1)
                                {
                                    t1.Text = "Not Reported";
                                }
                                l1.AutoSize = true;
                                Label la = new Label();
                                tlStats.AutoSize = true;
                                l1.Font = new Font("Arial", 13, FontStyle.Regular);
                                t1.Font = new Font("Arial", 13, FontStyle.Regular);
                                la.Font = new Font("Arial", 9, FontStyle.Regular);
                                la.AutoSize = true;
                                foreach (string sstrAuto in mLoc.auto_pull_stats)
                                {
                                    if (strin.ToUpper()==sstrAuto.ToUpper())
                                    {
                                        la.Text = "Can be auto imported";
                                    }
                                }
                                tlStats.Controls.Add(l1, 0, counter);
                                tlStats.Controls.Add(t1, 1, counter);
                                tlStats.Controls.Add(la, 3, counter);
                                counter++;
                            }
                        }
                    }
                }
                if (cbLocation.SelectedIndex != 0)
                {
                    Button b1 = new Button();
                    b1.AutoSize = true;
                    b1.Height = 30;
                    b1.BackColor = Color.LightGray;
                    b1.Text = "Submit";
                    b1.Font = new Font("Arial", 13, FontStyle.Regular);
                    b1.Click += delegate (object sender1, EventArgs e1) { DynamicButton_Click(sender, e, this); };
                    tlStats.Controls.Add(b1, 0, counter);
                    Button b2 = new Button();
                    b2.AutoSize = true;
                    b2.Height = 30;
                    b2.BackColor = Color.LightGray;
                    b2.Text = "Collect Automatic Stats";
                    b2.Font = new Font("Arial", 13, FontStyle.Regular);
                    b2.Click += delegate (object sender1, EventArgs e1) { CollectStats_Click(sender, e, this); };
                    tlStats.Controls.Add(b2, 1, counter);

                    this.Controls.Add(tlStats);
                    if (this.Width < tlStats.Width)
                    {
                        this.Width = tlStats.Width + 50;
                    }
                    if (this.Height < tlStats.Height + 100)
                    {
                        int monHeight = Screen.FromControl(this).WorkingArea.Height;
                        if (tlStats.Height > monHeight - 200)
                        {
                            this.Height = monHeight - 100;
                            this.HorizontalScroll.Enabled = true;
                            this.HorizontalScroll.Visible = true;
                        }
                        else
                        {
                            this.Height = tlStats.Height + 200;
                        }
                    }
                    int locHor = this.Width / 2 - tlStats.Width / 2;
                    tlStats.Location = new Point(locHor, 100);
                }
            }
            else
            {
                if (cbInputMonth.SelectedIndex == 0)
                {
                    cbInputMonth.BackColor = Color.Red;
                }
                else
                {
                    cbInputMonth.BackColor = Color.White;
                }
                if (cbInputYear.SelectedIndex == 0)
                {
                    cbInputYear.BackColor = Color.Red;
                }
                else
                {
                    cbInputYear.BackColor = Color.White;
                }
                if (cbLocation.SelectedIndex == 0)
                {
                    cbLocation.BackColor = Color.Red;
                }
                else
                {
                    cbLocation.BackColor = Color.White;
                }
            }
        }
        private void Textbox_Enter(object sender, EventArgs e, Form passedForm, TextBox t2)
        {
            if (t2.Text == "Not Reported")
            {
                t2.Text = "";
            }
        }
        private void Textbox_Leave(object sender, EventArgs e, Form passedForm, TextBox t2)
        {
            if ((string)t2.Tag == "string")
            {

            }
            else
            {
                double tempInt;
                if (t2.Text.Length == 0) { t2.Text = "Not Reported"; }
                else if (!double.TryParse(t2.Text, out tempInt))
                {
                    MessageBox.Show("Not a valid number -- Please reenter");
                    t2.Focus();
                }
            }
        }
        private void DynamicButton_Click(object sender, EventArgs e, Form passedForm)
        {
            string output = "";
            StatMessage statMessage = new StatMessage();
            statMessage.cpu_name = Environment.MachineName;
            foreach (Location mLoc in locations)
            {
                if (mLoc.loc_name.ToLower().Contains(cbLocation.GetItemText(cbLocation.SelectedItem).ToLower()))
                {
                    statMessage.id_loc = mLoc.id;
                    statMessage.location = cbLocation.GetItemText(cbLocation.SelectedItem).ToLower();
                }
            }
            statMessage.year = cbInputYear.GetItemText(cbInputYear.SelectedItem);
            statMessage.month = cbInputMonth.GetItemText(cbInputMonth.SelectedItem).ToLower();
            foreach (Control c in Controls)
            {
                if (c is TableLayoutPanel)
                {
                    foreach (Control cIn in c.Controls)
                    {
                        if (cIn is TextBox)
                        {
                            bool upload = false;
                            statMessage.stat_type = (String)cIn.Name;
                            statMessage.stat_user = statUser;
                            int countInt = 0;
                            if (cIn.Tag.ToString().ToLower().Contains("string"))
                            {
                                if (!cIn.Text.Contains("Not Reported"))
                                {
                                    statMessage.count = 0;
                                    statMessage.stat_comment = cIn.Text;
                                    upload = true;
                                }
                            }
                            else
                            {
                                if (int.TryParse(cIn.Text, out countInt))
                                {
                                    statMessage.count = countInt;
                                    upload = true;
                                }
                                else
                                {
                                    if (cIn.Text != "Not Reported")
                                    {
                                        MessageBox.Show($"Input for {statMessage.stat_type} is not a number and will not be uploaded");
                                    }
                                }
                            }
                            if (upload)
                            {
                                output += $"Stat: {statMessage.stat_type} Result: {apiStatWrapper.SetMonthlyStat(statMessage)}{Environment.NewLine}";
                            }
                        }
                    }
                }
            }
            MessageBox.Show(output);
        }
        private void CollectStats_Click(object sender, EventArgs e, Form passedForm)
        {
            StatMessage statMessage = new StatMessage();
            statMessage.cpu_name = Environment.MachineName;
            Location loc = new Location();
            foreach (Location mLoc in locations)
            {
                if (mLoc.loc_name.ToLower().Contains(cbLocation.GetItemText(cbLocation.SelectedItem).ToLower()))
                {
                    statMessage.id_loc = mLoc.id;
                    statMessage.location = cbLocation.GetItemText(cbLocation.SelectedItem).ToLower();
                    loc = mLoc;
                }
            }
            statMessage.year = cbInputYear.GetItemText(cbInputYear.SelectedItem);
            statMessage.month = cbInputMonth.GetItemText(cbInputMonth.SelectedItem).ToLower();
            foreach (Control c in Controls)
            {
                if (c is TableLayoutPanel)
                {
                    foreach (Control cIn in c.Controls)
                    {
                        if (cIn is TextBox)
                        {
                            if (!cIn.Tag.ToString().ToLower().Contains("string"))
                            {
                                if (statUser.monthly_admin == 1 || cIn.Text.Contains("Not Reported"))
                                {
                                    if (loc.auto_pull_stats.FindIndex(s => s.Contains((String)cIn.Name)) != -1)
                                    {
                                        statMessage.stat_type = (String)cIn.Name;
                                        int countM = apiStatWrapper.GetStatCountForMonth(statMessage);
                                        cIn.Text = countM.ToString();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            MessageBox.Show("Done");
        }
    }
}
