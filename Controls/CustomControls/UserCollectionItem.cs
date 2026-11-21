using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinFrmTalk.Model;
using RichTextBoxLinks;
using WinFrmTalk.View;
using System.Text.RegularExpressions;
using WinFrmTalk.Dictionarys;

namespace WinFrmTalk.Controls.CustomControls
{
    /// <summary>
    /// 我的收藏和我的讲课item项
    /// </summary>
    public partial class UserCollectionItem : UserControl
    {

        public delegate void EventCollectHandler(UserCollectionItem item, EventArgs eve);
        public delegate void EventCollectImageHandler(Collections data, int index);
        private const int IMAGE_WIDTH = 110;
        private const int IMAGE_HEIGHT = 60;

        /// <summary>
        /// 类型
        /// </summary>
        //public string type { get; set; }
        // public string CollectContent { get; set; }

        public UserCollectionItem()
        {
            InitializeComponent();
        }


        // 我的收藏数据
        public Collections Collections { get; set; }

        // 我的讲课数据
        public MyColleagues MyColleagues { get; set; }


        public event EventCollectHandler OnClickCollectItem;
        public event EventCollectImageHandler OnClickCollectImage;

        public void FillCollectData(Collections collect)
        {
            Collections = collect;

            // 填充根据不同的数据填充不同的子控件
            SetPanelControls(collect);

            // 收藏公共的数据
            lblComeFrom.Text = LanguageXmlUtils.GetValue("from:", "来自:", true) + Applicate.MyAccount.nickname;
            //收藏的时间
            lblTime.Text = TimeUtils.FromatTime(collect.createTime, "yyyy/MM/dd");
        }

        private void SetPanelControls(Collections collect)
        {
            switch (collect.type)
            {
                case "1":
                    // 图片和文字
                    SetImageText(collect);
                    break;
                case "2":
                    // 视频
                    SetVideoView(collect);
                    break;
                case "3":
                    // 文件
                    SetFileView(collect);
                    break;
                case "4":
                    // 语音
                    SetVoiceView(collect);
                    break;
                case "5":
                    // 链接
                    SetLinkView(collect);
                    break;
                default:
                    break;
            }

            if (collect.type != "1")
            {
                //添加遮罩层
                EQShowInfoPanelAlpha panAlpha = new EQShowInfoPanelAlpha()
                {
                    Name = "panAlpha",
                    Cursor = Cursors.Hand,
                    Dock = DockStyle.Fill,
                };
                panAlpha.Click += Item_Click;
                panel.Controls.Add(panAlpha);
                panAlpha.BringToFront();
            }
        }



        private void SetImageText(Collections collect)
        {
            int imageTop = 20;
            // 判断是否在文字
            if (!UIUtils.IsNull(collect.collectContent))
            {
                Label piclabel = new Label();
                piclabel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                piclabel.Font = new Font(Applicate.SetFont, 10f);
                piclabel.AutoSize = false;
                piclabel.AutoEllipsis = true;
                piclabel.Text = collect.collectContent;
                piclabel.Location = new Point(20, 15);
                //piclabel.MouseDown += Item_Click;
                piclabel.Size = new Size(400, 20);
                this.panel.Controls.Add(piclabel);

                imageTop = 45;
            }

            // 解析所有图片
            string[] strarr = collect.msg.Split(',');

            int imageLeft = 20;

            for (int i = 0; i < strarr.Length; i++)
            {
                if (imageLeft + IMAGE_WIDTH <= this.panel.Width)
                {
                    string url = strarr[i];
                    ImageViewx pic = new ImageViewx();
                    pic.Anchor = AnchorStyles.Left;
                    pic.Cursor = Cursors.Hand;
                    pic.Size = new Size(IMAGE_WIDTH, IMAGE_HEIGHT);
                    pic.Location = new Point(imageLeft, imageTop);
                    pic.Tag = i;
                    ImageLoader.Instance.Load(url).Into((image, path) =>
                    {
                        pic.Image = image;
                    });

                    pic.MouseDown += OnItemImage;

                    this.panel.Controls.Add(pic);

                    imageLeft = (15 + IMAGE_WIDTH) * (i + 1) + 20;
                }
            }
        }



        private void SetVideoView(Collections collect)
        {
            ImageViewxVideo picVideo = new ImageViewxVideo();
            picVideo.Anchor = AnchorStyles.Left;
            picVideo.Size = new Size(IMAGE_WIDTH, IMAGE_HEIGHT);
            picVideo.Location = new Point(20, 20);
            ThubImageLoader.Instance.LoadImage(collect.url, picVideo);
            //picVideo.MouseDown += Item_Click;
            panel.Controls.Add(picVideo);
        }

        private void SetFileView(Collections collect)
        {
            UserFileLeft panel_file = new UserFileLeft();
            panel_file.Cursor = Cursors.Hand;
            string fileNames = FileUtils.GetFileName(collect.Filename);
            panel_file.lab_fileName.Text = fileNames;
            panel_file.lab_fileSize.Text = UIUtils.FromatFileSize(collect.fileSize);
            panel_file.Location = new Point(20, 14);
            FrmHistoryChat.TypeFileToImage(collect.url, panel_file.lab_icon);
            //panel_file.MouseDown += Item_Click;
            panel.Controls.Add(panel_file);

        }

