using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using RichTextBoxLinks;
using WinFrmTalk.Dictionarys;
using WinFrmTalk.Helper;
using WinFrmTalk.Model;
using WinFrmTalk.View;
using WinFrmTalk.View.list;
using Timer = System.Windows.Forms.Timer;

namespace WinFrmTalk.Controls.CustomControls
{
    public partial class UserCollection : UserControl
    {
        #region 全局变量
        List<Collections> collectionlst = new List<Collections>();// 全部集合
        CollectionAdapter mAdapter;//收藏适配
        MyColleagueAdapter myColleagueAdapter;//我的讲课适配
        public List<MessageObject> ListMessage = new List<MessageObject>();//选中讲课消息集合

        // 我的讲课选中项
        private UserCollectionItem mSelectCourse;

        // 我的收藏选中项
        private UserCollectionItem CollectionItem;

        private bool isLoadCourseData; // 是否加载过数据了
        public const int minHeight = 50;//最小高度
        public static int BubbleWidth, BubbleHeight;//气泡宽度，气泡高度

        private void LoadLanguageText()
        {
            if (!Applicate.ENABLE_MULTI_LANGUAGE)
                return;

            btnWhole.Text = LanguageXmlUtils.GetValue("all_collection", btnWhole.Text);
            btnText.Text = LanguageXmlUtils.GetValue("text_collection", btnText.Text);
            btnImg.Text = LanguageXmlUtils.GetValue("image_collection", btnImg.Text);
            btnVideo.Text = LanguageXmlUtils.GetValue("video_collection", btnVideo.Text);
            btnLecture.Text = LanguageXmlUtils.GetValue("my_courseware", btnLecture.Text);
            tsmEdit.Text = LanguageXmlUtils.GetValue("edit_courseware_name", tsmEdit.Text);
            tsmSendLecture.Text = LanguageXmlUtils.GetValue("send_courseware", tsmSendLecture.Text);
            tsmDelete.Text = LanguageXmlUtils.GetValue("delete_courseware", tsmDelete.Text);
            tsmForward.Text = LanguageXmlUtils.GetValue("forward", tsmForward.Text);
            tsmDel.Text = LanguageXmlUtils.GetValue("delete", tsmDel.Text);
        }

        #endregion
        public UserCollection()
        {
            InitializeComponent();
            LoadLanguageText();
            mAdapter = new CollectionAdapter();//适配绑定（收藏）
            myColleagueAdapter = new MyColleagueAdapter();//适配绑定（我的讲课）
            myColleagueAdapter.SetMaengForm(this);
            // 更新收藏页
            Messenger.Default.Register<string>(this, MessageActions.UPDATE_COLLECT_LIST, (str) =>
            {
                refretime = 0;
            });

            // 更新我的讲课
            Messenger.Default.Register<string>(this, MessageActions.UPDATE_COURSE_LIST, (str) =>
            {
                isLoadCourseData = false;
                if (LanguageXmlUtils.GetValue("my_lecture", "我的课件").Equals(lblTitle.Text))
                {
                    btnLecture_Click(this.btnLecture, null);
                }
            });

            //SearchCollect.tips = "收藏文字";
            SearchCollect.tips = LanguageXmlUtils.GetValue("search", "搜索");
            SearchCollect.SearchEvent += SearchMeesageContent;
        }

        public LodingUtils loding;//等待符

        /// <summary>
        /// 显示等待符
        /// </summary>
        /// <param name="control">等待符显示的父控件</param>
        public void ShowLodingDialog(Control control)
        {
            if (loding == null)
            {
                loding = new LodingUtils();
                loding.parent = control;
                loding.Title = LanguageXmlUtils.GetValue("loading", "加载中");
                loding.start();
            }
        }



        /// <summary>
        /// 停止等待符
        /// </summary>
        public void HideLodingDialog()
        {
            if (loding != null)
            {
                loding.stop();
                loding = null;
            }
        }

        #region 加载全部数据
        public int refretime = 0;
        public void RefreData()
        {
            if (TimeUtils.CurrentIntTime() - refretime > 60)
            {
                refretime = TimeUtils.CurrentIntTime();
                WholeData();
            }
        }

