using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinFrmTalk.Controls;
using WinFrmTalk.Controls.CustomControls;
using WinFrmTalk.Model;
using WinFrmTalk.View.list;

namespace WinFrmTalk.View
{
    public partial class FrmMembersLst : FrmBase
    {
        public FrmMembersLst()
        {
            InitializeComponent();
            Delrooms = new List<RoomMember>();
            mAdapter = new MembersLstAdapter();


        }
        #region 变量
        private Friend Mfriend;//当前群
        private LodingUtils loding;//等待符
        private int Role; //当前成员的角色
        private bool isLoading;//是否正在加载
        public List<RoomMember> MeangeLst; //管理成员列表
        public int isshowmember;//0不显示群成员，1显示群成员
        List<RoomMember> RoomMemberLst;//全部群成员
        MembersLstAdapter mAdapter;
        public List<RoomMember> Delrooms;
        public bool issum;
        #endregion
        /// <summary>
        /// 设置当前群组的friend
        /// </summary>
        /// <param name="friend"></param>
        public void setdata(Friend friend)
        {
            Mfriend = friend;
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
        /// <summary>
        /// 关闭等待符
        /// </summary>
        private void CloseLodingDialog()
        {
            if (loding != null)
            {
                loding.stop();
                loding = null;
            }
        }
        /// <summary>
        /// 获取最后一个加入的时间
        /// </summary>
        /// <returns></returns>
        private string GetLastTime()
        {

            if (UIUtils.IsNull(RoomMemberLst))
            {
                return "";

            }

            return RoomMemberLst[RoomMemberLst.Count - 1].createTime.ToString();
        }
        public void LoadGroupMember()
        {
            //issum=true isshow


            if (issum)
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
                    if (Role == 1 || Role == 2)
                    {
                        palGroupMenber.FooterRefresh += LoadNextPageMenber;
                    }
                }
            }

