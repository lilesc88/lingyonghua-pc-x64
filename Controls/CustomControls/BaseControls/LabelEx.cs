using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Permissions;
using System.Windows.Forms;

namespace WinFrmTalk.Controls
{
    /// <summary>
    /// 文字带有描边
    /// </summary>
    public class LabelEx : Label
    {
        #region 双击变为单击
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();

                CreateParams cp = base.CreateParams;
                cp.ClassStyle &= ~0x0008;
                cp.ClassName = null;

                return cp;
            }
        }
        #endregion

        /// <summary>
        /// 是否带有描边
        /// </summary>
        public bool IsStroke { get; set; }
        private Color BorderColor { get; set; }
        private Brush fillBrush;
        private Pen borderPen;

        /// <summary>
        /// 文字带有描边
        /// </summary>
        public LabelEx()
        {
            //设置默认值
            IsStroke = false;
            BorderColor = Color.White;
            borderPen = new Pen(BorderColor, 3);
            fillBrush = Brushes.Black;
            this.Paint += Lbl_Paint;

            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }

        //获取需要绘制的path 
        private GraphicsPath GetStringPath(string s, float dpi, RectangleF rect, Font font, StringFormat format)
        {
            GraphicsPath path = new GraphicsPath();
            //计算文字高度
            float emSize = dpi * this.Font.SizeInPoints / 72;
            //向path中添加字符串及相应信息 
            path.AddString(s, this.Font.FontFamily, (int)this.Font.Style, emSize, rect, format);
            return path;
        }

        //重写label控件的paint方法
        public void Lbl_Paint(object sender, PaintEventArgs e)
        {
            if (this.Text == null || this.Text.Length < 1 || !IsStroke)
                return;

            Graphics g = e.Graphics;//相当于画笔
            Font f = this.Font;//设置的字体
            RectangleF rect = this.ClientRectangle;//获取控件的工作区
            //计算垂直偏移量
            float dy = (this.Height - g.MeasureString(this.Text, this.Font).Height) / 2.0f;
            //计算水平偏移
            float dx = (this.Width - g.MeasureString(this.Text, this.Font).Width) / 2.0f;
            dx = 5;
            //将文字显示的工作区偏移dx,dy，实现文字居中、水平居中、垂直居中
            rect.Offset(dx, dy);
            //文本布局信息 详细功能请查看注释 这里可有可无
            StringFormat format = StringFormat.GenericTypographic;
            float dpi = g.DpiY;
            using (GraphicsPath path = GetStringPath(this.Text, dpi, rect, f, format))
            {
                //if (isShadow)
                //{
                //    //阴影代码
                //    RectangleF off = rect;
                //    off.Offset(5, 5);//阴影偏移
                //    using (GraphicsPath offPath = GetStringPath(this.Text, dpi, off, f, format))
                //    {
                //        Brush b = new SolidBrush(Color.FromArgb(100, 0, 0, 0));//阴影颜色
                //        g.FillPath(b, offPath);
                //        //  g.DrawPath(borderPen, offPath);//给阴影描边
                //        b.Dispose();
                //    }
                //}
                g.SmoothingMode = SmoothingMode.AntiAlias;//设置字体质量
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                g.DrawPath(borderPen, path);//绘制轮廓（描边） borderPen 描边色

                g.FillPath(fillBrush, path);//填充轮廓（填充） fillBrush 填充色
            }
        }
    }
}
