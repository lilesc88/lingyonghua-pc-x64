using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WinFrmTalk.View;
using WinFrmTalk.Model;
using Newtonsoft.Json;
using WinFrmTalk.Helper.MVVM;
using Newtonsoft.Json.Linq;
using WinFrmTalk.Model.dao;

namespace WinFrmTalk.Controls.CustomControls
{
    public partial class USEGroupSet : UserControl
    {
        private void LoadLanguageText()
        {
            if (!Applicate.ENABLE_MULTI_LANGUAGE)
                return;

            lblOverdueTime.Text = LanguageXmlUtils.GetValue("msg_overdue_time", lblOverdueTime.Text);
            lblOverdueDate.Text = LanguageXmlUtils.GetValue("permanent", lblOverdueDate.Text);
            lblTop.FunctionName = LanguageXmlUtils.GetValue("top_chat", lblTop.FunctionName);
            lblNoDisturbing.FunctionName = LanguageXmlUtils.GetValue("no_disturbing", lblNoDisturbing.FunctionName);
            lblName.FunctionName = LanguageXmlUtils.GetValue("group_name", lblName.FunctionName);
            lblMenge.FunctionName = LanguageXmlUtils.GetValue("frmMagent_title", lblMenge.FunctionName);
            lblNotice.FunctionName = LanguageXmlUtils.GetValue("group_notice", lblNotice.FunctionName);
            lblNickname.FunctionName = LanguageXmlUtils.GetValue("nickname_in_group", lblNickname.FunctionName);
            lblQRCode.FunctionName = LanguageXmlUtils.GetValue("group_QR_code", lblQRCode.FunctionName);
            lblFile.FunctionName = LanguageXmlUtils.GetValue("group_file", lblFile.FunctionName);
            lblDes.FunctionName = LanguageXmlUtils.GetValue("group_details", lblDes.FunctionName);
            tsmHour.Text = LanguageXmlUtils.GetValue("one_hour", tsmHour.Text);
            tsmDay.Text = LanguageXmlUtils.GetValue("one_day", tsmDay.Text);
            tsmWeek.Text = LanguageXmlUtils.GetValue("one_week", tsmWeek.Text);
            tsmMonth.Text = LanguageXmlUtils.GetValue("one_month", tsmMonth.Text);
            tsmSeason.Text = LanguageXmlUtils.GetValue("one_season", tsmSeason.Text);
            tsmYear.Text = LanguageXmlUtils.GetValue("one_year", tsmYear.Text);
            tsmForever.Text = LanguageXmlUtils.GetValue("permanent", tsmForever.Text);
            btnclear.Text = LanguageXmlUtils.GetValue("btn_clear_chat_history", btnclear.Text);
            btnexite.Text = LanguageXmlUtils.GetValue("btn_delete_and_exit", btnclear.Text);
            btnseemore.Text = LanguageXmlUtils.GetValue("show_more_member", btnseemore.Text);
        }

        public USEGroupSet()
        {
            InitializeComponent();
            LoadLanguageText();
            //roomId = friend.roomId;
            //friend = friend.GetByUserId();//刷新一下获取最新的数据库
            //mFriend = friend;
            lblQRCode.lblleft.MouseDown += infoGroupQRCode_MouseDown;
            lblFile.lblleft.MouseDown += infoCard18_MouseDown;
            lblNickname.lblInfo.BringToFront();
            lblName.lblInfo.BringToFront();
            lblDes.MaxLenth = 50;
        }

        private string GroupName;//群名称
        private string GroupTips;//群公告
        private string MynickName;//我在群里的昵称
        private string GroupDiscription;//群组描述

        //当前群人数
        private int RoomUserCount = 0;
        //群人数上限
        private int RoomUserMax = 20;

        private List<Control> controlLst = new List<Control>();//加入面板中数据的集合
        private List<RoomMember> roommemberLst = new List<RoomMember>();//群成员列表
        private string CurrentUserid = Applicate.MyAccount.userId;
        private int CurrentRole;//当前用户角色
        private bool isChangeChatKey;//是否可以重置群组密钥
        private Friend mFriend;
        private LodingUtils loding;//等待符
        private Friend RoleHoste = new Friend();//群主
        int isNeedVerify = 0;//判断是否开启邀请好友验证
        int showMember = 0;//是否显示群成员
        int allowInviteFriend = 0;//是否允许邀请好友
        private RoomMember getnoticeinfo;
        Dictionary<string, Friend> Selectdata = new Dictionary<string, Friend>();//邀请好友的集合

        /// <summary>
        /// 设置当前roon的friend
        /// </summary>
        /// <param name="friend"></param>
        internal void SetRoomData(Friend friend)
        {
            friend = friend.GetByUserId();//刷新一下获取最新的数据库
            mFriend = friend;

        }

        #region 显示等待符
        /// <summary>
        /// 
        /// </summary>
        /// <param name="control">等待符所在的父容器</param>
        private void ShowLodingDialog(Control control)
        {
            this.loding = new LodingUtils { };


            this.loding.parent = control;
            this.loding.Title = LanguageXmlUtils.GetValue("loading", "加载中");
            this.loding.start();
        }
        #endregion 获取界面上的所有数据

        #region 填充群设置页面上的数据
        /// <summary>
        /// 填充群设置页面上的数据
        /// </summary>
        public void FillData()
        {
            LoadGroupData(true);//第一次加载数据

            RegistNotiy();//监听
            RegistAdd();
        }
        #endregion

        #region  注册添加群成员
        /// <summary>
        /// 注册添加群成员
        /// </summary>

        private void RegistAdd()
        {
            Messenger.Default.Register<string>(this, MessageActions.Room_UPDATE_ROOM_ADD, item => Rooom_ADD(item));
        }
        #endregion

        #region 注册群控制消息
        private void RegistNotiy()
        {
            //按回执更新已读消息状态
            Messenger.Default.Register<MessageObject>(this, MessageActions.XMPP_UPDATE_ROOM_CHANGE_MESSAGE, item => Getmonitor(item));

        }
        #endregion


        private void Rooom_ADD(string item)
        {
            LoadGroupData(false);
        }

        #region 接口加载数据

