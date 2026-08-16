using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFrmTalk.Controls.CustomControls;
//using WinFrmTalk.Controls.CustomControls;
using WinFrmTalk.Model;
using WinFrmTalk.Properties;
using WinFrmTalk.View;

namespace WinFrmTalk
{

    /// <summary>
    /// 主窗口页面显示定义
    /// </summary>
    public enum MainTabIndex
    {

        /// <summary>
        /// 最近消息列表
        /// </summary>
        RecentListPage,

        /// <summary>
        /// 最近消息列表
        /// </summary>
        RecentListPage_null,

        /// <summary>
        /// 联系人
        /// </summary>
        FriendsPage,

        /// <summary>
        /// 群组
        /// </summary>
        GroupPage,

        /// <summary>
        /// 好友验证页面
        /// </summary>
        FriendVerifyPage,

        /// <summary>
        /// 黑名单列表
        /// </summary>
        BlockPage,

        /// <summary>
        /// 同事页面
        /// </summary>
        Colleague,

        /// <summary>
        /// 收藏页面
        /// </summary>
        Collect,
        TagPage
    }

    /// <summary>
    /// 主窗口
    /// </summary>
    public partial class FrmMain : FrmBase
    {

        #region Const
        /// <summary>
        /// 通知显示提示消息
        /// </summary>
        public const string NOTIFY_NOTICE = "notify_tip_notice";

        //public bool isStattvoice = true;
        /// <summary>
        /// 开始新聊天
        /// </summary>
        public const string START_NEW_CHAT = nameof(START_NEW_CHAT);
        #endregion

        private MainTabIndex index;

        #region Public Member
        public MainTabIndex SelectIndex
        {
            get { return index; }
            set
            {
                index = value;
                switch (index)
                {
                    case MainTabIndex.RecentListPage:
                        recentListLayout.Visible = true;//显示最近消息列表
                        lblSessionSubTitle.Visible = true;//显示在线状态
                        groupListLayout.Visible = false;//隐藏群组列表
                        friendListLayout.Visible = false;//隐藏好友列表
                        ColleagueList.Visible = false;
                        MyCollection.Visible = false;
                        UserTagPage.Visible = false;
                        break;
                    case MainTabIndex.FriendsPage:
                    case MainTabIndex.FriendVerifyPage:
                    case MainTabIndex.BlockPage:
                        friendListLayout.Visible = true;
                        recentListLayout.Visible = false;
                        groupListLayout.Visible = false;
                        lblSessionSubTitle.Visible = false;
                        ColleagueList.Visible = false;
                        MyCollection.Visible = false;
                        UserTagPage.Visible = false;


                        if (mainPageLayout.IsCurrtFirend(Friend.ID_NEW_FRIEND))
                        {
                            // 关闭未读数量红点
                            friendListLayout.UpdateItemUnread(Friend.ID_NEW_FRIEND, 0);
                            // 清空了新的好友数量，所以只需要查询黑名单数量通知到左侧更新就行
                            int num1 = new Friend() { UserId = Friend.ID_BAN_LIST }.GetNuRedNum();
                            leftlayout.DisplayContactUnReadCount(num1);
                        }

                        if (mainPageLayout.IsCurrtFirend(Friend.ID_BAN_LIST))
                        {

                            // 关闭黑名单未读数量红点
                            friendListLayout.UpdateItemUnread(Friend.ID_BAN_LIST, 0);
                            // 清空了黑名单数量，所以只需要查询新的朋友数量通知到左侧更新就行
                            int num2 = new Friend() { UserId = Friend.ID_NEW_FRIEND }.GetNuRedNum();
                            leftlayout.DisplayContactUnReadCount(num2);
                        }
                        break;
                    case MainTabIndex.GroupPage:
                        groupListLayout.Visible = true;
                        recentListLayout.Visible = false;
                        lblSessionSubTitle.Visible = false;
                        friendListLayout.Visible = false;
                        ColleagueList.Visible = false;
                        MyCollection.Visible = false;
                        UserTagPage.Visible = false;
                        break;
                    case MainTabIndex.Colleague:
                        ColleagueList.Visible = true;
                        MyCollection.BringToFront();
                        groupListLayout.Visible = false;
                        recentListLayout.Visible = false;
                        lblSessionSubTitle.Visible = false;
                        friendListLayout.Visible = false;
                        MyCollection.Visible = false;
                        UserTagPage.Visible = false;
                        break;
                    case MainTabIndex.Collect:
                        ColleagueList.Visible = false;
                        ColleagueList.BringToFront();
                        MyCollection.Visible = true;
                        groupListLayout.Visible = false;
                        recentListLayout.Visible = false;
                        lblSessionSubTitle.Visible = false;
                        friendListLayout.Visible = false;
                        UserTagPage.Visible = false;
                        break;
                    case MainTabIndex.TagPage:
                        UserTagPage.Visible = true;
                        UserTagPage.BringToFront();
                        MyCollection.Visible = false;
                        groupListLayout.Visible = false;
                        recentListLayout.Visible = false;
                        lblSessionSubTitle.Visible = false;
                        friendListLayout.Visible = false;
                        break;
                    default:
                        break;
                }
                mainPageLayout.SelectedIndex = index;
            }
        }
        #endregion

