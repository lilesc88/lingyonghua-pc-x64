namespace WinFrmTalk
{
    partial class FrmTest
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
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.xListView1 = new TestListView.XListView();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(513, 213);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(183, 98);
            this.button2.TabIndex = 18;
            this.button2.Text = "倒序";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(513, 358);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(183, 98);
            this.button3.TabIndex = 19;
            this.button3.Text = "清空";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // xListView1
            // 
            this.xListView1.BackColor = System.Drawing.Color.Red;
            this.xListView1.Location = new System.Drawing.Point(95, 45);
            this.xListView1.Name = "xListView1";
            this.xListView1.ScrollBarWidth = 10;
            this.xListView1.Size = new System.Drawing.Size(325, 469);
            this.xListView1.TabIndex = 50;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(513, 86);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(183, 98);
            this.button1.TabIndex = 56;
            this.button1.Text = "正序";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // FrmTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(801, 595);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.xListView1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.KeyPreview = true;
            this.Name = "FrmTest";
            this.Text = "";
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private TestListView.XListView xListView1;
        private System.Windows.Forms.Button button1;
    }
}