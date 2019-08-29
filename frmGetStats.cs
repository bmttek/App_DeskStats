using DLL_Support;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using IniParser.Model;
using DataTable = System.Data.DataTable;
using Font = System.Drawing.Font;
using Label = System.Windows.Forms.Label;
using TextBox = System.Windows.Forms.TextBox;
using Point = System.Drawing.Point;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Reflection;
using APP_DeskStats.Functions;

namespace APP_DeskStats
{
    public partial class frmGetStats : Form
    {
        controlStats cStats = new controlStats();
        controlSettings cS = new controlSettings();
        controlLogs cL = new controlLogs();
        internal IniDataCaseInsensitive dataSettings;
        List<modelLocation> lMl;
        DataTable dtResults = new DataTable();
        public modelStat mPs;
        public frmGetStats()
        {
            InitializeComponent();
        }

        private void frmGetStats_Load(object sender, EventArgs e)
        {
            cbInputYear.Items.Add("Select Year");
            cbInputYear.Items.Add(DateTime.Now.AddYears(-3).ToString("yyyy"));
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
            lMl = (List<modelLocation>)cStats.statControl(dataSettings, controlStats.getLocations, "STATS", "LoadForm", this, null, "", "", false, 0);
            foreach (modelLocation mLoc in lMl)
            {
                if (mLoc.loc_view == 1)
                {
                    cbLocation.Items.Add(mLoc.loc_name);
                }
            }
            cbLocation.SelectedIndex = 0;
        }
        private DataTable cb_Changed()
        {
            DataTable dtResults = new DataTable();
            if (cbInputMonth.SelectedIndex != 0 && cbInputYear.SelectedIndex != 0 && cbLocation.SelectedIndex != 0)
            {
                mPs.cpu_name = Environment.MachineName;
                foreach (modelLocation mLoc in lMl)
                {
                    if (mLoc.loc_name.ToLower().Contains(cbLocation.GetItemText(cbLocation.SelectedItem).ToLower()))
                    {
                        mPs.id_loc = mLoc.id;
                        mPs.location = cbLocation.GetItemText(cbLocation.SelectedItem).ToLower();
                    }
                }
                mPs.year = cbInputYear.GetItemText(cbInputYear.SelectedItem);
                mPs.month = cbInputMonth.GetItemText(cbInputMonth.SelectedItem).ToLower();
                dtResults = (DataTable)cStats.statControl(dataSettings, controlStats.getMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, mPs.stat_type, mPs.stat_comment, false, mPs.id_loc, mPs.location, mPs.month, mPs.year);
            }
            return dtResults;
        }
        private void Textbox_Enter(object sender, EventArgs e, Form passedForm,TextBox t2)
        {
            if(t2.Text=="Not Reported")
            {
                t2.Text = "";
            }
        }
        private void Textbox_Leave(object sender, EventArgs e, Form passedForm, TextBox t2)
        {
            int tempInt;
            if(t2.Text.Length == 0) { t2.Text = "Not Reported"; }
            else if(!int.TryParse(t2.Text,out tempInt))
            {
                MessageBox.Show("Not a valid number -- Please reenter");
                t2.Focus();
            }
        }
        private void DynamicButton_Click(object sender, EventArgs e, Form passedForm)
        {
            string output = "";
            mPs.cpu_name = Environment.MachineName;
            foreach (modelLocation mLoc in lMl)
            {
                if (mLoc.loc_name.ToLower().Contains(cbLocation.GetItemText(cbLocation.SelectedItem).ToLower()))
                {
                    mPs.id_loc = mLoc.id;
                    mPs.location = cbLocation.GetItemText(cbLocation.SelectedItem).ToLower();
                }
            }
            mPs.year = cbInputYear.GetItemText(cbInputYear.SelectedItem);
            mPs.month = cbInputMonth.GetItemText(cbInputMonth.SelectedItem).ToLower();
            foreach (Control c in Controls)
            {
                if (c is TableLayoutPanel)
                {
                    foreach(Control cIn in c.Controls)
                    {
                        if (cIn is TextBox)
                        {
                            mPs.stat_type = (String)cIn.Tag;
                            int countInt = 0;
                            if (int.TryParse(cIn.Text,out countInt))
                            {
                                mPs.count = countInt;
                                output += $"Stat: {mPs.stat_type}: Result:{cStats.statControl(dataSettings, controlStats.postMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, mPs.stat_type, mPs.stat_comment, false, mPs.id_loc, mPs.location, mPs.month, mPs.year,mPs.count)}{Environment.NewLine}";
                            }
                            else
                            {
                                if (cIn.Text != "Not Reported")
                                {
                                    MessageBox.Show($"Input for {mPs.stat_type} is not a number and will not be uploaded");
                                }
                            }
                        }
                    }
                }
            }
            MessageBox.Show(output);
        }

