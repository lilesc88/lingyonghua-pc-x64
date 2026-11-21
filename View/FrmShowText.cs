using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinFrmTalk.Dictionarys;
using WinFrmTalk.Model;

namespace WinFrmTalk.View
{
    public partial class FrmShowText : FrmBase
    {
        private decimal rowNum = 0;
        //private int crl_height = 0;
        private string content = "";
        private string messageid = "";

        private FrmShowText(/*int height, string content = ""*/)
        {
            //this.crl_height = height;
            //this.content = content;
            //this.WindowState = FormWindowState.Maximized;
            InitializeComponent();
            txtContent.SetReadMode();  //不显示光标
            RegistNotiy();
        }
        private void RegistNotiy()
        {
            //按回执更新已读消息状态
            Messenger.Default.Register<MessageObject>(this, MessageActions.CLEAR_CURRENT_CONTENT, item => Getmonitor(item));
            Messenger.Default.Register<MessageObject>(this, MessageActions.XMPP_UPDATE_RECALL_MESSAGE, item => Getmonitor(item));

        }

        private void Getmonitor(MessageObject item)
        {
            if (item.messageId == messageid)
            {
                this.Close();
            }
        }

        public static FrmShowText ShowForm(int height, int font_height, String messageid, string content = "")
        {
            FrmShowText frmShowText = Applicate.GetWindow<FrmShowText>();
            if (frmShowText == null)
                frmShowText = new FrmShowText();
            frmShowText.rowNum = height / (decimal)font_height;
            frmShowText.content = content;
            frmShowText.messageid = messageid;
            return frmShowText;
        }


        public void LoadForm()
        {
            this.SuspendLayout();
            txtContent.Text = content.Replace("\n", "\r\n").Trim();
            //txtContent.MaximumSize = new Size(0, this.Height - txtContent.Location.Y);
            SizeF sizeF = EQControlManager.GetStrSizeFByTxtBox(txtContent);
            txtContent.Height = Convert.ToInt32(rowNum * txtContent.Font.Height);
            //txtContent.Width = this.Width - 14;
            int loc_y = Convert.ToInt32(Math.Ceiling((this.Height - 31 - txtContent.Height - 8) / 2.0)) + 31;   //31为顶部高度，8为底部空隙
            txtContent.Location = new Point(txtContent.Location.X, loc_y);
            //转换Emoji表情
            List<string> emojis = new List<string>();
            REUtils.MatchCollectionRE(content, REUtils.Emoji_Format, out emojis);
            foreach (string key in emojis)
            {
                string emoji_rtf = EmojiCodeDictionary.GetEmojiRtfByCode(key, false);
                txtContent.Rtf = txtContent.Rtf.Replace(key, emoji_rtf);
            }
            //居中显示
            txtContent.SelectAll();
            txtContent.SelectionAlignment = HorizontalAlignment.Center;
            txtContent.SelectionStart = txtContent.Text.Length;
            this.ResumeLayout();
            this.Show();
        }

        private void FrmShowText_FormClosed(object sender, FormClosedEventArgs e)
        {
            Messenger.Default.Unregister(this);
        }
    }
}
