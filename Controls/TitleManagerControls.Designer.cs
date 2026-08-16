namespace WinFrmTalk.Controls
{
    partial class TitleManagerControls
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
            this.roomLink = new WinFrmTalk.Controls.RoomUrlListControls();
            this.roomNotice = new WinFrmTalk.Controls.Roomannounce();
            this.SuspendLayout();
            // 
            // roomLink
            // 
            this.roomLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.roomLink.BackColor = System.Drawing.Color.White;
            this.roomLink.Location = new System.Drawing.Point(0, 0);
            this.roomLink.Margin = new System.Windows.Forms.Padding(0);
            this.roomLink.Name = "roomLink";
            this.roomLink.Size = new System.Drawing.Size(611, 33);
            this.roomLink.TabIndex = 4;
            // 
            // roomNotice
            // 
            this.roomNotice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.roomNotice.BackColor = System.Drawing.Color.White;
            this.roomNotice.Content = null;
            this.roomNotice.Location = new System.Drawing.Point(0, 68);
            this.roomNotice.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.roomNotice.Name = "roomNotice";
            this.roomNotice.NoticeId = null;
            this.roomNotice.Size = new System.Drawing.Size(611, 33);
            this.roomNotice.TabIndex = 5;
            // 
            // TitleManagerControls
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.roomLink);
            this.Controls.Add(this.roomNotice);
            this.Name = "TitleManagerControls";
            this.Size = new System.Drawing.Size(611, 102);
            this.ResumeLayout(false);

        }

        #endregion

        private RoomUrlListControls roomLink;
        private Roomannounce roomNotice;
    }
}
