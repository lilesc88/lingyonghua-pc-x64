using agsXMPP;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFrmTalk.Model;
using WinFrmTalk.Properties;
using WinFrmTalk.socket;

namespace WinFrmTalk
{
    public partial class LeftLayout : UserControl
    {

        #region Private Properties
        private MainTabIndex mCurrSelect;
        private MainTabIndex mLastSelect = MainTabIndex.RecentListPage;
        #endregion

        #region Const
        /// <summary>
        /// 显示固定总聊天消息未读数量
        /// </summary>
        public const string NOTIFY_CHAT_UNREADCOUNT = nameof(NOTIFY_CHAT_UNREADCOUNT);
        /// <summary>
        /// 显示固定总联系人未读数量
        /// </summary>
        public const string NOTIFY_CONTACT_UNREADCOUNT = nameof(NOTIFY_CONTACT_UNREADCOUNT);
        #endregion

        /// <summary>
        /// 主窗口对象
        /// </summary>
        public FrmMain MainForm { get; set; }

        /// <summary>
        /// 设置选中的对象
        /// </summary>
        public MainTabIndex SelectIndex
        {
            get { return mCurrSelect; }
            set
            {
                mCurrSelect = value;
                ChangeSelect(SelectIndex);
            }
        }


        private void ChangeSelect(MainTabIndex index)
        {
            if (index == mLastSelect)
            {
                return;
            }

            // 选中当前项
            ChangeViewSelect(index, true);
            // 取消选中上一个项
            ChangeViewSelect(mLastSelect, false);
            mLastSelect = index;
        }


        private void ChangeViewSelect(MainTabIndex index, bool isPress)
        {
            switch (index)
            {
                case MainTabIndex.RecentListPage:

                    btnRecent.Image = isPress ? Resources.ic_nav_chat_press : Resources.ic_nav_chat;//绿色
                    //DisplayChatUnReadCount(totalChatUnreadCount);//重绘消息未读角标
                    break;
                case MainTabIndex.FriendsPage:
                    btnContacts.Image = isPress ? Resources.ic_nav_contact_press : Resources.ic_nav_contact;//绿色
                    //DisplayContactUnReadCount(totalContactUnreadCount);//重绘联系人未读角标
                    break;
                case MainTabIndex.GroupPage:
                    btnGroup.Image = isPress ? Resources.ic_nav_group_press : Resources.ic_nav_group;//绿色
                    break;
                case MainTabIndex.Colleague:
                    btnColleague.Image = isPress ? Resources.ic_nav_colleagues_press : Resources.ic_nav_colleagues;
                    break;
                case MainTabIndex.Collect:
                    btnCollect.Image = isPress ? Resources.ic_nav_collect_press : Resources.ic_nav_collect;
                    break;
                case MainTabIndex.TagPage:
                    btnTags.Image = isPress ? Resources.ic_nav_tag_press : Resources.ic_nav_tag;
                    break;
            }
        }


        #region Constructor
        public LeftLayout()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        #endregion

        #region 初始化字体图标
        /// <summary>
        /// 初始化字体图标
        /// </summary>
        private void InitialIconFont()
        {
            //btnRecent.Font = new Font(Program.ApplicationFontCollection.Families.Last(), 20f);
            //btnContacts.Font = new Font(Program.ApplicationFontCollection.Families.Last(), 20f);
            //btnGroup.Font = new Font(Program.ApplicationFontCollection.Families.Last(), 20f);
            btnSettings.Font = new Font(Program.ApplicationFontCollection.Families.Last(), 20f);
        }
        #endregion

        #region 加载事件

        private void LeftLayout_Load(object sender, EventArgs e)
        {
            try
            {
                InitialIconFont();//初始化字体图标
                InitialMyDetail(false);//设置用户信息
        // 2026-08-16 延迟再刷一次头像（等头像下载/URLDATA 就绪，双保险防新注册用户头像不刷新）
        System.Threading.Timer headTimer2 = null;
        headTimer2 = new System.Threading.Timer(_ =>
        {
            InitialMyDetail(true);
            headTimer2?.Dispose();
        }, null, 2500, System.Threading.Timeout.Infinite);
                RegisterMessenger();
            }
            catch (Exception ex)
            {
                LogUtils.Log("LeftLayout_Load" + ex.Message);
            }
        }
        #endregion

