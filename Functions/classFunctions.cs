using DLL_Support;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Image = iTextSharp.text.Image;

namespace APP_DeskStats.Functions
{
    class classFunctions
    {
        public string locIDtoString(List<modelLocation> lMl, int id)
        {
            foreach(modelLocation mL in lMl)
            {
                if (mL.id == id) { return mL.loc_name; }
            }
            return "NONE";
        }
        public int locStringToID(List<modelLocation> lMl, string name)
        {
            foreach (modelLocation mL in lMl)
            {
                if (mL.loc_name  == name) { return mL.id; }
            }
            return 0;
        }
        public List<string> getButtonList(List<modelLocation> lMl, int id)
        {
            List<string> lStr = new List<string>();
            foreach(modelLocation mL in lMl)
            {
                if(mL.id == id)
                {
                    lStr = mL.loc_properties;
                }
            }
            return lStr;
        }
        private void DynamicButton_Click(object sender, EventArgs e, Form passedForm)
        {
            controlStats cStats = new controlStats();
            Form1 frmMain = (Form1)Application.OpenForms["Form1"];
            frmMain.timerHide = 0;
            modelStat mPs = frmMain.mUd;
            mPs.cpu_name = Environment.MachineName;
            mPs.id_loc = frmMain.mUd.id_loc;
            mPs.id_user = frmMain.mUd.id;
            mPs.stat_type = (sender as Button).Text.Split('(')[0].Trim();
            if ((string)(sender as Button).Tag == "time")
            {
                using (var frmComment = new frmGetComment())
                {
                    frmComment.valType = "number";
                    frmComment.label1.Text = "Amount of time spent:";
                    var result = frmComment.ShowDialog();
                    mPs.stat_comment = frmComment.textBox1.Text;
                }
            }
            if ((string)(sender as Button).Tag == "string")
            {
                using (var frmComment = new frmGetComment())
                {
                    frmComment.valType = "";
                    frmComment.label1.Text = "Enter Comment:";
                    var result = frmComment.ShowDialog();
                    mPs.stat_comment = frmComment.textBox1.Text;
                }
            }
            cStats.statControl(frmMain.dataSettings, controlStats.postStat, "STATS", "FormSetUserPreference", passedForm, null, mPs.stat_type, mPs.stat_comment, false, mPs.id_user);
            if (frmMain.BackColor == Color.Blue) { frmMain.BackColor = Color.White; }
            else if (frmMain.BackColor == Color.White) { frmMain.BackColor = Color.Blue; }
            string strCount = (sender as Button).Text.Split('(')[1].Replace(")",string.Empty);
            int countButton = 0;
            if(int.TryParse(strCount,out countButton))
            {
                countButton++;
            }
            (sender as Button).Text = $"{mPs.stat_type}({countButton.ToString()})";
        }
        public void setFormButtons(List<modelLocation> lMl, int id, Form1 frmMain)
        {
            frmMain.Width = 325;
            frmMain.Height = 68;
            frmMain.Controls.Remove(frmMain.Controls["tlMain"]);
            TableLayoutPanel tlMain = new TableLayoutPanel();
            tlMain.Location = new Point(30, 29);
            tlMain.Size = new Size(135, 34);
            tlMain.AutoSize = true;
            tlMain.Name = "tlMain";
            tlMain.ColumnCount = 0;
            tlMain.RowCount = 1;
            tlMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tlMain.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.AddColumns;
          
            List<string> lStr = getButtonList(lMl, id);
            int counter = 0;
            int widthForm = 30;
            int widthTable = 20;
            foreach(string str in lStr)
            {
                
                Button b1 = new Button();
                if (str.ToLower().Contains("(time)")) { b1.Tag = "time"; b1.Text = str.Replace("(time)", string.Empty); }
                else if (str.ToLower().Contains("(string)")) { b1.Tag = "string"; b1.Text = str.Replace("(string)", string.Empty); }
                else { b1.Text = str; }
                b1.Text = $"{b1.Text}(0)";
                b1.AutoSize = true;
                b1.Height = 30;
                b1.BackColor = Color.LightGray;
                b1.Font= new System.Drawing.Font("Arial", 13, FontStyle.Regular);
                b1.Click += delegate (object sender, EventArgs e) { DynamicButton_Click(sender, e, frmMain); };
                tlMain.Controls.Add(b1, counter, 0);
                widthForm += b1.Width+=9;
                widthTable += b1.Width+=9;
                if (frmMain.Width < widthForm) { frmMain.Width = widthForm; }
                if (tlMain.Width < widthTable) { tlMain.Width = widthTable; }
                counter++;
            }
            frmMain.Controls.Add(tlMain);
            Screen rightmost = Screen.AllScreens[0];
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.WorkingArea.Right > rightmost.WorkingArea.Right)
                    rightmost = screen;
            }

            frmMain.Left = rightmost.WorkingArea.Right - frmMain.Width;
            frmMain.Top = rightmost.WorkingArea.Bottom - frmMain.Height;

        }
        public PdfPCell createImageCell(Image image, int widthBorder = 0, int hAlign=Element.ALIGN_LEFT, int vAlign=Element.ALIGN_TOP)
        {
            PdfPCell pCell = new PdfPCell();
            pCell.AddElement(image);
            pCell.HorizontalAlignment = hAlign;
            pCell.VerticalAlignment = vAlign;
            pCell.BorderWidthBottom = widthBorder;
            pCell.BorderWidthLeft = widthBorder;
            pCell.BorderWidthTop = widthBorder;
            pCell.BorderWidthRight = widthBorder;
            return pCell;
        }
        public PdfPCell createTextCell(string data, iTextSharp.text.Font font, BaseColor baseColor, float borderTop, float borderRight, float borderBottom, float borderLeft, int rowSpan=0, int rotation=0,int hAlign = Element.ALIGN_LEFT, int vAlign = Element.ALIGN_TOP, int columnSpan=0)
        {
            PdfPCell pCell = new PdfPCell(new Phrase(data,font));
            pCell.HorizontalAlignment = hAlign;
            pCell.VerticalAlignment = vAlign;
            pCell.BorderWidthBottom = borderBottom;
            pCell.BorderWidthLeft = borderLeft;
            pCell.BorderWidthTop = borderTop;
            pCell.BorderWidthRight = borderRight;
            pCell.BackgroundColor = baseColor;
            if (rowSpan > 0)
            {
                pCell.Rowspan = rowSpan;
                pCell.NoWrap = true;
            }
            if (columnSpan > 0)
            {
                pCell.Colspan = columnSpan;
            }
            if(rotation > 0) { pCell.Rotation = rotation; }
            return pCell;
        }
    }
}
