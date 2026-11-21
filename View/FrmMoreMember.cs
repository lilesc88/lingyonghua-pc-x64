using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFrmTalk.Controls;
using WinFrmTalk.Controls.CustomControls;
using WinFrmTalk.Model;
using System.Threading;
using WinFrmTalk.View.list;
using Newtonsoft.Json.Linq;

namespace WinFrmTalk.View
{
    public delegate void DelData();

    public partial class FrmMoreMember : FrmBase
    {
        //  int pageindex = 0;

        MemberLstAdapter mAdapter;


        public List<MembersItem> membersItems = new List<MembersItem>();
        List<RoomMember> RoomMemberLst = new List<RoomMember>();

        public Friend RoleHoste = new Friend();//群主
        private string roomid; // 当前roomid
        public Friend mfriend; // 当前群组信息

        public int Role { get; set; }// 当前登录账号在群组中的角色

        // 是否正在删除群成员
        public bool issum { get; set; }
        public int isshowmember;

        private LodingUtils loding;//等待符

        private void LoadLanguageText()
        {
            if (!Applicate.ENABLE_MULTI_LANGUAGE)
                return;

            this.Text = LanguageXmlUtils.GetValue("frmMoreMember_title", this.Text);
        }


        public FrmMoreMember()
        {
            InitializeComponent();
            LoadLanguageText();
            this.Icon = (Icon)Properties.Resources.Icon.Clone();//加载icon图标（独立副本，避免共享句柄被释放导致闪退）

            mAdapter = new MemberLstAdapter();


            RegisterRoomChangeMsg();
            useSearch.tips = LanguageXmlUtils.GetValue("search", "搜索");
            // useSearch.OnShowLoadSeach += UseSearch_OnStartSeach;//注册代码的原因：可能会导致第一次搜索，一直转圈圈
            useSearch.SearchEvent += Search;

        }

        private void UseSearch_OnStartSeach()
        {
            ShowLodingDialog(palGroupMenber);
        }

        public void SetRoom(Friend friend)
        {
            roomid = friend.RoomId;
            mfriend = friend;
        }

        //注册群控制消息监听
        private void RegisterRoomChangeMsg()
        {
            //注册群控制消息监听

            Messenger.Default.Register<MessageObject>(this, MessageActions.XMPP_UPDATE_ROOM_CHANGE_MESSAGE, item => Getmonitor(item));
        }

        public void LoadGroupMember()
        {
            //issum=true isshow


            if(issum)
            {
                palGroupMenber.FooterRefresh += LoadNextPageMenber;
            }
            else
            {
                if (isshowmember != 0)
                {
                    palGroupMenber.FooterRefresh += LoadNextPageMenber;
                }
                else
                {
                    if(Role==1|| Role ==2)
                    {
                        palGroupMenber.FooterRefresh += LoadNextPageMenber;
                    }
                }
            }
            
            GetMeanger();

        }

        public List<RoomMember> MeangeLst = new List<RoomMember>();
        private void GetMeanger()
        {
            ShowLodingDialog(palGroupMenber);
            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/getRoom") //获取群详情
               .AddParams("access_token", Applicate.Access_Token)
               .AddParams("roomId", roomid)
               .Build()
               .Execute((success, result) =>
               {
                   CloseLodingDialog();
                   string mendata = UIUtils.DecodeString(result, "members");
                   List<RoomMember> members = JsonConvert.DeserializeObject<List<RoomMember>>(mendata);
                   if (members == null || members.Count < 50)
                   {
                       isLoading = true;
                   }

                   MeangeLst = members;

                   if (issum)
                   {
                       LoadHttpRoomData(false);
                   }
                   else
                   {
                       if (isshowmember != 0)
                       {
                           LoadHttpRoomData(false);
                       }
                       else
                       {
                           if (Role == 1 || Role == 2)
                           {
                               LoadHttpRoomData(false);//接口分页
                           }
                           else
                           {
                               LoadMangest();
                           }
                       }
                   }
               });
        }
        /// <summary>
        /// 加载管理人员和自己 
        /// </summary>
        public void LoadMangest()
        {
            RoomMember myself = new RoomMember { userId = Applicate.MyAccount.userId, roomId = roomid };
            myself = myself.GetRoomMember();
            MeangeLst.Add(myself);
            RoomMemberLst = MeangeLst;

            BindListView(false, MeangeLst.Count);
        }

