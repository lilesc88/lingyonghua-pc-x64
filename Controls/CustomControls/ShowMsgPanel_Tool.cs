using ScreenshotTool;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using WinFrmTalk.Live;
using WinFrmTalk.Model;
using WinFrmTalk.View;

namespace WinFrmTalk.Controls.CustomControls
{
    public partial class ShowMsgPanel_Tool : UserControl
    {
        public Action<ExpressionType, string> ExpressionAction = null;     //回调：添加emoji表情到文本框
        public Action SendFileAction = null;            //回调：选择文件发送
        public Action<FrmCapture> CaptureAction = null;     //回调：截图
        public Action RecordAudioAction = null;         //回调：录音
        public Action CameraAction = null;              //回调：拍照
        public Action PhotographyAction = null;         //回调：录像
        public Action<MouseEventArgs> QreplyAction = null;              //回调：快捷回复

        public ShowMsgPanel_Tool()
        {
            InitializeComponent();
            toolTip.ShowAlways = true;//是否显示提示框
            LoadLanguageText();
        }

        #region 国际化内容
        private void LoadLanguageText()
        {
            if (!Applicate.ENABLE_MULTI_LANGUAGE)
                return;

            //设置提示按钮和提示内容
            toolTip.SetToolTip(this.lblExpression, LanguageXmlUtils.GetValue("exps_default", "表情"));
            toolTip.SetToolTip(this.lblSendFile, LanguageXmlUtils.GetValue("tip_send_file", "发送文件"));
            toolTip.SetToolTip(this.lblScreen, LanguageXmlUtils.GetValue("tip_screen", "截图"));
            toolTip.SetToolTip(this.lblHistory, LanguageXmlUtils.GetValue("History", "聊天记录"));
            toolTip.SetToolTip(this.lblLocation, LanguageXmlUtils.GetValue("tip_send_position", "发送位置"));
            toolTip.SetToolTip(this.lblSoundRecord, LanguageXmlUtils.GetValue("tip_record", "录音"));
            toolTip.SetToolTip(this.lblCamera, LanguageXmlUtils.GetValue("tip_photograph", "拍照"));
            toolTip.SetToolTip(this.lblPhotography, LanguageXmlUtils.GetValue("tip_video", "录像"));
            toolTip.SetToolTip(this.lbl_qreply, LanguageXmlUtils.GetValue("tip_fast_reply", "快捷回复"));
            toolTip.SetToolTip(this.lblAudio, LanguageXmlUtils.GetValue("tip_audio_chat", "语音聊天"));
            toolTip.SetToolTip(this.lblVideo, LanguageXmlUtils.GetValue("tip_video_chat", "视频聊天"));
            toolTip.SetToolTip(this.lblTalk, LanguageXmlUtils.GetValue("tip_speank_chat", "对讲机"));
        }
        #endregion

        #region 工具栏

        #region 选择表情
        private FrmExpressionTab frmExpressionTab;      //表情列表窗口
        private void LblExpression_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            //获取鼠标点击表情时的坐标
            Point ms = Control.MousePosition;
            frmExpressionTab = FrmExpressionTab.GetExpressionTab();
            //修改列表索引
            frmExpressionTab.tabExpression.SelectedIndex = 0;
            //设置弹出窗起始坐标
            int location_x = ms.X - e.X - 8;
            int location_y = ms.Y - frmExpressionTab.Height - e.Y - 5;
            frmExpressionTab.Location = new Point(location_x, location_y);
            //传递对象给新窗口
            //frmExpressionTab.SetFriendTarget();
            //控件显示在最上层
            frmExpressionTab.TopMost = true;
            frmExpressionTab.Show();

            frmExpressionTab.expressionAction = this.ExpressionAction;
        }
        #endregion

