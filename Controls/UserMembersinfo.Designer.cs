namespace WinFrmTalk.Controls
{
    partial class UserMembersinfo
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
            this.pichend = new System.Windows.Forms.PictureBox();
            this.lblNameCard = new WinFrmTalk.Controls.LabelEx();
            this.lblLsttalktime = new WinFrmTalk.Controls.LabelEx();
            this.lblRole = new WinFrmTalk.Controls.LabelEx();
            this.lblNickname = new WinFrmTalk.Controls.LabelEx();
            ((System.ComponentModel.ISupportInitialize)(this.pichend)).BeginInit();
            this.SuspendLayout();
            // 
            // pichend
            // 
            this.pichend.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pichend.Location = new System.Drawing.Point(5, 5);
            this.pichend.Name = "pichend";
            this.pichend.Size = new System.Drawing.Size(30, 30);
            this.pichend.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pichend.TabIndex = 0;
            this.pichend.TabStop = false;
            // 
            // lblNameCard
            // 
            this.lblNameCard.AutoEllipsis = true;
            this.lblNameCard.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblNameCard.IsStroke = false;
            this.lblNameCard.Location = new System.Drawing.Point(376, 11);
            this.lblNameCard.Name = "lblNameCard";
            this.lblNameCard.Size = new System.Drawing.Size(104, 19);
            this.lblNameCard.TabIndex = 4;
            this.lblNameCard.Text = "labelEx2";
            this.lblNameCard.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblLsttalktime
            // 
            this.lblLsttalktime.AutoEllipsis = true;
            this.lblLsttalktime.AutoSize = true;
            this.lblLsttalktime.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblLsttalktime.IsStroke = false;
            this.lblLsttalktime.Location = new System.Drawing.Point(266, 12);
            this.lblLsttalktime.Name = "lblLsttalktime";
            this.lblLsttalktime.Size = new System.Drawing.Size(74, 17);
            this.lblLsttalktime.TabIndex = 3;
            this.lblLsttalktime.Text = "2020/04/02";
            this.lblLsttalktime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblRole
            // 
            this.lblRole.AutoEllipsis = true;
            this.lblRole.AutoSize = true;
            this.lblRole.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblRole.IsStroke = false;
            this.lblRole.Location = new System.Drawing.Point(188, 12);
            this.lblRole.Name = "lblRole";
            this.lblRole.Size = new System.Drawing.Size(44, 17);
            this.lblRole.TabIndex = 2;
            this.lblRole.Text = "管理员";
            this.lblRole.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblNickname
            // 
            this.lblNickname.AutoEllipsis = true;
            this.lblNickname.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblNickname.IsStroke = false;
            this.lblNickname.Location = new System.Drawing.Point(46, 11);
            this.lblNickname.Name = "lblNickname";
            this.lblNickname.Size = new System.Drawing.Size(106, 19);
            this.lblNickname.TabIndex = 1;
            this.lblNickname.Text = "labelEx1";
            this.lblNickname.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // UserMembersinfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblNameCard);
            this.Controls.Add(this.lblLsttalktime);
            this.Controls.Add(this.lblRole);
            this.Controls.Add(this.lblNickname);
            this.Controls.Add(this.pichend);
            this.Name = "UserMembersinfo";
            this.Size = new System.Drawing.Size(497, 40);
            this.MouseEnter += new System.EventHandler(this.FriendItem_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.FriendItem_MouseLeave);
            ((System.ComponentModel.ISupportInitialize)(this.pichend)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pichend;
        private LabelEx lblNickname;
        private LabelEx lblRole;
        private LabelEx lblLsttalktime;
        private LabelEx lblNameCard;
    }
}
