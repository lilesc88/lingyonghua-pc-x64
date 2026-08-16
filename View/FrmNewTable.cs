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
using WinFrmTalk.Model.dao;
using WinFrmTalk.View.list;

namespace WinFrmTalk.View
{
    public partial class FrmNewTable : FrmBase
    {
        public FrmNewTable()
        {
            InitializeComponent();
        }

        private void FrmNewTable_Load(object sender, EventArgs e)
        {

        }

        private void btnSend_Click(object sender, EventArgs e)
        {

        }

        private void FrmNewTable_FormClosed(object sender, FormClosedEventArgs e)
        {
            HttpUtils.Instance.PopView(this);

            //获取用户
            Friend targetFd = this.showMsgPanel.ChooseTarget.GetFdByUserId();
            if (targetFd == null || targetFd.Status != 2)
                return;


            // 获取最后一条消息用作更新列表
            MessageObject lastMsg = MessageObjectDao.Instance.GetLastMessage(targetFd.UserId);
            if (lastMsg == null)
            {
                lastMsg = ShiKuManager.GetMessageObject(targetFd);
            }

            if (UIUtils.IsNull(showMsgPanel.InputText) && targetFd.LastMsgType != 0)
            {
                targetFd.UpdateFriendLastType(0);
            }


            if (!UIUtils.IsNull(showMsgPanel.InputText) && targetFd.LastMsgType == 0)
            {
                targetFd.UpdateFriendLastType(MessageActions.DRAFT_TYPE);
                targetFd.UpdateFriendLastContent(showMsgPanel.InputText, MessageActions.DRAFT_TYPE, TimeUtils.CurrentIntTime());
            }

            showMsgPanel.SetChooseFriend(new Friend());
            ChatTargetDictionary.RemoveItem(targetFd.UserId);

            //更新最近消息列表
            Messenger.Default.Send(lastMsg, MessageActions.XMPP_SHOW_SINGLE_MESSAGE);

    
         
        }
    }
}
