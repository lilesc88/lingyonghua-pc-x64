using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using WinFrmTalk.Properties;
using WinFrmTalk.Model;
using WinFrmTalk.Helper;

namespace WinFrmTalk.View
{
    public partial class FrmVideoFlash : FrmBase
    {
        #region 全局变量
        private VlcPlayer player;
        public int Seconds { get; private set; }
        //路径
        public string LocationPath;
        private MessageObject messageList;
    // 2026-08-16 视频翻页：当前聊天所有视频消息列表 + 索引
    private List<MessageObject> mVideoList;
    private int mIndex;
        private bool isMessageObject;
        //大小
        public long fileSize;
        //单例
        private static FrmVideoFlash frm = null;
        private static LodingUtils loding = null;
        internal string FilePath;
        public string fileName;
        public bool isVideo;
        private double timer = 0.00;
        public bool isCollect { get; set; }//判断是不是

        #endregion
        #region 窗体加载
        //单例模式
        public static FrmVideoFlash CreateInstrance()
        {
            if (frm == null || frm.IsDisposed)
            {
                frm = new FrmVideoFlash();
            }
            if (frm != null)
            {
                frm.Activate();
            }
            return frm;
        }
        private FrmVideoFlash()
        {
            InitializeComponent();
            this.Icon = (Icon)Properties.Resources.Icon.Clone();//加载icon图标（独立副本，避免共享句柄被释放）
            this.SizeChanged += FrmVideoFlash_SizeChanged;
        }

        /// <summary>
    /// 2026-08-17 键盘左右键翻页视频（按钮已去除，键盘翻页不遮挡播放）
    /// </summary>
    private void FrmVideoFlash_KeyDown(object sender, KeyEventArgs e)
    {
        if (mVideoList == null || mVideoList.Count == 0) return;
        if (e.KeyCode == Keys.Left && mIndex > 0)
        {
            StopCurrentPlay();
            mIndex--;
            VidoShowList(mVideoList[mIndex]);
        }
        else if (e.KeyCode == Keys.Right && mIndex < mVideoList.Count - 1)
        {
            StopCurrentPlay();
            mIndex++;
            VidoShowList(mVideoList[mIndex]);
        }
    }

    private void FrmVideoFlash_SizeChanged(object sender, EventArgs e)
        {
            musicBar1.Refresh();
        }

