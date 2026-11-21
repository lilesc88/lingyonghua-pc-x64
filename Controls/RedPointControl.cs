using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using WinFrmTalk.Properties;

namespace WinFrmTalk.Controls
{
    public partial class RedPointControl : PictureBox
    {
        private int mUnread = 0;
        private Font mTextFont;
        private bool notTip = false;
        private Brush mRadBursh;

        public int UnreadCount
        {
            get { return mUnread; }
            set
            {
                mUnread = value;
                this.Refresh();
            }
        }

        public bool MsgNoPush
        {
            get { return notTip; }
            set
            {
                notTip = value;
                if (notTip)
                {
                    mRadBursh = Brushes.Gray;
                }
                else
                {
                    mRadBursh = Brushes.Red;
                }

                this.Refresh();
            }
        }

        public RedPointControl()
        {
            mTextFont = new Font("微软雅黑", 8.7f, FontStyle.Regular, GraphicsUnit.Point, 0);
            mRadBursh = Brushes.Red;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {


            float left = this.Width;

            if (UnreadCount > 0)
            {
                var graphics = pe.Graphics;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.CompositingQuality = CompositingQuality.HighQuality;

                // 计算出要绘制的区域
                RectangleF rect = CalcRectangle(graphics);
                left = rect.Left;
                // 绘制红点
                DrawUnreadPoint(graphics, rect);
                // 绘制文字
                DrawUnreadText(graphics, rect);
            }

            if (MsgNoPush)
            {
                var graphics = pe.Graphics;
                if (left == 0)
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                }

                var image = Resources.ic_msg_nopush;

                RectangleF srcRect = new RectangleF(left - this.Height, 0, this.Height, this.Height);
                RectangleF descRect = new RectangleF(2, 2, image.Width - 4, image.Height - 4);
                graphics.DrawImage(image, srcRect, descRect, GraphicsUnit.Pixel);
            }

        }

        private RectangleF CalcRectangle(Graphics graphics)
        {
            SizeF size = graphics.MeasureString(ToUnreadText(), mTextFont);
            size.Width = size.Width + 8;

            float left = size.Width < this.Height - 1 ? this.Width - this.Height - 1 : this.Width - size.Width;
            float width = size.Width < this.Height - 1 ? this.Height - 1 : size.Width;

            RectangleF rectangle = new RectangleF(left, 0, width, this.Height - 1);
            return rectangle;
        }

        private void DrawUnreadPoint(Graphics graphics, RectangleF rect)
        {
            if (rect.Width == rect.Height)
            {
                graphics.FillEllipse(mRadBursh, rect);
                return;
            }

            // 画一个矩形 + 两个圆
            int ruals = this.Height >> 1;
            float receWidth = rect.Width - rect.Height - 2;

            graphics.FillRectangle(mRadBursh, rect.Left + ruals, rect.Top, receWidth, rect.Height);
            graphics.FillEllipse(mRadBursh, rect.Left, rect.Top, rect.Height, rect.Height);
            graphics.FillEllipse(mRadBursh, rect.Left + receWidth + 1, rect.Top, rect.Height, rect.Height);
        }


        private void DrawUnreadText(Graphics graphics, RectangleF rect)
        {
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center; //居中
            format.LineAlignment = StringAlignment.Center; //右对齐

            string text = ToUnreadText();
            graphics.DrawString(text, mTextFont, Brushes.White, rect, format);
        }

        public string ToUnreadText()
        {
            if (UnreadCount <= 0)
            {
                return "";
            }

            if (UnreadCount < 1000)
            {
                return UnreadCount.ToString();
            }
            else
            {
                return string.Format("{0:F1}{1}", UnreadCount / 1000.0f, "K");
            }
        }
    }
}
