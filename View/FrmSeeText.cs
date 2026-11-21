using RichTextBoxLinks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WinFrmTalk.Dictionarys;
using WinFrmTalk.Helper;
using WinFrmTalk.Model;

namespace WinFrmTalk.View
{
    public partial class FrmSeeText : FrmBase
    {
        private void LoadLanguageText()
        {
            if (!Applicate.ENABLE_MULTI_LANGUAGE)
                return;

            this.Text = LanguageXmlUtils.GetValue("detail", this.Text);
            btnCollection.ToolTipText = LanguageXmlUtils.GetValue("collect", btnCollection.ToolTipText);
            btnShare.ToolTipText = LanguageXmlUtils.GetValue("forward", btnShare.ToolTipText);
            ToolMenuCopy.Text = LanguageXmlUtils.GetValue("copy", ToolMenuCopy.Text);
        }
        public string collectid = "";
        public FrmSeeText()
        {
            InitializeComponent();
            LoadLanguageText();
            this.Icon = (Icon)Properties.Resources.Icon.Clone();//加载icon图标（独立副本，避免共享句柄被释放导致闪退）
            RegistNotiy();

            //点击超链接
            txttext.DetectUrls = true;
            txttext.LinkClicked += (sender, e) =>
            {
                //System.Diagnostics.Process.Start(e.LinkText);
                FrmBrowser frmBrowser = new FrmBrowser();
                frmBrowser.OpenUrl(e.LinkText, Applicate.MyAccount.userId);
            };
        }
        private void RegistNotiy()
        {
            //按回执更新已读消息状态
            Messenger.Default.Register<string>(this, MessageActions.CLEAR_COLLECT_TEXT, item => Getmonitor(item));

        }

        private object Getmonitor(string item)
        {
            if(!UIUtils.IsNull( collectid))
            {
                if(item.Equals(collectid))
                {
                    HttpUtils.Instance.PopView(this);
                    this.Close();
                }
            }
            return null;
        }


        #region 属性和变量
        /// <summary>
        /// 等待符
        /// </summary>
        private static LodingUtils loding;

        private bool _iscollect;
        private static int emoji_count = 0;
        string text = "";
        private MessageObject _msg;
        /// <summary>
        /// 是否需要收藏功能
        /// </summary>
        public bool iscollect
        {
            get { return _iscollect; }
            set
            {
                _iscollect = value;
                if (value)
                {
                    btnCollection.Visible = true;
                }
                else
                {
                    btnCollection.Visible = false;
                }
            }
        }

        /// <summary>
        /// 是否需要收藏功能
        /// </summary>
        public bool IsForward
        {
            get { return btnShare.Visible; }
            set
            {
                btnShare.Visible = value;
            }
        }


        private void ShowLodingDialog(Control con)
        {
            loding = new LodingUtils();
            loding.parent = con;
            loding.Title = "加载中";
            loding.start();
        }

        /// <summary>
        /// 传入MessageObject
        /// </summary>
        public MessageObject Getmsg
        {
            get { return _msg; }
            set { _msg = value; }
        }
        
        
        public string Longtext
        {
            get { return txttext.Text; }
            set
            {
                this.SuspendLayout();
                txttext.Text = value;
                text = value;
                Calc_PanelWidth(txttext);

                if (!UIUtils.IsNull(value))
                {
                    txttext.SelectionStart = value.Length;
                }
                this.ResumeLayout();
            }
        }
        #endregion

        public void HideCursor()
        {
            txttext.ReadOnly = true;
        }

        /// <summary>
        /// 转发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnShare_Click(object sender, EventArgs e)
        {

            var frm = Applicate.GetWindow<FrmFriendSelect>();
            if (frm == null)
            {
                frm = new FrmFriendSelect();

            }
            else
            {
                frm.Activate();
                frm.WindowState = FormWindowState.Normal;
                return;

            }

            //frm = new FrmFriendSelect();

            frm.LoadFriendsData(1);
            frm.AddConfrmListener((dis) =>
            {
                foreach (var friend in dis)
                {
                    if (CollectUtils.EnableForward(friend.Value))
                    {
                        continue;
                    }

                    //文本表情
                    MessageObject messText = ShiKuManager.SendForwardMessage(friend.Value, new MessageObject() { content = text, type = kWCMessageType.Text });
                    Messenger.Default.Send(messText, token: MessageActions.XMPP_UPDATE_NORMAL_MESSAGE);

                }
                HttpUtils.Instance.ShowTip("转发成功");
            });
        }
        /// <summary>
        /// 收藏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCollection_Click(object sender, EventArgs e)
        {
            CollectUtils.CollectMessage(Getmsg);
        }
        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolMenuCopy_Click(object sender, EventArgs e)
        {
            if (txttext.Text != "")
            {
                txttext.SelectAll();
                string copy = txttext.SelectedText;//选中文字
                Clipboard.SetDataObject(copy);//复制
                //txttext.Select(0, 0);
            }


        }
        /// <summary>
        /// 重绘边框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStrip1_Paint(object sender, PaintEventArgs e)
        {
            if ((sender as ToolStrip).RenderMode == ToolStripRenderMode.System)
            {
                Rectangle rect = new Rectangle(0, 0, this.toolStrip1.Width, this.toolStrip1.Height - 2);
                e.Graphics.SetClip(rect);
            }
        }
        public static void Calc_PanelWidth(Control control)
        {
            if (!(control is RichTextBoxEx richContent))
                return;

            //临时建立一个容器装入内容
            RichTextBoxEx canv_Rich = control as RichTextBoxEx;

            //先取全部Text的值
            canv_Rich.Text = richContent.Text;
            //把code转为emoji
            canv_Rich.Font = new Font(Applicate.SetFont, 10);//用来设置字体的，一定不能少，不然会变成默认的宋体了
            canv_Rich.Rtf = GetLink(canv_Rich.Text);
            richContent.Rtf = canv_Rich.Rtf;

        }
        public static string GetLink(string msgText)
        {
            RichTextBoxEx richTextBox = new RichTextBoxEx();
            richTextBox.Font = Applicate.myFont;
            richTextBox.Text = msgText;
            //正则表达式
            MatchCollection msg = Regex.Matches(msgText, @"^http://([\w-]+\.)+[\w-]+(/[\w-./?%&=]*)?$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            foreach (Match match in msg)
            {
                int str_index = richTextBox.Text.IndexOf(match.Value);
                richTextBox.SelectionStart = str_index;
                richTextBox.SelectionLength = match.Value.Length;
                richTextBox.SelectedText = "";
                richTextBox.InsertLink(match.Value);
            }
            //emajio表情
            msg = Regex.Matches(richTextBox.Text, @"\[[a-z_-]*\]", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            emoji_count = msg.Count;
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

                richTextBox.Rtf = richTextBox.Rtf.Replace(newStr[i], EmojiCodeDictionary.GetEmojiRtfByCode(newStr[i]));
            }
            string result = richTextBox.Rtf;
            richTextBox.Dispose();
            return result;
        }
    }
}
