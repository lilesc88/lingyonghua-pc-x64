using System;
using System.Drawing;
using System.Windows.Forms;
using WinFrmTalk.Properties;

namespace WinFrmTalk.Controls
{
    /// <summary>
    /// 略缩图显示控件
    /// xuan
    /// 2019-12-26 15:07:49
    /// </summary>
    public class ImageViewxVideo : PictureBox
    {
        public int incosize = 42;

        // 等待符
        private PictureBox lodinview = null;
        // 当前状态
        private int state;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }



        public Image Image
        {
            get
            {
                return base.Image;
            }
            set
            {
                base.Image = value;
                state = 0;
            }
        }


        public void Error()
        {
            state = -1;
            this.Image = Resources.ic_black_rect;

            this.Refresh();
        }

        public void Loding()
        {
            state = 1;

            if (lodinview == null)
            {
                lodinview = new PictureBox();
                lodinview.Parent = this;
                lodinview.Size = new Size(incosize, incosize);

                lodinview.Location = new Point((Width - incosize) / 2, (Height - incosize) / 2);
                string path = Applicate.AppCurrentDirectory + "Resource\\load.gif";
                lodinview.Image = new Bitmap(path);
                lodinview.SizeMode = PictureBoxSizeMode.StretchImage;
            }

            this.Refresh();
        }

        protected override void OnBackgroundImageChanged(EventArgs e)
        {
            Console.WriteLine("不支持使用背景颜色");
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            var graphics = pe.Graphics;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            if (Image != null && state != 1)
            {
                // 居中缩放
                RectangleF centrect = CropCenterRect(Image, this.Width, this.Height);
                RectangleF location = new RectangleF(0, 0, Width, Height);

                // 绘制视频缩略图片
                graphics.DrawImage(Image, location, centrect, GraphicsUnit.Pixel);
            }

            // 绘制播放按钮
            DrawPlayIcon(graphics);
        }

        private void DrawPlayIcon(Graphics graphics)
        {
            if (state == 0)
            {
                if (lodinview != null)
                {
                    lodinview.Dispose();
                    lodinview = null;
                }

                Image image = Resources.jc_play_normal;

                Rectangle rectangle = new Rectangle(0, 0, image.Width, image.Height);
                Rectangle centrect = new Rectangle((Width - incosize) / 2, (Height - incosize) / 2, incosize, incosize);

                graphics.DrawImage(image, centrect, rectangle, GraphicsUnit.Pixel);
            }
            else if (state == -1)
            {
                if (lodinview != null)
                {
                    lodinview.Dispose();
                    lodinview = null;
                }

                //Image image = Resources.jc_play_error;

                //Rectangle rectangle = new Rectangle(0, 0, image.Width, image.Height);
                //Rectangle centrect = new Rectangle((Width - incosize) / 2, (Height - incosize) / 2, incosize, incosize);

                //graphics.DrawImage(image, centrect, rectangle, GraphicsUnit.Pixel);
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
    }
}
