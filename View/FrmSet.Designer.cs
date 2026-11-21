using System.Drawing;

namespace WinFrmTalk
{
    partial class FrmSet
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmSet));
            this.tabSet = new CCWin.SkinControl.SkinTabControl();
            this.tpAccountSet = new CCWin.SkinControl.SkinTabPage();
            this.btnImport = new System.Windows.Forms.Button();
            this.picHead = new WinFrmTalk.RoundPicBox();
            this.skinLine2 = new CCWin.SkinControl.SkinLine();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tpGeneralSet = new CCWin.SkinControl.SkinTabPage();
            this.cmbLanguage = new WinFrmTalk.Controls.EQComboBox();
            this.btnmuchSend = new System.Windows.Forms.Button();
            this.btninputAllroomtxt = new System.Windows.Forms.Button();
            this.btninputAllfriendtxt = new System.Windows.Forms.Button();
            this.btninputAllroomxls = new System.Windows.Forms.Button();
            this.btninputAllfriendxls = new System.Windows.Forms.Button();
            this.skinLine5 = new CCWin.SkinControl.SkinLine();
            this.btnDeleteChat = new System.Windows.Forms.Button();
            this.btnDeleteCache = new System.Windows.Forms.Button();
            this.tpPrivacySet = new CCWin.SkinControl.SkinTabPage();
            this.chkNewsState = new WinFrmTalk.Controls.CustomControls.USEToggle();
            this.lblMsgStatus = new System.Windows.Forms.Label();
            this.chkshowtips = new WinFrmTalk.Controls.CustomControls.USEToggle();
            this.lblMsgPopup = new System.Windows.Forms.Label();
            this.picallowtoway = new System.Windows.Forms.PictureBox();
            this.chkAllowtoCustomer = new WinFrmTalk.Controls.CustomControls.USEToggle();
            this.lblServicePattern = new System.Windows.Forms.Label();
            this.chkAllowtoNickname = new WinFrmTalk.Controls.CustomControls.USEToggle();
            this.chkAllowtoPhone = new WinFrmTalk.Controls.CustomControls.USEToggle();
            this.lblAddMeName = new System.Windows.Forms.Label();
            this.lblAddMePhone = new System.Windows.Forms.Label();
            this.lblallowtoway = new System.Windows.Forms.Label();
            this.lblAddWay = new System.Windows.Forms.Label();
            this.chkPrivacy = new WinFrmTalk.Controls.CustomControls.USEToggle();
            this.chkSendInput = new WinFrmTalk.Controls.CustomControls.USEToggle();
            this.chkMultipleDevices = new WinFrmTalk.Controls.CustomControls.USEToggle();
            this.picnewstime = new System.Windows.Forms.PictureBox();
            this.lblOverdueDate = new System.Windows.Forms.Label();
            this.lblRoamingTime = new System.Windows.Forms.Label();
            this.lblDevicesLogin = new System.Windows.Forms.Label();
            this.lblKnowTyping = new System.Windows.Forms.Label();
            this.lblFdValidation = new System.Windows.Forms.Label();
            this.skinLine4 = new CCWin.SkinControl.SkinLine();
            this.tpChangePwd = new CCWin.SkinControl.SkinTabPage();
            this.txtConfirmPwd = new System.Windows.Forms.TextBox();
            this.lblVerifyPwd = new System.Windows.Forms.Label();
            this.txtNewPwd = new System.Windows.Forms.TextBox();
            this.txtOldPwd = new System.Windows.Forms.TextBox();
            this.lblNewPwd = new System.Windows.Forms.Label();
            this.lblOldPwd = new System.Windows.Forms.Label();
            this.btnRevise = new LollipopButton();
            this.skinLine3 = new CCWin.SkinControl.SkinLine();
            this.tpAboutUs = new CCWin.SkinControl.SkinTabPage();
            this.lblCompanyName = new System.Windows.Forms.Label();
            this.lblCopyright = new System.Windows.Forms.Label();
            this.skinLine1 = new CCWin.SkinControl.SkinLine();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.lblEdition = new System.Windows.Forms.Label();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.cmsOverdueDate = new CCWin.SkinControl.SkinContextMenuStrip();
            this.tsmForever = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmHour = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmDay = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmWeek = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmMonth = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmSeason = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmYear = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmsynchronize = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripComboBox2 = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripComboBox3 = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripComboBox1 = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripComboBox4 = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripComboBox5 = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripComboBox6 = new System.Windows.Forms.ToolStripComboBox();
            this.tabSet.SuspendLayout();
            this.tpAccountSet.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picHead)).BeginInit();
            this.tpGeneralSet.SuspendLayout();
            this.tpPrivacySet.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picallowtoway)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picnewstime)).BeginInit();
            this.tpChangePwd.SuspendLayout();
            this.tpAboutUs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.cmsOverdueDate.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabSet
            // 
            this.tabSet.AnimatorType = CCWin.SkinControl.AnimationType.HorizSlide;
            this.tabSet.ArrowBaseColor = System.Drawing.SystemColors.Control;
            this.tabSet.ArrowBorderColor = System.Drawing.SystemColors.Control;
            this.tabSet.ArrowColor = System.Drawing.SystemColors.Control;
            this.tabSet.BackColor = System.Drawing.SystemColors.Control;
            this.tabSet.CloseRect = new System.Drawing.Rectangle(2, 2, 12, 12);
            this.tabSet.Controls.Add(this.tpAccountSet);
            this.tabSet.Controls.Add(this.tpGeneralSet);
            this.tabSet.Controls.Add(this.tpPrivacySet);
            this.tabSet.Controls.Add(this.tpChangePwd);
            this.tabSet.Controls.Add(this.tpAboutUs);
            this.tabSet.Cursor = System.Windows.Forms.Cursors.Default;
            this.tabSet.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tabSet.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabSet.HeadBack = null;
            this.tabSet.ImgTxtOffset = new System.Drawing.Point(0, 0);
            this.tabSet.ItemSize = new System.Drawing.Size(80, 36);
            this.tabSet.Location = new System.Drawing.Point(21, 46);
            this.tabSet.Multiline = true;
            this.tabSet.Name = "tabSet";
            this.tabSet.PageArrowDown = null;
            this.tabSet.PageArrowHover = null;
            this.tabSet.PageBaseColor = System.Drawing.SystemColors.Control;
            this.tabSet.PageBorderColor = System.Drawing.Color.White;
            this.tabSet.PageCloseHover = ((System.Drawing.Image)(resources.GetObject("tabSet.PageCloseHover")));
            this.tabSet.PageCloseNormal = ((System.Drawing.Image)(resources.GetObject("tabSet.PageCloseNormal")));
            this.tabSet.PageDown = null;
            this.tabSet.PageHover = ((System.Drawing.Image)(resources.GetObject("tabSet.PageHover")));
            this.tabSet.PageHoverTxtColor = System.Drawing.Color.DimGray;
            this.tabSet.PageImagePosition = CCWin.SkinControl.SkinTabControl.ePageImagePosition.Left;
            this.tabSet.PageNorml = null;
            this.tabSet.PageNormlTxtColor = System.Drawing.Color.Gray;
            this.tabSet.SelectedIndex = 1;
            this.tabSet.Size = new System.Drawing.Size(518, 478);
            this.tabSet.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabSet.TabIndex = 0;
            // 
            // tpAccountSet
            // 
            this.tpAccountSet.BackColor = System.Drawing.SystemColors.Control;
            this.tpAccountSet.Controls.Add(this.btnImport);
            this.tpAccountSet.Controls.Add(this.picHead);
            this.tpAccountSet.Controls.Add(this.skinLine2);
            this.tpAccountSet.Controls.Add(this.btnEdit);
            this.tpAccountSet.Controls.Add(this.btnCancel);
            this.tpAccountSet.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.tpAccountSet.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tpAccountSet.Location = new System.Drawing.Point(0, 36);
            this.tpAccountSet.Name = "tpAccountSet";
            this.tpAccountSet.Size = new System.Drawing.Size(518, 442);
            this.tpAccountSet.TabIndex = 4;
            this.tpAccountSet.TabItemImage = null;
            this.tpAccountSet.Text = "账号设置";
            // 
            // btnImport
            // 
            this.btnImport.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnImport.FlatAppearance.BorderSize = 0;
            this.btnImport.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnImport.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnImport.Location = new System.Drawing.Point(184, 259);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(120, 28);
            this.btnImport.TabIndex = 8;
            this.btnImport.Text = "一键导入国际化";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Visible = false;
            this.btnImport.Click += new System.EventHandler(this.BtnImport_Click);
            // 
            // picHead
            // 
            this.picHead.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.picHead.BackColor = System.Drawing.Color.Transparent;
            this.picHead.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.picHead.isDrawRound = true;
            this.picHead.Location = new System.Drawing.Point(194, 45);
            this.picHead.Name = "picHead";
            this.picHead.Size = new System.Drawing.Size(100, 100);
            this.picHead.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picHead.TabIndex = 7;
            this.picHead.TabStop = false;
            // 
            // skinLine2
            // 
            this.skinLine2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(197)))), ((int)(((byte)(197)))), ((int)(((byte)(197)))));
            this.skinLine2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.skinLine2.LineColor = System.Drawing.Color.DimGray;
            this.skinLine2.LineHeight = 1;
            this.skinLine2.Location = new System.Drawing.Point(0, 0);
            this.skinLine2.Name = "skinLine2";
            this.skinLine2.Size = new System.Drawing.Size(1, 405);
            this.skinLine2.TabIndex = 6;
            this.skinLine2.Text = "skinLine2";
            // 
            // btnEdit
            // 
            this.btnEdit.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnEdit.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnEdit.Location = new System.Drawing.Point(184, 172);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(120, 28);
            this.btnEdit.TabIndex = 1;
            this.btnEdit.Text = "编辑资料";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnCancel.Location = new System.Drawing.Point(184, 215);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(120, 28);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "退出登录";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // tpGeneralSet
            // 
            this.tpGeneralSet.BackColor = System.Drawing.SystemColors.Control;
            this.tpGeneralSet.Controls.Add(this.cmbLanguage);
            this.tpGeneralSet.Controls.Add(this.btnmuchSend);
            this.tpGeneralSet.Controls.Add(this.btninputAllroomtxt);
            this.tpGeneralSet.Controls.Add(this.btninputAllfriendtxt);
            this.tpGeneralSet.Controls.Add(this.btninputAllroomxls);
            this.tpGeneralSet.Controls.Add(this.btninputAllfriendxls);
            this.tpGeneralSet.Controls.Add(this.skinLine5);
            this.tpGeneralSet.Controls.Add(this.btnDeleteChat);
            this.tpGeneralSet.Controls.Add(this.btnDeleteCache);
            this.tpGeneralSet.Cursor = System.Windows.Forms.Cursors.Default;
            this.tpGeneralSet.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tpGeneralSet.Location = new System.Drawing.Point(0, 36);
            this.tpGeneralSet.Name = "tpGeneralSet";
            this.tpGeneralSet.Size = new System.Drawing.Size(518, 442);
            this.tpGeneralSet.TabIndex = 0;
            this.tpGeneralSet.TabItemImage = null;
            this.tpGeneralSet.Text = "通用设置";
            // 
            // cmbLanguage
            // 
            this.cmbLanguage.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cmbLanguage.BackColor = System.Drawing.SystemColors.Control;
            this.cmbLanguage.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.cmbLanguage.Items = new System.Collections.Generic.List<string>();
            this.cmbLanguage.Location = new System.Drawing.Point(170, 20);
            this.cmbLanguage.Name = "cmbLanguage";
            this.cmbLanguage.SelectedIndex = -1;
            this.cmbLanguage.Size = new System.Drawing.Size(178, 24);
            this.cmbLanguage.TabIndex = 14;
            this.cmbLanguage.Text = null;
            // 
            // btnmuchSend
            // 
            this.btnmuchSend.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnmuchSend.FlatAppearance.BorderSize = 0;
            this.btnmuchSend.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnmuchSend.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnmuchSend.Location = new System.Drawing.Point(170, 267);
            this.btnmuchSend.Name = "btnmuchSend";
            this.btnmuchSend.Size = new System.Drawing.Size(178, 28);
            this.btnmuchSend.TabIndex = 12;
            this.btnmuchSend.Text = "群发消息";
            this.btnmuchSend.UseVisualStyleBackColor = true;
            this.btnmuchSend.Click += new System.EventHandler(this.btnmuchSend_Click);
            // 
            // btninputAllroomtxt
            // 
            this.btninputAllroomtxt.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btninputAllroomtxt.FlatAppearance.BorderSize = 0;
            this.btninputAllroomtxt.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btninputAllroomtxt.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btninputAllroomtxt.Location = new System.Drawing.Point(170, 216);
            this.btninputAllroomtxt.Name = "btninputAllroomtxt";
            this.btninputAllroomtxt.Size = new System.Drawing.Size(178, 28);
            this.btninputAllroomtxt.TabIndex = 11;
            this.btninputAllroomtxt.Text = "导出所有群组聊天记录txt";
            this.btninputAllroomtxt.UseVisualStyleBackColor = true;
            this.btninputAllroomtxt.Click += new System.EventHandler(this.btninputAllroomtxt_Click);
            // 
            // btninputAllfriendtxt
            // 
            this.btninputAllfriendtxt.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btninputAllfriendtxt.FlatAppearance.BorderSize = 0;
            this.btninputAllfriendtxt.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btninputAllfriendtxt.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btninputAllfriendtxt.Location = new System.Drawing.Point(170, 167);
            this.btninputAllfriendtxt.Name = "btninputAllfriendtxt";
            this.btninputAllfriendtxt.Size = new System.Drawing.Size(178, 28);
            this.btninputAllfriendtxt.TabIndex = 10;
            this.btninputAllfriendtxt.Text = "导出全部好友聊天记录txt";
            this.btninputAllfriendtxt.UseVisualStyleBackColor = true;
            this.btninputAllfriendtxt.Click += new System.EventHandler(this.btninputAllfriendtxt_Click);
            // 
            // btninputAllroomxls
            // 
            this.btninputAllroomxls.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btninputAllroomxls.FlatAppearance.BorderSize = 0;
            this.btninputAllroomxls.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btninputAllroomxls.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btninputAllroomxls.Location = new System.Drawing.Point(170, 118);
            this.btninputAllroomxls.Name = "btninputAllroomxls";
            this.btninputAllroomxls.Size = new System.Drawing.Size(178, 28);
            this.btninputAllroomxls.TabIndex = 9;
            this.btninputAllroomxls.Text = "导出所有群组聊天记录xls";
            this.btninputAllroomxls.UseVisualStyleBackColor = true;
            this.btninputAllroomxls.Click += new System.EventHandler(this.btninputAllroomxls_Click);
            // 
            // btninputAllfriendxls
            // 
            this.btninputAllfriendxls.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btninputAllfriendxls.FlatAppearance.BorderSize = 0;
            this.btninputAllfriendxls.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btninputAllfriendxls.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btninputAllfriendxls.Location = new System.Drawing.Point(170, 69);
            this.btninputAllfriendxls.Name = "btninputAllfriendxls";
            this.btninputAllfriendxls.Size = new System.Drawing.Size(178, 28);
            this.btninputAllfriendxls.TabIndex = 8;
            this.btninputAllfriendxls.Text = "导出全部好友聊天记录xls";
            this.btninputAllfriendxls.UseVisualStyleBackColor = true;
            this.btninputAllfriendxls.Click += new System.EventHandler(this.btninputAllfriendxls_Click);
            // 
            // skinLine5
            // 
            this.skinLine5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(197)))), ((int)(((byte)(197)))), ((int)(((byte)(197)))));
            this.skinLine5.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.skinLine5.LineColor = System.Drawing.Color.DimGray;
            this.skinLine5.LineHeight = 1;
            this.skinLine5.Location = new System.Drawing.Point(0, 0);
            this.skinLine5.Name = "skinLine5";
            this.skinLine5.Size = new System.Drawing.Size(1, 405);
            this.skinLine5.TabIndex = 7;
            this.skinLine5.Text = "skinLine5";
            // 
            // btnDeleteChat
            // 
            this.btnDeleteChat.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnDeleteChat.FlatAppearance.BorderSize = 0;
            this.btnDeleteChat.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnDeleteChat.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnDeleteChat.Location = new System.Drawing.Point(170, 365);
            this.btnDeleteChat.Name = "btnDeleteChat";
            this.btnDeleteChat.Size = new System.Drawing.Size(178, 28);
            this.btnDeleteChat.TabIndex = 1;
            this.btnDeleteChat.Text = "清除聊天记录";
            this.btnDeleteChat.UseVisualStyleBackColor = true;
            this.btnDeleteChat.Click += new System.EventHandler(this.btnDeleteChat_Click);
            // 
            // btnDeleteCache
            // 
            this.btnDeleteCache.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnDeleteCache.FlatAppearance.BorderSize = 0;
            this.btnDeleteCache.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnDeleteCache.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnDeleteCache.Location = new System.Drawing.Point(170, 316);
            this.btnDeleteCache.Name = "btnDeleteCache";
            this.btnDeleteCache.Size = new System.Drawing.Size(178, 28);
            this.btnDeleteCache.TabIndex = 0;
            this.btnDeleteCache.Text = "清除缓存";
            this.btnDeleteCache.UseVisualStyleBackColor = true;
            this.btnDeleteCache.Click += new System.EventHandler(this.btnDeleteCache_Click);
            // 
            // tpPrivacySet
            // 
            this.tpPrivacySet.BackColor = System.Drawing.SystemColors.Control;
            this.tpPrivacySet.Controls.Add(this.chkNewsState);
            this.tpPrivacySet.Controls.Add(this.lblMsgStatus);
            this.tpPrivacySet.Controls.Add(this.chkshowtips);
            this.tpPrivacySet.Controls.Add(this.lblMsgPopup);
            this.tpPrivacySet.Controls.Add(this.picallowtoway);
            this.tpPrivacySet.Controls.Add(this.chkAllowtoCustomer);
            this.tpPrivacySet.Controls.Add(this.lblServicePattern);
            this.tpPrivacySet.Controls.Add(this.chkAllowtoNickname);
            this.tpPrivacySet.Controls.Add(this.chkAllowtoPhone);
            this.tpPrivacySet.Controls.Add(this.lblAddMeName);
            this.tpPrivacySet.Controls.Add(this.lblAddMePhone);
            this.tpPrivacySet.Controls.Add(this.lblallowtoway);
            this.tpPrivacySet.Controls.Add(this.lblAddWay);
            this.tpPrivacySet.Controls.Add(this.chkPrivacy);
            this.tpPrivacySet.Controls.Add(this.chkSendInput);
            this.tpPrivacySet.Controls.Add(this.chkMultipleDevices);
            this.tpPrivacySet.Controls.Add(this.picnewstime);
            this.tpPrivacySet.Controls.Add(this.lblOverdueDate);
            this.tpPrivacySet.Controls.Add(this.lblRoamingTime);
            this.tpPrivacySet.Controls.Add(this.lblDevicesLogin);
            this.tpPrivacySet.Controls.Add(this.lblKnowTyping);
            this.tpPrivacySet.Controls.Add(this.lblFdValidation);
            this.tpPrivacySet.Controls.Add(this.skinLine4);
            this.tpPrivacySet.Cursor = System.Windows.Forms.Cursors.Default;
            this.tpPrivacySet.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tpPrivacySet.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tpPrivacySet.Location = new System.Drawing.Point(0, 36);
            this.tpPrivacySet.Name = "tpPrivacySet";
            this.tpPrivacySet.Size = new System.Drawing.Size(518, 442);
            this.tpPrivacySet.TabIndex = 1;
            this.tpPrivacySet.TabItemImage = null;
            this.tpPrivacySet.Text = "隐私设置";
            // 
            // chkNewsState
            // 
            this.chkNewsState.BackColor = System.Drawing.Color.Transparent;
            this.chkNewsState.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.chkNewsState.Checked = false;
            this.chkNewsState.checkStyle = WinFrmTalk.Controls.CustomControls.CheckStyle.style1;
            this.chkNewsState.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkNewsState.Location = new System.Drawing.Point(330, 380);
            this.chkNewsState.Margin = new System.Windows.Forms.Padding(3, 23, 3, 23);
            this.chkNewsState.Name = "chkNewsState";
            this.chkNewsState.Size = new System.Drawing.Size(31, 30);
            this.chkNewsState.TabIndex = 46;
            this.chkNewsState.Click += new System.EventHandler(this.chkNewsState_Click);
            // 
            // lblMsgStatus
            // 
            this.lblMsgStatus.AutoSize = true;
            this.lblMsgStatus.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblMsgStatus.ForeColor = System.Drawing.Color.Gray;
            this.lblMsgStatus.Location = new System.Drawing.Point(97, 389);
            this.lblMsgStatus.Name = "lblMsgStatus";
            this.lblMsgStatus.Size = new System.Drawing.Size(104, 17);
            this.lblMsgStatus.TabIndex = 45;
            this.lblMsgStatus.Text = "发送消息阅读状态";
            // 
            // chkshowtips
            // 
            this.chkshowtips.BackColor = System.Drawing.Color.Transparent;
            this.chkshowtips.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.chkshowtips.Checked = false;
            this.chkshowtips.checkStyle = WinFrmTalk.Controls.CustomControls.CheckStyle.style1;
            this.chkshowtips.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkshowtips.Location = new System.Drawing.Point(330, 339);
            this.chkshowtips.Margin = new System.Windows.Forms.Padding(3, 16, 3, 16);
            this.chkshowtips.Name = "chkshowtips";
            this.chkshowtips.Size = new System.Drawing.Size(31, 30);
            this.chkshowtips.TabIndex = 44;
            this.chkshowtips.Click += new System.EventHandler(this.chkshowtips_Click);
            // 
            // lblMsgPopup
            // 
            this.lblMsgPopup.AutoSize = true;
            this.lblMsgPopup.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblMsgPopup.ForeColor = System.Drawing.Color.Gray;
            this.lblMsgPopup.Location = new System.Drawing.Point(97, 352);
            this.lblMsgPopup.Name = "lblMsgPopup";
            this.lblMsgPopup.Size = new System.Drawing.Size(68, 17);
            this.lblMsgPopup.TabIndex = 43;
            this.lblMsgPopup.Text = "新消息弹窗";
            // 
            // picallowtoway
            // 
            this.picallowtoway.BackColor = System.Drawing.Color.Transparent;
            this.picallowtoway.Image = global::WinFrmTalk.Properties.Resources.right;
            this.picallowtoway.Location = new System.Drawing.Point(406, 276);
            this.picallowtoway.Name = "picallowtoway";
            this.picallowtoway.Size = new System.Drawing.Size(12, 12);
            this.picallowtoway.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picallowtoway.TabIndex = 42;
            this.picallowtoway.TabStop = false;
            this.picallowtoway.Click += new System.EventHandler(this.allowtoway_Click);
            // 
            // chkAllowtoCustomer
            // 
            this.chkAllowtoCustomer.BackColor = System.Drawing.Color.Transparent;
            this.chkAllowtoCustomer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.chkAllowtoCustomer.Checked = false;
            this.chkAllowtoCustomer.checkStyle = WinFrmTalk.Controls.CustomControls.CheckStyle.style1;
            this.chkAllowtoCustomer.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkAllowtoCustomer.Location = new System.Drawing.Point(330, 306);
            this.chkAllowtoCustomer.Margin = new System.Windows.Forms.Padding(3, 11, 3, 11);
            this.chkAllowtoCustomer.Name = "chkAllowtoCustomer";
            this.chkAllowtoCustomer.Size = new System.Drawing.Size(31, 30);
            this.chkAllowtoCustomer.TabIndex = 41;
            this.chkAllowtoCustomer.Click += new System.EventHandler(this.chkAllowtoCustomer_Click);
            // 
            // lblServicePattern
            // 
            this.lblServicePattern.AutoSize = true;
            this.lblServicePattern.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblServicePattern.ForeColor = System.Drawing.Color.Gray;
            this.lblServicePattern.Location = new System.Drawing.Point(97, 313);
            this.lblServicePattern.Name = "lblServicePattern";
            this.lblServicePattern.Size = new System.Drawing.Size(56, 17);
            this.lblServicePattern.TabIndex = 40;
            this.lblServicePattern.Text = "客服模式";
            // 
            // chkAllowtoNickname
            // 
            this.chkAllowtoNickname.BackColor = System.Drawing.Color.Transparent;
            this.chkAllowtoNickname.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.chkAllowtoNickname.Checked = false;
            this.chkAllowtoNickname.checkStyle = WinFrmTalk.Controls.CustomControls.CheckStyle.style1;
            this.chkAllowtoNickname.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkAllowtoNickname.Location = new System.Drawing.Point(330, 179);
            this.chkAllowtoNickname.Margin = new System.Windows.Forms.Padding(3, 8, 3, 8);
            this.chkAllowtoNickname.Name = "chkAllowtoNickname";
            this.chkAllowtoNickname.Size = new System.Drawing.Size(31, 30);
            this.chkAllowtoNickname.TabIndex = 39;
            this.chkAllowtoNickname.Click += new System.EventHandler(this.chkAllowtoNickname_Click);
            // 
            // chkAllowtoPhone
            // 
            this.chkAllowtoPhone.BackColor = System.Drawing.Color.Transparent;
            this.chkAllowtoPhone.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.chkAllowtoPhone.Checked = false;
            this.chkAllowtoPhone.checkStyle = WinFrmTalk.Controls.CustomControls.CheckStyle.style1;
            this.chkAllowtoPhone.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkAllowtoPhone.Location = new System.Drawing.Point(330, 141);
            this.chkAllowtoPhone.Margin = new System.Windows.Forms.Padding(3, 8, 3, 8);
            this.chkAllowtoPhone.Name = "chkAllowtoPhone";
            this.chkAllowtoPhone.Size = new System.Drawing.Size(31, 30);
            this.chkAllowtoPhone.TabIndex = 38;
            this.chkAllowtoPhone.Click += new System.EventHandler(this.chkAllowtoPhone_Click);
            // 
            // lblAddMeName
            // 
            this.lblAddMeName.AutoSize = true;
            this.lblAddMeName.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblAddMeName.ForeColor = System.Drawing.Color.Gray;
            this.lblAddMeName.Location = new System.Drawing.Point(97, 189);
            this.lblAddMeName.Name = "lblAddMeName";
            this.lblAddMeName.Size = new System.Drawing.Size(116, 17);
            this.lblAddMeName.TabIndex = 37;
            this.lblAddMeName.Text = "允许通过昵称搜索我";
            // 
            // lblAddMePhone
            // 
            this.lblAddMePhone.AutoSize = true;
            this.lblAddMePhone.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblAddMePhone.ForeColor = System.Drawing.Color.Gray;
            this.lblAddMePhone.Location = new System.Drawing.Point(97, 148);
            this.lblAddMePhone.Name = "lblAddMePhone";
            this.lblAddMePhone.Size = new System.Drawing.Size(128, 17);
            this.lblAddMePhone.TabIndex = 36;
            this.lblAddMePhone.Text = "允许通过手机号搜索我";
            // 
            // lblallowtoway
            // 
            this.lblallowtoway.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblallowtoway.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblallowtoway.ForeColor = System.Drawing.Color.Gray;
            this.lblallowtoway.Location = new System.Drawing.Point(327, 273);
            this.lblallowtoway.Name = "lblallowtoway";
            this.lblallowtoway.Size = new System.Drawing.Size(114, 19);
            this.lblallowtoway.TabIndex = 33;
            this.lblallowtoway.Text = "二维码";
            this.lblallowtoway.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblallowtoway.Click += new System.EventHandler(this.allowtoway_Click);
            // 
            // lblAddWay
            // 
            this.lblAddWay.AutoSize = true;
            this.lblAddWay.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblAddWay.ForeColor = System.Drawing.Color.Gray;
            this.lblAddWay.Location = new System.Drawing.Point(97, 274);
            this.lblAddWay.Name = "lblAddWay";
            this.lblAddWay.Size = new System.Drawing.Size(92, 17);
            this.lblAddWay.TabIndex = 34;
            this.lblAddWay.Text = "允许加我的方式";
            // 
            // chkPrivacy
            // 
            this.chkPrivacy.BackColor = System.Drawing.Color.Transparent;
            this.chkPrivacy.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.chkPrivacy.Checked = false;
            this.chkPrivacy.checkStyle = WinFrmTalk.Controls.CustomControls.CheckStyle.style1;
            this.chkPrivacy.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkPrivacy.Location = new System.Drawing.Point(330, 17);
            this.chkPrivacy.Margin = new System.Windows.Forms.Padding(3, 8, 3, 8);
            this.chkPrivacy.Name = "chkPrivacy";
            this.chkPrivacy.Size = new System.Drawing.Size(31, 30);
            this.chkPrivacy.TabIndex = 32;
            this.chkPrivacy.Click += new System.EventHandler(this.chkPrivacy_CheckedChanged);
            // 
            // chkSendInput
            // 
            this.chkSendInput.BackColor = System.Drawing.Color.Transparent;
            this.chkSendInput.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.chkSendInput.Checked = false;
            this.chkSendInput.checkStyle = WinFrmTalk.Controls.CustomControls.CheckStyle.style1;
            this.chkSendInput.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkSendInput.Location = new System.Drawing.Point(330, 56);
            this.chkSendInput.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.chkSendInput.Name = "chkSendInput";
            this.chkSendInput.Size = new System.Drawing.Size(31, 30);
            this.chkSendInput.TabIndex = 30;
            this.chkSendInput.Click += new System.EventHandler(this.chkSendInput_CheckedChanged);
            // 
            // chkMultipleDevices
            // 
            this.chkMultipleDevices.BackColor = System.Drawing.Color.Transparent;
            this.chkMultipleDevices.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.chkMultipleDevices.Checked = false;
            this.chkMultipleDevices.checkStyle = WinFrmTalk.Controls.CustomControls.CheckStyle.style1;
            this.chkMultipleDevices.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkMultipleDevices.Location = new System.Drawing.Point(330, 98);
            this.chkMultipleDevices.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkMultipleDevices.Name = "chkMultipleDevices";
            this.chkMultipleDevices.Size = new System.Drawing.Size(31, 30);
            this.chkMultipleDevices.TabIndex = 29;
            this.chkMultipleDevices.Click += new System.EventHandler(this.chkMultipleDevices_CheckedChanged);
            // 
            // picnewstime
            // 
            this.picnewstime.BackColor = System.Drawing.Color.Transparent;
            this.picnewstime.Image = global::WinFrmTalk.Properties.Resources.right;
            this.picnewstime.Location = new System.Drawing.Point(406, 230);
            this.picnewstime.Name = "picnewstime";
            this.picnewstime.Size = new System.Drawing.Size(12, 12);
            this.picnewstime.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picnewstime.TabIndex = 28;
            this.picnewstime.TabStop = false;
            this.picnewstime.Click += new System.EventHandler(this.lblOverdueDate_Click);
            // 
            // lblOverdueDate
            // 
            this.lblOverdueDate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblOverdueDate.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblOverdueDate.ForeColor = System.Drawing.Color.Gray;
            this.lblOverdueDate.Location = new System.Drawing.Point(327, 229);
            this.lblOverdueDate.Name = "lblOverdueDate";
            this.lblOverdueDate.Size = new System.Drawing.Size(100, 14);
            this.lblOverdueDate.TabIndex = 26;
            this.lblOverdueDate.Text = "永久";
            this.lblOverdueDate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblOverdueDate.Click += new System.EventHandler(this.lblOverdueDate_Click);
            // 
            // lblRoamingTime
            // 
            this.lblRoamingTime.AutoSize = true;
            this.lblRoamingTime.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblRoamingTime.ForeColor = System.Drawing.Color.Gray;
            this.lblRoamingTime.Location = new System.Drawing.Point(97, 230);
            this.lblRoamingTime.Name = "lblRoamingTime";
            this.lblRoamingTime.Size = new System.Drawing.Size(80, 17);
            this.lblRoamingTime.TabIndex = 27;
            this.lblRoamingTime.Text = "消息漫游时长";
            // 
            // lblDevicesLogin
            // 
            this.lblDevicesLogin.AutoSize = true;
            this.lblDevicesLogin.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblDevicesLogin.ForeColor = System.Drawing.Color.Gray;
            this.lblDevicesLogin.Location = new System.Drawing.Point(97, 105);
            this.lblDevicesLogin.Name = "lblDevicesLogin";
            this.lblDevicesLogin.Size = new System.Drawing.Size(92, 17);
            this.lblDevicesLogin.TabIndex = 8;
            this.lblDevicesLogin.Text = "支持多设备登录";
            // 
            // lblKnowTyping
            // 
            this.lblKnowTyping.AutoSize = true;
            this.lblKnowTyping.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblKnowTyping.ForeColor = System.Drawing.Color.Gray;
            this.lblKnowTyping.Location = new System.Drawing.Point(97, 63);
            this.lblKnowTyping.Name = "lblKnowTyping";
            this.lblKnowTyping.Size = new System.Drawing.Size(128, 17);
            this.lblKnowTyping.TabIndex = 8;
            this.lblKnowTyping.Text = "让对方知道我正在输入";
            // 
            // lblFdValidation
            // 
            this.lblFdValidation.AutoSize = true;
            this.lblFdValidation.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblFdValidation.ForeColor = System.Drawing.Color.Gray;
            this.lblFdValidation.Location = new System.Drawing.Point(97, 24);
            this.lblFdValidation.Name = "lblFdValidation";
            this.lblFdValidation.Size = new System.Drawing.Size(56, 17);
            this.lblFdValidation.TabIndex = 8;
            this.lblFdValidation.Text = "好友验证";
            // 
            // skinLine4
            // 
            this.skinLine4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(197)))), ((int)(((byte)(197)))), ((int)(((byte)(197)))));
            this.skinLine4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.skinLine4.LineColor = System.Drawing.Color.DimGray;
            this.skinLine4.LineHeight = 1;
            this.skinLine4.Location = new System.Drawing.Point(0, 0);
            this.skinLine4.Name = "skinLine4";
            this.skinLine4.Size = new System.Drawing.Size(1, 405);
            this.skinLine4.TabIndex = 6;
            this.skinLine4.Text = "skinLine4";
            // 
            // tpChangePwd
            // 
            this.tpChangePwd.BackColor = System.Drawing.SystemColors.Control;
            this.tpChangePwd.Controls.Add(this.txtConfirmPwd);
            this.tpChangePwd.Controls.Add(this.lblVerifyPwd);
            this.tpChangePwd.Controls.Add(this.txtNewPwd);
            this.tpChangePwd.Controls.Add(this.txtOldPwd);
            this.tpChangePwd.Controls.Add(this.lblNewPwd);
            this.tpChangePwd.Controls.Add(this.lblOldPwd);
            this.tpChangePwd.Controls.Add(this.btnRevise);
            this.tpChangePwd.Controls.Add(this.skinLine3);
            this.tpChangePwd.Cursor = System.Windows.Forms.Cursors.Default;
            this.tpChangePwd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tpChangePwd.Location = new System.Drawing.Point(0, 36);
            this.tpChangePwd.Name = "tpChangePwd";
            this.tpChangePwd.Size = new System.Drawing.Size(518, 442);
            this.tpChangePwd.TabIndex = 2;
            this.tpChangePwd.TabItemImage = null;
            this.tpChangePwd.Text = "修改密码";
            // 
            // txtConfirmPwd
            // 
            this.txtConfirmPwd.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtConfirmPwd.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtConfirmPwd.Location = new System.Drawing.Point(204, 154);
            this.txtConfirmPwd.Name = "txtConfirmPwd";
            this.txtConfirmPwd.PasswordChar = '●';
            this.txtConfirmPwd.Size = new System.Drawing.Size(149, 25);
            this.txtConfirmPwd.TabIndex = 2;
            this.txtConfirmPwd.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtNewPwd_KeyPress);
            // 
            // lblVerifyPwd
            // 
            this.lblVerifyPwd.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblVerifyPwd.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblVerifyPwd.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lblVerifyPwd.Location = new System.Drawing.Point(78, 157);
            this.lblVerifyPwd.Name = "lblVerifyPwd";
            this.lblVerifyPwd.Size = new System.Drawing.Size(113, 17);
            this.lblVerifyPwd.TabIndex = 27;
            this.lblVerifyPwd.Text = "确认密码：";
            this.lblVerifyPwd.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtNewPwd
            // 
            this.txtNewPwd.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtNewPwd.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtNewPwd.Location = new System.Drawing.Point(204, 117);
            this.txtNewPwd.Name = "txtNewPwd";
            this.txtNewPwd.PasswordChar = '●';
            this.txtNewPwd.Size = new System.Drawing.Size(149, 25);
            this.txtNewPwd.TabIndex = 1;
            this.txtNewPwd.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtNewPwd_KeyPress);
            // 
            // txtOldPwd
            // 
            this.txtOldPwd.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtOldPwd.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtOldPwd.Location = new System.Drawing.Point(204, 80);
            this.txtOldPwd.Name = "txtOldPwd";
            this.txtOldPwd.PasswordChar = '●';
            this.txtOldPwd.Size = new System.Drawing.Size(149, 25);
            this.txtOldPwd.TabIndex = 0;
            this.txtOldPwd.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtNewPwd_KeyPress);
            // 
            // lblNewPwd
            // 
            this.lblNewPwd.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblNewPwd.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblNewPwd.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lblNewPwd.Location = new System.Drawing.Point(81, 120);
            this.lblNewPwd.Name = "lblNewPwd";
            this.lblNewPwd.Size = new System.Drawing.Size(110, 17);
            this.lblNewPwd.TabIndex = 23;
            this.lblNewPwd.Text = "新密码：";
            this.lblNewPwd.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblOldPwd
            // 
            this.lblOldPwd.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblOldPwd.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblOldPwd.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lblOldPwd.Location = new System.Drawing.Point(84, 83);
            this.lblOldPwd.Name = "lblOldPwd";
            this.lblOldPwd.Size = new System.Drawing.Size(107, 17);
            this.lblOldPwd.TabIndex = 24;
            this.lblOldPwd.Text = "旧密码：";
            this.lblOldPwd.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnRevise
            // 
            this.btnRevise.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnRevise.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(97)))), ((int)(((byte)(1)))));
            this.btnRevise.BGColor = "#1AAD19";
            this.btnRevise.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnRevise.FontColor = "#ffffff";
            this.btnRevise.Location = new System.Drawing.Point(204, 244);
            this.btnRevise.Name = "btnRevise";
            this.btnRevise.Size = new System.Drawing.Size(120, 38);
            this.btnRevise.TabIndex = 22;
            this.btnRevise.Text = "修改密码";
            this.btnRevise.Click += new System.EventHandler(this.btnRevise_Click);
            // 
            // skinLine3
            // 
            this.skinLine3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(197)))), ((int)(((byte)(197)))), ((int)(((byte)(197)))));
            this.skinLine3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.skinLine3.LineColor = System.Drawing.Color.DimGray;
            this.skinLine3.LineHeight = 1;
            this.skinLine3.Location = new System.Drawing.Point(0, 0);
            this.skinLine3.Name = "skinLine3";
            this.skinLine3.Size = new System.Drawing.Size(1, 405);
            this.skinLine3.TabIndex = 6;
            this.skinLine3.Text = "skinLine3";
            // 
            // tpAboutUs
            // 
            this.tpAboutUs.BackColor = System.Drawing.SystemColors.Control;
            this.tpAboutUs.Controls.Add(this.lblCompanyName);
            this.tpAboutUs.Controls.Add(this.lblCopyright);
            this.tpAboutUs.Controls.Add(this.skinLine1);
            this.tpAboutUs.Controls.Add(this.btnUpdate);
            this.tpAboutUs.Controls.Add(this.lblEdition);
            this.tpAboutUs.Controls.Add(this.picLogo);
            this.tpAboutUs.Cursor = System.Windows.Forms.Cursors.Default;
            this.tpAboutUs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tpAboutUs.Location = new System.Drawing.Point(0, 36);
            this.tpAboutUs.Name = "tpAboutUs";
            this.tpAboutUs.Size = new System.Drawing.Size(518, 442);
            this.tpAboutUs.TabIndex = 3;
            this.tpAboutUs.TabItemImage = null;
            this.tpAboutUs.Text = "关于我们";
            // 
            // lblCompanyName
            // 
            this.lblCompanyName.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblCompanyName.BackColor = System.Drawing.SystemColors.Control;
            this.lblCompanyName.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblCompanyName.ForeColor = System.Drawing.Color.Gray;
            this.lblCompanyName.Location = new System.Drawing.Point(50, 308);
            this.lblCompanyName.Name = "lblCompanyName";
            this.lblCompanyName.Size = new System.Drawing.Size(380, 17);
            this.lblCompanyName.TabIndex = 10;
            this.lblCompanyName.Text = "NULL";
            this.lblCompanyName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblCopyright
            // 
            this.lblCopyright.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblCopyright.BackColor = System.Drawing.SystemColors.Control;
            this.lblCopyright.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblCopyright.ForeColor = System.Drawing.Color.Gray;
            this.lblCopyright.Location = new System.Drawing.Point(50, 351);
            this.lblCopyright.Name = "lblCopyright";
            this.lblCopyright.Size = new System.Drawing.Size(380, 17);
            this.lblCopyright.TabIndex = 9;
            this.lblCopyright.Text = "NULL";
            this.lblCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // skinLine1
            // 
            this.skinLine1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(197)))), ((int)(((byte)(197)))), ((int)(((byte)(197)))));
            this.skinLine1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.skinLine1.LineColor = System.Drawing.Color.DimGray;
            this.skinLine1.LineHeight = 1;
            this.skinLine1.Location = new System.Drawing.Point(0, 0);
            this.skinLine1.Name = "skinLine1";
            this.skinLine1.Size = new System.Drawing.Size(1, 405);
            this.skinLine1.TabIndex = 5;
            this.skinLine1.Text = "skinLine1";
            // 
            // btnUpdate
            // 
            this.btnUpdate.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnUpdate.FlatAppearance.BorderSize = 0;
            this.btnUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnUpdate.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnUpdate.Location = new System.Drawing.Point(190, 197);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(100, 23);
            this.btnUpdate.TabIndex = 3;
            this.btnUpdate.Text = "检查更新";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // lblEdition
            // 
            this.lblEdition.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblEdition.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblEdition.Location = new System.Drawing.Point(190, 165);
            this.lblEdition.Name = "lblEdition";
            this.lblEdition.Size = new System.Drawing.Size(100, 20);
            this.lblEdition.TabIndex = 1;
            this.lblEdition.Text = "NULL";
            this.lblEdition.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // picLogo
            // 
            this.picLogo.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.picLogo.Image = global::WinFrmTalk.Properties.Resources.Logo;
            this.picLogo.Location = new System.Drawing.Point(190, 45);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(100, 100);
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picLogo.TabIndex = 0;
            this.picLogo.TabStop = false;
            // 
            // cmsOverdueDate
            // 
            this.cmsOverdueDate.Arrow = System.Drawing.Color.Black;
            this.cmsOverdueDate.Back = System.Drawing.Color.WhiteSmoke;
            this.cmsOverdueDate.BackColor = System.Drawing.Color.WhiteSmoke;
            this.cmsOverdueDate.BackRadius = 4;
            this.cmsOverdueDate.Base = System.Drawing.Color.WhiteSmoke;
            this.cmsOverdueDate.DropDownImageSeparator = System.Drawing.Color.Silver;
            this.cmsOverdueDate.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cmsOverdueDate.Fore = System.Drawing.Color.Black;
            this.cmsOverdueDate.HoverFore = System.Drawing.Color.Black;
            this.cmsOverdueDate.ItemAnamorphosis = false;
            this.cmsOverdueDate.ItemBorder = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(233)))), ((int)(((byte)(237)))));
            this.cmsOverdueDate.ItemBorderShow = false;
            this.cmsOverdueDate.ItemHover = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(233)))), ((int)(((byte)(237)))));
            this.cmsOverdueDate.ItemPressed = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(233)))), ((int)(((byte)(237)))));
            this.cmsOverdueDate.ItemRadius = 4;
            this.cmsOverdueDate.ItemRadiusStyle = CCWin.SkinClass.RoundStyle.None;
            this.cmsOverdueDate.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmForever,
            this.tsmHour,
            this.tsmDay,
            this.tsmWeek,
            this.tsmMonth,
            this.tsmSeason,
            this.tsmYear,
            this.tsmsynchronize});
            this.cmsOverdueDate.ItemSplitter = System.Drawing.Color.Silver;
            this.cmsOverdueDate.Name = "contextMenuStrip1";
            this.cmsOverdueDate.RadiusStyle = CCWin.SkinClass.RoundStyle.None;
            this.cmsOverdueDate.Size = new System.Drawing.Size(113, 180);
            this.cmsOverdueDate.SkinAllColor = true;
            this.cmsOverdueDate.Tag = "";
            this.cmsOverdueDate.TitleAnamorphosis = true;
            this.cmsOverdueDate.TitleColor = System.Drawing.Color.WhiteSmoke;
            this.cmsOverdueDate.TitleRadius = 4;
            this.cmsOverdueDate.TitleRadiusStyle = CCWin.SkinClass.RoundStyle.None;
            // 
            // tsmForever
            // 
            this.tsmForever.Name = "tsmForever";
            this.tsmForever.Size = new System.Drawing.Size(112, 22);
            this.tsmForever.Text = "永久";
            this.tsmForever.Click += new System.EventHandler(this.tsmForever_Click);
            // 
            // tsmHour
            // 
            this.tsmHour.Name = "tsmHour";
            this.tsmHour.Size = new System.Drawing.Size(112, 22);
            this.tsmHour.Text = "1小时";
            this.tsmHour.Click += new System.EventHandler(this.tsmHour_Click);
            // 
            // tsmDay
            // 
            this.tsmDay.Name = "tsmDay";
            this.tsmDay.Size = new System.Drawing.Size(112, 22);
            this.tsmDay.Text = "1天";
            this.tsmDay.Click += new System.EventHandler(this.tsmDay_Click);
            // 
            // tsmWeek
            // 
            this.tsmWeek.Name = "tsmWeek";
            this.tsmWeek.Size = new System.Drawing.Size(112, 22);
            this.tsmWeek.Text = "1周";
            this.tsmWeek.Click += new System.EventHandler(this.tsmWeek_Click);
            // 
            // tsmMonth
            // 
            this.tsmMonth.Name = "tsmMonth";
            this.tsmMonth.Size = new System.Drawing.Size(112, 22);
            this.tsmMonth.Text = "1月";
            this.tsmMonth.Click += new System.EventHandler(this.tsmMonth_Click);
            // 
            // tsmSeason
            // 
            this.tsmSeason.Name = "tsmSeason";
            this.tsmSeason.Size = new System.Drawing.Size(112, 22);
            this.tsmSeason.Text = "1季";
            this.tsmSeason.Click += new System.EventHandler(this.tsmSeason_Click);
            // 
            // tsmYear
            // 
            this.tsmYear.Name = "tsmYear";
            this.tsmYear.Size = new System.Drawing.Size(112, 22);
            this.tsmYear.Text = "1年";
            this.tsmYear.Click += new System.EventHandler(this.tsmYear_Click);
            // 
            // tsmsynchronize
            // 
            this.tsmsynchronize.Name = "tsmsynchronize";
            this.tsmsynchronize.Size = new System.Drawing.Size(112, 22);
            this.tsmsynchronize.Text = "不同步";
            this.tsmsynchronize.Click += new System.EventHandler(this.tsmsynchronize_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripComboBox2,
            this.toolStripComboBox3,
            this.toolStripComboBox1,
            this.toolStripComboBox4,
            this.toolStripComboBox5,
            this.toolStripComboBox6});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(182, 178);
            // 
            // toolStripComboBox2
            // 
            this.toolStripComboBox2.Name = "toolStripComboBox2";
            this.toolStripComboBox2.Size = new System.Drawing.Size(121, 25);
            this.toolStripComboBox2.Text = "名片";
            // 
            // toolStripComboBox3
            // 
            this.toolStripComboBox3.Name = "toolStripComboBox3";
            this.toolStripComboBox3.Size = new System.Drawing.Size(121, 25);
            this.toolStripComboBox3.Text = "名片";
            // 
            // toolStripComboBox1
            // 
            this.toolStripComboBox1.Name = "toolStripComboBox1";
            this.toolStripComboBox1.Size = new System.Drawing.Size(121, 25);
            this.toolStripComboBox1.Text = "群组";
            // 
            // toolStripComboBox4
            // 
            this.toolStripComboBox4.Name = "toolStripComboBox4";
            this.toolStripComboBox4.Size = new System.Drawing.Size(121, 25);
            this.toolStripComboBox4.Text = "手机号搜索";
            // 
            // toolStripComboBox5
            // 
            this.toolStripComboBox5.Name = "toolStripComboBox5";
            this.toolStripComboBox5.Size = new System.Drawing.Size(121, 25);
            this.toolStripComboBox5.Text = "昵称搜索";
            // 
            // toolStripComboBox6
            // 
            this.toolStripComboBox6.Name = "toolStripComboBox6";
            this.toolStripComboBox6.Size = new System.Drawing.Size(121, 25);
            this.toolStripComboBox6.Text = "其他";
            // 
            // FrmSet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(550, 544);
            this.Controls.Add(this.tabSet);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "FrmSet";
            this.ShowBorder = false;
            this.ShowDrawIcon = false;
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "设置";
            this.TitleColor = System.Drawing.Color.Gray;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FrmSet_FormClosed);
            this.Load += new System.EventHandler(this.FrmSet_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FrmSet_Paint);
            this.tabSet.ResumeLayout(false);
            this.tpAccountSet.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picHead)).EndInit();
            this.tpGeneralSet.ResumeLayout(false);
            this.tpPrivacySet.ResumeLayout(false);
            this.tpPrivacySet.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picallowtoway)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picnewstime)).EndInit();
            this.tpChangePwd.ResumeLayout(false);
            this.tpChangePwd.PerformLayout();
            this.tpAboutUs.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.cmsOverdueDate.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private CCWin.SkinControl.SkinTabControl tabSet;
        private CCWin.SkinControl.SkinTabPage tpGeneralSet;
        private CCWin.SkinControl.SkinTabPage tpPrivacySet;
        private CCWin.SkinControl.SkinTabPage tpChangePwd;
        private CCWin.SkinControl.SkinTabPage tpAboutUs;
        private System.Windows.Forms.Button btnDeleteChat;
        private System.Windows.Forms.Button btnDeleteCache;
        private System.Windows.Forms.PictureBox picLogo;
        private CCWin.SkinControl.SkinTabPage tpAccountSet;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Label lblEdition;
        private CCWin.SkinControl.SkinLine skinLine1;
        private CCWin.SkinControl.SkinLine skinLine2;
        private CCWin.SkinControl.SkinLine skinLine4;
        private CCWin.SkinControl.SkinLine skinLine3;
        private CCWin.SkinControl.SkinLine skinLine5;
        private System.Windows.Forms.Label lblCompanyName;
        private System.Windows.Forms.Label lblCopyright;
        private System.Windows.Forms.TextBox txtConfirmPwd;
        private System.Windows.Forms.Label lblVerifyPwd;
        private System.Windows.Forms.TextBox txtNewPwd;
        private System.Windows.Forms.TextBox txtOldPwd;
        private System.Windows.Forms.Label lblNewPwd;
        private System.Windows.Forms.Label lblOldPwd;
        public LollipopButton btnRevise;
        private System.Windows.Forms.Button btnEdit;
        private RoundPicBox picHead;
        private System.Windows.Forms.Label lblDevicesLogin;
        private System.Windows.Forms.Label lblKnowTyping;
        private System.Windows.Forms.Label lblFdValidation;
        private System.Windows.Forms.PictureBox picnewstime;
        private System.Windows.Forms.Label lblOverdueDate;
        private System.Windows.Forms.Label lblRoamingTime;
        private CCWin.SkinControl.SkinContextMenuStrip cmsOverdueDate;
        private System.Windows.Forms.ToolStripMenuItem tsmForever;
        private System.Windows.Forms.ToolStripMenuItem tsmHour;
        private System.Windows.Forms.ToolStripMenuItem tsmDay;
        private System.Windows.Forms.ToolStripMenuItem tsmWeek;
        private System.Windows.Forms.ToolStripMenuItem tsmMonth;
        private System.Windows.Forms.ToolStripMenuItem tsmSeason;
        private System.Windows.Forms.ToolStripMenuItem tsmYear;
        private Controls.CustomControls.USEToggle chkMultipleDevices;
        private Controls.CustomControls.USEToggle chkSendInput;
        private Controls.CustomControls.USEToggle chkPrivacy;
        private System.Windows.Forms.Label lblallowtoway;
        private System.Windows.Forms.Label lblAddWay;
        private System.Windows.Forms.Label lblAddMeName;
        private System.Windows.Forms.Label lblAddMePhone;
        private Controls.CustomControls.USEToggle chkAllowtoNickname;
        private Controls.CustomControls.USEToggle chkAllowtoPhone;
        private System.Windows.Forms.Button btninputAllroomxls;
        private System.Windows.Forms.Button btninputAllfriendxls;
        private System.Windows.Forms.Button btninputAllroomtxt;
        private System.Windows.Forms.Button btninputAllfriendtxt;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox2;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox3;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox1;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox4;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox5;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox6;
        private Controls.CustomControls.USEToggle chkAllowtoCustomer;
        private System.Windows.Forms.Label lblServicePattern;
        private System.Windows.Forms.PictureBox picallowtoway;
        private System.Windows.Forms.Button btnmuchSend;
        private Controls.CustomControls.USEToggle chkshowtips;
        private System.Windows.Forms.Label lblMsgPopup;
        private Controls.CustomControls.USEToggle chkNewsState;
        private System.Windows.Forms.Label lblMsgStatus;
        private System.Windows.Forms.ToolStripMenuItem tsmsynchronize;
        private System.Windows.Forms.Button btnImport;
        private WinFrmTalk.Controls.EQComboBox cmbLanguage;
    }
}