using APP_DeskStats.Functions;
using DLL_Support;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace APP_DeskStats
{
    public partial class FormsetUserPreferences : Form
    {
        private modelStat _mU;
        controlStats cStats = new controlStats();
        public modelStat mU
        {
            get { return _mU; }
            set { _mU = value; }
        }
        private  List<modelLocation> _lMl;
        public  List<modelLocation> lMl
        {
            get { return _lMl; }
            set { _lMl = value; }
        }
        private IniDataCaseInsensitive _dataSettings;
        public IniDataCaseInsensitive dataSettings
        {
            get { return _dataSettings; }
            set { _dataSettings = value; }
        }
        private bool _upload;
        public bool upload
        {
            get { return _upload; }
            set { _upload = value; }
        }
        public FormsetUserPreferences()
        {
            InitializeComponent();
            mU = new modelStat();
        }

        private void setUserPreferences_Load(object sender, EventArgs e)
        {
            timer1.Interval = 1000;
            timer1.Start();
            Form1 form1 = (Form1)Application.OpenForms["Form1"];
            if (lMl == null)
            {
                MessageBox.Show("Error getting location list from main form");
                this.Close();
                form1.error = "error";
            }
            foreach(modelLocation mL in lMl)
            {
                cbSelectLocation.Items.Add(mL.loc_name);
                if(mU.id_loc == mL.id)
                {
                    cbSelectLocation.SelectedIndex = cbSelectLocation.FindStringExact(mL.loc_name);
                }
            }
            settingAutoHideED.SelectedIndex = mU.autohide_enable;
            settingAutoHideTimeout.Text = mU.autohide_timeout.ToString();

        }

        private void btSave_Click(object sender, EventArgs e)
        {
            Form1 form1 = (Form1)Application.OpenForms["Form1"];
            if (cbSelectLocation.Text.Length < 5) { MessageBox.Show("Please select location"); }
            else
            {
                classFunctions cF = new classFunctions();
                foreach(modelLocation mL in lMl)
                {
                    if (cbSelectLocation.Text == mL.loc_name)
                    {
                        mU.id_loc = mL.id;
                        mU = (modelStat)cStats.statControl(dataSettings, controlStats.setUserPreferences, "STATS", "FormSetUserPreference",this, null, "", "", true, mU.id_loc,"none","none","none",0,settingAutoHideED.SelectedIndex,int.Parse(settingAutoHideTimeout.Text),0,"");
                        mU.autohide_enable = settingAutoHideED.SelectedIndex;
                        int tempInt = 0;
                        if (int.TryParse(settingAutoHideTimeout.Text, out tempInt))
                        {
                            mU.autohide_timeout = tempInt;
                        }
                        else
                        {
                            mU.autohide_timeout = 120;
                        }
                        form1.mUd = mU;
                        form1.lbLocation.Text = mL.loc_name;
                        this.Close();
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 form1 = (Form1)Application.OpenForms["Form1"];
            if (cbSelectLocation.Text.Length < 5) { MessageBox.Show("Please select location"); }
            else
            {
                classFunctions cF = new classFunctions();
                foreach (modelLocation mL in lMl)
                {
                    if (cbSelectLocation.Text == mL.loc_name)
                    {
                        mU.id_loc = mL.id;
                        mU.autohide_enable = settingAutoHideED.SelectedIndex;
                        int tempInt = 0;
                        if(int.TryParse(settingAutoHideTimeout.Text,out tempInt))
                        {
                            mU.autohide_timeout = tempInt;
                        }
                        else
                        {
                            mU.autohide_timeout = 120;
                        }
                        form1.mUd = mU;
                        form1.lbLocation.Text = mL.loc_name;
                        this.Close();
                    }
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(settingAutoHideED.Text.ToLower() == "true")
            {
                settingAutoHideTimeout.Enabled = true;
            }
            else
            {
                settingAutoHideTimeout.Enabled = false;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