        private void FrmVideoFlash_Load(object sender, EventArgs e)
        {
            if (isCollect) pboxColle.Visible = false;
            //跨线程
            Control.CheckForIllegalCrossThreadCalls = false;
            //验证文件是否存在
            if (!isMessageObject)
            {
                if (IsFileExists(fileName))
                {
                    pboxDown.Visible = false;
                    pboxFile.Visible = true;
                    pboxFile.Location = pboxDown.Location;//换坐标
                    FilePath = Path.GetDirectoryName(FilePath);
                }
            }
            //下载好了的视频
            else
            {

                string path = LocalDataUtils.GetStringData(messageList.content);
                if (File.Exists(path))
                {
                    // 下载好了
                    pboxDown.Visible = false;
                    pboxFile.Visible = true;
                    pboxFile.Location = pboxDown.Location;//换坐标
                    FilePath = Path.GetDirectoryName(path);
                }

            }
        }
        #endregion
        #region 验证方法
        /// <summary>
        /// 下载方法
        /// </summary>
        /// <param name="toFilePath"></param>
        /// <param name="fileUrl"></param>
        private void DownFile(string toFilePath, string fileUrl)
        {
            HttpDownloader.DownloadFile(fileUrl, toFilePath, (path) =>
            {
                if (!string.IsNullOrEmpty(path))
                {
                    HttpUtils.Instance.ShowTip("下载成功");
                }
            });
        }
        /// <summary>
        /// 文件是否存在
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool IsFileExists(string name)
        {
            // 判断下载地址是否有这个文件
            string filePath = Applicate.LocalConfigData.RoomFileFolderPath + name;
            return File.Exists(filePath);
        }
        #endregion
        #region 参数传递
        /// <summary>
        /// 群文件传递
        /// </summary>
        /// <param name="url"></param>
        public void VidoShow(string LocationPath)
        {
            //LocationPath = url;
            isMessageObject = false;
            pboxColle.Visible = false;
            if (!isVideo)
            {
                VidoPlay(LocationPath);

            }
            isVideo = true;
        }
        /// <summary>
        /// 聊天框传递
        /// </summary>
        /// <param name="messageObjects"></param>
        public void VidoShowList(MessageObject message)
        {
            if (message == null)
            {
                return;
            }
            pboxColle.Visible = true;
            messageList = message;
            isMessageObject = true;
            if (!isVideo)
            {
                LocationPath = message.fileName;    //自己发送的文件是有本地路径的
                if (!File.Exists(LocationPath))
                {
                    string filepath = message.content;
                    string fileName = FileUtils.GetFileName(filepath);
                    LocationPath = Applicate.LocalConfigData.VideoFolderPath + fileName;
                    if (!File.Exists(LocationPath))
                    {
                        // 2026-08-16 改为先下载到本地再播放（流式播放对慢速网络不可靠，易黑屏）
                        loding = new LodingUtils();
                        loding.parent = pboxVideo;
                        loding.size = pboxVideo.Size;
                        loding.start();
                        string toPath = LocationPath;
                        string toDir = Path.GetDirectoryName(toPath);
                        if (!string.IsNullOrEmpty(toDir) && !Directory.Exists(toDir))
                        {
                            Directory.CreateDirectory(toDir);
                        }
                        HttpDownloader.DownloadFile(filepath, toPath, (downPath) =>
                        {
                            if (this.IsHandleCreated)
                            {
                                this.Invoke(new Action(() =>
                                {
                                    PlayDownloadedVideo(toPath);
                                }));
                            }
                            else
                            {
                                PlayDownloadedVideo(toPath);
                            }
                        });
                        isVideo = true;
                        return;
                    }
                }
                VidoPlay(LocationPath);
                //复制过去
                //else
                //{
                //    //LocationPath = Applicate.LocalConfigData.AudioFolderPath + fileName;
                //    //FileInfo fileInfo = new FileInfo(filepath);
                //    VidoPlay(LocationPath);
                //    //fileInfo.CopyTo(LocationPath, true);
                //    //loding.stop();
                //}
            }
            isVideo = true;
        }
        /// <summary>
        /// 播放已下载好的本地视频（下载完成后调用）
        /// </summary>
        private void PlayDownloadedVideo(string path)
        {
            // 2026-08-17 下载失败/文件无效时停止加载，不播放空文件
            if (string.IsNullOrEmpty(path) || !File.Exists(path) || new FileInfo(path).Length == 0)
            {
                if (loding != null)
                {
                    try { loding.stop(); } catch (Exception ex) { ConsoleLog.Output(ex.Message); }
                    loding = null;
                }
                return;
            }
            try
            {
                if (loding != null)
                {
                    try { loding.stop(); } catch (Exception ex) { ConsoleLog.Output(ex.Message); }
                    loding = null;
                }
                LocationPath = path;
                VidoPlay(path);
            }
            catch (Exception ex)
            {
                ConsoleLog.Output(ex.Message);
            }
        }

