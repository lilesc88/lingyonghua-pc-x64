using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace WinFrmTalk
{
    public class TableLayoutPanelEx : TableLayoutPanel
    {
        private const int WM_ERASEBKGND = 0x0014;

        /// <summary>
        /// 聊天背景图片
        /// </summary>
        private Image _chatBgImage;
        public Image ChatBackgroundImage
        {
            get { return _chatBgImage; }
            set
            {
                _chatBgImage = value;
                this.Invalidate();
            }
        }

        public TableLayoutPanelEx()
        {
            // 打开控件的双缓冲
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            e.Control.MouseWheel += (sender, ev) => { this.OnMouseWheel(ev); };
            base.OnControlAdded(e);
        }

        /// <summary>
        /// 重写背景绘制：跳过默认的BackColor填充，保留WM_ERASEBKGND中画的图片
        /// </summary>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (_chatBgImage != null)
            {
                // 不调用base，避免BackColor覆盖背景图
                // WM_ERASEBKGND已经画了背景图，这里不需要再画
            }
            else
            {
                base.OnPaintBackground(e);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_ERASEBKGND && _chatBgImage != null)
            {
                // 直接在设备上下文上绘制背景图
                // 使用FromHdcInternal可以在双缓冲的DC上绘制，与OnPaint共享同一个缓冲区
                using (Graphics g = Graphics.FromHdcInternal(m.WParam))
                {
                    using (TextureBrush brush = new TextureBrush(_chatBgImage, WrapMode.Tile))
                    {
                        g.FillRectangle(brush, this.ClientRectangle);
                    }
                }
                m.Result = (IntPtr)1; // 告诉系统：我们已经处理了背景擦除
                return; // 不调用base.WndProc，完全跳过默认处理
            }
            base.WndProc(ref m);
        }
    }
}