        // 加载第一页群成员数据
        private void LoadHttpRoomData(bool isload)
        {
            // 显示等待框
            ShowLodingDialog(palGroupMenber);
            // 设置为正在加载中
            isLoading = true;
            //获取群管理成员
            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/get")
                .AddParams("access_token", Applicate.Access_Token)
                .AddParams("roomId", roomid)
                .AddParams("pageIndex", "0")
                .AddParams("pageSize", "50")
                .Build()
                .Execute((sccess, room) =>
                {
                    isLoading = false;
                    if (sccess)
                    {
                        // 当前用户在群的信息

                        string member = UIUtils.DecodeString(room, "member");
                        if (UIUtils.IsNull(member))
                        {
                            return;
                        }
                        var currtaccount = JsonConvert.DeserializeObject<RoomMember>(member);
                        Role = currtaccount.role;
                        Console.WriteLine("获取我在群组中的身份： " + this.Role);

                        // 群成员信息
                        string mendata = UIUtils.DecodeString(room, "members");
                        List<RoomMember> members = JsonConvert.DeserializeObject<List<RoomMember>>(mendata);
                        if (members == null || members.Count < 50)
                        {
                            isLoading = true;
                        }

                        if (!UIUtils.IsNull(mendata) && !issum)
                        {
                            for (int i = members.Count - 1; i > -1; i--)
                            {
                                if (members[i].role != 3)
                                {
                                    members.RemoveAt(i);
                                }
                            }
                        }

                        DataFilter(members, false);

                    }

                    CloseLodingDialog();
                });
        }

        // 分页加载群成员
        private void LoadNextPageMenber()
        {
            if (isLoading)
            {
                return;
            }

            isLoading = true;
            ShowLodingDialog(palGroupMenber);
            // 分页获取群成员
            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/member/getMemberListByPage")
                .AddParams("access_token", Applicate.Access_Token)
                .AddParams("roomId", roomid)
                .AddParams("joinTime", GetLastTime())
                .AddParams("pageSize", "50")
                .Build()
                .ExecuteJson<List<RoomMember>>((sccess, datas) =>
                {
                    isLoading = false;
                    if (sccess)
                    {
                        if (datas.Count < 50)
                        {
                            isLoading = true;
                        }
                        DataFilter(datas, true);
                    }

                    CloseLodingDialog();
                });
        }


        // 把数据显示到控件上
        private void DataFilter(List<RoomMember> data, bool append)
        {
            //if (UIUtils.IsNull(data))
            //{
            //    return;
            //}
            List<RoomMember> templst = new List<RoomMember>();
            int x = data.Count;
            for (int i = 0; i < x; i++)
            {
                data[i].roomId = roomid;
                data[i].InsertOrUpdate();
                templst.Add(data[i]);

                // 我不是群主，隐身人不可见
                if (data[i].role == 4 && Role != 1)
                {
                    templst.Remove(data[i]);
                }

                // 删除群成员的时候把自己挑出去
                if (issum && data[i].userId.Equals(Applicate.MyAccount.userId))
                {
                    templst.Remove(data[i]);
                }

                // 如果我是群主就可以删除管理员， 如果我是管理员就只能移出普通成员 #8932
                if (Role == 1)
                {
                    if (data[i].role == 1)
                    {
                        templst.Remove(data[i]);
                    }
                }
                else
                {
                    if (data[i].role == 1 || data[i].role == 2)
                    {
                        templst.Remove(data[i]);
                    }
                }
            }
            data = templst;

            if (append || issum)
            {
                RoomMemberLst.AddRange(data);

            }
            else
            {

                MeangeLst.AddRange(data);
                RoomMemberLst = MeangeLst;
            }
            BindListView(append, data.Count);

        }


        private void BindListView(bool append, int appendSize)
        {
            mAdapter.SetMenberForm(this);

            // 分析，从服务器获取的数据一定不重复
            //List<RoomMember> members = RoomMemberLst.Distinct(new Comparer()).ToList();
            //RoomMemberLst = members;

            if (append)
            {
                palGroupMenber.InsertRange(RoomMemberLst.Count - appendSize);
            }
            else
            {
                mAdapter.BindDatas(RoomMemberLst);
                palGroupMenber.SetAdapter(mAdapter);
            }
            CloseLodingDialog();
        }

        private string GetLastTime()
        {

            if (UIUtils.IsNull(RoomMemberLst))
            {
                return "";

            }

            return RoomMemberLst[RoomMemberLst.Count - 1].createTime.ToString();
        }

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