        /// <summary>
        /// 接口加载数据
        /// </summary>
        /// <param name="a">是否是此界面第一次加载数据</param>
        private void LoadGroupData(bool a)
        {
            ShowLodingDialog(this);//显示等待符
            //http get请求获得数据
            HttpUtils.Instance.InitHttp(this);
            //将数据保存
            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/get") //获取群详情
                .AddParams("access_token", Applicate.Access_Token)
                .AddParams("roomId", mFriend.RoomId)
                .AddParams("pageSize", "15")
                 .Build().AddErrorListener((code, err) =>
                 {
                     loding.stop();//关闭等待符
                 })
                 .Execute((sccess, room) =>
                 {
                     if (sccess)
                     {
                         //加载数据
                         FillListData(room, a);//将数据填充到界面中
                     }
                     else
                     {

                     }

                 });
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
                Invoke(new Action(() =>
                {
                    MainNotiy(msg);

                }));
            }
        }
        #region 消息监听
        /// <summary>
        /// 消息监听
        /// </summary>
        /// <param name="msg">传入的消息</param>
        public void MainNotiy(MessageObject msg)
        {
            if (mFriend != null && !string.Equals(msg.ChatJid, mFriend.UserId))
            {
                Console.WriteLine("不属于这个群的控制消息");
                return;
            }


            switch (msg.type)
            {
                case kWCMessageType.RoomNameChange://改群名

                    lblName.FunctionInfo = msg.content;
                    break;
                case kWCMessageType.RoomDismiss://解散

                    //群主解散和主动退出,当我打开群设置页面，我被提出或退出时
                    //我自己退群
                    if (msg.myUserId.Equals(msg.fromUserId))
                    {

                    }
                    else//被踢
                    {
                        FrmSMPGroupSet frmgroupset = (FrmSMPGroupSet)this.Parent;
                        frmgroupset.IsClose = true;
                        frmgroupset.Close();

                    }

                    break;
                case kWCMessageType.RoomExit://退群
                    if (msg.toUserId == Applicate.MyAccount.userId)//我主动退群
                    {
                        if (msg.FromId == CurrentUserid)//打开的是当前界面，将它关闭
                        {
                            FrmSMPGroupSet frmgroupset = (FrmSMPGroupSet)this.Parent;
                            frmgroupset.IsClose = true;
                            frmgroupset.Close();
                        }
                    }
                    else
                    {
                        //监听到有人退群，现在的处理是调一遍接口
                        LoadGroupData(false);
                    }
                    break;
                case kWCMessageType.RoomNotice://公告

                    lblNotice.FunctionInfo = msg.content;
                    break;
                case kWCMessageType.RoomInvite://进群
                    LoadGroupData(false);

                    break;
                case kWCMessageType.RoomMemberNameChange://改群内昵称

                    if (CurrentUserid == msg.fromUserId)
                    {
                        lblNickname.FunctionInfo = msg.content;
                    }
                    break;

                case kWCMessageType.RoomInsideVisiblity://允许显示群成员
                    showMember = Convert.ToInt32(msg.content);
                    mFriend.ShowMember = showMember;
                    if (showMember == 0)//不允许显示群成员
                    {
                        if (CurrentRole == 3 || CurrentRole == 4)//普通成员和隐身人
                        {
                            AdddataTopal(roommemberLst, false);
                        }
                    }
                    else
                    {
                        AdddataTopal(roommemberLst, true);//群主和管理员
                    }
                    break;
                case kWCMessageType.RoomIsVerify://群验证
                    isNeedVerify = Convert.ToInt32(msg.content);//是否开启群验证
                    mFriend.IsNeedVerify = isNeedVerify;//更新数据库
                    mFriend.UpdateNeedVerify();
                    break;
                case kWCMessageType.RoomManagerTransfer://群主转让
                    if (showMember == 0)//不显示群成员，panel中只有当前成员和群主，刷新群主
                    {
                        for (int i = 0; i < controlLst.Count; i++)
                        {
                            USEpicAddName use = (USEpicAddName)controlLst[i];
                            if (msg.toUserId == CurrentUserid)//如果自己被转让为群主
                            {
                                CurrentRole = 1;//当前的角色
                                LoadGroupData(false);
                            }
                            else
                            {
                                if (use.Tag.ToString() == "1")
                                {
                                    use.lblName.Text = msg.toUserName;
                                    ImageLoader.Instance.DisplayAvatar(msg.toUserId, use.pics);
                                    break;
                                }
                            }
                        }

                    }
                    else
                    {
                        if (msg.toUserId == CurrentUserid)//如果自己被转让为群主
                        {
                            CurrentRole = 1;
                            LoadGroupData(false);
                        }
                    }
                    //var member = new RoomMember() { userId = msg.toUserId, roomId = mFriend.RoomId, role = 1 };
                    //member.UpdateRole();
                    //member.userId = msg.fromUserId;
                    //member.role = 1;
                    break;
                case kWCMessageType.RoomAllowMemberInvite://允许普通好友邀请
                    allowInviteFriend = Convert.ToInt32(msg.content);
                    break;
                case kWCMessageType.RoomAdmin:
                    if (msg.toUserId == CurrentUserid)
                    {
                        CurrentRole = 2;
                        LoadGroupData(false);//我自己成为管理员
                    }
                    else
                    {

                    }
                    break;
                case kWCMessageType.RoomAllowUploadFile://允许上传文件
                    int allowupdatefile = Convert.ToInt32(msg.content);
                    mFriend.UpdateAllowUploadFile(allowupdatefile);//保存数据库
                    break;
                case kWCMessageType.RoomUserRecommend:
                    mFriend.AllowSendCard = Convert.ToInt32(msg.content);
                    mFriend.UpdateAllowSendCard(Convert.ToInt32(msg.content));
                    break;
                default:
                    return;
            }
        }
        #endregion

