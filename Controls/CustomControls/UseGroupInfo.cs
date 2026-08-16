using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinFrmTalk.Model;
using WinFrmTalk.View;
using WinFrmTalk.Properties;
using Newtonsoft.Json;

namespace WinFrmTalk.Controls.CustomControls
{
    public partial class UseGroupInfo : UserControl
    {
        private void LoadLanguageText()
        {
            if (!Applicate.ENABLE_MULTI_LANGUAGE)
                return;

            btnsend.Text = LanguageXmlUtils.GetValue("send_msg", btnsend.Text);
            btnSeeMember.Text = LanguageXmlUtils.GetValue("view_member", btnSeeMember.Text);
            lbldispose.Text = LanguageXmlUtils.GetValue("group_disbaned", lbldispose.Text);
        }

        public UseGroupInfo()
        {
            InitializeComponent();
            LoadLanguageText();
        }
        private int showMember = 0;
        List<Control> controlLst = new List<Control>();//加入面板中数据的集合
        List<PicChangeControl> member = new List<PicChangeControl>();
        int CurrentRole;
        //public RoomDetails room = new RoomDetails();
        private Friend _Room = new Friend();
        //private List<MembersItem> membersItems;
        private List<RoomMember> roomMemberList = new List<RoomMember>();
        List<MembersItem> memberlst = new List<MembersItem>();
        public Action<Friend> SendAction { get; set; }
        public Friend GroupInfo
        {
            get { return _Room; }
            set { _Room = value; }
        }
        string roomId;
        private void GroupInfo2_Load(object sender, EventArgs e)
        {

            RegistNotiy();
            //  LoadGroupData(roomId);

        }

        //请求服务器获取群数据

        #region 注册群控制消息
        private void RegistNotiy()
        {
            //按回执更新已读消息状态
            Messenger.Default.Register<MessageObject>(this, MessageActions.XMPP_UPDATE_ROOM_CHANGE_MESSAGE, item => Getmonitor(item));

        }
        #endregion
        /// <summary>
        /// 同步更新数据，当数据被更改时刷新UI
        /// </summary>
        /// <param name="msg">传入的消息</param>
        public void Getmonitor(MessageObject msg)
        {
            if (this.IsHandleCreated)
            {
                if (GroupInfo != null && string.Equals(msg.ChatJid, GroupInfo.UserId))
                {
                    Invoke(new Action(() =>
                    {
                        MainNotiy(msg);

                    }));
                }
            }
        }