        private delegate void DelegateString(string msg);

        private Icon mLogoInco;
        private Icon mTranInco;

        // 判断窗口是否处于焦点|前置
        public bool IsActivate = false;
        public bool isExit = false;

        // 是否开启闪动
        private bool TwinkState = true;
        // 是否开启声音
        private bool SoundState = true;


        #region Contructor
        /// <summary>
        /// Contructor
        /// </summary>
        public FrmMain()
        {
            InitializeComponent();
            //InitialNotifyIcon();
            //  FormClosing += ClosingHandle;
            SelectIndex = MainTabIndex.RecentListPage;
            leftlayout.MainForm = this;
            friendListLayout.MainForm = this;
            groupListLayout.MainForm = this;
            recentListLayout.MainForm = this;
            mainPageLayout.MainForm = this;

            mLogoInco = (Icon)Icon.FromHandle((Resources.Logo).GetHicon()).Clone();//2026-08-17 独立副本
            mTranInco = (Icon)Icon.FromHandle((Resources.trans).GetHicon()).Clone();//2026-08-17 独立副本
            this.OnMainHide += OnMainHideHanlder;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(FrmMain_FormClosing);
            this.Icon = (Icon)Properties.Resources.Icon.Clone();//加载icon图标（独立副本，避免共享句柄被释放）
            // 清理 30 天前的图片/视频/语音/文件缓存（后台线程，不阻塞界面）
            try
            {
                System.Threading.Thread clearCache = new System.Threading.Thread(ClearOldCache);
                clearCache.IsBackground = true;
                clearCache.Start();
            }
            catch (Exception ex)
            {
                ConsoleLog.Output(ex.Message);
            }
        }