        // 处理群控制消息
        public void MainNotiy(MessageObject msg)
        {
            UseRoleMember rolemember = new UseRoleMember();

            switch (msg.type)
            {
                case kWCMessageType.RoomManagerTransfer://转让群主
                    //新群主影响了两个地方1.列表中的角色2.如果当前界面删除时候角色被转换(可能是管理员)
                    string Newuserid = msg.toUserId;//新群主的userid
                    string OldUserid = msg.fromUserId;
                    RoomMember NewHorst = new RoomMember { userId = Newuserid, nickName = msg.toUserName, role = 1 };
                 
                    RoomMember OldHorst = new RoomMember { userId = OldUserid, nickName = msg.fromUserName, role = 3 };
                    //转让群主 ：1.原群主和新群主数据调换
                    string HorstRole = "";
                    int j = mAdapter.GetIndexByFriendId(OldUserid);
                    int x = mAdapter.GetIndexByFriendId(Newuserid);
                    if (j > -1)
                    {
                        HorstRole= LanguageXmlUtils.GetValue("leader", "群主");
                       
                        if (palGroupMenber.DataCreated(j))
                        {
                            rolemember = palGroupMenber.GetItemControl(j) as UseRoleMember;
                            rolemember.CurrentRole = HorstRole;
                           
                            mAdapter.ChangeData(j, NewHorst);
                            palGroupMenber.RefreshItem(j);

                            ImageLoader.Instance.DisplayGroupManager(NewHorst.userId, rolemember.pic_head, NewHorst.role);

                        }
                        else
                        {
                            mAdapter.GetDatas(j).role = 1;//更新数据源
                        }
                    }
                 
                    if (x > -1)
                    {
                        HorstRole = LanguageXmlUtils.GetValue("ordinary", "普通成员");
                        

                        if (palGroupMenber.DataCreated(x))
                        {
                            rolemember = palGroupMenber.GetItemControl(x) as UseRoleMember;
                            rolemember.CurrentRole = HorstRole;

                            mAdapter.ChangeData(x, OldHorst);
                            palGroupMenber.RefreshItem(x);

                            ImageLoader.Instance.DisplayGroupManager(OldHorst.userId, rolemember.pic_head, OldHorst.role);

                        }
                        else
                        {
                            mAdapter.GetDatas(x).role = 3;//更新数据源
                        }
                    }
                    

                    break;
                case kWCMessageType.RoomAdmin://管理员

                    // 通知聊天记录页刷新
                    int CurrentRole = 0;
                    string Roletext = "";
                    int m = mAdapter.GetIndexByFriendId(msg.toUserId);
                    if (m > -1)
                    {
                        if (msg.content == "0")//取消管理员
                        {
                            CurrentRole = 3;
                            Roletext = LanguageXmlUtils.GetValue("ordinary", "普通成员");

                        }
                        else if (msg.content == "1")//设置管理员
                        {
                            CurrentRole = 2;
                            Roletext = LanguageXmlUtils.GetValue("administrator", "管理员");

                        }
                        rolemember = palGroupMenber.GetItemControl(m) as UseRoleMember;

                        if (palGroupMenber.DataCreated(m))
                        {
                            rolemember = palGroupMenber.GetItemControl(m) as UseRoleMember;
                            rolemember.CurrentRole = Roletext;
                            ImageLoader.Instance.DisplayGroupManager(msg.toUserId, rolemember.pic_head, CurrentRole);
                        }
                        else
                        {
                            mAdapter.GetDatas(m).role = 2;//更新数据源
                        }
                    }

                    // rolemember = (UseRoleMember)mAdapter.OnCreateControl(mAdapter.GetIndexByFriendId(msg.toUserId));
                    // if (msg.content == "0")//取消管理员
                    // {
                    //     Roletext = "普通成员";

                    // }
                    // else if (msg.content == "1")//设置管理员
                    // {
                    //     Roletext = "管理员";

                    // }
                    // rolemember.CurrentRole = Roletext;
                    //mAdapter.


                    break;
                case kWCMessageType.RoomExit://退群
                    if (msg.toUserId == Applicate.MyAccount.userId)
                    {
                        //多点登陆时
                    }
                    else
                    { //某群员退出了群聊
                      // msg.type = kWCMessageType.Remind;

                        // LoadGroupData();
                        //谁退出了群
                        int i = mAdapter.GetIndexByFriendId(msg.toUserId);
                        if (i > -1)
                        {

                            rolemember = palGroupMenber.GetItemControl(i) as UseRoleMember;


                            palGroupMenber.RemoveItem(mAdapter.GetIndexByFriendId(msg.toUserId));
                            mAdapter.RemoveData(i);
                        }

                        // RoomMemberLst.Remove(RoomMemberLst[i]);

                    }
                    break;

                case kWCMessageType.RoomDismiss://群解散
                    this.Close();
                    break;
                case kWCMessageType.RoomAllowMemberInvite://允许普通好友邀请好友
                    mfriend.AllowInviteFriend = Convert.ToInt32(msg.content);
                    mfriend.UpdateAllowInviteFriend(Convert.ToInt32(msg.content));
                    break;
                case kWCMessageType.RoomIsVerify://群邀请确认
                    mfriend.IsNeedVerify = Convert.ToInt32(msg.content);
                    mfriend.UpdateNeedVerify();
                    break;
                case kWCMessageType.RoomInvite:
                    //在管理界面时有人加进来了

                    string addMemberUserid = msg.toUserId;//加入者userid
                    UseRoleMember men = new UseRoleMember();
                    men.friendData = new Friend();
                    men.friendData.UserId = addMemberUserid;
                    men.friendData = men.friendData.GetByUserId();//获取friend对象

                    if (men.friendData.NickName.Length > 5)
                    {
                        men.friendData.NickName = men.friendData.NickName.Substring(0, 4) + "...";
                    }

                    ImageLoader.Instance.DisplayAvatar(men.friendData.UserId, men.pic_head);

                    men.Tag = 3;
                    RoomMember roomMember = new RoomMember();
                    roomMember.userId = men.friendData.UserId;
                    roomMember.nickName = men.friendData.NickName;
                    roomMember.role = 3;
                    men.CurrentRole = LanguageXmlUtils.GetValue("ordinary", "普通成员");
                    //  MemberList.Add(men);
                    RoomMemberLst.Add(roomMember);
                    palGroupMenber.InsertItem(RoomMemberLst.Count - 1);

                    break;
                case kWCMessageType.RoomUserRecommend:
                    mfriend.AllowSendCard = Convert.ToInt32(msg.content);
                    mfriend.UpdateAllowSendCard(Convert.ToInt32(msg.content));
                    break;

                default:
                    return;
            }
        }




