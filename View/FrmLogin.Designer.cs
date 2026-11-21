using WinFrmTalk.Controls.SystemControls;
using WinFrmTalk.Properties;

namespace WinFrmTalk
{
    partial class FrmLogin
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmLogin));
            this.lbliconAccount = new System.Windows.Forms.Label();
            this.lbliconPassword = new System.Windows.Forms.Label();
            this.lblRegister = new LollipopFlatButton();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.txtTelephone = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.btnLogin = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.chkRememberPwd = new LollipopCheckBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnForgetPwd = new LollipopFlatButton();
            this.lblContry = new System.Windows.Forms.Label();
            this.login_tip = new LollipopFlatButton();
            this.picServer = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picServer)).BeginInit();
            this.SuspendLayout();
            // 
            // lbliconAccount
            // 
            resources.ApplyResources(this.lbliconAccount, "lbliconAccount");
            this.lbliconAccount.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lbliconAccount.Name = "lbliconAccount";
            // 
            // lbliconPassword
            // 
            resources.ApplyResources(this.lbliconPassword, "lbliconPassword");
            this.lbliconPassword.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lbliconPassword.Name = "lbliconPassword";
            // 
            // lblRegister
            // 
            this.lblRegister.BackColor = System.Drawing.Color.Transparent;
            this.lblRegister.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.lblRegister, "lblRegister");
            this.lblRegister.FontColor = "#1AAD19";
            this.lblRegister.Name = "lblRegister";
            this.lblRegister.Click += new System.EventHandler(this.lblRegister_Click);
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // txtTelephone
            // 
            this.txtTelephone.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.txtTelephone, "txtTelephone");
            this.txtTelephone.Name = "txtTelephone";
            this.txtTelephone.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtTelephone_KeyDown);
            this.txtTelephone.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtTelephone_KeyPress);
            this.txtTelephone.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtTelephone_KeyUp);
            // 
            // txtPassword
            // 
            this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.txtPassword, "txtPassword");
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.LoginKeyDown);
            // 
            // btnLogin
            // 
            this.btnLogin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(173)))), ((int)(((byte)(25)))));
            this.btnLogin.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(this.btnLogin, "btnLogin");
            this.btnLogin.ForeColor = System.Drawing.Color.Black;
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.UseVisualStyleBackColor = false;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnLogin);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // chkRememberPwd
            // 
            resources.ApplyResources(this.chkRememberPwd, "chkRememberPwd");
            this.chkRememberPwd.CheckColor = "#1AAD19";
            this.chkRememberPwd.Name = "chkRememberPwd";
            this.chkRememberPwd.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(210)))), ((int)(((byte)(210)))), ((int)(((byte)(210)))));
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(210)))), ((int)(((byte)(210)))), ((int)(((byte)(210)))));
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Name = "panel3";
            // 
            // btnForgetPwd
            // 
            this.btnForgetPwd.BackColor = System.Drawing.Color.Transparent;
            this.btnForgetPwd.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.btnForgetPwd, "btnForgetPwd");
            this.btnForgetPwd.FontColor = "#1AAD19";
            this.btnForgetPwd.Name = "btnForgetPwd";
            this.btnForgetPwd.Click += new System.EventHandler(this.btnForgetPwd_Click);
            // 
            // lblContry
            // 
            this.lblContry.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblContry.Image = global::WinFrmTalk.Properties.Resources.contry;
            resources.ApplyResources(this.lblContry, "lblContry");
            this.lblContry.Name = "lblContry";
            this.lblContry.Click += new System.EventHandler(this.cmbAreaCode_Click);
            // 
            // login_tip
            // 
            this.login_tip.BackColor = System.Drawing.Color.Transparent;
            this.login_tip.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.login_tip, "login_tip");
            this.login_tip.FontColor = "#000000";
            this.login_tip.Name = "login_tip";
            // 
            // picServer
            // 
            this.picServer.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            this.picServer.Image = global::WinFrmTalk.Properties.Resources.ic_setting;
            resources.ApplyResources(this.picServer, "picServer");
            this.picServer.Name = "picServer";
            this.picServer.TabStop = false;
            this.picServer.UseWaitCursor = true;
            this.picServer.Click += new System.EventHandler(this.picServer_Click);
            // 
            // FrmLogin
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.login_tip);
            this.Controls.Add(this.lblContry);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.chkRememberPwd);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.txtTelephone);
            this.Controls.Add(this.lbliconPassword);
            this.Controls.Add(this.lbliconAccount);
            this.Controls.Add(this.btnForgetPwd);
            this.Controls.Add(this.lblRegister);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.picServer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "FrmLogin";
            this.ShowBorder = false;
            this.TitleColor = System.Drawing.Color.Gray;
            this.TitleNeed = false;
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FrmLogin_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.LoginKeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picServer)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private LollipopFlatButton lblRegister;
        private System.Windows.Forms.Label lbliconAccount;
        private System.Windows.Forms.Label lbliconPassword;
        private System.Windows.Forms.TextBox txtTelephone;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Panel panel1;
        private LollipopCheckBox chkRememberPwd;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private LollipopFlatButton btnForgetPwd;
        private System.Windows.Forms.Label lblContry;
        private LollipopFlatButton login_tip;
        private System.Windows.Forms.PictureBox picServer;
    }
}