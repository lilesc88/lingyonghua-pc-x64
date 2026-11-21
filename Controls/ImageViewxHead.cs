using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WinFrmTalk.Controls
{
    /// <summary>
    /// 圆形头像控件+未读红点数量
    /// xuan
    /// 2019-12-26 15:07:49
    /// </summary>
    /// 
    public class ImageViewxHead : PictureBox
    {
        private const int RED_SIZE = 21;

        private int _num = 0;

        // 是否自动切圆
        public int Red_Num
        {
            get
            {
                return _num;
            }
            set
            {
                _num = value;
                this.Refresh();
            }
        }


        private SolidBrush mRedPen;
        private SolidBrush mTextBrush;
        private Font mTextFont;


        public ImageViewxHead()
        {
            mRedPen = new SolidBrush(Color.Red);
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

                // 图像居中切圆
                Image round = CutEllipse(Image, this.Width, this.Height);

                Rectangle destRect = new Rectangle(2, 2, this.Width - 3, this.Height - 3);
                Rectangle srcRect = new Rectangle(0, 0, round.Width, round.Height);

                // 绘制圆形头像
                graphics.DrawImage(round, destRect, srcRect, GraphicsUnit.Pixel);

                // 绘制红点
                DrawRedNum(graphics);
            }
        }


        private void DrawRedNum(Graphics graphics)
        {
            if (Red_Num > 0)
            {
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                // 绘制红点
                graphics.FillEllipse(mRedPen, this.Width - RED_SIZE - 1, 0, RED_SIZE, RED_SIZE);

                // 绘制文字f
                float textx = Red_Num.ToString().Length * -3.5f + 7.8f;
                graphics.DrawString(Red_Num.ToString(), mTextFont, mTextBrush, this.Width - RED_SIZE + textx, 2.5f);
            }
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
    }
}
