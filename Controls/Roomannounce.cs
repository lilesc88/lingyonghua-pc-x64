using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using WinFrmTalk.Model;
using Newtonsoft.Json;

namespace WinFrmTalk.Controls
{
    // 群公告滚动标题
    public partial class Roomannounce : UserControl
    {

        private const int SPEED = 1;

        public Roomannounce()
        {
            InitializeComponent();
        }

        public string NoticeId { get; set; }

        public string Content { get; set; }


        public event EventHandler CloseClick;

        public void UpdateData(string noticeId, string content)
        {
            this.NoticeId = noticeId;
            this.Content = content.Replace("\r", " ").Replace("\n", " ");
            lbladdtext.Text = "";

            RefreLayout();
        }


        public void RefreLayout()
        {
            int textwidth = FontWidth(lblText.Font, lblText, Content);

            lblText.Width = textwidth + 60;
            lbladdtext.Width = textwidth + 60;

            if (textwidth <= panel1.Width)
            {
                // 文字比较短的情况
                lblText.Text = Content;
                timer1.Stop();
                Point p = new Point(3, 5);
                lblText.Location = p;
            }
            else
            {
                lblText.Text = Content;
                lbladdtext.Text = Content;

                timer1.Enabled = true;
                timer1.Start();

                Point p = new Point();
                p.X = lblText.Size.Width + lblText.Location.X + 60;
                p.Y = lblText.Location.Y;
                lbladdtext.Location = p;

            }
        }


        /// <summary>
        /// 定时器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            //lbladdtext.Text = lblText.Text.Replace("\r", " ").Replace("\n", " ");
            //int txtwid = FontWidth(lblText.Font, lblText, lblText.Text);

            Point p1 = lblText.Location;
            Point p2 = lbladdtext.Location;

            p1.X -= SPEED;
            p2.X -= SPEED;

            if (p1.X <= (-lblText.Width))
            {
                p1.X = p2.X + lblText.Width + 60;
            }

            if (p2.X <= (-lbladdtext.Width))
            {
                p2.X = p1.X + lblText.Width + 60;
            }

            lbladdtext.Location = p2;
            lblText.Location = p1;
        }



        private void lblClose_Click(object sender, EventArgs e)
        {
            StopScroll();

            CloseClick?.Invoke(this, e);
        }


        public void StopScroll()
        {
            timer1.Stop();
        }


        /// <summary>
        /// 获取控件输入文字的长度
        /// </summary>
        /// <param name="font"></param>
        /// <param name="control"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        private int FontWidth(Font font, Control control, string str)
        {
            using (Graphics g = control.CreateGraphics())
            {
                SizeF siF = g.MeasureString(str, font); return (int)siF.Width;
            }
        }
    }
}
