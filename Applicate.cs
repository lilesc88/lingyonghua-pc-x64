using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using WinFrmTalk.Dictionarys;
using WinFrmTalk.Model;
using Microsoft.WindowsAPICodePack.Shell;
using WinFrmTalk.View;
using System.Configuration;
using System.Threading;

namespace WinFrmTalk
{
    public static class Applicate
    {
        #region 固定配置变量
       
       	public const string APP_CONFIG = "http://207.56.20.218:8094/config";

        // 自动解析域名 : 测试环境false  正式环境true
        public const bool AutoResolveDomain = true;

        // API_KEY值 
        public const string API_KEY = "";

        // 软件版本号
        public const string APP_VERSION = "1.0.2";

        // 是否能切换服务器
        public static bool ToggleService = false;

        // 数据库版本号
        public const int DB_VERSION = 31;

        // 能否导出聊天记录
        public const bool IsInputChatList = true;

        // 是否音视频版本包
        public const bool ENABLE_MEET = false;

        // 当前.net版本
        public const float CURRET_VERSION = 4.0f;

        // 是否开启自动加群
        public const bool ENABLE_AUTO_JOINROOM = true;

        // 是否开启红包
        public const bool ENABLE_RED_PACKAGE = true;

        // 默认拉起漫游初始时间 秒
        public const long DEF_START_TIME = 1546315200;

        // 默认字体样式
        public const string SetFont = "微软雅黑";

        // 判断是否已经打开音视频拨打界面或者接听界面
        public static bool IsOpenFrom = true;

        // 转发限制人数
        public const int ForwardMaxCount = 99;

        // 是否客服模式
        public static bool ServiceMode = false;

        // 是否开启端到端加密 - 默认关闭由服务器控制开关
        public static bool ENABLE_ASY_ENCRYPT = false;

        // 是否开启消息记录页图片显示缩略图
        public static bool ENABLE_Thumbnail = true;

        // APP名称
        public const string APP_NAME = "IM";
        //软件名称用于文件见保存
        public const string SoftWareName = "IMFiles";
        //是否显示消息弹窗
        public static bool ShowMsgTipWindowd = true;

        // 是否显示专业客服
        public const bool ShowKefuMode = true;

        // 是否启用国际化
        public static bool ENABLE_MULTI_LANGUAGE = true;    //appconfig读取不到的时候，会自动设为false
        #endregion

        #region 运行时缓存变量 

        private static JsonConfigData _URLDATA = new JsonConfigData();
        private static string access_Token;
        private static DataOfUserDetial myAccount = new DataOfUserDetial();

        /// <summary>
        /// 服务配置数据
        /// </summary>
        public static JsonConfigData URLDATA
        {
            get { return _URLDATA; }
            set { _URLDATA = value; }
        }

        /// <summary>
        /// 应用程序接口访问令牌
        /// </summary>
        public static string Access_Token
        {
            get { return access_Token; }
            set { access_Token = value; }
        }

        /// <summary>
        /// 登陆后服务器返回的httpkey 用于接口验参
        /// </summary>
        public static string HTTP_KEY { get; set; }

        /// <summary>
        /// 当前的用户
        /// </summary>
        public static DataOfUserDetial MyAccount
        {
            get { return myAccount; }
            set { myAccount = value; }
        }
        /// <summary>
        /// 当前软件路径
        /// </summary>
        //软件路径是指运行环境生成的路径例如： 当前运行的是bin-debug-x86
        public static string AppCurrentDirectory
        {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }

