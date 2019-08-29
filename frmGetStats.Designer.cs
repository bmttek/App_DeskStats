namespace APP_DeskStats
{
    partial class frmGetStats
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
            this.cbInputYear = new System.Windows.Forms.ComboBox();
            this.cbInputMonth = new System.Windows.Forms.ComboBox();
            this.cbLocation = new System.Windows.Forms.ComboBox();
            this.btnMonthlyReport = new System.Windows.Forms.Button();
            this.btnLocationReport = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cbInputYear
            // 
            this.cbInputYear.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbInputYear.FormattingEnabled = true;
            this.cbInputYear.Location = new System.Drawing.Point(171, 68);
            this.cbInputYear.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.cbInputYear.Name = "cbInputYear";
            this.cbInputYear.Size = new System.Drawing.Size(362, 56);
            this.cbInputYear.TabIndex = 0;
            // 
            // cbInputMonth
            // 
            this.cbInputMonth.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbInputMonth.FormattingEnabled = true;
            this.cbInputMonth.Location = new System.Drawing.Point(569, 68);
            this.cbInputMonth.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.cbInputMonth.Name = "cbInputMonth";
            this.cbInputMonth.Size = new System.Drawing.Size(445, 56);
            this.cbInputMonth.TabIndex = 1;
            // 
            // cbLocation
            // 
            this.cbLocation.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbLocation.FormattingEnabled = true;
            this.cbLocation.Location = new System.Drawing.Point(569, 159);
            this.cbLocation.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.cbLocation.Name = "cbLocation";
            this.cbLocation.Size = new System.Drawing.Size(445, 56);
            this.cbLocation.TabIndex = 2;
            // 
            // btnMonthlyReport
            // 
            this.btnMonthlyReport.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMonthlyReport.Location = new System.Drawing.Point(1096, 71);
            this.btnMonthlyReport.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnMonthlyReport.Name = "btnMonthlyReport";
            this.btnMonthlyReport.Size = new System.Drawing.Size(338, 56);
            this.btnMonthlyReport.TabIndex = 3;
            this.btnMonthlyReport.Text = "Get Monthly Report";
            this.btnMonthlyReport.UseVisualStyleBackColor = true;
            this.btnMonthlyReport.Click += new System.EventHandler(this.btnMonthlyReport_Click);
            // 
            // btnLocationReport
            // 
            this.btnLocationReport.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLocationReport.Location = new System.Drawing.Point(1096, 162);
            this.btnLocationReport.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnLocationReport.Name = "btnLocationReport";
            this.btnLocationReport.Size = new System.Drawing.Size(338, 56);
            this.btnLocationReport.TabIndex = 4;
            this.btnLocationReport.Text = "Get Location Report";
            this.btnLocationReport.UseVisualStyleBackColor = true;
            this.btnLocationReport.Click += new System.EventHandler(this.btnLocationReport_Click);
            // 
            // frmGetStats
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(1624, 1142);
            this.Controls.Add(this.btnLocationReport);
            this.Controls.Add(this.btnMonthlyReport);
            this.Controls.Add(this.cbLocation);
            this.Controls.Add(this.cbInputMonth);
            this.Controls.Add(this.cbInputYear);
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Name = "frmGetStats";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Get Stats";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.frmGetStats_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cbInputYear;
        private System.Windows.Forms.ComboBox cbInputMonth;
        private System.Windows.Forms.ComboBox cbLocation;
        private System.Windows.Forms.Button btnMonthlyReport;
        private System.Windows.Forms.Button btnLocationReport;
    }
}