        #region 从接口获得的数据显示在界面中
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keys">j接口获得的字典集合</param>
        /// <param name="isFrist">是不是第一次调取接口</param>
        private void FillListData(Dictionary<string, object> keys, bool isFrist)
        {
            var iconfont = Program.ApplicationFontCollection.Families.Last();
            RoomMember roomMember = new RoomMember { roomId = mFriend.RoomId };
            mFriend.Description = UIUtils.DecodeString(keys, "desc");

            RoomUserMax = UIUtils.DecodeInt(keys, "maxUserSize");
            RoomUserCount = UIUtils.DecodeInt(keys, "userSize");

            roommemberLst = roomMember.TransToMember(keys, mFriend.RoomId);
            mFriend.TransToMember(keys);
            string save = UIUtils.DecodeString(keys, "chatRecordTimeOut");
            //消息过期
            LocalDataUtils.SetStringData(mFriend.UserId + "chatRecordTimeOut" + Applicate.MyAccount.userId, save);
            OverdueDate(save, false);

            // 更新自己在群里面的数据
            string roomByme = UIUtils.DecodeString(keys, "member");
            if (!UIUtils.IsNull(roomByme))
            {
                bool isRefreRecent = false;

                var member = JsonConvert.DeserializeObject<Member>(roomByme);
                CurrentRole = member.role;
                MynickName = member.nickname;

                if (!string.Equals(Applicate.MyAccount.nickname, MynickName))
                {
                    RoomMemberDao.Instance.UpdateNickname(mFriend.RoomId, member.userId, MynickName);
                }

                // 置顶聊天
                if (member.openTopChatTime != mFriend.TopTime)
                {
                    int time = member.openTopChatTime;
                    mFriend.TopTime = time;
                    mFriend.UpdateTopTime(time);
                    isRefreRecent = true;
                }


                // 消息免打扰
                if (member.offlineNoPushMsg != mFriend.Nodisturb)
                {
                    mFriend.Nodisturb = member.offlineNoPushMsg;
                    mFriend.UpdateNodisturb();
                    isRefreRecent = true;
                }

                if (isRefreRecent)
                {
                    Messenger.Default.Send(mFriend, MessageActions.UPDATE_FRIEND_TOP);
                }
            }


            mFriend.IsSecretGroup = UIUtils.DecodeInt(keys, "isSecretGroup");
            mFriend.IsSecretGroup = UIUtils.DecodeInt(keys, "isSecretGroup");

            // 是否是端到端加密群组
            if (mFriend.IsSecretGroup == 1)
            {
                if (CurrentRole == 1)
                {
                    lblQRCode.FunctionName = LanguageXmlUtils.GetValue("reset_key", "重置群组通信密钥");
                    isChangeChatKey = true;
                }
                else
                {
                    lblQRCode.Visible = false;
                }
            }


            // 去刷新群头像
            Messenger.Default.Send(mFriend.UserId, MessageActions.UPDATE_HEAD);//发送刷新头像通知

            //消息过期仅群主可见
            if (CurrentRole != 1)
            {
                panel3.Visible = false;
            }
            //只有群主能解散，其余只能是退出群
            if (CurrentRole == 1)
            {
                btnexite.Text = LanguageXmlUtils.GetValue("btn_dissolve_group", "解散该群");
            }
            else
            {
                btnexite.Text = LanguageXmlUtils.GetValue("btn_delete_and_exit", "删除并退出 ");
            }
            Padding pading = new Padding();
            pading = btnexite.Margin;
            int left = (int)((palgroupCtl.Width - btnexite.Width) * 0.5);

            // btnexite.Margin = new Padding(left, pading.Top, pading.Right, pading.Bottom);

            allowInviteFriend = mFriend.AllowInviteFriend;
            if (isFrist)//第一次从接口获取数据
            {
                lblNotice.lblInfo.Click -= lblNotice.lblInfo_Click;
                //添加按钮

                btnAdd.pics.Click -= btnAdd.pics_Click;//取消点击图片的事件
                btnAdd.pics.Size = new Size(35, 35);

                btnAdd.lblName.Text = LanguageXmlUtils.GetValue("add", "添加");

                btnAdd.lblName.Location = new Point(10, btnAdd.pics.Size.Height + 8);
                btnAdd.lblName.Font = new Font(Applicate.SetFont, 8F);


                btnAdd.pics.Image = WinFrmTalk.Properties.Resources.Add_01;

                btnAdd.pics.Click += BtnAdd_Click1;
                btnAdd.Visible = true;
                //删除按钮

                btnDel.pics.Click -= btnDel.pics_Click;
                //  btnDel.pics.Size = new Size(35, 35);
                btnDel.lblName.Text = LanguageXmlUtils.GetValue("delete", "删除");


                btnDel.lblName.Location = new Point(10, btnDel.pics.Size.Height + 8);
                btnDel.lblName.Font = new Font(Applicate.SetFont, 8F);

                // btnDel.NickName = "删除";
                btnDel.pics.Image = WinFrmTalk.Properties.Resources.Aum;
                btnDel.Visible = true;

                if (CurrentRole == 3 || CurrentRole == 4)//普通成员和影身人
                {
                    btnDel.Visible = false;
                }
                btnDel.pics.Click += BtnDel_Click1;

                //置顶
                if (mFriend.TopTime == 0)
                {
                    lblTop.checkData.Checked = false;
                }
                else
                {
                    lblTop.checkData.Checked = true;
                }

                isNeedVerify = mFriend.IsNeedVerify;//群验证

                //对自定义控件中的子控件绑定，避免点击子控件没有事件响应
                lblFile.lblleft.MouseDown += infoCard18_MouseDown;
                lblMenge.lblleft.MouseDown += infoMenge_MouseDown;
                lblMenge.lblfeatures.MouseDown += infoMenge_MouseDown;

                lblNotice.lblleft.MouseDown += infoNotice_MouseDown;
                lblNotice.lblInfo.MouseDown += infoNotice_MouseDown;
                lblNotice.lblfeatures.MouseDown += infoNotice_MouseDown;

                // infoMenge.MouseHover += InfoMenge_MouseHover;
                //  infoMenge.MouseMove += InfoMenge_MouseMove;

                //消息免打扰
                lblNoDisturbing.checkData.Checked = mFriend.Nodisturb == 1; // LocalDataUtils.GetBoolData(mFriend.userId + "NOT_DISTURB" + CurrentUserid);
                lblTop.checkData.Click += CheckData_Click;
                lblNoDisturbing.checkData.Click += CheckData_Click;

                showMember = mFriend.ShowMember;//显示群成员

                ShowmembertTopal(showMember);

                lblName.FunctionInfo = mFriend.NickName.ToString();
                lblName.txtinfo.Visible = false;
                GroupName = lblName.FunctionInfo;
                //if (CurrentRole == 3 || CurrentRole == 4)
                //{
                //    lblName.lblInfo.Click -= lblName.lblInfo_Click;//普通成员不可修改群名片
                //}
                //else
                //{

                lblName.lblInfo.Click += Txtinfo_Click;
                // }

                roomMember.roomId = mFriend.RoomId;
                roomMember.userId = CurrentUserid;
                roomMember.GetRoomMember();

                //群描述

                lblDes.FunctionInfo = mFriend.Description;
                GroupDiscription = lblDes.FunctionInfo;
                GroupTips = lblNotice.lblInfo.Text;
                //昵称
                lblNickname.txtinfo.Visible = false;
                lblNickname.lblInfo.Click += Txtinfo_Click1;
                lblNickname.txtinfo.ReadOnly = true;

                //    lblNickname.txtinfo.KeyPress += txtRemarks_KeyPress;

                getnoticeinfo = roomMember;

                lblDes.txtinfo.KeyPress += lblDes_KeyPress;

                if (roomMember.NoticeLst.Count == 0 || roomMember.NoticeLst == null)
                {
                    //没有公告的普通成员和隐身人都不可点击
                    lblNotice.lblInfo.Text = LanguageXmlUtils.GetValue("none_notice", "暂无公告");

                }

                else
                {
                    lblNotice.FunctionInfo = roomMember.NoticeLst[0].text;
                }
                if (CurrentRole == 3 || CurrentRole == 4)
                {
                    lblNotice.lblInfo.Click -= lblNotice.lblInfo_Click;

                    lblNotice.lblleft.Click -= lblNotice.lblInfo_Click;

                    lblNotice.lblfeatures.Click -= lblNotice.lblInfo_Click;
                    lblNotice.txtinfo.ReadOnly = true;
                    lblNotice.txtinfo.Visible = false;
                }
                //判断
                if (MynickName != null)
                {
                    lblNickname.FunctionInfo = MynickName.ToString();
                }

            }
            else//不是首次加载
            {
                // AdddataTopal(roommemberLst, true);
                ShowmembertTopal(showMember);
            }

        }

        private void Txtinfo_Click1(object sender, EventArgs e)
        {


            FrmSMPGroupSet frmprent = this.Parent as FrmSMPGroupSet;
            frmprent.IsClose = false;


            FrmMyColleagueEidt frm = Applicate.GetWindow<FrmMyColleagueEidt>();
            if (frm != null)
            {
                frm.Activate();
                return;
            }

            frm = new FrmMyColleagueEidt();
            frm.maxLength = 15;
            frm.FormClosed += (sen, eve) =>
            {
                if (frmprent.IsClose != true)
                    frmprent.IsClose = true;
                lblName.txtinfo.Visible = false;
            };
            
            frm.Location = new Point(this.Location.X + (this.Width - frm.Width) / 2, this.Location.Y + (this.Height - frm.Height) / 2);
            frm.ColleagueName((data) =>
            {
                // frm.Close();
                lblNickname.lblInfo.Text = data;
                lblNickname.txtinfo.Text = data;
                MynickName = data;
                // frmprent.IsClose = true;
                ShowLodingDialog(panel1);
                frmprent.IsClose = true;
                HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/member/update")
                     .AddParams("access_token", Applicate.Access_Token)
                     .AddParams("roomId", mFriend.RoomId)
                     .AddParams("userId", CurrentUserid)
                     .AddParams("nickname", MynickName)
                  .Build().AddErrorListener((code, err) =>
                 {
                     loding.stop();
                     frm.Close();
                 })
              .Execute((sccess, su) =>
              {

                  if (sccess)
                  {
                      loding.stop();
                      //保存数据库

                      RoomMemberDao.Instance.UpdateCardname(mFriend.RoomId, CurrentUserid, MynickName);

                      lblNickname.txtinfo.Visible = false;

                      frm.Close();
                  }
              });

                frmprent.Activate();
                frmprent.IsClose = true;
            });

            string title = LanguageXmlUtils.GetValue("title_modify_groupnickname", "修改群内昵称");
            string name = LanguageXmlUtils.GetValue("name_group_nickname", "群内昵称", true);
            frm.ShowThis(title, name, false);

        }

