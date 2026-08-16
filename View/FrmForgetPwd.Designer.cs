namespace WinFrmTalk
{
    partial class FrmForgetPwd
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmForgetPwd));
            this.panel3 = new System.Windows.Forms.Panel();
            this.txtNewPwd = new System.Windows.Forms.TextBox();
            this.lbliconPassword = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.txtTelephone = new System.Windows.Forms.TextBox();
            this.lbliconAccount = new System.Windows.Forms.Label();
            this.lblVerifyPwd = new System.Windows.Forms.Label();
            this.txtConfirmPwd = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblImgCode = new System.Windows.Forms.Label();
            this.txtImgCode = new System.Windows.Forms.TextBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btnSendCode = new System.Windows.Forms.Button();
            this.picImgCode = new System.Windows.Forms.PictureBox();
            this.lblCode = new System.Windows.Forms.Label();
            this.txtCode = new System.Windows.Forms.TextBox();
            this.panel5 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnModifyPwd = new System.Windows.Forms.Button();
            this.tmrCode = new System.Windows.Forms.Timer(this.components);
            this.lblContry = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picImgCode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(210)))), ((int)(((byte)(210)))), ((int)(((byte)(210)))));
            this.panel3.Location = new System.Drawing.Point(100, 251);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(200, 1);
            this.panel3.TabIndex = 43;
            // 
            // txtNewPwd
            // 
            this.txtNewPwd.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtNewPwd.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtNewPwd.Location = new System.Drawing.Point(100, 235);
            this.txtNewPwd.Name = "txtNewPwd";
            this.txtNewPwd.PasswordChar = '●';
            this.txtNewPwd.Size = new System.Drawing.Size(200, 16);
            this.txtNewPwd.TabIndex = 1;
            // 
            // lbliconPassword
            // 
            this.lbliconPassword.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbliconPassword.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lbliconPassword.Location = new System.Drawing.Point(20, 235);
            this.lbliconPassword.Name = "lbliconPassword";
            this.lbliconPassword.Size = new System.Drawing.Size(80, 17);
            this.lbliconPassword.TabIndex = 42;
            this.lbliconPassword.Text = "新密码：";
            this.lbliconPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(210)))), ((int)(((byte)(210)))), ((int)(((byte)(210)))));
            this.panel2.Location = new System.Drawing.Point(167, 205);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(133, 1);
            this.panel2.TabIndex = 47;
            // 
            // txtTelephone
            // 
            this.txtTelephone.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtTelephone.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtTelephone.Location = new System.Drawing.Point(167, 189);
            this.txtTelephone.Name = "txtTelephone";
            this.txtTelephone.Size = new System.Drawing.Size(133, 16);
            this.txtTelephone.TabIndex = 0;
            // 
            // lbliconAccount
            // 
            this.lbliconAccount.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbliconAccount.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lbliconAccount.Location = new System.Drawing.Point(20, 189);
            this.lbliconAccount.Name = "lbliconAccount";
            this.lbliconAccount.Size = new System.Drawing.Size(80, 17);
            this.lbliconAccount.TabIndex = 45;
            this.lbliconAccount.Text = "账号：";
            this.lbliconAccount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblVerifyPwd
            // 
            this.lblVerifyPwd.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblVerifyPwd.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lblVerifyPwd.Location = new System.Drawing.Point(20, 281);
            this.lblVerifyPwd.Name = "lblVerifyPwd";
            this.lblVerifyPwd.Size = new System.Drawing.Size(80, 17);
            this.lblVerifyPwd.TabIndex = 42;
            this.lblVerifyPwd.Text = "确认新密码：";
            this.lblVerifyPwd.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtConfirmPwd
            // 
            this.txtConfirmPwd.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtConfirmPwd.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtConfirmPwd.Location = new System.Drawing.Point(100, 281);
            this.txtConfirmPwd.Name = "txtConfirmPwd";
            this.txtConfirmPwd.PasswordChar = '●';
            this.txtConfirmPwd.Size = new System.Drawing.Size(200, 16);
            this.txtConfirmPwd.TabIndex = 2;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(210)))), ((int)(((byte)(210)))), ((int)(((byte)(210)))));
            this.panel1.Location = new System.Drawing.Point(100, 297);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 1);
            this.panel1.TabIndex = 43;
            // 
            // lblImgCode
            // 
            this.lblImgCode.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblImgCode.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lblImgCode.Location = new System.Drawing.Point(18, 327);
            this.lblImgCode.Name = "lblImgCode";
            this.lblImgCode.Size = new System.Drawing.Size(82, 17);
            this.lblImgCode.TabIndex = 42;
            this.lblImgCode.Text = "图形验证码：";
            this.lblImgCode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtImgCode
            // 
            this.txtImgCode.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtImgCode.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtImgCode.Location = new System.Drawing.Point(100, 327);
            this.txtImgCode.Name = "txtImgCode";
            this.txtImgCode.Size = new System.Drawing.Size(124, 16);
            this.txtImgCode.TabIndex = 3;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(210)))), ((int)(((byte)(210)))), ((int)(((byte)(210)))));
            this.panel4.Location = new System.Drawing.Point(100, 343);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(124, 1);
            this.panel4.TabIndex = 43;
            // 
            // btnSendCode
            // 
            this.btnSendCode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(173)))), ((int)(((byte)(25)))));
            this.btnSendCode.FlatAppearance.BorderSize = 0;
            this.btnSendCode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSendCode.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnSendCode.ForeColor = System.Drawing.Color.White;
            this.btnSendCode.Location = new System.Drawing.Point(237, 366);
            this.btnSendCode.Name = "btnSendCode";
            this.btnSendCode.Size = new System.Drawing.Size(63, 24);
            this.btnSendCode.TabIndex = 49;
            this.btnSendCode.Text = "发送";
            this.btnSendCode.UseVisualStyleBackColor = false;
            this.btnSendCode.Click += new System.EventHandler(this.BtnSendCode_Click);
            // 
            // picImgCode
            // 
            this.picImgCode.Image = ((System.Drawing.Image)(resources.GetObject("picImgCode.Image")));
            this.picImgCode.Location = new System.Drawing.Point(236, 319);
            this.picImgCode.Name = "picImgCode";
            this.picImgCode.Size = new System.Drawing.Size(64, 25);
            this.picImgCode.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picImgCode.TabIndex = 48;
            this.picImgCode.TabStop = false;
            this.picImgCode.Click += new System.EventHandler(this.PicImgCode_Click);
            // 
            // lblCode
            // 
            this.lblCode.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblCode.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lblCode.Location = new System.Drawing.Point(20, 373);
            this.lblCode.Name = "lblCode";
            this.lblCode.Size = new System.Drawing.Size(80, 17);
            this.lblCode.TabIndex = 42;
            this.lblCode.Text = "验证码：";
            this.lblCode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtCode
            // 
            this.txtCode.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtCode.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtCode.Location = new System.Drawing.Point(100, 373);
            this.txtCode.Name = "txtCode";
            this.txtCode.Size = new System.Drawing.Size(124, 16);
            this.txtCode.TabIndex = 4;
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(210)))), ((int)(((byte)(210)))), ((int)(((byte)(210)))));
            this.panel5.Location = new System.Drawing.Point(100, 389);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(124, 1);
            this.panel5.TabIndex = 43;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::WinFrmTalk.Properties.Resources.Logo;
            this.pictureBox1.Location = new System.Drawing.Point(126, 62);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(68, 68);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 50;
            this.pictureBox1.TabStop = false;
            // 
            // btnModifyPwd
            // 
            this.btnModifyPwd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(173)))), ((int)(((byte)(25)))));
            this.btnModifyPwd.FlatAppearance.BorderSize = 0;
            this.btnModifyPwd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnModifyPwd.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnModifyPwd.ForeColor = System.Drawing.Color.White;
            this.btnModifyPwd.Location = new System.Drawing.Point(93, 429);
            this.btnModifyPwd.Name = "btnModifyPwd";
            this.btnModifyPwd.Size = new System.Drawing.Size(134, 37);
            this.btnModifyPwd.TabIndex = 49;
            this.btnModifyPwd.Text = "修改密码";
            this.btnModifyPwd.UseVisualStyleBackColor = false;
            this.btnModifyPwd.Click += new System.EventHandler(this.BtnModifyPwd_Click);
            // 
            // tmrCode
            // 
            this.tmrCode.Interval = 1000;
            this.tmrCode.Tick += new System.EventHandler(this.tmrCode_Tick);
            // 
            // lblContry
            // 
            this.lblContry.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblContry.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblContry.Image = global::WinFrmTalk.Properties.Resources.contry;
            this.lblContry.Location = new System.Drawing.Point(100, 186);
            this.lblContry.Name = "lblContry";
            this.lblContry.Size = new System.Drawing.Size(58, 21);
            this.lblContry.TabIndex = 66;
            this.lblContry.Text = "+86";
            this.lblContry.Click += new System.EventHandler(this.lblContry_Click);
            // 
            // FrmForgetPwd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(320, 505);
            this.Controls.Add(this.lblContry);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnModifyPwd);
            this.Controls.Add(this.btnSendCode);
            this.Controls.Add(this.picImgCode);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.txtTelephone);
            this.Controls.Add(this.lbliconAccount);
            this.Controls.Add(this.panel5);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.txtCode);
            this.Controls.Add(this.txtImgCode);
            this.Controls.Add(this.lblCode);
            this.Controls.Add(this.txtConfirmPwd);
            this.Controls.Add(this.lblImgCode);
            this.Controls.Add(this.lblVerifyPwd);
            this.Controls.Add(this.txtNewPwd);
            this.Controls.Add(this.lbliconPassword);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FrmForgetPwd";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "忘记密码";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmForgetPwd_FormClosing);
            this.Load += new System.EventHandler(this.FrmForgetPwd_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picImgCode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TextBox txtNewPwd;
        private System.Windows.Forms.Label lbliconPassword;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox txtTelephone;
        private System.Windows.Forms.Label lbliconAccount;
        private System.Windows.Forms.Label lblVerifyPwd;
        private System.Windows.Forms.TextBox txtConfirmPwd;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblImgCode;
        private System.Windows.Forms.TextBox txtImgCode;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button btnSendCode;
        private System.Windows.Forms.PictureBox picImgCode;
        private System.Windows.Forms.Label lblCode;
        private System.Windows.Forms.TextBox txtCode;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnModifyPwd;
        private System.Windows.Forms.Timer tmrCode;
        private System.Windows.Forms.Label lblContry;
    }
}