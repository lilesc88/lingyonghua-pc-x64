namespace WinFrmTalk.View
{
    partial class FrmSelectCountry
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
            this.lblName = new System.Windows.Forms.Label();
            this.xListView1 = new TestListView.XListView();
            this.SuspendLayout();
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lblName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lblName.Location = new System.Drawing.Point(5, 4);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(56, 17);
            this.lblName.TabIndex = 2;
            this.lblName.Text = "选择区号";
            // 
            // xListView1
            // 
            this.xListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xListView1.BackColor = System.Drawing.SystemColors.Control;
            this.xListView1.Location = new System.Drawing.Point(0, 24);
            this.xListView1.Name = "xListView1";
            this.xListView1.ScrollBarWidth = 10;
            this.xListView1.Size = new System.Drawing.Size(331, 500);
            this.xListView1.TabIndex = 3;
            // 
            // FrmSelectCountry
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(331, 532);
            this.Controls.Add(this.xListView1);
            this.Controls.Add(this.lblName);
            this.Name = "FrmSelectCountry";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "FrmControl";
            this.Load += new System.EventHandler(this.FrmSelectCountry_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblName;
        private TestListView.XListView xListView1;
    }
}