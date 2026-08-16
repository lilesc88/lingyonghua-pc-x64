using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WinFrmTalk.Controls
{
    /// <summary>
    /// 对讲机正在说话头像
    /// xuan
    /// 2019-12-26 15:07:49
    /// </summary>
    public class ImageViewxSpeak : PictureBox
    {
        // 是否是将图片切成圆形
        private int Raduis = 8;


        private Pen mRedPen;

        public ImageViewxSpeak()
        {
            //mRedPen = new SolidBrush(Color.FromArgb(255, 224, 224));
            mRedPen = new Pen(Color.FromArgb(255, 224, 224));
            mRedPen.Width = Raduis;
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

                // 绘制群管理边框
                DrawImageBorder(graphics);

            }
        }

        private void DrawImageBorder(Graphics graphics)
        {
            graphics.DrawEllipse(mRedPen, Raduis/2, Raduis / 2, this.Width - Raduis, this.Height - Raduis);
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
