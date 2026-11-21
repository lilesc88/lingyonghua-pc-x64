using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WinFrmTalk.Controls.CustomControls;
using WinFrmTalk.Model;
using WinFrmTalk.View;

namespace WinFrmTalk
{
    public static class MsgUtils
    {
        public static void FowardMsgToFd(MessageObject messageObject)
        {
            FrmSortSelect frmFriendSelect = Applicate.GetWindow<FrmSortSelect>();
            if (frmFriendSelect != null)
            {
                frmFriendSelect.Show();
                frmFriendSelect.BringToFront();
                Applicate.GetWindow<FrmMain>().ShowTipBox("当前已有好友选择器正在使用，请关闭后再尝试");
                return;
            }

            //选择转发的好友
            frmFriendSelect = new FrmSortSelect();
            frmFriendSelect.LoadFriendsData(true, true, true, true, true);
            frmFriendSelect.Show();
            frmFriendSelect.AddConfrmListener((UserFriends) =>
            {
                Applicate.SyncMainContext.Send((state) =>
                {
                    var frmSchedule = new FrmSchedule(UserFriends, (fd) =>
                    {
                        if (fd.IsGroup == 1)
                        {

                            if (fd.IsSecretGroup == 1)
                            {
                                if (UIUtils.IsNull(fd.ChatKeyGroup))
                                {
                                    // 没有密钥
                                    HttpUtils.Instance.ShowTip("不能转发消息到密钥丢失群");
                                    return;
                                }

                                if (fd.IsLostKeyGroup == 1)
                                {
                                    // 没有密钥
                                    HttpUtils.Instance.ShowTip("不能转发消息到密钥丢失群");
                                    return;
                                }
                            }


                            RoomMember roomMember = RoomMemberDao.Instance.GetRoomMemberById(fd.RoomId, Applicate.MyAccount.userId);

                            if (roomMember.role == 3)
                            {
                                // 是否全体禁言 管理员和群主除外
                                if (fd.AllBannedTalk != 0)
                                {
                                    // 全体禁言
                                    HttpUtils.Instance.ShowTip("不能转发消息到全体禁言群");
                                    return;
                                }

                                //是否单个禁言
                                if (roomMember.talkTime > TimeUtils.CurrentIntTime())
                                {
                                    HttpUtils.Instance.ShowTip("您已被禁止在此群发言");
                                    return;
                                }
                            }


                            if (roomMember.role == 4)
                            {
                                HttpUtils.Instance.ShowTip("您在此群为隐身人，不能发送消息");
                                return;
                            }
                        }
                        //如果为群组，自己为隐身人或者被禁言
                        MessageObject msg = ShiKuManager.SendForwardMessage(fd, messageObject);
                        //如果转发对象包括当前聊天对象，给UI添加消息气泡
                        //if (fd.UserId == ChooseTarget.UserId)
                        //{
                        //    //添加消息气泡通知
                        //    JudgeMsgIsAddToPanel(msg);
                        //}
                        Messenger.Default.Send(msg, MessageActions.XMPP_UPDATE_NORMAL_MESSAGE);
                    }, false);
                    frmSchedule.Show();
                }, null);
            });
        }
    }
}