        /// <summary>
        /// 软件文件路径，使用软件时候所有的分件会固定保存在这个路径下，这个路径下包括公用路径和当前账号路径
        /// </summary>
        public static string AppCurrentFeileDirectory
        {
            get
            {

                string path = KnownFolders.Documents.Path + @"\" + SoftWareName + @"\";

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }
        /// <summary>
        /// 当前账号路径
        /// 这个路径下保存的是这个账号相关的文件资源
        /// </summary>
        public static string AppCurrentPerson
        {
            get
            {
                string config = APP_CONFIG.Replace("http://", "").Replace(".", "").Replace("/config", "").Replace("www:", "").Replace(":", "").Replace("//", "").Replace("com", "");
                string path = AppCurrentFeileDirectory + Applicate.MyAccount.areaCode + Applicate.MyAccount.Telephone + config + @"\";

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }
        /// <summary>
        /// 公用目录 很多个账号同时都需要共享的文件资源
        /// </summary>
        public static string AppPublicDirectory
        {
            get
            {
                string path = AppCurrentFeileDirectory + @"All Users\";

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        private static Dictionary<string, string> _fdNames = new Dictionary<string, string>();

        /// <summary>
        /// 只记录好友的备注名
        /// key: userId, value: userName
        /// </summary>
        public static Dictionary<string, string> FdNames
        {
            get
            {
                if (_fdNames.Count < 1)
                {
                    var list = new Friend().GetAllFriends();
                    foreach (Friend fd in list)
                        _fdNames.Add(fd.UserId, string.IsNullOrWhiteSpace(fd.RemarkName) ? "" : fd.RemarkName);
                }

                return _fdNames;
            }
        }

        /// <summary>
        /// 默认的文本字体格式
        /// </summary>
        public static Font myFont = new Font(Applicate.SetFont, 10F);

        /// <summary>
        /// 用户是否通过密码验证
        /// true时Xmpp断线会重连
        /// false时Xmpp不会重连
        /// </summary>
        public static bool IsAccountVerified { get; set; }

        /// <summary>
        /// 获取当前操作系统是否为win7
        /// </summary>
        public static bool IsWin7System { get => UIUtils.GetOSystem_IsWin7(); }
        #endregion

        #region 登录后初始化账号数据
        /// <summary>
        /// 完成登录初始化账号数据，初始化并连接Xmpp
        /// <para> lzq-3.12 </para>
        /// </summary>
        /// <param name="item"></param>
        public static void InitAccountData(UserInfo user, string phone, string pwd, bool formLogin)
        {
            Applicate.Access_Token = user.access_token; //设置APIToken
            Applicate.IsAccountVerified = true; //记录为已登录
            Applicate.HTTP_KEY = user.httpKey;
            Applicate.MyAccount = new DataOfUserDetial()
            {
                userId = user.userId,
                nickname = user.nickname,
                Telephone = phone,
                password = pwd
            }; //赋值全局变量

            if (formLogin)
            {
                // 清除当前用户头像缓存，防止其他端修改pc不刷新问题
                ImageLoader.Instance.RefreshAvatar(Applicate.MyAccount.userId);
                // 2026-08-16 通知主界面立即刷新头像（LeftLayout 监听 UPDATE_HEAD 重新 DisplayAvatar）
                // 2026-08-16 延迟通知主界面刷新头像（等 FrmMain/LeftLayout 创建完成 + URLDATA 就绪后再刷）
                System.Threading.Timer headTimer = null;
                headTimer = new System.Threading.Timer(_ =>
                {
                    Messenger.Default.Send<string>(Applicate.MyAccount.userId, MessageActions.UPDATE_HEAD);
                    headTimer?.Dispose();
                }, null, 2000, System.Threading.Timeout.Infinite);
            }

            // 初始化用户设置 
            InitUserSetting(user.settings);

            // 设置最后离线时间
            CorrectOfflineTime(user.login);

            // 保存当前数据库版本
            LocalDataUtils.SetIntData("db_version" + Applicate.MyAccount.Telephone, DB_VERSION);
        }

        /// <summary>
        ///  初始化用户设置
        /// </summary>
        public static void InitUserSetting(UserSettings settings)
        {
            // 是否开启多点登录
            MultiDeviceManager.Instance.IsEnable = settings.multipleDevices == 1;
            if (myAccount != null)
            {
                myAccount.isShowMsgState = settings.isShowMsgState == 1;
                myAccount.sendInput = false;
            }
        }

        /// <summary>
        /// 设置离线时间，用于解决其他端查看了消息，PC还认为没有查看
        /// </summary>
        private static void CorrectOfflineTime(LoginInfo loginInfo)
        {
            string quitTime = LocalDataUtils.GetStringData(Applicate.QUIT_TIME);
            if (UIUtils.IsNull(quitTime))
            {
                if (loginInfo == null || loginInfo.offlineTime == 0)
                {
                    Applicate.MyAccount.OfflineTime = 1546315200;
                }
                else
                {
                    Applicate.MyAccount.OfflineTime = loginInfo.offlineTime;
                }
            }
            else
            {
                if (loginInfo == null || loginInfo.offlineTime == 0)
                {
                    Applicate.MyAccount.OfflineTime = Convert.ToInt64(quitTime) - 5;
                }
                else
                {
                    Applicate.MyAccount.OfflineTime = Math.Max(Convert.ToInt64(quitTime), loginInfo.offlineTime);

                }

            }
        }

        /// <summary>
        /// 用户离线key
        /// </summary>
        public static string QUIT_TIME
        {
            get { return "QUIT_TIME_" + MyAccount.userId; }
        }
        #endregion

        #region 本地化路径配置
        /// <summary>
        /// 本地配置数据(包括下载路径,头像地址)
        /// </summary>
        public static LocalConfig LocalConfigData { get; set; } =
            new LocalConfig
            {


                EmojiFolderPath = AppPublicDirectory + "Res\\Emoji\\",
                GifFolderPath = AppPublicDirectory + "Res\\Gif\\",
                //VideoFolderPath = AppCurrentPerson + "Downloads\\ShikuIM\\Audio\\",
                //ImageFolderPath = AppCurrentPerson + "Downloads\\ShikuIM\\Image\\",
                //VoiceFolderPath = AppCurrentPerson + "Downloads\\ShikuIM\\voice\\",
                //FileFolderPath = AppCurrentPerson + "Downloads\\ShikuIM\\File\\",
                //TempFilepath = AppCurrentPerson + "Downloads\\ShikuIM\\temp\\",
                //RoomFileFolderPath = AppCurrentPerson + "Downloads\\ShikuIM\\roomFile\\"
            };
        /// <summary>
        /// 文件夹包含文件复制
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="aimPath"></param>

        public static void copyDir(string srcPath, string aimPath)
        {
            try
            {
                //如果不存在目标路径，则创建之

                if (!System.IO.Directory.Exists(aimPath))
                {
                    System.IO.Directory.CreateDirectory(aimPath);
                }
                //令目标路径为aimPath\srcPath
                string srcdir = System.IO.Path.Combine(aimPath, System.IO.Path.GetFileName(srcPath));
                //如果源路径是文件夹，则令目标目录为aimPath\srcPath\
                if (Directory.Exists(srcPath))
                    srcdir += Path.DirectorySeparatorChar;
                // 如果目标路径不存在,则创建目标路径
                if (!System.IO.Directory.Exists(srcdir))
                {
                    System.IO.Directory.CreateDirectory(srcdir);
                }
                //获取源文件下所有的文件
                String[] files = Directory.GetFileSystemEntries(srcPath);
                foreach (string element in files)
                {
                    //如果是文件夹，循环
                    if (Directory.Exists(element))
                        copyDir(element, srcdir);
                    else
                        File.Copy(element, srcdir + Path.GetFileName(element), true);
                }
            }
            catch
            {
                Console.WriteLine("无法复制");
            }
        }
        #endregion

        #region 清除虚拟路径数据库

        public static void RefreshDataBase()
        {
            // 更新了数据库
            int dbVersion = LocalDataUtils.GetIntData("db_version" + Applicate.MyAccount.Telephone);
            if (dbVersion != DB_VERSION)
            {
                ClearLocalDataBase();
                ClearVirtualDatabase();
                return;
            }

            // 切换了服务器地址也要更新数据
            string lasturl = "http://118.89.73.191:9094/config";
            //string lasturl = LocalDataUtils.GetStringData("last_login_service");
            if (!string.Equals(lasturl, UIUtils.GetServer()))
   
            {
                ClearLocalDataBase();
            }
        }

        /// <summary>
        /// 只能在未登陆时调用
        /// </summary>
        public static void ClearVirtualDatabase()
        {
            // 这里是为了去清除 安装目录的所有数据
            // windows的坑 安装路径并不在C盘的pro,而是在被转移到了另一个位置，卸载时还不会删除，当数据库结构发送变化就会出问题
            string path = string.Format(@"C:\Users\{0}\AppData\Local\VirtualStore\Program Files (x86)\ShiKuIM\db", Environment.UserName);
            LogUtils.Save("获取到的虚拟路径：" + Environment.UserName);
            if (Directory.Exists(path))
            {
                DirectoryInfo root = new DirectoryInfo(path);
                FileInfo[] files = root.GetFiles();
                foreach (var item in files)
                {
                    if (!string.Equals("constant.db", item.Name) && !string.Equals("shiku.db", item.Name))
                    {
                        try

                        {

                            item.Delete();

                        }

                        catch (Exception ex)

                        {

                            // 文件被占用（数据库连接未释放/其他实例）时跳过，不阻塞登录

                            LogUtils.Error("删除本地文件失败：" + item.Name + " " + ex.Message);

                        }
                    }
                }
            }
        }

        public static void ClearLocalDataBase()
        {
            // 这里时为了解决切换服务器时，旧数据的数据没有清除，导致串了服务器的数据，导致数据异常
            string path = Applicate.AppCurrentPerson;
             
            LogUtils.Save("获取到的本地路径：" + Environment.UserName);
            if (Directory.Exists(path))
            {
                DirectoryInfo root = new DirectoryInfo(path);
                FileInfo[] files = root.GetFiles();
                foreach (var item in files)
                {
                    if (!string.Equals("constant.db", item.Name) && !string.Equals("shiku.db", item.Name))
                    {
                        try

                        {

                            item.Delete();

                        }

                        catch (Exception ex)

                        {

                            // 文件被占用（数据库连接未释放/其他实例）时跳过，不阻塞登录

                            LogUtils.Error("删除本地文件失败：" + item.Name + " " + ex.Message);

                        }
                    }
                }
            }
        }

        #endregion

        #region 本地化路径配置
        public static ConfigData GetDefConfig()
        {
            ConfigData data = new ConfigData();
           
            return data;
        }

        #endregion

        #region 获取已打开的窗口

        /// <summary>
        /// 获取已打开的窗口
        /// </summary>
        /// <typeparam name="T">窗口对象</typeparam>
        /// <returns></returns>
        public static T GetWindow<T>()
        {
            //T form = default(T);
            try
            {
                foreach (var tmpform in Application.OpenForms)
                {
                    if (tmpform is T)
                    {
                        return (T)tmpform;
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtils.Log("Get form error =========++++=+=+=+=+=+=+=+===" + ex.Message);
            }

            return default(T);
        }

        public static List<T> GetWindows<T>()
        {
            try
            {
                List<T> windows = new List<T>();
                foreach (var tmpform in Application.OpenForms)
                {
                    if (tmpform is T)
                    {
                        windows.Add((T)tmpform);
                    }
                }
                return windows;
            }
            catch (Exception ex)
            {
                LogUtils.Log("Get form error =========++++=+=+=+=+=+=+=+===" + ex.Message);
            }

            return new List<T>();
        }

        /// <summary>
        /// 标记是否可以打开录制
        /// </summary>
        /// <returns></returns>
        public static bool IsOpenRecordView(FrmBase frm = null)
        {

            // 已经打开了录像窗口，不能开启录制
            if (GetWindow<FrmTakeVideo>() != null)
            {
                if (frm == null)
                {
                    GetWindow<FrmMain>().ShowTip("请先关闭录像窗口");
                }
                else
                {
                    frm.ShowTip("请先关闭录像窗口");
                }

                return true;
            }

            // 已经开启了拍照窗口，不能开启录制
            if (GetWindow<FrmTakePhoto>() != null)
            {
                if (frm == null)
                {
                    GetWindow<FrmMain>().ShowTip("请先关闭拍照窗口");
                }
                else
                {
                    frm.ShowTip("请先关闭拍照窗口");
                }
                return true;
            }

            // 已经开启了录像窗口，不能开启录制
            if (GetWindow<FrmRecordVideo>() != null)
            {
                if (frm == null)
                {
                    GetWindow<FrmMain>().ShowTip("请先关闭录像窗口");
                }
                else
                {
                    frm.ShowTip("请先关闭录像窗口");
                }
                return true;
            }

            // 这里空了
             
            var main = GetWindow<FrmMain>();
            if (main.mainPageLayout.IsOpenRecordVoice() ) 
            {
                if (frm == null)
                {
                    GetWindow<FrmMain>().ShowTip("请先关闭当前录音");
                }
                else
                {
                    frm.ShowTip("请先关闭当前录音");
                }
                return true;
            };

            return false;
        }

        /// <summary>
        /// 登录冲突后 关闭所有窗体，并显示登录窗口
        /// </summary>
        internal static void ConflictClose()
        {
            HttpUtils.Instance.CloseAll();
        }
        #endregion

        #region 判断当前对象是否正在聊天
        /// <summary>
        /// 判断当前对象是否正在聊天
        /// </summary>
        /// <returns></returns>
        public static bool IsChatFriend(string userId)
        {
            Dictionary<string, MessageObjectDataDictionary> data = ChatTargetDictionary.GetChatTargetDictionary();
            return data.ContainsKey(userId);
        }

        #endregion

        #region liuhuan 2019/5/31 防止多次点击同一个窗体的变量
        public static bool isopen = true;
        public static List<string> userlst = new List<string>();//群管理用
        public static List<string> imagelist = new List<string>();//图片集合
        #endregion

        #region 关于线程

        /// <summary>
        /// 主线程的线程ID
        /// </summary>
        public static int MainThreadId { get; set; }

        /// <summary>
        /// 访问主线程的主体
        /// </summary>
        public static SynchronizationContext SyncMainContext { get; set; }    //由于入口是在登录窗口，所以登录时初始化，获得主线程

        public static bool IsMainThread()
        {
            return Thread.CurrentThread.ManagedThreadId == MainThreadId;
        }
        #endregion
    }
}

