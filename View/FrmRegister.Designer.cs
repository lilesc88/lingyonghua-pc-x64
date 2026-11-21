namespace WinFrmTalk
{
    partial class FrmRegister
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmRegister));
            this.lblPhoneNumber = new System.Windows.Forms.Label();
            this.lblPwd = new System.Windows.Forms.Label();
            this.lblImgCode = new System.Windows.Forms.Label();
            this.lblVerificationCode = new System.Windows.Forms.Label();
            this.picImgCode = new System.Windows.Forms.PictureBox();
            this.tmrCode = new System.Windows.Forms.Timer(this.components);
            this.txtTel = new WinFrmTalk.View.Common.NoPraseTextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtImgCode = new System.Windows.Forms.TextBox();
            this.txtCode = new System.Windows.Forms.TextBox();
            this.picLogo = new WinFrmTalk.RoundPicBox();
            this.btnSendCode = new System.Windows.Forms.Button();
            this.btnRegister = new System.Windows.Forms.Button();
            this.lblContry = new System.Windows.Forms.Label();
            this.lblnocode = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picImgCode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // lblPhoneNumber
            // 
            this.lblPhoneNumber.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblPhoneNumber.ForeColor = System.Drawing.Color.Gray;
            this.lblPhoneNumber.Location = new System.Drawing.Point(8, 152);
            this.lblPhoneNumber.Name = "lblPhoneNumber";
            this.lblPhoneNumber.Size = new System.Drawing.Size(85, 24);
            this.lblPhoneNumber.TabIndex = 11;
            this.lblPhoneNumber.Text = "手机号码：";
            this.lblPhoneNumber.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblPwd
            // 
            this.lblPwd.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblPwd.ForeColor = System.Drawing.Color.Gray;
            this.lblPwd.Location = new System.Drawing.Point(11, 193);
            this.lblPwd.Name = "lblPwd";
            this.lblPwd.Size = new System.Drawing.Size(82, 23);
            this.lblPwd.TabIndex = 11;
            this.lblPwd.Text = "密     码：";
            this.lblPwd.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblImgCode
            // 
            this.lblImgCode.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblImgCode.ForeColor = System.Drawing.Color.Gray;
            this.lblImgCode.Location = new System.Drawing.Point(14, 230);
            this.lblImgCode.Name = "lblImgCode";
            this.lblImgCode.Size = new System.Drawing.Size(80, 25);
            this.lblImgCode.TabIndex = 11;
            this.lblImgCode.Text = "图 形 码：";
            this.lblImgCode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblVerificationCode
            // 
            this.lblVerificationCode.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblVerificationCode.ForeColor = System.Drawing.Color.Gray;
            this.lblVerificationCode.Location = new System.Drawing.Point(17, 271);
            this.lblVerificationCode.Name = "lblVerificationCode";
            this.lblVerificationCode.Size = new System.Drawing.Size(77, 23);
            this.lblVerificationCode.TabIndex = 11;
            this.lblVerificationCode.Text = "验 证 码：";
            this.lblVerificationCode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // picImgCode
            // 
            this.picImgCode.Image = ((System.Drawing.Image)(resources.GetObject("picImgCode.Image")));
            this.picImgCode.Location = new System.Drawing.Point(206, 230);
            this.picImgCode.Name = "picImgCode";
            this.picImgCode.Size = new System.Drawing.Size(64, 25);
            this.picImgCode.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picImgCode.TabIndex = 4;
            this.picImgCode.TabStop = false;
            this.picImgCode.Click += new System.EventHandler(this.PicImgCode_Click);
            // 
            // tmrCode
            // 
            this.tmrCode.Interval = 1000;
            this.tmrCode.Tick += new System.EventHandler(this.TmrCode_Tick);
            // 
            // txtTel
            // 
            this.txtTel.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtTel.Location = new System.Drawing.Point(167, 155);
            this.txtTel.Name = "txtTel";
            this.txtTel.Size = new System.Drawing.Size(103, 23);
            this.txtTel.TabIndex = 0;
            this.txtTel.TextChanged += new System.EventHandler(this.txtTel_TextChanged);
            this.txtTel.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtTel_KeyPress);
            // 
            // txtPassword
            // 
            this.txtPassword.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtPassword.Location = new System.Drawing.Point(100, 193);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '●';
            this.txtPassword.Size = new System.Drawing.Size(170, 23);
            this.txtPassword.TabIndex = 1;
            // 
            // txtImgCode
            // 
            this.txtImgCode.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtImgCode.Location = new System.Drawing.Point(100, 231);
            this.txtImgCode.Name = "txtImgCode";
            this.txtImgCode.Size = new System.Drawing.Size(103, 23);
            this.txtImgCode.TabIndex = 2;
            // 
            // txtCode
            // 
            this.txtCode.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtCode.Location = new System.Drawing.Point(100, 271);
            this.txtCode.Name = "txtCode";
            this.txtCode.Size = new System.Drawing.Size(103, 23);
            this.txtCode.TabIndex = 3;
            // 
            // picLogo
            // 
            this.picLogo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.picLogo.Image = global::WinFrmTalk.Properties.Resources.Logo;
            this.picLogo.isDrawRound = true;
            this.picLogo.Location = new System.Drawing.Point(116, 42);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(80, 80);
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picLogo.TabIndex = 15;
            this.picLogo.TabStop = false;
            // 
            // btnSendCode
            // 
            this.btnSendCode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(173)))), ((int)(((byte)(25)))));
            this.btnSendCode.FlatAppearance.BorderSize = 0;
            this.btnSendCode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSendCode.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnSendCode.ForeColor = System.Drawing.Color.White;
            this.btnSendCode.Location = new System.Drawing.Point(207, 270);
            this.btnSendCode.Name = "btnSendCode";
            this.btnSendCode.Size = new System.Drawing.Size(63, 24);
            this.btnSendCode.TabIndex = 19;
            this.btnSendCode.Text = "发送";
            this.btnSendCode.UseVisualStyleBackColor = false;
            this.btnSendCode.Click += new System.EventHandler(this.BtnSendCode_Click);
            // 
            // btnRegister
            // 
            this.btnRegister.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(173)))), ((int)(((byte)(25)))));
            this.btnRegister.FlatAppearance.BorderSize = 0;
            this.btnRegister.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRegister.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnRegister.ForeColor = System.Drawing.Color.White;
            this.btnRegister.Location = new System.Drawing.Point(92, 343);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(128, 39);
            this.btnRegister.TabIndex = 20;
            this.btnRegister.Text = "下一步";
            this.btnRegister.UseVisualStyleBackColor = false;
            this.btnRegister.Click += new System.EventHandler(this.BtnRegister_Click);
            // 
            // lblContry
            // 
            this.lblContry.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblContry.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblContry.Image = global::WinFrmTalk.Properties.Resources.contry;
            this.lblContry.Location = new System.Drawing.Point(100, 156);
            this.lblContry.Name = "lblContry";
            this.lblContry.Size = new System.Drawing.Size(58, 21);
            this.lblContry.TabIndex = 46;
            this.lblContry.Text = "+86";
            this.lblContry.Click += new System.EventHandler(this.LblContry_Click);
            // 
            // lblnocode
            // 
            this.lblnocode.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblnocode.Location = new System.Drawing.Point(189, 302);
            this.lblnocode.Name = "lblnocode";
            this.lblnocode.Size = new System.Drawing.Size(108, 36);
            this.lblnocode.TabIndex = 50;
            this.lblnocode.Text = "收不到验证码？";
            this.lblnocode.Visible = false;
            this.lblnocode.Click += new System.EventHandler(this.Lblnocode_Click);
            // 
            // FrmRegister
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(314, 435);
            this.Controls.Add(this.lblnocode);
            this.Controls.Add(this.lblContry);
            this.Controls.Add(this.btnRegister);
            this.Controls.Add(this.btnSendCode);
            this.Controls.Add(this.picLogo);
            this.Controls.Add(this.txtCode);
            this.Controls.Add(this.txtImgCode);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.txtTel);
            this.Controls.Add(this.lblVerificationCode);
            this.Controls.Add(this.lblImgCode);
            this.Controls.Add(this.lblPwd);
            this.Controls.Add(this.lblPhoneNumber);
            this.Controls.Add(this.picImgCode);
            this.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "FrmRegister";
            this.ShowBorder = false;
            this.ShowDrawIcon = false;
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = " 注册";
            this.TitleColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmRegister_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.picImgCode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblPhoneNumber;
        private System.Windows.Forms.Label lblPwd;
        private System.Windows.Forms.Label lblImgCode;
        private System.Windows.Forms.Label lblVerificationCode;
        private System.Windows.Forms.PictureBox picImgCode;
        private System.Windows.Forms.Timer tmrCode;
        private View.Common.NoPraseTextBox txtTel;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtImgCode;
        private System.Windows.Forms.TextBox txtCode;
        private RoundPicBox picLogo;
        private System.Windows.Forms.Button btnSendCode;
        private System.Windows.Forms.Button btnRegister;
        private System.Windows.Forms.Label lblContry;
        private System.Windows.Forms.Label lblnocode;
    }
}