        /// <summary>
        /// 使用等待符
        /// </summary>
        private void ShowLodingDialog(Control con)
        {
            if (loding == null)
            {
                loding = new LodingUtils();
                loding.parent = con;
                loding.Title = LanguageXmlUtils.GetValue("loading", "加载中");
                loding.start();
            }
        }

        #region 请求邀请好友
        //public void RequestInviteContact(string roomId, List<string> datas, List<Friend> friends)
        //{
        //    string userids = JsonConvert.SerializeObject(datas);

        //    int isSecretGroup = mfriend.IsSecretGroup;

        //    Dictionary<string, string> value = new Dictionary<string, string>();
        //    value.Add("access_token", Applicate.Access_Token);
        //    value.Add("roomId", roomId);
        //    value.Add("text", userids);
        //    value.Add("isSecretGroup", isSecretGroup.ToString());

        //    if (1 == isSecretGroup)
        //    {
        //        string memkeys = GetMembersKey(friends, mfriend.ChatKeyGroup);
        //        value.Add("keys", memkeys);
        //    }

        //    //邀请群成员
        //    HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/member/update")
        //        .AddParams(value)
        //        .Build().Execute((sccess, data) =>
        //        {
        //            if (loding != null)
        //            {
        //                loding.stop();
        //            }
        //            if (sccess)
        //            {
        //                // 保存群成员到数据库
        //                foreach (var item in friends)
        //                {
        //                    RoomMember member = item.ToRoomMember(roomId);
        //                    member.InsertOrUpdate();
        //                }

        //                // 刷新群设置成员
        //                Messenger.Default.Send(roomId, MessageActions.Room_UPDATE_ROOM_ADD);
        //            }
        //        });
        //}



        // 获取群成员的 ras 公钥集合json
        //private string GetMembersKey(List<Friend> select, string chatKey)
        //{
        //    JObject json = new JObject();
        //    foreach (var friend in select)
        //    {
        //        string rskkey = RSA.EncryptBase64Pk1(chatKey, friend.RsaPublicKey);
        //        json.Add(friend.UserId, rskkey);
        //    }

        //    return json.ToString();
        //}

        #endregion

