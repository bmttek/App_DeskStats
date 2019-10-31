namespace APP_DeskStats.WinForms
{
    partial class FDesktop
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnInputStats = new System.Windows.Forms.Button();
            this.tlMain = new System.Windows.Forms.TableLayoutPanel();
            this.btChangeLoc = new System.Windows.Forms.Button();
            this.lbLocation = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btnClose = new System.Windows.Forms.Button();
            this.pbHide = new System.Windows.Forms.PictureBox();
            this.btnGetStats = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pbHide)).BeginInit();
            this.SuspendLayout();
            // 
            // btnInputStats
            // 
            this.btnInputStats.BackColor = System.Drawing.Color.LightGray;
            this.btnInputStats.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnInputStats.Location = new System.Drawing.Point(37, 3);
            this.btnInputStats.Name = "btnInputStats";
            this.btnInputStats.Size = new System.Drawing.Size(85, 27);
            this.btnInputStats.TabIndex = 11;
            this.btnInputStats.Text = "Input Stats";
            this.btnInputStats.UseVisualStyleBackColor = false;
            this.btnInputStats.Click += new System.EventHandler(this.BtnInputStats_Click);
            // 
            // tlMain
            // 
            this.tlMain.ColumnCount = 1;
            this.tlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlMain.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.AddColumns;
            this.tlMain.Location = new System.Drawing.Point(42, 36);
            this.tlMain.Name = "tlMain";
            this.tlMain.RowCount = 1;
            this.tlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlMain.Size = new System.Drawing.Size(135, 34);
            this.tlMain.TabIndex = 10;
            // 
            // btChangeLoc
            // 
            this.btChangeLoc.BackColor = System.Drawing.Color.LightGray;
            this.btChangeLoc.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btChangeLoc.Location = new System.Drawing.Point(208, 3);
            this.btChangeLoc.Name = "btChangeLoc";
            this.btChangeLoc.Size = new System.Drawing.Size(73, 27);
            this.btChangeLoc.TabIndex = 9;
            this.btChangeLoc.Text = "Settings";
            this.btChangeLoc.UseVisualStyleBackColor = false;
            this.btChangeLoc.Click += new System.EventHandler(this.BtChangeLoc_Click);
            // 
            // lbLocation
            // 
            this.lbLocation.AutoSize = true;
            this.lbLocation.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbLocation.ForeColor = System.Drawing.Color.Red;
            this.lbLocation.Location = new System.Drawing.Point(151, 33);
            this.lbLocation.Name = "lbLocation";
            this.lbLocation.Size = new System.Drawing.Size(52, 18);
            this.lbLocation.TabIndex = 8;
            this.lbLocation.Text = "NONE";
            this.lbLocation.Visible = false;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.LightGray;
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(282, 3);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(32, 27);
            this.btnClose.TabIndex = 14;
            this.btnClose.Text = "X";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // pbHide
            // 
            this.pbHide.Location = new System.Drawing.Point(9, 8);
            this.pbHide.Name = "pbHide";
            this.pbHide.Size = new System.Drawing.Size(25, 52);
            this.pbHide.TabIndex = 13;
            this.pbHide.TabStop = false;
            this.pbHide.Click += new System.EventHandler(this.PbHide_Click);
            // 
            // btnGetStats
            // 
            this.btnGetStats.BackColor = System.Drawing.Color.LightGray;
            this.btnGetStats.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGetStats.Location = new System.Drawing.Point(124, 3);
            this.btnGetStats.Name = "btnGetStats";
            this.btnGetStats.Size = new System.Drawing.Size(82, 27);
            this.btnGetStats.TabIndex = 12;
            this.btnGetStats.Text = "Get Stats";
            this.btnGetStats.UseVisualStyleBackColor = false;
            this.btnGetStats.Click += new System.EventHandler(this.btnGetStats_Click);
            // 
            // FDesktop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(330, 80);
            this.ControlBox = false;
            this.Controls.Add(this.btnInputStats);
            this.Controls.Add(this.tlMain);
            this.Controls.Add(this.btChangeLoc);
            this.Controls.Add(this.lbLocation);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.pbHide);
            this.Controls.Add(this.btnGetStats);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FDesktop";
            this.Text = "FDesktop";
            this.MouseHover += new System.EventHandler(this.FDesktop_MouseHover);
            ((System.ComponentModel.ISupportInitialize)(this.pbHide)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Button btnInputStats;
        internal System.Windows.Forms.TableLayoutPanel tlMain;
        internal System.Windows.Forms.Button btChangeLoc;
        public System.Windows.Forms.Label lbLocation;
        private System.Windows.Forms.Timer timer1;
        internal System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.PictureBox pbHide;
        internal System.Windows.Forms.Button btnGetStats;
    }
}