        private void SetVoiceView(Collections collect)
        {
            UserFileLeft panel_file = new UserFileLeft();
            panel_file.Cursor = Cursors.Hand;
            string fileNames = FileUtils.GetFileName(collect.Filename);
            panel_file.lab_fileName.Text = fileNames;
            panel_file.lab_fileSize.Text = UIUtils.FromatFileSize(collect.fileSize);
            panel_file.Location = new Point(20, 14);
            FrmHistoryChat.TypeFileToImage(collect.url, panel_file.lab_icon);
            //panel_file.MouseDown += Item_Click;
            panel.Controls.Add(panel_file);
        }

        private void SetLinkView(Collections collect)
        {
            RichTextBoxEx richText = new RichTextBoxEx();
            richText.Font = new Font(Applicate.SetFont, 9f);
            richText.Size = new Size(350, 50 + CheckHeight(collect.collectContent));

            richText.Location = new Point(20, 20);
            richText.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            //richText.Cursor = Cursors.Arrow;
            richText.BorderStyle = BorderStyle.None;
            richText.ScrollBars = RichTextBoxScrollBars.None;
            richText.ReadOnly = true;
            richText.Multiline = true;
            richText.DetectUrls = true;
            richText.Text = RemoveEnter(collect.collectContent);
            richText.SetReadMode();  //不显示光标
            //richText.DoubleClick += Item_Click;//点击超链接
            richText.LinkClicked += (sender, e) =>
            {
                //System.Diagnostics.Process.Start(e.LinkText);
                FrmBrowser frmBrowser = new FrmBrowser();
                frmBrowser.OpenUrl(e.LinkText, Applicate.MyAccount.userId);
            };
            Calc_PanelWidth(richText);
            panel.Controls.Add(richText);
        }

        public static int CheckHeight(string str)
        {
            if (UIUtils.IsNull(str))
            {
                return 0;
            }

            MatchCollection matchs = Regex.Matches(str, @"\[[a-z_-]*\]", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (matchs.Count > 1 && str.Length > 60)
            {
                return 15;
            }

            return 0;
        }

        private string RemoveEnter(string str)
        {
            if (UIUtils.IsNull(str))
            {
                return "";
            }

            return str.Replace("\n", "");
        }

        private void Item_Click(object sender, EventArgs e)
        {
            if (Collections.type.Equals("1") && ((MouseEventArgs)e).Button == MouseButtons.Left)
            {
                return;
            }

            OnClickCollectItem?.Invoke(this, e);
        }

        private void OnItemImage(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {
                var view = sender as ImageViewx;
                int index = Convert.ToInt32(view.Tag);

                OnClickCollectImage?.Invoke(Collections, index);
            }
            else
            {
                Item_Click(sender, e);
            }

        }

        public void Calc_PanelWidth(Control control)
        {
            if (!(control is RichTextBoxEx richContent))
                return;

            //临时建立一个容器装入内容
            RichTextBoxEx canv_Rich = control as RichTextBoxEx;
            //先取全部Text的值
            canv_Rich.Text = richContent.Text;
            //把code转为emoji
            canv_Rich.Rtf = GetLink(canv_Rich.Text);
            canv_Rich.Font = new Font(Applicate.SetFont, 10);//用来设置字体的，一定不能少，不然会变成默认的宋体了
            canv_Rich.BackColor = Color.WhiteSmoke;

            richContent.Rtf = canv_Rich.Rtf;
        }

        public string GetLink(string msgText)
        {
            RichTextBoxEx richTextBox = new RichTextBoxEx();
            richTextBox.Text = msgText;
            richTextBox.BackColor = Color.WhiteSmoke;
            MatchCollection msg = Regex.Matches(msgText, @"^http://([\w-]+\.)+[\w-]+(/[\w-./?%&=]*)?$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            foreach (Match match in msg)
            {
                int str_index = richTextBox.Text.IndexOf(match.Value);
                richTextBox.SelectionStart = str_index;
                richTextBox.SelectionLength = match.Value.Length;
                richTextBox.SelectedText = "";
                richTextBox.InsertLink(match.Value);
            }

            //正则表达式

            //emajio表情
            msg = Regex.Matches(richTextBox.Text, @"\[[a-z_-]*\]", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            int index = 0;
            string[] newStr = new string[msg.Count];
            foreach (Match item in msg)
            {
                newStr[index] = item.Groups[0].Value;
                index++;
            }
            //循环替换code为表情图片
            for (int i = 0; i < newStr.Length; i++)
            {
                //bool isMin = friends.userId == Applicate.MyAccount.userId;
                richTextBox.Rtf = richTextBox.Rtf.Replace(newStr[i], EnjoyCodeColor.GetEmojiRtfByCode(newStr[i], Color.WhiteSmoke));
            }
            string result = richTextBox.Rtf;
            richTextBox.Dispose();
            return result;
        }


        public void RefreshCourseTime(string courseTime)
        {

            long time = long.Parse(courseTime);

            lblTime.Text = TimeUtils.FromatTime(time, "yyyy/MM/dd");
        }


        internal void RefreshCourseName(string courseName)
        {
            Control[] result = this.panel.Controls.Find("panel_file", false);

            if (result != null)
            {
                var control = result[0] as UserFileLeft;
                control.lab_fileName.Text = LanguageXmlUtils.GetValue("name_courseware_name", "课件名称：", true) + UIUtils.LimitTextLength(courseName, 12,
            true);
            }

        }
    }
}