        /// <summary>
        /// 全部收藏数据
        /// </summary>
        private void WholeData()
        {
            lblTitle.Text = btnWhole.Text.Trim();
            tvClassTip.Visible = false;
            ShowLodingDialog(myTabLayoutPanel1);

            HttpUtils.Instance.InitHttp(this);
            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "user/collection/list")
               .AddParams("access_token", Applicate.Access_Token)
               .AddParams("userId", Applicate.MyAccount.userId)
               .Build().AddErrorListener((code, ss) =>
               {
                   loding.stop();
                   myTabLayoutPanel1.SetAdapter(mAdapter);
                   HttpUtils.Instance.ShowTip(ss);

               }).ExecuteJson<List<Collections>>((suss, data) =>
               {
                   if (loding != null)
                   {
                       loding.stop();
                   }

                   if (UIUtils.IsNull(data))
                   {
                       myTabLayoutPanel1.SetAdapter(mAdapter);
                       HttpUtils.Instance.ShowTip("暂无收藏");
                       return;
                   }

                   collectionlst = data;

                   mAdapter.SetMaengForm(this);
                   mAdapter.BindDatas(collectionlst);
                   myTabLayoutPanel1.SetAdapter(mAdapter);
               });
        }
        #endregion



        public void OnClickCollectionItem(UserCollectionItem item, EventArgs e)
        {
            if (((MouseEventArgs)e).Button == MouseButtons.Left)
            {

                var data = item.Collections;
                switch (data.type)
                {
                    case "1":
                        // 图片和文字
                        OpenFile(data);
                        break;
                    case "2":
                        // 视频
                        OpenVideo(data);
                        break;
                    case "3":
                        // 文件
                        OpenFile(data);
                        break;
                    case "4":
                        // 语音
                        OpenVoice(data);
                        break;
                    case "5":
                        // 链接
                        if (UIUtils.IsHttpUrl(data.msg))
                        {
                            OpenLink(data);
                        }
                        else
                        {
                            OpenTextLook(data);
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                CollectionItem = item;
            }


        }



        #region 收藏类型
        /// <summary>
        /// 全部
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnWhole_Click(object sender, EventArgs e)
        {
            myTabLayoutPanel1.ClearList();
            lblTitle.Text = "";
            lblTitle.Text = ((Control)sender).Text.Trim();
            mAdapter.SetMaengForm(this);
            mAdapter.BindDatas(collectionlst);
            myTabLayoutPanel1.SetAdapter(mAdapter);

            tvClassTip.Visible = false;
        }
        /// <summary>
        /// 文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnText_Click(object sender, EventArgs e)
        {
            ShowLodingDialog(myTabLayoutPanel1);
            myTabLayoutPanel1.ClearList();
            lblTitle.Text = "";
            lblTitle.Text = ((Control)sender).Text.Trim();
            mAdapter.SetMaengForm(this);

            List<Collections> textcollection = new List<Collections>();

            for (int i = 0; i < collectionlst.Count; i++)
            {
                if (collectionlst[i].type == "5")
                {
                    textcollection.Add(collectionlst[i]);
                }
            }
            mAdapter.BindDatas(textcollection);
            myTabLayoutPanel1.SetAdapter(mAdapter);
            loding.stop();

            tvClassTip.Visible = false;
        }
        /// <summary>
        /// 图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImg_Click(object sender, EventArgs e)
        {
            ShowLodingDialog(myTabLayoutPanel1);
            myTabLayoutPanel1.ClearList();
            lblTitle.Text = "";
            lblTitle.Text = ((Control)sender).Text.Trim();
            tvClassTip.Visible = false;
            mAdapter.SetMaengForm(this);
            List<Collections> imagecollection = new List<Collections>();

            for (int i = 0; i < collectionlst.Count; i++)
            {
                if (collectionlst[i].type == "1")
                {
                    imagecollection.Add(collectionlst[i]);
                }
            }
            mAdapter.BindDatas(imagecollection);
            myTabLayoutPanel1.SetAdapter(mAdapter);
            loding.stop();
        }

        /// <summary>
        /// 视频
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnVideo_Click(object sender, EventArgs e)
        {
            tvClassTip.Visible = false;
            ShowLodingDialog(myTabLayoutPanel1);
            myTabLayoutPanel1.ClearList();
            lblTitle.Text = "";
            lblTitle.Text = ((Control)sender).Text.Trim();

            mAdapter.SetMaengForm(this);
            List<Collections> vidiocollection = new List<Collections>();
            for (int i = 0; i < collectionlst.Count; i++)
            {
                if (collectionlst[i].type == "2")
                {
                    vidiocollection.Add(collectionlst[i]);
                }
            }
            mAdapter.BindDatas(vidiocollection);
            myTabLayoutPanel1.SetAdapter(mAdapter);


            if (loding != null)
            {
                loding.stop();
            }

        }
        #endregion

        #region 转发收藏
        private void tsmForward_Click(object sender, EventArgs e)
        {
            var frm = Applicate.GetWindow<FrmFriendSelect>();

            if (frm == null)
            {
                frm = new FrmFriendSelect();
            }
            else
            {
                frm.Activate();
                return;
            }

            frm.max_number = 15;
            frm.LoadFriendsData(1);
            frm.AddConfrmListener((dis) =>
            {
                HttpUtils.Instance.PopView(frm);
                foreach (var friend in dis)
                {
                    ForwardCollection(friend.Value, CollectionItem);
                }
            });
        }

        // 转发收藏
        private void ForwardCollection(Friend friend, UserCollectionItem collection)
        {
            if (CollectUtils.EnableForward(friend))
            {
                return;
            }

            MessageObject messageObjects = null;

            Collections collections = collection.Collections;

            switch (collections.type)
            {
                //图片
                case "1":

                    string[] conString = collections.msg.Split(',');
                    foreach (var msg in conString)
                    {
                        messageObjects = new MessageObject() { content = msg, type = kWCMessageType.Image,fromUserId= Applicate.MyAccount.userId,fromUserName= Applicate.MyAccount.nickname };
                        ShiKuManager.SendForwardMessage(friend, messageObjects);

                        Messenger.Default.Send(messageObjects, token: MessageActions.XMPP_UPDATE_NORMAL_MESSAGE);

                        HttpUtils.Instance.ShowTip("转发成功");
                        messageObjects = null;
                    }

                    if (!string.IsNullOrEmpty(collections.collectContent))
                    {
                        messageObjects = new MessageObject() { content = collections.collectContent, type = kWCMessageType.Image, fromUserId = Applicate.MyAccount.userId, fromUserName = Applicate.MyAccount.nickname };
                        ShiKuManager.SendForwardMessage(friend, messageObjects);

                        Messenger.Default.Send(messageObjects, token: MessageActions.XMPP_UPDATE_NORMAL_MESSAGE);

                        HttpUtils.Instance.ShowTip("转发成功");
                        messageObjects = null;
                    }
                    return;
                //视频
                case "2":
                    messageObjects = new MessageObject() { content = collections.url, fileName = FileUtils.GetFileName(collections.url), type = kWCMessageType.Video, fileSize = collections.fileSize,fromUserId = Applicate.MyAccount.userId, fromUserName = Applicate.MyAccount.nickname };
                    break;
                //文件
                case "3":
                    messageObjects = new MessageObject() { content = collections.url, fileName = FileUtils.GetFileName(collections.Filename), type = kWCMessageType.File, fileSize = collections.fileSize ,fromUserId = Applicate.MyAccount.userId, fromUserName = Applicate.MyAccount.nickname };
                    break;

                //文本表情
                case "5":
                    messageObjects = new MessageObject() { content = collections.collectContent, type = kWCMessageType.Text, fromUserId = Applicate.MyAccount.userId, fromUserName = Applicate.MyAccount.nickname };
                    break;

                case "4":    //语音
                    messageObjects = new MessageObject() { content = collections.url, fileName = collections.Filename, type = kWCMessageType.Voice, fileSize = collections.fileSize, timeLen = Convert.ToInt32(collections.fileLength) , fromUserId = Applicate.MyAccount.userId, fromUserName = Applicate.MyAccount.nickname };
                    break;
                case "6":       //表情
                case "7":         //SDK分享链接
                    break;
            }


            if (messageObjects != null)
            {
                ShiKuManager.SendForwardMessage(friend, messageObjects);

                Messenger.Default.Send(messageObjects, token: MessageActions.XMPP_UPDATE_NORMAL_MESSAGE);

                HttpUtils.Instance.ShowTip("转发成功");
            }

        }

        #endregion

        #region 删除收藏
        private void tsmDel_Click(object sender, EventArgs e)
        {

            bool result = HttpUtils.Instance.ShowPromptBox("确认删除", true);
            if (result && CollectionItem != null)
            {
                HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "user/emoji/delete")
                    .AddParams("access_token", Applicate.Access_Token)
                    .AddParams("emojiId", CollectionItem.Collections.emojiId)
                    .Build().Execute((su, da) =>
                    {
                        if (su)
                        {
                            int index = mAdapter.GetMessageIdByIndex(CollectionItem.Collections.emojiId);


                            HttpUtils.Instance.ShowTip("删除成功");
                            //if (index > -1)
                            //{
                            //    myTabLayoutPanel1.RemoveItem(index);
                            //    mAdapter.RemoveData(index);
                            //    HttpUtils.Instance.ShowTip("删除成功");
                            //}


                            //if (!btnWhole.Text.Trim().Equals(lblTitle.Text))
                            //{
                            //    for (int i = collectionlst.Count - 1; i > -1; i--)
                            //    {
                            //        if (collectionlst[i].emojiId == CollectionItem.Name)
                            //        {
                            //            collectionlst.RemoveAt(i);
                            //            break;
                            //        }
                            //    }
                            //}
                            // Messenger.Default.Send(msg, MessageActions.CLEAR_COLLECT_TEXT);

                            Messenger.Default.Send(CollectionItem.Collections.emojiId, MessageActions.CLEAR_COLLECT_TEXT);

                            WholeData();
                        }
                    });
            }
        }

        #endregion

        #region 点击我的讲课
        private void btnLecture_Click(object sender, EventArgs e)
        {

            lblTitle.Text = ((Control)sender).Text.Trim();


            // 加载过数据就不在去加载了
            if (isLoadCourseData)
            {
                tvClassTip.Visible = myColleagueAdapter.GetItemCount() == 0;
                myTabLayoutPanel1.SetAdapter(myColleagueAdapter);
                return;
            }

            ShowLodingDialog(myTabLayoutPanel1);
            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "user/course/list")
                .AddParams("access_token", Applicate.Access_Token)
                .AddParams("userId", Applicate.MyAccount.userId)
                .Build().ExecuteList<ColleaguesList>((suss, data) =>
                {
                    HideLodingDialog();
                    if (suss)
                    {
                        isLoadCourseData = true;
                        tvClassTip.Visible = UIUtils.IsNull(data.data);

                        if (!UIUtils.IsNull(data.data))
                        {
                            data.data.Reverse();
                        }
                        myColleagueAdapter.BindDatas(data.data);
                        myTabLayoutPanel1.SetAdapter(myColleagueAdapter);
                    }
                });
        }

