namespace WinFrmTalk.Controls.CustomControls
{
    partial class ShowMsgPanel_Tool
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShowMsgPanel_Tool));
            this.lbl_qreply = new System.Windows.Forms.Label();
            this.lblSoundRecord = new System.Windows.Forms.Label();
            this.lblPhotography = new System.Windows.Forms.Label();
            this.lblCamera = new System.Windows.Forms.Label();
            this.lblLocation = new System.Windows.Forms.Label();
            this.lblAudio = new System.Windows.Forms.Label();
            this.lblVideo = new System.Windows.Forms.Label();
            this.lblSLC = new System.Windows.Forms.Label();
            this.lblHistory = new System.Windows.Forms.Label();
            this.lblScreen = new System.Windows.Forms.Label();
            this.lblSendFile = new System.Windows.Forms.Label();
            this.lblExpression = new System.Windows.Forms.Label();
            this.lab_splitTool = new System.Windows.Forms.Label();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.lblRoomLive = new System.Windows.Forms.Label();
            this.lblTalk = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lbl_qreply
            // 
            this.lbl_qreply.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lbl_qreply.Dock = System.Windows.Forms.DockStyle.Left;
            this.lbl_qreply.Image = global::WinFrmTalk.Properties.Resources.ic_qreply;
            this.lbl_qreply.Location = new System.Drawing.Point(282, 0);
            this.lbl_qreply.Name = "lbl_qreply";
            this.lbl_qreply.Size = new System.Drawing.Size(34, 34);
            this.lbl_qreply.TabIndex = 26;
            this.toolTip.SetToolTip(this.lbl_qreply, "快捷回复");
            this.lbl_qreply.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Lbl_qreply_MouseClick);
            // 
            // lblSoundRecord
            // 
            this.lblSoundRecord.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblSoundRecord.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblSoundRecord.Image = ((System.Drawing.Image)(resources.GetObject("lblSoundRecord.Image")));
            this.lblSoundRecord.Location = new System.Drawing.Point(248, 0);
            this.lblSoundRecord.Name = "lblSoundRecord";
            this.lblSoundRecord.Size = new System.Drawing.Size(34, 34);
            this.lblSoundRecord.TabIndex = 25;
            this.toolTip.SetToolTip(this.lblSoundRecord, "录音");
            this.lblSoundRecord.MouseClick += new System.Windows.Forms.MouseEventHandler(this.LblSoundRecord_MouseClick);
            // 
            // lblPhotography
            // 
            this.lblPhotography.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblPhotography.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblPhotography.Image = ((System.Drawing.Image)(resources.GetObject("lblPhotography.Image")));
            this.lblPhotography.Location = new System.Drawing.Point(214, 0);
            this.lblPhotography.Name = "lblPhotography";
            this.lblPhotography.Size = new System.Drawing.Size(34, 34);
            this.lblPhotography.TabIndex = 24;
            this.toolTip.SetToolTip(this.lblPhotography, "录像");
            this.lblPhotography.MouseClick += new System.Windows.Forms.MouseEventHandler(this.LblPhotography_MouseClick);
            // 
            // lblCamera
            // 
            this.lblCamera.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblCamera.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblCamera.Image = ((System.Drawing.Image)(resources.GetObject("lblCamera.Image")));
            this.lblCamera.Location = new System.Drawing.Point(180, 0);
            this.lblCamera.Name = "lblCamera";
            this.lblCamera.Size = new System.Drawing.Size(34, 34);
            this.lblCamera.TabIndex = 23;
            this.toolTip.SetToolTip(this.lblCamera, "拍照");
            this.lblCamera.MouseClick += new System.Windows.Forms.MouseEventHandler(this.LblCamera_MouseClick);
            // 
            // lblLocation
            // 
            this.lblLocation.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblLocation.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblLocation.Image = ((System.Drawing.Image)(resources.GetObject("lblLocation.Image")));
            this.lblLocation.Location = new System.Drawing.Point(146, 0);
            this.lblLocation.Name = "lblLocation";
            this.lblLocation.Size = new System.Drawing.Size(34, 34);
            this.lblLocation.TabIndex = 22;
            this.toolTip.SetToolTip(this.lblLocation, "位置");
            this.lblLocation.MouseClick += new System.Windows.Forms.MouseEventHandler(this.LblLocation_MouseClick);
            // 
            // lblAudio
            // 
            this.lblAudio.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblAudio.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblAudio.Image = ((System.Drawing.Image)(resources.GetObject("lblAudio.Image")));
            this.lblAudio.Location = new System.Drawing.Point(589, 0);
            this.lblAudio.Name = "lblAudio";
            this.lblAudio.Size = new System.Drawing.Size(34, 34);
            this.lblAudio.TabIndex = 21;
            this.toolTip.SetToolTip(this.lblAudio, "语音通话");
            this.lblAudio.MouseClick += new System.Windows.Forms.MouseEventHandler(this.LblAudio_MouseClick);
            // 
            // lblVideo
            // 
            this.lblVideo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblVideo.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblVideo.Image = ((System.Drawing.Image)(resources.GetObject("lblVideo.Image")));
            this.lblVideo.Location = new System.Drawing.Point(623, 0);
            this.lblVideo.Name = "lblVideo";
            this.lblVideo.Size = new System.Drawing.Size(34, 34);
            this.lblVideo.TabIndex = 20;
            this.toolTip.SetToolTip(this.lblVideo, "视频通话");
            this.lblVideo.MouseClick += new System.Windows.Forms.MouseEventHandler(this.LblVideo_MouseClick);
            // 
            // lblSLC
            // 
            this.lblSLC.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblSLC.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblSLC.Image = ((System.Drawing.Image)(resources.GetObject("lblSLC.Image")));
            this.lblSLC.Location = new System.Drawing.Point(691, 0);
            this.lblSLC.Name = "lblSLC";
            this.lblSLC.Size = new System.Drawing.Size(34, 34);
            this.lblSLC.TabIndex = 27;
            this.lblSLC.Visible = false;
            this.lblSLC.MouseClick += new System.Windows.Forms.MouseEventHandler(this.LblSLC_MouseClick);
            // 
            // lblHistory
            // 
            this.lblHistory.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblHistory.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblHistory.Image = ((System.Drawing.Image)(resources.GetObject("lblHistory.Image")));
            this.lblHistory.Location = new System.Drawing.Point(112, 0);
            this.lblHistory.Name = "lblHistory";
            this.lblHistory.Size = new System.Drawing.Size(34, 34);
            this.lblHistory.TabIndex = 19;
            this.toolTip.SetToolTip(this.lblHistory, "聊天记录");
            this.lblHistory.MouseClick += new System.Windows.Forms.MouseEventHandler(this.LblHistory_MoseClick);
            // 
            // lblScreen
            // 
            this.lblScreen.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblScreen.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblScreen.Image = ((System.Drawing.Image)(resources.GetObject("lblScreen.Image")));
            this.lblScreen.Location = new System.Drawing.Point(78, 0);
            this.lblScreen.Name = "lblScreen";
            this.lblScreen.Size = new System.Drawing.Size(34, 34);
            this.lblScreen.TabIndex = 18;
            this.toolTip.SetToolTip(this.lblScreen, "截图");
            this.lblScreen.MouseClick += new System.Windows.Forms.MouseEventHandler(this.LblScreen_MouseClick);
            // 
            // lblSendFile
            // 
            this.lblSendFile.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblSendFile.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblSendFile.Image = ((System.Drawing.Image)(resources.GetObject("lblSendFile.Image")));
            this.lblSendFile.Location = new System.Drawing.Point(44, 0);
            this.lblSendFile.Name = "lblSendFile";
            this.lblSendFile.Size = new System.Drawing.Size(34, 34);
            this.lblSendFile.TabIndex = 17;
            this.toolTip.SetToolTip(this.lblSendFile, "发送文件");
            this.lblSendFile.MouseClick += new System.Windows.Forms.MouseEventHandler(this.LblSendFile_MouseClick);
            // 
            // lblExpression
            // 
            this.lblExpression.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblExpression.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblExpression.Image = global::WinFrmTalk.Properties.Resources.ExpressionNormal;
            this.lblExpression.Location = new System.Drawing.Point(0, 0);
            this.lblExpression.Name = "lblExpression";
            this.lblExpression.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblExpression.Size = new System.Drawing.Size(44, 34);
            this.lblExpression.TabIndex = 16;
            this.toolTip.SetToolTip(this.lblExpression, "表情");
            this.lblExpression.MouseClick += new System.Windows.Forms.MouseEventHandler(this.LblExpression_MouseClick);
            // 
            // lab_splitTool
            // 
            this.lab_splitTool.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lab_splitTool.BackColor = System.Drawing.Color.Gainsboro;
            this.lab_splitTool.Location = new System.Drawing.Point(0, 0);
            this.lab_splitTool.Name = "lab_splitTool";
            this.lab_splitTool.Size = new System.Drawing.Size(725, 1);
            this.lab_splitTool.TabIndex = 28;
            // 
            // lblRoomLive
            // 
            this.lblRoomLive.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblRoomLive.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblRoomLive.Image = ((System.Drawing.Image)(resources.GetObject("lblRoomLive.Image")));
            this.lblRoomLive.Location = new System.Drawing.Point(316, 0);
            this.lblRoomLive.Name = "lblRoomLive";
            this.lblRoomLive.Size = new System.Drawing.Size(34, 34);
            this.lblRoomLive.TabIndex = 29;
            this.toolTip.SetToolTip(this.lblRoomLive, "直播");
            this.lblRoomLive.Visible = false;
            // 
            // lblTalk
            // 
            this.lblTalk.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblTalk.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblTalk.Location = new System.Drawing.Point(657, 0);
            this.lblTalk.Name = "lblTalk";
            this.lblTalk.Size = new System.Drawing.Size(34, 34);
            this.lblTalk.TabIndex = 30;
            this.lblTalk.Visible = false;
            // 
            // ShowMsgPanel_Tool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.lab_splitTool);
            this.Controls.Add(this.lbl_qreply);
            this.Controls.Add(this.lblRoomLive);
            this.Controls.Add(this.lblPhotography);
            this.Controls.Add(this.lblCamera);
            this.Controls.Add(this.lblSoundRecord);
            this.Controls.Add(this.lblLocation);
            this.Controls.Add(this.lblHistory);
            this.Controls.Add(this.lblScreen);
            this.Controls.Add(this.lblSendFile);
            this.Controls.Add(this.lblExpression);
            this.Controls.Add(this.lblAudio);
            this.Controls.Add(this.lblVideo);
            this.Controls.Add(this.lblTalk);
            this.Controls.Add(this.lblSLC);
            this.Name = "ShowMsgPanel_Tool";
            this.Size = new System.Drawing.Size(725, 34);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Label lbl_qreply;
        private System.Windows.Forms.Label lblSoundRecord;
        private System.Windows.Forms.Label lblPhotography;
        private System.Windows.Forms.Label lblCamera;
        private System.Windows.Forms.Label lblLocation;
        public System.Windows.Forms.Label lblAudio;
        public System.Windows.Forms.Label lblVideo;
        public System.Windows.Forms.Label lblSLC;
        private System.Windows.Forms.Label lblHistory;
        private System.Windows.Forms.Label lblScreen;
        private System.Windows.Forms.Label lblSendFile;
        private System.Windows.Forms.Label lblExpression;
        private System.Windows.Forms.Label lab_splitTool;
        private System.Windows.Forms.ToolTip toolTip;
        public System.Windows.Forms.Label lblRoomLive;
        public System.Windows.Forms.Label lblTalk;
    }
}