        // 查看其他群成员信息
        public void Pic_head_Click(object sender, EventArgs e)
        {

            if (mfriend.AllowSendCard == 0 && Role != 1)
            {
                HttpUtils.Instance.ShowTip("当前群组禁止普通成员私聊，不允许查看其他成员信息");
            }
            else
            {
                RoundPicBox pic = (RoundPicBox)sender;
                UseRoleMember uSEpicAddName = (UseRoleMember)pic.Parent;
                FrmFriendsBasic frmFriendsBasic = new FrmFriendsBasic();
                frmFriendsBasic.ShowUserInfoByRoom(uSEpicAddName.friendData.UserId, mfriend.UserId, Role);
                frmFriendsBasic.Show();
            }
        }

        #region 添加自定义控件集合
        #endregion



        public void USEGrouops_Click(object sender, EventArgs e)
        {
            UseRoleMember uSEGrouops = (UseRoleMember)sender;

            string nickname = uSEGrouops.friendData.NickName;
            string selecuserid = uSEGrouops.friendData.UserId;

            if (ShowPromptBox("确定要将" + UIUtils.QuotationName(nickname) + "移除?"))
            {
                ShowLodingDialog(palGroupMenber);

                //删除某一个群成员
                HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/member/delete")
                    .AddParams("access_token", Applicate.Access_Token)
                    .AddParams("roomId", roomid)
                    .AddParams("userId", selecuserid)
                    .Build().Execute((sccess, room) =>
                    {
                        loding.stop();
                        if (sccess)
                        {
                            //通知管理员变更或者指定

                            // Messenger.Default.Send(SelectItems.friendItem1, MessageActions.Room_UPDATE_ROOM_DELETE);

                            //1.删除搜索列表中的数据，2.删除搜索之前（大列表）的数据
                            RoomMember Updateroommember = new RoomMember { roomId = roomid, userId = selecuserid };
                            List<string> userids = new List<string>();
                            for (int i = 0; i < result.Count; i++)
                            {
                                if (!userids.Contains(result[i].userId))
                                {
                                    userids.Add(RoomMemberLst[i].userId);
                                    for (int j = 0; j < RoomMemberLst.Count; j++)
                                    {
                                        if (selecuserid == RoomMemberLst[j].userId)
                                        {
                                            RoomMemberLst.RemoveAt(j);
                                        }
                                    }
                                    RoomMemberLst.Remove(Updateroommember);
                                }

                            }

                            Updateroommember = Updateroommember.GetRoomMember();

                            Updateroommember.DeleteByUserId();
                            int index = mAdapter.GetIndexByFriendId(selecuserid);
                            if (index > -1)
                            {
                                var height = mAdapter.OnMeasureHeight(index);

                                palGroupMenber.RemoveItem(index);
                                mAdapter.RemoveData(index);
                            }
                                }
                    });
            }
        }

        //鼠标事件
        private void USEGrouops_MouseEnter(object sender, EventArgs e)
        {
            FriendItem friendItem = (FriendItem)sender;
            friendItem.Parent.BackColor = Color.LightGray;
            // throw new NotImplementedException();
        }


        List<RoomMember> result = new List<RoomMember>();
        private string lastSearchText;
        ///群成员搜索

        public void Search(string currText)
        {

            result = new List<RoomMember>();

            if (string.IsNullOrEmpty(currText) && string.IsNullOrEmpty(lastSearchText))
            {
                LogUtils.Log("SearchTextChanged null");
                return;
            }

            if (string.Equals(lastSearchText, currText))
            {
                LogUtils.Log("SearchTextChanged Equals");
                return;
            }

            // 清空了搜索框
            if (string.IsNullOrEmpty(currText) && !string.IsNullOrEmpty(lastSearchText))
            {
                lastSearchText = currText;
                // 恢复原列表
                BindListView(false, 0);
                return;
            }

            lastSearchText = currText;
            if (!string.IsNullOrEmpty(currText))
            {
                // 加载搜索数据
                if (issum)
                {
                    HttpSearchMembes(roomid, currText);
                }
                else
                {
                    if (isshowmember != 0)
                    {
                        HttpSearchMembes(roomid, currText);
                    }
                    else
                    {
                        if (Role == 1 || Role == 2)
                        {
                            HttpSearchMembes(roomid, currText);
                        }
                        else
                        {
                            SearchMangent(currText);
                        }
                    }
                }
                

                return;

                mAdapter.SetMenberForm(this);
                mAdapter.BindDatas(result);
                palGroupMenber.SetAdapter(mAdapter);
                CloseLodingDialog();
            }

        }