        //播放的方法
        private void VidoPlay(string LocationPath)
        {
            tmrVideo.Enabled = true;
            //LocationPath = url;
            try
            {
                // 重置播放进度状态（防止上次播放残留影响新视频进度条）
                timer = 0;
                lastPlayTime = -1;
                sameTimeCount = 0;
                musicBar1.Value = 0.0;
                musicBar1.Visible = true;

                string pluginPath = Applicate.AppCurrentDirectory + "plugins\\";  //插件目录
                player = new VlcPlayer(pluginPath);
                player.SetRenderWindow(pboxVideo.Handle);//panel
                if (LocationPath != null && LocationPath.StartsWith("http"))
                {
                    player.PlayUrl(LocationPath);//网络视频流式播放
                }
                else
                {
                    player.LoadFile(LocationPath);//视频文件
                    player.Play();                     //视频长度
                }
                int durationSecond = (int)this.player.Duration;
                string Min = durationSecond / 60 + "";
                if (Min.Length == 1)
                {
                    Min = "0" + Min;
                }
                Seconds = durationSecond % 60;
                string secos = Seconds.ToString();
                if (secos.Length == 1)
                {
                    secos = "0" + secos;
                }
                string endTime = Min + ":" + secos;
                //结束时间
                lblEndTime.Text = endTime;
                //最大值
                if (timer >= player.Duration && player.Duration > 0)
                {
                    timer = 0;
                    musicBar1.Value = 0.0;
                    //停止计时器
                    pboxStar.Visible = false;
                    pboxStop.Visible = true;
                    pboxStop.Location = pboxStar.Location;
                }
            }
            catch (Exception ex)
            {
                // 播放初始化失败：安全停止，避免程序崩溃；重置状态允许重试
                tmrVideo.Stop();
                try
                {
                    if (player != null)
                    {
                        player.Release();
                    }
                }
                catch (Exception ex2)
                {
                    ConsoleLog.Output(ex2.Message);
                }
                player = null;
                isVideo = false;
                if (loding != null)
                {
                    try { loding.stop(); } catch (Exception ex3) { ConsoleLog.Output(ex3.Message); }
                    loding = null;
                }
                ConsoleLog.Output(ex.Message);
            }
        }
        #endregion
        #region 按钮功能
        //暂停
        private void pboStop_Click(object sender, EventArgs e)
        {
            if (player == null) return;
            musicBar1.Visible = true;
            pboxStar.Visible = true;
            pboxStop.Visible = false;
            player.Play();
            tmrVideo.Start();
            if (player.Duration > 0 && timer >= player.Duration)
            {
                timer = 0;
                player.Stop();
                musicBar1.Value = 0.0;
                tmrVideo.Stop();
                tmrVideo.Start();
                player.Play();
                tmrVideo_Tick(sender, e);
                pboxStar.Visible = true;
                pboxStop.Visible = false;
            }

        }
        //播放(快速的暂停跟继续导致进度条没有跟上，将计时器的频率值修改到100就可解决)
        private void pboxStar_Click(object sender, EventArgs e)
        {
            if (player == null) return;
            pboxStar.Visible = false;
            pboxStop.Visible = true;
            pboxStop.Location = pboxStar.Location;
            player.Pause();

            tmrVideo.Stop();

        }
        #endregion
        #region 2026-08-16 视频翻页（上一个/下一个）
        /// <summary>
        /// 显示视频列表并定位到指定索引（支持上一个/下一个翻看）
        /// </summary>
        public void ShowVideoList(List<MessageObject> videoList, int index)
        {
            if (videoList == null || videoList.Count == 0 || index < 0 || index >= videoList.Count)
            {
                return;
            }
            mVideoList = videoList;
            mIndex = index;
            StopCurrentPlay();
            VidoShowList(videoList[index]);
            // UpdateNavButtons(); //2026-08-17 按钮已去除
        }
        /// <summary>停止当前播放并释放资源（切换视频前调用）</summary>
        private void StopCurrentPlay()
        {
            try
            {
                if (tmrVideo != null) tmrVideo.Stop();
                if (player != null)
                {
                    try { player.Stop(); } catch (Exception ex) { ConsoleLog.Output(ex.Message); }
                    try { player.Release(); } catch (Exception ex) { ConsoleLog.Output(ex.Message); }
                    player = null;
                }
                if (loding != null)
                {
                    try { loding.stop(); } catch (Exception ex) { ConsoleLog.Output(ex.Message); }
                    loding = null;
                }
                isVideo = false; // 重置，下次 VidoShowList 会重新设置播放路径
                musicBar1.Value = 0.0;
            }
            catch (Exception ex)
            {
                ConsoleLog.Output(ex.Message);
            }
        }
        #endregion