        /// <summary>
        /// 消息监听 
        /// </summary>
        /// <param name="msg">传入的消息</param>
        public void MainNotiy(MessageObject msg)
        {

            switch (msg.type)
            {
                case kWCMessageType.RoomNameChange://改群名

                    lblTitle.Text = msg.content;
                    break;
                case kWCMessageType.RoomDismiss://解散

                    break;
                case kWCMessageType.RoomExit://退群

                    if (!string.Equals(msg.ChatJid, GroupInfo.UserId))
                    {
                        return;
                    }

                    if (msg.toUserId == Applicate.MyAccount.userId)//我主动退群
                    {
                        if (msg.FromId == Applicate.MyAccount.userId)//打开的是当前界面，将它关闭
                        {

                        }
                    }
                    else
                    {
                        //监听到有人退群，现在的处理是调一遍接口
                        Getmenberlist();
                    }
                    break;

                case kWCMessageType.RoomInvite://进群

                    if (string.Equals(msg.ChatJid, GroupInfo.UserId))
                    {
                        Getmenberlist();
                    }
                    break;
                case kWCMessageType.RoomMemberNameChange://改群内昵称


                    break;
                case kWCMessageType.RoomUserRecommend://允许私聊
                    GroupInfo.AllowSendCard = Convert.ToInt32(msg.content);
                    break;

                case kWCMessageType.RoomInsideVisiblity://允许显示群成员
                    showMember = Convert.ToInt32(msg.content);
                    GroupInfo.ShowMember = showMember;
                    GroupInfo.UpdateShowMember(showMember);
                    if (showMember == 0)//不允许显示群成员
                    {
                        if (CurrentRole == 3 || CurrentRole == 4)//普通成员和隐身人
                        {
                            AdddataTopal(memberlst, 0);
                        }
                    }
                    else
                    {
                        AdddataTopal(memberlst, 1);//群主和管理员
                    }
                    break;

                case kWCMessageType.RoomManagerTransfer://群主转让

                    // 修改老群组身份
                    ChangedListRole(msg.fromUserId, 3);
                    // 修改新群主身份
                    ChangedListRole(msg.toUserId, 1);

                    showMember = GroupInfo.ShowMember;
                    CurrentRole = RoomMemberDao.Instance.GetMemberRole(GroupInfo.RoomId, Applicate.MyAccount.userId);
                    if (showMember == 0)//不允许显示群成员
                    {
                        if (CurrentRole == 3 || CurrentRole == 4)//普通成员和隐身人
                        {
                            AdddataTopal(memberlst, 0);
                        }
                    }
                    else
                    {
                        AdddataTopal(memberlst, 1);//群主和管理员
                    }
                    break;

                case kWCMessageType.RoomAdmin:

                    break;
                default:
                    return;
            }
        }
        /// <summary>
        ///显示群组信息
        /// </summary>
        public void DisplayGroup(Friend group)
        {
            group = group.GetByUserId();

            lblTitle.Text = group.NickName;
            int length = lblTitle.Text.Length;
            if (length > 58)
            {
                lblTitle.Text = lblTitle.Text.Substring(0, 59) + "...";
            }

            pic.isDrawRound = false;
            //ImageLoader.Instance.DisplayAvatar(group.roomId, pic);
            ImageLoader.Instance.DisplayGroupAvatar(group.UserId, group.RoomId, pic);
            this.GroupInfo = group;
            roomId = group.RoomId;

            //if (group.status !=2)
            //{
            //    lbldispose.Visible = true;
            //}
            //else
            //{


            Getmenberlist();
        }
        private void Getmenberlist()
        {
            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/get") //获取群详情
                .AddParams("access_token", Applicate.Access_Token)
                .AddParams("roomId", roomId)
                .AddParams("pageSize", "15")
                 .Build().AddErrorListener((code, err) =>
                 {
                     // loding.stop();//关闭等待符
                 })
                 .Execute((sccess, result) =>
                 {
                     string mendata = UIUtils.DecodeString(result, "members");

                     string roomByme = UIUtils.DecodeString(result, "member");

                     var member = JsonConvert.DeserializeObject<Member>(roomByme);
                     CurrentRole = member.role;

                     List<MembersItem> members = JsonConvert.DeserializeObject<List<MembersItem>>(mendata);
                     if (members == null || members.Count < 13)
                     {
                         btnSeeMember.Visible = false;
                     }

                     else
                     {
                         btnSeeMember.Visible = true;
                     }
                     tabpal.Controls.Clear();
                     showmembertTopal(GroupInfo.ShowMember, members);
                     memberlst = members;
                 });
        }
        public void showmembertTopal(int showmember, List<MembersItem> memberlsts)
        {

            if (CurrentRole == 3 || CurrentRole == 4)
            {
                if (showmember == 0)
                {
                    AdddataTopal(memberlsts, 0);//不显示群成员
                }
                else
                {
                    AdddataTopal(memberlsts, 1);//显示群成员
                }
            }
            else//管理员和群主
            {
                AdddataTopal(memberlsts, 1);//显示群成员
            }

        }

