using System;

namespace WinFrmTalk
{
    partial class FrmRegisterInfo
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
            this.lblNickname = new System.Windows.Forms.Label();
            this.cmsSex = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.nan = new System.Windows.Forms.ToolStripMenuItem();
            this.nv = new System.Windows.Forms.ToolStripMenuItem();
            this.lblSex = new System.Windows.Forms.Label();
            this.lblBirthday = new System.Windows.Forms.Label();
            this.lblLivePlace = new System.Windows.Forms.Label();
            this.dtpBirthday = new System.Windows.Forms.DateTimePicker();
            this.txtNickname = new System.Windows.Forms.TextBox();
            this.txtRegion = new System.Windows.Forms.TextBox();
            this.cmsRegion = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.btnSubmit = new System.Windows.Forms.Button();
            this.lblAccountIM = new System.Windows.Forms.Label();
            this.txtAccount = new System.Windows.Forms.TextBox();
            this.picHead = new WinFrmTalk.RoundPicBox();
            this.cmbSex = new CCWin.SkinControl.SkinComboBox();
            this.cmsSex.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picHead)).BeginInit();
            this.SuspendLayout();
            // 
            // lblNickname
            // 
            this.lblNickname.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblNickname.ForeColor = System.Drawing.Color.Gray;
            this.lblNickname.Location = new System.Drawing.Point(32, 149);
            this.lblNickname.Name = "lblNickname";
            this.lblNickname.Size = new System.Drawing.Size(73, 17);
            this.lblNickname.TabIndex = 13;
            this.lblNickname.Text = "昵称：";
            this.lblNickname.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmsSex
            // 
            this.cmsSex.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nan,
            this.nv});
            this.cmsSex.Name = "contextMenuStrip1";
            this.cmsSex.Size = new System.Drawing.Size(89, 48);
            // 
            // nan
            // 
            this.nan.Name = "nan";
            this.nan.Size = new System.Drawing.Size(88, 22);
            this.nan.Text = "男";
            this.nan.Click += new System.EventHandler(this.Man_Click);
            // 
            // nv
            // 
            this.nv.Name = "nv";
            this.nv.Size = new System.Drawing.Size(88, 22);
            this.nv.Text = "女";
            this.nv.Click += new System.EventHandler(this.Woman_Click);
            // 
            // lblSex
            // 
            this.lblSex.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblSex.ForeColor = System.Drawing.Color.Gray;
            this.lblSex.Location = new System.Drawing.Point(32, 185);
            this.lblSex.Name = "lblSex";
            this.lblSex.Size = new System.Drawing.Size(73, 17);
            this.lblSex.TabIndex = 13;
            this.lblSex.Text = "性别：";
            this.lblSex.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblBirthday
            // 
            this.lblBirthday.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblBirthday.ForeColor = System.Drawing.Color.Gray;
            this.lblBirthday.Location = new System.Drawing.Point(32, 221);
            this.lblBirthday.Name = "lblBirthday";
            this.lblBirthday.Size = new System.Drawing.Size(73, 17);
            this.lblBirthday.TabIndex = 13;
            this.lblBirthday.Text = "出生日期：";
            this.lblBirthday.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblLivePlace
            // 
            this.lblLivePlace.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblLivePlace.ForeColor = System.Drawing.Color.Gray;
            this.lblLivePlace.Location = new System.Drawing.Point(32, 257);
            this.lblLivePlace.Name = "lblLivePlace";
            this.lblLivePlace.Size = new System.Drawing.Size(73, 17);
            this.lblLivePlace.TabIndex = 13;
            this.lblLivePlace.Text = "居住地：";
            this.lblLivePlace.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // dtpBirthday
            // 
            this.dtpBirthday.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dtpBirthday.Location = new System.Drawing.Point(110, 218);
            this.dtpBirthday.MaxDate = new System.DateTime(2019, 8, 21, 7, 31, 20, 728);
            this.dtpBirthday.Name = "dtpBirthday";
            this.dtpBirthday.Size = new System.Drawing.Size(148, 23);
            this.dtpBirthday.TabIndex = 2;
            this.dtpBirthday.Value = new System.DateTime(2019, 8, 21, 7, 31, 20, 728);
            // 
            // txtNickname
            // 
            this.txtNickname.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtNickname.Location = new System.Drawing.Point(110, 146);
            this.txtNickname.MaxLength = 50;
            this.txtNickname.Name = "txtNickname";
            this.txtNickname.Size = new System.Drawing.Size(148, 23);
            this.txtNickname.TabIndex = 0;
            this.txtNickname.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtNickname_KeyPress);
            // 
            // txtRegion
            // 
            this.txtRegion.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtRegion.Location = new System.Drawing.Point(110, 254);
            this.txtRegion.Name = "txtRegion";
            this.txtRegion.Size = new System.Drawing.Size(148, 23);
            this.txtRegion.TabIndex = 3;
            this.txtRegion.Click += new System.EventHandler(this.TxtRegion_Click);
            this.txtRegion.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtRegion_KeyPress);
            // 
            // cmsRegion
            // 
            this.cmsRegion.Name = "cmsRegion";
            this.cmsRegion.Size = new System.Drawing.Size(61, 4);
            // 
            // btnSubmit
            // 
            this.btnSubmit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(173)))), ((int)(((byte)(25)))));
            this.btnSubmit.FlatAppearance.BorderSize = 0;
            this.btnSubmit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSubmit.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnSubmit.ForeColor = System.Drawing.Color.White;
            this.btnSubmit.Location = new System.Drawing.Point(91, 327);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(128, 39);
            this.btnSubmit.TabIndex = 19;
            this.btnSubmit.Text = "提交";
            this.btnSubmit.UseVisualStyleBackColor = false;
            this.btnSubmit.Click += new System.EventHandler(this.BtnRegister_Click);
            // 
            // lblAccountIM
            // 
            this.lblAccountIM.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblAccountIM.ForeColor = System.Drawing.Color.Gray;
            this.lblAccountIM.Location = new System.Drawing.Point(32, 293);
            this.lblAccountIM.Name = "lblAccountIM";
            this.lblAccountIM.Size = new System.Drawing.Size(73, 17);
            this.lblAccountIM.TabIndex = 13;
            this.lblAccountIM.Text = "通讯号：";
            this.lblAccountIM.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblAccountIM.Visible = false;
            // 
            // txtAccount
            // 
            this.txtAccount.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtAccount.Location = new System.Drawing.Point(110, 290);
            this.txtAccount.Name = "txtAccount";
            this.txtAccount.Size = new System.Drawing.Size(148, 23);
            this.txtAccount.TabIndex = 3;
            this.txtAccount.Visible = false;
            this.txtAccount.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtAccount_KeyPress);
            // 
            // picHead
            // 
            this.picHead.BackColor = System.Drawing.Color.Transparent;
            this.picHead.BackgroundImage = global::WinFrmTalk.Properties.Resources.avatar_default;
            this.picHead.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.picHead.isDrawRound = true;
            this.picHead.Location = new System.Drawing.Point(110, 31);
            this.picHead.Name = "picHead";
            this.picHead.Size = new System.Drawing.Size(80, 80);
            this.picHead.TabIndex = 27;
            this.picHead.TabStop = false;
            this.picHead.Click += new System.EventHandler(this.PicHead_Click);
            // 
            // cmbSex
            // 
            this.cmbSex.ArrowColor = System.Drawing.Color.Black;
            this.cmbSex.BaseColor = System.Drawing.SystemColors.Window;
            this.cmbSex.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.cmbSex.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbSex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSex.FormattingEnabled = true;
            this.cmbSex.Items.AddRange(new object[] {
            "男",
            "女"});
            this.cmbSex.Location = new System.Drawing.Point(110, 182);
            this.cmbSex.Name = "cmbSex";
            this.cmbSex.Size = new System.Drawing.Size(148, 22);
            this.cmbSex.TabIndex = 64;
            this.cmbSex.WaterText = "";
            // 
            // FrmRegisterInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(300, 400);
            this.Controls.Add(this.cmbSex);
            this.Controls.Add(this.picHead);
            this.Controls.Add(this.btnSubmit);
            this.Controls.Add(this.txtAccount);
            this.Controls.Add(this.txtRegion);
            this.Controls.Add(this.txtNickname);
            this.Controls.Add(this.dtpBirthday);
            this.Controls.Add(this.lblAccountIM);
            this.Controls.Add(this.lblLivePlace);
            this.Controls.Add(this.lblBirthday);
            this.Controls.Add(this.lblSex);
            this.Controls.Add(this.lblNickname);
            this.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.MaximizeBox = false;
            this.Name = "FrmRegisterInfo";
            this.ShowDrawIcon = false;
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "";
            this.TitleColor = System.Drawing.Color.Gray;
            this.Load += new System.EventHandler(this.FrmRegisterInfo_Load);
            this.cmsSex.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picHead)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblNickname;
        private System.Windows.Forms.Label lblSex;
        private System.Windows.Forms.Label lblBirthday;
        private System.Windows.Forms.Label lblLivePlace;
        private System.Windows.Forms.DateTimePicker dtpBirthday;
        private System.Windows.Forms.ContextMenuStrip cmsSex;
        private System.Windows.Forms.ToolStripMenuItem nan;
        private System.Windows.Forms.ToolStripMenuItem nv;
        private System.Windows.Forms.TextBox txtNickname;
        private System.Windows.Forms.TextBox txtRegion;
        private System.Windows.Forms.ContextMenuStrip cmsRegion;
        private System.Windows.Forms.Button btnSubmit;
        private System.Windows.Forms.Label lblAccountIM;
        private System.Windows.Forms.TextBox txtAccount;
        private RoundPicBox picHead;
        private CCWin.SkinControl.SkinComboBox cmbSex;
    }
}