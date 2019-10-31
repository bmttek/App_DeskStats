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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace APP_DeskStats
{
    public partial class FSettings : FormBase
    {
        #region Dependencies
        internal StatUser statUser;
        internal List<Location> locations;
        internal string location;
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
        public FSettings(List<Location> locations, StatUser statUser, IFormFactory formFactory, IStatProvider statProvider, IErrorOutput errorOutput, IIlsProvider ilsProvider, IConfigManager configManager, NotificationManager notify, IOperationProgress operationProgress, ChangeTracker changeTracker)
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
            apiStatWrapper = new ApiStatsWrapper(errorOutput, configManager, formFactory);
            FormClosing += FSettings_FormClosing;
            Closed += FSettings_Closed;
        }
        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            PostInitializeComponent();
        }
        private void PostInitializeComponent()
        {
        }
        private void FSettings_FormClosing(object sender, FormClosingEventArgs e)
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

        private void FSettings_Closed(object sender, EventArgs e)
        {
            //SaveToolStripLocation();
            Pipes.KillServer();
            closed = true;
        }
        #endregion

        private void setUserPreferences_Load(object sender, EventArgs e)
        {
            if (locations == null)
            {
                MessageBox.Show("Error getting location list from main form");
                this.Close();
            }
            foreach(Location mL in locations)
            {
                cbSelectLocation.Items.Add(mL.loc_name);
                if(statUser.id_loc == mL.id)
                {
                    cbSelectLocation.SelectedIndex = cbSelectLocation.FindStringExact(mL.loc_name);
                    location = mL.loc_name;
                }
            }
            settingAutoHideED.SelectedIndex = statUser.autohide_enable;
            settingAutoHideTimeout.Text = statUser.autohide_timeout.ToString();
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            if (cbSelectLocation.Text.Length < 5) { MessageBox.Show("Please select location"); }
            else
            {
                foreach(Location mL in locations)
                {
                    if (cbSelectLocation.Text == mL.loc_name)
                    {
                        statUser.id_loc = mL.id;
                        statUser = apiStatWrapper.UpdateUser(statUser);
                        statUser.autohide_enable = settingAutoHideED.SelectedIndex;
                        int tempInt = 0;
                        if (int.TryParse(settingAutoHideTimeout.Text, out tempInt))
                        {
                            statUser.autohide_timeout = tempInt;
                        }
                        else
                        {
                            statUser.autohide_timeout = 120;
                        }
                        location = mL.loc_name;
                        statUser = apiStatWrapper.UpdateUser(statUser);
                        DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (cbSelectLocation.Text.Length < 5) { MessageBox.Show("Please select location"); }
            else
            {
                foreach (Location mL in locations)
                {
                    if (cbSelectLocation.Text == mL.loc_name)
                    {
                        statUser.id_loc = mL.id;
                        statUser.autohide_enable = settingAutoHideED.SelectedIndex;
                        int tempInt = 0;
                        if(int.TryParse(settingAutoHideTimeout.Text,out tempInt))
                        {
                            statUser.autohide_timeout = tempInt;
                        }
                        else
                        {
                            statUser.autohide_timeout = 120;
                        }
                        location = mL.loc_name;
                        DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
            }
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult =  DialogResult.OK;
            this.Close();
        }
    }
}
