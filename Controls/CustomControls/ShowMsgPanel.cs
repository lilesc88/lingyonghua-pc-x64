using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WinFrmTalk.Model;
using WinFrmTalk.View.list;
using WinFrmTalk.Dictionarys;
using System.IO;
using WinFrmTalk.Helper;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using WinFrmTalk.Helper.MVVM;
using WinFrmTalk.View;
using RichTextBoxLinks;
using System.Collections.Specialized;
using WinFrmTalk.socket;
using System.Diagnostics;
using Newtonsoft.Json;
using WinFrmTalk.Model.dao;

namespace WinFrmTalk.Controls.CustomControls
{

    public enum DialogBox
    {
        Normal = 0,
        OfficialAccount = 1,
        MultiSelect = 2
    }

    public partial class ShowMsgPanel : UserControl, IShowMsgPanel
    {
        #region 非启用状态不改变背景色
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int wndproc);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        public const int GWL_STYLE = -16;
        public const int WS_DISABLED = 0x8000000;

        public static void SetControlEnabled(Control c, bool enabled)
        {
            if (enabled)
            { SetWindowLong(c.Handle, GWL_STYLE, (~WS_DISABLED) & GetWindowLong(c.Handle, GWL_STYLE)); }
            else
            { SetWindowLong(c.Handle, GWL_STYLE, WS_DISABLED | GetWindowLong(c.Handle, GWL_STYLE)); }
        }
        #endregion
        private bool isDragging = false;

        private Point dragStartPoint;
        private Rectangle dragRectangle;

        #region private member
        private delegate void DelegateFriend(Friend friend);
        private List<FileCollect> fileCollects = new List<FileCollect>();  //拖拽的文件
        public MsgTabAdapter msgTabAdapter = new MsgTabAdapter();        //适配器
        Control crl_content = null;         //选中的聊天框内容控件
        Control selectControl = null;       //contextMenuStrip的右键选中对象
        int rowIndex = 0;                   //被点击的行的绑定的集合的索引号
        private double lastMsgTime = 0;     //最后一条消息的时间戳
        private int emoji_num = 0;          //当前文本框有多少个emoji表情
        private List<string> emoji_codes = new List<string>();   //记录文本框中的emojiCode
        private int readNum = 0;            //未读数量计数
        private string readMsgId = "";      //未读信息的第一条id
        private bool isDownloadRoaming = false;     //是否正在拉取漫游，避免选择聊天对象还未拉取到最新消息就已经生成气泡
        private bool isOpenOnlineStatus = false;    //是否显示在线状态
        private DownMsgRecord msgRecord;    //漫游接口的对象
        private int page_index = 1;         //当前的翻页页数
        private bool isFirstLoad = false;   //是否切换聊天对象后，第一次加载聊天记录
        private bool isCanTrigger_NameChanged = false;    //是否切换聊天对象后，第一次更改标题的聊天对象名称
        private Dictionary<string, string> SrcImages = new Dictionary<string, string>();    //保存文本框内需要发送的图片，key-path，value-img_rtf
        //private List<string> fileCollect_imageRtf = new List<string>();
        /// <summary>
        /// 失去焦点时添加的msg集合
        /// </summary>
        public List<MessageObject> DeactivateMsgList = new List<MessageObject>();
        //List<string> _ImageList = new List<string>();//图片集合（用于文件拖拽时添加以及删除时）
        //List<string> _ImageList2 = new List<string>();//删除图片暂存时用
        //private static Microsoft.Office.Interop.PowerPoint.Presentation PpointFile;
        //private static Microsoft.Office.Interop.PowerPoint.ApplicationClass myWordApp = new Microsoft.Office.Interop.PowerPoint.ApplicationClass();
        // 红包
        private string RedCommand;
        private string RedId;
        private string Redopen;
        public List<List<string>> CommandLst = new List<List<string>>();
        Stopwatch stopwatch = new Stopwatch();
        /// <summary>
        /// 最后一条查看的信息，用于新消息未读数量功能
        /// </summary>
        private string LastReadMsgId { get; set; }
        /// <summary>
        /// 未读消息数量，用于新消息未读数量功能
        /// </summary>
        private int UnReadMsgNum { get; set; }
        #endregion

        #region public member
        public int isHaveReadDel = 0;      //本次聊天列表是否含有阅后即焚消息
        public int IsRoomUserRecommend => ChooseTarget.AllowSendCard;     //是否允许群成员私聊
        //选择的联系人
        public Friend ChooseTarget
        {
            get => msgTabAdapter.choose_target;
            set => msgTabAdapter.choose_target = value;
        }
        public int isSeparateChat = 0;      //记录是否为独立聊天窗体
        //对话框的当前状态
        public DialogBox dialogBox { get; set; }
        #endregion