        private void btnMonthlyReport_Click(object sender, EventArgs e)
        {
            classFunctions cF = new classFunctions();
            if (cbInputMonth.SelectedIndex != 0 && cbInputYear.SelectedIndex != 0)
            {
                try
                {
                    string stat_year = cbInputYear.GetItemText(cbInputYear.SelectedItem);
                    string stat_year_prev = cbInputYear.GetItemText(cbInputYear.Items[cbInputYear.SelectedIndex - 1]);
                    string stat_month = cbInputMonth.GetItemText(cbInputMonth.SelectedItem);
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
                                    pdfHeaderTable.AddCell(cF.createImageCell(logo, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                }
                                else
                                {
                                    pdfHeaderTable.AddCell(cF.createTextCell("", fontHeader, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                }
                                pdfHeaderTable.AddCell(cF.createTextCell($"{stat_year} Statistics - {stat_month}", fontHeader, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                pdfHeaderTable.TotalWidth = pdfDoc.PageSize.Width - 40;
                                pdfHeaderTable.WriteSelectedRows(0, -1, 20, pdfDoc.PageSize.Height - currentHeight, writer.DirectContent);
                                currentHeight += pdfHeaderTable.TotalHeight + 20;
                                float[] columnWidth = { 1, 5, 2, 2, 2, 2 };
                                PdfPTable tableQuestions = new PdfPTable(columnWidth);
                                tableQuestions.TotalWidth = pdfDoc.PageSize.Width - 40;
                                tableQuestions.AddCell(cF.createTextCell($"QUESTIONS", fontSectionTitle, BaseColor.GRAY, 1, 1, 1, 1, 5, 90, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableQuestions.AddCell(cF.createTextCell($"", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableQuestions.AddCell(cF.createTextCell($"Month {stat_year}", fontSectionText, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableQuestions.AddCell(cF.createTextCell($"YTD  {stat_year}", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableQuestions.AddCell(cF.createTextCell($"Month {stat_year_prev}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableQuestions.AddCell(cF.createTextCell($"YTD {stat_year_prev}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                int totalCurYearTD = 0;
                                int totalCurYearMonth = 0;
                                int totalPrevYearTD = 0;
                                int totalPrevYearMonth = 0;
                                int lineCurYearTD = 0;
                                int lineCurYearMonth = 0;
                                int linePrevYearTD = 0;
                                int linePrevYearMonth = 0;
                                lineCurYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Directional,Reference,Assistance", "", false, mPs.id_loc, "Adult Services,Local History,Audio Visual,Computer Lab,Young Adult,Computer Lab", stat_month, stat_year);
                                lineCurYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Directional,Reference,Assistance", "", false, mPs.id_loc, "Adult Services,Local History,Audio Visual,Computer Lab,Young Adult,Computer Lab", $"year-{stat_month}", stat_year);
                                linePrevYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Directional,Reference,Assistance", "", false, mPs.id_loc, "Adult Services,Local History,Audio Visual,Computer Lab,Young Adult,Computer Lab", stat_month, stat_year_prev);
                                linePrevYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Directional,Reference,Assistance", "", false, mPs.id_loc, "Adult Services,Local History,Audio Visual,Computer Lab,Young Adult,Computer Lab", $"year-{stat_month}", stat_year_prev);
                                totalCurYearMonth += lineCurYearMonth;
                                totalCurYearTD += lineCurYearTD;
                                totalPrevYearMonth += linePrevYearMonth;
                                totalPrevYearTD += linePrevYearTD;
                                tableQuestions.AddCell(cF.createTextCell($"Adult & Young Adult Services", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableQuestions.AddCell(cF.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableQuestions.AddCell(cF.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableQuestions.AddCell(cF.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableQuestions.AddCell(cF.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                lineCurYearMonth = 0;
                                lineCurYearTD = 0;
                                linePrevYearMonth = 0;
                                linePrevYearTD = 0;
                                lineCurYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Directional,Reference,Circulation Desk - Directional Questions", "", false, mPs.id_loc, "Customer Services", stat_month, stat_year);
                                lineCurYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Directional,Reference,Circulation Desk - Directional Questions", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year);
                                linePrevYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Directional,Reference,Circulation Desk - Directional Questions", "", false, mPs.id_loc, "Customer Services", stat_month, stat_year_prev);
                                linePrevYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Directional,Reference,Circulation Desk - Directional Questions", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year_prev);
                                totalCurYearMonth += lineCurYearMonth;
                                totalCurYearTD += lineCurYearTD;
                                totalPrevYearMonth += linePrevYearMonth;
                                totalPrevYearTD += linePrevYearTD;
                                tableQuestions.AddCell(cF.createTextCell($"Customer Services", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableQuestions.AddCell(cF.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableQuestions.AddCell(cF.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableQuestions.AddCell(cF.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableQuestions.AddCell(cF.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                lineCurYearMonth = 0;
                                lineCurYearTD = 0;
                                linePrevYearMonth = 0;
                                linePrevYearTD = 0;
                                lineCurYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Directional,Reference", "", false, mPs.id_loc, "Youth Services", stat_month, stat_year);
                                lineCurYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Directional,Reference", "", false, mPs.id_loc, "Youth Services", $"year-{stat_month}", stat_year);
                                linePrevYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Directional,Reference", "", false, mPs.id_loc, "Youth Services", stat_month, stat_year_prev);
                                linePrevYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Directional,Reference", "", false, mPs.id_loc, "Youth Services", $"year-{stat_month}", stat_year_prev);
                                totalCurYearMonth += lineCurYearMonth;
                                totalCurYearTD += lineCurYearTD;
                                totalPrevYearMonth += linePrevYearMonth;
                                totalPrevYearTD += linePrevYearTD;
                                tableQuestions.AddCell(cF.createTextCell($"Youth Services", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableQuestions.AddCell(cF.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableQuestions.AddCell(cF.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableQuestions.AddCell(cF.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableQuestions.AddCell(cF.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableQuestions.AddCell(cF.createTextCell($"Total", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableQuestions.AddCell(cF.createTextCell($"{totalCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableQuestions.AddCell(cF.createTextCell($"{totalCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableQuestions.AddCell(cF.createTextCell($"{totalPrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableQuestions.AddCell(cF.createTextCell($"{totalPrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
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
                                tableCirculation.AddCell(cF.createTextCell($"CIRCULATION", fontSectionTitle, BaseColor.GRAY, 1, 1, 1, 1, 6, 90, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableCirculation.AddCell(cF.createTextCell($"", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableCirculation.AddCell(cF.createTextCell($"Month {stat_year}", fontSectionText, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableCirculation.AddCell(cF.createTextCell($"YTD  {stat_year}", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableCirculation.AddCell(cF.createTextCell($"Month {stat_year_prev}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableCirculation.AddCell(cF.createTextCell($"YTD {stat_year_prev}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                lineCurYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Circulation - Adult - Checkout,Circulation - Adult - Renewed", "", false, mPs.id_loc, "Customer Services", stat_month, stat_year);
                                lineCurYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Circulation - Adult - Checkout,Circulation - Adult - Renewed", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year);
                                linePrevYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Circulation - Adult - Checkout,Circulation - Adult - Renewed", "", false, mPs.id_loc, "Customer Services", stat_month, stat_year_prev);
                                linePrevYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Circulation - Adult - Checkout,Circulation - Adult - Renewed", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year_prev);
                                totalCurYearMonth += lineCurYearMonth;
                                totalCurYearTD += lineCurYearTD;
                                totalPrevYearMonth += linePrevYearMonth;
                                totalPrevYearTD += linePrevYearTD;
                                tableCirculation.AddCell(cF.createTextCell($"Adult", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableCirculation.AddCell(cF.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableCirculation.AddCell(cF.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableCirculation.AddCell(cF.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableCirculation.AddCell(cF.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                lineCurYearMonth = 0;
                                lineCurYearTD = 0;
                                linePrevYearMonth = 0;
                                linePrevYearTD = 0;
                                lineCurYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Circulation - Teen - Checkout,Circulation - Teen - Renewed", "", false, mPs.id_loc, "Customer Services", stat_month, stat_year);
                                lineCurYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Circulation - Teen - Checkout,Circulation - Teen - Renewed", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year);
                                linePrevYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Circulation - Teen - Checkout,Circulation - Teen - Renewed", "", false, mPs.id_loc, "Customer Services", stat_month, stat_year_prev);
                                linePrevYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Circulation - Teen - Checkout,Circulation - Teen - Renewed", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year_prev);
                                totalCurYearMonth += lineCurYearMonth;
                                totalCurYearTD += lineCurYearTD;
                                totalPrevYearMonth += linePrevYearMonth;
                                totalPrevYearTD += linePrevYearTD;
                                tableCirculation.AddCell(cF.createTextCell($"Young Adult", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableCirculation.AddCell(cF.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableCirculation.AddCell(cF.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableCirculation.AddCell(cF.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableCirculation.AddCell(cF.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                lineCurYearMonth = 0;
                                lineCurYearTD = 0;
                                linePrevYearMonth = 0;
                                linePrevYearTD = 0;
                                lineCurYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Circulation - Juvenile - Checkout,Circulation - Juvenile - Renewed", "", false, mPs.id_loc, "Customer Services", stat_month, stat_year);
                                lineCurYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Circulation - Juvenile - Checkout,Circulation - Juvenile - Renewed", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year);
                                linePrevYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Circulation - Juvenile - Checkout,Circulation - Juvenile - Renewed", "", false, mPs.id_loc, "Customer Services", stat_month, stat_year_prev);
                                linePrevYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Circulation - Juvenile - Checkout,Circulation - Juvenile - Renewed", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year_prev);
                                totalCurYearMonth += lineCurYearMonth;
                                totalCurYearTD += lineCurYearTD;
                                totalPrevYearMonth += linePrevYearMonth;
                                totalPrevYearTD += linePrevYearTD;
                                tableCirculation.AddCell(cF.createTextCell($"Youth", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableCirculation.AddCell(cF.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableCirculation.AddCell(cF.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableCirculation.AddCell(cF.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableCirculation.AddCell(cF.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                lineCurYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Online Resources - Hoopla - Downloads,Online Resources - Media on Demand - Downloads,Online Resources - Zinio", "", false, mPs.id_loc, "Adult Services", stat_month, stat_year);
                                lineCurYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Online Resources - Hoopla - Downloads,Online Resources - Media on Demand - Downloads,Online Resources - Zinio", "", false, mPs.id_loc, "Adult Services", $"year-{stat_month}", stat_year);
                                linePrevYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Online Resources - Hoopla - Downloads,Online Resources - Media on Demand - Downloads,Online Resources - Zinio", "", false, mPs.id_loc, "Adult Services", stat_month, stat_year_prev);
                                linePrevYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Online Resources - Hoopla - Downloads,Online Resources - Media on Demand - Downloads,Online Resources - Zinio", "", false, mPs.id_loc, "Adult Services", $"year-{stat_month}", stat_year_prev);
                                totalCurYearMonth += lineCurYearMonth;
                                totalCurYearTD += lineCurYearTD;
                                totalPrevYearMonth += linePrevYearMonth;
                                totalPrevYearTD += linePrevYearTD;
                                tableCirculation.AddCell(cF.createTextCell($"eRescources", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableCirculation.AddCell(cF.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableCirculation.AddCell(cF.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableCirculation.AddCell(cF.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableCirculation.AddCell(cF.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableCirculation.AddCell(cF.createTextCell($"Total", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableCirculation.AddCell(cF.createTextCell($"{totalCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableCirculation.AddCell(cF.createTextCell($"{totalCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableCirculation.AddCell(cF.createTextCell($"{totalPrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableCirculation.AddCell(cF.createTextCell($"{totalPrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
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
                                //Circulation
                                PdfPTable tableRescourceSharing = new PdfPTable(columnWidth);
                                tableRescourceSharing.TotalWidth = pdfDoc.PageSize.Width - 40;
                                tableRescourceSharing.AddCell(cF.createTextCell($"RESOURCE SHARING", fontSectionTitle, BaseColor.GRAY, 1, 1, 1, 1, 9, 90, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"Month {stat_year}", fontSectionText, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"YTD  {stat_year}", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"Month {stat_year_prev}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"YTD {stat_year_prev}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"Loans filled:", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                lineCurYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Reserved Items - Patron selfplaced holds", "", false, mPs.id_loc, "Customer Services", stat_month, stat_year);
                                lineCurYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Reserved Items - Patron selfplaced holds", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year);
                                linePrevYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Reserved Items - Patron selfplaced holds", "", false, mPs.id_loc, "Customer Services", stat_month, stat_year_prev);
                                linePrevYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Reserved Items - Patron selfplaced holds", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year_prev);
                                totalCurYearMonth += lineCurYearMonth;
                                totalCurYearTD += lineCurYearTD;
                                totalPrevYearMonth += linePrevYearMonth;
                                totalPrevYearTD += linePrevYearTD;
                                tableRescourceSharing.AddCell(cF.createTextCell($"      Patron self-placed holds", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                lineCurYearMonth = 0;
                                lineCurYearTD = 0;
                                linePrevYearMonth = 0;
                                linePrevYearTD = 0;
                                lineCurYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Interlibrary Loan - Requests from our patrons filled", "", false, mPs.id_loc, "Customer Services", stat_month, stat_year);
                                lineCurYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Interlibrary Loan - Requests from our patrons filled", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year);
                                linePrevYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Interlibrary Loan - Requests from our patrons filled", "", false, mPs.id_loc, "Customer Services", stat_month, stat_year_prev);
                                linePrevYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Interlibrary Loan - Requests from our patrons filled", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year_prev);
                                totalCurYearMonth += lineCurYearMonth;
                                totalCurYearTD += lineCurYearTD;
                                totalPrevYearMonth += linePrevYearMonth;
                                totalPrevYearTD += linePrevYearTD;
                                tableRescourceSharing.AddCell(cF.createTextCell($"      ILL Requests by patrons of OLPL", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                lineCurYearMonth = 0;
                                lineCurYearTD = 0;
                                linePrevYearMonth = 0;
                                linePrevYearTD = 0;
                                lineCurYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Interlibrary Loan - Requests from other libraries", "", false, mPs.id_loc, "Customer Services", stat_month, stat_year);
                                lineCurYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Interlibrary Loan - Requests from other libraries", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year);
                                linePrevYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Interlibrary Loan - Requests from other libraries", "", false, mPs.id_loc, "Customer Services", stat_month, stat_year_prev);
                                linePrevYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Interlibrary Loan - Requests from other libraries", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year_prev);
                                totalCurYearMonth += lineCurYearMonth;
                                totalCurYearTD += lineCurYearTD;
                                totalPrevYearMonth += linePrevYearMonth;
                                totalPrevYearTD += linePrevYearTD;
                                tableRescourceSharing.AddCell(cF.createTextCell($"      ILL Requests by patrons of {Environment.NewLine}             sharing libraries", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"Reciprocal Borrowing:", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                lineCurYearMonth = 0;
                                lineCurYearTD = 0;
                                linePrevYearMonth = 0;
                                linePrevYearTD = 0;
                                lineCurYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Circulation - Reciprocal borrowing OLPL patrons", "", false, mPs.id_loc, "Customer Services", stat_month, stat_year);
                                lineCurYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Circulation - Reciprocal borrowing OLPL patrons", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year);
                                linePrevYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Circulation - Reciprocal borrowing OLPL patrons", "", false, mPs.id_loc, "Customer Services", stat_month, stat_year_prev);
                                linePrevYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Circulation - Reciprocal borrowing OLPL patrons", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year_prev);
                                totalCurYearMonth += lineCurYearMonth;
                                totalCurYearTD += lineCurYearTD;
                                totalPrevYearMonth += linePrevYearMonth;
                                totalPrevYearTD += linePrevYearTD;
                                tableRescourceSharing.AddCell(cF.createTextCell($"      OLPL Patrons", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                lineCurYearMonth = 0;
                                lineCurYearTD = 0;
                                linePrevYearMonth = 0;
                                linePrevYearTD = 0;
                                lineCurYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Circulation - Reciprocal borrowing at OLPL", "", false, mPs.id_loc, "Customer Services", stat_month, stat_year);
                                lineCurYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Circulation - Reciprocal borrowing at OLPL", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year);
                                linePrevYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Circulation - Reciprocal borrowing at OLPL", "", false, mPs.id_loc, "Customer Services", stat_month, stat_year_prev);
                                linePrevYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Circulation - Reciprocal borrowing at OLPL", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year_prev);
                                totalCurYearMonth += lineCurYearMonth;
                                totalCurYearTD += lineCurYearTD;
                                totalPrevYearMonth += linePrevYearMonth;
                                totalPrevYearTD += linePrevYearTD;
                                tableRescourceSharing.AddCell(cF.createTextCell($"      At OLPL", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"Total", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"{totalCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"{totalCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"{totalPrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRescourceSharing.AddCell(cF.createTextCell($"{totalPrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
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
                                float[] columnWidthCollections = { 1, 8, 3, 3};
                                PdfPTable tableCollection = new PdfPTable(columnWidthCollections);
                                tableCollection.TotalWidth = pdfDoc.PageSize.Width - 40;
                                tableCollection.AddCell(cF.createTextCell($"COLLECTIONS", fontSectionTitle, BaseColor.GRAY, 1, 1, 1, 1, 5, 90, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableCollection.AddCell(cF.createTextCell($"                                           ", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableCollection.AddCell(cF.createTextCell($"{stat_year}", fontSectionText, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                tableCollection.AddCell(cF.createTextCell($"{stat_year_prev}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 0, 1, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                lineCurYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Collection - Adult", "", false, mPs.id_loc, "Administration", stat_month, stat_year);
                                linePrevYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Collection - Adult", "", false, mPs.id_loc, "Administration", stat_month, stat_year_prev);
                                totalCurYearMonth += lineCurYearMonth;
                                totalCurYearTD += lineCurYearTD;
                                totalPrevYearMonth += linePrevYearMonth;
                                totalPrevYearTD += linePrevYearTD;
                                tableCollection.AddCell(cF.createTextCell($"Adult", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableCollection.AddCell(cF.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableCollection.AddCell(cF.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                lineCurYearMonth = 0;
                                lineCurYearTD = 0;
                                linePrevYearMonth = 0;
                                linePrevYearTD = 0;
                                lineCurYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Collection - Young Adult", "", false, mPs.id_loc, "Administration", stat_month, stat_year);
                                linePrevYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Collection - Young Adult", "", false, mPs.id_loc, "Administration", stat_month, stat_year_prev);
                                totalCurYearMonth += lineCurYearMonth;
                                totalCurYearTD += lineCurYearTD;
                                totalPrevYearMonth += linePrevYearMonth;
                                totalPrevYearTD += linePrevYearTD;
                                tableCollection.AddCell(cF.createTextCell($"Young Adult", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableCollection.AddCell(cF.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableCollection.AddCell(cF.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                lineCurYearMonth = 0;
                                lineCurYearTD = 0;
                                linePrevYearMonth = 0;
                                linePrevYearTD = 0;
                                lineCurYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Collection - Youth", "", false, mPs.id_loc, "Administration", stat_month, stat_year);
                                linePrevYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Collection - Youth", "", false, mPs.id_loc, "Administration", stat_month, stat_year_prev);
                                totalCurYearMonth += lineCurYearMonth;
                                totalCurYearTD += lineCurYearTD;
                                totalPrevYearMonth += linePrevYearMonth;
                                totalPrevYearTD += linePrevYearTD;
                                tableCollection.AddCell(cF.createTextCell($"Youth", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableCollection.AddCell(cF.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableCollection.AddCell(cF.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableCollection.AddCell(cF.createTextCell($"Total", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableCollection.AddCell(cF.createTextCell($"{totalCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableCollection.AddCell(cF.createTextCell($"{totalPrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
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
                                tableRegistration.AddCell(cF.createTextCell($"REGISTRATION   {Environment.NewLine}AND TRAFFIC      ", fontSectionTitle, BaseColor.GRAY, 1, 1, 1, 1, 8, 90, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"Month {stat_year}", fontSectionText, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"YTD  {stat_year}", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"Month {stat_year_prev}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"YTD {stat_year_prev}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                lineCurYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Registration - Total Patrons", "", false, mPs.id_loc, "Customer Services", stat_month, stat_year);
                                lineCurYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Registration - Total Patrons", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year);
                                linePrevYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Registration - Total Patrons", "", false, mPs.id_loc, "Customer Services", stat_month, stat_year_prev);
                                linePrevYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Registration - Total Patrons", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year_prev);
                                totalCurYearMonth += lineCurYearMonth;
                                totalCurYearTD += lineCurYearTD;
                                totalPrevYearMonth += linePrevYearMonth;
                                totalPrevYearTD += linePrevYearTD;
                                tableRegistration.AddCell(cF.createTextCell($"Patrons registered", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                lineCurYearMonth = 0;
                                lineCurYearTD = 0;
                                linePrevYearMonth = 0;
                                linePrevYearTD = 0;
                                lineCurYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Registration - Chicago Patrons Registered", "", false, mPs.id_loc, "Customer Services", stat_month, stat_year);
                                lineCurYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Registration - Chicago Patrons Registered", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year);
                                linePrevYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Registration - Chicago Patrons Registered", "", false, mPs.id_loc, "Customer Services", stat_month, stat_year_prev);
                                linePrevYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Registration - Chicago Patrons Registered", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year_prev);
                                totalCurYearMonth += lineCurYearMonth;
                                totalCurYearTD += lineCurYearTD;
                                totalPrevYearMonth += linePrevYearMonth;
                                totalPrevYearTD += linePrevYearTD;
                                tableRegistration.AddCell(cF.createTextCell($"Chicago patrons registered", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                lineCurYearMonth = 0;
                                lineCurYearTD = 0;
                                linePrevYearMonth = 0;
                                linePrevYearTD = 0;
                                lineCurYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Circulation Desk - Telephone Calls", "", false, mPs.id_loc, "Customer Services", stat_month, stat_year);
                                lineCurYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Circulation Desk - Telephone Calls", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year);
                                linePrevYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Circulation Desk - Telephone Calls", "", false, mPs.id_loc, "Customer Services", stat_month, stat_year_prev);
                                linePrevYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Circulation Desk - Telephone Calls", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year_prev);
                                totalCurYearMonth += lineCurYearMonth;
                                totalCurYearTD += lineCurYearTD;
                                totalPrevYearMonth += linePrevYearMonth;
                                totalPrevYearTD += linePrevYearTD;
                                tableRegistration.AddCell(cF.createTextCell($"Incoming phone calls", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                lineCurYearMonth = 0;
                                lineCurYearTD = 0;
                                linePrevYearMonth = 0;
                                linePrevYearTD = 0;
                                lineCurYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Door count - East,Door count - West", "", false, mPs.id_loc, "Administration", stat_month, stat_year);
                                lineCurYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Door count - East,Door count - West", "", false, mPs.id_loc, "Administration", $"year-{stat_month}", stat_year);
                                linePrevYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Door count - East,Door count - West", "", false, mPs.id_loc, "Administration", stat_month, stat_year_prev);
                                linePrevYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Door count - East,Door count - West", "", false, mPs.id_loc, "Administration", $"year-{stat_month}", stat_year_prev);
                                totalCurYearMonth += lineCurYearMonth;
                                totalCurYearTD += lineCurYearTD;
                                totalPrevYearMonth += linePrevYearMonth;
                                totalPrevYearTD += linePrevYearTD;
                                tableRegistration.AddCell(cF.createTextCell($"Door count", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                lineCurYearMonth = 0;
                                lineCurYearTD = 0;
                                linePrevYearMonth = 0;
                                linePrevYearTD = 0;
                                lineCurYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Meeting Rooms - Booked", "", false, mPs.id_loc, "Publications", stat_month, stat_year);
                                lineCurYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Meeting Rooms - Booked", "", false, mPs.id_loc, "Publications", $"year-{stat_month}", stat_year);
                                linePrevYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Meeting Rooms - Booked", "", false, mPs.id_loc, "Publications", stat_month, stat_year_prev);
                                linePrevYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Meeting Rooms - Booked", "", false, mPs.id_loc, "Publications", $"year-{stat_month}", stat_year_prev);
                                totalCurYearMonth += lineCurYearMonth;
                                totalCurYearTD += lineCurYearTD;
                                totalPrevYearMonth += linePrevYearMonth;
                                totalPrevYearTD += linePrevYearTD;
                                tableRegistration.AddCell(cF.createTextCell($"Meeting room usage", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                lineCurYearMonth = 0;
                                lineCurYearTD = 0;
                                linePrevYearMonth = 0;
                                linePrevYearTD = 0;
                                lineCurYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Group Study", "", false, mPs.id_loc, "Computer Lab", stat_month, stat_year);
                                lineCurYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Group Study", "", false, mPs.id_loc, "Computer Lab", $"year-{stat_month}", stat_year);
                                linePrevYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Group Study", "", false, mPs.id_loc, "Computer Lab", stat_month, stat_year_prev);
                                linePrevYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Group Study", "", false, mPs.id_loc, "Computer Lab", $"year-{stat_month}", stat_year_prev);
                                totalCurYearMonth += lineCurYearMonth;
                                totalCurYearTD += lineCurYearTD;
                                totalPrevYearMonth += linePrevYearMonth;
                                totalPrevYearTD += linePrevYearTD;
                                tableRegistration.AddCell(cF.createTextCell($"Group study room usage", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"Total", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{totalCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{totalCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{totalPrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableRegistration.AddCell(cF.createTextCell($"{totalPrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
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
                                tableComputerUse.AddCell(cF.createTextCell($"COMPUTER USE {Environment.NewLine} (SESSIONS)", fontSectionTitle, BaseColor.GRAY, 1, 1, 1, 1, 5, 90, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                tableComputerUse.AddCell(cF.createTextCell($"", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableComputerUse.AddCell(cF.createTextCell($"Month {stat_year}", fontSectionText, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableComputerUse.AddCell(cF.createTextCell($"YTD  {stat_year}", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableComputerUse.AddCell(cF.createTextCell($"Month {stat_year_prev}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableComputerUse.AddCell(cF.createTextCell($"YTD {stat_year_prev}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                lineCurYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Computer Sessions", "", false, mPs.id_loc, "Computer LAB", stat_month, stat_year);
                                lineCurYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Computer Sessions", "", false, mPs.id_loc, "Computer LAB", $"year-{stat_month}", stat_year);
                                linePrevYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Computer Sessions", "", false, mPs.id_loc, "Computer LAB", stat_month, stat_year_prev);
                                linePrevYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Computer Sessions", "", false, mPs.id_loc, "Computer LAB", $"year-{stat_month}", stat_year_prev);
                                totalCurYearMonth += lineCurYearMonth;
                                totalCurYearTD += lineCurYearTD;
                                totalPrevYearMonth += linePrevYearMonth;
                                totalPrevYearTD += linePrevYearTD;
                                tableComputerUse.AddCell(cF.createTextCell($"Adult & Young Adult Services", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableComputerUse.AddCell(cF.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableComputerUse.AddCell(cF.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableComputerUse.AddCell(cF.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableComputerUse.AddCell(cF.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                lineCurYearMonth = 0;
                                lineCurYearTD = 0;
                                linePrevYearMonth = 0;
                                linePrevYearTD = 0;
                                lineCurYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Computer Usage - Sessions", "", false, mPs.id_loc, "Youth Services", stat_month, stat_year);
                                lineCurYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Computer Usage - Sessions", "", false, mPs.id_loc, "Youth Services", $"year-{stat_month}", stat_year);
                                linePrevYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Computer Usage - Sessions", "", false, mPs.id_loc, "Youth Services", stat_month, stat_year_prev);
                                linePrevYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Computer Usage - Sessions", "", false, mPs.id_loc, "Youth Services", $"year-{stat_month}", stat_year_prev);
                                totalCurYearMonth += lineCurYearMonth;
                                totalCurYearTD += lineCurYearTD;
                                totalPrevYearMonth += linePrevYearMonth;
                                totalPrevYearTD += linePrevYearTD;
                                tableComputerUse.AddCell(cF.createTextCell($"Youth Services", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableComputerUse.AddCell(cF.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableComputerUse.AddCell(cF.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableComputerUse.AddCell(cF.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableComputerUse.AddCell(cF.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                lineCurYearMonth = 0;
                                lineCurYearTD = 0;
                                linePrevYearMonth = 0;
                                linePrevYearTD = 0;
                                lineCurYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Wireless Sessions", "", false, mPs.id_loc, "Computer LAB", stat_month, stat_year);
                                lineCurYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Wireless Sessions", "", false, mPs.id_loc, "Computer LAB", $"year-{stat_month}", stat_year);
                                linePrevYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Wireless Sessions", "", false, mPs.id_loc, "Computer LAB", stat_month, stat_year_prev);
                                linePrevYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Wireless Sessions", "", false, mPs.id_loc, "Computer LAB", $"year-{stat_month}", stat_year_prev);
                                totalCurYearMonth += lineCurYearMonth;
                                totalCurYearTD += lineCurYearTD;
                                totalPrevYearMonth += linePrevYearMonth;
                                totalPrevYearTD += linePrevYearTD;
                                tableComputerUse.AddCell(cF.createTextCell($"Wireless ", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableComputerUse.AddCell(cF.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableComputerUse.AddCell(cF.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableComputerUse.AddCell(cF.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableComputerUse.AddCell(cF.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableComputerUse.AddCell(cF.createTextCell($"Total", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableComputerUse.AddCell(cF.createTextCell($"{totalCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableComputerUse.AddCell(cF.createTextCell($"{totalCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableComputerUse.AddCell(cF.createTextCell($"{totalPrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableComputerUse.AddCell(cF.createTextCell($"{totalPrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
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
                                tableSocialMedia.AddCell(cF.createTextCell($"ONLINE USE", fontSectionTitle, BaseColor.GRAY, 1, 1, 1, 1, 4, 90, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                tableSocialMedia.AddCell(cF.createTextCell($"", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableSocialMedia.AddCell(cF.createTextCell($"Month {stat_year}", fontSectionText, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableSocialMedia.AddCell(cF.createTextCell($"YTD  {stat_year}", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableSocialMedia.AddCell(cF.createTextCell($"Month {stat_year_prev}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableSocialMedia.AddCell(cF.createTextCell($"YTD {stat_year_prev}", fontSectionText, BaseColor.LIGHT_GRAY, 1, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                lineCurYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Website - Hits", "", false, mPs.id_loc, "Publications", stat_month, stat_year);
                                lineCurYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Website - Hits", "", false, mPs.id_loc, "Publications", $"year-{stat_month}", stat_year);
                                linePrevYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Website - Hits", "", false, mPs.id_loc, "Publications", stat_month, stat_year_prev);
                                linePrevYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Website - Hits", "", false, mPs.id_loc, "Publications", $"year-{stat_month}", stat_year_prev);
                                tableSocialMedia.AddCell(cF.createTextCell($"Web Page Unique Visitors", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableSocialMedia.AddCell(cF.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableSocialMedia.AddCell(cF.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableSocialMedia.AddCell(cF.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableSocialMedia.AddCell(cF.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                lineCurYearMonth = 0;
                                lineCurYearTD = 0;
                                linePrevYearMonth = 0;
                                linePrevYearTD = 0;
                                lineCurYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Facebook - Followers,Instagram - Followers,Pintrest - Followers", "", false, mPs.id_loc, "Publications,Audio Visual,Local History,Young Adult", stat_month, stat_year);
                                lineCurYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Facebook - Followers,Instagram - Followers,Pintrest - Followers", "", false, mPs.id_loc, "Publications,Audio Visual,Local History,Young Adult", $"year-{stat_month}", stat_year);
                                linePrevYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Facebook - Followers,Instagram - Followers,Pintrest - Followers", "", false, mPs.id_loc, "Publications,Audio Visual,Local History,Young Adult", stat_month, stat_year_prev);
                                linePrevYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Facebook - Followers,Instagram - Followers,Pintrest - Followers", "", false, mPs.id_loc, "Publications,Audio Visual,Local History,Young Adult", $"year-{stat_month}", stat_year_prev);
                                tableSocialMedia.AddCell(cF.createTextCell($"Social Networking & Marketing (Followers)", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableSocialMedia.AddCell(cF.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableSocialMedia.AddCell(cF.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableSocialMedia.AddCell(cF.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableSocialMedia.AddCell(cF.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                lineCurYearMonth = 0;
                                lineCurYearTD = 0;
                                linePrevYearMonth = 0;
                                linePrevYearTD = 0;
                                lineCurYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Online Resources - Ancestry - Sessions,Online Resources - Ebsco - Sessions,Online Resources - Heritage Quest - Sessions,Online Resources - Heritage Quest - Sessions,Online Resources - Gale Virtual Reference - Views,Online Resources - Morningstar - Sessions,Online Resources - Novelist Plus - Sessions,Online Resources - Proquest ChgoTrib - Sessions,Online Resources - Value Line - Report Views,Online Resources - Newsbank - Logins,Online Resources - Chicago Record Information Serv - Unique Visitors,Online Resources - All Data Articles,Online Resources - Kanopy - Plays", "", false, mPs.id_loc, "Audio Visual,Adult Services", stat_month, stat_year);
                                lineCurYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Online Resources - Ancestry - Sessions,Online Resources - Ebsco - Sessions,Online Resources - Heritage Quest - Sessions,Online Resources - Heritage Quest - Sessions,Online Resources - Gale Virtual Reference - Views,Online Resources - Morningstar - Sessions,Online Resources - Novelist Plus - Sessions,Online Resources - Proquest ChgoTrib - Sessions,Online Resources - Value Line - Report Views,Online Resources - Newsbank - Logins,Online Resources - Chicago Record Information Serv - Unique Visitors,Online Resources - All Data Articles,Online Resources - Kanopy - Plays", "", false, mPs.id_loc, "Audio Visual,Adult Services", $"year-{stat_month}", stat_year);
                                linePrevYearMonth = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Online Resources - Ancestry - Sessions,Online Resources - Ebsco - Sessions,Online Resources - Heritage Quest - Sessions,Online Resources - Heritage Quest - Sessions,Online Resources - Gale Virtual Reference - Views,Online Resources - Morningstar - Sessions,Online Resources - Novelist Plus - Sessions,Online Resources - Proquest ChgoTrib - Sessions,Online Resources - Value Line - Report Views,Online Resources - Newsbank - Logins,Online Resources - Chicago Record Information Serv - Unique Visitors,Online Resources - All Data Articles,Online Resources - Kanopy - Plays", "", false, mPs.id_loc, "Audio Visual,Adult Services", stat_month, stat_year_prev);
                                linePrevYearTD = (int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Online Resources - Ancestry - Sessions,Online Resources - Ebsco - Sessions,Online Resources - Heritage Quest - Sessions,Online Resources - Heritage Quest - Sessions,Online Resources - Gale Virtual Reference - Views,Online Resources - Morningstar - Sessions,Online Resources - Novelist Plus - Sessions,Online Resources - Proquest ChgoTrib - Sessions,Online Resources - Value Line - Report Views,Online Resources - Newsbank - Logins,Online Resources - Chicago Record Information Serv - Unique Visitors,Online Resources - All Data Articles,Online Resources - Kanopy - Plays", "", false, mPs.id_loc, "Audio Visual,Adult Services", $"year-{stat_month}", stat_year_prev);
                                tableSocialMedia.AddCell(cF.createTextCell($"Online Resources (Sessions)", fontSectionText, BaseColor.WHITE, 0, 1, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableSocialMedia.AddCell(cF.createTextCell($"{lineCurYearMonth.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableSocialMedia.AddCell(cF.createTextCell($"{lineCurYearTD.ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableSocialMedia.AddCell(cF.createTextCell($"{linePrevYearMonth.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableSocialMedia.AddCell(cF.createTextCell($"{linePrevYearTD.ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
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
                                tableProgramming.AddCell(cF.createTextCell($"PROGRAMMING", fontSectionTitle, BaseColor.GRAY, 1, 1, 1, 1, 10, 90, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"Attendance {stat_year}", fontSectionTextSmall, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"# of Programs {stat_year}", fontSectionTextSmall, BaseColor.WHITE, 1, 0, 1, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"# of Programs YTD {stat_year}", fontSectionTextSmall, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"Attendance {stat_year_prev}", fontSectionTextSmall, BaseColor.LIGHT_GRAY, 1, 0, 1, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"# of Programs {stat_year_prev}", fontSectionTextSmall, BaseColor.LIGHT_GRAY, 1, 0, 1, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"# of Programs YTD {stat_year_prev}", fontSectionTextSmall, BaseColor.LIGHT_GRAY, 1, 1, 1, 0, 0, 0, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"Adult Services", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"   Library sponsored", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Attendance", "", false, mPs.id_loc, "Adult Services,Computer Lab,Local History", $"year-{stat_month}", stat_year)).ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Programs", "", false, mPs.id_loc, "Adult Services,Computer Lab,Local History", stat_month, stat_year)).ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_RIGHT));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Programs", "", false, mPs.id_loc, "Adult Services,Computer Lab,Local History", $"year-{stat_month}", stat_year)).ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Attendance", "", false, mPs.id_loc, "Adult Services,Computer Lab,Local History", $"year-{stat_month}", stat_year_prev)).ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Programs", "", false, mPs.id_loc, "Adult Services,Computer Lab,Local History", stat_month, stat_year_prev)).ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Programs", "", false, mPs.id_loc, "Adult Services,Computer Lab,Local History", $"year-{stat_month}", stat_year_prev)).ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"   Community outreach", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Community Outreach - Att", "", false, mPs.id_loc, "Adult Services,Computer Lab,Local History", $"year-{stat_month}", stat_year)).ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Community Outreach - Prog", "", false, mPs.id_loc, "Adult Services,Computer Lab,Local History", stat_month, stat_year)).ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Community Outreach - Prog", "", false, mPs.id_loc, "Adult Services,Computer Lab,Local History", $"year-{stat_month}", stat_year)).ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Community Outreach - Att", "", false, mPs.id_loc, "Adult Services,Computer Lab,Local History", $"year-{stat_month}", stat_year_prev)).ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Community Outreach - Prog", "", false, mPs.id_loc, "Adult Services,Computer Lab,Local History", stat_month, stat_year_prev)).ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Community Outreach - Prog", "", false, mPs.id_loc, "Adult Services,Computer Lab,Local History", $"year-{stat_month}", stat_year_prev)).ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"   Computer classes", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Attendance Classes", "", false, mPs.id_loc, "Computer Lab", $"year-{stat_month}", stat_year)).ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Classes", "", false, mPs.id_loc, "Computer Lab", stat_month, stat_year)).ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Classes", "", false, mPs.id_loc, "Computer Lab", $"year-{stat_month}", stat_year)).ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Attendance Classes", "", false, mPs.id_loc, "Computer Lab", $"year-{stat_month}", stat_year_prev)).ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Classes", "", false, mPs.id_loc, "Computer Lab", stat_month, stat_year_prev)).ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Classes", "", false, mPs.id_loc, "Computer Lab", $"year-{stat_month}", stat_year_prev)).ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"Young Adult Services", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Attendance", "", false, mPs.id_loc, "Young Adult", $"year-{stat_month}", stat_year)).ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Programs", "", false, mPs.id_loc, "Young Adult", stat_month, stat_year)).ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Programs", "", false, mPs.id_loc, "Young Adult", $"year-{stat_month}", stat_year)).ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Attendance", "", false, mPs.id_loc, "Young Adult", $"year-{stat_month}", stat_year_prev)).ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Programs", "", false, mPs.id_loc, "Young Adult", stat_month, stat_year_prev)).ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Programs", "", false, mPs.id_loc, "Young Adult", $"year-{stat_month}", stat_year_prev)).ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"Youth Services", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"   Library sponsored", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Youth Programs - Library Sponsored - Attendance,Youth Programs - Crafts Contests Activities - Attendance,Youth Programs - Passive Programming - Attendance", "", false, mPs.id_loc, "Youth Services", $"year-{stat_month}", stat_year)).ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Youth Programs - Library Sponsored - Number,Youth Programs - Crafts Contests Activities - Number,Youth Programs - Passive Programming - Number", "", false, mPs.id_loc, "Youth Services", stat_month, stat_year)).ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Youth Programs - Library Sponsored - Number,Youth Programs - Crafts Contests Activities - Number,Youth Programs - Passive Programming - Number", "", false, mPs.id_loc, "Youth Services", $"year-{stat_month}", stat_year)).ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Youth Programs - Library Sponsored - Attendance,Youth Programs - Crafts Contests Activities - Attendance,Youth Programs - Passive Programming - Attendance", "", false, mPs.id_loc, "Youth Services", $"year-{stat_month}", stat_year_prev)).ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Youth Programs - Library Sponsored - Number,Youth Programs - Crafts Contests Activities - Number,Youth Programs - Passive Programming - Number", "", false, mPs.id_loc, "Youth Services", stat_month, stat_year_prev)).ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Youth Programs - Library Sponsored - Number,Youth Programs - Crafts Contests Activities - Number,Youth Programs - Passive Programming - Number", "", false, mPs.id_loc, "Youth Services", $"year-{stat_month}", stat_year_prev)).ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"   Community outreach", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Youth Programs - Com. Outreach Internal - Attendance,Youth Programs - Com. Outreach External - Attendance", "", false, mPs.id_loc, "Youth Services", $"year-{stat_month}", stat_year)).ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Youth Programs - Com. Outreach Internal - Number,Youth Programs - Com. Outreach External - Number", "", false, mPs.id_loc, "Youth Services", stat_month, stat_year)).ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Youth Programs - Com. Outreach Internal - Number,Youth Programs - Com. Outreach External - Number", "", false, mPs.id_loc, "Youth Services", $"year-{stat_month}", stat_year)).ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Youth Programs - Com. Outreach Internal - Attendance,Youth Programs - Com. Outreach External - Attendance", "", false, mPs.id_loc, "Youth Services", $"year-{stat_month}", stat_year_prev)).ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Youth Programs - Com. Outreach Internal - Number,Youth Programs - Com. Outreach External - Number", "", false, mPs.id_loc, "Youth Services", stat_month, stat_year_prev)).ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Youth Programs - Com. Outreach Internal - Number,Youth Programs - Com. Outreach External - Number", "", false, mPs.id_loc, "Youth Services", $"year-{stat_month}", stat_year_prev)).ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 0, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"Homebound Visits", fontSectionText, BaseColor.WHITE, 0, 1, 1, 0, 0, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Homebound - Visits", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year)).ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Homebound - Visits", "", false, mPs.id_loc, "Customer Services", stat_month, stat_year)).ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Homebound - Visits", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year)).ToString("#,###")}", fontSectionText, BaseColor.WHITE, 0, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Homebound - Visits", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year_prev)).ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Homebound - Visits", "", false, mPs.id_loc, "Customer Services", stat_month, stat_year_prev)).ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 0, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.AddCell(cF.createTextCell($"{((int)cStats.statControl(dataSettings, controlStats.getCOuntMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Homebound - Visits", "", false, mPs.id_loc, "Customer Services", $"year-{stat_month}", stat_year_prev)).ToString("#,###")}", fontSectionText, BaseColor.LIGHT_GRAY, 0, 1, 1, 0, 0, 0, Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE));
                                tableProgramming.WriteSelectedRows(0, -1, 20, pdfDoc.PageSize.Height - currentHeight, writer.DirectContent);
                                currentHeight += tableProgramming.TotalHeight + 20;
                                // Programming
                                float[] columnWidthHighlights = { 1, 13 };
                                PdfPTable tableHighlights = new PdfPTable(columnWidthHighlights);
                                tableHighlights.TotalWidth = pdfDoc.PageSize.Width - 40;
                                string strHi = (string)cStats.statControl(dataSettings, controlStats.getStringMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, "Highlights(string)", "", false, mPs.id_loc, "Administration", stat_month, stat_year);
                                if (strHi.ToLower().Contains("not reported"))
                                {
                                    strHi = "";
                                }
                                tableHighlights.AddCell(cF.createTextCell($"HIGHLIGHTS", fontSectionTitle, BaseColor.GRAY, 1, 1, 1, 1, 1, 90, Element.ALIGN_LEFT, Element.ALIGN_MIDDLE));
                                tableHighlights.AddCell(cF.createTextCell($"{strHi}", fontSectionText, BaseColor.WHITE, 1, 1, 1, 0, 0, 0, Element.ALIGN_JUSTIFIED, Element.ALIGN_MIDDLE));
                                tableHighlights.WriteSelectedRows(0, -1, 20, pdfDoc.PageSize.Height - currentHeight, writer.DirectContent);
                                currentHeight += tableHighlights.TotalHeight + 20;
                                pdfDoc.Close();
                                System.Diagnostics.Process.Start(saveFileDialog.FileName);
                            }
                        }
                    }
                }
                catch (Exception eW)
                {
                    MessageBox.Show($"Error Monthly Stat Report: {eW.ToString()}");
                    cL.fileWriteLog($"Error Monthly Stat Report: {eW.ToString()}", "STAT", "formGetStats", dataSettings);
                }
            }
            else
            {
                MessageBox.Show("Please select Year and Month to get monthly report!");
            }
        }

        private void btnLocationReport_Click(object sender, EventArgs e)
        {
            if (cbInputMonth.SelectedIndex != 0 && cbInputYear.SelectedIndex != 0 && cbLocation.SelectedIndex != 0)
            {
                this.Controls.Remove(this.Controls["tlStats"]);
                TableLayoutPanel tlStats = new TableLayoutPanel();
                tlStats.Location = new Point(15, 100);
                tlStats.Size = new Size(135, 34);
                tlStats.AutoSize = true;
                tlStats.Name = "tlStats";
                tlStats.ColumnCount = 2;
                tlStats.RowCount = 1;
                tlStats.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
                tlStats.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.AddRows;
                int counter = 0;
                foreach (modelLocation mLoc in lMl)
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
                                                t1.Enabled = false;
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
                                            t1.Enabled = false;
                                        }
                                    }
                                }
                                if (t1.Text.Length < 1)
                                {
                                    t1.Text = "Not Reported";
                                }
                                t1.Enabled = false;
                                l1.AutoSize = true;
                                tlStats.AutoSize = true;
                                l1.Font = new Font("Arial", 13, FontStyle.Regular);
                                t1.Font = new Font("Arial", 13, FontStyle.Regular);
                                tlStats.Controls.Add(l1, 0, counter);
                                tlStats.Controls.Add(t1, 1, counter);
                                counter++;
                            }
                        }
                    }
                }
                if (cbLocation.SelectedIndex != 0)
                {
                    this.Controls.Add(tlStats);
                    if (this.Width < tlStats.Width)
                    {
                        this.Width = tlStats.Width + 50;
                    }
                    if (this.Height < tlStats.Height + 200)
                    {
                        int monHeight = Screen.FromControl(this).WorkingArea.Height;
                        if (tlStats.Height > monHeight - 200)
                        {
                            this.Height = monHeight - 200;
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
            else
            {
                MessageBox.Show("Please Select all drop downs to get stats!");
            }
        }
    }
}
