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
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using APP_DeskStats.Helpers;
using System.Drawing;

namespace APP_DeskStats.WinForms
{
    public partial class FGetStats : FormBase
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
        public FGetStats(List<Location> locations, StatUser statUser, IFormFactory formFactory, IStatProvider statProvider, IErrorOutput errorOutput, IIlsProvider ilsProvider, IConfigManager configManager, NotificationManager notify, IOperationProgress operationProgress, ChangeTracker changeTracker)
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
        private void Textbox_Enter(object sender, EventArgs e, Form passedForm, TextBox t2)
        {
            if (t2.Text == "Not Reported")
            {
                t2.Text = "";
            }
        }

        private void btnMonthlyReport_Click(object sender, EventArgs e)
        {
            if (cbInputMonth.SelectedIndex == 0 || cbInputYear.SelectedIndex == 0)
            {
                MessageBox.Show("Please select Year and Month");
            } else
            {
                try
                {
                    report_fields_data report_Fields_Data = new report_fields_data();
                    report_Fields_Data.report_Fields = apiStatWrapper.GetReportFields();
                    if (report_Fields_Data.report_Fields.Count == 0) { MessageBox.Show("Database error please contact IT"); }
                    else
                    {
                        string stat_year = cbInputYear.GetItemText(cbInputYear.SelectedItem);
                        string stat_year_prev = cbInputYear.GetItemText(cbInputYear.Items[cbInputYear.SelectedIndex - 1]);
                        string stat_month = cbInputMonth.GetItemText(cbInputMonth.SelectedItem);
                        DateTime dtReport = Convert.ToDateTime($"01-{stat_month}-{stat_year}");
                        string stat_months_so_far = "";
                        foreach (DateTime dTMonth in Enumerable.Range(1, dtReport.Month).Select(m => new DateTime(DateTime.Today.Year, m, 1)).ToList())
                        {
                            if(stat_months_so_far.Length == 0)
                            {
                                stat_months_so_far = dTMonth.ToString("MMMM").ToLower();
                            }
                            else
                            {
                                stat_months_so_far = stat_months_so_far + "," + dTMonth.ToString("MMMM").ToLower();
                            }
                        }
                        using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                        {
                            saveFileDialog.FileName = $"{stat_month}{stat_year}BoardReport.pdf";
                            if (saveFileDialog.ShowDialog() == DialogResult.OK)
                            {
                                FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write, FileShare.None);
                                using (Document pdfDoc = new Document(PageSize.LETTER, 10f, 10f, 10f, 10f))
                                {
                                    float currentHeight = 30;
                                    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, fs);
                                    pdfDoc.Open();
                                    pdfDoc.NewPage();
                                    string currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                                    string dirCommon = Path.GetFullPath(Path.Combine(currentPath, ".."));
                                    dirCommon = Path.GetFullPath(Path.Combine(dirCommon, "Common"));
                                    PdfPTable pdfHeaderTable = new PdfPTable(2);
                                    pdfHeaderTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                                    var fontHeader = FontFactory.GetFont("Calibri", 14, BaseColor.BLACK);
                                    var fontSectionTitle = FontFactory.GetFont("Calibri", 14, BaseColor.BLACK);
                                    var fontSectionText = FontFactory.GetFont("Calibri", 12, BaseColor.BLACK);
                                    var fontSectionTextSmall = FontFactory.GetFont("Calibri", 10, BaseColor.BLACK);
                                    if (File.Exists($"{dirCommon}\\Images\\OLPL.png"))
                                    {
                                        iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance($"{dirCommon}\\Images\\OLPL.png");
                                        logo.ScaleAbsoluteWidth(100);
                                        pdfHeaderTable.AddCell(ReportHelper.createImageCell(logo, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    }
                                    else
                                    {
                                        pdfHeaderTable.AddCell(ReportHelper.createTextCell("", fontHeader, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    }
                                    pdfHeaderTable.AddCell(ReportHelper.createTextCell($"{stat_year} Statistics - {stat_month}", fontHeader, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    pdfHeaderTable.TotalWidth = pdfDoc.PageSize.Width - 40;
                                    pdfHeaderTable.WriteSelectedRows(0, -1, 20, pdfDoc.PageSize.Height - currentHeight, writer.DirectContent);
                                    currentHeight += pdfHeaderTable.TotalHeight + 20;
                                    float[] columnWidth = { 1, 5, 2, 2, 2, 2 };
                                    PdfPTable tableQuestions = new PdfPTable(columnWidth);
                                    tableQuestions.TotalWidth = pdfDoc.PageSize.Width - 40;
                                    tableQuestions.AddCell(ReportHelper.createTextCell($"QUESTIONS", fontSectionTitle, BaseColor.GRAY, 1, 1, 1, 1, 5, 90, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableQuestions.AddCell(ReportHelper.createTextCell($"", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableQuestions.AddCell(ReportHelper.createTextCell($"Month {stat_year}", fontSectionText, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableQuestions.AddCell(ReportHelper.createTextCell($"YTD  {stat_year}", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableQuestions.AddCell(ReportHelper.createTextCell($"Month {stat_year_prev}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableQuestions.AddCell(ReportHelper.createTextCell($"YTD {stat_year_prev}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    int totalCurYearTD = 0;
                                    int totalCurYearMonth = 0;
                                    int totalPrevYearTD = 0;
                                    int totalPrevYearMonth = 0;
                                    int lineCurYearTD = 0;
                                    int lineCurYearMonth = 0;
                                    int linePrevYearTD = 0;
                                    int linePrevYearMonth = 0;
                                    StatMessage statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("QuestionsAdultServicesLocations").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("QuestionsAdultServicesType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    lineCurYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year;
                                    statMessage.month = stat_months_so_far;
                                    lineCurYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_month;
                                    linePrevYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_months_so_far;
                                    linePrevYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    totalCurYearMonth += lineCurYearMonth;
                                    totalCurYearTD += lineCurYearTD;
                                    totalPrevYearMonth += linePrevYearMonth;
                                    totalPrevYearTD += linePrevYearTD;
                                    tableQuestions.AddCell(ReportHelper.createTextCell($"Adult & Young Adult Services", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableQuestions.AddCell(ReportHelper.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableQuestions.AddCell(ReportHelper.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableQuestions.AddCell(ReportHelper.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableQuestions.AddCell(ReportHelper.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    lineCurYearMonth = 0;
                                    lineCurYearTD = 0;
                                    linePrevYearMonth = 0;
                                    linePrevYearTD = 0;
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("QuestionsCustomerServicesLocations").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("QuestionsCustomerServicesType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    lineCurYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year;
                                    statMessage.month = stat_months_so_far;
                                    lineCurYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_month;
                                    linePrevYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_months_so_far;
                                    linePrevYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    totalCurYearMonth += lineCurYearMonth;
                                    totalCurYearTD += lineCurYearTD;
                                    totalPrevYearMonth += linePrevYearMonth;
                                    totalPrevYearTD += linePrevYearTD;
                                    tableQuestions.AddCell(ReportHelper.createTextCell($"Customer Services", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableQuestions.AddCell(ReportHelper.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableQuestions.AddCell(ReportHelper.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableQuestions.AddCell(ReportHelper.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableQuestions.AddCell(ReportHelper.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    lineCurYearMonth = 0;
                                    lineCurYearTD = 0;
                                    linePrevYearMonth = 0;
                                    linePrevYearTD = 0;
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("QuestionsYouthServicesLocations").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("QuestionsYouthServicesType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    lineCurYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year;
                                    statMessage.month = stat_months_so_far;
                                    lineCurYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_month;
                                    linePrevYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_months_so_far;
                                    linePrevYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    totalCurYearMonth += lineCurYearMonth;
                                    totalCurYearTD += lineCurYearTD;
                                    totalPrevYearMonth += linePrevYearMonth;
                                    totalPrevYearTD += linePrevYearTD; 
                                    tableQuestions.AddCell(ReportHelper.createTextCell($"Youth Services", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableQuestions.AddCell(ReportHelper.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableQuestions.AddCell(ReportHelper.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableQuestions.AddCell(ReportHelper.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableQuestions.AddCell(ReportHelper.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableQuestions.AddCell(ReportHelper.createTextCell($"Total", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableQuestions.AddCell(ReportHelper.createTextCell($"{totalCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableQuestions.AddCell(ReportHelper.createTextCell($"{totalCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableQuestions.AddCell(ReportHelper.createTextCell($"{totalPrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableQuestions.AddCell(ReportHelper.createTextCell($"{totalPrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableQuestions.WriteSelectedRows(0, -1, 20, pdfDoc.PageSize.Height - currentHeight, writer.DirectContent);
                                    lineCurYearMonth = 0;
                                    lineCurYearTD = 0;
                                    linePrevYearMonth = 0;
                                    linePrevYearTD = 0;
                                    totalCurYearMonth = 0;
                                    totalCurYearTD = 0;
                                    totalPrevYearMonth = 0;
                                    totalPrevYearTD = 0;
                                    currentHeight += tableQuestions.TotalHeight + 20;
                                    //Circulation
                                    PdfPTable tableCirculation = new PdfPTable(columnWidth);
                                    tableCirculation.TotalWidth = pdfDoc.PageSize.Width - 40;
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"CIRCULATION", fontSectionTitle, BaseColor.GRAY, 1, 1, 1, 1, 6, 90, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"Month {stat_year}", fontSectionText, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"YTD  {stat_year}", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"Month {stat_year_prev}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"YTD {stat_year_prev}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("CirculationAdultLocations").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("CirculationAdultType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    lineCurYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year;
                                    statMessage.month = stat_months_so_far;
                                    lineCurYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_month;
                                    linePrevYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_months_so_far;
                                    linePrevYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    totalCurYearMonth += lineCurYearMonth;
                                    totalCurYearTD += lineCurYearTD;
                                    totalPrevYearMonth += linePrevYearMonth;
                                    totalPrevYearTD += linePrevYearTD; 
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"Adult", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    lineCurYearMonth = 0;
                                    lineCurYearTD = 0;
                                    linePrevYearMonth = 0;
                                    linePrevYearTD = 0;
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("CirculationYouthLocations").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("CirculationYouthType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    lineCurYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year;
                                    statMessage.month = stat_months_so_far;
                                    lineCurYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_month;
                                    linePrevYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_months_so_far;
                                    linePrevYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    totalCurYearMonth += lineCurYearMonth;
                                    totalCurYearTD += lineCurYearTD;
                                    totalPrevYearMonth += linePrevYearMonth;
                                    totalPrevYearTD += linePrevYearTD; 
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"Young Adult", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    lineCurYearMonth = 0;
                                    lineCurYearTD = 0;
                                    linePrevYearMonth = 0;
                                    linePrevYearTD = 0;
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("CirculationYoungAdultLocations").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("CirculationYoungAdultType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    lineCurYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year;
                                    statMessage.month = stat_months_so_far;
                                    lineCurYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_month;
                                    linePrevYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_months_so_far;
                                    linePrevYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    totalCurYearMonth += lineCurYearMonth;
                                    totalCurYearTD += lineCurYearTD;
                                    totalPrevYearMonth += linePrevYearMonth;
                                    totalPrevYearTD += linePrevYearTD; 
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"Youth", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("CirculationEResourcesLocations").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("CirculationEResourcesType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    lineCurYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year;
                                    statMessage.month = stat_months_so_far;
                                    lineCurYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_month;
                                    linePrevYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_months_so_far;
                                    linePrevYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage); 
                                    totalCurYearMonth += lineCurYearMonth;
                                    totalCurYearTD += lineCurYearTD;
                                    totalPrevYearMonth += linePrevYearMonth;
                                    totalPrevYearTD += linePrevYearTD;
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"eRescources", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"Total", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"{totalCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"{totalCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"{totalPrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCirculation.AddCell(ReportHelper.createTextCell($"{totalPrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCirculation.WriteSelectedRows(0, -1, 20, pdfDoc.PageSize.Height - currentHeight, writer.DirectContent);
                                    lineCurYearMonth = 0;
                                    lineCurYearTD = 0;
                                    linePrevYearMonth = 0;
                                    linePrevYearTD = 0;
                                    totalCurYearMonth = 0;
                                    totalCurYearTD = 0;
                                    totalPrevYearMonth = 0;
                                    totalPrevYearTD = 0;
                                    currentHeight += tableCirculation.TotalHeight + 20;
                                    //Rescource Sharing
                                    PdfPTable tableRescourceSharing = new PdfPTable(columnWidth);
                                    tableRescourceSharing.TotalWidth = pdfDoc.PageSize.Width - 40;
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"RESOURCE SHARING", fontSectionTitle, BaseColor.GRAY, 1, 1, 1, 1, 9, 90, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"Month {stat_year}", fontSectionText, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"YTD  {stat_year}", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"Month {stat_year_prev}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"YTD {stat_year_prev}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"Loans filled:", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("RescourceSharingPatronSelfPlacedLocations").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("RescourceSharingPatronSelfPlacedType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    lineCurYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year;
                                    statMessage.month = stat_months_so_far;
                                    lineCurYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_month;
                                    linePrevYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_months_so_far;
                                    linePrevYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    totalCurYearMonth += lineCurYearMonth;
                                    totalCurYearTD += lineCurYearTD;
                                    totalPrevYearMonth += linePrevYearMonth;
                                    totalPrevYearTD += linePrevYearTD;
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"      Patron self-placed holds", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    lineCurYearMonth = 0;
                                    lineCurYearTD = 0;
                                    linePrevYearMonth = 0;
                                    linePrevYearTD = 0;
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("RescourceSharingILLRequestsOLPLLocations").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("RescourceSharingILLRequestsOLPLType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    lineCurYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year;
                                    statMessage.month = stat_months_so_far;
                                    lineCurYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_month;
                                    linePrevYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_months_so_far;
                                    linePrevYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage); 
                                    totalCurYearMonth += lineCurYearMonth;
                                    totalCurYearTD += lineCurYearTD;
                                    totalPrevYearMonth += linePrevYearMonth;
                                    totalPrevYearTD += linePrevYearTD;
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"      ILL Requests by patrons of OLPL", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    lineCurYearMonth = 0;
                                    lineCurYearTD = 0;
                                    linePrevYearMonth = 0;
                                    linePrevYearTD = 0;
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("RescourceSharingILLRequestsOtherLocations").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("RescourceSharingILLRequestsOtherType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    lineCurYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year;
                                    statMessage.month = stat_months_so_far;
                                    lineCurYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_month;
                                    linePrevYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_months_so_far;
                                    linePrevYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage); 
                                    totalCurYearMonth += lineCurYearMonth;
                                    totalCurYearTD += lineCurYearTD;
                                    totalPrevYearMonth += linePrevYearMonth;
                                    totalPrevYearTD += linePrevYearTD;
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"      ILL Requests by patrons of {Environment.NewLine}             sharing libraries", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"Reciprocal Borrowing:", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                    lineCurYearMonth = 0;
                                    lineCurYearTD = 0;
                                    linePrevYearMonth = 0;
                                    linePrevYearTD = 0;
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("RescourceSharingRecipricalOLPLPatronsLocations").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("RescourceSharingRecipricalOLPLPatronsType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    lineCurYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year;
                                    statMessage.month = stat_months_so_far;
                                    lineCurYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_month;
                                    linePrevYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_months_so_far;
                                    linePrevYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage); 
                                    totalCurYearMonth += lineCurYearMonth;
                                    totalCurYearTD += lineCurYearTD;
                                    totalPrevYearMonth += linePrevYearMonth;
                                    totalPrevYearTD += linePrevYearTD;
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"      OLPL Patrons", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    lineCurYearMonth = 0;
                                    lineCurYearTD = 0;
                                    linePrevYearMonth = 0;
                                    linePrevYearTD = 0;
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("RescourceSharingRecipricalAtOLPLLocations").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("RescourceSharingRecipricalAtOLPLType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    lineCurYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year;
                                    statMessage.month = stat_months_so_far;
                                    lineCurYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_month;
                                    linePrevYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_months_so_far;
                                    linePrevYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage); 
                                    totalCurYearMonth += lineCurYearMonth;
                                    totalCurYearTD += lineCurYearTD;
                                    totalPrevYearMonth += linePrevYearMonth;
                                    totalPrevYearTD += linePrevYearTD;
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"      At OLPL", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"Total", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"{totalCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"{totalCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"{totalPrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.AddCell(ReportHelper.createTextCell($"{totalPrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRescourceSharing.WriteSelectedRows(0, -1, 20, pdfDoc.PageSize.Height - currentHeight, writer.DirectContent);
                                    lineCurYearMonth = 0;
                                    lineCurYearTD = 0;
                                    linePrevYearMonth = 0;
                                    linePrevYearTD = 0;
                                    totalCurYearMonth = 0;
                                    totalCurYearTD = 0;
                                    totalPrevYearMonth = 0;
                                    totalPrevYearTD = 0;
                                    currentHeight += tableRescourceSharing.TotalHeight + 20;
                                    //Collections
                                    float[] columnWidthCollections = { 1, 8, 3, 3 };
                                    PdfPTable tableCollection = new PdfPTable(columnWidthCollections);
                                    tableCollection.TotalWidth = pdfDoc.PageSize.Width - 40;
                                    tableCollection.AddCell(ReportHelper.createTextCell($"COLLECTIONS", fontSectionTitle, BaseColor.GRAY, 1, 1, 1, 1, 5, 90, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCollection.AddCell(ReportHelper.createTextCell($"                                           ", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableCollection.AddCell(ReportHelper.createTextCell($"{stat_year}", fontSectionText, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCollection.AddCell(ReportHelper.createTextCell($"{stat_year_prev}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("CollectionsAdultLocations").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("CollectionsAdultType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    lineCurYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_month;
                                    linePrevYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    totalCurYearMonth += lineCurYearMonth;
                                    totalCurYearTD += lineCurYearTD;
                                    totalPrevYearMonth += linePrevYearMonth;
                                    totalPrevYearTD += linePrevYearTD;
                                    tableCollection.AddCell(ReportHelper.createTextCell($"Adult", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableCollection.AddCell(ReportHelper.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCollection.AddCell(ReportHelper.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    lineCurYearMonth = 0;
                                    lineCurYearTD = 0;
                                    linePrevYearMonth = 0;
                                    linePrevYearTD = 0;
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("CollectionsYoungAdultLocations").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("CollectionsYoungAdultType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    lineCurYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_month;
                                    linePrevYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage); 
                                    totalCurYearMonth += lineCurYearMonth; 
                                    totalCurYearTD += lineCurYearTD;
                                    totalPrevYearMonth += linePrevYearMonth;
                                    totalPrevYearTD += linePrevYearTD;
                                    tableCollection.AddCell(ReportHelper.createTextCell($"Young Adult", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableCollection.AddCell(ReportHelper.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCollection.AddCell(ReportHelper.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    lineCurYearMonth = 0;
                                    lineCurYearTD = 0;
                                    linePrevYearMonth = 0;
                                    linePrevYearTD = 0;
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("CollectionsYouthLocations").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("CollectionsYouthType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    lineCurYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_month;
                                    linePrevYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage); 
                                    totalCurYearMonth += lineCurYearMonth;
                                    totalCurYearTD += lineCurYearTD;
                                    totalPrevYearMonth += linePrevYearMonth;
                                    totalPrevYearTD += linePrevYearTD;
                                    tableCollection.AddCell(ReportHelper.createTextCell($"Youth", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableCollection.AddCell(ReportHelper.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCollection.AddCell(ReportHelper.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCollection.AddCell(ReportHelper.createTextCell($"Total", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCollection.AddCell(ReportHelper.createTextCell($"{totalCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCollection.AddCell(ReportHelper.createTextCell($"{totalPrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableCollection.WriteSelectedRows(0, -1, 20, pdfDoc.PageSize.Height - currentHeight, writer.DirectContent);
                                    lineCurYearMonth = 0;
                                    lineCurYearTD = 0;
                                    linePrevYearMonth = 0;
                                    linePrevYearTD = 0;
                                    totalCurYearMonth = 0;
                                    totalCurYearTD = 0;
                                    totalPrevYearMonth = 0;
                                    totalPrevYearTD = 0;
                                    pdfDoc.NewPage();
                                    currentHeight = 20;
                                    pdfHeaderTable.WriteSelectedRows(0, -1, 20, pdfDoc.PageSize.Height - currentHeight, writer.DirectContent);
                                    currentHeight += pdfHeaderTable.TotalHeight + 20;
                                    //REGISTRATION AND TRAFFIC
                                    PdfPTable tableRegistration = new PdfPTable(columnWidth);
                                    tableRegistration.TotalWidth = pdfDoc.PageSize.Width - 40;
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"REGISTRATION   {Environment.NewLine}AND TRAFFIC      ", fontSectionTitle, BaseColor.GRAY, 1, 1, 1, 1, 8, 90, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"Month {stat_year}", fontSectionText, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"YTD  {stat_year}", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"Month {stat_year_prev}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"YTD {stat_year_prev}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    int patronsRegistered = 0;
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("TrafficPatronsRegisteredTotalLocation").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("TrafficPatronsRegisteredTotalType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    patronsRegistered = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("TrafficPatronsRegisteredLocation").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("TrafficPatronsRegisteredType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    lineCurYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year;
                                    statMessage.month = stat_months_so_far;
                                    lineCurYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_month;
                                    linePrevYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_months_so_far;
                                    linePrevYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"Patrons registered (Total: {patronsRegistered.ToString()})", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    lineCurYearMonth = 0;
                                    lineCurYearTD = 0;
                                    linePrevYearMonth = 0;
                                    linePrevYearTD = 0;
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("TrafficChicagoPatronsRegisteredLocation").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("TrafficChicagoPatronsRegisteredType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    lineCurYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year;
                                    statMessage.month = stat_months_so_far;
                                    lineCurYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_month;
                                    linePrevYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_months_so_far;
                                    linePrevYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"Chicago patrons registered", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    lineCurYearMonth = 0;
                                    lineCurYearTD = 0;
                                    linePrevYearMonth = 0;
                                    linePrevYearTD = 0;
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("TrafficIncommingPhoneCallsLocation").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("TrafficIncommingPhoneCallsType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    lineCurYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year;
                                    statMessage.month = stat_months_so_far;
                                    lineCurYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_month;
                                    linePrevYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_months_so_far;
                                    linePrevYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"Incoming phone calls", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    lineCurYearMonth = 0;
                                    lineCurYearTD = 0;
                                    linePrevYearMonth = 0;
                                    linePrevYearTD = 0;
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("TrafficDoorCountLocation").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("TrafficDoorCountType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    lineCurYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year;
                                    statMessage.month = stat_months_so_far;
                                    lineCurYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_month;
                                    linePrevYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_months_so_far;
                                    linePrevYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"Door count", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    lineCurYearMonth = 0;
                                    lineCurYearTD = 0;
                                    linePrevYearMonth = 0;
                                    linePrevYearTD = 0;
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("TrafficMeetingRoomUsageLocation").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("TrafficMeetingRoomUsageType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    lineCurYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year;
                                    statMessage.month = stat_months_so_far;
                                    lineCurYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_month;
                                    linePrevYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_months_so_far;
                                    linePrevYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"Meeting room usage", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    lineCurYearMonth = 0;
                                    lineCurYearTD = 0;
                                    linePrevYearMonth = 0;
                                    linePrevYearTD = 0;
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("TrafficeGroupStudyUsageLocation").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("TrafficeGroupStudyUsageType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    lineCurYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year;
                                    statMessage.month = stat_months_so_far;
                                    lineCurYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_month;
                                    linePrevYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_months_so_far;
                                    linePrevYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"Group study room usage", fontSectionText, BaseColor.WHITE, 0, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRegistration.AddCell(ReportHelper.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableRegistration.WriteSelectedRows(0, -1, 20, pdfDoc.PageSize.Height - currentHeight, writer.DirectContent);
                                    lineCurYearMonth = 0;
                                    lineCurYearTD = 0;
                                    linePrevYearMonth = 0;
                                    linePrevYearTD = 0;
                                    totalCurYearMonth = 0;
                                    totalCurYearTD = 0;
                                    totalPrevYearMonth = 0;
                                    totalPrevYearTD = 0;
                                    currentHeight += tableRegistration.TotalHeight + 20;
                                    //ComputerUse
                                    PdfPTable tableComputerUse = new PdfPTable(columnWidth);
                                    tableComputerUse.TotalWidth = pdfDoc.PageSize.Width - 40;
                                    tableComputerUse.AddCell(ReportHelper.createTextCell($"COMPUTER USE {Environment.NewLine} (SESSIONS)", fontSectionTitle, BaseColor.GRAY, 1, 1, 1, 1, 5, 90, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                    tableComputerUse.AddCell(ReportHelper.createTextCell($"", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableComputerUse.AddCell(ReportHelper.createTextCell($"Month {stat_year}", fontSectionText, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableComputerUse.AddCell(ReportHelper.createTextCell($"YTD  {stat_year}", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableComputerUse.AddCell(ReportHelper.createTextCell($"Month {stat_year_prev}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableComputerUse.AddCell(ReportHelper.createTextCell($"YTD {stat_year_prev}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("PCUseSessionsAYALocation").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("PCUseSessionsAYAType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    lineCurYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year;
                                    statMessage.month = stat_months_so_far;
                                    lineCurYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_month;
                                    linePrevYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_months_so_far;
                                    linePrevYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    totalCurYearMonth += lineCurYearMonth;
                                    totalCurYearTD += lineCurYearTD;
                                    totalPrevYearMonth += linePrevYearMonth;
                                    totalPrevYearTD += linePrevYearTD;
                                    tableComputerUse.AddCell(ReportHelper.createTextCell($"Adult & Young Adult Services", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableComputerUse.AddCell(ReportHelper.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableComputerUse.AddCell(ReportHelper.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableComputerUse.AddCell(ReportHelper.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableComputerUse.AddCell(ReportHelper.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    lineCurYearMonth = 0;
                                    lineCurYearTD = 0;
                                    linePrevYearMonth = 0;
                                    linePrevYearTD = 0;
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("PCUseSessionsYouthLocation").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("PCUseSessionsYouthType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    lineCurYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year;
                                    statMessage.month = stat_months_so_far;
                                    lineCurYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_month;
                                    linePrevYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_months_so_far;
                                    linePrevYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage); 
                                    totalCurYearMonth += lineCurYearMonth;
                                    totalCurYearTD += lineCurYearTD;
                                    totalPrevYearMonth += linePrevYearMonth;
                                    totalPrevYearTD += linePrevYearTD;
                                    tableComputerUse.AddCell(ReportHelper.createTextCell($"Youth Services", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableComputerUse.AddCell(ReportHelper.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableComputerUse.AddCell(ReportHelper.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableComputerUse.AddCell(ReportHelper.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableComputerUse.AddCell(ReportHelper.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    lineCurYearMonth = 0;
                                    lineCurYearTD = 0;
                                    linePrevYearMonth = 0;
                                    linePrevYearTD = 0;
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("PCUseSessionsWirelessLocation").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("PCUseSessionsWirelessType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    lineCurYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year;
                                    statMessage.month = stat_months_so_far;
                                    lineCurYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_month;
                                    linePrevYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_months_so_far;
                                    linePrevYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage); 
                                    totalCurYearMonth += lineCurYearMonth;
                                    totalCurYearTD += lineCurYearTD;
                                    totalPrevYearMonth += linePrevYearMonth;
                                    totalPrevYearTD += linePrevYearTD;
                                    tableComputerUse.AddCell(ReportHelper.createTextCell($"Wireless ", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableComputerUse.AddCell(ReportHelper.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableComputerUse.AddCell(ReportHelper.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableComputerUse.AddCell(ReportHelper.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableComputerUse.AddCell(ReportHelper.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableComputerUse.AddCell(ReportHelper.createTextCell($"Total", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableComputerUse.AddCell(ReportHelper.createTextCell($"{totalCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableComputerUse.AddCell(ReportHelper.createTextCell($"{totalCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableComputerUse.AddCell(ReportHelper.createTextCell($"{totalPrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableComputerUse.AddCell(ReportHelper.createTextCell($"{totalPrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableComputerUse.WriteSelectedRows(0, -1, 20, pdfDoc.PageSize.Height - currentHeight, writer.DirectContent);
                                    lineCurYearMonth = 0;
                                    lineCurYearTD = 0;
                                    linePrevYearMonth = 0;
                                    linePrevYearTD = 0;
                                    totalCurYearMonth = 0;
                                    totalCurYearTD = 0;
                                    totalPrevYearMonth = 0;
                                    totalPrevYearTD = 0;
                                    currentHeight += tableComputerUse.TotalHeight + 55;
                                    //Social Media
                                    PdfPTable tableSocialMedia = new PdfPTable(columnWidth);
                                    tableSocialMedia.TotalWidth = pdfDoc.PageSize.Width - 40;
                                    tableSocialMedia.AddCell(ReportHelper.createTextCell($"ONLINE USE", fontSectionTitle, BaseColor.GRAY, 1, 1, 1, 1, 4, 90, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                    tableSocialMedia.AddCell(ReportHelper.createTextCell($"", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableSocialMedia.AddCell(ReportHelper.createTextCell($"Month {stat_year}", fontSectionText, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableSocialMedia.AddCell(ReportHelper.createTextCell($"YTD  {stat_year}", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableSocialMedia.AddCell(ReportHelper.createTextCell($"Month {stat_year_prev}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableSocialMedia.AddCell(ReportHelper.createTextCell($"YTD {stat_year_prev}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("PCUseWebHitsLocation").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("PCUseWebHitsType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    lineCurYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year;
                                    statMessage.month = stat_months_so_far;
                                    lineCurYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_month;
                                    linePrevYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_months_so_far;
                                    linePrevYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage); 
                                    tableSocialMedia.AddCell(ReportHelper.createTextCell($"Web Page Unique Visitors", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableSocialMedia.AddCell(ReportHelper.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableSocialMedia.AddCell(ReportHelper.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableSocialMedia.AddCell(ReportHelper.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableSocialMedia.AddCell(ReportHelper.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    lineCurYearMonth = 0;
                                    lineCurYearTD = 0;
                                    linePrevYearMonth = 0;
                                    linePrevYearTD = 0;
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("PCUseSocialNetworkingLocation").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("PCUseSocialNetworkingType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    lineCurYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year;
                                    statMessage.month = stat_months_so_far;
                                    lineCurYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_month;
                                    linePrevYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_months_so_far;
                                    linePrevYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage); 
                                    tableSocialMedia.AddCell(ReportHelper.createTextCell($"Social Networking & Marketing (Followers)", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableSocialMedia.AddCell(ReportHelper.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableSocialMedia.AddCell(ReportHelper.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableSocialMedia.AddCell(ReportHelper.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableSocialMedia.AddCell(ReportHelper.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    lineCurYearMonth = 0;
                                    lineCurYearTD = 0;
                                    linePrevYearMonth = 0;
                                    linePrevYearTD = 0;
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("PCUseOnlineRescourcesLocation").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("PCUseOnlineRescourcesType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    lineCurYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year;
                                    statMessage.month = stat_months_so_far;
                                    lineCurYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_month;
                                    linePrevYearMonth = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    statMessage.year = stat_year_prev;
                                    statMessage.month = stat_months_so_far;
                                    linePrevYearTD = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableSocialMedia.AddCell(ReportHelper.createTextCell($"Online Resources (Sessions)", fontSectionText, BaseColor.WHITE, 0, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableSocialMedia.AddCell(ReportHelper.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableSocialMedia.AddCell(ReportHelper.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableSocialMedia.AddCell(ReportHelper.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableSocialMedia.AddCell(ReportHelper.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableSocialMedia.WriteSelectedRows(0, -1, 20, pdfDoc.PageSize.Height - currentHeight, writer.DirectContent);
                                    lineCurYearMonth = 0;
                                    lineCurYearTD = 0;
                                    linePrevYearMonth = 0;
                                    linePrevYearTD = 0;
                                    totalCurYearMonth = 0;
                                    totalCurYearTD = 0;
                                    totalPrevYearMonth = 0;
                                    totalPrevYearTD = 0;
                                    currentHeight += tableSocialMedia.TotalHeight + 20;
                                    // Programming
                                    float[] columnWidthProgramming = { 2, 7, 3, 3, 3, 3, 3, 3 };
                                    PdfPTable tableProgramming = new PdfPTable(columnWidthProgramming);
                                    tableProgramming.TotalWidth = pdfDoc.PageSize.Width - 40;
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"PROGRAMMING", fontSectionTitle, BaseColor.GRAY, 1, 1, 1, 1, 10, 90, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"Attendance {stat_year}", fontSectionTextSmall, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"# of Programs {stat_year}", fontSectionTextSmall, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"# of Programs YTD {stat_year}", fontSectionTextSmall, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"Attendance {stat_year_prev}", fontSectionTextSmall, BaseColor.LIGHT_GRAY, 1, 0, 1, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"# of Programs {stat_year_prev}", fontSectionTextSmall, BaseColor.LIGHT_GRAY, 1, 0, 1, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"# of Programs YTD {stat_year_prev}", fontSectionTextSmall, BaseColor.LIGHT_GRAY, 1, 1, 1, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"Adult Services", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"   Library sponsored", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    int temp = 0;
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("ProgrammingAdultSerivesLibrarySponseredLocation").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("ProgrammingAdultSerivesLibrarySponseredTypeAttendance").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.stat_type = report_Fields_Data.GetReportField("ProgrammingAdultSerivesLibrarySponseredTypePrograms").GetValueString();
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_RIGHT));
                                    temp = 0;
                                    statMessage.month = stat_months_so_far;
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.month = stat_month;
                                    statMessage.stat_type = report_Fields_Data.GetReportField("ProgrammingAdultSerivesLibrarySponseredTypeAttendance").GetValueString();
                                    statMessage.year = stat_year_prev;
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.stat_type = report_Fields_Data.GetReportField("ProgrammingAdultSerivesLibrarySponseredTypePrograms").GetValueString();
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.month = stat_months_so_far;
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"   Community outreach", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("ProgrammingAdultSerivesCommunityOutreachLocation").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("ProgrammingAdultSerivesCommunityOutreachTypeAttendance").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.stat_type = report_Fields_Data.GetReportField("ProgrammingAdultSerivesCommunityOutreachTypePrograms").GetValueString();
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.month = stat_months_so_far;
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.month = stat_month;
                                    statMessage.stat_type = report_Fields_Data.GetReportField("ProgrammingAdultSerivesCommunityOutreachTypeAttendance").GetValueString();
                                    statMessage.year = stat_year_prev;
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.stat_type = report_Fields_Data.GetReportField("ProgrammingAdultSerivesCommunityOutreachTypePrograms").GetValueString();
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.month = stat_months_so_far;
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"   Computer classes", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("ProgrammingAdultSerivesComputerClassesLocation").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("ProgrammingAdultSerivesComputerClassesTypeAttendance").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.stat_type = report_Fields_Data.GetReportField("ProgrammingAdultSerivesComputerClassesTypePrograms").GetValueString();
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.month = stat_months_so_far;
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.month = stat_month;
                                    statMessage.stat_type = report_Fields_Data.GetReportField("ProgrammingAdultSerivesComputerClassesTypeAttendance").GetValueString();
                                    statMessage.year = stat_year_prev;
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.stat_type = report_Fields_Data.GetReportField("ProgrammingAdultSerivesComputerClassesTypePrograms").GetValueString();
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.month = stat_months_so_far;
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"Young Adult Services", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("ProgrammingYoungAdultLocation").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("ProgrammingYoungAdultTypeAttendance").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.stat_type = report_Fields_Data.GetReportField("ProgrammingYoungAdultTypePrograms").GetValueString();
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.month = stat_months_so_far;
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.month = stat_month;
                                    statMessage.stat_type = report_Fields_Data.GetReportField("ProgrammingAdultSerivesComputerClassesTypeAttendance").GetValueString();
                                    statMessage.year = stat_year_prev;
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.stat_type = report_Fields_Data.GetReportField("ProgrammingAdultSerivesComputerClassesTypePrograms").GetValueString();
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.month = stat_months_so_far;
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"Youth Services", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"   Library sponsored", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("ProgrammingYouthLibrarySponseredLocation").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("ProgrammingYouthLibrarySponseredTypeAttendance").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.stat_type = report_Fields_Data.GetReportField("ProgrammingYouthLibrarySponseredTypePrograms").GetValueString();
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.month = stat_months_so_far;
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.month = stat_month;
                                    statMessage.stat_type = report_Fields_Data.GetReportField("ProgrammingYouthLibrarySponseredTypeAttendance").GetValueString();
                                    statMessage.year = stat_year_prev;
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.stat_type = report_Fields_Data.GetReportField("ProgrammingYouthLibrarySponseredTypePrograms").GetValueString();
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.month = stat_months_so_far;
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"   Community outreach", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("ProgrammingYouthOutreachLocation").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("ProgrammingYouthOutreachTypeAttendance").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.stat_type = report_Fields_Data.GetReportField("ProgrammingYouthOutreachTypePrograms").GetValueString();
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.month = stat_months_so_far;
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.month = stat_month;
                                    statMessage.stat_type = report_Fields_Data.GetReportField("ProgrammingYouthOutreachTypeAttendance").GetValueString();
                                    statMessage.year = stat_year_prev;
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.stat_type = report_Fields_Data.GetReportField("ProgrammingYouthLibrarySponseredTypePrograms").GetValueString();
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.month = stat_months_so_far;
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"Homebound Visits", fontSectionText, BaseColor.WHITE, 0, 1, 1, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"N/A", fontSectionText, BaseColor.WHITE, 0, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("ProgrammingHomeboundLocation").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("ProgrammingHomeboundTypePrograms").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.month = stat_months_so_far;
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"N/A", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.month = stat_month;
                                    statMessage.year = stat_year_prev;
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    temp = 0;
                                    statMessage.month = stat_months_so_far;
                                    temp = apiStatWrapper.GetMonthlyStatCountForMonth(statMessage);
                                    tableProgramming.AddCell(ReportHelper.createTextCell($"{temp.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                    tableProgramming.WriteSelectedRows(0, -1, 20, pdfDoc.PageSize.Height - currentHeight, writer.DirectContent);
                                    currentHeight += tableProgramming.TotalHeight + 20;
                                    // Highlights
                                    float[] columnWidthHighlights = { 1, 13 };
                                    PdfPTable tableHighlights = new PdfPTable(columnWidthHighlights);
                                    tableHighlights.TotalWidth = pdfDoc.PageSize.Width - 40;
                                    string strHi = "";
                                    statMessage = new StatMessage
                                    {
                                        location = report_Fields_Data.GetReportField("HighlightsLocation").GetValueString(),
                                        stat_type = report_Fields_Data.GetReportField("HighlightsType").GetValueString(),
                                        year = stat_year,
                                        month = stat_month
                                    };
                                    strHi = apiStatWrapper.GetMonthlyStatComment(statMessage);
                                    if (strHi.ToLower().Contains("not reported"))
                                    {
                                        strHi = "";
                                    }
                                    tableHighlights.AddCell(ReportHelper.createTextCell($"HIGHLIGHTS", fontSectionTitle, BaseColor.GRAY, 1, 1, 1, 1, 1, 90, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                    tableHighlights.AddCell(ReportHelper.createTextCell($"{strHi}", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_JUSTIFIED, Element.ALIGN_MIDDLE));
                                    tableHighlights.WriteSelectedRows(0, -1, 20, pdfDoc.PageSize.Height - currentHeight, writer.DirectContent);
                                    currentHeight += tableHighlights.TotalHeight + 20;
                                    pdfDoc.Close();
                                    System.Diagnostics.Process.Start(saveFileDialog.FileName);
                                }
                            }
                        }
                    }
                }
                catch (Exception eW)
                {
                    MessageBox.Show($"Error Monthly Stat Report: {eW.ToString()}");
                    //cL.fileWriteLog($"Error Monthly Stat Report: {eW.ToString()}", "STAT", "formGetStats", dataSettings);
                }
            }
        }

        private void btnLocationReport_Click(object sender, EventArgs e)
        {
            if (cbInputMonth.SelectedIndex == 0 || cbInputYear.SelectedIndex == 0 || cbLocation.SelectedIndex == 0)
            {
                MessageBox.Show("Please select Year and Month and Location");
            }
            else
            {
                this.Controls.Remove(this.Controls["tlStats"]);
                TableLayoutPanel tlStats = new TableLayoutPanel();
                tlStats.Location = new Point(15, 100);
                tlStats.Size = new Size(135, 34);
                tlStats.AutoSize = true;
                tlStats.Name = "tlStats";
                tlStats.ColumnCount = 2;
                tlStats.RowCount = 1;
                tlStats.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                tlStats.GrowStyle = TableLayoutPanelGrowStyle.AddRows;
                int counter = 0;
                foreach (Location mLoc in locations)
                {
                    if (mLoc.loc_name.ToLower().Contains(cbLocation.GetItemText(cbLocation.SelectedItem).ToLower()))
                    {
                        dtResults = populateLocationReport();
                        foreach (string strin in mLoc.loc_stats)
                        {
                            if (!strin.ToLower().Contains("none"))
                            {
                                string strMatch = "";
                                Label l1 = new Label();
                                dynamic t1 = new TextBox();
                                if (strin.ToLower().Contains("(time)"))
                                {
                                    t1.Name = strin;
                                    t1.Tag = "time";
                                    strMatch = strin.Replace("(time)", string.Empty);
                                    l1.Text = strin.Replace("(time)", string.Empty);
                                }
                                else if (strin.ToLower().Contains("(string)"))
                                {
                                    t1 = new RichTextBox();
                                    t1.Width = 400;
                                    t1.Name = strin;
                                    t1.Tag = "string";
                                    l1.Text = strin.Replace("(string)", string.Empty);
                                    strMatch = strin.Replace("(string)", string.Empty);
                                }
                                else { l1.Text = strin; t1.Tag = ""; t1.Name = strin; strMatch = strin; }
                                foreach (DataRow dtRow in dtResults.Rows)
                                {
                                    if (dtRow["type"].ToString().ToLower().Contains(strMatch.ToLower()))
                                    {
                                        try
                                        {
                                            if (t1.Tag.ToString().ToLower().Contains("string"))
                                            {
                                                t1.Text = dtRow["comment"].ToString();
                                                t1.WordWrap = true;
                                                //t1.Enabled = false;
                                            }
                                            else
                                            {
                                                t1.Text = dtRow["count"].ToString();
                                                t1.Enabled = false;
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
                                tlStats.AutoSize = true;
                                l1.Font = new System.Drawing.Font("Arial", 13, FontStyle.Regular);
                                t1.Font = new System.Drawing.Font("Arial", 13, FontStyle.Regular);
                                tlStats.Controls.Add(l1, 0, counter);
                                tlStats.Controls.Add(t1, 1, counter);
                                counter++;
                            }
                        }
                    }
                }
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
                tlStats.Location = new Point(locHor, 150);
            }

        }
        internal DataTable populateLocationReport()
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
    }
}