        /// <summary>
        /// 清理 30 天前的缓存文件（图片/视频/语音/文件），防止本地缓存无限膨胀
        /// </summary>
        private void ClearOldCache()
        {
            try
            {
                string dir = Applicate.AppCurrentPerson + @"Downloads\ShikuIM\";
                if (!System.IO.Directory.Exists(dir))
                {
                    return;
                }
                DateTime deadline = DateTime.Now.AddDays(-30);
                // 覆盖全部缓存子目录（注意 voice 是小写，Video 原配置曾误指向 Audio）
                string[] subs = { "Image", "Video", "Audio", "voice", "File", "temp", "roomFile" };
                foreach (string sub in subs)
                {
                    string subDir = dir + sub;
                    if (!System.IO.Directory.Exists(subDir))
                    {
                        continue;
                    }
                    foreach (string f in System.IO.Directory.GetFiles(subDir, "*", System.IO.SearchOption.AllDirectories))
                    {
                        try
                        {
                            if (System.IO.File.GetLastWriteTime(f) < deadline)
                            {
                                System.IO.File.Delete(f);
                            }
                        }
                        catch (Exception ex)
                        {
                            ConsoleLog.Output(ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleLog.Output(ex.Message);
            }
        }

        private void RefreshMainMenuSet()
        {
            string loginuserId = Applicate.MyAccount.userId;
            // 是否开启闪动
            TwinkState = LocalDataUtils.GetIntData(loginuserId + "twink_state", 1) == 1;
            // 是否开启声音
            SoundState = LocalDataUtils.GetIntData(loginuserId + "sound_state", 1) == 1;

            tsbCloseFlicker.Checked = TwinkState;
            tsbClosevoice.Checked = SoundState;


        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isExit)
            {
                e.Cancel = false;//取消关闭
            }
            else
            {
                e.Cancel = true;//取消关闭
                this.WindowState = FormWindowState.Normal;//最小化窗体
                this.Hide();
            }

            // this.ShowInTaskbar = false;//不在任务栏显示
        }
        #endregion

        #region 窗口关闭时

        private void ClosingHandle(object sender, FormClosingEventArgs e)
        {
            isTwinkle = false;
            isExit = true;
            // 保存用户退出时间
            LocalDataUtils.SetStringData(Applicate.QUIT_TIME, TimeUtils.CurrentIntTime().ToString());
            Application.Exit();
        }
        #endregion

        #region 窗口隐藏时

        private void OnMainHideHanlder()
        {
            mainPageLayout.OnMainHide();
        }
        #endregion

        #region 发送消息至好友
        public void SendMessageToFriend(Friend friend)
        {
            if (IsSeparateChatFriend(friend.UserId))
            {
                LogUtils.Log("激活独立窗口");
            }
            else
            {
                this.SelectIndex = MainTabIndex.RecentListPage;//显示最近消息列表
                this.leftlayout.SelectIndex = MainTabIndex.RecentListPage;//改变左侧按钮颜色


                MessageObject local = new MessageObject() { FromId = friend.UserId }.GetLastMessage();
                if (local == null || UIUtils.IsNull(local.messageId) || UIUtils.IsNull(local.content) || local.type == kWCMessageType.kWCMessageTypeNone)
                {
                    friend.Content = string.Empty;
                    friend.UpdateLastContent(string.Empty, TimeUtils.CurrentTimeDouble());
                }
                else
                {
                    string lastContent = friend.ToLastContentTip(local.type, local.content, local.fromUserId, local.fromUserName, local.toUserName);
                    lastContent = local.isReadDel == 1 ? "[阅后即焚消息]" : lastContent;
                    friend.Content = lastContent;
                    friend.UpdateLastContent(lastContent, TimeUtils.CurrentTimeDouble());
                }

                recentListLayout.SendMessageToFriend(friend);//发送消息
            }
        }
        #endregion

        #region 窗口加载事件
        /// <summary>
        /// 窗口加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmMain_Load(object sender, EventArgs e)
        {
            // 高分辨率适配：确保窗口不超过屏幕可用区域
            var workingArea = Screen.GetWorkingArea(this.Location);
            if (this.Width > workingArea.Width || this.Height > workingArea.Height)
            {
                int newWidth = Math.Min(this.Width, workingArea.Width);
                int newHeight = Math.Min(this.Height, workingArea.Height);
                this.Size = new Size(newWidth, newHeight);
                // 居中显示
                this.StartPosition = FormStartPosition.CenterScreen;
                this.Location = new Point(
                    workingArea.X + (workingArea.Width - this.Width) / 2,
                    workingArea.Y + (workingArea.Height - this.Height) / 2);
            }
            // 动态设置最小尺寸，不超过可用区域
            this.MinimumSize = new Size(
                Math.Min(785, workingArea.Width),
                Math.Min(660, workingArea.Height));

            InitialFontIcons();

            RegisterMessenger();

            plList.Height = this.Height;
            replace = true;
            RefreshIcon();
            // 刷新右键菜单设置声音和闪动
            RefreshMainMenuSet();
            ////注册热键Ctrl+B，Id号为101。HotKey.KeyModifiers.Ctrl也可以直接使用数字2来表示。   
            //HotKey.RegisterHotKey(Handle, 101, HotKey.KeyModifiers.Ctrl, Keys.B);
            ////注册热键Ctrl+Alt+D，Id号为102。HotKey.KeyModifiers.Alt也可以直接使用数字1来表示。   
            //HotKey.RegisterHotKey(Handle, 102, HotKey.KeyModifiers.Alt | HotKey.KeyModifiers.Ctrl, Keys.D);
            ////注册热键F5，Id号为103。   
            //HotKey.RegisterHotKey(Handle, 103, HotKey.KeyModifiers.None, Keys.F5);

            Microsoft.Win32.SystemEvents.TimeChanged += SystemEvents_TimeChanged;
        }

        /// <summary>
        /// 监听系统时间改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private bool isUpdateTime = false;
        private void SystemEvents_TimeChanged(object sender, EventArgs e)
        {
            if (isUpdateTime)
            {
                isUpdateTime = false;
                Console.WriteLine("用户修改了本地时间：{0:g}", DateTime.Now);
                TimeUtils.SyscHttpTime();
                return;
            }

            // 每次改变系统时间会回调两次，所以用update过滤掉一次
            if (!isUpdateTime)
            {
                isUpdateTime = true;
            }
        }
        #endregion

        //protected override void WndProc(ref Message m)
        //{
        //    const int WM_HOTKEY = 0x0312;
        //    //按快捷键    
        //    switch (m.Msg)
        //    {
        //        case WM_HOTKEY:
        //            switch (m.WParam.ToInt32())
        //            {
        //                case 100:    //按下的是alt+z  
        //                             //CaptureImageTool capture = new CaptureImageTool();

        //                    //capture.ShowDialog();

        //                    //MouseEventArgs e = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);
        //                    //mainPageLayout.sendMsgPanel.LblScreen_MouseClick(null, e);
                            //    break;
        //                    //case 101:    //按下的是Ctrl+B   
        //                    //    //此处填写快捷键响应代码   
        //                    //    this.Text = "按下的是Ctrl+B";
        //                    //    break;
        //                    //case 102:    //按下的是Alt+D   
        //                    //    //此处填写快捷键响应代码   
        //                    //    this.Text = "按下的是Ctrl+Alt+D";
        //                    //    break;
        //                    //case 103:
        //                    //    this.Text = "F5";
        //                    //    break;
        //            }
                            //    break;
        //    }
        //    base.WndProc(ref m);
        //}
        /// <summary>
        /// 是否正在进行闪烁
        /// </summary>
        public bool isTwinkle;
        private Thread taskTwinkle;


        #region 注册消息以接收通知
        /// <summary>
        /// 注册消息以接收通知 (内部委托需单独列为方法，以在窗口关闭时注销通知)
        /// </summary>
        ///
        ///
        /// 
        private void RegisterMessenger()
        {
            Messenger.Default.Register<MessageObject>(this, CommonNotifications.XmppMsgRecived, ProcessNewMessage);
            Messenger.Default.Register<Friend>(this, FrmMain.START_NEW_CHAT, SendMessageToFriend);
            Messenger.Default.Register<string>(this, MessageActions.CLEAR_FRIEND_MSGS, ClearFriendMessages);
            Messenger.Default.Register<string>(this, MessageActions.RESTART_APP, RestartApp);
            Messenger.Default.Register<string>(this, MessageActions.CLOSE_NOTIFY_INCO, CloseNotifyInco);
            Messenger.Default.Register<MessageObject>(this, MessageActions.XMPP_UPDATE_NORMAL_MESSAGE, (item) =>
            {  
                //是否为一条正常群组的消息
                if (item.isGroup == 1)
                {
                    Friend group = new Friend() { UserId = item.FromId }.GetFdByUserId();
                    if (group == null || group.Status != 2)
                    {
                        return;
                    }
                }

                if (item.IsVisibleMsg() && item.fromUserId != Applicate.MyAccount.userId)
                {
                    //如果在独立窗口中
                    if (Applicate.IsChatFriend(item.fromUserId) && !item.fromUserId.Equals(this.mainPageLayout.sendMsgPanel.ChooseTarget.UserId))
                        return;

                    if (SoundState && item.Nodisturb == 0)
                    {
                        PlayTipSound();
                    }

                    // 主窗口处于非激活状态 才去闪动图标
                    if (!IsActivate && !isTwinkle)
                    {
                        // 关闭闪动选项开启时 才去闪动图标
                        if (TwinkState)
                        {
                            // 非免打扰消息 才去闪动图标
                            if (item.Nodisturb == 0)
                            {
                                // 如果没有闪动就开启闪动
                                isTwinkle = true;
                                taskTwinkle = new Thread(TimeTwinkleTick);
                                taskTwinkle.Start();
                            }
                        }
                    }

                    // 主窗口不处于激活状态，有新消息显示提示窗

                        Invoke(new Action(() =>
                        {
                            Friend sender = item.GetFriend();

                        if (!Applicate.ShowMsgTipWindowd)
                        {
                            return;
                        }

                            if (sender == null || UIUtils.IsNull(sender.UserId))
                            {
                                return;
                            }

                            if (sender.IsGroup == 1 && sender.Status == 0)
                            {
                                // 直播间消息过滤掉
                                return;
                            }

                            if (sender.Nodisturb == 1)
                            {
                                // 开启了消息免打扰
                                return;
                            }

                        this.FlashWin();

                        if (this.Visible)
                        {
                            return;
                        }

                        if (!Applicate.ShowMsgTipWindowd)
                        {
                            return;
                        }

                            FrmNewsTip frmNewsTip = null;
                            List<FrmNewsTip> windows = Applicate.GetWindows<FrmNewsTip>();
                            if (windows.Count > 0)
                            {
                                //遍历所有FrmNewsTip查询是否已存在该对象的窗体
                                foreach (FrmNewsTip window in windows)
                                {
                                    if (sender.UserId.Equals(window.sender.UserId))
                                    {
                                        window.RefreshData(item);
                                        window.TopMost = true;
                                        return;
                                    }
                                }
                            }
                            frmNewsTip = new FrmNewsTip();
                            frmNewsTip.BindData(sender, item);
                            frmNewsTip.Show();
                            frmNewsTip.TopMost = true;
                        }));


                    //if ("关闭闪动".Equals(tsbCloseFlicker.Text))
                    //{
                    //    if (!isTwinkle && item.Nodisturb == 0)
                    //    {
                    //        if (!mainPageLayout.IsChatFriend(item.ChatJid))
                    //        {

                    //        }
                    //        else if (!IsActivate)
                    //        {
                    //            isTwinkle = true;
                    //            taskTwinkle = new Thread(TimeTwinkleTick);
                    //            taskTwinkle.Start();
                    //        }
                    //    }
                    //}
                }
            });

            Messenger.Default.Register<int>(this, LeftLayout.NOTIFY_CHAT_UNREADCOUNT, (count) =>
            {
                if (count == 0 && IsActivate)
                {
                    isTwinkle = false;
                    replace = true;
                    RefreshIcon();
                }
            });
        }


        long playtime;
        private void PlayTipSound()
        {
            long curr = TimeUtils.CurrentTime();
            if (curr - playtime > 2)
            {
                playtime = curr;
                SoundPlayer soundPlayer = new SoundPlayer("Tips.wav");
                soundPlayer.Play();
            }

        }
        #endregion

        #region 加载主窗口数据
        public void MainLoadData(string phone)
        {
            TimeUtils.SyscHttpTime();
            //UpdateMyInfo();
            recentListLayout.LoadData();
            friendListLayout.LoadData();
            groupListLayout.LoadRoomList();

            Task.Factory.StartNew(() =>
            {
                ShiKuManager.InitialXmpp();
            });

            LocalDataUtils.SetIntData("just_forgetpwd" + phone, 0);
        }

        //private void UpdateMyInfo()
        //{
        //    HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "user/get")
        //       .AddParams("access_token", Applicate.Access_Token)
        //       .Build().Execute((suss, data) =>
        //       {
        //           if (suss)
        //           {
        //               string dhPublicKey = UIUtils.DecodeString(data, "dhMsgPublicKey");
        //               string encryptDhPriKey = UIUtils.DecodeString(data, "dhMsgPrivateKey");
        //               if (!UIUtils.IsNull(encryptDhPriKey))
        //               {
        //                   string dhPrivateKey = SecureChatUtil.AesDecryptDHPrivateKey(Applicate.MyAccount.password, encryptDhPriKey);
        //                   Applicate.MyAccount.dhPrivateKey = dhPrivateKey;
        //                   Applicate.MyAccount.dhPublicKey = dhPublicKey;
        //               }

        //               string rsaPublicKey = UIUtils.DecodeString(data, "rsaMsgPublicKey");
        //               string encryptRsaPriKey = UIUtils.DecodeString(data, "rsaMsgPrivateKey");
        //               if (!UIUtils.IsNull(encryptRsaPriKey))
        //               {
        //                   string rsaPrivateKey = AES.DecryptBase64(encryptRsaPriKey, Applicate.MyAccount.password);
        //                   Applicate.MyAccount.rsaPublicKey = rsaPublicKey;
        //                   Applicate.MyAccount.rsaPrivateKey = rsaPrivateKey;
        //               }

        //           }
        //       });
        //}
        #endregion

        #region 设置选中最近消息项
        /// <summary>
        /// 设置选中最近消息项(用于显示最近的)
        /// </summary>
        /// <param name="currentItem"></param>
        public void SetRecentSelectedItem(NewsItem currentItem)
        {
            mainPageLayout.SetChoiceFriend(currentItem.FriendData, currentItem.FriendData.MsgNum);//设置对话好友
            if (currentItem.FriendData.MsgNum > 0)
                currentItem.FriendData.MsgNum = 0;//不显示未读数量
        }
        #endregion

        #region 初始化字体图标
        /// <summary>
        /// 初始化字体图标
        /// </summary>
        private void InitialFontIcons()
        {
            NotifyControl.MouseDoubleClick += (sen, ev) =>
            {
                //this.WindowState = FormWindowState.Normal;
                MainShow();
                isTwinkle = false;
                replace = true;
                RefreshIcon();
            };
        }
        #endregion

      
        #region 显示选中群详情
        public void SelectGroup(Friend item)
        {
            mainPageLayout.SelectGroup(item);
        }
        #endregion

        #region 清除单个好友聊天记录
        public void ClearFriendMessages(string toUserid)
        {
            if (Thread.CurrentThread.IsBackground)
            {
                var main = new DelegateString(ClearFriendMessages);
                Invoke(main, toUserid);
                return;
            }

            // 清除聊天记录
            MessageObject message = new MessageObject() { FromId = toUserid };
            message.DeleteTable();

            // 清除服务器数据
            ClearServerFriendMsg(toUserid);
        }
        #endregion

        #region 处理新消息
        /// <summary>
        /// 处理新消息
        /// </summary>
        /// <param name="newMsg"></param>
        private void ProcessNewMessage(MessageObject newMsg)
        {

        }
        #endregion


        public void GetDrop(DragEventArgs eve)
        {
            if (eve.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] paths = (string[])eve.Data.GetData(System.Windows.DataFormats.FileDrop);
                if (paths != null && paths.Length != 0)
                {
                    if (!paths[0].Contains("."))//如果不是文件不操作
                    {
                        return;
                    }
                    if (paths.Length > 1)//发送多个文件只能以文件方式发送
                    {
                        foreach (var item in paths)
                        {
                            LogUtils.Log("FrmMain_DragDrop" + item);
                            /*
                            ShiKuManager.SendMessageFile(new MessageListItem
                            {
                                Jid = Sess.Jid,
                                ShowTitle = Sess.NickName,
                                MessageItemContent = Sess.MyMemberNickname
                            }, item);*/
                        }
                    }
                    else//单个文件时
                    {
                        string FileType = paths[0].Substring(paths[0].LastIndexOf('.'), paths[0].Length - paths[0].LastIndexOf('.'));
                        switch (FileType)
                        {
                            case ".png":
                            case ".jpg":
                            case ".jpeg":
                            case ".bmp":
                            case ".gif":
                                //SendImageTooltipVisible = true;
                                //SendFileTooltipVisible = false;
                                break;
                            default:
                                //SendImageTooltipVisible = false;
                                //SendFileTooltipVisible = true;
                                break;
                        }

                    }
                }
            }
        }

        private void leftlayout_Load(object sender, EventArgs e)
        {

        }

        private void FrmMain_SizeChanged(object sender, EventArgs e)
        {
            plList.Height = this.Height;
            // mainPageLayout.Height =(int) (this.Height * 0.62);
            // mainPageLayout.Width = (int)(this.Width * 0.96);
            mainPageLayout.Width = this.Width - mainPageLayout.Location.X;
            Console.WriteLine(mainPageLayout.sendMsgPanel.xListView.vScrollBar.Location);
            mainPageLayout.Height = this.Height - mainPageLayout.Location.Y - 1;

        }
        /// <summary>
        /// 截图快捷键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmMain_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.KeyCode == Keys.Z && e.Modifiers == Keys.Alt)        //alt+Z
            //{
            //    CaptureImageTool capture = new CaptureImageTool();
            //    capture.ShowDialog();
            //}
        }

        private void FrmMain_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Console.WriteLine("e.KeyChar is " + e.KeyChar + "\r\ne.Handled is " + e.Handled);


        }