        #region 注册通知
        private void RegisterMessenger()
        {
            #region 最近消息未读角标
            Messenger.Default.Register<int>(this, LeftLayout.NOTIFY_CHAT_UNREADCOUNT, (count) =>
            {
                DisplayChatUnReadCount(count);
            });

            #endregion


            #region 朋友列表的未读角标
            Messenger.Default.Register<int>(this, LeftLayout.NOTIFY_CONTACT_UNREADCOUNT, (count) =>
            {
                DisplayContactUnReadCount(count);
            });

            #endregion

            Messenger.Default.Register<SocketConnectionState>(this, MessageActions.XMPP_UPDATE_STATE, (count) =>
            {
                ChangeXmppState(count);
            });

            #region 刷新头像
            Messenger.Default.Register<string>(this, MessageActions.UPDATE_HEAD, (userId) =>
             {
                 if (Applicate.MyAccount.userId.Equals(userId))
                 {
                     InitialMyDetail(true);
                 }
             });
            #endregion



        }
        #endregion

        #region 显示联系人未读角标
        public void DisplayContactUnReadCount(int unreadcount)
        {
            if (unreadcount > 999)
            {
                unreadcount = 999;
            }

            btnContacts.UnreadCount = unreadcount;
        }
        #endregion

        #region 显示聊天时的未读总数
        /// <summary>
        /// 显示聊天时的未读总数
        /// </summary>
        /// <param name="unreadcount"></param>
        /// 
        public void DisplayChatUnReadCount(int unreadcount)
        {
            if (unreadcount > 999)
            {
                unreadcount = 999;
            }

            btnRecent.UnreadCount = unreadcount;
        }
        #endregion

        #region 设置主窗口自己的信息
        /// <summary>
        /// 设置主窗口自己的信息
        /// </summary>
        private void InitialMyDetail(bool louk)
        {
            if (louk)
            {
                Task.Factory.StartNew(() =>
                {
                    // 修改禅道 #8760
                    Thread.Sleep(600);
                    LoadMyAvatar();
                });
            }
            else
            {
                LoadMyAvatar();
            }
        }

        // 2026-08-31 v23.6 修复：pic_myIcon 的 SizeMode=StretchImage 只对 Image 属性生效，
        // DisplayAvatar 链式 .Background() 设置的是 BackgroundImage → 头像设置后不显示（灰色/空白）。
        // 改用回调方式设置 Image 属性，配合 StretchImage 正确显示头像。
        private void LoadMyAvatar()
        {
            ImageLoader.Instance.DisplayAvatar(Applicate.MyAccount.userId, (bitmap) =>
            {
                if (bitmap == null || pic_myIcon == null || pic_myIcon.IsDisposed)
                {
                    return;
                }
                // v23.10 修复（代码审查 B1）：回调可能来自下载/线程池线程，直接设置控件会跨线程更新
                // 且 CheckForIllegalCrossThreadCalls=false 掩盖问题导致头像偶发不刷新/GDI+ 异常；
                // 统一切回 pic_myIcon 所属 UI 线程再设置。
                Action setAvatar = () =>
                {
                    try
                    {
                        if (pic_myIcon == null || pic_myIcon.IsDisposed || pic_myIcon.Disposing)
                        {
                            return;
                        }
                        pic_myIcon.Image = bitmap;
                    }
                    catch (Exception ex)
                    {
                        ConsoleLog.Output("LoadMyAvatar set image error: " + ex.Message);
                    }
                };
                if (pic_myIcon.InvokeRequired)
                {
                    if (pic_myIcon.IsHandleCreated)
                    {
                        pic_myIcon.BeginInvoke(setAvatar);
                    }
                }
                else
                {
                    setAvatar();
                }
            });
        }
        #endregion

        #region 设置点击事件
        /// <summary>
        /// 设置点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Settings_Click(object sender, EventArgs e)
        {
            if (e is MouseEventArgs me)
                if (me.Button != MouseButtons.Left)
                    return;

            //FrmTest test = new FrmTest();
            //test.Show();
            //return;

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
                tmpset.WindowState = FormWindowState.Normal; ;
            }
        }
        #endregion

        #region 最近消息点击
        private void btnRecent_Click(object sender, EventArgs e)
        {
            if (e is MouseEventArgs me)
                if (me.Button != MouseButtons.Left)
                    return;

            Console.WriteLine("btnRecent_Click");
            SelectIndex = MainTabIndex.RecentListPage;

            MainForm.SelectIndex = MainTabIndex.RecentListPage;
        }
        #endregion