        /// <summary>
        /// <para>shshowMember是否显示群成员</para>
        ///<para>memberlsts 成员集合</para>
        /// </summary>
        /// <param name="memberlsts"></param>
        /// <param name="showMember"></param>
        private void AdddataTopal(List<MembersItem> memberlsts, int showMember)
        {
            controlLst.Clear();
            tabpal.Controls.Clear();

            int j = 0, k = 0;


            for (int i = 0; i < memberlsts.Count; i++)
            {

                USEUserGridItem uSEpicAddName = new USEUserGridItem();
                uSEpicAddName.CurrentRole = CurrentRole;
                uSEpicAddName.Tag = memberlsts[i].role;
                uSEpicAddName.NickName = memberlsts[i].nickname;
                uSEpicAddName.Userid = memberlsts[i].userId;
                ImageLoader2.Instance.DisplayAvatar(memberlsts[i].userId, (bitmap) =>
                {
                    uSEpicAddName.PicHead.Image = bitmap;
                });

                uSEpicAddName.ChangeRole(memberlsts[i].role);
                uSEpicAddName.Margin = new Padding(10, 8, 3, 3);
                uSEpicAddName.PicHead.Click += Pics_Click;

                if (showMember == 0)//是否显示群成员
                {
                    if (memberlsts[i].role == 1 || memberlsts[i].role == 2 || memberlsts[i].userId == Applicate.MyAccount.userId)
                    {
                        controlLst.Add(uSEpicAddName);

                        tabpal.Controls.Add(controlLst[j]);
                        j++;
                    }
                    if (j < 13)
                    {
                        btnSeeMember.Visible = false;
                    }

                    else
                    {
                        btnSeeMember.Visible = true;
                    }
                }
                else
                {
                    if (memberlsts[i].role == 4 && CurrentRole != 1 && memberlsts[i].userId != Applicate.MyAccount.userId)
                    {
                        continue;
                    }
                    controlLst.Add(uSEpicAddName);
                    tabpal.Controls.Add(controlLst[k]);
                    k++;
                }

            }

        }



        private void Pics_Click(object sender, EventArgs e)
        {
            if (GroupInfo.AllowSendCard == 0 && CurrentRole != 1)
            {

                HttpUtils.Instance.ShowTip("当前群组禁止普通成员私聊，不允许查看其他成员信息");
            }
            else
            {
                ImageViewxRoomManager pic = (ImageViewxRoomManager)sender;
                USEUserGridItem uSEpicAddName = (USEUserGridItem)pic.Parent;
                FrmFriendsBasic frmFriendsBasic = new FrmFriendsBasic();
                frmFriendsBasic.ShowUserInfoByRoom(uSEpicAddName.Userid, GroupInfo.UserId, CurrentRole);
                frmFriendsBasic.Show();
            }
        }


        private void btnSeeMember_Click(object sender, EventArgs e)
        {

            FrmMoreMember moreMember = Applicate.GetWindow<FrmMoreMember>();
            if (moreMember == null)
            {
                moreMember = new FrmMoreMember();
            }
            else
            {
                moreMember.Activate();
                return;
            }

            moreMember.SetRoom(GroupInfo);
            moreMember.Role = CurrentRole;
            moreMember.Show();
            moreMember.BringToFront();
            moreMember.isshowmember = showMember;
            moreMember.LoadGroupMember();
        }

        internal void ChangeGroupName(string nickName)
        {
            GroupInfo.NickName = nickName;
            lblTitle.Text = nickName;
        }


        // 发消息按钮
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (SendAction != null)
            {
                SendAction(GroupInfo);
            }
        }
        private int FontWidth(Font font, Control control, string str)
        {
            using (Graphics g = control.CreateGraphics())
            {
                SizeF siF = g.MeasureString(str, font); return (int)siF.Width;
            }
        }

        public void ChangeLine()
        {
            int wid = FontWidth(lblTitle.Font, lblTitle, lblTitle.Text);
            int line = wid / lblTitle.Width;
        }
        /// <summary>
        /// 处理显示不全的问题（）如果高度不够显示一行就不显示
        /// </summary>
        /// 
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangedListRole(string userId, int role)
        {
            if (UIUtils.IsNull(memberlst))
            {
                return;
            }
            //lh 2020/4/8 注释转让群主更新图像
            //USEUserGridItem uSE = new USEUserGridItem();
            //for (int i = 0; i < tabpal.Controls.Count; i++)
            //{
            //     uSE = (USEUserGridItem)tabpal.Controls[i];
            //    if(uSE.Userid== userId)
            //    {
            //        break;
            //    }
            //}

            foreach (var item in memberlst)
            {
                if (string.Equals(item.userId, userId))
                {
                    item.role = role;
                    return;
                }
            }
        }
    }
}