        #region 进度条
        /// <summary>
        /// 播放进度（连续不增长次数，用于未知时长的网络流判断播放结束）
        /// </summary>
        private int sameTimeCount = 0;
        private double lastPlayTime = -1;
        /// <summary>
        /// 播放进度条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrVideo_Tick(object sender, EventArgs e)
        {
            if (player == null)
            {
                tmrVideo.Stop();
                return;
            }
            double duration = player.Duration;
            if (duration > 0)
            {
                // 已知时长：正常推进
                timer += 0.1;
                // 画面已出则关掉加载圈
                if (timer > 0.3 && loding != null)
                {
                    try { loding.stop(); } catch (Exception ex) { ConsoleLog.Output(ex.Message); }
                    loding = null;
                }
                if (timer >= duration)
                {
                    timer = duration;
                    musicBar1.Value = 100;
                    tmrVideo.Stop();
                    pboxStar.Visible = false;
                    pboxStop.Visible = true;
                    pboxStop.Location = pboxStar.Location;
                    musicBar1.Visible = false;
                }
                else
                {
                    musicBar1.Value = timer / duration * 100;
                }
            }
            else
            {
                // 未知时长（网络流解析不到）：用播放器实际进度判断结束，进度条不除零
                double t = player.GetPlayTime();
                if (t < 0) t = 0;
                if (t <= 0)
                {
                    // 尚未起播，等待缓冲
                    lastPlayTime = 0;
                    sameTimeCount = 0;
                    return;
                }
                // 有画面了，关掉加载圈
                if (loding != null)
                {
                    try { loding.stop(); } catch (Exception ex) { ConsoleLog.Output(ex.Message); }
                    loding = null;
                }
                if (lastPlayTime > 0 && Math.Abs(t - lastPlayTime) < 0.05)
                {
                    sameTimeCount++;
                    // 连续约2秒（20次×100ms）时间不再前进 → 视为播放结束
                    if (sameTimeCount >= 20)
                    {
                        timer = t;
                        tmrVideo.Stop();
                        pboxStar.Visible = false;
                        pboxStop.Visible = true;
                        pboxStop.Location = pboxStar.Location;
                        musicBar1.Visible = false;
                        try { player.Stop(); } catch (Exception ex) { ConsoleLog.Output(ex.Message); }
                        sameTimeCount = 0;
                    }
                }
                else
                {
                    sameTimeCount = 0;
                }
                lastPlayTime = t;
            }
        }
        /// <summary>
        /// 拖动播放进度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void musicBar1_MouseClick(object sender, MouseEventArgs e)
        {
            if (player == null) return;
            //当前值
            //player.Stop();
            // tmrVideo.Stop();


            //if (timer >= player.Duration)
            //{
            //    pboxStar.Visible = true;
            //    pboxStop.Visible = false;
            //    player.Play();
            //    tmrVideo.Start();
            //}

            if (player.Duration <= 0) return; // 未知时长无法定位
            double pos = e.X / (double)musicBar1.Width * musicBar1.MaxValue;
            musicBar1.Value = pos;
            double time = pos / musicBar1.MaxValue * player.Duration;
            timer = time;
            player.SetPlayTime(time);


            //  tmrVideo.Start();
            //tmrVideo.Start();
            //设置时间后进度条有更新，但是视频没有

        }
        #endregion

