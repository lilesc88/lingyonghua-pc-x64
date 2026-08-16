namespace WinFrmTalk
{
    partial class LeftLayout
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }


        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnRecent = new WinFrmTalk.Controls.ImageViewx();
            this.btnContacts = new WinFrmTalk.Controls.ImageViewx();
            this.btnGroup = new System.Windows.Forms.PictureBox();
            this.btnColleague = new System.Windows.Forms.PictureBox();
            this.btnCollect = new System.Windows.Forms.PictureBox();
            this.btnTags = new System.Windows.Forms.PictureBox();
            this.btnSettings = new LollipopFlatButton();
            this.pic_myIcon = new WinFrmTalk.RoundPicBox();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnRecent)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnContacts)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnGroup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnColleague)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCollect)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnTags)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic_myIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.flowLayoutPanel1.Controls.Add(this.btnRecent);
            this.flowLayoutPanel1.Controls.Add(this.btnContacts);
            this.flowLayoutPanel1.Controls.Add(this.btnGroup);
            this.flowLayoutPanel1.Controls.Add(this.btnColleague);
            this.flowLayoutPanel1.Controls.Add(this.btnCollect);
            this.flowLayoutPanel1.Controls.Add(this.btnTags);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 75);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(59, 507);
            this.flowLayoutPanel1.TabIndex = 14;
            // 
            // btnRecent
            // 
            this.btnRecent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRecent.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRecent.Image = global::WinFrmTalk.Properties.Resources.ic_nav_chat_press;
            this.btnRecent.Location = new System.Drawing.Point(0, 0);
            this.btnRecent.Margin = new System.Windows.Forms.Padding(0);
            this.btnRecent.Name = "btnRecent";
            this.btnRecent.Size = new System.Drawing.Size(60, 55);
            this.btnRecent.TabIndex = 11;
            this.btnRecent.TabStop = false;
            this.btnRecent.UnreadCount = 0;
            this.btnRecent.UnreadMargin = 8;
            this.btnRecent.UnreadSize = 20;
            this.btnRecent.Click += new System.EventHandler(this.btnRecent_Click);
            this.btnRecent.DoubleClick += new System.EventHandler(this.OnDoubleClickRecent);
            // 
            // btnContacts
            // 
            this.btnContacts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnContacts.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnContacts.Image = global::WinFrmTalk.Properties.Resources.ic_nav_contact;
            this.btnContacts.Location = new System.Drawing.Point(0, 55);
            this.btnContacts.Margin = new System.Windows.Forms.Padding(0);
            this.btnContacts.Name = "btnContacts";
            this.btnContacts.Size = new System.Drawing.Size(60, 55);
            this.btnContacts.TabIndex = 11;
            this.btnContacts.TabStop = false;
            this.btnContacts.UnreadCount = 0;
            this.btnContacts.UnreadMargin = 8;
            this.btnContacts.UnreadSize = 20;
            this.btnContacts.Click += new System.EventHandler(this.btnContacts_Click);
            // 
            // btnGroup
            // 
            this.btnGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGroup.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGroup.Image = global::WinFrmTalk.Properties.Resources.ic_nav_group;
            this.btnGroup.Location = new System.Drawing.Point(0, 110);
            this.btnGroup.Margin = new System.Windows.Forms.Padding(0);
            this.btnGroup.Name = "btnGroup";
            this.btnGroup.Size = new System.Drawing.Size(60, 55);
            this.btnGroup.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.btnGroup.TabIndex = 11;
            this.btnGroup.TabStop = false;
            this.btnGroup.Click += new System.EventHandler(this.btnGroup_Click);
            // 
            // btnColleague
            // 
            this.btnColleague.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnColleague.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnColleague.Image = global::WinFrmTalk.Properties.Resources.ic_nav_colleagues;
            this.btnColleague.Location = new System.Drawing.Point(0, 165);
            this.btnColleague.Margin = new System.Windows.Forms.Padding(0);
            this.btnColleague.Name = "btnColleague";
            this.btnColleague.Size = new System.Drawing.Size(60, 55);
            this.btnColleague.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.btnColleague.TabIndex = 12;
            this.btnColleague.TabStop = false;
            this.btnColleague.Click += new System.EventHandler(this.BtnCon_Click);
            // 
            // btnCollect
            // 
            this.btnCollect.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCollect.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCollect.Image = global::WinFrmTalk.Properties.Resources.ic_nav_collect;
            this.btnCollect.Location = new System.Drawing.Point(0, 220);
            this.btnCollect.Margin = new System.Windows.Forms.Padding(0);
            this.btnCollect.Name = "btnCollect";
            this.btnCollect.Size = new System.Drawing.Size(60, 55);
            this.btnCollect.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.btnCollect.TabIndex = 13;
            this.btnCollect.TabStop = false;
            this.btnCollect.Click += new System.EventHandler(this.BtnCollect_Click);
            // 
            // btnTags
            // 
            this.btnTags.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTags.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTags.Image = global::WinFrmTalk.Properties.Resources.ic_nav_tag;
            this.btnTags.Location = new System.Drawing.Point(0, 275);
            this.btnTags.Margin = new System.Windows.Forms.Padding(0);
            this.btnTags.Name = "btnTags";
            this.btnTags.Size = new System.Drawing.Size(60, 55);
            this.btnTags.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.btnTags.TabIndex = 11;
            this.btnTags.TabStop = false;
            this.btnTags.Click += new System.EventHandler(this.BtnTags_Click);
            // 
            // btnSettings
            // 
            this.btnSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSettings.BackColor = System.Drawing.Color.Transparent;
            this.btnSettings.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSettings.FontColor = "#508ef5";
            this.btnSettings.Location = new System.Drawing.Point(0, 598);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(60, 62);
            this.btnSettings.TabIndex = 9;
            this.btnSettings.Text = "";
            this.btnSettings.Click += new System.EventHandler(this.Settings_Click);
            // 
            // pic_myIcon
            // 
            this.pic_myIcon.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pic_myIcon.isDrawRound = true;
            this.pic_myIcon.Location = new System.Drawing.Point(9, 13);
            this.pic_myIcon.Margin = new System.Windows.Forms.Padding(0);
            this.pic_myIcon.Name = "pic_myIcon";
            this.pic_myIcon.Size = new System.Drawing.Size(42, 42);
            this.pic_myIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pic_myIcon.TabIndex = 10;
            this.pic_myIcon.TabStop = false;
            this.pic_myIcon.Click += new System.EventHandler(this.pic_myIcon_Click);
            this.pic_myIcon.Paint += new System.Windows.Forms.PaintEventHandler(this.pic_myIcon_Paint);
            // 
            // LeftLayout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(42)))), ((int)(((byte)(44)))));
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.pic_myIcon);
            this.Controls.Add(this.btnSettings);
            this.Name = "LeftLayout";
            this.Size = new System.Drawing.Size(60, 660);
            this.Load += new System.EventHandler(this.LeftLayout_Load);
            this.flowLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.btnRecent)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnContacts)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnGroup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnColleague)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCollect)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnTags)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic_myIcon)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private LollipopFlatButton btnSettings;
        private RoundPicBox pic_myIcon;
        private WinFrmTalk.Controls.ImageViewx btnRecent;
        private WinFrmTalk.Controls.ImageViewx btnContacts;
        private System.Windows.Forms.PictureBox btnGroup;
        private System.Windows.Forms.PictureBox btnColleague;
        private System.Windows.Forms.PictureBox btnCollect;
        private System.Windows.Forms.PictureBox btnTags;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