        public void ClearServerFriendMsg(string toUserid)
        {
            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "tigase/emptyMyMsg")
                .AddParams("access_token", Applicate.Access_Token)
                .AddParams("type", "0")
                .AddParams("toUesrId", toUserid)
                .Build().Execute(null);
        }

        private void FrmMain_DoubleClick(object sender, EventArgs e)
        {

        }


        #region 任务栏的右键菜单

        /// <summary>
        /// 显示主窗体
        /// </summary>
        public void MainShow()
        {
            //this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            this.Show();
            this.Activate();

            RefreshList();
        }


        /// <summary>
        /// 显示主界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbManin_Click(object sender, EventArgs e)
        {
            MainShow();
        }

        private void RefreshList()
        {
            mainPageLayout.RefreshView();
            switch (SelectIndex)
            {
                case MainTabIndex.RecentListPage:
                    recentListLayout.Refresh();
                    lblSessionSubTitle.Visible = false;
                    break;
                case MainTabIndex.FriendsPage:
                case MainTabIndex.FriendVerifyPage:
                case MainTabIndex.BlockPage:
                    friendListLayout.Refresh();
                    break;
                case MainTabIndex.GroupPage:
                    groupListLayout.Refresh();
                    break;
                case MainTabIndex.Colleague:
                    ColleagueList.Refresh();
                    break;
                case MainTabIndex.Collect:
                    MyCollection.Refresh();
                    break;
                case MainTabIndex.TagPage:
                    UserTagPage.Refresh();
                    break;
              
                default:
                    break;
            }

        }