        #region 图标功能
        /// <summary>
        /// 声音
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pboxVoice_Click(object sender, EventArgs e)
        {
            if (player == null) return;
            if (player.GetVolume() > 0)
            {
                player.SetVolume(0);
                pboxSound.Image = Resources.mute;
            }
            else
            {
                player.SetVolume(100);
                pboxSound.Image = Resources.volume;

            }
        }
        /// <summary>
        /// 转发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pboxZhuan_Click(object sender, EventArgs e)
        {
            string httpurl = "";
            string locapath = "";
            if (isMessageObject)
            {
                fileSize = messageList.fileSize;
                httpurl = messageList.content;
                string fileName = FileUtils.GetFileName(httpurl);
                LocationPath = Applicate.LocalConfigData.VideoFolderPath + fileName;

            }
            else
            {
                //群文件传过来的路径
                httpurl = LocationPath;
                locapath = FilePath;
            }
            //长度
            long size = fileSize;
            //开好友选择器


            var view = Applicate.GetWindow<FrmFriendSelect>();
            if (view != null)
            {
                //ShowTip("转发窗口已打开");
                view.Activate();
                return;
            }

            //调用好友选择器
            var frmFriend = new FrmFriendSelect();
            //数据加载
            frmFriend.LoadFriendsData();
            frmFriend.AddConfrmListener((userLiser) =>
            {
                //添加
                foreach (var item in userLiser)
                {
                    //调研xmpp
                    MessageObject messImg = ShiKuManager.SendVideoMessage(item.Value, httpurl, locapath, size);
                    Messenger.Default.Send(messImg, token: MessageActions.XMPP_UPDATE_NORMAL_MESSAGE);
                }

            });
        }
        /// <summary>
        /// 收藏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pboxColle_Click(object sender, EventArgs e)
        {
            if (isMessageObject)
            {
                CollectUtils.CollectMessage(messageList);
            }

        }
        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pboxFile_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            //打开跟目录
            openFile.InitialDirectory = FilePath;
            //名称
            openFile.Title = LanguageXmlUtils.GetValue("choose_file", "请选择");
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                //播放
                LocationPath = openFile.FileName;
                System.Diagnostics.Process.Start(LocationPath);
            }
        }
        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pboxDown_Click_1(object sender, EventArgs e)
        {
            try
            {
                string foldPath = "";
                //文件路径
                string filePath = "";
                string httpUrl = "";
                if (isMessageObject && messageList != null)
                {
                    filePath = LocationPath;
                    httpUrl = messageList.content;
                }
                else
                {
                    filePath = LocationPath;
                }
                //文件名（本地路径取不到就取 URL 的文件名）
                fileName = FileUtils.GetFileName(!string.IsNullOrEmpty(filePath) ? filePath : httpUrl);
                if (string.IsNullOrEmpty(fileName))
                {
                    HttpUtils.Instance.ShowTip("文件不存在");
                    return;
                }
                //选择框
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.Description = LanguageXmlUtils.GetValue("choose_file_path", "请选择文件路径");
                //确认选择
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    foldPath = dialog.SelectedPath + "\\" + fileName;
                    if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    {
                        // 本地文件存在：直接复制
                        File.Copy(filePath, foldPath, true);
                        if (messageList != null)
                        {
                            LocalDataUtils.SetStringData(messageList.content, foldPath);
                        }
                        HttpUtils.Instance.ShowTip("下载成功");
                    }
                    else if (!string.IsNullOrEmpty(httpUrl))
                    {
                        // 流式播放未落盘：走 HTTP 下载
                        HttpDownloader.DownloadFile(httpUrl, foldPath, (p) =>
                        {
                            HttpUtils.Instance.ShowTip(string.IsNullOrEmpty(p) ? "下载失败" : "下载成功");
                        });
                    }
                    else
                    {
                        HttpUtils.Instance.ShowTip("文件不存在");
                        return;
                    }
                    //获去到更目录
                    FilePath = Path.GetDirectoryName(foldPath);
                    //隐藏下载按钮
                    pboxDown.Visible = false;
                    pboxFile.Visible = true;
                    pboxFile.Location = pboxDown.Location;//换坐标
                }
            }
            catch (Exception ex)
            {
                ConsoleLog.Output(ex.Message);
                HttpUtils.Instance.ShowTip("下载失败：" + ex.Message);
            }
        }
        #endregion
        #region 图标提示功能
        //下载
        private void pboxDown_MouseEnter(object sender, EventArgs e)
        {
            string msg = LanguageXmlUtils.GetValue("download", "下载");
            toolTip1.SetToolTip(pboxDown, msg);

        }

        private void toolTip1_Draw_1(object sender, DrawToolTipEventArgs e)
        {

        }

        private void pboxZhuan_MouseEnter(object sender, EventArgs e)
        {
            string msg = LanguageXmlUtils.GetValue("forward", "转发");
            toolTip1.SetToolTip(pboxZhuan, msg);
        }

        private void pboxFile_MouseEnter(object sender, EventArgs e)
        {
            string msg = LanguageXmlUtils.GetValue("open", "打开");
            toolTip1.SetToolTip(pboxFile, msg);
        }

        private void pboxColle_MouseEnter(object sender, EventArgs e)
        {
            string msg = LanguageXmlUtils.GetValue("collect", "收藏");
            toolTip1.SetToolTip(pboxColle, msg);
        }
        #endregion
        #region 窗体关闭
        /// <summary>
        /// 窗体关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmVideoFlash_FormClosed(object sender, FormClosedEventArgs e)
        {
            // 不 Dispose 窗体（避免释放共享 Icon 句柄导致其他窗体崩溃），置空单例下次重建
            frm = null;
            //停止播放
            try
            {
                if (player != null)
                {
                    player.Stop();
                    player.Release();
                    player = null;
                }
                tmrVideo.Stop();
                isVideo = false;
            }
            catch (Exception ex)
            {
                ConsoleLog.Output(ex.Message);
            }
            Helpers.ClearMemory();
        }
        #endregion

    }
}
