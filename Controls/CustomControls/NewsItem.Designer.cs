using System.Windows.Forms;
using WinFrmTalk.View.Common;

namespace WinFrmTalk.Controls.CustomControls
{
    partial class NewsItem
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
            this.lab_time = new System.Windows.Forms.Label();
            this.lab_name = new System.Windows.Forms.Label();
            this.pic_head = new System.Windows.Forms.PictureBox();
            this.mRedPoint = new WinFrmTalk.Controls.RedPointControl();
            this.lab_content = new WinFrmTalk.View.Common.RecentLable();
            ((System.ComponentModel.ISupportInitialize)(this.pic_head)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mRedPoint)).BeginInit();
            this.SuspendLayout();
            // 
            // lab_time
            // 
            this.lab_time.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lab_time.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lab_time.ForeColor = System.Drawing.Color.DimGray;
            this.lab_time.Location = new System.Drawing.Point(172, 12);
            this.lab_time.Name = "lab_time";
            this.lab_time.Size = new System.Drawing.Size(80, 18);
            this.lab_time.TabIndex = 6;
            this.lab_time.Text = "2019/11/11";
            this.lab_time.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lab_name
            // 
            this.lab_name.AutoEllipsis = true;
            this.lab_name.Font = new System.Drawing.Font("微软雅黑", 10.75F);
            this.lab_name.Location = new System.Drawing.Point(48, 10);
            this.lab_name.Name = "lab_name";
            this.lab_name.Size = new System.Drawing.Size(112, 20);
            this.lab_name.TabIndex = 7;
            this.lab_name.Text = "Name";
            this.lab_name.UseMnemonic = false;
            // 
            // pic_head
            // 
            this.pic_head.Location = new System.Drawing.Point(6, 12);
            this.pic_head.Name = "pic_head";
            this.pic_head.Size = new System.Drawing.Size(35, 35);
            this.pic_head.TabIndex = 11;
            this.pic_head.TabStop = false;
            // 
            // mRedPoint
            // 
            this.mRedPoint.Location = new System.Drawing.Point(197, 34);
            this.mRedPoint.MsgNoPush = false;
            this.mRedPoint.Name = "mRedPoint";
            this.mRedPoint.Size = new System.Drawing.Size(50, 16);
            this.mRedPoint.TabIndex = 12;
            this.mRedPoint.TabStop = false;
            this.mRedPoint.UnreadCount = 0;
            // 
            // lab_content
            // 
            this.lab_content.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lab_content.AtMe = 0;
            this.lab_content.AutoEllipsis = true;
            this.lab_content.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lab_content.ForeColor = System.Drawing.Color.DimGray;
            this.lab_content.IsDraft = false;
            this.lab_content.Location = new System.Drawing.Point(51, 33);
            this.lab_content.Name = "lab_content";
            this.lab_content.Size = new System.Drawing.Size(155, 18);
            this.lab_content.TabIndex = 8;
            this.lab_content.UseMnemonic = false;
            // 
            // NewsItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.Controls.Add(this.mRedPoint);
            this.Controls.Add(this.pic_head);
            this.Controls.Add(this.lab_time);
            this.Controls.Add(this.lab_content);
            this.Controls.Add(this.lab_name);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "NewsItem";
            this.Size = new System.Drawing.Size(250, 60);
            this.Load += new System.EventHandler(this.NewsItem_Load);
            this.MouseEnter += new System.EventHandler(this.NewsItem_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.NewsItem_MouseLeave);
            ((System.ComponentModel.ISupportInitialize)(this.pic_head)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mRedPoint)).EndInit();
            this.ResumeLayout(false);

        }


        #endregion
        private System.Windows.Forms.Label lab_time;
        private System.Windows.Forms.Label lab_name;
        private WinFrmTalk.View.Common.RecentLable lab_content;
        public PictureBox pic_head;
        private RedPointControl mRedPoint;
    }
}
