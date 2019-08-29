namespace APP_DeskStats
{
    partial class Form1
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
            this.lbLocation = new System.Windows.Forms.Label();
            this.btChangeLoc = new System.Windows.Forms.Button();
            this.tlMain = new System.Windows.Forms.TableLayoutPanel();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btnInputStats = new System.Windows.Forms.Button();
            this.btnGetStats = new System.Windows.Forms.Button();
            this.pbHide = new System.Windows.Forms.PictureBox();
            this.btnClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pbHide)).BeginInit();
            this.SuspendLayout();
            // 
            // lbLocation
            // 
            this.lbLocation.AutoSize = true;
            this.lbLocation.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbLocation.ForeColor = System.Drawing.Color.Red;
            this.lbLocation.Location = new System.Drawing.Point(294, 65);
            this.lbLocation.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lbLocation.Name = "lbLocation";
            this.lbLocation.Size = new System.Drawing.Size(101, 36);
            this.lbLocation.TabIndex = 0;
            this.lbLocation.Text = "NONE";
            this.lbLocation.Visible = false;
            // 
            // btChangeLoc
            // 
            this.btChangeLoc.BackColor = System.Drawing.Color.LightGray;
            this.btChangeLoc.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btChangeLoc.Location = new System.Drawing.Point(407, 7);
            this.btChangeLoc.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btChangeLoc.Name = "btChangeLoc";
            this.btChangeLoc.Size = new System.Drawing.Size(146, 52);
            this.btChangeLoc.TabIndex = 1;
            this.btChangeLoc.Text = "Settings";
            this.btChangeLoc.UseVisualStyleBackColor = false;
            this.btChangeLoc.Click += new System.EventHandler(this.btChangeLoc_Click);
            // 
            // tlMain
            // 
            this.tlMain.ColumnCount = 1;
            this.tlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlMain.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.AddColumns;
            this.tlMain.Location = new System.Drawing.Point(76, 71);
            this.tlMain.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.tlMain.Name = "tlMain";
            this.tlMain.RowCount = 1;
            this.tlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlMain.Size = new System.Drawing.Size(270, 65);
            this.tlMain.TabIndex = 2;
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon1.BalloonTipText = "Open Stats";
            this.notifyIcon1.BalloonTipTitle = "Stats";
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // btnInputStats
            // 
            this.btnInputStats.BackColor = System.Drawing.Color.LightGray;
            this.btnInputStats.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnInputStats.Location = new System.Drawing.Point(66, 7);
            this.btnInputStats.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnInputStats.Name = "btnInputStats";
            this.btnInputStats.Size = new System.Drawing.Size(170, 52);
            this.btnInputStats.TabIndex = 4;
            this.btnInputStats.Text = "Input Stats";
            this.btnInputStats.UseVisualStyleBackColor = false;
            this.btnInputStats.Click += new System.EventHandler(this.btnInputStats_Click);
            // 
            // btnGetStats
            // 
            this.btnGetStats.BackColor = System.Drawing.Color.LightGray;
            this.btnGetStats.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGetStats.Location = new System.Drawing.Point(241, 7);
            this.btnGetStats.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnGetStats.Name = "btnGetStats";
            this.btnGetStats.Size = new System.Drawing.Size(164, 52);
            this.btnGetStats.TabIndex = 5;
            this.btnGetStats.Text = "Get Stats";
            this.btnGetStats.UseVisualStyleBackColor = false;
            this.btnGetStats.Click += new System.EventHandler(this.btnGetStats_Click);
            // 
            // pbHide
            // 
            this.pbHide.Location = new System.Drawing.Point(10, 17);
            this.pbHide.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.pbHide.Name = "pbHide";
            this.pbHide.Size = new System.Drawing.Size(50, 100);
            this.pbHide.TabIndex = 6;
            this.pbHide.TabStop = false;
            this.pbHide.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.LightGray;
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(555, 7);
            this.btnClose.Margin = new System.Windows.Forms.Padding(6);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(64, 52);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "X";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(629, 137);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.pbHide);
            this.Controls.Add(this.btnGetStats);
            this.Controls.Add(this.btnInputStats);
            this.Controls.Add(this.tlMain);
            this.Controls.Add(this.btChangeLoc);
            this.Controls.Add(this.lbLocation);
            this.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Name = "Form1";
            this.ShowInTaskbar = false;
            this.Text = "Form1";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.MouseHover += new System.EventHandler(this.Form1_MouseHover);
            ((System.ComponentModel.ISupportInitialize)(this.pbHide)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.Label lbLocation;
        internal System.Windows.Forms.TableLayoutPanel tlMain;
        internal System.Windows.Forms.Button btChangeLoc;
        internal System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.Timer timer1;
        internal System.Windows.Forms.Button btnInputStats;
        internal System.Windows.Forms.Button btnGetStats;
        private System.Windows.Forms.PictureBox pbHide;
        internal System.Windows.Forms.Button btnClose;
    }
}

