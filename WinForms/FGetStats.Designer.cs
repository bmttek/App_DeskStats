namespace APP_DeskStats.WinForms
{
    partial class FGetStats
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
            this.cbLocation = new System.Windows.Forms.ComboBox();
            this.cbInputMonth = new System.Windows.Forms.ComboBox();
            this.cbInputYear = new System.Windows.Forms.ComboBox();
            this.btnLocationReport = new System.Windows.Forms.Button();
            this.btnMonthlyReport = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cbLocation
            // 
            this.cbLocation.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbLocation.FormattingEnabled = true;
            this.cbLocation.Location = new System.Drawing.Point(358, 96);
            this.cbLocation.Name = "cbLocation";
            this.cbLocation.Size = new System.Drawing.Size(290, 33);
            this.cbLocation.TabIndex = 5;
            // 
            // cbInputMonth
            // 
            this.cbInputMonth.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbInputMonth.FormattingEnabled = true;
            this.cbInputMonth.Location = new System.Drawing.Point(358, 35);
            this.cbInputMonth.Name = "cbInputMonth";
            this.cbInputMonth.Size = new System.Drawing.Size(290, 33);
            this.cbInputMonth.TabIndex = 4;
            // 
            // cbInputYear
            // 
            this.cbInputYear.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbInputYear.FormattingEnabled = true;
            this.cbInputYear.Location = new System.Drawing.Point(28, 35);
            this.cbInputYear.Name = "cbInputYear";
            this.cbInputYear.Size = new System.Drawing.Size(314, 33);
            this.cbInputYear.TabIndex = 3;
            // 
            // btnLocationReport
            // 
            this.btnLocationReport.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLocationReport.Location = new System.Drawing.Point(653, 99);
            this.btnLocationReport.Margin = new System.Windows.Forms.Padding(2);
            this.btnLocationReport.Name = "btnLocationReport";
            this.btnLocationReport.Size = new System.Drawing.Size(229, 29);
            this.btnLocationReport.TabIndex = 7;
            this.btnLocationReport.Text = "Get Monthly Location Report";
            this.btnLocationReport.UseVisualStyleBackColor = true;
            this.btnLocationReport.Click += new System.EventHandler(this.btnLocationReport_Click);
            // 
            // btnMonthlyReport
            // 
            this.btnMonthlyReport.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMonthlyReport.Location = new System.Drawing.Point(653, 37);
            this.btnMonthlyReport.Margin = new System.Windows.Forms.Padding(2);
            this.btnMonthlyReport.Name = "btnMonthlyReport";
            this.btnMonthlyReport.Size = new System.Drawing.Size(229, 29);
            this.btnMonthlyReport.TabIndex = 6;
            this.btnMonthlyReport.Text = "Get Board Report ";
            this.btnMonthlyReport.UseVisualStyleBackColor = true;
            this.btnMonthlyReport.Click += new System.EventHandler(this.btnMonthlyReport_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(23, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 25);
            this.label1.TabIndex = 8;
            this.label1.Text = "Year";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(363, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 25);
            this.label2.TabIndex = 9;
            this.label2.Text = "Location";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(363, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 25);
            this.label3.TabIndex = 10;
            this.label3.Text = "Month";
            // 
            // FGetStats
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(979, 248);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnLocationReport);
            this.Controls.Add(this.btnMonthlyReport);
            this.Controls.Add(this.cbLocation);
            this.Controls.Add(this.cbInputMonth);
            this.Controls.Add(this.cbInputYear);
            this.Name = "FGetStats";
            this.Text = "Get Stats";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbLocation;
        private System.Windows.Forms.ComboBox cbInputMonth;
        private System.Windows.Forms.ComboBox cbInputYear;
        private System.Windows.Forms.Button btnLocationReport;
        private System.Windows.Forms.Button btnMonthlyReport;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}