        #region 最近消息双击
        private void OnDoubleClickRecent(object sender, EventArgs e)
        {
            Console.WriteLine("OnDoubleClickRecent");
            var recent = MainForm.GetRecentList();
            var uncount = recent.NextUnpoint();
            // 重绘消息未读角标
            DisplayChatUnReadCount(uncount);
        }
        #endregion

        #region 联系人点击
        private void btnContacts_Click(object sender, EventArgs e)
        {
            if (e is MouseEventArgs me)
                if (me.Button != MouseButtons.Left)
                    return;

            SelectIndex = MainTabIndex.FriendsPage;
            MainForm.SelectIndex = MainTabIndex.FriendsPage;
        }
        #endregion

        #region 群组点击
        private void btnGroup_Click(object sender, EventArgs e)
        {
            if (e is MouseEventArgs me)
                if (me.Button != MouseButtons.Left)
                    return;

            SelectIndex = MainTabIndex.GroupPage;
            MainForm.SelectIndex = MainTabIndex.GroupPage;
        }
        #endregion

        #region 我的同事点击
        private void BtnCon_Click(object sender, EventArgs e)
        {
            if (e is MouseEventArgs me)
                if (me.Button != MouseButtons.Left)
                    return;

            SelectIndex = MainTabIndex.Colleague;
            MainForm.SelectIndex = MainTabIndex.Colleague;
            //刷新收藏页面
            MainForm.ColleagueList.RefreData();
        }
        #endregion

        #region 标签显示
        private void BtnTags_Click(object sender, EventArgs e)
        {
            if (e is MouseEventArgs me)
                if (me.Button != MouseButtons.Left)
                    return;

            SelectIndex = MainTabIndex.TagPage;
            MainForm.SelectIndex = MainTabIndex.TagPage;

            MainForm.UserTagPage.DataLableLoad();
        }
        #endregion

        #region 收藏点击
        private void BtnCollect_Click(object sender, EventArgs e)
        {
            if (e is MouseEventArgs me)
                if (me.Button != MouseButtons.Left)
                    return;

            SelectIndex = MainTabIndex.Collect;
            MainForm.SelectIndex = MainTabIndex.Collect;
            MainForm.MyCollection.RefreData();//刷新收藏页面
        }
        #endregion

        #region 头像点击
        private void pic_myIcon_Click(object sender, EventArgs e)
        {
            if (e is MouseEventArgs me)
                if (me.Button != MouseButtons.Left)
                    return;

            FrmFriendsBasic detial = new FrmFriendsBasic();
            detial.ShowUserInfoById(Applicate.MyAccount.userId);//显示个人详情
        }
        #endregion

        public Color Statebrg = Color.Gray;

        private void ChangeXmppState(SocketConnectionState state)
        {
            //Console.WriteLine("ChangeXmppState" + state);
            switch (state)
            {
                case SocketConnectionState.Authenticated:
                    Statebrg = ColorTranslator.FromHtml("#0AD007"); // 变绿色 - 在线
                    this.Refresh();
                    break;
                case SocketConnectionState.Disconnected:
                    Statebrg = ColorTranslator.FromHtml("#BBBBBB"); // 变灰色 - 离线
                    this.Refresh();
                    break;

                default:
                    Statebrg = Color.Yellow;// 变黄色 - 连接中
                    this.Refresh();
                    break;
            }
        }

        private void pic_myIcon_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.DrawEllipse(new Pen(Statebrg), new Rectangle(pic_myIcon.Width - 14, pic_myIcon.Height - 14, 12, 12));
            Brush b = new SolidBrush(Statebrg);
            g.FillEllipse(b, new Rectangle(pic_myIcon.Width - 14, pic_myIcon.Height - 14, 12, 12));
        }

        #region 点击所有的位置均让弹出菜单失焦隐藏
        private void BindClick_LostFocus()
        {
            //this.Click += (sender, e) =>
            //{
            //    MainForm.leftLayout_List.Visible = false;
            //};
            //foreach (Control crl in this.Controls)
            //{
            //    if (crl == btnSettings)
            //        continue;
            //    crl.Click += (sender, e) =>
            //    {
            //        MainForm.leftLayout_List.Visible = false;
            //    };
            //}
        }
        #endregion

    }
}