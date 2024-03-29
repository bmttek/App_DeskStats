﻿namespace APP_DeskStats
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
            this.cbInputYear.Location = new System.Drawing.Point(86, 35);
            this.cbInputYear.Name = "cbInputYear";
            this.cbInputYear.Size = new System.Drawing.Size(183, 33);
            this.cbInputYear.TabIndex = 0;
            // 
            // cbInputMonth
            // 
            this.cbInputMonth.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbInputMonth.FormattingEnabled = true;
            this.cbInputMonth.Location = new System.Drawing.Point(284, 35);
            this.cbInputMonth.Name = "cbInputMonth";
            this.cbInputMonth.Size = new System.Drawing.Size(224, 33);
            this.cbInputMonth.TabIndex = 1;
            // 
            // cbLocation
            // 
            this.cbLocation.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbLocation.FormattingEnabled = true;
            this.cbLocation.Location = new System.Drawing.Point(284, 83);
            this.cbLocation.Name = "cbLocation";
            this.cbLocation.Size = new System.Drawing.Size(224, 33);
            this.cbLocation.TabIndex = 2;
            // 
            // btnMonthlyReport
            // 
            this.btnMonthlyReport.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMonthlyReport.Location = new System.Drawing.Point(548, 37);
            this.btnMonthlyReport.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnMonthlyReport.Name = "btnMonthlyReport";
            this.btnMonthlyReport.Size = new System.Drawing.Size(169, 29);
            this.btnMonthlyReport.TabIndex = 3;
            this.btnMonthlyReport.Text = "Get Monthly Report";
            this.btnMonthlyReport.UseVisualStyleBackColor = true;
            this.btnMonthlyReport.Click += new System.EventHandler(this.btnMonthlyReport_Click);
            // 
            // btnLocationReport
            // 
            this.btnLocationReport.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLocationReport.Location = new System.Drawing.Point(548, 84);
            this.btnLocationReport.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnLocationReport.Name = "btnLocationReport";
            this.btnLocationReport.Size = new System.Drawing.Size(169, 29);
            this.btnLocationReport.TabIndex = 4;
            this.btnLocationReport.Text = "Get Location Report";
            this.btnLocationReport.UseVisualStyleBackColor = true;
            this.btnLocationReport.Click += new System.EventHandler(this.btnLocationReport_Click);
            // 
            // frmGetStats
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(812, 594);
            this.Controls.Add(this.btnLocationReport);
            this.Controls.Add(this.btnMonthlyReport);
            this.Controls.Add(this.cbLocation);
            this.Controls.Add(this.cbInputMonth);
            this.Controls.Add(this.cbInputYear);
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