using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace APP_DeskStats
{
    public partial class frmGetComment : Form
    {
        internal string valType;
        public frmGetComment()
        {
            InitializeComponent();
            valType = "";
        }

        private void frmGetComment_Load(object sender, EventArgs e)
        {

        }

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
                this.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