        #endregion


        #region 点击我的讲课详情
        // 点击我的讲课详情
        internal void OnClickCourseItem(UserCollectionItem item, MouseEventArgs eve)
        {
            if (eve.Button == MouseButtons.Right)
            {
                mSelectCourse = item;
                if (mSelectCourse != null)
                {
                    mSelectCourse.ContextMenuStrip = null;
                }

                mSelectCourse = item;
                mSelectCourse.ContextMenuStrip = cmsLecture;
                return;
            }

            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "user/course/get")
                      .AddParams("courseId", item.Name)
                      .AddParams("access_token", Applicate.Access_Token)
                      .Build().ExecuteJson<List<CourseMessage>>((state, jsonArray) =>
                      {
                          if (state)
                          {
                              ListMessage = new List<MessageObject>();

                              foreach (var arrItme in jsonArray)
                              {
                                  MessageObject msg = arrItme.ToMessageObject();

                                  //解析出错
                                  if (msg == null)
                                  {
                                      continue;
                                  }

                                  if (msg.isEncrypt > 0)
                                  {
                                      SkSSLUtils.DecryptRecordMessage(msg, msg.ChatJid.Length > 18);
                                  }

                                  if (msg.isEncrypt != 3)
                                  {
                                      ListMessage.Add(msg);
                                  }
                              }

                              if (eve.Button == MouseButtons.Left)
                              {
                                  if (mSelectCourse != null)
                                  {
                                      mSelectCourse.ContextMenuStrip = null;
                                  }

                                  mSelectCourse = item;


                                  //选择转发的好友
                                  var frmHistoryMsg = Applicate.GetWindow<FrmHistoryMsg>();
                                  if (frmHistoryMsg == null)
                                  {
                                      frmHistoryMsg = new FrmHistoryMsg()
                                      {
                                          messages = ListMessage,
                                          FromLocal = false
                                      };
                                      frmHistoryMsg.Text = btnLecture.Text.Trim();
                                      frmHistoryMsg.Show();
                                  }
                                  else
                                  {
                                      frmHistoryMsg.messages = ListMessage;
                                      frmHistoryMsg.FromLocal = false;
                                      frmHistoryMsg.Text = btnLecture.Text.Trim();
                                      frmHistoryMsg.Activate();
                                  }
                              }
                          }
                      });
        }

        #endregion

        #region 修改我的讲课名称

        private void tsmEdit_Click(object sender, EventArgs e)
        {
            var frm = Applicate.GetWindow<FrmMyColleagueEidt>();

            if (frm == null)
            {
                frm = new FrmMyColleagueEidt();
            }
            else
            {
                frm.Activate();
                return;
            }

            string courName = mSelectCourse.Tag.ToString();
            frm.NameEdit = courName;// panel_file.lab_fileName.Text.Remove(0, 5);
            frm.ColleagueName((courseName) =>
            {
                HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "user/course/update")
                    .AddParams("access_token", Applicate.Access_Token)
                    .AddParams("courseId", mSelectCourse.Name)
                    .AddParams("courseName", courseName)
                    .Build().Execute((suss, data) =>
                    {
                        if (suss)
                        {
                            frm.Close();
                            HttpUtils.Instance.ShowTip("修改成功");
                            myColleagueAdapter.UpdateCourseName(mSelectCourse.Name, courseName);
                            mSelectCourse.RefreshCourseName(courseName);
                            mSelectCourse.Tag = courseName;
                        }
                    });

            });

            string title = LanguageXmlUtils.GetValue("title_modify_courseware", "修改课件");
            string name = LanguageXmlUtils.GetValue("name_courseware_name", "课件名称：", true);
            frm.ShowThis(title, name);
        }

        #endregion

        #region 发送讲课
        private void tsmSendLecture_Click(object sender, EventArgs e)
        {
            var schedule = Applicate.GetWindow<FrmSchedule>();
            if (schedule != null)
            {
                HttpUtils.Instance.ShowTip("请等待当前发送任务完成", true);
                return;
            }


            var frm = Applicate.GetWindow<FrmFriendSelect>();

            if (frm == null)
            {
                frm = new FrmFriendSelect();
            }
            else
            {
                frm.Activate();
                return;
            }
            frm.max_number = 1;
            frm.LoadFriendsData(1);
            frm.AddConfrmListener((data) =>
            {
                if (data == null || data.Count == 0)
                {
                    HttpUtils.Instance.ShowTip("数据错误");
                    return;
                }

                Friend friend = null;
                foreach (var item in data.Values)
                {
                    friend = item;
                }

                if (friend == null || UIUtils.IsNull(ListMessage))
                {
                    HttpUtils.Instance.ShowTip("数据错误");
                    return;
                }
                else
                {
                    if (friend.IsGroup == 1)
                    {
                        if (friend.IsSecretGroup == 1)
                        {
                            if (UIUtils.IsNull(friend.ChatKeyGroup))
                            {
                                // 没有密钥
                                HttpUtils.Instance.ShowTip("不能转发消息到密钥丢失群");
                                return;
                            }

                            if (friend.IsLostKeyGroup == 1)
                            {
                                // 没有密钥
                                HttpUtils.Instance.ShowTip("不能转发消息到密钥丢失群");
                                return;
                            }
                        }

                        var roomMember = RoomMemberDao.Instance.GetRoomMemberById(friend.RoomId, Applicate.MyAccount.userId);
                        if (roomMember.role == 4)
                        {
                            HttpUtils.Instance.ShowTip("隐身人不能发送课件");
                            return;
                        }

                        if (roomMember.role == 3)
                        {
                            if (friend.AllowSpeakCourse != 1)
                            {
                                HttpUtils.Instance.ShowTip("已开启普通成员不能发送课件");
                                return;
                            }

                            //管理员和群主除外 是否全体禁言
                            if (friend.AllBannedTalk != 0)
                            {
                                // 全体禁言
                                HttpUtils.Instance.ShowTip("不能发送讲课到全体禁言群");
                                return;
                            }

                            //是否单个禁言
                            if (roomMember.talkTime > TimeUtils.CurrentTime())
                            {
                                HttpUtils.Instance.ShowTip("您已被禁止在此群发言");
                                return;
                            }
                        }

                    }
                }


                HttpUtils.Instance.PopView(frm);

                this.Invoke(new Action(() =>
                {

                    var frmSchedule = new FrmSchedule();
                    frmSchedule.SendCourseMessage(friend, ListMessage);
                    frmSchedule.Compte = () =>
                    {
                        HttpUtils.Instance.PopView(frmSchedule);
                        HttpUtils.Instance.ShowTip("课件发送成功");
                    };
                }));

            });
        }

        private void SendCourseMessage(Friend friend, List<MessageObject> listMessage, int index)
        {
            Thread.Sleep(500);

            MessageObject mess = ShiKuManager.SendForwardMessage(friend, ListMessage[index]);
            Messenger.Default.Send(mess, token: MessageActions.XMPP_UPDATE_NORMAL_MESSAGE);

            if ((++index) < listMessage.Count)
            {
                SendCourseMessage(friend, listMessage, index);
            }
            else
            {
                HttpUtils.Instance.ShowTip("课件发送成功");
            }
        }
        #endregion

        #region 收藏搜索

        // 搜索逻辑代码
        private void SearchMeesageContent(string inputStr)
        {


            HideLodingDialog();

            if (string.IsNullOrEmpty(inputStr))
            {
                lblTitle.Text = btnWhole.Text.Trim();
                // 还原数据
                RevertData();
            }
            else
            {
                lblTitle.Text = LanguageXmlUtils.GetValue("search_result", "搜索结果");
                List<Collections> searchResult = SearchNickName(inputStr);
                if (UIUtils.IsNull(searchResult))
                {
                    mAdapter.BindDatas(new List<Collections>());
                    myTabLayoutPanel1.ClearList();
                }
                else
                {
                    mAdapter.BindDatas(searchResult);
                    myTabLayoutPanel1.SetAdapter(mAdapter);
                }
            }
        }
        /// <summary>
        /// 还原数据
        /// </summary>
        private void RevertData()
        {
            myTabLayoutPanel1.ClearList();
            mAdapter.BindDatas(collectionlst);
            myTabLayoutPanel1.SetAdapter(mAdapter);
        }


        private List<Collections> SearchNickName(string text)
        {
            List<Collections> data = new List<Collections>();
            foreach (var item in collectionlst)
            {
                // 只搜索文字
                if ("5".Equals(item.type) && UIUtils.Contains(item.msg, text))
                {
                    data.Add(item);
                }
            }

            return data;
        }

        private void tvClassTip_Click(object sender, EventArgs e)
        {
            var look = Applicate.GetWindow<FrmLookImage>();
            if (look == null)
            {
                look = new FrmLookImage();
            }
            else
            {
                look.Activate();
            }

            // look.ShowImageList(msgs, 0);
            string url = "https://www.shiku.co/lecture_guide_pc.jpg";
            look.pictureBox1_SetImage(url, false);

        }
        #endregion

        #region 右键删除我的讲课

        private void tsmDelete_Click(object sender, EventArgs e)
        {
            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "user/course/delete")
                .AddParams("access_token", Applicate.Access_Token)
                .AddParams("courseId", mSelectCourse.Name)
                .Build().Execute((suss, data) =>
                {
                    if (suss)
                    {
                        HttpUtils.Instance.ShowTip("删除成功");

                        int index = myColleagueAdapter.GetIndexByName(mSelectCourse.Name);
                        if (index > -1)
                        {
                            myTabLayoutPanel1.RemoveItem(index);
                            myColleagueAdapter.RemoveData(index);
                        }
                    }
                });
        }

        #endregion

        // 查看收藏中的图文
        public void OnClickCollectionImage(Collections data, int index)
        {
            List<MessageObject> message = new List<MessageObject>();
            string[] strarr = data.msg.Split(',');

            foreach (var image in strarr)
            {
                var msg = new MessageObject() { content = image, fileName = data.Filename, fileSize = Convert.ToInt64(data.fileLength), type = kWCMessageType.Image };
                message.Add(msg);
            }


            var frm = Applicate.GetWindow<FrmLookImage>();
            if (frm == null)
            {
                frm = new FrmLookImage();
            }
            else
            {
                frm.Activate();
            }

            frm.ShowImageList(message, index, true);
            frm.Show();
        }

        // 查看收藏中的图视频
        private void OpenVideo(Collections data)
        {
            var video = Applicate.GetWindow<FrmVideoFlash>();
            if (video != null)
            {
                video.Close();
            }

            FrmVideoFlash frm = FrmVideoFlash.CreateInstrance();
            var msg = new MessageObject() { content = data.url, fileName = FileUtils.GetFileName(data.url), fileSize = data.fileSize, type = kWCMessageType.Video };
            frm.VidoShowList(msg);
            frm.isCollect = true;
            frm.Show();
        }

        // 查看收藏中的录音
        private void OpenVoice(Collections data)
        {
            var main = Applicate.GetWindow<FrmMain>();
            var frm = Applicate.GetWindow<FrmvoiceTest>();

            if (frm != null)
            {
                frm.StopPlay();
                frm.Close();
            }

            frm = new FrmvoiceTest();
            var msg = new MessageObject() { content = data.url, fileSize = data.fileSize, timeLen = Convert.ToInt32(data.fileLength) };
            if (!frm.GetDevicde())
            {
                HttpUtils.Instance.ShowTip("未发现音频播放设备");
                return;
            }

            frm.SetAutoClose();
            frm.SetData(msg);
            frm.Size = new Size(myTabLayoutPanel1.Width, 72);
            frm.Location = new Point(main.Location.X + 326, main.Location.Y + 65);
            frm.Show();
        }

        // 查看收藏中的链接
        private void OpenLink(Collections data)
        {

            string url = REUtils.MatchStrUrl(data.msg);
            if (UIUtils.IsNull(url))
            {
                OpenTextLook(data);
                return;
            }


            var msg = new MessageObject() { content = url };

            var frmSeeText = Applicate.GetWindow<FrmBrowser>();
            if (frmSeeText == null)
            {
                frmSeeText = new FrmBrowser();
                frmSeeText.BrowserShow(msg, true);
            }
            else
            {
                frmSeeText.Activate();
                frmSeeText.WindowState = FormWindowState.Normal;
                frmSeeText.BrowserShow(msg, true);
            }
        }

        // 查看文本
        private void OpenTextLook(Collections data)
        {
            var msg = new MessageObject() { content = data.msg };

            var frmSeeText = Applicate.GetWindow<FrmSeeText>();
            if (frmSeeText == null)
            {
                frmSeeText = new FrmSeeText();
                frmSeeText.Longtext = data.msg;
                frmSeeText.collectid = data.emojiId;
                frmSeeText.IsForward = false;
                frmSeeText.iscollect = false;
                frmSeeText.Show();
            }
            else
            {
                frmSeeText.WindowState = FormWindowState.Normal;
                frmSeeText.Activate();
                if (frmSeeText.collectid != data.emojiId)
                {
                    frmSeeText.IsForward = false;
                    frmSeeText.iscollect = false;
                    frmSeeText.Longtext = data.msg;
                    frmSeeText.collectid = data.emojiId;
                }
            }
            //FrmShowText frmShowText = FrmShowText.ShowForm(richTextBox.Height, richTextBox.Font.Height, messageObject.messageId, msg.content);
            //frmShowText.LoadForm();
            //frmShowText.BringToFront();
        }



        public bool isOpenFileing = false;
        // 查看收藏中的图文件
        private void OpenFile(Collections data)
        {

            if (isOpenFileing)
            {
                Applicate.GetWindow<FrmMain>().ShowTip("正在打开文件中…");
                return;
            }

            isOpenFileing = true;
            string url = data.url;
            string sname = FileUtils.GetFileName(url);

            //文件的本地路径
            string localPath = Applicate.LocalConfigData.FileFolderPath + sname;

            //本地已存在该文件，并且不是下载中   这样会有一个bug,就是服务器的文件和实际下载的文件不一样
            if (File.Exists(localPath))
            {
                //打开文件
                //System.Diagnostics.Process.Start(localPath);
                StartExplorer(localPath);

            }
            else
            {
                //开始下载
                DownloadEngine.Instance.DownUrl(url)
                .SavePath(localPath)
                .Down((path) =>
                {
                    if (string.IsNullOrEmpty(path))
                        return;
                    //打开文件
                    StartExplorer(path);
                });
            }
        }



        private void StartExplorer(string path)
        {
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(2500);
                isOpenFileing = false;

            });

            try
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
                psi.Arguments = "/e,/select," + path;
                System.Diagnostics.Process.Start(psi);


            }
            catch (Exception)
            {
                HttpUtils.Instance.ShowTip("不能打开此类型文件");
            }
        }
    }
}