        /// <summary>
        /// 关闭闪动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbCloseFlicker_Click(object sender, EventArgs e)
        {
            TwinkState = !TwinkState;
       
            tsbCloseFlicker.Checked = TwinkState;
            if (TwinkState)
            {
                isTwinkle = false;
                replace = true;
            }
            RefreshIcon();

            int value = TwinkState ? 1 : -1;
            // 是否开启闪动
            LocalDataUtils.SetIntData(Applicate.MyAccount.userId + "twink_state", value);

        }

        /// <summary>
        /// 关闭声音
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbClosevoice_Click(object sender, EventArgs e)
        {  
            SoundState = !SoundState;
            tsbClosevoice.Checked = SoundState;

            int value = SoundState ? 1 : -1;
            LocalDataUtils.SetIntData(Applicate.MyAccount.userId + "sound_state", value);
        }

        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbSet_Click(object sender, EventArgs e)
        {
            var tmpset = Applicate.GetWindow<FrmSet>();
            var parent = Applicate.GetWindow<FrmMain>();
            if (tmpset == null)
            {
                var set = new FrmSet();
                set.Location = new Point(parent.Location.X + (parent.Width - set.Width) / 2, parent.Location.Y + (parent.Height - set.Height) / 2);//居中
                set.Show();
            }
            else
            {
                tmpset.Activate();
                tmpset.WindowState = FormWindowState.Normal;
            }
        }

        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbExit_Click(object sender, EventArgs e)
        {
            LocalDataUtils.SetStringData(Applicate.QUIT_TIME, TimeUtils.CurrentIntTime().ToString());
            ShiKuManager.ApplicationExit();

            //释放NotifyControl的所有资源，以保证托盘图标在程序关闭时立即消失
            NotifyControl.Dispose();
            //此处调用退出接口
         
            Application.ExitThread();
            Application.Exit();
            Process.GetCurrentProcess().Kill();
        }


