using DLL_Support;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IniParser.Model;
using System.Windows.Interop;

namespace APP_DeskStats
{
    public partial class frmInputStats : Form
    {
        controlStats cStats = new controlStats();
        controlSettings cS = new controlSettings();
        internal IniDataCaseInsensitive dataSettings;
        List<modelLocation> lMl;
        DataTable dtResults = new DataTable();
        public modelStat mPs;
        public frmInputStats()
        {
            InitializeComponent();
        }

        private void frmInputStats_Load(object sender, EventArgs e)
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
                                t1.Leave += delegate (object sender1, EventArgs e1) { Textbox_Leave(sender, e, this,t1); };
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
                                    string strAuto = cS.getValue("STATS", "autoPullInt", dataSettings).ToUpper();
                                    if (cS.getValue("STATS", "autoPullInt", dataSettings).ToUpper().Contains(strMatch.ToUpper()))
                                    {
                                        int results = (int)cStats.statControl(dataSettings, controlStats.getCountStat, "STATS", "FormInputStatsGetCount", this, lMl, strMatch, mPs.stat_comment, false, mPs.id_loc, mPs.location, mPs.month, mPs.year);
                                        if (results > 0)
                                        {
                                            t1.Text = results.ToString();
                                        }
                                    }
                                    else if (cS.getValue("STATS", "autoPullTime", dataSettings).ToUpper().Contains(strMatch))
                                    {

                                    }
                                }
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
                    Button b1 = new Button();
                    b1.AutoSize = true;
                    b1.Height = 30;
                    b1.BackColor = Color.LightGray;
                    b1.Text = "Submit";
                    b1.Font = new Font("Arial", 13, FontStyle.Regular);
                    b1.Click += delegate (object sender1, EventArgs e1) { DynamicButton_Click(sender, e, this); };
                    tlStats.Controls.Add(b1, 0, counter);
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
                if(cbInputMonth.SelectedIndex == 0)
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
        private void Textbox_Enter(object sender, EventArgs e, Form passedForm,TextBox t2)
        {
            if(t2.Text=="Not Reported")
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
                            bool upload = false;
                            mPs.stat_type = (String)cIn.Name;
                            int countInt = 0;
                            if (cIn.Tag.ToString().ToLower().Contains("string"))
                            {
                                if (!cIn.Text.Contains("Not Reported"))
                                {
                                    mPs.count = 0;
                                    mPs.stat_comment = cIn.Text;
                                    upload = true;
                                }
                            }
                            else
                            {
                                if (int.TryParse(cIn.Text, out countInt))
                                {
                                    mPs.count = countInt;
                                    upload = true;
                                }
                                else
                                {
                                    if (cIn.Text != "Not Reported")
                                    {
                                        MessageBox.Show($"Input for {mPs.stat_type} is not a number and will not be uploaded");
                                    }
                                }
                            }
                            if (upload)
                            {
                                output += $"Stat: {mPs.stat_type}: Result:{cStats.statControl(dataSettings, controlStats.postMonthlyStat, "STATS", "FormPostMonthlyStats", this, lMl, mPs.stat_type, mPs.stat_comment, false, mPs.id_loc, mPs.location, mPs.month, mPs.year, mPs.count)}{Environment.NewLine}";
                            }
                        }
                    }
                }
            }
            MessageBox.Show(output);
        }
    }
}