        #region 选择文件发送
        private void LblSendFile_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            SendFileAction?.Invoke();
        }
        #endregion

        private void LblScreen_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            FrmCapture frmCapture = new FrmCapture(false, false);
            frmCapture.ShowDialog();
            if (frmCapture.DialogResult == DialogResult.OK)
            {
                CaptureAction?.Invoke(frmCapture);
            }

            if (!Visible)
            {
                this.Parent.Parent.Show();
            }
        }

        #region 历史聊天记录
        private void LblHistory_MoseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (this.Parent.Parent is ShowMsgPanel showMsgPanel)
            {
                var frmHistory = Applicate.GetWindow<FrmHistoryChat>();

                if (frmHistory == null)
                {
                    frmHistory = new FrmHistoryChat();
                    frmHistory.ShowFriendMsg(showMsgPanel.ChooseTarget);
                    frmHistory.Show();
                }
                else
                {
                    frmHistory.Activate();
                    frmHistory.ShowFriendMsg(showMsgPanel.ChooseTarget);
                }
            }
        }
        #endregion

        #region 选择并发送定位
        private void LblLocation_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (this.Parent.Parent is ShowMsgPanel showMsgPanel)
            {
                Console.WriteLine("" + ShiKuManager.GetXmppState());
                if (ShiKuManager.GetXmppState() != socket.SocketConnectionState.Authenticated)
                {
                    HttpUtils.Instance.ShowTip("网络异常，不能定位");
                    return;
                }

                FrmSedLocation frmSedLocation = FrmSedLocation.CreateInstrance();
                frmSedLocation.initCefSharp(showMsgPanel.ChooseTarget);
                //frmSedLocation.Show();
            }
        }
        #endregion

        #region 录音
        private void LblSoundRecord_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            RecordAudioAction?.Invoke();
        }
        #endregion

        #region 拍照
        private void LblCamera_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (Applicate.IsOpenRecordView())
            {
                return;
            }

            CameraAction?.Invoke();
        }
        #endregion

        #region 录像
        private void LblPhotography_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (Applicate.IsOpenRecordView())
            {
                return;
            }

            PhotographyAction?.Invoke();
        }
        #endregion

        #region 群组直播
        private void LblRoomLive_MouseDown(object sender, MouseEventArgs e)
        {
            //if (this.Parent.Parent is ShowMsgPanel showMsgPanel)
            //{
            //    if (showMsgPanel.ChooseTarget.IsGroup != 1)
            //        return;
            //    //int role = RoomMemberDao.Instance.GetMemberRole(showMsgPanel.ChooseTarget.RoomId, Applicate.MyAccount.userId);
            //    bool isAdmin = IsAdmin(showMsgPanel.ChooseTarget.RoomId, Applicate.MyAccount.userId);
            //    if (!isAdmin && showMsgPanel.ChooseTarget.AllowOpenLive == 0)
            //    {
            //        HttpUtils.Instance.ShowTip("该群组不允许普通成员开启直播！");
            //        return;
            //    }
            //    if (Applicate.IsOpenRecordView())
            //        return;

            //    //RoomLive roomLive = new RoomLive();
            //    //roomLive.GetRoom_Live(ChooseTarget.RoomId);
            //    //避免直播间重复打开
            //    LiveCardBean liveCardBean = LiveRoomNotice.lives.Where(live => live.Key == showMsgPanel.ChooseTarget.RoomId).Select(l => l.Value).FirstOrDefault();
            //    if (liveCardBean == null)
            //    {
            //        liveCardBean = new LiveCardBean();
            //        liveCardBean.liveRoomId = showMsgPanel.ChooseTarget.RoomId;
            //        liveCardBean.isOpen = false;
            //        LiveRoomNotice.lives.Add(showMsgPanel.ChooseTarget.RoomId, liveCardBean);
            //    }
            //    if (!liveCardBean.isOpen)
            //    {
            //        liveCardBean.isOpen = true;
            //        try
            //        {
            //            new RoomLive().GetRoom_Live(showMsgPanel.ChooseTarget.RoomId);
            //        }
            //        catch (Exception)
            //        {
            //            liveCardBean.isOpen = false;
            //        }
            //    }
            //}
        }
        #endregion

        #region 快捷回复
        private void Lbl_qreply_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            QreplyAction?.Invoke(e);
        }
        #endregion

        #region 发起语音聊天
        private void LblAudio_MouseClick(object sender, MouseEventArgs e)
        {
          
        }
        #endregion

        #region 发起视频聊天
        private void LblVideo_MouseClick(object sender, MouseEventArgs e)
        {
            
        }
        #endregion

        #region 公众号快捷栏
        private void LblSLC_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (this.Parent.Parent is ShowMsgPanel showMsgPanel)
            {
                //serviceListCrl.SetList(ChooseTarget, this);
                showMsgPanel.SetDialogBoxSize(DialogBox.OfficialAccount);
            }
        }
        #endregion

        #endregion

        private bool IsAdmin(string roomId, string userId)
        {
            bool isAdmin = true;
            if (!string.IsNullOrEmpty(roomId))
            {
                int role = RoomMemberDao.Instance.GetMemberRole(roomId, userId);
                if (role > 2)
                    isAdmin = false;
    }
            else
                isAdmin = false;
            return isAdmin;
}
    }
}