        #endregion

        public bool replace = false;

        public void RefreshIcon()
        {
            if (replace)
            {
                NotifyControl.Icon = mLogoInco;
            }
            else
            {
                NotifyControl.Icon = mTranInco;
            }
        }

        private void TimeTwinkleTick()
        {
            while (isTwinkle)
            {
                Thread.Sleep(500);

                if (isTwinkle)
                {
                    replace = !replace;
                    if (!isExit && !this.IsDisposed)
                    {
                        Invoke(new Action(RefreshIcon));
                    }

                }
                else
                {
                    replace = true;
                    if (!isExit)
                    {
                        Invoke(new Action(RefreshIcon));
                    }
                    return;
                }
            }
        }

        private void FrmMain_ResizeEnd(object sender, EventArgs e)
        {
            //this.Text = "2width:" + this.Width.ToString() + " height:" + this.Height.ToString();
            //if (this.Width <= 770)
            //{
            //    this.Width = 770;
            //}

            //if (this.Height <= 660)
            //{
            //    this.Height = 660;
            //}
        }

        #region 是否正在独立窗口聊天
        // 判断该id是否正在独立窗口
        public bool IsSeparateChatFriend(string userId)
        {

            if (Applicate.IsChatFriend(userId))
            {
                if (mainPageLayout.IsChatFriend(userId))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        private void FrmMain_Leave(object sender, EventArgs e)
        {
            HotKey.UnregisterHotKey(Handle, 100);
        }

        private void FrmMain_Activated(object sender, EventArgs e)
        {
            //注册热键Shift+S，Id号为100。HotKey.KeyModifiers.Shift也可以直接使用数字4来表示。   
            HotKey.RegisterHotKey(Handle, 100, HotKey.KeyModifiers.Alt, Keys.Z);
            //Console.WriteLine("获取焦点");
            IsActivate = true;

            // 批量发送已读
            if (!UIUtils.IsNull(mainPageLayout.sendMsgPanel.DeactivateMsgList))
            {
                foreach (MessageObject msg in mainPageLayout.sendMsgPanel.DeactivateMsgList)
                {
                    //获取聊天对象
                    Friend sendFd = mainPageLayout.sendMsgPanel.ChooseTarget;
                    //new Friend() { UserId = msg.ToId.Equals(Applicate.MyAccount.userId) ? msg.FromId : msg.ToId }.GetFdByUserId();
                    int role = mainPageLayout.sendMsgPanel.msgTabAdapter.MyCurrtRole;
                    ShiKuManager.SendReadMessage(sendFd, msg, role);
                }

                if (mainPageLayout.sendMsgPanel.DeactivateMsgList != null)
                {
                    mainPageLayout.sendMsgPanel.DeactivateMsgList.Clear();
                }
            }
        }

        private void FrmMain_Deactivate(object sender, EventArgs e)
        {
            //注册热键Shift+S，Id号为100。HotKey.KeyModifiers.Shift也可以直接使用数字4来表示。   
            //Console.WriteLine("失去焦点");
            if (ShiKuManager.IsAuthenticated())
            {
                //防止被异常杀死记录不了最后离线消息了
                LocalDataUtils.SetStringData(Applicate.QUIT_TIME, TimeUtils.CurrentIntTime().ToString());
            }

            IsActivate = false;
        }


        public RecentListLayout GetRecentList()
        {
            return recentListLayout;
        }

        private void CloseNotifyInco(string str)
        {

            if (Thread.CurrentThread.IsBackground)
            {
                var main = new DelegateString(CloseNotifyInco);
                Invoke(main, str);
                return;
            }

            NotifyControl.Visible = false;

        }
        private void RestartApp(string str)
        {
            LogUtils.Error("RestartApp triggered! code=" + str + " stack=" + new System.Diagnostics.StackTrace().ToString());

            if (Thread.CurrentThread.IsBackground)
            {
                var main = new DelegateString(RestartApp);
                Invoke(main, str);
                return;
            }

            ShiKuManager.ApplicationExit();
            LocalDataUtils.SetStringData(Applicate.QUIT_TIME, TimeUtils.CurrentIntTime().ToString());
            //此处调用退出接口
            Application.ExitThread();
            Application.Exit();
            Application.Restart();
            Process.GetCurrentProcess().Kill();
        }

        private void Leftlayout_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //FrmTest frmTest = new FrmTest();
            //frmTest.Show();
        }
    }
}
