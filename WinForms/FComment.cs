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
    public partial class FComment : FormBase
    {
        #region Dependencies
        internal string valType;
        internal string label;
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
        public FComment(string label, string valType, IFormFactory formFactory, IStatProvider statProvider, IErrorOutput errorOutput, IIlsProvider ilsProvider, IConfigManager configManager, NotificationManager notify, IOperationProgress operationProgress, ChangeTracker changeTracker)
        {
            this.configManager = configManager;
            this.valType = valType;
            this.label = label;
            InitializeComponent();
            this.notify = notify;
            this.operationProgress = operationProgress;
            this.statProvider = statProvider;
            this.changeTracker = changeTracker;
            notify.ParentForm = this;
            this.formFactory = formFactory;
            this.errorOutput = errorOutput;
            this.ilsProvider = ilsProvider;
            FormClosing += FComment_FormClosing;
            Closed += FComment_Closed;
        }
        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            PostInitializeComponent();
        }
        private void PostInitializeComponent()
        {
            this.label1.Text = label;
        }
        private void FComment_FormClosing(object sender, FormClosingEventArgs e)
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

        private void FComment_Closed(object sender, EventArgs e)
        {
            //SaveToolStripLocation();
            Pipes.KillServer();
            closed = true;
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            bool ok = true;
            if (valType == "number")
            {
                int n;
                if (!int.TryParse(textBox1.Text, out n))
                {
                    ok = false;
                    MessageBox.Show("Please enter a number in the filed provided");
                }
            }
            if (ok)
            {
                DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
