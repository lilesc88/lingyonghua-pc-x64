using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using WinFrmTalk.Model;
using WinFrmTalk.View;
using WinFrmTalk.View.Common;

namespace WinFrmTalk.Controls
{
    public partial class RoomUrlListControls : UserControl
    {
        public RoomUrlListControls()
        {
            InitializeComponent();
        }

        internal void SetData(string json_urls)
        {

            if (UIUtils.IsNull(json_urls))
            {
                return;
            }

            var noticeinfo = JsonConvert.DeserializeObject<List<RoomTitleUrls>>(json_urls);


            this.Controls.Clear();


            int lineHeight = 20;
            int lineWidth = 12;

            int rlinew = lineWidth >> 1;

            int top = 0;
            if (lineHeight < this.Height)
            {
                top = (this.Height - lineHeight) / 2;
            }

            int width = this.Width / noticeinfo.Count;
            int index = 0;

            if (noticeinfo.Count > 0)
            {
                width -= rlinew;
            }

            foreach (var item in noticeinfo)
            {
                Label label = new Label();
                label.AutoSize = false;
                label.AutoEllipsis = true;
                label.Size = new Size(width, lineHeight);
                label.Location = new Point(index * width + (index * lineWidth), top);
                label.Text = item.name;
                label.ForeColor = Color.Blue;
                label.BackColor = Color.FromArgb(253, 112, 112);
                label.TextAlign = ContentAlignment.MiddleCenter;
                label.Tag = item.url;
                label.Click += Label_Click;
                this.Controls.Add(label);
                index++;
            }
        }

        private void Label_Click(object sender, EventArgs e)
        {
            Label label = sender as Label;

            string url = label.Tag as string;
            OpenLink(url);
        }


        // 查看收藏中的链接
        private void OpenLink(string url)
        {
            var msg = new MessageObject() { content = url };

            var frmSeeText = Applicate.GetWindow<FrmBrowser>();
            if (frmSeeText == null)
            {
                frmSeeText = new FrmBrowser();
                frmSeeText.BrowserShow(msg, true);
            }
            else
            {
                frmSeeText.Activate();
                frmSeeText.WindowState = FormWindowState.Normal;
                frmSeeText.BrowserShow(msg, true);
            }
        }
    }


    public class RoomTitleUrls
    {
        public string name;
        public string url;
    }
}