        private void Txtinfo_Click(object sender, EventArgs e)
        {

            FrmSMPGroupSet frmprent = this.Parent as FrmSMPGroupSet;
            frmprent.IsClose = false;
            if (CurrentRole == 3 || CurrentRole == 4)
            {
                var main = HttpUtils.Instance.FindFrm(typeof(FrmMain));
                main.ShowTip("非管理人员不允许修改");
                frmprent.IsClose = true;
                return;
            }
            FrmMyColleagueEidt frm = new FrmMyColleagueEidt();
            //  frm.NameEdit = SelectItem.Position;
            frm.FormClosed += (sen, eve) =>
            {
                if (frmprent.IsClose != true)
                    frmprent.IsClose = true;
                lblName.txtinfo.Visible = false;
            };
            frm.maxLength = 15;
            frm.Location = new Point(this.Location.X + (this.Width - frm.Width) / 2, this.Location.Y + (this.Height - frm.Height) / 2);
            frm.ColleagueName((data) =>
            {
                // frm.Close();
                lblName.lblInfo.Text = data;
                lblName.txtinfo.Text = data;
                GroupName = data;
                // frmprent.IsClose = true;
                ShowLodingDialog(panel1);
                frmprent.IsClose = true;
                HttpUtils.Instance.InitHttp(this);
                HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/update") //获取群详情
              .AddParams("access_token", Applicate.Access_Token)
              .AddParams("roomId", mFriend.RoomId)
               .AddParams("roomName", GroupName)
                 .Build().AddErrorListener((code, err) =>
                 {
                     loding.stop();
                     frm.Close();
                     //  frmprent.IsClose = true;
                 })
              .Execute((sccess, su) =>
              {

                  if (sccess)
                  {
                      loding.stop();
                      mFriend.NickName = GroupName;
                      mFriend.UpdateNickName();//更新数据库中的群昵称
                      lblName.txtinfo.Visible = false;


                      frm.Close();
                  }
                  else
                  {

                  }

              }
              );
                frmprent.IsClose = true;
            }

            );

            string title = LanguageXmlUtils.GetValue("title_modify_groupname", "修改群名称");
            string name = LanguageXmlUtils.GetValue("name_group_name", "群组名称", true);
            frm.ShowThis(title, name, false);



        }