        private void HttpSearchMembes(string roomId, string keyword)
         {
           
                ShowLodingDialog(palGroupMenber);

                // 搜索群成员
                HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/member/list")

                    .AddParams("roomId", roomid)
                    .AddParams("keyword", keyword)
                    .Build()
                    .ExecuteJson<List<RoomMember>>((sccess, datas) =>
                    {

                        if (sccess && !UIUtils.IsNull(datas))
                        {
                            if (issum && !UIUtils.IsNull(datas))
                            {
                                for (int i = datas.Count - 1; i > -1; i--)
                                {
                                    var mem = datas[i];
                                // 不能删除自己
                                if (string.Equals(mem.userId, Applicate.MyAccount.userId))
                                    {
                                        datas.RemoveAt(i);
                                    }
                                    else if (mem.role == 1 || (Role == mem.role))
                                    {
                                    // 不能删除群主 我是管理也不能删除管理
                                    datas.RemoveAt(i);
                                    }

                                }
                                result.Clear();
                                result = datas;
                            }

                            mAdapter.SetMenberForm(this);
                            mAdapter.BindDatas(datas);
                            palGroupMenber.SetAdapter(mAdapter);
                        }
                        else
                        {
                            palGroupMenber.ClearList();
                        }


                        CloseLodingDialog();
                    });
          
        }
        public void SearchMangent( string keyword)
        {
            if (!UIUtils.IsNull(RoomMemberLst))
            {
                result.Clear();

                for (int i = RoomMemberLst.Count - 1; i > -1; i--)
                {
                    var mem = RoomMemberLst[i];
                    Friend friend = new Friend { UserId = mem.userId };
                    friend = friend.GetByUserId();
                    // 不能删除自己
                    if (UIUtils.Contains(mem.nickName, keyword) || UIUtils.Contains(mem.remarkName, keyword) || UIUtils.Contains(friend.RemarkName, keyword))
                    {
                        result.Add(mem);
                    }
                }

                mAdapter.SetMenberForm(this);
                mAdapter.BindDatas(result);
                palGroupMenber.SetAdapter(mAdapter);
            }
            else
            {
                palGroupMenber.ClearList();
            }
        }

        private void CloseLodingDialog()
        {
            if (loding != null)
            {
                loding.stop();
                loding = null;
            }
        }
        /// <summary> 
        /// 汉字转化为拼音
        /// </summary> 
        /// <param name="str">汉字</param> 
        /// <returns>全拼</returns> 
        public static string GetPinyin(string str)
        {
            string r = string.Empty;
            foreach (char obj in str)
            {
                try
                {
                    //ChineseChar chineseChar = new ChineseChar(obj);
                    //string t = chineseChar.Pinyins[0].ToString();
                    //r += t.Substring(0, t.Length - 1);
                }
                catch
                {
                    r += obj.ToString();
                }
            }
            return r;
        }


        private void palGroupMenber_Load(object sender, EventArgs e)
        {

        }

        //  Point currentpoint = new Point();
        //  private int Refreshtime;
        private bool isLoading;

        public void refresh()
        {

            //palGroupMenber.AddScollerTop(() =>
            //    {

            //      if(TimeUtils.CurrentIntTime()- Refreshtime>5 ||(Refreshtime==0))
            //        {
            //            Point p = new Point();
            //            p = palGroupMenber.Location;
            //            currentpoint = palGroupMenber.Location;
            //            palLoading.Visible = true;
            //            p.Y = p.Y + 80;
            //            palGroupMenber.Location = p;

            //            ShowLodingDialog(palLoading);
            //            palGroupMenber.ClearTabel();
            //            Refreshtime = TimeUtils.CurrentIntTime();

            //            LoadHttpRoomData(true);//当此方法完成后再执行下面的

            //        }

            //    });

        }
        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void FrmMoreMember_FormClosed(object sender, FormClosedEventArgs e)
        {
            Messenger.Default.Unregister(this);//反注册
        }


        public class Comparer : IEqualityComparer<RoomMember>
        {
            public bool Equals(RoomMember x, RoomMember y)
            {
                //这里定义比较的逻辑
                return x.userId == y.userId && x.roomId == y.roomId;
            }

            public int GetHashCode(RoomMember obj)
            {
                //返回字段的HashCode，只有HashCode相同才会去比较
                return obj.userId.GetHashCode();
            }
        }
    }
}

