namespace APP_DeskStats
{
    partial class frmInputStats
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
            this.SuspendLayout();
            // 
            // cbInputYear
            // 
            this.cbInputYear.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbInputYear.FormattingEnabled = true;
            this.cbInputYear.Location = new System.Drawing.Point(108, 41);
            this.cbInputYear.Name = "cbInputYear";
            this.cbInputYear.Size = new System.Drawing.Size(183, 33);
            this.cbInputYear.TabIndex = 0;
            this.cbInputYear.SelectedIndexChanged += new System.EventHandler(this.cbLocation_SelectedValueChanged);
            // 
            // cbInputMonth
            // 
            this.cbInputMonth.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbInputMonth.FormattingEnabled = true;
            this.cbInputMonth.Location = new System.Drawing.Point(306, 41);
            this.cbInputMonth.Name = "cbInputMonth";
            this.cbInputMonth.Size = new System.Drawing.Size(175, 33);
            this.cbInputMonth.TabIndex = 1;
            this.cbInputMonth.SelectedIndexChanged += new System.EventHandler(this.cbLocation_SelectedValueChanged);
            // 
            // cbLocation
            // 
            this.cbLocation.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbLocation.FormattingEnabled = true;
            this.cbLocation.Location = new System.Drawing.Point(498, 41);
            this.cbLocation.Name = "cbLocation";
            this.cbLocation.Size = new System.Drawing.Size(214, 33);
            this.cbLocation.TabIndex = 2;
            this.cbLocation.SelectedValueChanged += new System.EventHandler(this.cbLocation_SelectedValueChanged);
            // 
            // frmInputStats
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(812, 594);
            this.Controls.Add(this.cbLocation);
            this.Controls.Add(this.cbInputMonth);
            this.Controls.Add(this.cbInputYear);
            this.Name = "frmInputStats";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Input Stats";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.frmInputStats_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cbInputYear;
        private System.Windows.Forms.ComboBox cbInputMonth;
        private System.Windows.Forms.ComboBox cbLocation;
    }
}