        private void Frm_FormClosed(object sender, FormClosedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ClearHeadChehe(string roomJid)
        {
            int roomJidCode = UIUtils.HashCode(roomJid);
            int a = Math.Abs(roomJidCode % 10000);
            int b = Math.Abs(roomJidCode % 20000);

            string roomUrl = Applicate.URLDATA.data.downloadAvatarUrl + "avatar/o/" + a + "/" + b + "/" + roomJid + ".jpg";
            ImageCacheManager.Instance.ClearImageCache(roomUrl);
        }

        private void CheckData_Click(object sender, EventArgs e)
        {

            USEToggle lollipop = (USEToggle)sender;
            string tag = lollipop.Parent.Tag.ToString();
            switch (tag)
            {

                case "2":// 消息免打扰
                    Console.WriteLine(LanguageXmlUtils.GetValue("no_disturbing", "消息免打扰") + lollipop.Checked);
                    mFriend.Nodisturb = lollipop.Checked ? 1 : 0;
                    mFriend.UpdateNodisturb();
                    Messenger.Default.Send(mFriend, MessageActions.UPDATE_FRIEND_DISTURB);
                    //LocalDataUtils.SetBoolData(mFriend.userId + "NOT_DISTURB" + CurrentUserid, lollipop.Checked);
                    RequestRoomSetting(mFriend, 0, lollipop.Checked);
                    break;

                case "3":// 消息置顶

                    int time = lollipop.Checked ? TimeUtils.CurrentIntTime() : 0;
                    mFriend.UpdateTopTime(time);
                    mFriend.TopTime = time;
                    Messenger.Default.Send(mFriend, MessageActions.UPDATE_FRIEND_TOP);
                    RequestRoomSetting(mFriend, 1, lollipop.Checked);
                    break;
            }
        }


        #endregion

        
        /// <summary>
        /// 保存消息免打扰-阅后即焚-置顶状态到服务器
        /// </summary>
        /// <param name="friend">群组</param>
        /// <param name="type">0== 免打扰 1== 置顶  </param>
        private void RequestRoomSetting(Friend friend, int type, bool isOpen)
        {
            string topvalue = isOpen ? "1" : "0";

            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/member/setOfflineNoPushMsg")
                   .AddParams("access_token", Applicate.Access_Token)
                   .AddParams("userId", Applicate.MyAccount.userId)
                   .AddParams("roomId", mFriend.RoomId)
                   .AddParams("type", type.ToString())
                   .AddParams("offlineNoPushMsg", topvalue)
                   .NoErrorTip()
                   .Build().Execute(null);
        }


        #region  不是首次显示从接口获取的数据(只显示成员信息，不显示群控制消息)
        /// <summary>CurrentRole
        /// 
        /// </summary>
        /// <param name="showmember">1.显示群成员0.不显示群成员</param>
        public void ShowmembertTopal(int showmember)
        {
            if (CurrentRole == 3 || CurrentRole == 4)//普通群成员，隐身人
            {
                if (showMember == 0)
                {
                    AdddataTopal(roommemberLst, false);//不显示群成员
                }
                else
                {
                    AdddataTopal(roommemberLst, true);//显示群成员
                }
            }
            else//管理员和群主
            {
                AdddataTopal(roommemberLst, true);//显示群成员
            }

        }
        #endregion

        #region 删除群成员
        //点击删除按钮
        private void BtnDel_Click1(object sender, EventArgs e)
        {
            var frm = Applicate.GetWindow<FrmMoreMember>();
            if (frm == null)
            {
                FrmMoreMember moreMember = new FrmMoreMember();//群成员
                moreMember.SetRoom(mFriend);
                moreMember.issum = true;
                moreMember.Role = CurrentRole;
                moreMember.Text = LanguageXmlUtils.GetValue("delete_member", "删除群成员");
                moreMember.Show();
                moreMember.isshowmember = showMember;
                moreMember.BringToFront();
                moreMember.LoadGroupMember();
            }
            else
            {
                frm.Activate();
            }
        }
        #endregion
        //添加按钮事件

        #region 添加群成员
        private void BtnAdd_Click1(object sender, EventArgs e)
        {
            FrmSMPGroupSet frm = this.Parent as FrmSMPGroupSet;
            if (CurrentRole == 4)
            {
                //获取 role再进行判断

                frm.IsClose = false;

                if (HttpUtils.Instance.ShowPromptBox("隐身人不允许邀请好友"))
                {
                    frm.IsClose = true;
                }
            }
            else if (CurrentRole == 3 && allowInviteFriend == 0)
            {
                //获取 role再进行判断z     //群主

                frm.IsClose = false;

                if (HttpUtils.Instance.ShowPromptBox("普通成员不允许邀请好友"))
                {
                    frm.IsClose = true;
                }
            }
            else if (mFriend.IsLostKeyGroup == 1)
            {
                frm.IsClose = false;

                FrmBase main = HttpUtils.Instance.FindFrm(typeof(FrmMain));
                if (main.ShowPromptBox("密钥丢失状态不能邀请好友"))
                {
                    frm.IsClose = true;
                }
            }
            else
            {
                InviteTogroup();//邀请好友
            }

        }
        #endregion
  #region 开关按钮
        //选择按钮发生改变
        private void CheckData_CheckedChanged(object sender, EventArgs e)
        {
            LollipopToggle lollipop = (LollipopToggle)sender;
            string tag = lollipop.Parent.Tag.ToString();
            switch (tag)
            {

                case "2":// 消息免打扰
                    Console.WriteLine("消息免打扰 CheckData_CheckedChanged");
                    //Newsdisturb = lollipop.Checked ? 1 : 0;
                    //LocalDataUtils.SetBoolData(mFriend.userId + "NOT_DISTURB" + CurrentUserid, lollipop.Checked);
                    break;

                case "3":// 消息置顶
                    int time = lollipop.Checked ? TimeUtils.CurrentIntTime() : 0;
                    mFriend.UpdateTopTime(time);
                    Messenger.Default.Send(mFriend, MessageActions.UPDATE_FRIEND_TOP);
                    break;
            }

        }
        #endregion        //键盘输入
        #region 修改参数后输入回车
        private void txtRemarks_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (CurrentRole == 4)
            {
                var main = HttpUtils.Instance.FindFrm(typeof(FrmMain));
                main.ShowTip("隐身人不允许修改昵称");
                return;
            }
            if (e.KeyChar == 13)//回车
            {
                //SendKeys.Send("{tab}");
                txtRemarks_Leave(sender, e);

            }
        }
        private void lblDes_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (CurrentRole != 1)
            {
                var main = HttpUtils.Instance.FindFrm(typeof(FrmMain));
                main.ShowTip("非群主不允许修改不允许修改");
                return;
            }
            if (e.KeyChar == 13)//回车
            {
                //SendKeys.Send("{tab}");
                txtRemarks_Leave(sender, e);

            }
        }
        #endregion

        #region  回车后更新数据到界面
        //更新数据

        private void txtRemarks_Leave(object sender, EventArgs e)
        {
            TextBox text = (TextBox)sender;
            text.Visible = false;
            string a = text.Text.ToString();
            string tag = text.Parent.Tag.ToString();
            InfoCard info = (InfoCard)text.Parent;
            // info.FunctionInfo = a;
            if (text.Text != info.FunctionInfo)
            {
                switch (tag)
                {
                    case "1"://群名称
                        GroupName = a;
                        updategroupname();
                        break;
                    case "2"://群公告
                        GroupTips = a;
                        UpdatePlacard();
                        break;
                    case "3"://昵称
                        MynickName = a;
                        Console.Write("昵称为" + a);
                        updateNickName();
                        break;
                    case "4"://群组描述
                        GroupDiscription = a;
                        updatedesc();
                        break;
                }

                info.FunctionInfo = a.ToString();
            }
            else
            {

            }
        }
        #endregion

        #region 修改当前用户群内昵称
        private void updateNickName()
        {

            ShowLodingDialog(panel1);//显示等待符
            HttpUtils.Instance.InitHttp(this);

            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/member/update")
                .AddParams("access_token", Applicate.Access_Token)
                .AddParams("roomId", mFriend.RoomId)
                .AddParams("userId", CurrentUserid)
                .AddParams("nickname", MynickName)
                .Build().Execute((sccess, data) =>
                {
                    //停止等待符
                    loding.stop();
                    if (sccess)
                    {
                        //保存数据库
                        RoomMemberDao.Instance.UpdateCardname(mFriend.RoomId, CurrentUserid, MynickName);
                    }
                });
            lblDes.txtinfo.Visible = false;
        }
        #endregion

        // 更新群公告
        private void UpdatePlacard()
        {
            //获取群详情
            ShowLodingDialog(panel1);

            HttpUtils.Instance.InitHttp(this);
            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/update")
                .AddParams("access_token", Applicate.Access_Token)
                .AddParams("roomId", mFriend.RoomId)
                .AddParams("notice", GroupTips)
                .Build().Execute((sccess, data) =>
                {
                    loding.stop();
                });
        }
        #region 更新群描述
        private void updatedesc()
        {

            ShowLodingDialog(panel1);
            HttpUtils.Instance.InitHttp(this);
            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/update") //获取群详情
          .AddParams("access_token", Applicate.Access_Token)
          .AddParams("roomId", mFriend.RoomId)
          .AddParams("desc", GroupDiscription)

             .Build().AddErrorListener((code, err) =>
             {
                 loding.stop();
             })
          .Execute((sccess, data) =>
          {

              if (sccess)
              {
                  loding.stop();
                  // mFriend.description = GroupName;
                  // mFriend.UpdateNickName();//更新数据库中的群昵称

              }
              else
              {

              }

          }
          );
            lblDes.txtinfo.Visible = false;
        }
        #endregion

        #region 更新群组名称
        private void updategroupname()
        {

            ShowLodingDialog(panel1);

            HttpUtils.Instance.InitHttp(this);
            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/update") //获取群详情
          .AddParams("access_token", Applicate.Access_Token)
          .AddParams("roomId", mFriend.RoomId)
           .AddParams("roomName", GroupName)
             .Build().AddErrorListener((code, err) =>
             {
                 loding.stop();
             })
          .Execute((sccess, data) =>
          {

              if (sccess)
              {
                  loding.stop();
                  mFriend.NickName = GroupName;
                  mFriend.UpdateNickName();//更新数据库中的群昵称
                  lblName.txtinfo.Visible = false;


              }
              else
              {

              }

          }
          );
            //   infoName.txtinfo.Visible = false;
        }
        #endregion


        #region 邀请好友
        /// <summary>
        /// 是取消分页加载,还是取数据库的数据？
        /// </summary>
        private void InviteTogroup()
        {
            //从数据库获取到的群成员
            List<RoomMember> memberlst = RoomMemberDao.Instance.GetRommMemberListByHead(mFriend.RoomId);


            Frminvitefriend frmSelect = Applicate.GetWindow<Frminvitefriend>();
            if (frmSelect == null)
            {
                frmSelect = new Frminvitefriend();
                frmSelect.SetRoomUserSize(RoomUserCount, RoomUserMax);
            }
            else
            {
                frmSelect.SetRoomUserSize(RoomUserCount, RoomUserMax);
                frmSelect.Activate();
                return;
            }

            frmSelect.StartPosition = FormStartPosition.CenterScreen;
            bool isEncrypt = mFriend.IsSecretGroup == 1;
            //由于roommemberLst的大数量为15会导致删除出现重复
            frmSelect.InviteFriendToRoom(memberlst, isEncrypt);

            frmSelect.AddConfrmListener((Selectdata) =>
            {
                ShowLodingDialog(panel2);

                if (isNeedVerify == 0 || CurrentRole == 1)
                {
                    List<string> datas = new List<string>();
                    List<Friend> friends = new List<Friend>();
                    foreach (var item in Selectdata.Keys)
                    {
                        datas.Add(item);
                        friends.Add(Selectdata[item]);
                    }

                    RequestInviteContact(mFriend.RoomId, datas, friends);
                }//开启了群验证
                else
                {
                    FrmROOMVerify frmROOMVerify = new FrmROOMVerify();
                    frmROOMVerify.ShowDialog();
                    List<Friend> friendlst = new List<Friend>();//添加好友的集合
                    if (frmROOMVerify.DialogResult == DialogResult.OK)
                    {
                        string Reson = frmROOMVerify.textReson.Text;//邀请好友的原因
                        foreach (var item in Selectdata.Values)
                        {
                            friendlst.Add(item);
                        }
                        //获取群主
                        ShiKuManager.SendRoomverification(RoleHoste, friendlst, Reson, mFriend.UserId);//发消息
                    }
                }

            });
        }
        #endregion

        #region 请求邀请好友
        public void RequestInviteContact(string roomId, List<string> datas, List<Friend> friends)
        {
            string userids = JsonConvert.SerializeObject(datas);

            int isSecretGroup = mFriend.IsSecretGroup;

            Dictionary<string, string> value = new Dictionary<string, string>();
            value.Add("access_token", Applicate.Access_Token);
            value.Add("roomId", roomId);
            value.Add("text", userids);
            value.Add("isSecretGroup", isSecretGroup.ToString());

            if (1 == isSecretGroup)
            {
                string chatkey = SecureChatUtil.DecryptChatkey(mFriend.UserId, mFriend.ChatKeyGroup, Applicate.API_KEY);
                string memkeys = GetMembersKey(friends, chatkey);
                value.Add("keys", memkeys);
            }

            //邀请群成员
            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/member/update")
                .AddParams(value)
                .Build().Execute((sccess, data) =>
                {
                    loding.stop();
                    if (sccess)
                    {
                        var str = UIUtils.DecodeString(data, "data");
                        string names = string.Empty;
                        // 保存群成员到数据库
                        foreach (var item in friends)
                        {
                            if (str.Contains(item.UserId))
                            {
                                names += "," + item.NickName;
                            }

                            // 邀请两种情况，不在群组已经成功邀请，在群组的走到这里说明本地没有
                            RoomMember member = item.ToRoomMember(roomId);
                            RoomMemberDao.Instance.InsertOrUpdate(member);
                        }

                        // 刷新群设置成员
                        Messenger.Default.Send(roomId, MessageActions.Room_UPDATE_ROOM_ADD);
                        if (!UIUtils.IsNull(names))
                        {
                            names = names.Substring(1, names.Length - 1);
                            HttpUtils.Instance.ShowPromptBox("下列群成员已在群组中:\n" + names);
                        }

                    }
                });
        }



        // 获取群成员的 ras 公钥集合json
        private string GetMembersKey(List<Friend> select, string chatKey)
        {
            JObject json = new JObject();
            foreach (var friend in select)
            {
                string rskkey = RSA.EncryptBase64Pk1(chatKey, friend.RsaPublicKey);
                json.Add(friend.UserId, rskkey);
            }

            return json.ToString();
        }

        #endregion

        //查看更多，显示群成员界面
        private void btnSeeMore_Click(object sender, EventArgs e)
        {
            var moreMember = Applicate.GetWindow<FrmMembersLst>();
            if (moreMember == null)
            {
                moreMember = new FrmMembersLst();//群成员

                moreMember.setdata(mFriend);
                moreMember.issum = false;
                //  moreMember.Role = CurrentRole;
                moreMember.isshowmember = showMember;
                moreMember.Show();
                moreMember.BringToFront();
                moreMember.LoadGroupMember();
            }
            else
            {
                moreMember.Activate();
                return;
            }

        }

        #region 打开群管理

        /// <summary>
        /// 点击群管理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void infoMenge_MouseDown(object sender, MouseEventArgs e)
        {

            //if (CurrentRole == 3 || CurrentRole == 4)
            //{

            //}
            //else
            //{
            //如果当前群组打开后就不再打开反之就要打开

            //避免同一窗体多次被点击

            if (Applicate.userlst.Count == 0)
            {
                Applicate.userlst.Add(mFriend.UserId);
                Applicate.isopen = true;
            }
            else
            {
                if (Applicate.userlst.Contains(mFriend.UserId))
                {
                    Applicate.isopen = false;
                }
                else
                {
                    Applicate.userlst.Add(mFriend.UserId);
                    Applicate.isopen = true;
                }
            }

            //
            if (Applicate.isopen)
            {

                FrmMagent frmMagent = new FrmMagent();
                // frmMagent.TopMost = true;

                frmMagent.SetRoomData(mFriend);

                frmMagent.BringToFront();
                frmMagent.LoadData();//加载数据
                                     // frmMagent.Show();

            }


            // }
        }
        #endregion

        #region 打开群文件
        /// <summary>
        /// 群文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void infoCard18_MouseDown(object sender, MouseEventArgs e)
        {
            var fileListFrm = HttpUtils.Instance.FindFrm(typeof(FrmGroupFileList));

            if (fileListFrm == null)
            {
                FrmGroupFileList frmGroupFileList = new FrmGroupFileList();
                frmGroupFileList.ShowRoomFileList(mFriend.RoomId, mFriend);
            }
            else
            {
                var frm = fileListFrm as FrmGroupFileList;
                frm.ShowRoomFileList(mFriend.RoomId, mFriend, false);
            }
        }
        #endregion

        #region 打开群公告

        /// <summary>
        ///  群公告
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void infoNotice_MouseDown(object sender, MouseEventArgs e)
        {
            if (getnoticeinfo.NoticeLst.Count == 0 || getnoticeinfo.NoticeLst == null)
            {


                if (CurrentRole == 3 || CurrentRole == 4)
                {
                    var main = HttpUtils.Instance.FindFrm(typeof(FrmMain));
                    main.ShowTip("暂无公告");

                    return;

                }
            }


            var fileListFrm = HttpUtils.Instance.FindFrm(typeof(FrmGrouptips));

            if (fileListFrm == null)
            {
                FrmGrouptips grouptips = new FrmGrouptips();
                grouptips.SetData(mFriend);
                grouptips.CurrentRole = CurrentRole;
                grouptips.Show();
                grouptips.LoadData();
            }
            else
            {
                // 窗口已被打开
                var frm = fileListFrm as FrmGrouptips;

                if (string.Equals(frm.roomId, mFriend.RoomId))
                {
                    // 激活窗口

                }
                else
                {
                    // 刷新窗口 数据
                    frm.SetData(mFriend);
                    frm.CurrentRole = CurrentRole;
                    frm.LoadData();
                }
            }

        }
        #endregion

        #region 清空聊天记录
        /// <summary>
        /// 清空聊天记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblClearChat_Click(object sender, EventArgs e)
        {
            FrmSMPGroupSet frm = this.Parent as FrmSMPGroupSet;
            frm.IsClose = false;
            if (Applicate.GetWindow<FrmMain>().ShowPromptBox("确认删除聊天记录？此操作将会删除服务器上的聊天数据，无法漫游"))
            {

                //调用接口
                HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/member/setBeginMsgTime"). //删除消息
                    AddParams("access_token", Applicate.Access_Token).
                    AddParams("roomId", mFriend.RoomId).
                    Build().ExecuteJson<object>((sccess, obj) =>   //返回值说明： text：加密后的内容
                    {

                    });

                // 修改禅道11363 2020-3-18 18:39:52
                var text = Applicate.GetWindow<FrmShowText>();
                if (text != null)
                {
                    text.Close();
                }

                MessageObject messageObject = new MessageObject() { FromId = Applicate.MyAccount.userId, toUserId = mFriend.UserId, ToId = mFriend.UserId };

                mFriend.UpdateClearMessageState(1);

                MsgRoamTaskDao.Instance.DeleteTaskAll(mFriend.UserId);

                Messenger.Default.Send(mFriend.UserId, token: EQFrmInteraction.ClearFdMsgsSingle);


            }

            frm.Activate();
            frm.IsClose = true;
        }
        #endregion
        //添加到数据到面板中
        /// <summary>
        /// <para>shshowMember是否显示群成员</para>
        ///<para>memberlsts 成员集合</para>
        /// </summary>
        /// <param name="memberlsts">群成员的集合</param>
        /// <param name="showMember">显示群成员</param>
        private void AdddataTopal(List<RoomMember> memberlsts, bool showMember)
        {
            #region 群管理，删除，群描述是否可见，
            if (CurrentRole == 1 || CurrentRole == 2)//群主和管理员
            {
                lblMenge.Visible = true;
            }
            else
            {
                lblMenge.Visible = false; ;
            }
            //if (CurrentRole == 1)
            //{
            //    lblDes.Visible = true;
            //}
            //else
            //{
            //    lblDes.Visible = false;
            //}
            int membercount = 0;

            if (btnDel.Visible == false)//删除不可见最多显示7个群成员，删除可见最多显示六个
            {
                membercount = memberlsts.Count > 7 ? 7 : memberlsts.Count;
                btnseemore.Visible = memberlsts.Count > 7 ? true : false;
            }
            else
            {
                membercount = memberlsts.Count > 6 ? 6 : memberlsts.Count;
                btnseemore.Visible = memberlsts.Count > 6 ? true : false;
            }
            #endregion
            tabpalMove(controlLst);//清除面板的数据
            controlLst.Clear();
            #region 向面板中添加数据
            int j = 0, k = 0;
            for (int i = 0; i < membercount; i++)
            {

                USEpicAddName uSEpicAddName = new USEpicAddName();

                uSEpicAddName.pics.Size = new Size(35, 35);
                uSEpicAddName.Size = new Size(50, 58);

                //uSEpicAddName.lblName.Location = new Point(0, uSEpicAddName.pics.Size.Height + 8);
                uSEpicAddName.lblName.Font = new Font(Applicate.SetFont, 8F);
                uSEpicAddName.Tag = memberlsts[i].role;

                uSEpicAddName.NickName = memberlsts[i].GetSetname();
                uSEpicAddName.CurrentRole = CurrentRole;
                uSEpicAddName.Userid = memberlsts[i].userId;
                ImageLoader.Instance.DisplayAvatar(memberlsts[i].userId, uSEpicAddName.pics);

                //如果触发click事件时，窗口不关闭
                uSEpicAddName.pics.Click -= uSEpicAddName.pics_Click;
                uSEpicAddName.pics.Click += Pics_Click;


                uSEpicAddName.Margin = new Padding(3, 5, 3, 5);

                if (memberlsts[i].role == 1)
                {
                    RoleHoste.UserId = memberlsts[i].userId;
                    if (RoleHoste.ExistsFriend())
                    {
                        RoleHoste = RoleHoste.GetByUserId();
                    }
                    else
                    {
                        RoleHoste.NickName = memberlsts[i].nickName;
                    }

                }
                if (!showMember)//是否显示群成员
                {
                    if (memberlsts[i].role == 1 || memberlsts[i].role == 2 || memberlsts[i].userId == CurrentUserid)
                    {
                        controlLst.Add(uSEpicAddName);

                        tabMember.Controls.Add(controlLst[j]);
                        j++;
                    }
                    if (roommemberLst.Count > 7)
                    {
                        btnseemore.Visible = true;//管理员可能有多个 
                    }
                    else
                    {
                        btnseemore.Visible = false;
                    }

                }
                else
                {
                    if (memberlsts[i].role == 4 && CurrentRole != 1 && memberlsts[i].userId != CurrentUserid)
                    {
                        continue;
                    }
                    controlLst.Add(uSEpicAddName);
                    tabMember.Controls.Add(controlLst[k]);
                    k++;
                }

            }
            #endregion
            changePanelHeigh(controlLst.Count);//动态显示两个panel的高度
        }

        /// <summary>
        /// 点击群图像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Pics_Click(object sender, EventArgs e)
        {
            if (mFriend.AllowSendCard == 0 && CurrentRole != 1)
            {
                HttpUtils.Instance.ShowTip("当前群组禁止普通成员私聊，不允许查看其他成员信息");
            }
            else
            {
                RoundPicBox pic = (RoundPicBox)sender;
                USEpicAddName uSEpicAddName = (USEpicAddName)pic.Parent;
                FrmSMPGroupSet frm = this.Parent as FrmSMPGroupSet;
                frm.IsClose = false;
                FrmFriendsBasic frmFriendsBasic = new FrmFriendsBasic();
                frmFriendsBasic.ShowUserInfoByRoom(uSEpicAddName.Userid, mFriend.UserId, CurrentRole);//显示群图像卡片
                frmFriendsBasic.frmSMP = frm;
                frmFriendsBasic.Show();
                frm.IsClose = true;
            }

            //throw new NotImplementedException();
        }

        /// <summary>
        /// 移除panel中的控件
        /// </summary>
        /// <param name="conList"></param>
        private void tabpalMove(List<Control> conList)
        {
            for (int i = 0; i < conList.Count; i++)
            {
                tabMember.Controls.Remove(conList[i]);
            }
        }

        /// <summary>
        /// 群二维码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void infoGroupQRCode_MouseDown(object sender, MouseEventArgs e)
        {

            if (isChangeChatKey)
            {
                // 重置群组密钥
                FrmSMPGroupSet frm = this.Parent as FrmSMPGroupSet;
                frm.IsClose = false;

                // 先去获取所有群成员的rsa公钥 然后重新加密上传到服务器
                if (HttpUtils.Instance.ShowPromptBox("重置群组通信密钥，服务器上该群组的聊天记录将被清空"))
                {
                    ShowLodingDialog(panel1);
                    RequestMembersRsaPublicKey();
                }
                else
                {
                    frm.IsClose = true;
                }
            }
            else
            {
                FrmQRCode frm = new FrmQRCode();
                Control parent = Applicate.GetWindow<FrmMain>();
                frm.Location = frm.Location = new Point(parent.Location.X + (parent.Width - frm.Width) / 2, parent.Location.Y + (parent.Height - frm.Height) / 2);//居中
                frm.RoomShow(mFriend.RoomId);

            }
        }


        /// <summary>
        /// 请求所有群成员的rsa公钥
        /// </summary>
        private void RequestMembersRsaPublicKey()
        {
            //获取所有群成员的公钥
            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/getAllMemberRsaPublicKey")
               .AddParams("roomId", mFriend.RoomId)
               .Build().Execute((sucess, data) =>
               {
                   if (sucess)
                   {
                       RequestResetChatKey(data);
                   }
                   else
                   {
                       FrmSMPGroupSet frm = this.Parent as FrmSMPGroupSet;
                       frm.IsClose = true;
                       HttpUtils.Instance.ShowTip("重置失败");
                       loding.stop();
                   }
               });
        }

        /// <summary>
        /// 重置密钥
        /// </summary>
        private void RequestResetChatKey(Dictionary<string, object> memberKeys)
        {
            string ChatKey = Guid.NewGuid().ToString("N");
            string keysStr = GetMembersKey(memberKeys, ChatKey);

            // 重置密钥
            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/resetGroupChatKey")
               .AddParams("roomId", mFriend.RoomId)
               .AddParams("keys", keysStr)
               .Build().Execute((sucess, data) =>
               {
                   FrmSMPGroupSet frm = this.Parent as FrmSMPGroupSet;
                   frm.IsClose = true;
                   loding.stop();
                   if (sucess)
                   {
                       HttpUtils.Instance.ShowTip("重置成功");
                       mFriend.ChatKeyGroup = ChatKey;
                       mFriend.UpdateChatKeyGroup(ChatKey);
                   }
               });

        }

        // 获取群成员的 ras 公钥集合json
        private string GetMembersKey(Dictionary<string, object> data, string chatKey)
        {
            JObject json = new JObject();

            foreach (KeyValuePair<string, object> item in data)
            {

                string rasPublickey = item.Value.ToString();
                string enkey = RSA.EncryptBase64Pk1(chatKey, rasPublickey);
                json.Add(item.Key, enkey);
            }

            return json.ToString();
        }

        /// <summary>
        /// 过期时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblOverdueDate_Click(object sender, EventArgs e)
        {
            cmsOverdueDate.Show(lblOverdueDate, 0, lblOverdueDate.Height);
        }
        /// <summary>
        /// 消息过期事件调取接口
        /// </summary>
        /// <param name="date"></param>
        private void HttpSubDate(string date)
        {
            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/update")//更新群
                .AddParams("access_token", Applicate.Access_Token)
                .AddParams("chatRecordTimeOut", date)
                .AddParams("roomId", mFriend.RoomId)
                .Build().Execute((sucess, data) =>
                {
                    if (sucess)
                    {
                        OverdueDate(date, true);
                        LogUtils.Log("修改成功");
                    }
                });
        }


        /// <summary>
        /// 设置消息过期内容
        /// </summary>
        /// <param name="date"></param>
        private void OverdueDate(string date, bool a)
        {

            string save = date;
            switch (date)
            {
                case "-1":
                    lblOverdueDate.Text = LanguageXmlUtils.GetValue("permanent", "永不");
                    save = "0";
                    break;
                case "0":
                    lblOverdueDate.Text = LanguageXmlUtils.GetValue("permanent", "永不");
                    break;
                case "0.04":
                    lblOverdueDate.Text = LanguageXmlUtils.GetValue("one_hour", "1小时");
                    break;
                case "1":
                case "1.0":
                    lblOverdueDate.Text = LanguageXmlUtils.GetValue("one_day", "1天");
                    break;
                case "7":
                case "7.0":
                    lblOverdueDate.Text = LanguageXmlUtils.GetValue("one_week", "1周");
                    break;
                case "30":
                case "30.0":
                    lblOverdueDate.Text = LanguageXmlUtils.GetValue("one_week", "1月");
                    break;
                case "90":
                case "90.0":
                    lblOverdueDate.Text = LanguageXmlUtils.GetValue("one_season", "1季");
                    break;
                case "365":
                case "365.0":
                    lblOverdueDate.Text = LanguageXmlUtils.GetValue("one_year", "1年");
                    break;
                default:
                    LogUtils.Log("修改失败");
                    save = "0";
                    break;
            }
            if (a)
            {
                LocalDataUtils.SetStringData(mFriend.UserId + "chatRecordTimeOut" + Applicate.MyAccount.userId, save);
            }

        }
        #region 消息过期时间

        /// <summary>
        /// 永久
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmForever_Click(object sender, EventArgs e)
        {
            HttpSubDate("-1");
        }
        /// <summary>
        /// 一小时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmHour_Click(object sender, EventArgs e)
        {
            HttpSubDate("0.04");
        }
        /// <summary>
        /// 一天
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmDay_Click(object sender, EventArgs e)
        {
            HttpSubDate("1.0");
        }
        /// <summary>
        /// 一周
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmWeek_Click(object sender, EventArgs e)
        {
            HttpSubDate("7.0");
        }
        /// <summary>
        /// 一月
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmMonth_Click(object sender, EventArgs e)
        {
            HttpSubDate("30.0");
        }
        /// <summary>
        /// 一季
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmSeason_Click(object sender, EventArgs e)
        {
            HttpSubDate("90.0");

        }
        /// <summary>
        /// 一年
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmYear_Click(object sender, EventArgs e)
        {
            HttpSubDate("365.0");
        }
        #endregion
        #region  鼠标背景事件的背景色
        /// <summary>
        /// 鼠标离开事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel1_MouseLeave(object sender, EventArgs e)
        {
            Panel pal = (Panel)sender;
            pal.BackColor = Color.White;
            //  txtinfo.BackColor = Color.White;

        }
        /// <summary>
        /// 鼠标移入事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel3_MouseMove(object sender, MouseEventArgs e)
        {
            Panel pal = (Panel)sender;
            pal.BackColor = ColorTranslator.FromHtml("#D8D8D9");//悬浮颜色
                                                                //  txtinfo.BackColor = ColorTranslator.FromHtml("#D8D8D9");//悬浮颜色
        }
        #endregion
        #region 自适应改变面板的高度
        /// <summary>
        /// 设计时分为上下两个面板(上面显示图像，下面显示群组信息，由于上面是动态的当上面只有一行的时候它的高度就是最小值，两行的时候就是最大值，下面的值也是动态的，由于群管理是动态可见的（道理同上）)
        /// </summary>
        /// <param name="count"> 群成员面板需要显示成员的个数</param>
        private void changePanelHeigh(int count)
        {
            Point p = new Point();
            Point p2 = new Point();
            if (btnDel.Visible)//删除可见
            {
                if (count > 2)//tabMember俩行
                {
                    p.X = 6; p.Y = 163;
                    palgroupCtl.Location = p;
                    p2.X = 6; p2.Y = 528;
                    // palGroupDis.Location = p2;
                }
                else//tabMember一行
                {
                    p.X = 6; p.Y = tabMember.Location.Y + 75;
                    palgroupCtl.Location = p;
                    p2.X = 6; p2.Y = 528 - 70;
                    panel1.Size = new Size(panel1.Size.Width, panel1.Size.Height - 100);
                }
            }
            else
            {
                if (count > 3) //tabMember俩行
                {

                    p.X = 6; p.Y = 163;
                    palgroupCtl.Location = p;
                    p2.X = 6; p2.Y = 528;
                    // palGroupDis.Location = p2;
                }
                else//tabMember一行
                {
                    p.X = 6; p.Y = tabMember.Location.Y + 75;
                    palgroupCtl.Location = p;
                    p2.X = 6; p2.Y = 528 - 70;

                }
            }

            if (CurrentRole == 1)
            {

            }
            loding.stop();
            palgroupCtl.Visible = true;
        }
        #endregion
        /// <summary>
        /// 绑定滚动条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void USEGroupSet_Load(object sender, EventArgs e)
        {
            showInfoVScroll.SetCurrentPanel(panel1.Name);//滚动的范围
            showInfoVScroll.v_scale = 30; //鼠标滚动一格
            foreach (Control ctl in panel1.Controls)
            {
                AddCrlMouseEnter(ctl);
            }
        }
        private void View_MouseEnter(object sender, EventArgs e)
        {
            //添加 try -catch 在win7下双击最近消息列表（独立窗体）会报异常：无法激活不可见或已禁用的控件
            try
            {
                this.ActiveControl = (Control)sender;
            }
            catch (Exception ex)
            {

            }
        }
        private void AddCrlMouseEnter(Control crl)
        {
            foreach (Control item in crl.Controls)
            {
                if (item.Controls.Count > 0)
                    AddCrlMouseEnter(item);
                item.MouseEnter += (sender, e) => View_MouseEnter(sender, e);
            }
        }
        /// <summary>
        /// 退出群组
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //1.判断当前用户的角色，如果是群主就解散该群，如果是普通成员就删除该群
        //删除并推出群组
        private void btnDelAndExite_Click(object sender, EventArgs e)
        {
            FrmSMPGroupSet frm = new FrmSMPGroupSet();
            frm = (FrmSMPGroupSet)this.Parent;

            //获取 role再进行判断
            #region 群主
            if (CurrentRole == 1)
            {
                frm.IsClose = false;

                if (HttpUtils.Instance.ShowPromptBox("确定要退出解散群吗？"))
                {
                    ShowLodingDialog(panel1);
                    HttpUtils.Instance.InitHttp(this);
                    HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/delete") //删除群组
                    .AddParams("access_token", Applicate.Access_Token)
                    .AddParams("roomId", mFriend.RoomId)
                    .Build().AddErrorListener((code, err) =>
                    {
                        loding.stop();
                    })
                    .ExecuteJson<object>((sccess, room) =>
                    {
                        //infoCard14.FunctionInfo = GroupName+"AGG";
                        if (sccess)
                        {
                            
                            MessageObjectDao.Instance.DeleteMessageTable(mFriend.UserId);

                            loding.stop();
                            frm.IsClose = true;
                            frm.Close();
                        }

                    });
                }
                else
                {
                    //不解散群
                    //   dr = DialogResult.Cancel;
                    frm.IsClose = true;
                }
            }
            #endregion

            #region 群成员
            else
            {
                frm.IsClose = false;
                // 删除群成员

                if (HttpUtils.Instance.ShowPromptBox("确定要退出该群吗？"))
                {

                    ShowLodingDialog(panel1);
                    HttpUtils.Instance.InitHttp(this);
                    HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/member/delete") //删除群组
                    .AddParams("access_token", Applicate.Access_Token)
                    .AddParams("roomId", mFriend.RoomId)
                    .AddParams("userId", Applicate.MyAccount.userId)
                    .Build().AddErrorListener((code, err) =>
                    {
                        loding.stop();
                        // frm.IsClose = true;
                        //  frm.Close();

                    })
                    .Execute((sccess, room) =>
                    {
                        if (sccess)
                        {
                            MessageObjectDao.Instance.DeleteMessageTable(mFriend.UserId);

                            loding.stop();
                            frm.IsClose = true;
                            frm.Close();
                        }
                        else
                        {

                        }
                    });
                }
                else
                {
                    //dr = DialogResult.Cancel;
                    frm.IsClose = true;
                }
            }
            #endregion
        }
    }
}
