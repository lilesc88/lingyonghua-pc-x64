using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WinFrmTalk.Controls
{
    /// <summary>
    /// 自动居中缩放图片控件
    /// xuan
    /// 2019-12-26 15:07:49
    /// </summary>
    public class ImageViewx : PictureBox
    {
        // 是否是将图片切成圆形
        private bool isRound = false;

        // 未读数量
        private int mUnread = 0;
        private int unread_size = 20;
        private int unread_hegith = 18;
        private int unread_margin = 0;

        public int UnreadSize
        {
            get { return unread_size; }
            set
            {
                unread_size = value;
                this.Refresh();
            }
        }

        public int UnreadCount
        {
            get { return mUnread; }
            set
            {
                mUnread = value;
                this.Refresh();
            }
        }

        public int UnreadMargin
        {
            get { return unread_margin; }
            set
            {
                unread_margin = value;
            }
        }



        // 是否把图片切成圆形
        public void ChangeImageRound(bool isRound)
        {
            this.isRound = isRound;
            this.Refresh();
        }

        private SolidBrush mTextBrush;
        private Font mTextFont;

        public ImageViewx()
        {
            mTextBrush = new SolidBrush(Color.FromArgb(250, Color.White));
            mTextFont = new Font("微软雅黑", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
        }


        protected override void OnBackgroundImageChanged(EventArgs e)
        {
            Console.WriteLine("不支持使用背景颜色");
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (Image != null)
            {
                var graphics = pe.Graphics;

                if (isRound)
                {
                    // 图像切圆
                    Image round = CutEllipse(Image, this.Width, this.Height);

                    Rectangle destRect = new Rectangle(2, 2, this.Width - 1, this.Height - 1);
                    Rectangle srcRect = new Rectangle(0, 0, round.Width, round.Height);
                    // 绘制圆形头像
                    graphics.DrawImage(round, destRect, srcRect, GraphicsUnit.Pixel);
                }
                else
                {
                    // 居中缩放
                    RectangleF srcRect = CropCenterRect(Image, this.Width, this.Height);
                    // 绘制大小和位置
                    RectangleF descRect = new RectangleF(Padding.Left, Padding.Top, this.Width - (Padding.Left + Padding.Right), this.Height - (Padding.Top + Padding.Bottom));
                    // 绘制图片
                    graphics.DrawImage(Image, descRect, srcRect, GraphicsUnit.Pixel);
                }


                if (mUnread > 0)
                {
                    DrawRedNum(graphics);
                }
            }
        }



        // 居中缩放
        private RectangleF CropCenterRect(Image image, float width, float height)
        {
            float img_width = image.Width;
            float img_height = image.Height;

            float new_width = 0;
            float new_height = 0;

            //获取合适的比例进行裁剪
            new_width = img_width;
            new_height = img_width / width * height;

            if (new_height > img_height)
            {
                new_width = img_height / new_height * new_width;
                new_height = img_height;
            }

            return new RectangleF((image.Width - new_width) / 2, (image.Height - new_height) / 2, new_width, new_height);
        }


        // 图片居中内切圆
        private Image CutEllipse(Image image, int width, int height)
        {
            int new_size = Math.Min(image.Width, image.Height);

            int centx = (image.Width - new_size) / 2;
            int centy = (image.Height - new_size) / 2;

            Bitmap bm = new Bitmap(new_size, new_size);

            using (Graphics g = Graphics.FromImage(bm))
            {
                GraphicsPath gpath = new GraphicsPath();

                gpath.AddEllipse(0, 0, new_size, new_size);

                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.CompositingQuality = CompositingQuality.HighQuality;

                g.SetClip(gpath);

                var srcRect = new Rectangle(centx, centy, new_size, new_size);
                var descRect = new Rectangle(0, 0, new_size, new_size);
                g.DrawImage(image, descRect, srcRect, GraphicsUnit.Pixel);
            }
            return bm;
        }



        /// <summary>
        /// 绘制未读数量
        /// </summary>
        /// <param name="graphics"></param>
        private void DrawRedNum(Graphics graphics)
        {
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            // 计算出要绘制的区域
            RectangleF rect = CalcRectangle(graphics);
            // 绘制红点
            DrawUnreadPoint(graphics, rect);
            // 绘制文字
            DrawUnreadText(graphics, rect);

            //// 绘制红点
            //graphics.FillEllipse(mRedPen, this.Width - unread_size - 1 - UnreadMargin, UnreadMargin, unread_size, unread_size);

            //// 绘制文字f
            //float textx = mUnread.ToString().Length * -3.5f + 7.8f;
            //graphics.DrawString(mUnread.ToString(), mTextFont, mTextBrush, this.Width - unread_size + textx - UnreadMargin, 1.5f + UnreadMargin);
        }


        private RectangleF CalcRectangle(Graphics graphics)
        {
            SizeF size = graphics.MeasureString(ToUnreadText(), mTextFont);
            size.Width = size.Width + 6;

            float left = this.Width - (UnreadMargin + size.Width);
            if (size.Width < unread_hegith)
            {
                left = left - 5;
            }
            else
            {
                left = left + (size.Width / 10);
            }

            float width = size.Width < unread_hegith ? unread_hegith : size.Width;

            RectangleF rectangle = new RectangleF(left, UnreadMargin, width, unread_hegith);
            return rectangle;
        }

        private void DrawUnreadPoint(Graphics graphics, RectangleF rect)
        {
            if (rect.Width == rect.Height)
            {
                graphics.FillEllipse(Brushes.Red, rect);
                return;
            }

            // 画一个矩形 + 两个圆
            int ruals = unread_hegith >> 1;
            float receWidth = rect.Width - rect.Height;

            graphics.FillRectangle(Brushes.Red, rect.Left + ruals, rect.Top-1, receWidth, rect.Height);
            graphics.FillEllipse(Brushes.Red, rect.Left, rect.Top - 1, rect.Height, rect.Height);
            graphics.FillEllipse(Brushes.Red, rect.Left + receWidth , rect.Top - 1, rect.Height, rect.Height);
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