        #region 草稿回显
        private void DraftShow(Friend friend, Friend ChooseTarget)
        {
            // 避免草稿回显触发文本框事件
            txtChangedState = 0;

            if (ChooseTarget != null && !UIUtils.IsNull(ChooseTarget.UserId))
            {
                //需要保存草稿的对象
                LocalDataUtils.SetStringData(Applicate.MyAccount.userId + "_DRAFT_" + ChooseTarget.UserId, txtSend.Rtf);

                //用正则表达式，获取图片rtf
                MatchCollection matchs = Regex.Matches(txtSend.Rtf, @"{\\pict[a-z0-9\\\s]*}", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                foreach (Match match in matchs)
                {
                    txtSend.Rtf = txtSend.Rtf.Replace(match.Value, "[图片]");
                }

                string content = txtSend.Text.Replace("@", "");

                // 如果输入框为空 那么就去刷新最后一条聊天内容
                if (UIUtils.IsNull(content))
                {
                    MessageObject local = MessageObjectDao.Instance.GetLastMessage(ChooseTarget.UserId);
                    if (local != null)
                    {
                        content = ChooseTarget.ToLastContentTip(local.type, local.content, local.fromUserId, local.fromUserName, local.toUserName);
                        if (ChooseTarget.IsGroup == 1 && local.type < kWCMessageType.Remind)
                        {
                            content = local.fromUserName + ":" + content;
                        }

                        if (local.isReadDel == 1)
                        {
                            content = "[阅后即焚消息]";
                        }
                    }

                    ChooseTarget.Content = content;
                    ChooseTarget.LastMsgType = 0;
                    ChooseTarget.UpdateFriendLastContent(content, 0, TimeUtils.CurrentIntTime());
                }
                else
                {
                    // 如果输入框不为空 那么就去刷新草稿
                    ChooseTarget.Content = content;
                    ChooseTarget.LastMsgType = MessageActions.DRAFT_TYPE;
                    ChooseTarget.UpdateFriendLastContent(content, MessageActions.DRAFT_TYPE, TimeUtils.CurrentIntTime());
                }

                Messenger.Default.Send(ChooseTarget, token: MessageActions.UPDATE_FRIEND_DRAFT);
            }

            //新的聊天对象
            if (friend != null)
            {
                //获取新聊天对象的草稿
                string draft = LocalDataUtils.GetStringData(Applicate.MyAccount.userId + "_DRAFT_" + friend.UserId);
                if (string.IsNullOrEmpty(draft))
                    txtSend.Text = draft;
                else
                {

                    //emoji回显
                    using (RichTextBox richTextBox = new RichTextBox())
                    {
                        richTextBox.Rtf = draft;
                        foreach (string emoji_code in EmojiCodeDictionary.GetEmojiDataNotMine().Keys)
                        {
                            string emoji_rtf = EmojiCodeDictionary.GetEmojiRtfByCode(emoji_code);
                            if (richTextBox.Rtf.IndexOf(emoji_rtf) >= 0)
                            {
                                //不存在才添加
                                if (emoji_codes.FindIndex(code => code.Equals("[" + emoji_code + "]")) < 0)
                                {
                                    emoji_num++;
                                    emoji_codes.Add("[" + emoji_code + "]");
                                }
                            }
                        }
                    }
                    //文本框回显
                    txtSend.Rtf = draft;
                }
            }
            //更新@符的数量
            atLastCount = atCurrentCount = txtSend.Text.Split('@').Length - 1;
            txtChangedState = 1;
        }

        public string InputText { get { return txtSend.Text; } }


        #endregion

        #region 初次加载控件

        private void ShowMsgPanel_Load(object sender, EventArgs e)
        {
            RegisterMessengers();       //注册通知
            this.SuspendLayout();
            try
            {
                //添加滚动到顶部的监听
                xListView.HeaderRefresh += () =>
                {
                    if (msgTabAdapter.FirstMsgIndex == 1)
                        LoadMsg();
                };
                isOpenOnlineStatus = Applicate.URLDATA.data.isOpenOnlineStatus == 1;

                //允许拖拽(liuhuan)
                txtSend.AllowDrop = true;
                txtSend.DragDrop += TxtSend_DragDrop;
                txtSend.DragEnter += TxtSend_DragEnter;

                // 设置聊天背景图片 - 已去除纸飞机背景 (2026-08-16)
                //string bgPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource", "chat_bg.png");
                //if (System.IO.File.Exists(bgPath))
                //{
                //    Image chatBgImage = Image.FromFile(bgPath);
                //    xListView.BackgroundImage = chatBgImage;
                //    xListView.BackgroundImageLayout = ImageLayout.Stretch;
                //    xListView.panel1.BackColor = Color.Transparent;
                //}

            }
            catch (Exception ex) { LogHelper.log.Error("----加载控件出错，方法（ShowMsgPanel_Load）: \r\n" + ex.Message); }
            this.ResumeLayout();
        }


        /// <summary>
        /// 是否已经开启了客户模式
        /// </summary>
        public void ChangeTalksBtnUI(string str)
        {
            if (ChooseTarget.IsGroup == 0)
            {
                btnEnding.Visible = Applicate.ServiceMode;
            }
            else
            {
                btnEnding.Visible = false;
            }
        }


        #region 实例化对象
        private void LoadLanguageText()
        {
            if (!Applicate.ENABLE_MULTI_LANGUAGE)
                return;

            btnEnding.Text = LanguageXmlUtils.GetValue("btn_end_chat", btnEnding.Text);
            btnSend.Text = LanguageXmlUtils.GetValue("btn_send", btnSend.Text);
            menuItem_Collect.Text = LanguageXmlUtils.GetValue("collect", menuItem_Collect.Text);
            menuItem_Copy.Text = LanguageXmlUtils.GetValue("copy", menuItem_Copy.Text);
            menuItem_Delete.Text = LanguageXmlUtils.GetValue("delete", menuItem_Delete.Text);
            menuItem_Download.Text = LanguageXmlUtils.GetValue("download", menuItem_Download.Text);
            menuItem_MultiSelect.Text = LanguageXmlUtils.GetValue("multi_select", menuItem_MultiSelect.Text);
            menuItem_paste.Text = LanguageXmlUtils.GetValue("paste", menuItem_paste.Text);
            menuItem_Recall.Text = LanguageXmlUtils.GetValue("recall", menuItem_Recall.Text);
            menuItem_Relay.Text = LanguageXmlUtils.GetValue("forward", menuItem_Relay.Text);
            menuItem_SaveAs.Text = LanguageXmlUtils.GetValue("save_as", menuItem_SaveAs.Text); 
            menuItem_OpenFileFolder.Text = LanguageXmlUtils.GetValue("show_in_folder", menuItem_OpenFileFolder.Text);
            menuItem_SaveCustomize.Text = LanguageXmlUtils.GetValue("save_expression", menuItem_SaveCustomize.Text);
            menuItem_Reply.Text = LanguageXmlUtils.GetValue("reply", menuItem_Reply.Text);
        }

        public ShowMsgPanel()
        {
            InitializeComponent();
            LoadLanguageText();
            msgTabAdapter = new MsgTabAdapter
            {
                xListView = this.xListView
            };
            msgRecord = new DownMsgRecord(this);    //实例化漫游接口
            //绑定列表添加控件的方法
            xListView.panel1.ControlAdded += Panel1_ControlAdded;

            xListView.FooterRefresh += () =>
            {

                if (unReadNumPanel != null)
                {
                    RefreshUnreadNumUi("", 0, false, true);
                }
                UnReadMsgNum = 0;
            };


            #region @通知的点击监听
            AtMePanel.AddEvent((msgId) =>
            {
                //更新@状态
                ChooseTarget.UpdateAtMeState(0);

                if (ChooseTarget.IsGroup != 1)
                    return;

                //获取收到@的message
                MessageObject msg = msgTabAdapter.TargetMsgData.GetMsg(msgId);

                ChooseTarget.IsAtMe = 0;
                //清空At 刷新最后一条消息
                //Messenger.Default.Send(ChooseTarget, MessageActions.UPDATE_FRIEND_LAST_CONTENT);

                LocalDataUtils.SetStringData(ChooseTarget.UserId + "GROUP_AT_MESSAGEID" + Applicate.MyAccount.userId, "");
                //跟踪到该信息位置
                if (msg != null)
                {
                    int index = msgTabAdapter.msgList.FindIndex(m => m.messageId.Equals(msg.messageId));
                    xListView.ShowRangeStart(index, 0, true);
                }
                //如果不存在则直追踪到第一行
                else
                    xListView.ShowRangeStart(0, 0, true);
            });
            #endregion

            #region 新消息标识点击监听
            unReadNumPanel.AddListen((msgId) =>
            {
                //查找本地数据库
                MessageObject msg = new MessageObject() { FromId = ChooseTarget.UserId, messageId = msgId }.GetMessageObject();

                // 向下箭头，追踪到底部 禅道#7584
                if (!unReadNumPanel.direction)
                {
                    LastReadMsgId = string.Empty;
                    UnReadMsgNum = 0;
                    int end = msgTabAdapter.msgList.Count - 1;
                    xListView.ShowRangeEnd(end, 0, true);
                    return;
                }

                //查询的结果为空
                if (string.IsNullOrEmpty(msg.messageId))
                {
                    return;
                }


                //清空最新消息未读数量和字段
                LastReadMsgId = string.Empty;
                UnReadMsgNum = 0;

                #region 追踪行
                Task.Factory.StartNew(() =>
                {
                    //在字典中查找
                    int index = msgTabAdapter.msgList.FindIndex(m => m.messageId.Equals(msgId));

                    //跟踪到该信息位置
                    if (index > -1)
                    {
                        if (IsHandleCreated)
                            Invoke(new Action(() => { xListView.ShowRangeStart(index, 0, true); }));

                    }
                    //不在列表中，则必定在数据库中
                    else
                    {
                        int time = 0;
                        while (msgTabAdapter.msgList.Count < 200 && time < 10)
                        {
                            index = msgTabAdapter.msgList.FindIndex(m => m.messageId == msgId);
                            if (index > -1)
                            {
                                if (IsHandleCreated)
                                    Invoke(new Action(() => { xListView.ShowRangeStart(index, 0, true); }));
                                return;
                            }
                            //加载更多聊天记录
                            //msgTabAdapter.AddMoreMsg();
                            if (IsHandleCreated)
                                Invoke(new Action(() => { LoadMsg(); }));

                            time++;
                        }
                        if (IsHandleCreated)
                            Invoke(new Action(() => { xListView.ShowRangeStart(1, 0, true); }));
                    }
                });
                #endregion
            });
            #endregion

            #region 录音回调
            userSoundRecording.PathCallback = (localPath, timespan) =>
            {
                //修改焦点
                txtSend.Focus();

                //不能发言
                if (CollectUtils.EnableForward(ChooseTarget))
                    return;

                int timeLen = timespan;
                int fileSize = Convert.ToInt32(new FileInfo(localPath).Length);
                //先生成气泡
                MessageObject msg = ShiKuManager.SendVoiceMessage(ChooseTarget, "", localPath, fileSize, timeLen, false);
                JudgeMsgIsAddToPanel(msg);
                //上传音频文件
                UploadEngine.Instance.From(localPath).
                    UploadFile((success, url) =>
                    {
                        if (success)
                        {
                            msg.content = url;
                            msg.UpdateMessageContent();
                            ShiKuManager.SendMessage(msg);

                            //获取控件并转化文件
                            EQBaseControl talk_panel = GetTalkPanelByMsg(msg.messageId);
                            var result = talk_panel.Controls.Find("crl_content", true);
                            if (result.Length > 0 && result[0] is Panel panel_voice && talk_panel is EQVoiceControl voice_crl)
                            {
                                voice_crl.DownloadAndChangeVoice(panel_voice);
                            }
                        }
                    });
            };
            #endregion

            BindTool();
        }
        #endregion

        #region 拖入控件边界时发生（liuhuan/2019/4/22）
        /// <summary>
        /// 拖入控件边界时发生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtSend_DragEnter(object sender, DragEventArgs e)
        {
            //if (e.Data.GetDataPresent(DataFormats.FileDrop))
            //{
            //    //e.Effect = DragDropEffects.Copy;
            //    e.Effect = DragDropEffects.Move;
            //}
            //else
            //{
            //    e.Effect = DragDropEffects.None;
            //}
            //如果拖拽的文件是pptx的空文件，会出现生成的图片为空（为解决）
            e.Effect = DragDropEffects.Move;
            String[] supportedFormats = e.Data.GetFormats(true);
            if (supportedFormats != null)
            {
                List<string> sfList = new List<string>(supportedFormats);
                if (sfList.Contains(DataFormats.FileDrop.ToString()))
                {
                    txtSend.EnableAutoDragDrop = false;
                }
                else
                {
                    txtSend.EnableAutoDragDrop = true;

                }
            }
        }
        #endregion

        #region  在完成拖放操作时完成（liuhuan/2019/4/22）
        /// <summary>
        /// 在完成拖放操作时完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtSend_DragDrop(object sender, DragEventArgs e)
        {
            //string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            //if (files.Length > 0)
            //    fileCollect.Add(files[0]);
            //foreach (string file in files)
            //{
            //    FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            //    StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default);
            //    sr.Close();
            //}
            String[] supportedFormats = e.Data.GetFormats(true);
            if (supportedFormats != null)
            {
                List<string> sfList = new List<string>(supportedFormats);
                if (sfList.Contains(DataFormats.FileDrop.ToString()))
                {
                    string[] fileList = (string[])((System.Array)e.Data.GetData(DataFormats.FileDrop));
                    foreach (string fileName in fileList)
                    {
                        FileInfo fileInfo = new FileInfo(fileName);

                        if (Directory.Exists(fileName))//如果是文件夹，不允许生成图片
                        {
                            HttpUtils.Instance.ShowTip("暂不支持文件夹发送");
                            continue;
                        }
                        if (fileInfo.Length == 0 || fileInfo == null)//文件为空的情况
                        {
                            HttpUtils.Instance.ShowTip("文件为空,不允许发送请重新选择");
                            continue;
                        }
                        if (fileName.Contains(".pptx") || fileName.Contains(".ppt"))
                        {

                            //PpointFile = myWordApp.Presentations.Open(fileName, MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoFalse);
                            //int pages = PpointFile.Slides.Count;
                            //PpointFile.Close();
                            //if (pages == 0)
                            //{
                            //    HttpUtils.Instance.ShowTip("不允许发送空白ppt");
                            //    continue;
                            //}
                        }
                        if (!fileCollects.Contains(fileName))
                        {
                            if (EQControlManager.IsImage(fileName))
                            {
                                //往文本框直接粘贴一次图片便可，否则会发送两次
                                Image image = Image.FromFile(fileName);
                                int new_width = image.Width, new_height = image.Height;
                                EQControlManager.ModifyWidthAndHeight(ref new_width, ref new_height, 150, 150);
                                Image new_image = new Bitmap(image, new_width, new_height);

                                Clipboard.Clear();
                                Clipboard.SetDataObject(new_image, true, 3, 200);//将图片放在剪贴板中
                                if (txtSend.CanPaste(DataFormats.GetFormat(DataFormats.Bitmap)))    //粘贴数据
                                {
                                    txtSend.Paste();
                                    string new_rtf = EQControlManager.ImageToRtf(new_image);
                                    if (!string.IsNullOrEmpty(new_rtf))
                                    {
                                        if (!SrcImages.ContainsKey(fileName))
                                            SrcImages.Add(fileName, new_rtf);
                                    }
                                }
                            }
                            else
                            {
                                FileCollectUtils.AddFileCollectToBox(this.txtSend, fileName, fileCollects);
                            }

                        }


                    }
                }
            }
            txtSend.EnableAutoDragDrop = true;
        }

        #endregion

        #region liuhuan2019/08/03获取图片的字符串
        /// <summary>
        /// 获取richtextbox所有的图片的纯rtf代码(不包括高度宽度像素等信息)
        /// </summary>
        /// <param name="_ImageList">返回图片rtf字符串列表</param>
        private void ReadImg(ref List<string> _ImageList)
        {
            _ImageList.Clear();
            string _RtfText = txtSend.Rtf;
            while (true)
            {
                int _Index = _RtfText.IndexOf("objdata");
                if (_Index == -1) break;
                _RtfText = _RtfText.Remove(0, _Index + 8);
                _Index = _RtfText.IndexOf("\r\n");
                _RtfText = _RtfText.Remove(0, _Index);
                _Index = _RtfText.IndexOf("}");
                _ImageList.Add(_RtfText.Substring(0, _Index).Replace("\r\n", ""));
                _RtfText = _RtfText.Remove(0, _Index);
            }
        }
        #endregion
        #region  发送拖拽后的文件（liuhuan/2019/4/22）
        /// <summary>
        /// 拖拽后点击发送文件
        /// </summary>
        private void DroupFeileSend()
        {
            //if (_ImageList.Count == 0)
            //{
            //    fileCollect.Clear();
            //}
            try
            {
                foreach (FileCollect item in fileCollects)
                {
                    //判断是否还存在该项（可能已经被删除）
                    if (txtSend.Rtf.IndexOf(item.ImgRtf) < 0)
                        continue;

                    string local_path = item.FilePath;
                    if (File.Exists(local_path))
                    {
                        bool isVideo = FileUtils.JudgeIsVideoFile(local_path);
                        //如果为视频文件
                        if (isVideo)
                        {
                            //先生成气泡
                            int fileSize = Convert.ToInt32(new FileInfo(local_path).Length);
                            MessageObject msg = ShiKuManager.SendVideoMessage(ChooseTarget, "", local_path, fileSize, false);
                            msg.isLoading = 1;
                            JudgeMsgIsAddToPanel(msg);

                            //获取气泡
                            EQBaseControl talk_panel = GetTalkPanelByMsg(msg.messageId);
                            if (talk_panel != null)
                            {
                                UploadEngine.Instance.From(local_path).
                                //上传完成
                                UploadFile((success, url) =>
                                {
                                    UploadVideo(talk_panel, msg, url, success);
                                });
                            }
                        }
                        else
                            UploadFileOrImage(local_path, Convert.ToInt32(new FileInfo(local_path).Length));
                    }
                    else
                        HttpUtils.Instance.ShowTip("路径不存在：" + local_path);
                }
            }
            catch (Exception ex) { LogHelper.log.Error("----------发送拖拽后的文件出错：方法（DroupFeileSend）\n" + ex.Message); }
            //txtSend.Clear();
        }
        #endregion

        #region 注册通知
        /// <summary>
        /// 注册通知  (注释可将鼠标移至Notifaction属性上查看)    
        /// </summary>
        private void RegisterMessengers()
        {
            //注册添加消息气泡到消息列表
            Messenger.Default.Register<MessageObject>(this, MessageActions.XMPP_REFRESH_MESSAGE, item => RefreshMessageItem(item.messageId));
            //注册往文本框添加emoji表情
            //Messenger.Default.Register<string>(this, EQFrmInteraction.AddEmojiToTxtSend, item => AddEmojiToTxtSend(item));
            //注册发送消息时添加消息气泡
            //Messenger.Default.Register<MessageObject>(this, EQFrmInteraction.SendMsgAddBubble, item => AddMessageToPanel(item));
            //多选操作结束
            Messenger.Default.Register<Friend>(this, EQFrmInteraction.MultiSelectEnd, item => 
            {
                if (!string.IsNullOrEmpty(ChooseTarget.UserId))
                {
                    if (ChooseTarget.UserId.Equals(item.UserId))
                        LblClose_Click(null, null);
                }
            });
            #region 清空UI
            //注册清空UI（单向）
            Messenger.Default.Register<string>(this, EQFrmInteraction.ClearFdMsgsSingle, (userId) =>
            {
                //请求接口删除聊天记录
                MessageObject message = new MessageObject() { FromId = userId };
                message.DeleteTable();

                Friend fd = new Friend() { UserId = userId }.GetByUserId();

                // 清除服务器数据
                if (fd.IsGroup == 0)
                {
                    ClearServerFriendMsg(userId);
                }

                // 清空页面数据
                if (ChooseTarget != null && string.Equals(userId, ChooseTarget.UserId))
                {
                    ClearUI(ChooseTarget);
                }

                // 刷新最近消息记录页

                fd.UpdateClearContent();
                Messenger.Default.Send(fd, token: MessageActions.UPDATE_CHATLIST_CLEAR_FRIEND);
            });

            //注册清空UI（双向）
            Messenger.Default.Register<string>(this, MessageActions.CLEAR_FRIEND_MSGS, (userId) =>
            {
                // 清空页面数据
                if (ChooseTarget != null && string.Equals(userId, ChooseTarget.UserId))
                {
                    ClearUI(ChooseTarget);
                }

                // 刷新最近消息记录页
                Friend fd = new Friend() { UserId = userId }.GetByUserId();
                fd.UpdateClearContent();
                Messenger.Default.Send(fd, token: MessageActions.UPDATE_CHATLIST_CLEAR_FRIEND);
            });
            #endregion
            //注册移除某一行
            Messenger.Default.Register<string>(this, EQFrmInteraction.RemoveMsgOfPanel, item => RemoveMsgOfPanel(item));
            //按回执更新送达消息状态（成功）
            Messenger.Default.Register<MessageObject>(this, MessageActions.XMPP_UPDATE_SEND_SUCCESS, item => DrawIsSend(item));
            //按回执更新送达消息状态（失败）
            Messenger.Default.Register<MessageObject>(this, MessageActions.XMPP_UPDATE_SEND_FAILED, item => DrawIsSend(item));
            //按回执更新已读消息状态
            Messenger.Default.Register<MessageObject>(this, MessageActions.XMPP_UPDATE_RECEIVED_READ, item => DrawIsRead(item));
            //给消息列表添加消息
            Messenger.Default.Register<MessageObject>(this, MessageActions.XMPP_UPDATE_NORMAL_MESSAGE, item => JudgeMsgIsAddToPanel(item));
            //收到了一个群组控制消息
            Messenger.Default.Register<MessageObject>(this, MessageActions.XMPP_UPDATE_ROOM_CHANGE_MESSAGE, item => UpdateRoomUIByMessage(item));
            //收到了一个撤回通知
            Messenger.Default.Register<MessageObject>(this, MessageActions.XMPP_UPDATE_RECALL_MESSAGE, item => RecallMsg(item));
            //收到了一个群禁言通知
            Messenger.Default.Register<MessageObject>(this, MessageActions.ROOM_UPDATE_BANNED_TALK, item => UpdateBannedTalk(item));
            //收到了一个修改备注通知
            Messenger.Default.Register<Friend>(this, MessageActions.UPDATE_FRIEND_REMARKS, item => ModifyFriendName(item));
            //收到了一个批量删除通知
            Messenger.Default.Register<List<MessageObject>>(this, EQFrmInteraction.BatchDeleteMsg, item => BatchDeleteMsg(item));
            //收到了一个@我的通知
            Messenger.Default.Register<Friend>(this, MessageActions.ROOM_UPDATE_AT_ME, item => AtMeShowPanel(item));
            //收到我主动@别人的通知
            Messenger.Default.Register<MessageObject>(this, EQFrmInteraction.AddAtUserToTxtSend, item => AddAtUserToTxtSend(item));
            //重新上传图片并更新气泡
            Messenger.Default.Register<MessageObject>(this, EQFrmInteraction.ResumeUploadImageMsg, item => ResumeUploadImageMsg(item));
            //重新上传视频并更新气泡
            Messenger.Default.Register<MessageObject>(this, EQFrmInteraction.ResumeUploadVideoMsg, ResumeUploadVideoMsg);
            //多点登录上线离线消息
            Messenger.Default.Register<string>(this, MessageActions.UPDATE_DEVICE_STATE, item => UpdateDeviceState(item));
            //独立聊天窗口收到删除好友的通知需要关闭窗体
            Messenger.Default.Register<Friend>(this, MessageActions.DELETE_FRIEND, DeleteFdCloseFrm);
            //拉黑的通知
            Messenger.Default.Register<Friend>(this, MessageActions.ADD_BLACKLIST, DeleteFdCloseFrm);

            Messenger.Default.Register<Friend>(this, MessageActions.UPDATE_FRIEND_READDEL, (lacal) =>
            {
                if (ChooseTarget != null && lacal.UserId.Equals(ChooseTarget.UserId))
                {
                    ChooseTarget.IsOpenReadDel = lacal.IsOpenReadDel;
                }
            });
            Messenger.Default.Register<string>(this, MessageActions.XMPP_UPDATE_CUSTOMERSERVICE, ChangeTalksBtnUI);

            Messenger.Default.Register<List<List<string>>>(this, MessageActions.RED_UPDATE_COMMAND, SetRedCommand);
            Messenger.Default.Register<string>(this, MessageActions.RED_OPEN_COMMAND, OPENRedCommand);
            // 监听群组密钥丢失状态
            Messenger.Default.Register<Friend>(this, MessageActions.UPDATE_CHATKEY_STATE, (friend) =>
            {
                if (ChooseTarget != null && string.Equals(friend.UserId, ChooseTarget.UserId))
                {
                    if (ChooseTarget.IsLostKeyGroup != friend.IsLostKeyGroup)
                    {
                        ChooseTarget.IsLostKeyGroup = friend.IsLostKeyGroup;

                        // 刷新文本框状态
                        RefreshSendBoxState(ChooseTarget);

                        if (friend.IsLostKeyGroup == 0)
                        {
                            requestKeyPanel.IsShowView("", false);
                        }

                    }
                }
            });

            Messenger.Default.Register<Friend>(this, MessageActions.UPDATE_SERVICE_LOCK, (friend) =>
            {
                if (ChooseTarget != null && string.Equals(friend.UserId, ChooseTarget.UserId))
                {
                    ChooseTarget.IsDismiss = friend.IsDismiss;
                    // 刷新文本框状态
                    RefreshSendBoxState(ChooseTarget);
                }
            });
        }
        private void OPENRedCommand(string id)
        {
            RedpaperUIUtils.OpenRedPacket(id);
        }
        #region 口令红包        
        private void SetRedCommand(List<List<string>> keyValues)
        {
            if (!CommandLst.Contains(keyValues[0]))
            {
                CommandLst.Add(keyValues[0]);
            }

            //CommandLst = keyValues;
            int count = keyValues.Count - 1;
            txtSend.Text = keyValues[count][0];
            RedCommand = keyValues[count][0];
            RedId = keyValues[count][1];
            Redopen = keyValues[count][2];
        }
        #endregion

        #endregion
        #endregion

        #region 切换聊天对象
        #region 更换聊天对象，清除缓存
        /// <summary>
        /// 更换聊天对象，清除缓存
        /// </summary>
        /// <param name="friend">切换签的聊天对象</param>
        /// <param name="isDispon"></param>
        private void ClearUI(Friend friend, bool isDispon = true)
        {
            //关闭设置页
            //if (frmSet != null)
            //{
            //    frmSet.Close();
            //    frmSet = null;
            //}
            //关闭之前对象的接口调用
            if (ChooseTarget != null && !string.IsNullOrEmpty(ChooseTarget.UserId))
            {
                HttpUtils.Instance.Cancel(ChooseTarget.UserId);
                HttpUtils.Instance.Cancel(ChooseTarget.UserId + "roomget");
            }


            //清除已选择的emoji
            emoji_num = 0;
            emoji_codes = new List<string>();

            //阅后即焚处理
            SaveReadDelTime();
            isHaveReadDel = 0;

            serviceListCrl.isShowMultiSelect = false;   //设置不显示多选面板，用于客服快捷栏布局

            requestKeyPanel.IsShowView("", false);

            //非独立窗体
            if (isSeparateChat == 0)
            {
                //获取上个聊天对象
                if (ChooseTarget != null && !string.IsNullOrEmpty(ChooseTarget.UserId))
                {
                    var last_target = ChatTargetDictionary.GetMsgData(ChooseTarget.UserId);
                    //如果上个聊天对象不为独立窗体
                    if (last_target.isSeparateChat == 0)
                    {
                        //清除上个聊天对象的消息
                        if (ChooseTarget != null && !string.IsNullOrEmpty(ChooseTarget.UserId))
                        {
                            if (last_target.GetMsgList().Count > 0)
                                last_target.RemoveAllData();
                        }
                        //移除切换好友前的对象
                        ChatTargetDictionary.RemoveItem(ChooseTarget.UserId);
                    }
                }
            }
            else
            {
                //上一个对象必定为空
                //if(string.IsNullOrEmpty(choose_target.userId))
                //{
                //获取新独立窗体的聊天对象
                //targetMsgData = ChatTargetDictionary.GetMsgData(friend.userId);
                var current_target = ChatTargetDictionary.GetMsgData(friend.UserId);
                //清除数据
                if (current_target != null && current_target.GetMsgList().Count > 0)
                    current_target.RemoveAllData();
                //}
            }

            if (msgTabAdapter != null)
            {
                msgTabAdapter.ClearRoomRole();
            }

            Friend lastChat = ChooseTarget;
            //更新聊天对象
            ChooseTarget = friend;
            //刷新对方头像（防止其他端修改头像后 PC 显示旧头像）
            if (friend != null && friend.IsGroup == 0)
            {
                ImageLoader.Instance.RefreshAvatar(friend.UserId);
            }
            //草稿回显
            DraftShow(friend, lastChat);
            //清空聊天列表并绑定新的数据
            msgTabAdapter.direction = false;        //倒序排列
            msgTabAdapter.choose_target = ChooseTarget;
            //保存是否为独立聊天
            msgTabAdapter.TargetMsgData.isSeparateChat = this.isSeparateChat;
            //List<MessageObject> msgList = msgTabAdapter.LoadObjectLocalMsg();   //获取本地数据
            BubbleBgDictionary.RemoveAllBg();       //移除所有缓存的背景图
            //msgTabAdapter.BindData(msgList);        //绑定数据源
            xListView.SetAdapter(msgTabAdapter);    //绑定适配器

            //Helpers.ClearMemory();
            ////草稿回显时会重置，不需要再重置
            //atCurrentCount = 0;    //重置@符数量
            //atLastCount = 0;

            //重置向上翻页
            //canAddMsg = 1;
            //清空录制
            ClearTranscribe();

            //关闭等待符
            loding.stop();

            //重置漫游状态
            isDownloadRoaming = false;

            //取消录音
            if (userSoundRecording.SoundState)
                userSoundRecording.StopSound();

            lastMsgTime = 0;    //重置最后一条消息的时间

            //隐藏并清空回复面板
            replyPanel.ReplyMsg = null;
            replyPanel.Visible = false;;

            page_index = 1;      //漫游页数重置
                                 //1.将复制好的图片放入发送框中，切换一个聊天对象并复制发送图片，再切换回去原先的图片无法发
                                 // SrcImages = new Dictionary<string, string>();
            titleManager.Clear();

        }
        #endregion

        #region 清空聊天缓存，保存新选择的聊天对象
        /// <summary>
        /// 清空聊天缓存，保存新选择的聊天对象
        /// <para>如果聊天对象不变，请不要调用</para>
        /// </summary>
        /// <param name="friend">聊天对象</param>
        public void SetChooseFriend(Friend friend, int readNum = 0, string msgId = "", int isSeparateChat = 0)
        {
            //避免因为错误退出，界面挂起没恢复
            this.ResumeLayout();
            xListView.panel1.ResumeLayout();

            this.SuspendLayout();
            try
            {
                //记录未读的相关信息
                this.readNum = readNum;
                this.readMsgId = msgId;
                friend.MsgNum = 1;
                userSize = -1;
                //记录是否为独立聊天对象
                this.isSeparateChat = isSeparateChat;

                //设置空的聊天对象，重置面板
                if (friend == null || string.IsNullOrEmpty(friend.UserId))
                {
                    ClearUI(friend);
                    ChooseTarget = null;
                    return;
                }
                //点击相同的对象
                //if (choose_target != null && choose_target.userId == friend.userId)
                //    return;

                //this.SuspendLayout();
                //panShade.BringToFront();    //遮罩层使界面加载过程不可见    

                ClearUI(friend);
                //保存聊天对象
                //choose_target = friend;

                isCanTrigger_NameChanged = true;
                //修改标题
                labName.Text = EQControlManager.StrAddEllipsis(friend.GetRemarkName(), labName.Font, 240);

                // 刷新会话按钮UI
                ChangeTalksBtnUI(string.Empty);
                //修改禅道9869
                IsShowReadPersons(friend.ShowRead == 0 ? false : true);

                //获取群设置
                if (ChooseTarget.IsGroup == 1)
                {
                    GetRoomSetting();

                    if (msgTabAdapter != null)
                    {
                        msgTabAdapter.MyCurrtRole = GetRoomMemberRole(Applicate.MyAccount.userId);
                    }

                    // 增加容错，自动加群防止收不到群消息
                    if (!UIUtils.IsNull(ChooseTarget.UserId))
                    {
                        ShiKuManager.mSocketCore.JoinRoom(ChooseTarget.UserId, 0);
                    }
                }
                else
                {
                    //获取好友状态到标题栏
                    SetOnlineState();
                    //设置音视频的可见度
                    if (Applicate.ENABLE_MEET && Applicate.CURRET_VERSION > 4.0f)
                    {
                        // 我的设备不允许音视频
                        if (ChooseTarget.UserType != FriendType.DEVICE_TYPE)
                        {
                            showMsgPanel_Tool.lblAudio.Visible = true;
                            showMsgPanel_Tool.lblVideo.Visible = true;
                        }
                        else
                        {
                            showMsgPanel_Tool.lblAudio.Visible = false;
                            showMsgPanel_Tool.lblVideo.Visible = false;
                        }
                    }
                }

                //复选框列不可见
                //if (msgTabAdapter.TargetMsgData.isMultiSelect)
                if (this.dialogBox == DialogBox.MultiSelect)
                {
                    LblClose_Click(null, null);
                }

                ////显示聊天发送框而不是多选操作面板
                //if (!(friend.UserType == 2 && ChooseTarget.UserType == 2))
                //    IsShowPanelMultiSelect(false);  

                //控制是否显示公众号快捷栏
                //serviceListCrl.SetList(ChooseTarget, this);
                if (Applicate.URLDATA.data.enableMpModule == 1 && ChooseTarget.UserType == 2)
                {
                    SetDialogBoxSize(DialogBox.OfficialAccount);
                    showMsgPanel_Tool.lblSLC.Visible = true;
                }
                else
                {
                    SetDialogBoxSize(DialogBox.Normal);
                    showMsgPanel_Tool.lblSLC.Visible = false;
                }

                //先更新界面再
                Application.DoEvents();

                //先进行漫游
                //FirstDownloadRoaming();
                if (msgTabAdapter.msgList.Count > 0)
                {
                    msgTabAdapter.TargetMsgData.RemoveAllData();
                    xListView.ClearList();
                }

                page_index = 1;
                isFirstLoad = true;
                msgRecord.LoadMsgDatas(friend, page_index, null);

                // 刷新发送消息面板
                RefreshSendBoxState(friend);

                //控制是否显示未读数量悬浮标识
                IsShowUnReadNumPanel(readNum, readMsgId);

                //是否显示音视频按钮
                if (!Applicate.ENABLE_MEET || Applicate.CURRET_VERSION < 4.6f)
                {
                    showMsgPanel_Tool.lblAudio.Visible = false;
                    showMsgPanel_Tool.lblVideo.Visible = false;
                }
                //控制是否要显示@角标
                AtMeShowPanel(friend);

                //如果有空白滚动到底部
                //RollBottom_HaveEmpty();

                txtSend.Focus();    //文本输入框自动获取焦点
                if (ChooseTarget.Content.Contains("您好我是客户客户" + ChooseTarget.UserId + ",很高兴为您服务，请问有什么问题可以帮您的?"))
                {
                    lab_detial.Visible = false;
                }
                else
                {
                    lab_detial.Visible = true; ;
                }
            }
            catch (Exception ex)
            {
                LogHelper.log.Error("-------------保存聊天对象出错，方法（SetChooseFriend）: \r\n" + ex.Message);
            }
            this.ResumeLayout();
        }
        #endregion

        #endregion

        #region 处理群通知UI更新
        private void UpdateRoomUIByMessage(MessageObject msg)
        {
            try
            {
                if (ChooseTarget == null)
                    return;
                if (ChooseTarget.IsGroup == 0 || !ChooseTarget.UserId.Equals(msg.objectId))
                    return;

                switch (msg.type)
                {
                    //是否允许私聊
                    case kWCMessageType.RoomUserRecommend:
                        if (msg.content == "1")  //允许群成员私聊
                        {
                            ChooseTarget.AllowSendCard = 1;
                        }
                        else    //不允许群成员私聊
                        {
                            ChooseTarget.AllowSendCard = 0;
                        }
                        break;
                    //群管理员转让
                    case kWCMessageType.RoomManagerTransfer:

                        // 刷新发送消息面板
                        RefreshSendBoxState(ChooseTarget);
                        // 更新群成员头像生成的气泡
                        // UpdateRoomMemberImage(msg);
                        break;
                    //群公告
                    case kWCMessageType.RoomNotice:
                    case kWCMessageType.RoomNoticeEdit:
                        // 缺少公告id,
                        titleManager.ShowRoomNotice("1", msg.content);
                        break; ;
                    //解散群聊
                    case kWCMessageType.RoomDismiss:
                        //string remindTxt = "该群已被解散";
                        //IsShow_BannedTalkPanel(true, remindTxt);
                        break;
                    //修改房间名称
                    case kWCMessageType.RoomNameChange:
                        ChooseTarget.NickName = msg.content;
                        if (this.IsHandleCreated)
                            Invoke(new Action(() =>
                            {
                                //msg.content + "（" + userSize + "人）";
                                //labName.Text = UIUtils.LimitTextLength(msg.content, 18, true) + "（" + userSize + "人）";

                                if (userSize > -1)
                                {
                                    var unit = LanguageXmlUtils.GetValue("people", "人");
                                    var name = EQControlManager.StrAddEllipsis(msg.content, labName.Font, 240);
                                    labName.Text = string.Format("{0}({1}{2})", name, userSize, unit);
                                }
                                else
                                {
                                    labName.Text = msg.content;
                                }
                            }));
                        break;

                    case kWCMessageType.RoomReadVisiblity:
                    //显示阅读人数
                        ChooseTarget.ShowRead = Convert.ToInt32(msg.content);
                        IsShowReadPersons(ChooseTarget.ShowRead == 0 ? false : true);
                        break;
                    case kWCMessageType.RoomInsideVisiblity:
                        //显示群内成员
                        ChooseTarget.ShowMember = Convert.ToInt32(msg.content);
                        break;
                    //群讲课
                    case kWCMessageType.RoomAllowSpeakCourse:
                        if (this.IsHandleCreated)
                            Invoke(new Action(() =>
                            {
                                if (Applicate.ENABLE_MEET && Applicate.CURRET_VERSION > 4.0f)
                                {
                                    ChooseTarget.AllowConference = Convert.ToInt32(msg.content);
                                    //关闭后，改变普通成员的音视频按钮状态
                                    SetControlEnabled(showMsgPanel_Tool.lblAudio, ChooseTarget.AllowSpeakCourse == 0 && !JudgeIsAdmin(Applicate.MyAccount.userId) ? false : true);
                                    SetControlEnabled(showMsgPanel_Tool.lblVideo, ChooseTarget.AllowSpeakCourse == 0 && !JudgeIsAdmin(Applicate.MyAccount.userId) ? false : true);
                                }
                                else
                                {
                                    SetControlEnabled(showMsgPanel_Tool.lblAudio, false);
                                    SetControlEnabled(showMsgPanel_Tool.lblVideo, false);
                                }
                            }));
                        break;
                    //群会议开关
                    case kWCMessageType.RoomAllowConference:
                        if (this.IsHandleCreated)
                            Invoke(new Action(() =>
                            {
                                if (Applicate.ENABLE_MEET && Applicate.CURRET_VERSION > 4.0f)
                                {
                                    ChooseTarget.AllowConference = Convert.ToInt32(msg.content);
                                    //关闭群会议，普通成员不允许点击视频和音频
                                    //SetControlEnabled(lblAudio, choose_target.allowConference == 0 && !JudgeIsAdmin(Applicate.MyAccount.userId) ? false : true);
                                    //SetControlEnabled(lblVideo, choose_target.allowConference == 0 && !JudgeIsAdmin(Applicate.MyAccount.userId) ? false : true);
                                    showMsgPanel_Tool.lblAudio.Visible = ChooseTarget.AllowConference == 0 && !JudgeIsAdmin(Applicate.MyAccount.userId) ? false : true;
                                    showMsgPanel_Tool.lblVideo.Visible = ChooseTarget.AllowConference == 0 && !JudgeIsAdmin(Applicate.MyAccount.userId) ? false : true;
                                }
                                else
                                {
                                    showMsgPanel_Tool.lblAudio.Visible = false;
                                    showMsgPanel_Tool.lblVideo.Visible = false;
                                }
                            }));
                        break;
                    //入群通知
                    case kWCMessageType.RoomInvite:
                        if (msg.readPersons == -1)  //离线消息
                            return;

                        if (this.IsHandleCreated)
                            Invoke(new Action(() =>
                            {

                                Console.WriteLine("userSize = " + userSize);
                                if (userSize > -1)
                                {
                                    userSize++;
                                    var unit = LanguageXmlUtils.GetValue("people", "人");
                                    var name = EQControlManager.StrAddEllipsis(msg.content, labName.Font, 240);
                                    labName.Text = string.Format("{0}({1}{2})", name, userSize, unit);
                                }
                            }));
                        break;
                    //退群通知
                    case kWCMessageType.RoomExit:
                        if (msg.readPersons == -1)  //离线消息
                            return;

                        if (this.IsHandleCreated)
                            Invoke(new Action(() =>
                            {
                                //如果是自己的退群通知
                                if (msg.toUserId == Applicate.MyAccount.userId)
                                {
                                    // 这条消息应该是看不到的
                                    //IsShow_BannedTalkPanel(true, "您已经不在该群中");
                                }
                                else
                                {
                                    //修改群标题人数
                                    if (userSize > -1)
                                    {
                                        userSize--;
                                        var unit = LanguageXmlUtils.GetValue("people", "人");
                                        var name = EQControlManager.StrAddEllipsis(msg.content, labName.Font, 240);
                                        labName.Text = string.Format("{0}({1}{2})", name, userSize, unit);
                                    }
                                    Console.WriteLine("userSize = " + userSize);
                                }
                            }));
                        break;
                    case kWCMessageType.RoomUnseenRole:
                        //隐身人是否为自己
                        if (msg.ToId.Equals(Applicate.MyAccount.userId))
                        {
                            // 刷新发送消息面板
                            RefreshSendBoxState(ChooseTarget);
                            msgTabAdapter.MyCurrtRole = 4;
                        }
                        break;
                    case kWCMessageType.RoomMemberNameChange:
                        //群成员名称修改
                        msgTabAdapter.rm_utils.UpdateRoomMemberName_Member(msg.content, msg.fromUserId, ChooseTarget.RoomId);
                        UpdateRoomMemberName(msg.fromUserId);   //更新已经生成的气泡
                        break;
                    case kWCMessageType.RoomAdmin:
                        // 更新群成员头像生成的气泡
                        UpdateRoomMemberImage(msg);
                        break;
                }
            }
            catch (Exception ex)
            {
                LogHelper.log.Error("----处理群设置消息出错，方法（UpdateRoomUIByMessage）: \r\n" + ex.Message);
            }
        }
        #endregion

        #region 列表容器添加和移除控件
        private void Panel1_ControlAdded(object sender, ControlEventArgs e)
        {
            // 设置消息项背景透明，让聊天背景图透出来
            try { e.Control.BackColor = Color.Transparent; } catch { }
            
            #region 添加右键菜单
            //EQShowInfoPanelAlpha panelAlpha = null;
            for (int index = e.Control.Controls.Count - 1; index > -1; index--)
            {
                Control pan_crl = e.Control.Controls[index];
                if (pan_crl is EQBaseControl talk_panel)
                {
                    foreach (Control item in talk_panel.Controls)
                    {
                        switch (item.Name)
                        {
                            case "lab_msg": break;
                            case "lab_hand": break;
                            //内容控件在气泡内
                            case "image_panel":
                                //item.MouseDown += Content_MouseDown;
                                foreach (Control crl in item.Controls)
                                {
                                    if (string.Equals(crl.Name, "crl_content"))
                                    {
                                        crl.MouseDown += Content_MouseDown;
                                        AddContentMouseDownToCrl(crl);  //给所有子控件设置右键菜单事件
                                        crl.ContextMenuStrip = cmsMsgMenu;
                                    }
                                }
                                break;
                            case "crl_content":
                                item.MouseDown += Content_MouseDown;
                                AddContentMouseDownToCrl(item);  //给所有子控件设置右键菜单事件
                                item.ContextMenuStrip = cmsMsgMenu;
                                break;
                            default:
                                break;
                        }
                    }
                }

                //开启多选后遮罩层在上
                //else if (pan_crl is EQShowInfoPanelAlpha e_panelAlpha)
                //    panelAlpha = e_panelAlpha;
            }
            //if (panelAlpha != null)
            //    //if (msgTabAdapter.TargetMsgData.isMultiSelect)
            //    if (this.dialogBox == DialogBox.MultiSelect)
            //        panelAlpha.BringToFront();
            #endregion
        }

        #region 移除并销毁某一条消息
        private void RemoveMsgOfPanel(string msgId)
        {
            MessageObject msg = msgTabAdapter.TargetMsgData.GetMsg(msgId);
            if (msg == null)
                return;

            int index = msgTabAdapter.msgList.FindIndex(m => m.messageId == msg.messageId);
            Action action = new Action(() =>
            {
                Messenger.Default.Send(msg, MessageActions.CLEAR_CURRENT_CONTENT);

                //从容器中移除控件
                xListView.RemoveItem(index);
                
                //从字典中移除
                msgTabAdapter.TargetMsgData.RemoveMsgData(msgId);

                //删除数据库数据
                msg.DeleteData();

                if (xListView.Progress == 100)
                {
                    int end = msgTabAdapter.msgList.Count - 1;
                    xListView.ShowRangeEnd(end, 0, true);
                }

            });
            if (this.IsHandleCreated)
                Invoke(action);
        }
        #endregion

        #region 刷新一整个消息气泡
        private void RefreshMessageItem(string messageid)
        {
            MessageObject msg = msgTabAdapter.TargetMsgData.GetMsg(messageid);
            if (msg == null)
                return;

            int index = msgTabAdapter.msgList.FindIndex(m => m.messageId == msg.messageId);
            Action action = new Action(() =>
            {
                var newmsg = msg.GetMessageObject();
                newmsg.BubbleWidth = 0;
                newmsg.BubbleHeight = 0;
                msgTabAdapter.TargetMsgData.UpdateMsg(newmsg);
                xListView.RefreshItem(index);
            });
            if (this.IsHandleCreated)
                Invoke(action);
        }
        #endregion

        #region 替换为remind类型气泡
        private void ReplaceMsgToRemind(string msgId, string content)
        {
            //不存在该消息
            if (msgTabAdapter.TargetMsgData.GetMsg(msgId) == null)
                return;

            //获取msg
            MessageObject msg = msgTabAdapter.TargetMsgData.GetMsg(msgId);

            msg.type = kWCMessageType.Remind;
            msg.content = content;
            //msg.isReadDel = 0;
            msg.UpdateData();

            Action action = new Action(() =>
            {
                xListView.panel1.SuspendLayout();
                try
                {
                    //避免重复右键菜单，先更新UI
                    msg.BubbleWidth = 0; msg.BubbleHeight = 0;
                    msgTabAdapter.TargetMsgData.UpdateMsg(msg);
                    int index = msgTabAdapter.msgList.FindIndex(m => m.messageId == msg.messageId);
                    xListView.RefreshItem(index);
                    RollBottom_HaveEmpty();
                }
                catch (Exception ex) { LogHelper.log.Error("-------------替换为remind类型气泡出错，方法（ReplaceMsgToRemind）: \r\n" + ex.Message); }
                xListView.panel1.ResumeLayout();
            });
            if (this.IsHandleCreated)
                Invoke(action);
        }
        #endregion
        #endregion

        #region 展示等待符
        LodingUtils loding = new LodingUtils();     //新创一个等待符
        private void StartLoding(Control parent_crl, string title = "")
        {
            //loding = new LodingUtils();
            loding.Title = title;
            loding.size = new Size(30, 30);
            loding.parent = parent_crl;
            loding.BgColor = Color.Transparent;
            loding.start();
        }
        #endregion

        #region 发送消息
        int ln = 0;
        List<string> _ImageList = new List<string>();
        List<string> _ImageList2 = new List<string>();


        #region 发送按钮发送
        private void BtnSend_Click(object sender, EventArgs e)
        {
            txtSend.Focus();
            //选择对象不能为空
            if (ChooseTarget == null || string.IsNullOrEmpty(ChooseTarget.UserId))
                return;
            if (txtSend.Text == null)
            { return; }
            //如果当前为禁言中
            if (!btnSend.Visible)
                return;

            //当文件被移除时并没有从集合中删除
            if (fileCollects != null && fileCollects.Count > 0)
            {
                DroupFeileSend();
            }

            try
            {
                //保存变量，异步执行会导致变量出错
                string strSend = txtSend.Text;



                string rtfSend = txtSend.Rtf;
                txtSend.Clear();
                int eji_num = emoji_num;
                List<string> eji_codes = emoji_codes;
                int at_count = atCurrentCount;
                List<Friend> atFriends = list_atFriends;
                MessageObject replyMsg = replyPanel.ReplyMsg;
                Friend fdSend = ChooseTarget.GetByUserId();
                if (this.SrcImages == null)
                {
                    this.SrcImages = new Dictionary<string, string>();
                }
                Dictionary<string, string> SrcImages = this.SrcImages.Copy();
                //List<string> fileCollect_imageRtf = new List<string>();
                //if (this.fileCollect_imageRtf != null)
                //{
                //    fileCollect_imageRtf = this.fileCollect_imageRtf.Copy();
                //}
                List<FileCollect> fCollects = (fileCollects != null && fileCollects.Count > 0) ? fileCollects.Copy() : new List<FileCollect>();
                Thread sendThread = new Thread(() =>
                {
                    //输入不能为空
                    if (string.IsNullOrEmpty(strSend) && eji_num < 1)
                    {
                        //不存在图片
                        if (!EQControlManager.JudgeRtfHaveImg(rtfSend))
                            return;
                    }

                    //emoji表情转为code
                    if (eji_codes != null && eji_codes.Count > 0)
                        rtfSend = EmojiPngToCode(rtfSend, eji_codes);
                    //用于rtf的转化
                    using (RichTextBox richTextBox = new RichTextBox())
                    {
                        richTextBox.Rtf = rtfSend;

                        #region 解析图片并单独发送
                        try
                        {
                            //用正则表达式，获取图片rtf
                            //MatchCollection matchs = Regex.Matches(richTextBox.Rtf, @"{\\object[^}]+}", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                            //foreach (Match item in matchs)
                            //{
                            //    richTextBox.Rtf = richTextBox.Rtf.Replace(item.Value, "");
                            //}

                            //List<Image> list_images = new List<Image>();

                            //先移除所有文件缩略图图片，避免错误发送
                            foreach (FileCollect item in fCollects)
                            {
                                string value = item.ImgRtf;
                                richTextBox.Rtf = richTextBox.Rtf.Replace(value, "");
                            }

                            //通过粘贴剪切图片等正常途径添加的图片
                            foreach (string filePath in SrcImages.Keys)
                            {
                                string old_rtf = richTextBox.Rtf;
                                string img_rtf = SrcImages[filePath].Replace("{\\*\\picprop{\\sp{\\sn wzDescription}{\\sv Image}}{\\sp{\\sn posv}{\\sv 1}}\r\n}\\pngblip", "\\wmetafile8");
                                //var result = old_rtf.Split(new string[] { img_rtf }, StringSplitOptions.None);
                                int pic_num = old_rtf.Split(new string[] { img_rtf }, StringSplitOptions.None).Length - 1;  //该图片有多少张
                                richTextBox.Rtf = richTextBox.Rtf.Replace(img_rtf, "");
                                //如果替换之后内容不变，代表图片已经从文本框删除
                                if (old_rtf.Equals(richTextBox.Rtf))
                                    continue;

                                //保存图片到本地
                                //string filePath = EQControlManager.RtfToImageSave(item.Value);

                                //可能发送多张相同的图片
                                for (int index = 0; index < pic_num; index++)
                                {
                                    //发送图片
                                    if (!string.IsNullOrEmpty(filePath))
                                    {
                                        //添加气泡到列表
                                        MessageObject msg = ShiKuManager.SendImageMessage(fdSend, "", filePath, Convert.ToInt32(new FileInfo(filePath).Length), false);
                                        JudgeMsgIsAddToPanel(msg);

                                        //上传完成
                                        UploadEngine.Instance.From(filePath).
                                            UploadFile((success, url_path) =>
                                            {
                                                MessageObject t_msg = msgTabAdapter.TargetMsgData.GetMsg(msg.messageId);
                                                if (t_msg != null)
                                                {
                                                    //修改气泡的图片和样式
                                                    //string name = "talk_panel_" + t_msg.messageId;
                                                    EQBaseControl talk_panel = GetTalkPanelByMsg(t_msg.messageId);
                                                    if (talk_panel != null && talk_panel is EQImageControl imageCrl)
                                                    {
                                                        UploadImage(imageCrl, t_msg, url_path, success);
                                                    }
                                                }
                                                //如果当前聊天对象已切换，则不更新UI只发送
                                                if (!ChooseTarget.UserId.Equals(msg.fromUserId) && success)
                                                {
                                                    msg.content = url_path;
                                                    msg.UpdateMessageContent();
                                                    ShiKuManager.SendMessage(msg);
                                                }
                                            });
                                    }
                                }
                            }
                            //通过其他渠道在文本框添加的图片（如输入法的表情）
                            if (EQControlManager.JudgeRtfHaveImg(richTextBox.Rtf))
                            {
                                //用正则表达式，获取图片rtf
                                MatchCollection matchs = Regex.Matches(richTextBox.Rtf, @"{\\pict[^}]+}", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                                foreach (Match item in matchs)
                                {
                                    string item_rtf = item.Value;
                                    Image img = EQControlManager.RtfToImageSave(item_rtf);
                                    string filePath = Applicate.LocalConfigData.ImageFolderPath + Guid.NewGuid().ToString("N") + ".png";
                                    img.Save(filePath);

                                    //添加气泡到列表
                                    MessageObject msg_item = ShiKuManager.SendImageMessage(fdSend, "", filePath, Convert.ToInt32(new FileInfo(filePath).Length), false);
                                    JudgeMsgIsAddToPanel(msg_item);

                                    //上传完成
                                    UploadEngine.Instance.From(filePath).
                                        UploadFile((success, url_path) =>
                                        {
                                            MessageObject t_msg = msgTabAdapter.TargetMsgData.GetMsg(msg_item.messageId);
                                            if (t_msg != null)
                                            {
                                                //修改气泡的图片和样式
                                                string name = "talk_panel_" + t_msg.messageId;
                                                EQBaseControl talk_panel = GetTalkPanelByMsg(t_msg.messageId);
                                                if (talk_panel is EQImageControl imageCrl)
                                                {
                                                    UploadImage(imageCrl, t_msg, url_path, success);
                                                }
                                            }
                                            //如果当前聊天对象已切换，则不更新UI只发送
                                            if (!ChooseTarget.UserId.Equals(msg_item.fromUserId) && success)
                                            {
                                                msg_item.content = url_path;
                                                msg_item.UpdateMessageContent();


                                                //ShiKuManager.xmpp.SendMessage(msg);
                                            }
                                        });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.log.Error("----解析图片出错，方法（btnSend_Click） : " + ex.Message);
                        }
                        #endregion

                        //发送文本消息
                        try
                        {
                            if (!string.IsNullOrEmpty(strSend) && !string.IsNullOrWhiteSpace(richTextBox.Text))
                            {
                                //如果是长文本消息
                                if (richTextBox.Text.Length > 3000)
                                    SendTxtFile_LongTextMsg(richTextBox.Text.TrimEnd());
                                else
                                {
                                    // 修改搜狗输入法回车后直接发出文字的问题#7303
                                    //var textarray = richTextBox.Text.ToArray();
                                    //if (textarray.Length > 0)
                                    //{
                                    //    char cc = textarray[textarray.Length - 1];
                                    //    if (10 != cc && sender == null)
                                    //    {
                                    //        return;
                                    //    }
                                    //}

                                    MessageObject txt_msg = null;
                                    //回复消息
                                    if (replyMsg != null && !string.IsNullOrEmpty(replyMsg.messageId))
                                        txt_msg = ShiKuManager.SendReplayMessage(fdSend, replyMsg, richTextBox.Text, false);
                                    else
                                    {
                                        // 修改禅道7938
                                        if (at_count > 0 && !UIUtils.IsNull(atFriends))
                                        {
                                            atFriends = atFriends.FindIndex(f => f.UserId.Equals("allRoomMember")) > -1 ? new List<Friend>() : atFriends;
                                            txt_msg = ShiKuManager.SendAtMessage(fdSend, atFriends, richTextBox.Text, false);
                                        }
                                        else
                                            txt_msg = ShiKuManager.SendTextMessage(fdSend, richTextBox.Text.TrimEnd(), false);

                                        if (txt_msg.content != null)
                                        {
                                            for (int i = 0; i < CommandLst.Count; i++)
                                            {
                                                if (CommandLst[i].Contains(txt_msg.content))
                                                {
                                                    RedId = CommandLst[i][1];
                                                    Messenger.Default.Send(RedId, MessageActions.RED_OPEN_COMMAND);// 更新UI消息
                                                    CommandLst.Remove(CommandLst[i]);
                                                }
                                            }
                                        }
                                    }
                                    JudgeMsgIsAddToPanel(txt_msg);
                                    //添加消息气泡
                                    //指定发送的UserId
                                    ShiKuManager.SendMessage(txt_msg);
                                }
                            }


                            //清空发送框
                            txtSend.Focus();
                            txtSend.Clear();
                        }
                        catch (Exception ex)
                        {
                            LogHelper.log.Error("----发送文本消息出错，方法（btnSend_Click） : " + ex.Message);
                        }
                    }

                });
                if (this.IsHandleCreated)
                {
                    sendThread.SetApartmentState(ApartmentState.STA);
                    sendThread.Start();
                }
            }
            catch (Exception ex) { LogHelper.log.Error("----发送消息出错，方法（btnSend_Click） : " + ex.Message, ex); }

            emoji_num = 0;
            atCurrentCount = 0;
            list_atFriends = new List<Friend>();
            fileCollects.Clear();
            //bug7688()
            // this.SrcImages = new Dictionary<string, string>();
            //this.fileCollect_imageRtf = new List<string>();

            //清空回复面板
            if (replyPanel.ReplyMsg != null && !string.IsNullOrEmpty(replyPanel.ReplyMsg.messageId))
            {
                replyPanel.ReplyMsg = new MessageObject();
                replyPanel.Visible = false;;
            }
        }
        #endregion

        #region 长文本转文件消息发送
        private void SendTxtFile_LongTextMsg(string txt_msg)
        {
            //保存的本地路径   //文件命名规则：2019-06-25 时间戳
            string local_path = Applicate.LocalConfigData.FileFolderPath +
                string.Format(@"文件({0}).txt", DateTime.Now.ToString("yyyy-MM-dd") + " " + TimeUtils.CurrentIntTime());
            FileStream fs = new FileStream(local_path, FileMode.OpenOrCreate);
            StreamWriter wr = null;
            wr = new StreamWriter(fs);
            wr.WriteLine(txt_msg);
            wr.Close();

            //上传并发送文件
            int file_size = Convert.ToInt32(new FileInfo(local_path).Length);
            UploadFileOrImage(local_path, file_size);
        }
        #endregion

        #endregion

        #region 文本框事件
        #region 发送消息文本框，文本变更事件
        private bool isChangingEmoji = false;   //是否正在转换emoji表情
        private int txtChangedState = 1;    //0为正在选择@成员，不再弹出好友选择器
        private int atCurrentCount = 0;    //@的人数
        private int atLastCount = 0;
        private List<Friend> list_atFriends = new List<Friend>();
        private FrmFriendSelect frmFriendSelect = null;
        private void TxtSend_TextChanged(object sender, EventArgs e)
        {
            #region 图片被删除 liuhuan2019/08/03
            if (ln > txtSend.TextLength && fileCollects.Count != 0)
            {
                ReadImg(ref _ImageList2);
                //获取删除的图片
                List<string> reducelist = _ImageList.Except(_ImageList2).ToList();
                if (reducelist.Count > 0) //代表有删除过图片
                {
                    int index = _ImageList.FindIndex(r => r == reducelist[0]);
                    if (index != -1)
                    {
                        fileCollects.RemoveAt(index);
                    }
                }
            }
            ln = txtSend.TextLength;
            ReadImg(ref _ImageList);
            #endregion

            #region 群@功能 at群成员  @群成员
            if (ChooseTarget != null && ChooseTarget.IsGroup == 1)
            {
                string[] sArray = txtSend.Text.Split('@');
                atCurrentCount = sArray.Length - 1;
                if (atCurrentCount > 0 && txtChangedState == 1)
                {
                    //是否新输入了@字符
                    if ((atCurrentCount - atLastCount) > 0)
                    {
                        string addAtMember = "";    //@的群员
                        int addAtCount = 0;         //单次好友选择器添加的@数量
                        if (frmFriendSelect != null && !frmFriendSelect.IsDisposed)
                        {
                            //在当前聊天对象下输入了@
                            if (frmFriendSelect.RoomId.Equals(ChooseTarget.RoomId))
                            {
                                frmFriendSelect.Show();
                                frmFriendSelect.BringToFront();
                                atCurrentCount++;
                                atLastCount = atCurrentCount;
                                return;
                            }
                            //换了聊天对象，但没有关闭好友选择器
                            frmFriendSelect.Close();
                            frmFriendSelect.Dispose();
                            frmFriendSelect = null;
                        }



                        frmFriendSelect = new FrmFriendSelect();

                        // 关闭了显示群成员且不是群主管理员
                        if (ChooseTarget.ShowMember == 0 && !JudgeIsAdmin(Applicate.MyAccount.userId))
                        {
                            // 告诉好友选择器不要显示普通群成员
                            frmFriendSelect.LoadFriendsData(ChooseTarget, true, 1);
                        }
                        else
                        {
                        frmFriendSelect.LoadFriendsData(ChooseTarget, true);
                        }

                        frmFriendSelect.AddConfrmListener((UserFriends) =>
                        {
                            if (UserFriends == null || UserFriends.Count == 0)
                            {
                                txtSend.Text = "";
                                atLastCount = atCurrentCount;
                                return;
                            }

                            txtChangedState = 0;
                            foreach (var friend in UserFriends.Values)
                            {
                                addAtMember += "@" + (string.IsNullOrEmpty(friend.RemarkName) ? friend.NickName : friend.RemarkName) + " ";
                                addAtCount++;
                                list_atFriends.Add(friend);
                            }

                            //选择完好友后
                            if (txtSend.SelectionStart > 0)
                            {
                                string insertLastStr = txtSend.Text.Substring(txtSend.SelectionStart - 1, 1);
                                if (insertLastStr.Equals("@"))
                                {
                                    addAtMember = addAtMember.Substring(1);     //去除第一个@符
                                }
                            }
                            int SelectIndex = txtSend.SelectionStart;
                            txtSend.Text = txtSend.Text.Insert(txtSend.SelectionStart, addAtMember);
                            //EQPosition txtSend_CursorPosition = EQControlManager.GetRichTextPosition(txtSend);
                            atCurrentCount += addAtCount - 1;     //需要手动计算好友选择器中选择了几个好友
                            atLastCount = atCurrentCount;

                            //修改@颜色
                            //EQControlManager.GroupAtModifyColor(txtSend);
                            txtSend.SelectionStart = SelectIndex + addAtMember.Length;
                            txtChangedState = 1;
                        });
                    }
                }
                //如果是从文本框中去除了一些@符，或者是没有选择好友又回到文本框输入@
                //if(atCurrentCount < atLastCount)
                //{
                atLastCount = atCurrentCount;
                //}
            }
            #endregion


            //if (lastInputCount - txtSend.TextLength == -1)
            //{
            //    lastInputCount = txtSend.TextLength;
            //    char cc = txtSend.Text.ElementAt(txtSend.TextLength - 1);
            //    if (cc == 93)
            //    {
            //        return;
            //    }
            //    Console.WriteLine("ccc");
            //}

            #region 修改emojiCode转图片
            //正在转化emoji表情
            if (!isChangingEmoji)
            {
                if (string.IsNullOrEmpty(txtSend.Text) || txtSend.Text.IndexOf("[") < 0)
                    return;

                Console.WriteLine("TextChanged_Emoji");
                //匹配符合规则的表情code
                //MatchCollection matchs = Regex.Matches(txtSend.Text, @"\[[a-z_-]*\]", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                //if (matchs.Count > 0)
                //{
                isChangingEmoji = true;
                txtSend.SuspendLayout();
                try
                {
                    //string rtf = txtSend.Rtf;
                    //Parallel.ForEach(matchs.OfType<Match>(), match =>
                    //{
                    //    string emoji_rtf = EmojiCodeDictionary.GetEmojiRtfByCode(match.Value);
                    //    if (!string.IsNullOrEmpty(emoji_rtf))
                    //    {
                    //        emoji_codes.Add(match.Value);
                    //        rtf = rtf.Replace(match.Value, emoji_rtf);
                    //    }
                    //});
                    //txtSend.Rtf = rtf;    
                    //

                    bool isParallel = false;
                    stopwatch.Restart();
                    string[] rtfs = txtSend.Rtf.Split(']');
                    Parallel.For(0, rtfs.Length, (index, loopState) =>
                    //Parallel.ForEach(rtfs.OfType<string>(), rtf =>
                    {
                        if (string.IsNullOrEmpty(rtfs[index]) || rtfs[index].IndexOf("[") < 0)
                            loopState.Break();
                        //匹配符合规则的表情code
                        MatchCollection matchs = Regex.Matches(rtfs[index] + "]", @"\[[a-z_-]*\]", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        if (matchs.Count > 0)
                        {
                            string emoji_rtf = EmojiCodeDictionary.GetEmojiRtfByCode(matchs[0].Value);
                            if (!string.IsNullOrEmpty(emoji_rtf))
                            {
                                isParallel = true;
                                emoji_codes.Add(matchs[0].Value);
                                rtfs[index] = (rtfs[index] + "]").Replace(matchs[0].Value, emoji_rtf);
                            }

                        }
                    });

                    // 修改禅道#6891 还是会有问题
                    if (isParallel)
                    {
                        string new_rtf = string.Empty;
                        for (int i = 0; i < rtfs.Length; i++)
                        {
                            new_rtf += rtfs[i];
                        }
                        txtSend.Rtf = new_rtf;//rtf赋值
                    }

                    //List<string> replacedEmoji = new List<string>();      
                    //foreach (Match match in matchs)
                    //{
                    //    if (replacedEmoji.Contains(match.Value))
                    //        continue;

                    //    string emoji_rtf = EmojiCodeDictionary.GetEmojiRtfByCode(match.Value);
                    //    if (!string.IsNullOrEmpty(emoji_rtf))
                    //    {
                    //        if (!emoji_codes.Contains(match.Value))
                    //            emoji_codes.Add(match.Value);
                    //        txtSend.Rtf = txtSend.Rtf.Replace(match.Value, emoji_rtf);
                    //    }

                    //    replacedEmoji.Add(match.Value);
                    //}
                }
                catch (Exception ex)
                {
                    LogHelper.log.Error("----转化emoji出错，方法（txtSend_TextChanged） : " + ex.Message, ex);
                }
                txtSend.ResumeLayout();
                isChangingEmoji = false;
                stopwatch.Stop();
                //Console.WriteLine("输出时间：" + stopwatch.ElapsedMilliseconds + "ms");
                //}
            }
            #endregion
        }
        #endregion

        #region 给对方发送我正在输入的通知
        private void TxtSend_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (ChooseTarget.UserType > FriendType.USER_TYPE || ChooseTarget.IsGroup == 1)
                    return;
                ShiKuManager.SendInputMessage(ChooseTarget);
            }
        }
        #endregion

        #region 监控按键
        private void TxtSend_KeyUp(object sender, KeyEventArgs e)
        {
            //当Ctrl+V按太快，会导致e.Control == false
            if (e.KeyCode == Keys.V)
            {
                try
                {
                    if (e.Control)
                        LogHelper.log.Debug("KeyUp start => Ctrl+V");
                    //Console.WriteLine("KeyUp complate => Ctrl+V");
                    else
                        LogHelper.log.Debug("KeyUp start => V");
                    //Console.WriteLine("KeyUp complate => V");
                    if (!string.IsNullOrEmpty(paste_path))
                    {
                        //IDataObject IData = Clipboard.GetDataObject();
                        //if (IData.GetDataPresent(DataFormats.Bitmap))
                        //using (Image paste_image = Image.FromFile(paste_path))
                        //{
                        if (!BitmapUtils.IsNull(paste_image))
                        {
                            //Console.WriteLine("width: " + paste_image.Width);
                            //Console.WriteLine("height: " + paste_image.Height);
                            //恢复到初始
                            Clipboard.Clear();
                            Clipboard.SetDataObject(paste_image, true, 3, 200);//将图片放在剪贴板中
                            paste_path = string.Empty;
                        }
                        //}
                    }
                }
                catch (Exception ex) { LogHelper.log.Error("-----------粘贴图片到文本框时出错，txtSend_KeyUp：", ex); }
            }
            //if (e.KeyData == Keys.Return)
            //{
            //    BtnSend_Click(null, null);

            //    var textarray = txtSend.Text.ToArray();
            //    if (textarray.Length > 0 && 10 == textarray[textarray.Length - 1])
            //    {
            //        txtSend.Clear();
            //    }

            //}
            //防止输入框内文本回车换行  #不可以在此添加清除，否则在搜狗输入法按回车会导致错误的触发
            //if (e.KeyData == Keys.Enter)
            //    txtSend.Text = "";

        }

        private static string paste_path = "";      //用于短暂保存粘贴板的图片
        private static Image paste_image = null;      //用于短暂保存粘贴板的图片
        private void TxtSend_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyData == Keys.Enter)
            {
                MouseEventArgs eventArgs = new MouseEventArgs(MouseButtons.Left, 1, 1, 1, 0);
                BtnSend_Click(btnSend, eventArgs);
            }
            //Ctrl+V
            if (e.Control && e.KeyCode == Keys.V)
            {
                try
                {
                    ////stopwatch.Restart();
                    //IDataObject IData = Clipboard.GetDataObject();
                    //if (IData.GetDataPresent(DataFormats.Bitmap))
                    //{
                    //    //保存路径
                    //    string filePath = Applicate.LocalConfigData.ImageFolderPath + Guid.NewGuid().ToString("N") + ".png";
                    //    //获取黏贴板的图片
                    //    Image image = (Bitmap)IData.GetData(DataFormats.Bitmap);
                    //    //MemoryStream stream = (MemoryStream)IData.GetData(DataFormats.Dib);
                    //    //var img = Image.FromStream(stream);
                    //    //Image image = Clipboard.GetImage();
                    //    paste_image = image;
                    //    paste_path = filePath;
                    //    int new_width = image.Width, new_height = image.Height;
                    //    EQControlManager.ModifyWidthAndHeight(ref new_width, ref new_height, 150, 150);
                    //    Image new_image = new Bitmap(image, new_width, new_height);

                    //    Clipboard.Clear();
                    //    Clipboard.SetDataObject(new_image, true, 3, 200);//将图片放在剪贴板中

                    //    //Task.Factory.StartNew(() =>
                    //    //{
                    //    image.MySave(filePath, ImageFormat.Png);
                    //    //获取截图的图片的RTF
                    //    using (RichTextBox richTextBox = new RichTextBox())
                    //    {
                    //        richTextBox.Paste();
                    //        string txt_rtf = richTextBox.Rtf;
                    //        string image_rtf = EQControlManager.subRtf(txt_rtf);
                    //        SrcImages.Add(filePath, image_rtf);
                    //        //Console.WriteLine("SrcImage_Count: " + SrcImages.Count);
                    //        //Console.WriteLine("filePath:" + filePath);
                    //    }
                    //    //});
                    //}
                    //if (IData.GetDataPresent(DataFormats.Rtf))      //Ctrl+C对方的消息后，可能会有emoji的rtf
                    //{
                    //    using (RichTextBox richTextBox = new RichTextBox())
                    //    {
                    //        richTextBox.Paste();
                    //        foreach (string emoji_code in EmojiCodeDictionary.GetEmojiDataNotMine().Keys)
                    //        {
                    //            string emoji_rtf = EmojiCodeDictionary.GetEmojiRtfByCode(emoji_code);
                    //            if (richTextBox.Rtf.IndexOf(emoji_rtf) >= 0)
                    //            {
                    //                //不存在才添加
                    //                if (emoji_codes.FindIndex(code => code.Equals("[" + emoji_code + "]")) < 0)
                    //                {
                    //                    emoji_num++;
                    //                    emoji_codes.Add("[" + emoji_code + "]");
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    ////检查是否黏贴文件
                    //if (Clipboard.GetFileDropList().Count > 0)
                    //{
                    //    var strCollection = Clipboard.GetFileDropList();
                    //    foreach (string item in strCollection)
                    //        FileCollectUtils.AddFileCollectToBox(this.txtSend, item, fileCollects);
                    //}
                    ////stopwatch.Stop();
                    ////Console.WriteLine("输出时间：" + stopwatch.ElapsedMilliseconds + "ms");
                    //LogHelper.log.Debug("KeyDown complate => Ctrl+V");
                    ////Console.WriteLine("KeyDown complate => Ctrl+V");

                    e.Handled = true;
                    MenuItem_paste_Click(this.menuItem_paste, new MouseEventArgs(MouseButtons.Left, 1, 1, 1, 0));
                    txtSend.SelectionStart = txtSend.Text.Length;
                }
                catch (Exception ex) { LogHelper.log.Error("-----------粘贴数据到文本框时出错，txtSend_KeyDown：", ex); }
            }
        }

        private void TxtSend_KeyPress(object sender, KeyPressEventArgs e)
        {
            //if (e.KeyChar == 13)     //回车键
            //{
            //    int last_index = txtSend.Rtf.LastIndexOf("\\par\r\n");
            //    if (last_index > -1)
            //    {
            //        txtSend.Rtf = txtSend.Rtf.Remove(last_index, 6);
            //        //txtSend.Text = txtSend.Text.TrimEnd();
            //        if (!string.IsNullOrEmpty(txtSend.Text))
            //            BtnSend_Click(null, null);
            //    }
            //    //var textarray = txtSend.Text.ToArray();
            //    //if (textarray.Length > 0 && 10 == textarray[textarray.Length - 1])
            //    //{
            //    //    txtSend.Clear();
            //    //}
            //}
        }
        #endregion

        private void TxtSend_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                //只对图片的双击做处理
                if (txtSend.SelectedRtf.IndexOf("{\\pict") < 0)
                    return;

                Image image = null;
                string filePath = "";
                string image_rtf = EQControlManager.subRtf(txtSend.SelectedRtf);
                if (image_rtf.LastIndexOf("}\r\n") == image_rtf.Length - "}\r\n".Length)
                    image_rtf = image_rtf.Remove(image_rtf.LastIndexOf("}\r\n"));
                if (SrcImages.ContainsValue(image_rtf))
                {
                    var result = SrcImages.FirstOrDefault(d => d.Value.Equals(image_rtf));
                    if (result.Key != null && File.Exists(result.Key))
                    {
                        filePath = result.Key;
                        image = Image.FromFile(filePath);
                    }
                }
                //没有在字典中获得到图片
                if (image == null || BitmapUtils.IsNull(image))
                {
                    image = EQControlManager.RtfToImageSave(image_rtf);
                }

                if (image != null && !BitmapUtils.IsNull(image))
                {
                    //打开图片编辑器
                    FrmEditImage frmEditImage = new FrmEditImage()
                    {
                        Image = image,
                        filePath = filePath
                    };
                    frmEditImage.Show();
                    frmEditImage.action_ok = (img) =>
                    {
                        int new_width = img.Width, new_height = img.Height;
                        EQControlManager.ModifyWidthAndHeight(ref new_width, ref new_height, 150, 150);
                        Image new_image = new Bitmap(img, new_width, new_height);
                        string new_rtf = EQControlManager.ImageToRtf(new_image);
                        if (!string.IsNullOrEmpty(new_rtf))
                        {
                            txtSend.Rtf = txtSend.Rtf.Replace(image_rtf, new_rtf);
                            SrcImages.Add(frmEditImage.filePath, new_rtf);
                        }
                    };
                }
                else
                    LogHelper.log.Error("---------------双击弹出图片无法获取到图片：\r\n image: " + image);
            }
            catch (Exception ex) { LogHelper.log.Error("---------------双击弹出图片编辑窗出错：\r\n" + ex.Message); }
        }
        #endregion

        #region 按回执更新送达消息状态
        private void DrawIsSend(MessageObject msg)
        {
            MessageObject t_msg = msgTabAdapter.TargetMsgData.GetMsg(msg.messageId);
            //不属于自己的消息回执
            if (t_msg == null)
                return;
            t_msg.isSend = msg.isSend;
            // 修改禅道9394
            t_msg.signature = msg.signature;
            if (t_msg == null || string.IsNullOrEmpty(t_msg.messageId))
                return;

            if (t_msg.fromUserId != Applicate.MyAccount.userId || ChooseTarget == null)
                return;

            //获得需要更新的控件
            EQBaseControl talk_panel = GetTalkPanelByMsg(t_msg.messageId);

            //是否存在该气泡消息和送达标识
            if (talk_panel == null || talk_panel.Controls["lab_msg"] == null)
                return;

            //群聊没有送达
            if (ChooseTarget.IsGroup == 1 && t_msg.isSend == 1)
            {
                if (this.IsHandleCreated)
                {

                    Invoke(new Action(() =>
                    {
                        if (ChooseTarget.ShowRead == 1)
                            EQControlManager.DrawReadPerson(t_msg.readPersons, (Label)talk_panel.Controls["lab_msg"]);
                        else
                        {
                            var result = talk_panel.Controls.Find("lab_msg", true);
                            if (result.Length > 0 && result[0] is Label lab_msg)
                                lab_msg.Visible = false;
                        }
                    }));
                }
                else
                {
                    Console.WriteLine("IsHandleCreated:  " + t_msg.content);
                }

                return;
            }
            //修改气泡的发送状态
            Action action = new Action(() =>
            {
                if (talk_panel.Controls["lab_msg"] != null)
                    EQControlManager.DrawIsSend(t_msg, (Label)talk_panel.Controls["lab_msg"]);
            });
            if (this.IsHandleCreated)
                Invoke(action);
        }
        #endregion

        #region 按回执更新已读消息状态
        private void DrawIsRead(MessageObject msg)
        {
            //获得需要更新的控件
            EQBaseControl talk_panel = GetTalkPanelByMsg(msg.messageId);

            //是否存在该气泡消息
            if (talk_panel == null || (talk_panel.Controls["lab_msg"] == null && talk_panel.Controls["lab_readDeleted"] == null))
            {
                // 气泡还没创建
                msgTabAdapter.TargetMsgData.UpdateMsg(msg);
                return;
            }

            MessageObject t_msg = msgTabAdapter.TargetMsgData.GetMsg(msg.messageId);
            //是否为阅后即焚的已读通知
            int isReadDel = 0;
            if (t_msg != null)
                isReadDel = t_msg.isReadDel;
            if (isReadDel == 1)
            {
                if (msg.FromId != Applicate.MyAccount.userId)
                {
                    RemoveMsgOfPanel(msg.messageId);
                }
                else
                {
                    ReplaceMsgToRemind(msg.messageId, "对方查看了您的这条阅后即焚消息");
                }
            }
            else
            {
                if (talk_panel.Controls["lab_msg"] != null && talk_panel.Controls["lab_msg"] is Label lab_msg)
                {

                    //群需要开启群已读
                    if (ChooseTarget.IsGroup == 1)
                    {
                        if (this.IsHandleCreated)
                            Invoke(new Action(() =>
                            {
                                //显示群已读
                                if (ChooseTarget.ShowRead == 1)
                                {
                                    Console.WriteLine("DrawReadPersondd " + msg.readPersons + " ,  " + msg.content);
                                    EQControlManager.DrawReadPerson(msg.readPersons, lab_msg);
                                }
                            }));


                        if (!this.IsHandleCreated)
                        {
                            Console.WriteLine("DrawReadPerson:  " + msg.content);
                        }
                    }
                    else
                    {
                        if (msg.fromUserId != Applicate.MyAccount.userId)
                            return;

                        Action action = new Action(() =>
                        {
                            EQControlManager.DrawIsRead(lab_msg);
                        });
                        if (this.IsHandleCreated)
                            Invoke(action);
                    }
                }
            }
        }
        #endregion

        #region 处理emoji表情
        private void AddEmojiToTxtSend(string emoji_code)
        {
            int selectionIndex = txtSend.SelectionStart;    //记录鼠标的当前光标
            ////获取表情字典
            //Dictionary<string, string> emojiData = EmojiCodeDictionary.GetEmojiCodeDataDictionary();
            ////获取emoji图并转为rtf
            //string emoji_rtf = GetEmoji(emoji_code, Color.White);
            ////截取emoji图片部分的rtf
            //emoji_rtf = subRtf(emoji_rtf);
            ////如果字典中不存在，则添加，用作后来的互转
            //if (!emojiData.ContainsKey(emoji_rtf))
            //    emojiData.Add(emoji_rtf, emoji_code);

            //创建一个副本
            //RichTextBox richSend = new RichTextBox();
            //richSend.Rtf = EmojiPngToCode(txtSend.Rtf);
            ////从光标处添加
            //richSend.SelectedText += emoji_code;
            //richSend.Rtf = GetEmoji(richSend.Text, Color.White);
            //txtSend.Rtf = richSend.Rtf;
            //richSend.Dispose();

            txtSend.SelectedText += emoji_code;
            //txtSend.Rtf = GetEmojiByRtf(txtSend.Rtf, Color.White);
            //txtSend.Rtf = txtSend.Rtf.Replace(emoji_code, EmojiCodeDictionary.GetEmojiRtfByCode(emoji_code));

            emoji_num++;
            //emoji_codes.Add(emoji_code);
            Helpers.ClearMemory();

            txtSend.SelectionStart = selectionIndex + 1;    //修改光标的位置
        }

        /// <summary>
        /// 根据传递的rtf转为code文本
        /// </summary>
        /// <param name="rtf"></param>
        /// <returns></returns>
        public static string EmojiPngToCode(string rtf, List<string> emoji_codes)
        {
            ////获取表情字典
            //Dictionary<string, string> emojiData = EmojiCodeDictionary.GetEmojiCodeDataDictionary();

            ////创建一个副本
            //RichTextBox richSend = new RichTextBox();
            //richSend.Rtf = rtf;
            ////通过循环，把rtf图片替换为code
            //foreach (var key in emojiData.Keys)
            //{
            //    RichTextBox rich = new RichTextBox();
            //    rich.Text = emojiData[key];
            //    string replace = subRtf(rich.Rtf).Trim();
            //    richSend.Rtf = richSend.Rtf.Replace(key, replace);
            //    rich.Dispose();
            //}
            //string new_rtf = richSend.Rtf;
            //richSend.Dispose();

            string new_rtf = rtf;
            foreach (string emojiCode in emoji_codes)
            {
                //string code = "[" + emojiCode + "]";
                string code = emojiCode;
                string emojiRtf = EmojiCodeDictionary.GetEmojiRtfByCode(code);
                if (!string.IsNullOrWhiteSpace(emojiRtf))
                    new_rtf = new_rtf.Replace(emojiRtf, code);
            }
            return new_rtf;
        }

        private string SubRtf(string emoji_rtf)
        {
            //rtf1–> RTF版本
            //ansi–> 字符集
            //ansicpg936–> 简体中文
            //deff0–> 默认字体0
            //deflang1033–> 美国英语
            //deflangfe2052–> 中国汉语
            //fonttb–> 字体列表
            //f0->字体0
            //fcharset134->GB2312国标码
            //‘cb\’ce\’cc\’e5–> 宋体
            int startIndex = emoji_rtf.IndexOf("\\viewkind4\\uc1\\pard\\lang2052\\f0\\fs18") + "\\viewkind4\\uc1\\pard\\lang2052\\f0\\fs18".Length;
            emoji_rtf = emoji_rtf.Substring(startIndex);
            int endIndex = emoji_rtf.IndexOf("\\par");
            emoji_rtf = emoji_rtf.Substring(0, endIndex);
            return emoji_rtf;
        }

        #region StringToEmoji
        /// <summary>
        /// 传递含有emoji code的文本，返回转化为图片后的rtf字符串
        /// </summary>
        /// <param name="ric_text">含有emoji code的文本</param>
        /// <param name="bg_cloor">填充绘画底色的背景色</param>
        /// <returns></returns>
        private void GetEmoji(string ric_text, Color bg_cloor)
        {
            //RichTextBox richTextBox = new RichTextBox
            //{
            //    Text = ric_text
            //};
            ////匹配符合规则的表情code
            //MatchCollection match = Regex.Matches(richTextBox.Text, @"\[[a-z_-]*\]", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            //string[] newStr = new string[match.Count];
            ////不用做记录的变量
            //int index = 0;
            //foreach (Match item in match)
            //{
            //    newStr[index] = item.Groups[0].Value;
            //    index++;
            //}

            ////循环替换code为表情图片
            //for (int i = 0; i < newStr.Length; i++)
            //{
            //    //获取表情code在RichTextBox的位置
            //    index = richTextBox.Text.IndexOf(newStr[i]);
            //    //表情code去除[]
            //    string image_name = newStr[i].Replace("[", "").Replace("]", "");
            //    //给剪切板设置图片对象
            //    string path = string.Format(@"Res\Emoji\{0}.png", image_name);
            //    if (!File.Exists(path))
            //        break;

            //    //获取RichTextBox控件中鼠标焦点的索引位置
            //    richTextBox.SelectionStart = index;
            //    //从鼠标焦点处开始选中几个字符
            //    richTextBox.SelectionLength = newStr[i].Length;
            //    //清空剪切板，防止里面之前有内容
            //    Clipboard.Clear();
            //    Bitmap bmp = new Bitmap(path);
            //    Bitmap newBmp = new Bitmap(25, 25);
            //    Graphics g = Graphics.FromImage(newBmp);
            //    //g.Clear(bg_cloor);

            //    //画圆形
            //    GraphicsPath gpath = new GraphicsPath();
            //    gpath.AddEllipse(0, 0, 25, 25);
            //    g.SmoothingMode = SmoothingMode.AntiAlias;
            //    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //    g.CompositingQuality = CompositingQuality.HighQuality;
            //    g.SetClip(gpath);

            //    g.DrawImage(bmp, new Rectangle(0, 0, 25, 25), new Rectangle(0, 0, bmp.Width, bmp.Height), GraphicsUnit.Pixel);
            //    g.Dispose();
            //    Clipboard.SetImage(newBmp);
            //    //将图片粘贴到鼠标焦点位置(选中的字符都会被图片覆盖)
            //    richTextBox.Paste();
            //    Clipboard.Clear();
            //}

            //string result = richTextBox.Rtf;
            //richTextBox.Dispose();
            //return result;
        }
        #endregion

        #region RtfToEmoji
        /// <summary>
        /// 传递含有emoji code的文本，返回转化为图片后的rtf字符串
        /// </summary>
        /// <param name="ric_text">含有emoji code的文本</param>
        /// <param name="bg_cloor">填充绘画底色的背景色</param>
        /// <returns></returns>
        //private string GetEmojiByRtf(string ric_rtf, Color bg_cloor)
        //{
        //    RichTextBox richTextBox = new RichTextBox
        //    {
        //        Rtf = ric_rtf
        //    };
        //    //匹配符合规则的表情code
        //    MatchCollection match = Regex.Matches(richTextBox.Text, @"\[[a-z_-]*\]", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        //    string[] newStr = new string[match.Count];
        //    //不用做记录的变量
        //    int index = 0;
        //    foreach (Match item in match)
        //    {
        //        newStr[index] = item.Groups[0].Value;
        //        index++;
        //    }

        //    //循环替换code为表情图片
        //    for (int i = 0; i < newStr.Length; i++)
        //    {
        //        //获取表情code在RichTextBox的位置
        //        index = richTextBox.Text.IndexOf(newStr[i]);
        //        //表情code去除[]
        //        string image_name = newStr[i].Replace("[", "").Replace("]", "");
        //        //给剪切板设置图片对象
        //        string path = string.Format(@"Res\Emoji\{0}.png", image_name);
        //        if (!File.Exists(path))
        //            break;

        //        //获取RichTextBox控件中鼠标焦点的索引位置
        //        richTextBox.SelectionStart = index;
        //        //从鼠标焦点处开始选中几个字符
        //        richTextBox.SelectionLength = newStr[i].Length;
        //        //清空剪切板，防止里面之前有内容
        //        Clipboard.Clear();
        //        Bitmap bmp = new Bitmap(path);
        //        Bitmap newBmp = new Bitmap(25, 25);
        //        Graphics g = Graphics.FromImage(newBmp);
        //        g.Clear(bg_cloor);

        //        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

        //        g.DrawImage(bmp, new Rectangle(0, 0, 25, 25), new Rectangle(0, 0, bmp.Width, bmp.Height), GraphicsUnit.Pixel);
        //        g.Dispose();
        //        Clipboard.SetImage(newBmp);
        //        //将图片粘贴到鼠标焦点位置(选中的字符都会被图片覆盖)
        //        richTextBox.Paste();
        //        Clipboard.Clear();
        //    }

        //    string result = richTextBox.Rtf;
        //    richTextBox.Dispose();
        //    return result;
        //}
        #endregion
        #endregion

        #region Send File

        #region 上传文件
        private void UploadFileOrImage(string fileLocation, int fileSize)
        {
            try
            {
                MessageObject msg = null;
                if (JudgeIsImage(fileLocation))
                    msg = ShiKuManager.SendImageMessage(ChooseTarget, "", fileLocation, fileSize, false);
                else
                    msg = ShiKuManager.SendFileMessage(ChooseTarget, "", fileLocation, fileSize, false);
                //添加气泡
                JudgeMsgIsAddToPanel(msg);

                //获取气泡
                EQBaseControl talk_panel = GetTalkPanelByMsg(msg.messageId);
                if (talk_panel == null)
                    return;


                if (msg.type == kWCMessageType.File && talk_panel is EQFileControl fileControl)
                {
                    UploadEngine.Instance.From(fileLocation, msg.messageId).
                    //上传中
                    UpProgress((progress) =>
                    {
                        if (fileControl != null)
                            fileControl.isDownloading = true;
                        //if (talk_panel != null)
                        //{
                        //    //获取到文件的panel
                        //    if (talk_panel.Controls.Find("image_panel", true).Length > 0 && talk_panel.Controls.Find("image_panel", true)[0] is Panel image_panel)
                        //        if (image_panel.Controls.Find("crl_content", true).Length > 0 && image_panel.Controls.Find("crl_content", true)[0] is FilePanelLeft filePanel)
                        //        {
                        fileControl.panel_file.lab_lineLime.BringToFront();
                        fileControl.panel_file.lab_lineLime.Width = Convert.ToInt32(fileControl.panel_file.lab_lineSilver.Width * ((decimal)progress / 100));
                        if ((progress / 100) == 1)
                            fileControl.panel_file.lab_lineLime.Width = 0;
                        //        }
                        //}
                    }).
                    UpSpeed((speed) =>
                    {
                        if (fileControl != null)
                        {
                            //获取到文件的panel
                            //if (talk_panel.Controls.Find("image_panel", true).Length > 0 && talk_panel.Controls.Find("image_panel", true)[0] is Panel image_panel)
                            //    if (image_panel.Controls.Find("crl_content", true).Length > 0 && image_panel.Controls.Find("crl_content", true)[0] is FilePanelLeft filePanel)
                            //    {
                            fileControl.panel_file.lblSpeed.Visible = true;
                            fileControl.panel_file.lblSpeed.BringToFront();
                            fileControl.panel_file.lblSpeed.Text = speed + @"/s";
                            //}
                        }
                    }).
                    //上传完成
                    UploadFile((success, url) =>
                    {
                        fileControl.isDownloading = false;
                        //获取到文件的panel
                        //if (talk_panel.Controls.Find("image_panel", true).Length > 0 && talk_panel.Controls.Find("image_panel", true)[0] is Panel image_panel)
                        //    if (image_panel.Controls.Find("crl_content", true).Length > 0 && image_panel.Controls.Find("crl_content", true)[0] is FilePanelLeft filePanel)
                        //关闭下载速度
                        fileControl.panel_file.lblSpeed.Visible = false;

                        msg.content = url;
                        msg.UpdateMessageContent();
                        if (success)
                        {
                            //string localPath = Applicate.LocalConfigData.FileFolderPath + FileUtils.GetFileName(msg.fileName);
                            //if (File.Exists(localPath))//如果对应文件存在 先删除文件(再下载文件)
                            //{
                            //    try
                            //    {
                            //        var r = new Random(Guid.NewGuid().GetHashCode());//产生不重复的随机数
                            //        string filename = FileUtils.GetFileName(msg.fileName);
                            //        string suffix = FileUtils.GetFileExtension(msg.fileName);//取出后缀
                            //        string filename1 = filename.Replace(suffix, "") + "(" + r.Next(0, 1000) + ")" + suffix;//合成名称
                            //        msg.fileName = Applicate.LocalConfigData.FileFolderPath + filename1;//重新赋值
                            //        msg.UpdateFilename();
                            //    }
                            //    catch (Exception)
                            //    {

                            //    }
                            //}
                            ShiKuManager.SendMessage(msg);
                        }
                        else
                        {
                            msg.isSend = -1;
                            msg.UpdateIsSend(-1);
                            EQControlManager.DrawIsSend(msg, talk_panel.lab_msg);
                        }

                    });
                }
                else if (msg.type == kWCMessageType.Image)
                {

                    UploadEngine.Instance.From(fileLocation).
                    //上传完成
                    UploadFile((success, url) =>
                    {
                        UploadImage(talk_panel, msg, url, success);
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.log.Error("----上传文件出错，方法（UploadFileOrImage） : " + ex.Message, ex);
            }
        }
        #endregion

        #region 上传图片
        private void UploadImage(EQBaseControl talk_panel, MessageObject msg, string url, bool success)
        {
            #region 更新msg
            //上传失败
            if (string.IsNullOrEmpty(url) || !success)
            {
                msg.isSend = -1;
                msg.content = "error";
                msg.UpdateData();
            }
            else
            {
                msg.content = url;
                msg.UpdateMessageContent();
                ShiKuManager.SendMessage(msg);
            }
            #endregion

            #region 更新UI
            msg = msgTabAdapter.TargetMsgData.GetMsg(msg.messageId);
            if (msg == null)
                return;
            //修改气泡的图片和样式
            if (talk_panel is EQImageControl imageCrl)
            {
                if (imageCrl.Controls.Find("crl_content", true).Length > 0 && imageCrl.Controls.Find("crl_content", true)[0] is PictureBox pic_image)
                {
                    //上传失败
                    if (string.IsNullOrEmpty(url) || !success)
                    {
                        //关闭等待符
                        if (pic_image.Controls.Find("loding", true).Length > 0 && pic_image.Controls.Find("loding", true)[0] is USELoding loding)
                        {
                            loding.Dispose();
                            Helpers.ClearMemory();
                        }
                        var result = imageCrl.Controls.Find("lab_msg", false);
                        if (result.Length > 0 && result[0] is Label lab_msg)
                            EQControlManager.DrawIsSend(msg, lab_msg);
                    }
                    else
                    {
                        imageCrl.LoadImage(pic_image);
                    }
                }
            }
            #endregion
        }
        #endregion

        #region 上传视频
        private void UploadVideo(EQBaseControl talk_panel, MessageObject msg, string url, bool success)
        {
            #region 更新msg
            //上传失败
            if (string.IsNullOrEmpty(url) || !success)
            {
                msg.isSend = -1;
                msg.content = "error";
                msg.UpdateData();
            }
            else
            {
                msg.content = url;
                msg.UpdateMessageContent();
                ShiKuManager.SendMessage(msg);
            }
            #endregion

            #region 更新ui
            msg = msgTabAdapter.TargetMsgData.GetMsg(msg.messageId);
            if (msg != null)
            {
                //修改气泡的图片和样式
                if (talk_panel is EQVideoControl videoCrl)
                {
                    var video_result = videoCrl.Controls.Find("crl_content", true);
                    if (video_result.Length > 0 && video_result[0] is PictureBox pic_content)
                    {
                        //上传失败
                        if (string.IsNullOrEmpty(url) || !success)
                        {
                            var loding_result = pic_content.Controls.Find("loding", true);
                            //关闭等待符
                            if (loding_result.Length > 0 && loding_result[0] is USELoding loding)
                            {
                                loding.Dispose();
                                Helpers.ClearMemory();
                            }
                            var result = videoCrl.Controls.Find("lab_msg", false);
                            if (result.Length > 0 && result[0] is Label lab_msg)
                                EQControlManager.DrawIsSend(msg, lab_msg);
                        }
                        else
                        {
                            videoCrl.LoadVideo(pic_content);
                        }
                    }
                }
            }
            #endregion
        }
        #endregion

        #region 判断是否为图片
        private bool JudgeIsImage(string fileName)
        {
            try
            {

                if (!File.Exists(fileName))
                {
                    return false;
                }

                string extension = FileUtils.GetFileExtension(fileName);

                if (UIUtils.IsNull(extension))
                {
                    return false;
                }

                if (string.Equals(".jpg", extension.ToLower()))
                {
                    return true;
                }

                if (string.Equals(".jpeg", extension.ToLower()))
                {
                    return true;
                }

                if (string.Equals(".gif", extension.ToLower()))
                {
                    return true;
                }

                if (string.Equals(".png", extension.ToLower()))
                {
                    return true;
                }

                if (string.Equals(".bmp", extension.ToLower()))
                {
                    return true;
                }
                if (string.Equals(".jpeg", extension.ToLower()))
                {
                    return true;
                }
                if (string.Equals(".webp", extension.ToLower()))
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                LogHelper.log.Error("----JudgeIsImage : " + ex.Message, ex);
            }
            return false;
        }
        #endregion
        #endregion

        #region 右键菜单

        #region 获取选中控件的msg
        private MessageObject GetMsgBySelectCrl()
        {
            string messageId = selectControl.Name.Replace("talk_panel_", "");
            return msgTabAdapter.TargetMsgData.GetMsg(messageId);
        }
        #endregion

        #region 删除
        private void MenuItem_Delete_Click(object sender, EventArgs e)
        {
            if (selectControl.Name.IndexOf("talk_panel_") < 0)
                return;

            MessageObject msg = GetMsgBySelectCrl();
            if (msg == null)
                return;
            //获取最后一条消息
            MessageObject lastMsg = msgTabAdapter.TargetMsgData.GetLastIndexMsg();
            //从界面中移除
            msgTabAdapter.RemoveData(rowIndex);

            //调用接口删除
            bool isLastMsg = false;
            //如果是最后一条消息，需要通知最近聊天列表更新最后一条消息
            if (lastMsg != null && lastMsg.messageId == msg.messageId)
                isLastMsg = true;
            //调用接口从服务端删除
            CollectUtils.DelServerMessages(msg, isLastMsg);

            //滚动到底部
            RollBottom_HaveEmpty();

            //更新列表最后一条消息的时间
            //if (isLastMsg)
            //{
            //    int endIndex = msgTabAdapter.msgList.Count() - 1;
            //    lastMsg = msgTabAdapter.msgList[endIndex];
            //    lastMsgTime = lastMsg == null ? 0 : lastMsg.timeSend;
            //}
        }
        #endregion

        #region 撤回
        private void MenuItem_Recall_Click(object sender, EventArgs e)
        {
            if (selectControl == null || selectControl.Name.IndexOf("talk_panel_") < 0)
                return;

            MessageObject messageObject = GetMsgBySelectCrl();
            if (messageObject == null)
                return;

            // 判断我能否撤回这条消息

            // 我一定能撤回自己的消息  群聊我是管理可以随意撤回消息
            bool isOwnerRecall = ChooseTarget.IsGroup == 1 && JudgeIsAdmin(Applicate.MyAccount.userId);
            if (messageObject.myUserId.Equals(messageObject.fromUserId) || isOwnerRecall)
            {
                // 可以撤回

                //重新创建一个msg
                MessageObject msg = messageObject.CopyMessage();
                msg.type = kWCMessageType.Remind;
                msg.content = "你撤回了一条消息";

                if (IsHandleCreated)
                {
                    Invoke(new Action(() =>
                    {
                        //避免重复右键菜单，先更新UI
                        msgTabAdapter.TargetMsgData.UpdateMsg(msg);
                        int index = msgTabAdapter.msgList.FindIndex(m => m.messageId == msg.messageId);
                        xListView.RefreshItem(index);
                    }));
                }

                //调用接口
                HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "tigase/deleteMsg"). //删除消息
                    AddParams("access_token", Applicate.Access_Token).
                    AddParams("type", msg.isGroup == 0 ? "1" : "2").    //1 单聊  2 群聊
                    AddParams("delete", "2").   //1： 删除属于自己的消息记录 2：撤回 删除整条消息记录
                    AddParams("messageId", msg.messageId).
                    AddParams("roomJid", msg.toUserId).
                    Build().ExecuteJson<object>((sccess, obj) =>   //返回值说明： text：加密后的内容
                        {
                            //删除成功
                            if (sccess)
                            {
                                ShiKuManager.SendRecallMessage(msg.GetFriend(), msg);
                                HttpUtils.Instance.ShowTip("撤回成功");
                            }
                        });
            }


            ////重新创建一个msg
            //MessageObject copyMsg = messageObject.CopyMessage();
            //copyMsg.type = kWCMessageType.Remind;
            //copyMsg.content = "你撤回了一条消息";
            //RecallMsg(copyMsg, true);
            RollBottom_HaveEmpty();
        }

        private void RecallMsg(MessageObject msg, bool isSendRecall = false)
        {
            //该消息是否为当前聊天对象
            if (msg.GetFriend().UserId != ChooseTarget.UserId)
                return;
            if (msg.type == kWCMessageType.Withdraw)
            {
                Invoke(new Action(() =>
                {
                    msg.BubbleHeight = 0; msg.BubbleWidth = 0;
                    //避免重复右键菜单，先更新UI
                    msgTabAdapter.TargetMsgData.UpdateMsg(msg);
                    //var s = msgTabAdapter.msgList[rowIndex];
                    int index = msgTabAdapter.msgList.FindIndex(m => m.messageId == msg.messageId);
                    xListView.RefreshItem(index);
                    RollBottom_HaveEmpty();
                }));
                return;
            }

            if (IsHandleCreated)
            {
                Invoke(new Action(() =>
                {
                    msg.BubbleHeight = 0; msg.BubbleWidth = 0;
                    //避免重复右键菜单，先更新UI
                    msgTabAdapter.TargetMsgData.UpdateMsg(msg);
                    //var s = msgTabAdapter.msgList[rowIndex];
                    int index = msgTabAdapter.msgList.FindIndex(m => m.messageId == msg.messageId);
                    xListView.RefreshItem(index);
                    RollBottom_HaveEmpty();
                }));
            }

            //接收到通知代表别人已经发了
            if (isSendRecall)
            {
                //群主撤回消息
                bool isOwnerRecall = ChooseTarget.IsGroup == 1 && JudgeIsGroupOwner(Applicate.MyAccount.userId);
                //自己撤回的消息
                if (msg.fromUserId.Equals(Applicate.MyAccount.userId) || isOwnerRecall)
                {
                    //调用接口
                    HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "tigase/deleteMsg"). //删除消息
                        AddParams("access_token", Applicate.Access_Token).
                        AddParams("type", msg.isGroup == 0 ? "1" : "2").    //1 单聊  2 群聊
                        AddParams("delete", "2").   //1： 删除属于自己的消息记录 2：撤回 删除整条消息记录
                        AddParams("messageId", msg.messageId).
                        AddParams("roomJid", msg.toUserId).
                        Build().ExecuteJson<object>((sccess, obj) =>   //返回值说明： text：加密后的内容
                        {
                            //删除成功
                            if (sccess)
                            {
                                ShiKuManager.SendRecallMessage(msg.GetFriend(), msg);
                                //int result = messageObject.UpdateData();
                                HttpUtils.Instance.ShowTip("撤回成功");
                            }
                        });
                }
            }
        }

        #endregion

        #region 复制
        private void MenuItem_Copy_Click(object sender, EventArgs e)
        {
            if (selectControl == null || selectControl.Name.IndexOf("talk_panel_") < 0)
                return;

            MessageObject msg = GetMsgBySelectCrl();
            if (msg == null)
                return;
            switch (msg.type)
            {
                case kWCMessageType.Image:
                    //清空剪切板，防止里面之前有内容
                    Clipboard.Clear();
                    //给剪切板设置图片对象
                    PictureBox picBox = crl_content as PictureBox;
                    if (FileUtils.IsGif(msg.content))
                    {
                        string fileName = FileUtils.GetFileName(msg.content);
                        string filePath = Applicate.LocalConfigData.ImageFolderPath + fileName;
                        if (File.Exists(filePath))
                        {
                            StringCollection strcoll = new StringCollection
                            {
                                filePath
                            };
                            Clipboard.SetFileDropList(strcoll);
                        }
                    }
                    else
                    {
                        Image image = picBox.BackgroundImage;
                        //string msgId = picBox.Tag != null ? picBox.Tag.ToString() : "";
                        //if (!string.IsNullOrEmpty(msgId))
                        //{
                        //    MessageObject image_msg = msgTabAdapter.TargetMsgData.GetMsg(msgId);
                        //if (BitmapUtils.IsNull(image))
                        //{
                        //string filePath = Applicate.LocalConfigData.ImageFolderPath + msgId + ".png";
                        string fileName = FileUtils.GetFileName(msg.content);
                        string filePath = Applicate.LocalConfigData.ImageFolderPath + fileName;

                        if (File.Exists(filePath))
                        {
                            image = Image.FromFile(filePath);
                        }
                        //}
                        //}
                        //Bitmap bitmap = new Bitmap(image);
                        Clipboard.Clear();
                        Clipboard.SetDataObject(image, true, 3, 200);
                    }
                    break;
                case kWCMessageType.Text:
                    //清空剪切板，防止里面之前有内容
                    Clipboard.Clear();
                    //给剪切板设置图片对象
                    if (crl_content is RichTextBoxEx richTextBox)
                    {
                        string content = "";
                        if (string.IsNullOrEmpty(richTextBox.SelectedRtf))
                            content = richTextBox.Tag.ToString().Trim();
                        else
                        {
                            bool isOneself = msg.FromId.Equals(Applicate.MyAccount.userId);
                            string selected_rtf = richTextBox.SelectedRtf;
                            selected_rtf = selected_rtf.Replace("{\\*\\picprop}", "");
                            foreach (string item in EmojiCodeDictionary.GetEmojiDataIsMine().Keys)
                            {
                                string emoji_code = "[" + item + "]";
                                selected_rtf = selected_rtf.Replace(EmojiCodeDictionary.GetEmojiRtfByCode(item, isOneself), emoji_code);
                            }
                            using (RichTextBox ri = new RichTextBox())
                            {
                                ri.Rtf = selected_rtf;
                                content = ri.Text;
                            }
                        }
                        Clipboard.SetText(content);
                    }
                    break;
                case kWCMessageType.Reply:
                    //清空剪切板，防止里面之前有内容
                    Clipboard.Clear();
                    //给剪切板设置图片对象
                    //RichTextBox richTextBox = crl_content as RichTextBox;
                    //string content = richTextBox.Tag.ToString();
                    Clipboard.SetText(msg.content);
                    break;
                case kWCMessageType.File:
                    //先判断本地是否存在（自己发的消息）
                    string localPath = msg.fileName;
                    if (!File.Exists(localPath))
                    {
                        localPath = Applicate.LocalConfigData.FileFolderPath + FileUtils.GetFileName(msg.fileName);
                        if (!File.Exists(localPath))
                        {
                            DownloadFile(msg, localPath, true);
                        }
                    }
                    else
                    {
                        StringCollection strcoll = new StringCollection
                        {
                            localPath
                        };
                        Clipboard.SetFileDropList(strcoll);
                    }
                    break;
                case kWCMessageType.Video:
                    string videoPath = msg.fileName;
                    if (!File.Exists(videoPath))
                    {
                        videoPath = Applicate.LocalConfigData.VideoFolderPath + FileUtils.GetFileName(msg.content);
                        if (!File.Exists(videoPath))
                        {
                            HttpUtils.Instance.ShowTip("视频文件不存在，需要重新下载！");
                            DownloadEngine.Instance.DownUrl(msg.content).Down((path) =>
                            {
                                StringCollection strcoll = new StringCollection();
                                strcoll.Add(path);
                                Clipboard.Clear();
                                Clipboard.SetFileDropList(strcoll);
                                HttpUtils.Instance.ShowTip("复制视频成功！");
                            });
                        }
                    }
                    StringCollection v_strcoll = new StringCollection();
                    v_strcoll.Add(videoPath);
                    Clipboard.Clear();
                    Clipboard.SetFileDropList(v_strcoll);
                    HttpUtils.Instance.ShowTip("复制视频成功！");
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region 转发
        private void MenuItem_Relay_Click(object sender, EventArgs e)
        {
            //获取选择的Message
            if (selectControl == null || selectControl.Name.IndexOf("talk_panel_") < 0)
                return;
            MessageObject messageObject = GetMsgBySelectCrl();
            if (messageObject == null)
                return;

            MsgUtils.FowardMsgToFd(messageObject);
        }
        #endregion

        #region 多选
        /// <summary>
        /// 开启多选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_MultiSelect_Click(object sender, EventArgs e)
        {
            xListView.SuspendLayout();
            try
            {
                //msgTabAdapter.TargetMsgData.isMultiSelect = true;
                foreach (Control crl in xListView.panel1.Controls)
                {
                    foreach (Control item in crl.Controls)
                    {
                        if (item is CheckBoxEx checkBox)
                        {

                            checkBox.Visible = true;
                            checkBox.Checked = false;
                        }
                        else if (item is EQBaseControl talk_panel && !talk_panel.isOneSelf)
                        {
                            if (talk_panel is EQRemindControl)
                                continue;
                            talk_panel.Location = new Point(talk_panel.Location.X + 22, talk_panel.Location.Y);
                        }
                        else if (item is EQShowInfoPanelAlpha panelAlpha)
                        {
                            panelAlpha.Visible = true;
                            panelAlpha.BringToFront();
                        }
                    }
                }
            }
            catch (Exception ex) { LogUtils.Error(ex.Message); }
            xListView.ResumeLayout();
            ////清空之前被多选的集合
            multiSelectPanel.List_Msgs = new List<MessageObject>();
            multiSelectPanel.FdTalking = ChooseTarget;
            //展示多选
            //IsShowPanelMultiSelect(true);
            SetDialogBoxSize(DialogBox.MultiSelect);

        }

        /// <summary>
        /// 展示多选面板
        /// </summary>
        /// <param name="visible"></param>
        private void IsShowPanelMultiSelect(bool visible)
        {
            this.SuspendLayout();
            try
            {
                panMultiSelect.Visible = visible;
                Bottom_Panel.Visible = !visible;
                if (!visible)
                {
                    serviceListCrl.isShowMultiSelect = false;   //设置不显示多选面板
                    panMultiSelect.Dock = DockStyle.None;
                    Bottom_Panel.Dock = DockStyle.Bottom;
                    Bottom_Panel.Visible = true;
                    //multiSelectPanel.Visible = false;
                    panMultiSelect.SendToBack();
                }
                else
                {
                    //快捷栏转多选
                    if (serviceListCrl.Visible)
                    {
                        //serviceListCrl.isShowMultiSelect = true;
                        serviceListCrl.SetList(ChooseTarget, this);
                    }

                    panMultiSelect.Dock = DockStyle.Bottom;
                    Bottom_Panel.Dock = DockStyle.None;
                    Bottom_Panel.Visible = false;
                    //multiSelectPanel.Visible = true;
                    panMultiSelect.BringToFront();
                }
            }
            catch (Exception ex) { LogHelper.log.Error("-------------展示多选面板出错，方法（IsShowPanelMultiSelect）: \r\n" + ex.Message); }
            this.ResumeLayout();
        }

        #region 关闭多选面板
        private void LblClose_Click(object sender, EventArgs e)
        {
            //msgTabAdapter.TargetMsgData.isMultiSelect = false;
            foreach (Control crl in xListView.panel1.Controls)
            {
                foreach (Control item in crl.Controls)
                {
                    if (item is CheckBoxEx checkBox)
                        checkBox.Visible = false;
                    else if (item is EQBaseControl talk_panel && !talk_panel.isOneSelf)
                        talk_panel.Location = new Point(talk_panel.Location.X - 22, talk_panel.Location.Y);
                }
                foreach (Control item in crl.Controls)
                    if (item is EQShowInfoPanelAlpha panelAlpha)
                    {
                        panelAlpha.Visible = false;
                        panelAlpha.SendToBack();
                        break;
                    }
            }
            //IsShowPanelMultiSelect(false);
            SetDialogBoxSize(DialogBox.Normal);
        }
        #endregion

        #region 批量删除
        private void BatchDeleteMsg(List<MessageObject> list_msgs)
        {
            if (list_msgs == null || list_msgs.Count < 1)
                return;

            xListView.panel1.SuspendLayout();
            try
            {
                MessageObject lastMsg = msgTabAdapter.TargetMsgData.GetLastIndexMsg();
                bool isLastMsg = false;     //记录是否含有最后一条消息在内
                foreach (MessageObject msg in list_msgs)
                {
                    //如果是最后一条消息，需要通知最近聊天列表更新最后一条消息
                    if (msg != null && lastMsg != null && lastMsg.rowIndex == msg.rowIndex)
                    {
                        isLastMsg = true;
                        lastMsgTime = msg.timeSend;     //更新列表最后一条消息的时间
                    }

                    //从界面中移除
                    int index = msgTabAdapter.msgList.FindIndex(m => m.messageId == msg.messageId);
                    msgTabAdapter.RemoveData(index);
                }
                //调用接口删除数据
                CollectUtils.DelServerMessages(list_msgs, isLastMsg);

                //滚动到底部
                RollBottom_HaveEmpty();
            }
            catch (Exception) { }
            xListView.panel1.ResumeLayout();
        }
        #endregion
        #endregion

        #region 收藏
        private void MenuItem_Collect_Click(object sender, EventArgs e)
        {
            //获取选择的Message
            if (selectControl == null || selectControl.Name.IndexOf("talk_panel_") < 0)
                return;
            string messageId = selectControl.Name.Replace("talk_panel_", "");
            MessageObject messageObject = msgTabAdapter.TargetMsgData.GetMsg(messageId);

            CollectUtils.CollectMessage(messageObject);
        }
        #endregion

        #region 回复
        private void MenuItem_Reply_Click(object sender, EventArgs e)
        {
            MessageObject msg = GetMsgBySelectCrl();
            replyPanel.lblContent.Text = EQControlManager.ChangeContentByType(msg);
            replyPanel.ReplyMsg = msg;
            replyPanel.Location = new Point(0, Bottom_Panel.Location.Y - replyPanel.Height);
            replyPanel.Visible = true;
            replyPanel.BringToFront();
        }
        #endregion

        #region 打开文件夹
        private void MenuItem_OpenFileFolder_Click(object sender, EventArgs e)
        {
            bool isExists = false;
            MessageObject msg = GetMsgBySelectCrl();
            string filePath = EQControlManager.GetFilePathByType(msg, out isExists);
            //文件不存在
            if (!isExists)
            {
                HttpUtils.Instance.ShowTip("文件不存在");
                return;
            }
            System.Diagnostics.Process.Start("explorer.exe", "/select," + filePath);
        }
        #endregion

        #region 另存为
        private void MenuItem_SaveAs_Click(object sender, EventArgs e)
        {
            MessageObject msg = GetMsgBySelectCrl();

            string filename = FileUtils.GetFileName(msg.fileName);

            //选择文件夹路径
            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "另存为..",
                FileName = filename,
                Filter = @"所有文件|*.*"
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                //文件的本地路径
                string localPath = dialog.FileName;
                if (string.IsNullOrEmpty(localPath))
                    return;

                //下载文件
                if (msg.type == kWCMessageType.File)
                {
                    //localPath += "\\" + FileUtils.GetFileName(msg.fileName);
                    DownloadFile(msg, localPath);
                }
                else
                {
                    //localPath += "\\" + FileUtils.GetFileName(msg.content);
                    DownloadEngine.Instance.DownUrl(msg.content).SavePath(localPath)
                        .Down((path) =>
                        {
                            HttpUtils.Instance.ShowTip("另存为成功:" + path);
                        });
                }
            }

        }
        #endregion

        #region 下载
        private void MenuItem_Download_Click(object sender, EventArgs e)
        {
            MessageObject msg = GetMsgBySelectCrl();
            if (msg == null)
                return;
            if (msg.type == kWCMessageType.File)
            {
                //对比文件是否已下载过
                string sname = FileUtils.GetFileName(msg.content);
                //文件的本地路径
                string localPath = Applicate.LocalConfigData.FileFolderPath + FileUtils.GetFileName(msg.fileName);
                var r = new Random(Guid.NewGuid().GetHashCode());//产生不重复的随机数
                string filename = FileUtils.GetFileName(msg.fileName);
                string suffix = FileUtils.GetFileExtension(msg.fileName);//取出后缀
                string filename1 = filename.Replace(suffix, "") + "(" + r.Next(0, 1000) + ")" + suffix;//合成名称
                localPath = Applicate.LocalConfigData.FileFolderPath + filename1;//重新赋值
                msg.signature = filename;
                msg.UpdateSignature();
                msg.fileName = localPath;
                msg.objectId = sname;
                msg.UpdateObjAndFileN();

                DownloadFile(msg, localPath);
            }
        }
        #endregion

        #region 存表情
        private void MenuItem_SaveCustomize_Click(object sender, EventArgs e)
        {
            MessageObject msg = GetMsgBySelectCrl();
            if (msg == null)
                return;
            // http收藏表情
            CollectUtils.CollectExpression(msg);
        }
        #endregion

        #region 录制
        private int isStartTrans = 0;       //是否开始录制
        private double startTransTime = 0;    //开始录制的时间
        private double endTransTime = 0;      //结束录制的时间
        private void MenuItem_Transcribe_Click(object sender, EventArgs e)
        {
            // 修改禅道8365
            if (ChooseTarget.IsGroup == 1 && ChooseTarget.IsSecretGroup == 1)
            {
                HttpUtils.Instance.ShowTip("私密群组禁止录制课程");
                return;
            }

            isStartTrans = isStartTrans == 1 ? 0 : 1;
            menuItem_Transcribe.Text = isStartTrans == 1 ? 
                LanguageXmlUtils.GetValue("stop_transcribe", "停止录制") :
                LanguageXmlUtils.GetValue("start_transcribe", "开始录制");

            //开始录制
            if (isStartTrans == 1)
            {
                MessageObject selectMsg = GetMsgBySelectCrl();
                startTransTime = selectMsg.timeSend;
            }
            //结束录制
            else
            {
                MessageObject selectMsg = GetMsgBySelectCrl();
                endTransTime = selectMsg.timeSend;
                FrmMyColleagueEidt frmMyColleagueEidt = new FrmMyColleagueEidt();

                frmMyColleagueEidt.ColleagueName((data) =>
                {
                    frmMyColleagueEidt.Close();
                    HttpUtils.Instance.PopView(frmMyColleagueEidt);
                    TeachUlUtils.CourseMade(data, startTransTime, endTransTime, msgTabAdapter.TargetMsgData);

                });

                string title = LanguageXmlUtils.GetValue("title_record_courseware", "课件录制");
                string name = LanguageXmlUtils.GetValue("name_courseware_name", "课件名称", true);
                frmMyColleagueEidt.ShowThis(title, name);
            }
        }

        /// <summary>
        /// 取消录制
        /// </summary>
        private void ClearTranscribe()
        {
            isStartTrans = 0;
            startTransTime = 0;    //开始录制的时间
            endTransTime = 0;      //结束录制的时间
            menuItem_Transcribe.Text = LanguageXmlUtils.GetValue("start_transcribe", "开始录制");
        }
        #endregion

        #region 鼠标右键菜单
        //不同的控件鼠标点击事件也不同
        private void Content_MouseDown(object sender, MouseEventArgs e)
        {
            crl_content = sender as Control;
            string crl_type = crl_content.GetType().Name;

            if (e.Button == MouseButtons.Right)
            {
                //记录被选中的控件
                selectControl = crl_content.Parent;
                while (true)
                {
                    if (selectControl.Name.IndexOf("talk_panel") > -1)
                        break;
                    else
                        selectControl = selectControl.Parent;
                }

                //如果是文件消息类型，下载中没有右键菜单
                if (selectControl is EQFileControl fileControl)
                {
                    if (fileControl.isDownloading)
                    {
                        foreach (ToolStripItem item in cmsMsgMenu.Items)
                            item.Visible = false;
                        return;
                    }
                }


                MessageObject msg = msgTabAdapter.TargetMsgData.GetMsg(selectControl.Name.Replace("talk_panel_", ""));
                //获得绑定数据的集合的索引
                rowIndex = msgTabAdapter.msgList.FindIndex(m => m == msg);
                //rowIndex = msg.rowIndex;    //记录被选中行
                //判断是否为本人
                bool isOneself = msg.fromUserId != Applicate.MyAccount.userId ? false : true;
                //设置右键菜单的可见度
                if (msg != null)
                    new KWTypeMenuStripDictionary().SettingMenuStripVisible(ref cmsMsgMenu, msg.type, isOneself, msg.isReadDel == 1);
                //如果自己为群主或管理员，则可以撤回所有信息(红包和转账不能被撤回)
                if (ChooseTarget.IsGroup == 1 && JudgeIsAdmin(Applicate.MyAccount.userId) && msg.type != kWCMessageType.RedPacket && msg.type != kWCMessageType.TRANSFER)
                    cmsMsgMenu.Items["menuItem_Recall"].Visible = true;
                //除了阅后即焚，所有自己的消息都可以进行录制（只能往后录制）
                if (msg.fromUserId == Applicate.MyAccount.userId && msg.isReadDel == 0)
                {
                    if ((isStartTrans == 1 && startTransTime <= msg.timeSend) || isStartTrans == 0)
                    {
                        menuItem_Transcribe.Visible = true;
                        separator_one.Visible = true;
                    }
                }
                else
                {
                    menuItem_Transcribe.Visible = false;
                    separator_one.Visible = false;
                }


                //如果正在录音，则不能进行回复
                menuItem_Reply.Enabled = userSoundRecording.SoundState ? false : true;
                //如果文件不存在，则不显示打开文件夹，而是显示下载
                if (msg.type == kWCMessageType.File)
                {
                    bool isExist = false;
                    string filePath = EQControlManager.GetFilePathByType(msg, out isExist);
                    //文件不存在
                    if (!isExist)
                    {
                        menuItem_OpenFileFolder.Visible = false;
                        menuItem_Download.Visible = true;
                    }
                    else
                    {
                        menuItem_OpenFileFolder.Visible = true;
                        menuItem_Download.Visible = false;
                    }
                }
                //如果是发送中或者发送失败的消息，不存在撤回
                if (msg.isSend != 1)
                {
                    menuItem_Recall.Visible = false;
                }
                cmsMsgMenu.RightToLeft = RightToLeft.No;
            }
        }
        #endregion

        #region 给所有子控件设置右键菜单事件
        private void AddContentMouseDownToCrl(Control crl)
        {
            foreach (Control child_crl in crl.Controls)
            {
                child_crl.MouseDown += Content_MouseDown;
                child_crl.ContextMenuStrip = cmsMsgMenu;
                if (child_crl.Controls.Count > 0)
                    AddContentMouseDownToCrl(child_crl);
            }
        }
        #endregion

        #endregion

        #region Set Title
        #region 显示正在输入
        private void IsShowTyping()
        {
            if (this.IsHandleCreated)
            {
                Invoke(new Action(() =>
                {
                    labName.Text = "对方正在输入...";
                }));
            }

            if (this.IsHandleCreated)
            {
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(15000);
                    if (this.IsHandleCreated)
                    {
                Invoke(new Action(() =>
                {
                    labName.Text = ChooseTarget.GetRemarkName();
                }));
                    }
            });
        }
        }
        #endregion

        #region 收到通知修改聊天对象名字
        private void ModifyFriendName(Friend friend)
        {
            string newName = friend.GetRemarkName();
            //单聊并且为当前聊天对象
            if (friend.UserId == ChooseTarget.UserId && friend.IsGroup == 0)
            {
                labName.Text = newName;
                //是否在线
                SetOnlineState();
            }
            //修改字典
            if (Applicate.FdNames.ContainsKey(friend.UserId))
                Applicate.FdNames[friend.UserId] = newName;
        }
        #endregion
        #endregion

        #region Upload File

        #region 重新上传图片
        private void ResumeUploadImageMsg(MessageObject msg)
        {
            string filePath = msg.fileName;
            if (!File.Exists(filePath))
                return;

            UploadEngine.Instance.From(filePath).
                //上传中
                UpProgress((progress) =>
                {

                }).
                //上传完成
                UploadFile((success, url) =>
                {
                    msg = msgTabAdapter.TargetMsgData.GetMsg(msg.messageId);
                    EQBaseControl talk_panel = GetTalkPanelByMsg(msg.messageId);
                    //更新UI
                    if (talk_panel != null && talk_panel is EQImageControl imageCrl)
                        UploadImage(imageCrl, msg, url, success);
                });
        }
        #endregion

        #region 重新上传视频
        private void ResumeUploadVideoMsg(MessageObject msg)
        {
            string filePath = msg.fileName;
            if (!File.Exists(filePath))
                return;

            UploadEngine.Instance.From(filePath).
                //上传完成
                UploadFile((success, url) =>
                {
                    msg = msgTabAdapter.TargetMsgData.GetMsg(msg.messageId);
                    EQBaseControl talk_panel = GetTalkPanelByMsg(msg.messageId);
                    //更新UI
                    if (talk_panel != null && talk_panel is EQVideoControl videoCrl)
                        UploadVideo(videoCrl, msg, url, success);
                });
        }
        #endregion
        #endregion

        #region 获取身份
        #region 判断是否为群主或者管理员
        private bool JudgeIsAdmin(string userId)
        {
            if (ChooseTarget.IsGroup == 1)
            {
                int role = GetRoomMemberRole(userId);
                if (role == 1 || role == 2)
                    return true;
            }
            return false;
        }
        #endregion

        #region 判断是否为群主
        private bool JudgeIsGroupOwner(string userId)
        {
            if (ChooseTarget.IsGroup == 1)
            {
                int role = GetRoomMemberRole(userId);
                if (role == 1)
                    return true;
            }
            return false;
        }
        #endregion

        #region 判断是否为隐身人
        private bool JudgeIsUnseen(string userId)
        {
            if (ChooseTarget.IsGroup == 1)
            {
                int role = GetRoomMemberRole(userId);
                if (role == 4)
                    return true;
            }
            return false;
        }

        private int GetRoomMemberRole(string userId)
        {
            if (ChooseTarget.IsGroup == 1)
            {
                int role = new RoomMember() { roomId = ChooseTarget.RoomId, userId = userId }.GetRoleByUserId();
                return role;
            }

            return 0;
        }
        #endregion

        #endregion

        #region 判断是否添加气泡到列表
        private void JudgeMsgIsAddToPanel(MessageObject msg)
        {
            #region 过滤消息
            if (ChooseTarget == null || msg == null || string.IsNullOrEmpty(msg.messageId))
                return;
            //正在输入。。通知
            if (msg.type == kWCMessageType.Typing)
            {
                if (msg.fromUserId == ChooseTarget.UserId)
                    IsShowTyping();
                return;
            }
            //不生成气泡的消息类型
            if (!msg.IsVisibleMsg() && msg.type != kWCMessageType.RoomIsVerify)
                return;
            //已存在该气泡控件
            if (xListView.panel1.Controls["talk_panel_" + msg.messageId] != null)
                return;
            //content为空的消息不显示
            if (string.IsNullOrEmpty(msg.content))
            {
                switch (msg.type)
                {
                    case kWCMessageType.Text:
                        msg.content = " ";
                        break;
                    case kWCMessageType.Image:
                        break;
                    case kWCMessageType.File:
                        break;
                    case kWCMessageType.Video:
                        break;
                    case kWCMessageType.Voice:
                        break;
                    default:
                        return;
                }
            }

            #region 判断消息是否为当前聊天对象的
            //多点登录的FromId也是自己，需要判断ToId是否为当前对象
            if (msg.FromId == Applicate.MyAccount.userId)
            {
                if (msg.ToId != ChooseTarget.UserId)
                    return;
            }
            //FromId和ToId必须是自己和当前聊天对象，FromId或者ToId为自己时，另一个必定为聊天对象
            else if (msg.FromId == ChooseTarget.UserId)
            {
                if (msg.ToId != Applicate.MyAccount.userId)
                    return;
            }
            //FromId既不是自己也不是聊天对象，该消息必定不为该聊天页面
            else
                return;
            #endregion

            //过期消息
            if (msg.deleteTime < TimeUtils.CurrentTimeDouble() && msg.deleteTime > 0)
            {
                msg.DeleteData();
                return;
            }
            #endregion

            //获取滚动条的位置
            int progress = xListView.Progress;
            //是否在底部
            //if (progress == 100 || xListView.Height >= xListView.panel1.Height)
            //    isInsert = true;
            bool isInsert = JudgeIsInsert(msg);
            if (this.IsHandleCreated)
                Invoke(new Action(() =>
                {
                    try
                    {
                        //添加消息气泡
                        msgTabAdapter.TargetMsgData.AddMsgData(msg);
                        int index = msgTabAdapter.msgList.FindIndex(m => m.messageId == msg.messageId);
                        //msgTabAdapter.msgList.Insert(end, msg);
                        xListView.InsertItem(index, isInsert);
                        //更新未读数量

                        if (msgTabAdapter.msgList.Count > 2)
                            LastReadMsgId = string.IsNullOrEmpty(LastReadMsgId) ? msgTabAdapter.msgList[msgTabAdapter.msgList.Count - 2].messageId : LastReadMsgId;

                        //如果是自己发送的消息，或者当前滚动到了底部，则调用滚动到底部的方法
                        if (msg.fromUserId == Applicate.MyAccount.userId || progress == 100)
                        {
                            UnReadMsgNum = 0;
                            int end = msgTabAdapter.msgList.Count - 1;
                            //bool isFillCrl = Applicate.GetWindow<FrmMain>().WindowState != FormWindowState.Minimized;
                            xListView.ShowRangeEnd(end, 0, true);
                        }
                        //显示未读消息数量
                        else
                        {
                            UnReadMsgNum++;

                            if (progress == 0 && xListView.panel1.Height < xListView.Height)
                            {
                                UnReadMsgNum = 0;
                            }

                            if (UnReadMsgNum > 0 && !string.IsNullOrEmpty(LastReadMsgId))
                            {

                                RefreshUnreadNumUi(LastReadMsgId, UnReadMsgNum, true, false);
                            }
                        }
                        //如果数据已经加载了400条，继续滚动会超过最大值
                        if (msgTabAdapter.msgList.Count > 300)
                        {
                            ////如果第一行是显示更多聊天记录
                            //if (msgTabAdapter.FirstMsgIndex > 0)
                            //{
                            //    msgTabAdapter.msgList[0].content = "更多消息请在消息记录中查阅";
                            //    xListView.RefreshItem(0);
                            //}

                            ////清除一部分数据
                            //ClearTopMsg();

                            //重新加载数据
                            AfreshBindData();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.log.Error("----添加气泡出错，方法（JudgeMsgIsAddToPanel） : " + ex.Message, ex);
                    }
                }));
        }
        #endregion

        private bool JudgeIsInsert(MessageObject msg)
        {
            if (xListView.Height >= xListView.panel1.Height)
                return true;

            //1.判断消息是否是对方发送的
            //2.判断滚动条的位置决定是否在底部
            //3.位置不在底部返回false，默认为true
            int progress = xListView.Progress;      //获取滚动条的位置
            if (!msg.IsMySend())
            {
                if (progress != 100 && Math.Abs(xListView.panel1.Location.Y) != xListView.panel1.Height - xListView.Height)
                    return false;
            }
            return true;
        }


        public void RefreshUnreadNumUi(string msgId, int readNum, bool isShow, bool isUp)
        {
            if (unReadNumPanel != null)
            {
                unReadNumPanel.IsShowPanel(msgId, readNum, isShow, isUp);
                if (isShow)
                {
                    int height = titleManager.MeasureHeight();
                    int width = xListView.Width - unReadNumPanel.Width - 10;
                    unReadNumPanel.Location = new Point(width, height + 40);
                }
            }
        }

        #region 所有需要调用接口的方法

        #region 下载文件
        private void DownloadFile(MessageObject msg, string localPath, bool isCopy = false)
        {
            var result = selectControl.Controls.Find("image_panel", true);
            if (result.Length > 0 && result[0] is Panel image_panel)
            {
                result = image_panel.Controls.Find("panel_file", true);
                if (image_panel.Controls[0] is FilePanelLeft panel_file)
                {
                    #region download file
                    //正在下载
                    if (panel_file.lab_lineLime.Width > 0)
                        return;

                    //开始下载
                    panel_file.isDownloading = true;
                    //下载文件
                    DownloadEngine.Instance.DownUrl(msg.content, msg.messageId)
                    .DownProgress((progress) =>
                    {
                        panel_file.lab_lineLime.BringToFront();
                        panel_file.lab_lineLime.Width = Convert.ToInt32(panel_file.lab_lineSilver.Width * ((decimal)progress / 100));
                        if ((progress / 100) == 1)
                            panel_file.lab_lineLime.Width = 0;
                    }).
                    DownSpeed((speed) =>
                    {
                        panel_file.lblSpeed.Visible = true;
                        panel_file.lblSpeed.BringToFront();
                        panel_file.lblSpeed.Text = speed + @"/s";
                    })
                    .SavePath(localPath)    //保存路径
                    .Down((path) =>
                    {
                        panel_file.lblSpeed.Visible = false;
                        //下载完成
                        panel_file.isDownloading = false;

                        if (string.IsNullOrEmpty(path))
                            return;

                        if (isCopy)
                        {
                            StringCollection strcoll = new StringCollection
                            {
                                localPath
                            };
                            Clipboard.SetFileDropList(strcoll);
                        }

                        HttpUtils.Instance.ShowTip("下载完成：" + path);
                    });
                    #endregion
                }
            }
        }
        #endregion

        #region 获取群设置并设置群人数和群公告
        private int userSize = 0;
        private void GetRoomSetting()
        {
            if (ChooseTarget.UserType > FriendType.USER_TYPE || ChooseTarget.Status != 2 || ChooseTarget == null)
                return;

            if (UIUtils.IsNull(ChooseTarget.RoomId))
            {

                Console.WriteLine("群聊没有 roomID" + ChooseTarget);

                return;
            }

            //防止因为接口异步导致名称不对
            //string name = ChooseTarget.NickName;
            //避免切换聊天对象
            string userId = ChooseTarget.UserId;
            //http get请求获得数据
            HttpUtils.Instance.InitHttp(this);
            //将数据保存
            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/getRoom") //获取群详情
                .AddParams("access_token", Applicate.Access_Token)
                .AddParams("roomId", ChooseTarget.RoomId)
                .Build()
                .BindTag(ChooseTarget.UserId + "roomget")
                .Execute((success, result) =>
                {
                    //防止因为接口异步导致聊天对象不对
                    if (!userId.Equals(ChooseTarget.UserId))
                        return;
                    Friend choose_fd = new Friend() { UserId = userId }.GetFdByUserId();
                    if (ChooseTarget == null || choose_fd.IsGroup == 0 /*|| choose_fd.IsDismiss == 1*/)     //单次获取接口失败，下次还是会去获取
                        return;


                    if (success && choose_fd != null)
                    {
                        string requestUserId = UIUtils.DecodeString(result, "jid");
                        if (ChooseTarget == null || !requestUserId.Equals(ChooseTarget.UserId))
                        {
                            return;
                        }

                        Thread getRoom_thread = new Thread(() =>
                        {
                            userSize = UIUtils.DecodeInt(result, "userSize");


                            int chatkeyState = choose_fd.IsLostKeyGroup;
                            int showread = choose_fd.ShowRead;

                            // 是否锁定的群组
                            int dismiss = UIUtils.DecodeInt(result, "s");
                            if (dismiss != 1)
                            {
                                // 非等于1就是锁定的群组
                                ChooseTarget.IsDismiss = 2;
                            }
                            else
                            {
                                choose_fd.IsDismiss = 0;
                                ChooseTarget.IsDismiss = 0;
                            }
                            ChooseTarget.UpdateDismiss(ChooseTarget.IsDismiss);

                            // 加密类型
                            int entype = UIUtils.DecodeInt(result, "encryptType");
                            choose_fd.UpdateEncrypt(entype);


                            // 解析数据
                            choose_fd.TransToMember(result);
                            //更新群设置（修改吵到新建的群默认开启没有更新开启已读）
                            ChooseTarget.ShowRead = choose_fd.ShowRead;
                            ChooseTarget.ShowMember = choose_fd.ShowMember;

                            // 修改禅道bug5138
                            if (showread != choose_fd.ShowRead)
                            {
                                choose_fd.UpdateShowRead();

                                if (msgTabAdapter.msgList != null && choose_fd.ShowRead == 1)
                                {
                                    foreach (var item in msgTabAdapter.msgList)
                                    {
                                        ShiKuManager.SendReadMessage(choose_fd, item, 0);
                                    }
                                }
                            }

                            if (choose_fd.AllBannedTalk != ChooseTarget.AllBannedTalk)
                            {
                                ChooseTarget.AllBannedTalk = choose_fd.AllBannedTalk;
                            }

                            // 修改禅道8534
                            if (choose_fd.IsSecretGroup == 1)
                            {
                                ChooseTarget.IsLostKeyGroup = choose_fd.IsLostKeyGroup;

                                // 私密群组
                                if (chatkeyState == 0 && choose_fd.IsLostKeyGroup == 1)
                                {
                                    // 密钥丢失状态  需要去请求密钥
                                    if (Applicate.ENABLE_ASY_ENCRYPT)
                                    {
                                        ShiKuManager.SendRequstChatKeyMessage(choose_fd);
                                    }
                                }

                                requestKeyPanel.IsShowView(ChooseTarget.UserId, choose_fd.IsLostKeyGroup == 1);
                            }

                            ////等待气泡加载，减少主线程拥堵
                            //Thread.Sleep(3000);


                            //是否显示群公告
                            //bool isShowNotice = LocalDataUtils.GetBoolData(choose_fd.UserId + "show_notice");
                            string json_notice = UIUtils.DecodeString(result, "notice");

                            string room_urls = UIUtils.DecodeString(result, "roomTitleUrl");

                            if (IsHandleCreated)
                                Applicate.SyncMainContext.Send((obj) =>
                                {
                                    if (ChooseTarget == null || choose_fd.IsGroup == 0 || !choose_fd.UserId.Equals(ChooseTarget.UserId) 
                                        || choose_fd.IsDismiss == 1 || this.IsDisposed || showMsgPanel_Tool.IsDisposed)
                                        return;
                                    #region 更新UI

                                    try
                                    {
                                        // 标题（群名称+人数）
                                        var unit = LanguageXmlUtils.GetValue("people", "人");
                                        var name = EQControlManager.StrAddEllipsis(ChooseTarget.GetRemarkName(), labName.Font, 240);
                                        labName.Text = string.Format("{0}({1}{2})", name, userSize, unit);

                                        if (Applicate.ENABLE_MEET && Applicate.CURRET_VERSION > 4.0f)
                                        {
                                            //关闭群会议，普通成员不允许点击视频和音频
                                            showMsgPanel_Tool.lblAudio.Visible = choose_fd.AllowConference == 0 && !JudgeIsAdmin(Applicate.MyAccount.userId) ? false : true;
                                            showMsgPanel_Tool.lblVideo.Visible = choose_fd.AllowConference == 0 && !JudgeIsAdmin(Applicate.MyAccount.userId) ? false : true;
                                        }
                                        else
                                        {
                                            showMsgPanel_Tool.lblAudio.Visible = false;
                                            showMsgPanel_Tool.lblVideo.Visible = false;
                                        }

                                        // 刷新群内直播按钮
                                        //if (ChooseTarget != null && string.Equals(choose_fd.UserId, ChooseTarget.UserId))
                                        //{
                                        //    // 刷新群组标题
                                        //    if (choose_fd.IsDismiss == 0)
                                        //    {
                                        //        titleManager.DecodeJson(json_notice, room_urls, liveUserName, liveStatus);

                                        //        if (unReadNumPanel.Visible)
                                        //        {
                                        //            int height = titleManager.MeasureHeight();
                                        //            int width = xListView.Width - unReadNumPanel.Width - 10;
                                        //            unReadNumPanel.Location = new Point(width, height + 40);
                                        //        }
                                        //    }

                                        //    showMsgPanel_Tool.SetLblRoomLiveVisible(choose_fd.AllowOpenLive);
                                        //}


                                        // 刷新发送消息面板
                                        RefreshSendBoxState(ChooseTarget);
                                    }
                                    catch(Exception ex)
                                    {
                                        LogUtils.Error(ex.Message);
                                    }
                                    #endregion
                                }, null);
                        })
                        {
                            IsBackground = true
                        };
                        getRoom_thread.Start();
                    }
                    else
                    {
                        choose_fd.IsDismiss = 1;
                        choose_fd.UpdateDismiss(1);
                        // 刷新发送消息面板
                        RefreshSendBoxState(choose_fd);
                    }
                });
        }


        #endregion

        #region 获取好友是否在线
        private void SetOnlineState()
        {
            if (!isOpenOnlineStatus)
                return;

            if (ChooseTarget.IsDevice())
            {
                bool isOnline = MultiDeviceManager.Instance.IsDeviceLine(ChooseTarget.UserId);
                string userName = ChooseTarget.GetRemarkName();
                labName.Text = userName + (isOnline ? "（在线）" : "（离线）");
                return;
            }

            string userId = ChooseTarget.UserId;
            if (userId.Length > 5 && userId.Length < 12)
            {
                //http get请求获得数据
                HttpUtils.Instance.InitHttp(this);
                HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "user/getOnLine") //获取群详情
                    .AddParams("access_token", Applicate.Access_Token)
                    .AddParams("userId", userId)
                    .Build()
                    .BindTag(ChooseTarget.UserId)
                    .Execute((success, result) =>
                    {
                        if (success && userId.Equals(ChooseTarget.UserId))
                        {
                            string userName = ChooseTarget.GetRemarkName();
                            //在线
                            if (result["data"].ToString().Equals("1"))
                                labName.Text = userName + "（在线）";
                            else
                                labName.Text = userName + "（离线）";
                        }
                    });
            }
        }
        #endregion

        #region 请求接口单向清除聊天记录
        public void ClearServerFriendMsg(string toUserid)
        {
            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "tigase/emptyMyMsg")
                .AddParams("access_token", Applicate.Access_Token)
                .AddParams("type", "1")
                .AddParams("toUesrId", toUserid)
                .Build().Execute(null);
        }
        #endregion

        #endregion

        #region 刷新发送消息面板

        /// <summary>
        /// 刷新文本框状态
        /// </summary>
        public void RefreshSendBoxState(Friend friend)
        {
            if (Thread.CurrentThread.IsBackground)
            {
                var main = new DelegateFriend(RefreshSendBoxState);
                Invoke(main, friend);
                return;
            }

            if (friend == null || UIUtils.IsNull(friend.UserId))
            {
                return;
            }

            // 文本框状态 优先级：
            // 群组锁定>全体禁言>个人禁言>隐身人>密钥丢失>异常状态
            if (friend.IsDismiss == 2)
            {
                var content = string.Format("此{0}已被锁定,不能发送消息", friend.IsGroup == 0 ? "好友" : "群组");
                IsShow_BannedTalkPanel2(true, content);
                return;
            }

            // 单聊无限制
            if (friend.IsGroup == 0)
            {
                IsShow_BannedTalkPanel2(false);
                return;
            }

            var member = RoomMemberDao.Instance.GetRoomMemberById(friend.RoomId, Applicate.MyAccount.userId);

            //是否全体禁言 管理员和群主除外
            if (friend.AllBannedTalk != 0 && member.role != 1 && member.role != 2)
            {
                IsShow_BannedTalkPanel2(true, "本群已进行全体禁言");
                return;
            }

            //是否单个禁言 群主除外
            if (member.role != 1 && member.talkTime > TimeUtils.CurrentTime())
            {
                var content = string.Format("禁言时间至：{0}", TimeUtils.FromatTime(member.talkTime, "yyyy/MM/dd HH:mm:ss"));
                IsShow_BannedTalkPanel2(true, content);
                return;
            }

            // 是否为隐身人
            if (JudgeIsUnseen(Applicate.MyAccount.userId))
            {
                IsShow_BannedTalkPanel2(true, "您当前为隐身人状态,不能发送消息");
                return;
            }

            // 是否密钥丢失
            if (friend.IsSecretGroup == 1 && friend.IsLostKeyGroup == 1)
            {
                IsShow_BannedTalkPanel2(true, "丢失密钥状态,不能发送消息");
                return;
            }

            // 异常状态
            if (friend.IsDismiss == 1)
            {
                IsShow_BannedTalkPanel2(true, "群信息无法获取,不能发送消息");
                return;
            }

            // 能走到这里就是正常状态
            IsShow_BannedTalkPanel2(false);
        }


        /// <summary>
        /// 请不在逻辑中直接调用此方法，譬如你的逻辑是要解除禁言，解开发送框
        /// 直接调用此方法解开发送框的话，如果处于丢失密钥状态就会产生bug
        /// 如果有其他判断请在上面的  RefreshSendBoxState 方法中增加判断
        /// </summary>
        /// <param name="isShow">是否显示</param>
        /// <param name="remindTxt">提示文本内容</param>
        private void IsShow_BannedTalkPanel2(bool isShow, string remindTxt = "")
        {
            if (isShow)
            {
                //隐藏发送按钮
                btnSend.Visible = false;
                txtSend.Visible = false;
                txtSend.Clear();

                if (Bottom_Panel.Controls["lblRemind"] != null && Bottom_Panel.Controls["lblRemind"] is Label lbl)
                {
                    lbl.Text = remindTxt;
                    int location_x = (Bottom_Panel.Width - lbl.Width) / 2;
                    int location_y = (Bottom_Panel.Height - lbl.Height) / 2;
                    lbl.Location = new Point(location_x, location_y);
                }
                else
                {
                    //文字提醒
                    Label lblRemind = new Label
                    {
                        Name = "lblRemind",
                        AutoSize = true,
                        Font = new Font(Applicate.SetFont, 10f),
                        Text = remindTxt,
                        //lblRemind.Size = new Size(200, 23);
                        ForeColor = Color.FromArgb(157, 157, 157),
                        Anchor = AnchorStyles.None
                    };
                    Bottom_Panel.Controls.Add(lblRemind);
                    lblRemind.Focus();
                    int location_x = (Bottom_Panel.Width - lblRemind.Width) / 2;
                    int location_y = (Bottom_Panel.Height - lblRemind.Height) / 2;
                    lblRemind.Location = new Point(location_x, location_y);
                }

                SetControlEnabled(this.Bottom_Panel, false);
            }
            else
            {
                //恢复发送按钮
                btnSend.Visible = true;
                txtSend.Visible = true;
                if (Bottom_Panel.Controls["lblRemind"] != null && Bottom_Panel.Controls["lblRemind"] is Label lblRemind)
                {
                    txtSend.Controls.Remove(lblRemind);
                    lblRemind.Dispose();
                }
                SetControlEnabled(this.Bottom_Panel, true);
            }
        }

        private void UpdateBannedTalk(MessageObject msg)
        {
            if (!msg.objectId.Equals(ChooseTarget.UserId))
                return;

            // 我被禁言 or  取消禁言
            if (msg.myUserId.Equals(msg.toUserId))
            {
                if(ChooseTarget.AllBannedTalk==1)
                {
                    if (msg.type == kWCMessageType.RoomMemberBan)
                    {
                        return;
                    }
                }
                
                // 刷新面板
                ChooseTarget.AllBannedTalk = string.Equals("0", msg.content) ? 0 : 1;
                RefreshSendBoxState(ChooseTarget);
            }
        }
        #endregion

        #region 控制是否显示新消息标识
        private void IsShowUnReadNumPanel(int readNum, string msgId)
        {
            bool isShow = false;    //是否显示新消息标识

            //没有新消息直接不显示
            if (readNum > 0 && !string.IsNullOrEmpty(msgId))
            {
                List<MessageObject> msgList = msgTabAdapter.msgList;
                //int dataCount = msgTabAdapter.TargetMsgData.GetMessageModelDataDictionary().Count;
                //if (dataCount > 0 && msgTabAdapter.msgList[0].type == kWCMessageType.labMoreMsg)
                //    dataCount--;
                //1.如果列表当前的msg数量小于未读数量，显示
                //if (readNum > msgList.Count)
                //    isShow = true;
                ////2.如果未读行数的总高度大于可显示区域，显示
                //else
                //{
                float totalHeight = 0;
                int listViewHeight = xListView.Height;
                if (listViewHeight == 0)
                    isShow = false;
                else
                {
                    int msgIndex = msgList.FindIndex(m => m.messageId == msgId);
                    if (msgIndex > -1)
                        for (int index = msgIndex; index < msgList.Count; index++)
                        {
                            totalHeight += msgTabAdapter.OnMeasureHeight(index);
                            if (totalHeight > listViewHeight)
                            {
                                isShow = true;
                                break;
                            }
                        }
                }
                //}
            }

            RefreshUnreadNumUi(msgId, readNum, isShow, true);
        }
        #endregion

        #region 显示@浮标
        private void AtMeShowPanel(Friend room)
        {
            if (ChooseTarget == null || ChooseTarget.IsGroup != 1 || room.UserId != ChooseTarget.UserId)
            {
                AtMePanel.SendToBack();
                return;
            }

            if (room.IsAtMe == 0)
            {
                AtMePanel.SendToBack();
                return;
            }
            else
            {
                room.UpdateAtMeState(0);
                room.IsAtMe = 0;
            }

            bool isup = true;    //方向
            //获取收到@的messageId
            string messageid = LocalDataUtils.GetStringData(room.UserId + "GROUP_AT_MESSAGEID" + Applicate.MyAccount.userId);
            MessageObject msg = msgTabAdapter.TargetMsgData.GetMsg(messageid);
            if (msg != null)
            {
                //如果@的消息在显示的范围内，则不显示
                int total_height = 0;
                int at_index = msgTabAdapter.msgList.FindIndex(m => m.messageId.Equals(messageid));
                for (int index = at_index; index < msgTabAdapter.msgList.Count; index++)
                {
                    total_height += xListView.GetItemHeight(index);
                    if (total_height > xListView.Height)
                        break;
                }
                if (total_height <= xListView.Height)
                {
                    //清空At
                    LocalDataUtils.SetStringData(room.UserId + "GROUP_AT_MESSAGEID" + Applicate.MyAccount.userId, "");
                    return;
                }

                AtMePanel.Changedata(room, isup);
                AtMePanel.BringToFront();
            }
        }
        #endregion

        #region 自己主动@人的通知
        private void AddAtUserToTxtSend(MessageObject at_msg)
        {
            //不是当前聊天对象的通知
            if (!at_msg.objectId.Equals(ChooseTarget.UserId))
                return;

            //当前为禁言状态
            if (txtSend.Controls["lblRemind"] != null)
                return;

            atLastCount = ++atCurrentCount;
            Friend friend = new Friend()
            {
                UserId = at_msg.fromUserId,
                NickName = at_msg.fromUserName,
                IsGroup = 0
            };
            list_atFriends.Add(friend);

            txtSend.SelectedText += at_msg.content;
        }
        #endregion

        #region 更新设备的状态
        private void UpdateDeviceState(string userId)
        {
            if (ChooseTarget == null || ChooseTarget.UserType != FriendType.DEVICE_TYPE || !isOpenOnlineStatus)
                return;

            //修改在线状态
            bool isOnline = MultiDeviceManager.Instance.IsDeviceLine(ChooseTarget.UserId);
            labName.Text = ChooseTarget.NickName/* + (isOnline ? "（在线）" : "（离线）")*/;
        }
        #endregion

        #region 记录阅后即焚的时间
        private void SaveReadDelTime()
        {
            if (isHaveReadDel == 0)
                return;

            foreach (var msg in msgTabAdapter.TargetMsgData.GetMsgList())
            {
                //阅后即焚消息
                if (msg.isReadDel == 1 && msg.ReadDelTime > 0)
                {
                    //记录当前的秒数
                    LocalDataUtils.SetIntData(Applicate.MyAccount.userId + "_ReadDelTime_" + msg.messageId, msg.ReadDelTime);
                    //msg.isReadDel = 0;
                    var crl = GetTalkPanelByMsg(msg.messageId);
                    if (crl.readDel_timer != null)
                    {
                        crl.readDel_timer.Stop();
                        crl.readDel_timer.Dispose();
                    }
                }
            }
        }
        #endregion

        private EQBaseControl GetTalkPanelByMsg(string msgId)
        {
            string name = "panel_" + msgId;

            Control panel = xListView.panel1.Controls[name];
            if (panel == null)
                return null;

            var result = panel.Controls.Find("talk_panel_" + msgId, true);
            if (result.Length > 0 && result[0] is EQBaseControl talk_panel)
                return talk_panel;

            return null;
        }

        #region 工具栏

        private void BindTool()
        {
            #region 表情的回调
            showMsgPanel_Tool.ExpressionAction = (type, code) =>
            {
                //不能发言
                if (CollectUtils.EnableForward(ChooseTarget))
                    return;

                switch (type)
                {
                    //选择了emoji表情
                    case ExpressionType.Emoji:
                        AddEmojiToTxtSend(code);
                        break;
                    case ExpressionType.Gif:
                        MessageObject gif_msg = ShiKuManager.SendGifMessage(ChooseTarget, code, false);
                        //Messenger.Default.Send(gif_msg, token: MessageActions.XMPP_UPDATE_NORMAL_MESSAGE);   //添加消息气泡通知
                        JudgeMsgIsAddToPanel(gif_msg);
                        ShiKuManager.SendMessage(gif_msg);
                        break;
                    case ExpressionType.Image:
                        //此处code为url
                        ImageLoader.Instance.Load(code).Into((bit, path) =>
                        {
                            int fileSize = Convert.ToInt32(new FileInfo(path).Length / 1024);
                            MessageObject img_msg = ShiKuManager.SendImageMessage(ChooseTarget, code, path, fileSize, false);
                            //Messenger.Default.Send(img_msg, token: MessageActions.XMPP_UPDATE_NORMAL_MESSAGE);   //添加消息气泡通知
                            JudgeMsgIsAddToPanel(img_msg);
                            ShiKuManager.SendMessage(img_msg);
                        });
                        break;
                    case ExpressionType.Dice:
                        MessageObject dice_msg = ShiKuManager.SendDiceMessage(ChooseTarget, false);
                        JudgeMsgIsAddToPanel(dice_msg);
                        ShiKuManager.SendMessage(dice_msg);
                        break;
                    case ExpressionType.Jsb:
                        MessageObject jsb_msg = ShiKuManager.SendJsbMessage(ChooseTarget, false);
                        JudgeMsgIsAddToPanel(jsb_msg);
                        ShiKuManager.SendMessage(jsb_msg);
                        break;
                }
            };
            #endregion

            #region 选择文件发送回调
            showMsgPanel_Tool.SendFileAction = () =>
            {
                OpenFileDialog dialog = new OpenFileDialog
                {
                    Multiselect = true,//该值确定是否可以选择多个文件
                    Title = "请选择文件",
                    Filter = "所有文件 (*.*)|*.*" +
                "|图像 (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png" +
                "|视频 (*.avi;*.mp4;*.rmvb;*.flv)|*.avi;*.mp4;*.rmvb;*.flv"
                };

                //完成选择文件的操作
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string file in dialog.FileNames)
                    {
                        //FileStream fsRead = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        //Clipboard.SetDataObject(fsRead, true, 3, 500);

                        int fileSize = Convert.ToInt32(new FileInfo(file).Length);
                        if (fileSize < 1)
                        {
                            HttpUtils.Instance.ShowTip("文件“" + file + "”为空文件不能发送");
                            continue;
                        }
                        if(fileSize > 200 * 1024 * 1024)
                        {
                            HttpUtils.Instance.ShowTip("文件“" + file + "”超过200M，不能发送");
                            continue;
                        }
                        bool isVideo = FileUtils.JudgeIsVideoFile(file);
                        //如果为视频文件
                        if (isVideo)
                        {
                            //先生成气泡
                            MessageObject msg = ShiKuManager.SendVideoMessage(ChooseTarget, file, file, fileSize, false);
                            msg.isLoading = 1;
                            JudgeMsgIsAddToPanel(msg);

                            //获取气泡
                            EQBaseControl talk_panel = GetTalkPanelByMsg(msg.messageId);
                            if (talk_panel != null)
                            {
                                UploadEngine.Instance.From(file).
                                //上传中

                                //上传完成
                                UploadFile((success, url) =>
                                {
                                    UploadVideo(talk_panel, msg, url, success);
                                });
                            }
                        }
                        else
                            UploadFileOrImage(file, Convert.ToInt32(new FileInfo(file).Length));   //上传图片
                    }
                    dialog.Dispose();
                }
            };
            #endregion

            #region 截图回调
            showMsgPanel_Tool.CaptureAction = (frmCapture) =>
            {
                //不能发言
                if (CollectUtils.EnableForward(ChooseTarget))
                    return;

                string FilePath = Applicate.LocalConfigData.ImageFolderPath + Guid.NewGuid().ToString("N") + ".png";
                frmCapture.m_imgLastLayer.MySave(FilePath, ImageFormat.Png);
                bool txt_readOnly = txtSend.ReadOnly;
                txtSend.ReadOnly = false;
                int new_width = frmCapture.m_imgLastLayer.Width, new_height = frmCapture.m_imgLastLayer.Height;
                EQControlManager.ModifyWidthAndHeight(ref new_width, ref new_height, 150, 150);
                Image new_image = new Bitmap(frmCapture.m_imgLastLayer, new_width, new_height);

                Clipboard.Clear();
                Clipboard.SetImage(new_image);
                //将图片粘贴到鼠标焦点位置(选中的字符都会被图片覆盖)
                txtSend.Paste();
                txtSend.ReadOnly = txt_readOnly;
                if (SrcImages == null)
                {
                    SrcImages = new Dictionary<string, string>();
                }
                //获取截图的图片的RTF
                using (RichTextBox richTextBox = new RichTextBox())
                {
                    richTextBox.Paste();
                    string txt_rtf = richTextBox.Rtf;
                    string image_rtf = EQControlManager.subRtf(txt_rtf);
                    SrcImages.Add(FilePath, image_rtf);
                }

                //恢复到初始
                Clipboard.Clear();
                Clipboard.SetImage(frmCapture.m_imgLastLayer);
                //capture.Image.Dispose();
            };
            #endregion

            #region 录音回调
            showMsgPanel_Tool.RecordAudioAction = () =>
            {
                //判断是否正在回复消息
                if (replyPanel.ReplyMsg != null && !string.IsNullOrEmpty(replyPanel.ReplyMsg.messageId))
                {
                    var result = HttpUtils.Instance.ShowPromptBox("当前正在回复消息\r\n是否取消回复开启录音");
                    if (result)
                    {
                        //清空并隐藏恢复面板
                        replyPanel.ReplyMsg = new MessageObject();
                        replyPanel.Visible = false;;                        
                    }
                    else
                        return;
                }


                //正在录音
                var view = this.Parent as FrmNewTable;
                if (Applicate.IsOpenRecordView(view))
                {
                    return;
                }

                //开启录音功能
                if (userSoundRecording.IsCanSoundRecord())
                {
                    ShowSoundRecord();
                }
                else
                {
                    userSoundRecording.SendToBack();
                    HttpUtils.Instance.ShowTip("未发现录音设备");
                    //MessageBox.Show("未发现录音设备", "警告");
                }
            };
            #endregion

            #region 拍照回调
            showMsgPanel_Tool.CameraAction = () =>
            {
                FrmTakePhoto takephoto = FrmTakePhoto.GetInstance(ChooseTarget.UserId);
                if (takephoto.iscontentpoto())
                {
                    takephoto.Show();
                    //点击发送的回调
                    takephoto.takeimage = (image, localPath, userId) =>
                    {
                        //不能发言
                        if (CollectUtils.EnableForward(ChooseTarget))
                            return;

                        if (string.IsNullOrEmpty(localPath) || !File.Exists(localPath))
                            return;

                        Friend sendFd = new Friend() { UserId = userId }.GetByUserId();
                        //添加气泡到列表
                        MessageObject msg = ShiKuManager.SendImageMessage(sendFd, "", localPath, Convert.ToInt32(new FileInfo(localPath).Length), false);
                        msg.InsertData();
                        var chat = ChatTargetDictionary.GetMsgData(userId);
                        if (chat.GetItemCount() > 0)
                        {
                            if (chat.isSeparateChat != 1)
                                JudgeMsgIsAddToPanel(msg);
                            UploadEngine.Instance.From(localPath).
                                //上传完成
                                UploadFile((success, url_path) =>
                                {
                                    msg.content = url_path;
                                    if (chat.isSeparateChat == 1)    //独立窗口
                                    {
                                        msg.UpdateData();
                                        Messenger.Default.Send(msg, MessageActions.XMPP_UPDATE_NORMAL_MESSAGE);
                                        ShiKuManager.SendMessage(msg);
                                    }
                                    else
                                    {
                                        msg = chat.GetMsg(msg.messageId);
                                        if (msg != null)
                                        {
                                            //修改气泡的图片和样式
                                            EQBaseControl talk_panel = GetTalkPanelByMsg(msg.messageId);
                                            if (talk_panel != null && talk_panel is EQImageControl imageCrl)
                                            {
                                                UploadImage(imageCrl, msg, url_path, success);
                                            }
                                        }
                                    }
                                });
                        }
                        else    //聊天对象不存在于独立窗口或者主窗口
                        {
                            UploadEngine.Instance.From(localPath).
                                //上传完成
                                UploadFile((success, url_path) =>
                                {
                                    if (success && !string.IsNullOrEmpty(url_path))
                                    {
                                        msg.content = url_path;
                                        msg.UpdateData();
                                        ShiKuManager.SendMessage(msg);
                                    }
                                    else
                                    {
                                        msg.content = "error";
                                        msg.UpdateData();
                                    }
                                });
                        }
                    };
                }
                else
                {
                    HttpUtils.Instance.FindFrm(typeof(FrmMain)).ShowTip("未发现拍照设备或设备占用");
                    //MessageBox.Show("未发现拍照设备", "警告");
                    return;
                }
            };
            #endregion

            #region 录像回调
            showMsgPanel_Tool.PhotographyAction = () =>
            {
                //不能发言
                FrmTakeVideo frmTakeVideo = FrmTakeVideo.GetInstance();
                if (frmTakeVideo.iscontentpoto())
                {
                    frmTakeVideo.Show();
                    frmTakeVideo.videoInfo = (localPath) =>
                    {
                        //文件不存在
                        if (!File.Exists(localPath))
                            return;

                        // 禅道9064
                        HttpUtils.Instance.PopView(frmTakeVideo);

                        //先生成气泡
                        int fileSize = Convert.ToInt32(new FileInfo(localPath).Length);
                        MessageObject msg = ShiKuManager.SendVideoMessage(ChooseTarget, localPath, localPath, fileSize, false);
                        JudgeMsgIsAddToPanel(msg);

                        //获取气泡
                        EQBaseControl talk_panel = GetTalkPanelByMsg(msg.messageId);
                        if (talk_panel != null)
                        {
                            UploadEngine.Instance.From(localPath).
                            //上传完成
                            UploadFile((success, url) =>
                            {
                                UploadVideo(talk_panel, msg, url, success);
                            });
                        }
                    };
                }
                else
                    HttpUtils.Instance.FindFrm(typeof(FrmMain)).ShowTip("未发现拍照设备或设备占用");
            };
            #endregion

            #region 快捷回复回调
            showMsgPanel_Tool.QreplyAction = (e) =>
            {
                FrmReplay frmReplay = new FrmReplay();
                frmReplay.loadData(e);
                frmReplay.Sometimetext = (text) =>
                {
                    txtSend.Clear();
                    txtSend.Text = txtSend.Text + text;//字符串的拼接
                    frmReplay.Close();
                };
            };
            #endregion
        }

        #region 选择表情
        private FrmExpressionTab frmExpressionTab;      //表情列表窗口
        private void LblExpression_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            //获取鼠标点击表情时的坐标
            Point ms = Control.MousePosition;
            frmExpressionTab = FrmExpressionTab.GetExpressionTab();
            //修改列表索引
            frmExpressionTab.tabExpression.SelectedIndex = 0;
            //设置弹出窗起始坐标
            int location_x = ms.X - e.X - 8;
            int location_y = ms.Y - frmExpressionTab.Height - e.Y - 5;
            frmExpressionTab.Location = new Point(location_x, location_y);
            //传递对象给新窗口
            //frmExpressionTab.SetFriendTarget();
            //控件显示在最上层
            frmExpressionTab.TopMost = true;
            frmExpressionTab.Show();

            //回调
            frmExpressionTab.expressionAction = (type, code) =>
            {
                switch (type)
                {
                    //选择了emoji表情
                    case ExpressionType.Emoji:
                        AddEmojiToTxtSend(code);
                        break;
                    case ExpressionType.Gif:
                        MessageObject gif_msg = ShiKuManager.SendGifMessage(ChooseTarget, code, false);
                        //Messenger.Default.Send(gif_msg, token: MessageActions.XMPP_UPDATE_NORMAL_MESSAGE);   //添加消息气泡通知
                        JudgeMsgIsAddToPanel(gif_msg);
                        ShiKuManager.SendMessage(gif_msg);
                        break;
                    case ExpressionType.Image:
                        //此处code为url
                        ImageLoader.Instance.Load(code).Into((bit, path) =>
                        {
                            int fileSize = Convert.ToInt32(new FileInfo(path).Length / 1024);
                            MessageObject img_msg = ShiKuManager.SendImageMessage(ChooseTarget, code, path, fileSize, false);
                            //Messenger.Default.Send(img_msg, token: MessageActions.XMPP_UPDATE_NORMAL_MESSAGE);   //添加消息气泡通知
                            JudgeMsgIsAddToPanel(img_msg);
                            ShiKuManager.SendMessage(img_msg);
                        });
                        break;
                    case ExpressionType.Dice:
                        MessageObject dice_msg = ShiKuManager.SendDiceMessage(ChooseTarget, false);
                        JudgeMsgIsAddToPanel(dice_msg);
                        ShiKuManager.SendMessage(dice_msg);
                        break;
                    case ExpressionType.Jsb:
                        MessageObject jsb_msg = ShiKuManager.SendJsbMessage(ChooseTarget, false);
                        JudgeMsgIsAddToPanel(jsb_msg);
                        ShiKuManager.SendMessage(jsb_msg);
                        break;
                }
            };
        }
        #endregion

        #region 选择文件并发送
        private void LblSendFile_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = true,//该值确定是否可以选择多个文件
                Title = "请选择文件",
                Filter = "所有文件 (*.*)|*.*" +
                "|图像 (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png" +
                "|视频 (*.avi;*.mp4;*.rmvb;*.flv)|*.avi;*.mp4;*.rmvb;*.flv"
            };

            //完成选择文件的操作
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                foreach (string file in dialog.FileNames)
                {
                    //FileStream fsRead = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    //Clipboard.SetDataObject(fsRead, true, 3, 500);

                    bool isVideo = FileUtils.JudgeIsVideoFile(file);
                    //如果为视频文件
                    if (isVideo)
                    {
                        //先生成气泡
                        int fileSize = Convert.ToInt32(new FileInfo(file).Length);
                        MessageObject msg = ShiKuManager.SendVideoMessage(ChooseTarget, file, file, fileSize, false);
                        msg.isLoading = 1;
                        JudgeMsgIsAddToPanel(msg);

                        //获取气泡
                        EQBaseControl talk_panel = GetTalkPanelByMsg(msg.messageId);
                        if (talk_panel != null)
                        {
                            UploadEngine.Instance.From(file).
                            //上传中

                            //上传完成
                            UploadFile((success, url) =>
                            {
                                UploadVideo(talk_panel, msg, url, success);
                            });
                        }
                    }
                    else
                        UploadFileOrImage(file, Convert.ToInt32(new FileInfo(file).Length));   //上传图片
                }
                dialog.Dispose();
            }
        }
        #endregion

        #region 截图
        public void LblScreen_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //CaptureImageTool capture = new CaptureImageTool();

                // capture.SelectCursor = CursorManager.ArrowNew;
                //  capture.DrawCursor = CursorManager.CrossNew;

                //if (capture.ShowDialog() == DialogResult.OK)

                ScreenshotTool.FrmCapture frmCapture = new ScreenshotTool.FrmCapture(false, false);
                frmCapture.ShowDialog();
                if (frmCapture.DialogResult == DialogResult.OK)
                {
                    string FilePath = Applicate.LocalConfigData.ImageFolderPath + Guid.NewGuid().ToString("N") + ".png";
                    frmCapture.m_imgLastLayer.MySave(FilePath, ImageFormat.Png);
                    bool txt_readOnly = txtSend.ReadOnly;
                    txtSend.ReadOnly = false;
                    int new_width = frmCapture.m_imgLastLayer.Width, new_height = frmCapture.m_imgLastLayer.Height;
                    EQControlManager.ModifyWidthAndHeight(ref new_width, ref new_height, 150, 150);
                    Image new_image = new Bitmap(frmCapture.m_imgLastLayer, new_width, new_height);

                    Clipboard.Clear();
                    Clipboard.SetImage(new_image);
                    //将图片粘贴到鼠标焦点位置(选中的字符都会被图片覆盖)
                    txtSend.Paste();
                    txtSend.ReadOnly = txt_readOnly;
                    if (SrcImages == null)
                    {
                        SrcImages = new Dictionary<string, string>();
                    }
                    //获取截图的图片的RTF
                    using (RichTextBox richTextBox = new RichTextBox())
                    {
                        richTextBox.Paste();
                        string txt_rtf = richTextBox.Rtf;
                        string image_rtf = EQControlManager.subRtf(txt_rtf);
                        SrcImages.Add(FilePath, image_rtf);
                    }

                    //恢复到初始
                    Clipboard.Clear();
                    Clipboard.SetImage(frmCapture.m_imgLastLayer);
                    //capture.Image.Dispose();
                }

                if (!Visible)
                {
                    Show();
                }
            }
        }
        #endregion

        #region 历史聊天记录
        private void LblHistory_MoseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;


            var frmHistory = Applicate.GetWindow<FrmHistoryChat>();

            if (frmHistory == null)
            {
                frmHistory = new FrmHistoryChat();
                frmHistory.ShowFriendMsg(ChooseTarget);
                frmHistory.Show();
            }
            else
            {
                frmHistory.Activate();
                frmHistory.ShowFriendMsg(ChooseTarget);
            }

        }
        #endregion

        #region 选择并发送定位
        private void LblLocation_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;


            //Console.WriteLine("" + ShiKuManager.GetXmppState());
            if (ShiKuManager.GetXmppState() != SocketConnectionState.Authenticated)
            {
                HttpUtils.Instance.ShowTip("网络异常，不能定位");
                return;
            }

            FrmSedLocation frmSedLocation = FrmSedLocation.CreateInstrance();
            frmSedLocation.initCefSharp(ChooseTarget);
            //frmSedLocation.Show();
        }
        #endregion

        #region 录音
        private void LblSoundRecord_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            //判断是否正在回复消息
            if (replyPanel.ReplyMsg != null && !string.IsNullOrEmpty(replyPanel.ReplyMsg.messageId))
            {
                var result = HttpUtils.Instance.ShowPromptBox("当前正在回复消息\r\n是否取消回复开启录音");
                if (result)
                {
                    //清空并隐藏恢复面板
                    replyPanel.ReplyMsg = new MessageObject();
                    replyPanel.Visible = false;;
                }
                else
                    return;
            }


            //正在录音
            var view = this.Parent as FrmNewTable;
            if (Applicate.IsOpenRecordView(view))
            {
                return;
            }

            //开启录音功能
            if (userSoundRecording.IsCanSoundRecord())
            {
                ShowSoundRecord();
            }
            else
            {
                userSoundRecording.SendToBack();
                HttpUtils.Instance.ShowTip("未发现录音设备");
                //MessageBox.Show("未发现录音设备", "警告");
            }
        }

        private void ShowSoundRecord()
        {
            var tt = Bottom_Panel;
            userSoundRecording.Visible = true;
            userSoundRecording.Location = new Point(0, tt.Location.Y - userSoundRecording.Height);
            userSoundRecording.lbltime.Text = "";
            //userSoundRecording.lbltime.BringToFront();
            replyPanel.Visible = false;;
            replyPanel.Visible = false;
            userSoundRecording.BringToFront();
        }
        #endregion

        #region 拍照
        private void LblCamera_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (Applicate.IsOpenRecordView())
            {
                return;
            }


            FrmTakePhoto takephoto = FrmTakePhoto.GetInstance(ChooseTarget.UserId);
            if (takephoto.iscontentpoto())
            {
                takephoto.Show();
                //点击发送的回调
                takephoto.takeimage = (image, localPath, userId) =>
                {
                    if (string.IsNullOrEmpty(localPath) || !File.Exists(localPath))
                        return;

                    Friend sendFd = new Friend() { UserId = userId }.GetByUserId();
                    //添加气泡到列表
                    MessageObject msg = ShiKuManager.SendImageMessage(sendFd, "", localPath, Convert.ToInt32(new FileInfo(localPath).Length), false);
                    msg.InsertData();
                    var chat = ChatTargetDictionary.GetMsgData(userId);
                    if (chat.GetItemCount() > 0)
                    {
                        if (chat.isSeparateChat != 1)
                            JudgeMsgIsAddToPanel(msg);
                        UploadEngine.Instance.From(localPath).
                            //上传完成
                            UploadFile((success, url_path) =>
                            {
                                msg.content = url_path;
                                if (chat.isSeparateChat == 1)    //独立窗口
                                {
                                    msg.UpdateData();
                                    Messenger.Default.Send(msg, MessageActions.XMPP_UPDATE_NORMAL_MESSAGE);
                                    ShiKuManager.SendMessage(msg);
                                }
                                else
                                {
                                    msg = chat.GetMsg(msg.messageId);
                                    if (msg != null)
                                    {
                                        //修改气泡的图片和样式
                                        EQBaseControl talk_panel = GetTalkPanelByMsg(msg.messageId);
                                        if (talk_panel != null && talk_panel is EQImageControl imageCrl)
                                        {
                                            UploadImage(imageCrl, msg, url_path, success);
                                        }
                                    }
                                }
                            });
                    }
                    else    //聊天对象不存在于独立窗口或者主窗口
                    {
                        UploadEngine.Instance.From(localPath).
                            //上传完成
                            UploadFile((success, url_path) =>
                            {
                                if (success && !string.IsNullOrEmpty(url_path))
                                {
                                    msg.content = url_path;
                                    msg.UpdateData();
                                    ShiKuManager.SendMessage(msg);
                                }
                                else
                                {
                                    msg.content = "error";
                                    msg.UpdateData();
                                }
                            });
                    }
                };
            }
            else
            {
                HttpUtils.Instance.FindFrm(typeof(FrmMain)).ShowTip("未发现拍照设备或设备占用");
                //MessageBox.Show("未发现拍照设备", "警告");
                return;
            }
        }
        #endregion

        #region 录像
        private void LblPhotography_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (Applicate.IsOpenRecordView())
            {
                return;
            }

            //刘欢实现方式
            FrmTakeVideo frmTakeVideo = FrmTakeVideo.GetInstance();
            if (frmTakeVideo.iscontentpoto())
            {
                frmTakeVideo.Show();
                frmTakeVideo.videoInfo = (localPath) =>
                {
                    //文件不存在
                    if (!File.Exists(localPath))
                        return;

                    // 禅道9064
                    HttpUtils.Instance.PopView(frmTakeVideo);

                    //先生成气泡
                    int fileSize = Convert.ToInt32(new FileInfo(localPath).Length);
                    MessageObject msg = ShiKuManager.SendVideoMessage(ChooseTarget, localPath, localPath, fileSize, false);
                    JudgeMsgIsAddToPanel(msg);

                    //获取气泡
                    EQBaseControl talk_panel = GetTalkPanelByMsg(msg.messageId);
                    if (talk_panel != null)
                    {
                        UploadEngine.Instance.From(localPath).
                        //上传完成
                        UploadFile((success, url) =>
                        {
                            UploadVideo(talk_panel, msg, url, success);
                        });
                    }
                };
            }
            else
                HttpUtils.Instance.FindFrm(typeof(FrmMain)).ShowTip("未发现拍照设备或设备占用");

            ////FFMPEG实现录像
            //FrmRecordVideo frmRecordVideo = new FrmRecordVideo();
            //frmRecordVideo.videoInfo = (localPath) =>
            //{
            //    //文件不存在
            //    if (!File.Exists(localPath))
            //        return;

            //    // 禅道9064
            //    HttpUtils.Instance.PopView(frmRecordVideo);

            //    //先生成气泡
            //    int fileSize = Convert.ToInt32(new FileInfo(localPath).Length);
            //    MessageObject msg = ShiKuManager.SendVideoMessage(ChooseTarget, localPath, localPath, fileSize, false);
            //    JudgeMsgIsAddToPanel(msg);

            //    //获取气泡
            //    EQBaseControl talk_panel = GetTalkPanelByMsg(msg.messageId);
            //    if (talk_panel != null)
            //    {
            //        UploadEngine.Instance.From(localPath).
            //        //上传完成
            //        UploadFile((success, url) =>
            //        {
            //            UploadVideo(talk_panel, msg, url, success);
            //        });
            //    }
            //};
            //frmRecordVideo.Show();
        }
        #endregion

        #region 快捷回复
        // 常用语
        private void Lbl_qreply_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            FrmReplay frmReplay = new FrmReplay();
            frmReplay.loadData(e);
            frmReplay.Sometimetext = (text) =>
            {
                txtSend.Clear();
                txtSend.Text = txtSend.Text + text;//字符串的拼接
                frmReplay.Close();
            };
        }
        #endregion

        #region 发起语音聊天
        private void LblAudio_MouseClick(object sender, MouseEventArgs e)
        {
           
        }
        #endregion

        #region 发起视频聊天
        private void LblVideo_MouseClick(object sender, MouseEventArgs e)
        {
            
        }
        #endregion

        #region 公众号快捷栏
        private void LblSLC_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            //serviceListCrl.SetList(ChooseTarget, this);
            SetDialogBoxSize(DialogBox.OfficialAccount);
        }
        #endregion

        #region 对讲机
        private void lblTalk_MouseClick(object sender, MouseEventArgs e)
        {
           
        }
        #endregion

        #region 点击弹出设置页
        FrmSuspension frmSet;   //设置页
        private void Lab_detial_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (string.IsNullOrEmpty(ChooseTarget.UserId))
                return;

            if (ChooseTarget.Status == 0)
            {
                HttpUtils.Instance.ShowTip("陌生人无法查看好友设置");
                return;
            }

            //如果聊天对象为群组
            if (ChooseTarget.IsGroup == 1)
            {
                frmSet = new FrmSMPGroupSet() { room = ChooseTarget };
            }
            else
            {
                if (ChooseTarget.UserType == 2)
                {
                    frmSet = new FrmOfficalAccountSet { friend = ChooseTarget };
                }
                else
                {
                    frmSet = new FrmSingleSet() { friend = ChooseTarget };
                }
            }

            //获取三个点控件相对屏幕的位置
            Point point = PointToScreen(xListView.Location);
            point = new Point(point.X + (xListView.Width - frmSet.Width), point.Y);

            frmSet.StartPosition = FormStartPosition.Manual;
            frmSet.Location = point;
            frmSet.Height = this.Height - panTitle.Height - 1;
            frmSet.Show(this.Parent);
        }

        private void LabName_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            Lab_detial_MouseClick(lab_detial, e);
        }
        #endregion

        #region 显示或隐藏群已读人数
        private void IsShowReadPersons(bool isShow)
        {
            var msg_list = msgTabAdapter.msgList;
            foreach (MessageObject msg in msg_list)
            {
                EQBaseControl talk_panel = GetTalkPanelByMsg(msg.messageId);
                if (talk_panel == null)
                    continue;
                //显示已读人数标识
                var crls = talk_panel.Controls.Find("lab_msg", true);
                if (crls.Length > 0 && crls[0] is Label lab_msg)
                    if (this.IsHandleCreated)
                    {
                        Invoke(new Action(() =>
                        {
                            if (lab_msg.Tag != null)
                            {
                                int status = 0;
                                try
                                {
                                    status = Convert.ToInt32(lab_msg.Tag.ToString().Substring(lab_msg.Tag.ToString().IndexOf("_") + 1));
                                }
                                catch (Exception ex) { LogHelper.log.Error("-------------显示或隐藏群已读人数出错，方法（IsShowReadPersons）: \r\n" + ex.Message); }
                                finally
                                {
                                    if (status != msg.isSend)
                                        EQControlManager.DrawReadPerson(msg.readPersons, lab_msg);
                                }

                            }
                            lab_msg.Visible = isShow;
                        }));


                        crls = talk_panel.Controls.Find("lab_redPoint", true);
                        if (crls.Length > 0 && crls[0] is Label lab_redPoint)
                        {
                            if (isShow)
                            {
                                //红点下移
                                Invoke(new Action(() =>
                                {
                                    if (lab_redPoint.Location.Y < 38)
                                        lab_redPoint.Location = new Point(lab_redPoint.Location.X, 38);
                                }));
                            }
                            else
                            {
                                //红点回到居中
                                Invoke(new Action(() =>
                                {
                                    int loc_y = (talk_panel.Height - lab_redPoint.Height + 9) / 2;  //气泡上面有头像所以下面要加上才能平均
                                    lab_redPoint.Location = new Point(lab_redPoint.Location.X, loc_y);
                                }));
                            }
                        }
                    }
            }
        }
        #endregion

        #region 如果底部有空白滚动到底部

        public void RollBottom_HaveEmpty()
        {
            // 1.如果相等代表已经在底部
            // 2.如果panel1小于列表高度，则不滚动
            if ((Math.Abs(xListView.panel1.Location.Y) + xListView.Height) != xListView.panel1.Height
                && xListView.Height < xListView.panel1.Height
                && msgTabAdapter.msgList.Count() > 1)
            {
                xListView.ShowRangeEnd(msgTabAdapter.msgList.Count() - 1, 0, true);
        }
        }
        #endregion

        #region 文本发送框的右键菜单
        #region 粘贴
        private void MenuItem_paste_Click(object sender, EventArgs e)
        {
            /*
             * 1.当粘贴板只有一张图片时，IData.GetDataPresent(DataFormats.Bitmap)为true，可以获取到单张图片,
             * 切不计算入Clipboard.GetFileDropList().Count内
             * 2.当粘贴板有两张或以上的图片时，Clipboard.GetFileDropList().Count > 0，
             * 且IData.GetDataPresent(DataFormats.Bitmap)为false
            */
            IDataObject IData = Clipboard.GetDataObject();
            if (IData.GetDataPresent(DataFormats.Text) || IData.GetDataPresent(DataFormats.UnicodeText))
            {
                txtSend.Paste();
            }
            if (IData.GetDataPresent(DataFormats.Rtf))      //Ctrl+C对方的消息后，可能会有emoji的rtf
            {
                using (RichTextBox richTextBox = new RichTextBox())
                {
                    richTextBox.Paste();
                    foreach (string emoji_code in EmojiCodeDictionary.GetEmojiDataNotMine().Keys)
                    {
                        string emoji_rtf = EmojiCodeDictionary.GetEmojiRtfByCode(emoji_code);
                        if (richTextBox.Rtf.IndexOf(emoji_rtf) >= 0)
                        {
                            //不存在才添加
                            if (emoji_codes.FindIndex(code => code.Equals("[" + emoji_code + "]")) < 0)
                            {
                                emoji_num++;
                                emoji_codes.Add("[" + emoji_code + "]");
                            }
                        }
                    }
                }
            }
            if (IData.GetDataPresent(DataFormats.Bitmap))
            {
                //保存路径
                string filePath = Applicate.LocalConfigData.ImageFolderPath + Guid.NewGuid().ToString("N") + ".png";
                //获取黏贴板的图片
                Image image = (Bitmap)IData.GetData(DataFormats.Bitmap);
                if (BitmapUtils.IsNull(image))
                    return;
                int new_width = image.Width, new_height = image.Height;
                EQControlManager.ModifyWidthAndHeight(ref new_width, ref new_height, 150, 150);
                Image new_image = new Bitmap(image, new_width, new_height);
                image.MySave(filePath, ImageFormat.Png); ;

                Clipboard.Clear();
                Clipboard.SetImage(new_image);
                txtSend.Paste();
                if (SrcImages == null)
                {
                    SrcImages = new Dictionary<string, string>();
                }
                //获取截图的图片的RTF
                using (RichTextBox richTextBox = new RichTextBox())
                {

                    richTextBox.Paste();
                    string txt_rtf = richTextBox.Rtf;
                    string image_rtf = EQControlManager.subRtf(txt_rtf);
                    SrcImages.Add(filePath, image_rtf);
                }

                //恢复到初始
                Clipboard.Clear();
                Clipboard.SetImage(image);
                image.Dispose();
            }

            //检查是否黏贴文件
            if (Clipboard.GetFileDropList().Count > 0)
            {
                var strCollection = Clipboard.GetFileDropList();
                foreach (string item in strCollection)
                    FileCollectUtils.AddFileCollectToBox(this.txtSend, item, fileCollects);
            }
            //txtSend.Paste();
        }
        #endregion
        #endregion

        private void LabName_TextChanged(object sender, EventArgs e)
        {
            if (isCanTrigger_NameChanged)
            {
                isCanTrigger_NameChanged = false;
                EQControlManager.StrAddEllipsis(labName, labName.Font, this.Width - 230);
                isCanTrigger_NameChanged = true;
                //labName.Width += 130;
            }
        }

        #region 漫游
        public void OnShowLoading()
        {
            //显示等待符
            StartLoding(xListView, "正在下载漫游消息...");
            isDownloadRoaming = true;
        }

        #region 加载消息记录
        public void OnDownRecord(List<MessageObject> data)
        {
            //xListView.SuspendLayout();
            try
            {
                //销毁等待符
                loding.stop();
                //结束漫游
                isDownloadRoaming = false;
                //如果既不存在更多的本地消息，也没有漫游记录可以拉取
                if (ChooseTarget == null)
                {
                    return;
                }

                if (Applicate.ENABLE_ASY_ENCRYPT)
                {
                    var friend = ChooseTarget.GetByUserId();
                    if (page_index == 1 && friend.IsEncrypt == 3)
                    {
                        if (data == null)
                        {
                            data = new List<MessageObject>();
                        }

                        MessageObject msg = ShiKuManager.GetMessageObject(ChooseTarget);
                        msg.type = kWCMessageType.Remind;
                        msg.content = "私密聊天，消息已开启端到端加密";
                        data.Add(msg);
                    }
                }

                ////List<MessageObject> msg_list = DownRoamAndLoadLocalMsg();   
                List<MessageObject> msg_list = data;//获取本地数据
                if (msg_list == null || msg_list.Count == 0)
                    msgTabAdapter.NotCanAddMoreMsg();
                if (msg_list != null && msg_list.Count > 0)
                {
                    //批量加载
                    msgTabAdapter.BindData(msg_list);
                    int index = msgTabAdapter.FirstMsgIndex;
                    //xListView.InsertRange(index);

                    // 修改禅道bug#7359
                    bool ismore = msg_list[0].type == kWCMessageType.labMoreMsg;

                    //添加lable，显示更多聊天记录
                    if (msgTabAdapter.msgList.Count >= MsgTabAdapter.row_insert || ismore)
                        msgTabAdapter.CanAddMoreMsg();

                    xListView.SetAdapter(msgTabAdapter);

                    // 刷新漫游消息到消息列表 #9511
                    if (UIUtils.IsNull(ChooseTarget.Content))
                    {
                        var local = msg_list[0].GetLastMessage();
                        if (!UIUtils.IsNull(local.messageId) && local.IsVisibleMsg())
                        {
                            string content = ChooseTarget.ToLastContentTip(local.type, local.content, local.fromUserId, local.fromUserName, local.toUserName);
                            if (ChooseTarget.IsGroup == 1 && local.type < kWCMessageType.Remind)
                            {
                                content = local.fromUserName + ":" + content;
                            }

                            if (local.isReadDel == 1)
                            {
                                content = "[阅后即焚消息]";
                            }

                            ChooseTarget.UpdateLastContent(content, local.timeSend);
                            Messenger.Default.Send(ChooseTarget, token: MessageActions.UPDATE_FRIEND_LAST_CONTENT);
                        }
                    }
                }

                //第一次添加集合是切换聊天对象时，所以需要滚动到底部 ChooseTarget.IsSendRecipt == 10 代表搜聊天记录
                //if (isFirstLoad && ChooseTarget.IsSendRecipt != 10)
                //{
                //    isFirstLoad = false;
                //    RollBottom_HaveEmpty();
                //}
            }
            catch (Exception ex) { LogHelper.log.Error("-------------加载消息记录出错，方法（OnDownRecord）: \r\n" + ex.Message); }
            //xListView.ResumeLayout();
        }
        #endregion

        public void LoadMsg()
        {
            //如果数据已经加载了400条，继续滚动会超过最大值
            if (msgTabAdapter.msgList.Count > 400)
            {
                //如果第一行是显示更多聊天记录
                if (msgTabAdapter.FirstMsgIndex > 0 && !msgTabAdapter.msgList[0].content.Equals("更多消息请在消息记录中查阅"))
                {
                    msgTabAdapter.msgList[0].content = "更多消息请在消息记录中查阅";
                    xListView.RefreshItem(0);
                }
                return;
            }

            if (isDownloadRoaming)
                return;
            isDownloadRoaming = true;
            //msgTabAdapter.AddMoreMsg();
            MessageObject msg = msgTabAdapter.TargetMsgData.GetFirstIndexMsg();
            page_index++;

            msgRecord.LoadMsgDatas(ChooseTarget, page_index, msg);
        }
        #endregion

        /// <summary>
        /// 清除过多的消息气泡并进行位移
        /// </summary>
        private void ClearTopMsg()
        {
            int start_index = msgTabAdapter.FirstMsgIndex;
            int msg_count = msgTabAdapter.msgList.Count;
            if (msg_count < 1 || msg_count < 100)
                return;

            int clear_count = msg_count - 50;
            //清除数据
            for (int index = start_index; index < clear_count + start_index; index++)
            {
                string remove_msgId = msgTabAdapter.msgList[start_index].messageId;
                msgTabAdapter.TargetMsgData.RemoveMsgData(remove_msgId);
            }

            xListView.DeleteRange(start_index, clear_count);
        }

        private void AfreshBindData()
        {
            try
            {
                var current_target = ChatTargetDictionary.GetMsgData(ChooseTarget.UserId);
                //清除数据
                if (current_target != null && current_target.GetMsgList().Count > 0)
                    current_target.RemoveAllData();
                BubbleBgDictionary.RemoveAllBg();       //移除所有缓存的背景图

                //清空聊天列表并绑定新的数据
                msgTabAdapter.direction = false;        //倒序排列
                msgTabAdapter.choose_target = ChooseTarget;
                //保存是否为独立聊天
                msgTabAdapter.TargetMsgData.isSeparateChat = this.isSeparateChat;
                page_index = 1;         //漫游页数重置
                isFirstLoad = true;     //能够触发滚动到底部
                msgRecord.LoadMsgDatas(ChooseTarget, page_index, null);
                //List<MessageObject> msgList = msgTabAdapter.LoadObjectLocalMsg();   //获取本地数据
                //msgTabAdapter.BindData(msgList);        //绑定数据源
                xListView.SetAdapter(msgTabAdapter);    //绑定适配器

                Helpers.ClearMemory();
            }
            catch (Exception ex)
            {
                LogHelper.log.Error("----重新绑定数据出错，方法（AfreshBindData） : " + ex.Message, ex);
            }
        }

        #region 当前聊天对象已删除
        private void DeleteFdCloseFrm(Friend friend)
        {
            if (ChooseTarget == null || friend == null)
            {
                return;
            }

            if (string.Equals(friend.UserId, ChooseTarget.UserId))
            {
                if (this.Parent is FrmNewTable frmNewTable)
                {
                    //获取用户
                    Friend targetFd = ChooseTarget;
                    //获取当前对象的字典
                    var targetData = ChatTargetDictionary.GetMsgData(targetFd.UserId);
                    //移除数据和对象
                    targetData.RemoveAllData();
                    ChatTargetDictionary.RemoveItem(targetFd.UserId);

                    frmNewTable.Close();
                }
            }
        }
        #endregion

        /// <summary>
        /// 客服模式下结束会话
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEnding_Click(object sender, EventArgs e)
        {
            MessageObject msg = ShiKuManager.GetMessageObject(ChooseTarget);
            msg.type = kWCMessageType.CustomerEndConnnect;
            msg.content = "结束会话";
            msg.isEncrypt = 0;
            msg.isReadDel = 0;
            ShiKuManager.SendMessage(msg);
        }

        private void ShowMsgPanel_SizeChanged(object sender, EventArgs e)
        {
            if (xListView.Width != this.Width && this.Width > this.MinimumSize.Width)
            {
                xListView.Location = new Point(0, 35);
                Size changedSize = new Size(this.Width, this.Height - panTitle.Height - Bottom_Panel.Height - 1);
                xListView.Size = changedSize;
            }
            }

        /// <summary>
        /// 重新请求密钥
        /// </summary>
        private void RequestKeyPanelClick(object sender, EventArgs e)
        {
            if (ChooseTarget.UserId.Equals(requestKeyPanel.CurrentUserId))
            {
                ShiKuManager.SendRequstChatKeyMessage(ChooseTarget);
            }
        }

        /// <summary>
        /// 更新当前聊天中群员的名称
        /// </summary>
        public void UpdateRoomMemberName(string userId)
        {
            List<MessageObject> update_msgs = new List<MessageObject>();
            foreach (var msg in msgTabAdapter.msgList)
            {
                if (msg == null || string.IsNullOrEmpty(msg.messageId) || string.IsNullOrEmpty(msg.fromUserId) || !msg.IsVisibleMsg() || msg.type == kWCMessageType.Remind)
                    continue;
                if (msg.fromUserId.Equals(userId))
                {
                    update_msgs.Add(msg);
                }
            }
            //遍历修改气泡中的群昵称
            foreach (var msg in update_msgs)
            {
                var crl = GetTalkPanelByMsg(msg.messageId);
                crl.messageObject.SenderName = msgTabAdapter.rm_utils.GetSenderNameByMsg(msg, ChooseTarget.RoomId);
                //Invoke(new Action(() =>
                //{
                crl.UpdateUserName();
                //}));                
            }
        }

        /// <summary>
        /// 更新当前聊天中群员的头像
        /// </summary>
        public void UpdateRoomMemberImage(MessageObject message)
        {
            int role = "1".Equals(message.content) ? 2 : 0;


            msgTabAdapter.ChangeRoomMemberRole(message.toUserId, role);

            foreach (var msg in msgTabAdapter.msgList)
            {
                if (msg == null || string.IsNullOrEmpty(msg.messageId) || string.IsNullOrEmpty(msg.fromUserId) || !msg.IsVisibleMsg() || msg.type == kWCMessageType.Remind)
                {
                    continue;
                }

                if (msg.fromUserId.Equals(message.toUserId))
                {
                    var crl = GetTalkPanelByMsg(msg.messageId);
                    crl.UpdateUserImageRole(role);
                }
            }
        }


        public void CancelVoiceRecord()
        {
            //取消录音
            if (userSoundRecording.SoundState)
                userSoundRecording.StopSound();
        }


        /// <summary>
        /// 设置对话框的尺寸（包括XList的高度变化）
        /// </summary>
        /// <param name="to_dialogBox">转变后的状态</param>
        public void SetDialogBoxSize(DialogBox to_dialogBox)
        {
            int totalLineHeight = 4;        //分割线造成的间距
            switch (this.dialogBox)
            {
                case DialogBox.Normal:
                    if (to_dialogBox == DialogBox.OfficialAccount)
                    {
                        //自动绑定控件数据
                        serviceListCrl.BindData(ChooseTarget, this);

                        //设置快捷栏
                        serviceListCrl.BringToFront();
                        serviceListCrl.Visible = true;
                        //设置消息面板高度
                        //int diff = Bottom_Panel.Height - this.Height;
                        //xListView.Height += diff;
                        xListView.Height = this.Height - panTitle.Height - serviceListCrl.Height - totalLineHeight;
                        //设置底部面板
                        Bottom_Panel.SendToBack();
                        Bottom_Panel.Visible = false;

                        this.dialogBox = DialogBox.OfficialAccount;
                    }
                    else if (to_dialogBox == DialogBox.MultiSelect)
                    {
                        panMultiSelect.Visible = true;
                        Bottom_Panel.Visible = false;
                        panMultiSelect.Dock = DockStyle.Bottom;
                        Bottom_Panel.Dock = DockStyle.None;
                        panMultiSelect.BringToFront();

                        this.dialogBox = DialogBox.MultiSelect;
                    }
                    break;
                case DialogBox.OfficialAccount:
                    if (to_dialogBox == DialogBox.Normal)
                    {
                        serviceListCrl.SendToBack();
                        serviceListCrl.Visible = false;
                        //设置消息面板高度
                        xListView.Height = this.Height - panTitle.Height - Bottom_Panel.Height - totalLineHeight;
                        //设置底部面板
                        if (panMultiSelect.Visible)   //如果显示的是多选面板，则隐藏
                        {
                            panMultiSelect.Visible = false;
                            Bottom_Panel.Dock = DockStyle.Bottom;
                            panMultiSelect.Dock = DockStyle.None;
                        }
                        Bottom_Panel.BringToFront();
                        Bottom_Panel.Visible = true;

                        this.dialogBox = DialogBox.Normal;
                    }
                    else if (to_dialogBox == DialogBox.MultiSelect)
                    {
                        serviceListCrl.SendToBack();
                        serviceListCrl.Visible = false;
                        //设置消息面板高度
                        xListView.Height = this.Height - panTitle.Height - panMultiSelect.Height - totalLineHeight;
                        //设置底部面板
                        if (Bottom_Panel.Visible)   //如果显示的是标准对话框，则隐藏
                        {
                            Bottom_Panel.Visible = false;
                            panMultiSelect.Dock = DockStyle.Bottom;
                            Bottom_Panel.Dock = DockStyle.None;
                        }
                        panMultiSelect.BringToFront();
                        panMultiSelect.Visible = true;

                        this.dialogBox = DialogBox.MultiSelect;
                    }
                    break;
                case DialogBox.MultiSelect:
                    if (to_dialogBox == DialogBox.Normal)
                    {
                        panMultiSelect.Visible = false;
                        Bottom_Panel.Visible = true;
                        panMultiSelect.Dock = DockStyle.None;
                        Bottom_Panel.Dock = DockStyle.Bottom;
                        panMultiSelect.SendToBack();

                        this.dialogBox = DialogBox.Normal;
                    }
                    //else if (this.dialogBox == DialogBox.OfficialAccount)     //不存在该情况
                    //{

                    //}
                    break;
            }
            #endregion
        }

        private void LblExpression_MouseHover(object sender, EventArgs e)
        {

    }

        private void LblSendFile_MouseHover(object sender, EventArgs e)
        {

}

        private void LblScreen_MouseHover(object sender, EventArgs e)
        {

        }

        private void LblHistory_MouseHover(object sender, EventArgs e)
        {

        }

        private void LblVideo_MouseHover(object sender, EventArgs e)
        {

        }

        private void LblAudio_MouseHover(object sender, EventArgs e)
        {

        }

        private void LblLocation_MouseHover(object sender, EventArgs e)
        {

        }

        private void LblCamera_MouseHover(object sender, EventArgs e)
        {

        }

        private void LblPhotography_MouseHover(object sender, EventArgs e)
        {

        }

        private void LblSoundRecord_MouseHover(object sender, EventArgs e)
        {

        }

        private void XListView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPoint = e.Location;

                foreach (var control in xListView.panel1.Controls)
                {
                    if (control is Control item)
                    {
                        if (item is CheckBoxEx checkBox)
                        {
                            checkBox.Checked = false;
                        }
                    }
                }
            }
        }

        private void XListView_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                dragRectangle = new Rectangle(
                    Math.Min(dragStartPoint.X, e.X),
                    Math.Min(dragStartPoint.Y, e.Y),
                    Math.Abs(dragStartPoint.X - e.X),
                    Math.Abs(dragStartPoint.Y - e.Y)
                );
                
                foreach (var control in xListView.panel1.Controls)
                {
                    if (control is Control item && item.Bounds.IntersectsWith(dragRectangle))
                    {
                        if (item is CheckBoxEx checkBox)
                        {
                            checkBox.Checked = true;
                        }
                    }
                }
            }
        }

        private void XListView_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }
    }
}