            GetMeanger();

        }
        /// <summary>
        /// 获取管理员成员
        /// </summary>
        private void GetMeanger()
        {
            ShowLodingDialog(palGroupMenber);
            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/getRoom") //获取群详情
               .AddParams("access_token", Applicate.Access_Token)
               .AddParams("roomId", Mfriend.RoomId)
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
            RoomMember myself = new RoomMember { userId = Applicate.MyAccount.userId, roomId = Mfriend.RoomId };
            myself = myself.GetRoomMember();
            MeangeLst.Add(myself);
            RoomMemberLst = MeangeLst;

            BindListView(false, MeangeLst.Count);
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
                .AddParams("roomId", Mfriend.RoomId)
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
                .AddParams("roomId", Mfriend.RoomId)
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
                data[i].roomId = Mfriend.RoomId;
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
        /// <summary>
        /// 删除好友（多选）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnDel_Click(object sender, EventArgs e)
        {
            if (issum)
            {
                UserMembersinfo uSEGrouops = (UserMembersinfo)sender;

                string nickname = uSEGrouops.RoomMember.nickName;
                string selecuserid = uSEGrouops.RoomMember.userId;

                if (ShowPromptBox("确定要将" + UIUtils.QuotationName(nickname) + "移除?"))
                {
                    ShowLodingDialog(palGroupMenber);

                    //删除某一个群成员
                    HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/member/delete")
                        .AddParams("access_token", Applicate.Access_Token)
                        .AddParams("roomId", Mfriend.RoomId)
                        .AddParams("userId", selecuserid)
                        .Build().Execute((sccess, room) =>
                        {
                            loding.stop();
                            if (sccess)
                            {
                                //通知管理员变更或者指定

                                // Messenger.Default.Send(SelectItems.friendItem1, MessageActions.Room_UPDATE_ROOM_DELETE);

                                //1.删除搜索列表中的数据，2.删除搜索之前（大列表）的数据
                                RoomMember Updateroommember = new RoomMember { roomId = Mfriend.UserId, userId = selecuserid };
                                List<string> userids = new List<string>();
                                for (int i = 0; i < RoomMemberLst.Count; i++)
                                {
                                    if (!userids.Contains(RoomMemberLst[i].userId))
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





                #region 预留多选删除代码

                //palGroupMenber.SuspendLayout();
                //try
                //{
                //    mAdapter.isMultiSelect = true;
                //    //展示多选框
                //    panel1.BringToFront();
                //    panel1.Visible = true;

                //    foreach (Control crl in palGroupMenber.panel1.Controls)
                //    {
                //        foreach (Control item in crl.Controls)
                //        {
                //            if (item is CheckBoxEx checkBox)
                //            {
                //                checkBox.Visible = true;
                //                checkBox.Checked = false;
                //            }
                //            else if (item is UserMembersinfo talk_panel )
                //            {

                //                talk_panel.Location = new Point(talk_panel.Location.X + 22, talk_panel.Location.Y);
                //            }

                //        }
                //    }
                //}
                //catch (Exception ex) { LogUtils.Error(ex.Message); }
                //palGroupMenber.ResumeLayout();
                //////清空之前被多选的集合
                ////multiSelectPanel.List_Msgs = new List<MessageObject>();
                ////multiSelectPanel.FdTalking = ChooseTarget;
                //////展示多选
                //////IsShowPanelMultiSelect(true);
                ////SetDialogBoxSize(DialogBox.MultiSelect);
                #endregion
            }
        }
        /// <summary>
        /// 多选删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnmuiltDel_Click(object sender, EventArgs e)
        {
            //UseRoleMember uSEGrouops = (UseRoleMember)sender;

            //string nickname = uSEGrouops.friendData.NickName;
            //string selecuserid = uSEGrouops.friendData.UserId;

            //if (ShowPromptBox("确定要将" + UIUtils.QuotationName(nickname) + "移除?"))
            //{
            //    ShowLodingDialog(palGroupMenber);

            //    //删除某一个群成员
            //    HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/member/delete")
            //        .AddParams("access_token", Applicate.Access_Token)
            //        .AddParams("roomId", Mfriend.RoomId)
            //        .AddParams("userId", selecuserid)
            //        .Build().Execute((sccess, room) =>
            //        {
            //            loding.stop();
            //            if (sccess)
            //            {
            //                //通知管理员变更或者指定

            //                // Messenger.Default.Send(SelectItems.friendItem1, MessageActions.Room_UPDATE_ROOM_DELETE);

            //                //1.删除搜索列表中的数据，2.删除搜索之前（大列表）的数据
            //                RoomMember Updateroommember = new RoomMember { roomId = roomid, userId = selecuserid };
            //                List<string> userids = new List<string>();
            //                for (int i = 0; i < result.Count; i++)
            //                {
            //                    if (!userids.Contains(result[i].userId))
            //                    {
            //                        userids.Add(RoomMemberLst[i].userId);
            //                        for (int j = 0; j < RoomMemberLst.Count; j++)
            //                        {
            //                            if (selecuserid == RoomMemberLst[j].userId)
            //                            {
            //                                RoomMemberLst.RemoveAt(j);
            //                            }
            //                        }
            //                        RoomMemberLst.Remove(Updateroommember);
            //                    }

            //                }

            //                Updateroommember = Updateroommember.GetRoomMember();

            //                Updateroommember.DeleteByUserId();
            //                int index = mAdapter.GetIndexByFriendId(selecuserid);
            //                if (index > -1)
            //                {
            //                    var height = mAdapter.OnMeasureHeight(index);

            //                    palGroupMenber.RemoveItem(index);
            //                    mAdapter.RemoveData(index);

            //                    // 删除比较靠下的元素需要自动收缩列表
            //                    if (palGroupMenber.Progress > 90 && index > mAdapter.GetItemCount() - 10)
            //                    {

            //                        int locy = palGroupMenber.ListPanelLocationY() + height;
            //                        if (locy < 0)
            //                        {
            //                            palGroupMenber.MovePanelLocation(locy);
            //                            Console.WriteLine("向下移动列表" + locy);
            //                        }
            //                    }
            //                }
            //            }
            //        });
            //}
        }
        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            panel1.Visible = false;
            panel1.SendToBack();
            Delrooms = new List<RoomMember>();
        }

        private void FrmMembersLst_Load(object sender, EventArgs e)
        {
            panel1.SendToBack();//置于底层

        }
    }
}
