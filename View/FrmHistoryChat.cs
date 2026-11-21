using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinFrmTalk.Model;
using WinFrmTalk.Controls;
using WinFrmTalk.Properties;
using WinFrmTalk.Controls.CustomControls;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using RichTextBoxLinks;
using WinFrmTalk.Dictionarys;
using WinFrmTalk.Helper;
using Newtonsoft.Json;
using TestListView;
using WinFrmTalk.View.list;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Specialized;
using WinFrmTalk.Model.dao;

namespace WinFrmTalk.View
{
    public partial class FrmHistoryChat : FrmBase
    {
        #region 全局变量
        //选中的label控件
        public Label is_check;
        public static Friend friends;
        //所有消息
        public static List<MessageObject> listData;

        private bool isOpenForm = false;
        private static int emoji_count = 0;
        public int type;
        RichTextBoxEx richText = new RichTextBoxEx();
        ImageViewxVideo pictureBoxVideo;
        PictureBox pictureBoxImage = new PictureBox();
        FilePanelLeft panel_file = new FilePanelLeft();
        Panel pnlReplay = new Panel();
        ChatItem chatFile = new ChatItem();
        private HistotyChatAdapter mAdapter;
        private bool isScrollBar;
        private int page = 1;
        #endregion
        private void LoadLanguageText()
        {
            if (!Applicate.ENABLE_MULTI_LANGUAGE)
                return;

            lblAll.Text = LanguageXmlUtils.GetValue("history_all", lblAll.Text);
            lblFile.Text = LanguageXmlUtils.GetValue("history_file", lblFile.Text);
            lblImage.Text = LanguageXmlUtils.GetValue("history_image", lblImage.Text);
            lblVideo.Text = LanguageXmlUtils.GetValue("history_video", lblVideo.Text);
        }

        private void FrmHistoryChat_Load(object sender, EventArgs e)
        {
            this.Icon = (Icon)Properties.Resources.Icon.Clone();//加载icon图标（独立副本，避免共享句柄被释放导致闪退）//加载icon图标
            this.Text = LanguageXmlUtils.GetValue("frmHistoryChat_title", "历史记录");
        }


        #region 窗体加载
        public FrmHistoryChat()
        {

            InitializeComponent();
            LoadLanguageText();

            mAdapter = new HistotyChatAdapter();

            xListView.FooterRefresh += OnFooterRefresh;
            xListView.HeaderRefresh += OnHeaderRefresh;

            lblAll.ForeColor = ColorTranslator.FromHtml("#1AAD19");
            lblAll.Font = new Font(Applicate.SetFont, 12);
            is_check = lblAll;

            if (!Applicate.IsInputChatList)
            {
                lblNext.Visible = false;
            }
            SearchMessage.tips = LanguageXmlUtils.GetValue("search_history_msgs", "搜索聊天记录");
            SearchMessage.SearchEvent += HistorySearch;
            SearchMessage.ChangeUnselectColor(Color.FromArgb(230, 229, 229));
        }

        int listPager = 0;

        //向上滚动
        private void OnHeaderRefresh()
        {
            if (listPager > 0)
            {
                listPager = 0;
                page = 1;
                List<MessageObject> messages = MessageObjectDao.Instance.GetHistoryMessagesAll(friends.UserId, page);

                listData.Clear();
                listData.AddRange(messages);
                mAdapter.BindFriendData(listData);
                xListView.SetAdapter(mAdapter);

            }
        }

        //向下滚动
        private void OnFooterRefresh()
        {
            if (isScrollBar)
            {
                page++;

                List<MessageObject> messages = null;

                if (UIUtils.IsNull(SearchMessage.Content))
                {
                    messages = MessageObjectDao.Instance.GetHistoryMessagesAll(friends.UserId, page);
                }
                else
                {
                    messages = MessageObjectDao.Instance.GetHistoryListByContent(friends.UserId, SearchMessage.Content, page);
                }

                if (messages.Count <= 0)
                {
                    return;
                }

                if (listData.Count() > 200)
                {
                    listPager++;
                    listData.Clear();
                    listData.AddRange(messages);
                    mAdapter.BindFriendData(listData);
                    xListView.SetAdapter(mAdapter);
                    return;
                }

                int index = listData.Count;
                listData.AddRange(messages);
                mAdapter.FrmHistoryChat = this;
                mAdapter.BindFriendData(listData);
                xListView.InsertRange(index);
            }
        }

        #endregion

        #region 数据验证
        /// <summary>
        /// 头像
        /// </summary>
        /// <param name="url"></param>
        /// <param name="incoImage"></param>
        public static void TypeFileToImage(string url, PictureBox incoImage)
        {
            int type = 0;
            if (url == "12" || url == "13")
            {
                type = Convert.ToInt32(url);
            }
            else
            {
                type = FileUtils.GetFileTypeByNanme(url);
            }
            //获取当前文件选择的头像

            //ͼƬ
            if (type == 1)
            {
                ImageLoader.Instance.Load(url).NoCache().Into(incoImage);

            }
            else if (type == 2)
            {

                //music
                incoImage.Image = Resources.ic_muc_flie_type_y;

            }
            else if (type == 3)
            {
                //视频
                incoImage.Image = Resources.ic_muc_flie_type_v;

            }
            else if (type == 4)
            {
                //ppt
                incoImage.Image = Resources.ic_muc_flie_type_p;

            }
            else if (type == 5)
            {
                //xlse
                incoImage.Image = Resources.ic_muc_flie_type_x;

            }
            else if (type == 6)
            {
                //world
                incoImage.Image = Resources.ic_muc_flie_type_w;

            }
            else if (type == 7)
            {
                //zrp
                incoImage.Image = Resources.ic_muc_flie_type_z;

            }
            else if (type == 8)
            {
                //txt
                incoImage.Image = Resources.ic_muc_flie_type_t;

            }
            else if (type == 9)
            {
                //??
                incoImage.Image = Resources.ic_muc_flie_type_what;

            }
            else if (type == 10)
            {
                //pdf
                incoImage.Image = Resources.ic_muc_flie_type_f;

            }
            else if (type == 11)
            {

                incoImage.Image = Resources.ic_muc_flie_type_a;

            }
            else if (type == 12)//红包
            {
                incoImage.Image = Resources.ic_historyredpacket;

            }
            else if (type == 13)//转账
            {
                incoImage.Image = Resources.ic_historytransfer;

            }
            else if (type == 56 || type == 55)
            {
                incoImage.Image = Resources.ic_flie_type_voice;
            }
        }
        /// <summary>
        /// 文件下载
        /// </summary>
        /// <param name="toFilePath"></param>
        /// <param name="fileUrl"></param>
        public static void DownFile(string toFilePath, string fileUrl, bool isOpen = false)
        {
            HttpDownloader.DownloadFile(fileUrl, toFilePath, (path) =>
            {
                if (!string.IsNullOrEmpty(path))
                {
                    // 下载成功
                    LogUtils.Log("下载成功！" + path);
                    if (!isOpen)
                    {
                        //打开此文件
                        System.Diagnostics.Process.Start(path);
                    }
                }
            });
        }
        #endregion

        #region  控件点击事件
        private void lblAll_Click(object sender, EventArgs e)
        {
            //清楚原有数据
            xListView.ClearList();
            var views = new List<Control>();

            var text = UIUtils.IsNull(SearchMessage.txt_Search.Text) ? LanguageXmlUtils.GetValue("history_all", "全部") : LanguageXmlUtils.GetValue("search", "搜索");
            lblAll.Text = text;

            #region 字体颜色
            Label label = (Label)sender;
            if (is_check != null && is_check != label)
            {
                //点击当前
                Font f = new Font(Applicate.SetFont, 12);
                label.ForeColor = ColorTranslator.FromHtml("#1AAD19");
                label.Font = f;
                //点击其它按钮
                Font f2 = new Font(Applicate.SetFont, 12);
                is_check.ForeColor = Color.Black;
                is_check.Font = f2;
                is_check = label;
            }
            #endregion
            #region 全部
            if ((Label)sender == lblAll)
            {
                LodingUtils loding = new LodingUtils();
                loding.parent = xListView;
                loding.size = xListView.Size;
                loding.start();
                xListView.Visible = true;
                flpTable.Visible = false;
                //判断是否在加载
                mAdapter.isclickFileButton = false;
                mAdapter.FrmHistoryChat = this;
                mAdapter.BindFriendData(listData);
                xListView.SetAdapter(mAdapter);
                //重新查询
                HistorySearch(SearchMessage.txt_Search.Text);
                loding.stop();
            }
            #endregion
            #region 文件
            if ((Label)sender == lblFile)
            {
                xListView.Visible = true;
                flpTable.Visible = false;
                List<MessageObject> listFileData = ShowMsgFileList(friends.UserId);
                foreach (var item in listFileData)
                {
                    FileInfonmation(item);
                    mAdapter.isclickFileButton = true;
                    mAdapter.FrmHistoryChat = this;
                    mAdapter.BindFriendData(listFileData);
                    xListView.SetAdapter(mAdapter);
                }
            }
            #endregion
            #region ͼƬ
            if ((Label)sender == lblImage)
            {
                type = 3;
                //清除
                ImageIngomation();
            }
            #endregion
            #region 视频
            if ((Label)sender == lblVideo)
            {
                type = 4;
                VdioShow();
            }
            #endregion
        }
        #endregion

        #region 加载分类数据
        //视频
        private void VdioShow()
        {
            //清除
            flpTable.Controls.Clear();
            foreach (var item in listData)
            {
                if (item.type == kWCMessageType.Video)
                {
                    xListView.Visible = false;
                    flpTable.Visible = true;

                    pictureBoxVideo = new ImageViewxVideo();
                    pictureBoxVideo.Tag = item;
                    pictureBoxVideo.Location = new Point(52, 117);
                    pictureBoxVideo.Size = new Size(95, 95);

                    ThubImageLoader.Instance.LoadImage(item.content, pictureBoxVideo);

                    //绑定当前索引
                    pictureBoxVideo.Tag = item;
                    pictureBoxVideo.MouseClick += new MouseEventHandler(Vido_clike);
                    flpTable.Controls.Add(pictureBoxVideo);
                    BindMenuItemList(pictureBoxVideo, item);
                    this.Controls.Add(flpTable);
                }
            }
        }
        //ͼƬ
        private void ImageIngomation()
        {
            flpTable.Controls.Clear();
            foreach (var item in listData)
            {
                if (item.type == kWCMessageType.Image)
                {
                    xListView.Visible = false;
                    flpTable.Visible = true;
                    pictureBoxImage = new PictureBox();
                    pictureBoxImage.Location = new Point(52, 117);
                    pictureBoxImage.Size = new Size(95, 95);
                    ImageLoader.Instance.Load(item.content).NoCache().Into((bitmap, path) =>
                    {
                        bitmap = EQControlManager.ClipImage(bitmap, pictureBoxImage.Width, pictureBoxImage.Height);
                        pictureBoxImage.BackgroundImageLayout = ImageLayout.Zoom;
                        pictureBoxImage.BackgroundImage = bitmap;
                    });
                    //pictureBoxImage.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBoxImage.Dock = DockStyle.None;
                    //绑定当前索引
                    pictureBoxImage.Name = item.messageId;
                    pictureBoxImage.Tag = item;
                    pictureBoxImage.MouseClick += new MouseEventHandler(MouseClie);
                    flpTable.Controls.Add(pictureBoxImage);
                    BindMenuItemList(pictureBoxImage, item);
                    this.Controls.Add(flpTable);
                }
            }
        }
        //文件
        public ChatItem FileInfonmation(MessageObject message)
        {
            //实例化控件
            chatFile = new ChatItem();
            chatFile.Time = TimeUtils.ChatLastTime(message.timeSend);
            UserFileLeft panel_file = new UserFileLeft();
            panel_file.Cursor = Cursors.Hand;
            
            string fileNames = FileUtils.GetFileName(message.fileName);
            fileNames = string.IsNullOrEmpty(message.signature) ? fileNames : message.signature;
            fileNames = EQControlManager.StrAddEllipsis(fileNames, panel_file.lab_fileName.Font, 300);
            //    messageObject.fileName.Substring(messageObject.content.LastIndexOf("/") + 1);
            panel_file.lab_fileName.Text = string.IsNullOrEmpty(message.signature) ? fileNames : message.signature;
            panel_file.lab_fileSize.Text = UIUtils.FromatFileSize(message.fileSize) + " " + message.fromUserName;
            //panel_file.Location = new Point(70, 12);
            panel_file.Location = new Point(chatFile.lblName.Location.X, chatFile.lblName.Location.Y);
            panel_file.BringToFront();
            panel_file.Tag = message;
            fileNames = string.IsNullOrEmpty(message.signature) ? fileNames : message.signature;
            FrmHistoryChat.TypeFileToImage(message.content, panel_file.lab_icon);
            chatFile.lblName.Visible = false;

            foreach (Control itemControl in panel_file.Controls)
            {
                itemControl.MouseClick += (sender, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        string path = "";
                        string fileName = "";
                        path = message.content;
                        fileName = FileUtils.GetFileName(message.content);
                        string filePath = Applicate.LocalConfigData.RoomFileFolderPath + fileName;
                        //下载
                        FrmHistoryChat.DownFile(filePath, path);
                    }
                };
                if (itemControl.Controls.Count > 0)
                    foreach (Control itemcl in itemControl.Controls)
                        itemcl.MouseClick += (sender, e) =>
                        {
                            if (e.Button == MouseButtons.Left)
                            {
                                string path = "";
                                string fileName = "";
                                path = message.content;
                                fileName = FileUtils.GetFileName(message.content);
                                string filePath = Applicate.LocalConfigData.RoomFileFolderPath + fileName;
                                //下载
                                FrmHistoryChat.DownFile(filePath, path);
                            }
                        };
            }
            chatFile.Controls.Add(panel_file);
            chatFile.Size = new Size(xListView.Width - 20 - 33, 97);
            BindMenuItemList(panel_file, message);
            return chatFile;
        }
        #endregion

        #region  数据加载
        // 显示数据
        public void ShowFriendMsg(Friend friend)
        {
            if (!isOpenForm)
            {
                this.Show();
                friends = friend;


                if (is_check != null)
                {
                    is_check.ForeColor = Color.Black;
                }
                lblAll.ForeColor = ColorTranslator.FromHtml("#1AAD19");
                lblAll.Text = LanguageXmlUtils.GetValue("history_all", "全部");
                is_check = lblAll;
                xListView.Visible = true;
                flpTable.Visible = false;

                lblNickName.Text = friend.GetRemarkName();
                LodingUtils loding = new LodingUtils();
                loding.parent = xListView;
                loding.size = xListView.Size;
                loding.start();
                var views = new List<Control>();
                listData = ShowMsgList(friend.UserId);
                if (listData.Count >= 30)
                {
                    isScrollBar = true;
                }
                BindData(listData);




                SearchMessage.ResumeUi();

                loding.stop();
            }
            else
            {
                return;
            }
        }
        //数据绑定
        private void BindData(List<MessageObject> list)
        {
            mAdapter.FrmHistoryChat = this;
            mAdapter.BindFriendData(list);
            xListView.SetAdapter(mAdapter);
        }

        //显示讲课数据
        public void ShowMessageList(List<MessageObject> datas, string title)
        {
            //昵称
            LodingUtils loding = new LodingUtils();
            loding.parent = xListView;
            loding.size = xListView.Size;
            loding.start();
            listData = datas;

            lblNickName.Text = title;
            lblAll.Visible = false;
            lblFile.Visible = false;
            lblImage.Visible = false;
            lblVideo.Visible = false;
            this.Show();
            BindData(datas);
            loding.stop();
        }

        [DllImport("user32.dll", EntryPoint = "SendMessageA")]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, string lParam);
        public static void CalculateWidthAndHeight_Text(MessageObject msg, int maxWidth)
        {
            using (RichTextBoxEx richTextBox = new RichTextBoxEx())
            {
                //正常消息
                richTextBox.Size = new Size(maxWidth, 20);
                richTextBox.Text = msg.content;
                richTextBox.Font = new Font(Applicate.SetFont, 10F);
                richTextBox.MinimumSize = new Size(0, 25);  //最小高度25
                richTextBox.ReadOnly = true;
                richTextBox.ScrollBars = RichTextBoxScrollBars.None;
                richTextBox.WordWrap = true;
                float max_width = 0;
                //通过自适应高度计算
                richTextBox.ContentsResized += new ContentsResizedEventHandler((sender, e) =>
                {
                    if (string.IsNullOrEmpty(richTextBox.Text))
                        return;

                    string str_rtf = richTextBox.Rtf;
                    if (msg.BubbleWidth < maxWidth)
                    {
                        //分割每一行
                        string[] str_row = richTextBox.Text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string item in str_row)
                        {
                            int item_width = (int)EQControlManager.GetStringTheSize(item, richTextBox.Font).Width;
                            if (item_width > max_width)
                                max_width = item_width;
                            //到达了最大宽度
                            if (max_width >= maxWidth)
                            {
                                max_width = maxWidth;
                                break;
                            }
                        }
                        if (max_width < 1)
                            return;
                        //如果文字宽度不足260
                        if (max_width < maxWidth)
                            msg.BubbleWidth = (int)max_width + 5;
                        else
                            msg.BubbleWidth = maxWidth + 5;
                    }
                    //if (msg.BubbleHeight > e.NewRectangle.Height)
                    msg.BubbleHeight = e.NewRectangle.Height;
                    richTextBox.Size = new Size(msg.BubbleWidth + 2, msg.BubbleHeight);
                    richTextBox.Rtf = str_rtf;
                });
                int EM_GETLINECOUNT = 0x00BA;//获取总行数的消息号 
                SendMessage(richTextBox.Handle, EM_GETLINECOUNT, IntPtr.Zero, "");
            }
        }
        #endregion

        #region 绑定右键菜单
        // 绑定右键菜单
        public void BindMenuItemList(Control item, MessageObject message)
        {
            var copyitem = new MenuItem(LanguageXmlUtils.GetValue("copy", "复制"), (sen, eve) =>
            {
                CopyData(message);
                HttpUtils.Instance.ShowTip("复制成功");
            });
            var sendcard = new MenuItem(LanguageXmlUtils.GetValue("forward", "转发"), (sen, eve) =>
            {
                ForwardMessage(message);
            });
            var colleitem = new MenuItem(LanguageXmlUtils.GetValue("collect", "收藏"), (sen, eve) =>
            {

                CollectUtils.CollectMessage(message);
            });
            var cm = new ContextMenu();
            cm.MenuItems.Add(copyitem);
            cm.MenuItems.Add(sendcard);
            cm.MenuItems.Add(colleitem);
            item.ContextMenu = cm;//设置右键菜单
            item.MouseDown += ClickSelectItem;//设置事件
        }
        //复制数据
        private void CopyData(MessageObject msg)
        {
            #region liuhuan
            //if (message.type == kWCMessageType.Text || message.type == kWCMessageType.Reply)
            //{
            //    Clipboard.Clear();
            //    string selected = richText.SelectedText;
            //    if (selected.Length == 0)
            //    {
            //        Clipboard.SetText(message.content);
            //        return;
            //    }
            //    //给剪切板设置对象
            //    Clipboard.SetText(message.content);
            //}
            //if (message.type == kWCMessageType.Image)
            //{
            //    Clipboard.Clear();
            //    //给剪切板设置图片对象
            //    Clipboard.SetImage(pictureBoxImage.Image);
            //}
            //if (message.type == kWCMessageType.File || message.type == kWCMessageType.Video)
            //{
            //    string fileNames = FileUtils.GetFileName(message.fileName);
            //    string path = "";
            //    path = Applicate.LocalConfigData.RoomFileFolderPath + fileNames;
            //    if (!File.Exists(message.content))
            //    {
            //        DownFile(path, message.content, true);
            //    }
            //    else
            //    {
            //        path = message.content;
            //    }
            //    Clipboard.Clear();
            //    System.Collections.Specialized.StringCollection files = new System.Collections.Specialized.StringCollection();
            //    files.Add(path);
            //    Clipboard.SetFileDropList(files);
            //}
            #endregion

            #region dzj_2020_03_24
            if (msg == null)
                return;
            switch (msg.type)
            {
                case kWCMessageType.Image:
                    if (FileUtils.IsGif(msg.content))
                    {
                        string fileName = FileUtils.GetFileName(msg.content);
                        string filePath = Applicate.LocalConfigData.ImageFolderPath + fileName;
                        if (File.Exists(filePath))
                        {
                            StringCollection i_strcoll = new StringCollection
                            {
                                filePath
                            };
                            Clipboard.SetFileDropList(i_strcoll);
                        }
                    }
                    else
                    {
                        Image image = null;
                        if (File.Exists(msg.fileName))
                        {
                            image = Image.FromFile(msg.fileName);
                        }
                        else
                        {
                            string fileName = FileUtils.GetFileName(msg.content);
                            string filePath = Applicate.LocalConfigData.ImageFolderPath + fileName;

                            if (File.Exists(filePath))
                            {
                                image = Image.FromFile(filePath);
                            }
                        }
                        if (!BitmapUtils.IsNull(image))
                        {
                            Clipboard.Clear();
                            Clipboard.SetDataObject(image, true, 3, 200);
                        }
                    }
                    break;
                case kWCMessageType.Text:
                    //清空剪切板，防止里面之前有内容
                    Clipboard.Clear();
                    //给剪切板设置图片对象
                    string content = "";
                    if (string.IsNullOrEmpty(richText.SelectedRtf))
                    {
                        if (richText.Tag is MessageObject messageObject)
                            content = messageObject.content.Trim();
                    }
                    else
                    {
                        //bool isOneself = msg.FromId.Equals(Applicate.MyAccount.userId);
                        //string selected_rtf = richText.SelectedRtf;
                        //selected_rtf = selected_rtf.Replace("{\\*\\picprop}", "");
                        //foreach (string item in EmojiCodeDictionary.GetEmojiDataIsMine().Keys)
                        //{
                        //    string emoji_code = "[" + item + "]";
                        //    selected_rtf = selected_rtf.Replace(EmojiCodeDictionary.GetEmojiRtfByCode(item, isOneself), emoji_code);
                        //}
                        using (RichTextBox ri = new RichTextBox())
                        {
                            ri.Rtf = richText.SelectedRtf;
                            content = ri.Text;
                        }
                    }
                    Clipboard.SetText(content);
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
                            //DownloadFile(msg, localPath, true);
                            HttpUtils.Instance.ShowTip("文件不存在，无法复制！");
                    }
                    else
                    {
                        StringCollection f_strcoll = new StringCollection
                        {
                            localPath
                        };
                        Clipboard.SetFileDropList(f_strcoll);
                    }
                    break;
                case kWCMessageType.Video:
                    string videoPath = msg.fileName;
                    if (!File.Exists(videoPath))
                    {
                        videoPath = Applicate.LocalConfigData.VideoFolderPath + FileUtils.GetFileName(msg.content);
                        if (!File.Exists(videoPath))
                        {
                            HttpUtils.Instance.ShowTip("视频文件不存在，无法复制！");
                        }
                    }
                    StringCollection strcoll = new StringCollection();
                    strcoll.Add(videoPath);
                    Clipboard.Clear();
                    Clipboard.SetFileDropList(strcoll);
                    break;
                default:
                    break;
            }
            #endregion
        }
        #endregion

        #region 转发消息
        private void ForwardMessage(MessageObject message)
        {

            FrmFriendSelect frm = Applicate.GetWindow<FrmFriendSelect>();
            if (frm != null)
            {
                frm.Show();
                frm.BringToFront();
                Applicate.GetWindow<FrmMain>().ShowTipBox("当前已有好友选择器正在使用，请关闭后再尝试");
                return;
            }

            frm = new FrmFriendSelect();
            frm.LoadFriendsData(1);
            frm.AddConfrmListener((dis) =>
            {
                HttpUtils.Instance.PopView(frm);

                foreach (var friend in dis)
                {
                    if (CollectUtils.EnableForward(friend.Value))
                    {
                        continue;
                    }

                    // 调用 xmpp 转发消息
                    MessageObject msg = ShiKuManager.SendForwardMessage(friend.Value, message);

                    //如果转发对象包括当前聊天对象，给UI添加消息气泡
                    if (Applicate.IsChatFriend(friend.Value.UserId))
                    {
                        //添加消息气泡通知
                        Messenger.Default.Send(msg, token: MessageActions.XMPP_UPDATE_NORMAL_MESSAGE);
                    }
                }
            });
        }

        #endregion


        #region 选中对应项
        private void ClickSelectItem(object sender, EventArgs e)
        {
            Control item = sender as Control;
            MessageObject message = item.Tag as MessageObject;
            //给全局变量赋值
            if (message.type == kWCMessageType.Image)
            {
                //给全局变量赋值
                pictureBoxImage = sender as PictureBox;

            }
            else if (message.type == kWCMessageType.Text)
            {
                richText = sender as RichTextBoxEx;
            }
            else if (message.type == kWCMessageType.Video)
            {
                pictureBoxVideo = sender as ImageViewxVideo;

            }
            else if (message.type == kWCMessageType.File)
            {
                panel_file = sender as FilePanelLeft;
                chatFile = sender as ChatItem;
            }
            else if (message.type == kWCMessageType.Reply)
            {
                richText = sender as RichTextBoxEx;
            }
        }
        #endregion

        #region 图片点击事件
        /// <summary>
        /// 打开图片查看器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MouseClie(object sender, MouseEventArgs e)
        {
            //获取当前的图片
            PictureBox picture = (PictureBox)sender;
            if (e.Button == MouseButtons.Left)
            {
                var frm = Applicate.GetWindow<FrmLookImage>();

                if (frm == null)
                {
                    frm = new FrmLookImage();
                }
                else
                {
                    frm.Activate();
                    return;
                }

                //获取Tag值
                string messageid = (string)picture.Name;
                //清除
                List<MessageObject> listmessage = ShowMsgImageList(friends.UserId);
                //索引
                int index = 0;
                for (int i = 0; i < listmessage.Count; i++)
                {
                    if (listmessage[i].messageId.Equals(messageid))
                    {
                        index = i;
                        break;

                    }
                    frm.fielSize = listmessage[i].fileSize.ToString();
                }
                frm.ShowImageList(listmessage, index);
            }

        }
        //视频播放
        public static void Vido_clike(object sender, MouseEventArgs e)
        {
            PictureBox picture = (PictureBox)sender;
            if (e.Button == MouseButtons.Left)
            {
                FrmVideoFlash frm = FrmVideoFlash.CreateInstrance();
                frm.VidoShowList((MessageObject)picture.Tag);
                frm.Show();
            }
        }
        #endregion

        #region 数据库查询
        //加载所有
        public static List<MessageObject> ShowMsgList(string toUserid)
        {
            //分页加载数据
            List<MessageObject> messages = MessageObjectDao.Instance.GetHistoryMessagesAll(toUserid,  1);
            return messages;
        }
        //文件
        public static List<MessageObject> ShowMsgFileList(string toUserid)
        {
            //分页加载数据
            List<MessageObject> messages = MessageObjectDao.Instance.GetHistoryMessagesByType(toUserid, kWCMessageType.File, 1);
            return messages;

        }
        //ͼƬ
        public static List<MessageObject> ShowMsgImageList(string toUserid)
        {
            //分页加载数据
            List<MessageObject> messages = MessageObjectDao.Instance.GetHistoryMessagesByType(toUserid, kWCMessageType.Image, 1);
            return messages;
        }

        #endregion

        #region 提取超链接与emaijo表情
        public static void Calc_PanelWidth(Control control)
        {
            if (!(control is RichTextBoxEx richContent))
                return;

            //临时建立一个容器装入内容
            RichTextBoxEx canv_Rich = control as RichTextBoxEx;
            //先取全部Text的值
            canv_Rich.Text = richContent.Text;
            //把code转为emoji
            canv_Rich.Rtf = GetLink(canv_Rich.Text);
            canv_Rich.Font = new Font(Applicate.SetFont, 10);//用来设置字体的，一定不能少，不然会变成默认的宋体了
            richContent.Rtf = canv_Rich.Rtf;
        }
        public static string GetLink(string msgText)
        {
            RichTextBoxEx richTextBox = new RichTextBoxEx();
            richTextBox.Text = msgText;
            MatchCollection msg = Regex.Matches(msgText, @"^http://([\w-]+\.)+[\w-]+(/[\w-./?%&=]*)?$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            foreach (Match match in msg)
            {
                int str_index = richTextBox.Text.IndexOf(match.Value);
                richTextBox.SelectionStart = str_index;
                richTextBox.SelectionLength = match.Value.Length;
                richTextBox.SelectedText = "";
                richTextBox.InsertLink(match.Value);
            }

            //正则表达式

            //emajio表情
            msg = Regex.Matches(richTextBox.Text, @"\[[a-z_-]*\]", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            emoji_count = msg.Count;
            int index = 0;
            string[] newStr = new string[msg.Count];
            foreach (Match item in msg)
            {
                newStr[index] = item.Groups[0].Value;
                index++;
            }
            //循环替换code为表情图片
            for (int i = 0; i < newStr.Length; i++)
            {
                //bool isMin = friends.userId == Applicate.MyAccount.userId;
                richTextBox.Rtf = richTextBox.Rtf.Replace(newStr[i], EnjoyCodeColor.GetEmojiRtfByCode(newStr[i], Color.White));
                //  richTextBox.Rtf = richTextBox.Rtf.Replace(newStr[i], EmojiCodeDictionary.GetEmojiRtfByCode(newStr[i]));
            }
            string result = richTextBox.Rtf;
            richTextBox.Dispose();
            return result;
        }
        #endregion

        #region 查询消息记录

        public void HistorySearch(string content)
        {
            lblAll.Text = LanguageXmlUtils.GetValue("history_all", "全部");
            SelectedType(lblAll);
            is_check = lblAll;
            flpTable.Controls.Clear();
            flpTable.Visible = false;
            xListView.Visible = true;
            if (!string.IsNullOrEmpty(content))
            {

                content = content.Trim();
                if (UIUtils.IsNull(content))
                {
                    return;
                }

                xListView.ClearList();
                LodingUtils loding = new LodingUtils();
                loding.parent = xListView;
                loding.size = xListView.Size;
                loding.start();
                List<MessageObject> newListData = new List<MessageObject>();

                if (lblAll.Visible)
                {
                    page = 1;
                    newListData = MessageObjectDao.Instance.GetHistoryListByContent(friends.UserId, content, page);
                }
                else
                {
                    foreach (var item in listData)
                    {
                        if (item.isReadDel == 0 && item.content.Contains(content))
                        {
                            newListData.Add(item);
                        }
                    }
                }

                mAdapter.isclickFileButton = false;
                loding.stop();


                BindData(newListData);

                lblAll.Text = LanguageXmlUtils.GetValue("search", "搜索");
                SelectedType(lblAll);
                is_check = lblAll;
            }
            else
            {
                mAdapter.isclickFileButton = false;
                BindData(listData);

                lblAll.Text = LanguageXmlUtils.GetValue("history_all", "全部");
                SelectedType(lblAll);
                is_check = lblAll;
            }
        }
        #endregion
        public void SelectedType(Label label)
        {
            if (is_check != null)
            {
                is_check.ForeColor = Color.Black;
            }

            //点击当前
            label.ForeColor = ColorTranslator.FromHtml("#1AAD19");
        }

        #region 导出excel 文件和 txt 文件
        /// <summary>
        /// 获取文件路径
        /// </summary>
        /// <returns></returns>

        public string GetImagePath()
        {
            string personImgPath = "";
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = LanguageXmlUtils.GetValue("choose_file_path", "请选择文件路径");

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                personImgPath = dialog.SelectedPath;

            }

            return personImgPath;
        }

        /// <summary>
        /// 获取与该好友的所有聊天记录（不分页)
        /// </summary>
        /// <param name="toUserid"></param>
        /// <returns></returns>
        public static List<MessageObject> ShowAllMsgList(string toUserid)
        {
            List<MessageObject> messages = new MessageObject()
            {
                FromId = Applicate.MyAccount.userId,
                ToId = toUserid
            }.LoadRecordMsg();
            return messages;
        }

        #endregion


        private void lblNickName_TextChanged(object sender, EventArgs e)
        {
            //EQControlManager.StrAddEllipsis(lblNickName, lblNickName.Font, this.Width - 40);
        }

        private void lblNext_Click(object sender, EventArgs e)
        {
            CmsChat.Show(lblNext, 0, lblNext.Height);
        }
        /// <summary>
        /// 导出excel文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemInputEls_Click(object sender, EventArgs e)
        {
            if (!this.MenuItemInputEls.Enabled)
            { return; }
            MenuItemInputEls.Enabled = false;
            string personImgPath = "";
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = LanguageXmlUtils.GetValue("choose_file_path", "请选择文件路径");

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                personImgPath = dialog.SelectedPath;

            }

            else
            {
                return;
            }

            var frmSchedule = new FrmSchedule();
            frmSchedule.FormClosed += FrmSchedule_FormClosed;
            frmSchedule.Exportmessage(personImgPath, friends);
            frmSchedule.Compte = () =>
            {


                MenuItemInputEls.Enabled = true;
                HttpUtils.Instance.PopView(frmSchedule);
                ShowTip("导出成功");
            };
        }
        /// <summary>
        ///  导出txt文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuIteminputTxt_Click(object sender, EventArgs e)
        {
            if (!this.MenuIteminputTxt.Enabled)
            { return; }
            this.MenuIteminputTxt.Enabled = false;
            string personImgPath = "";
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = LanguageXmlUtils.GetValue("choose_file_path", "请选择文件路径");

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                personImgPath = dialog.SelectedPath;

            }

            else
            {
                return;
            }
            var frmSchedule = new FrmSchedule();
            frmSchedule.FormClosed += FrmSchedule_FormClosed;
            frmSchedule.ExportTxtmessage(personImgPath, friends);
            frmSchedule.Compte = () =>
            {
                MenuIteminputTxt.Enabled = true;
                HttpUtils.Instance.PopView(frmSchedule);
                ShowTip("导出成功");
            };
        }

        private void FrmSchedule_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!MenuIteminputTxt.Enabled)
            {
                MenuIteminputTxt.Enabled = true;
            }
            if (!MenuItemInputEls.Enabled)
            {
                MenuItemInputEls.Enabled = true;
            }

        }

        private void lblNext_MouseEnter(object sender, EventArgs e)
        {
            lblNext.BackColor = ColorTranslator.FromHtml("#E5E5E5");
        }

        private void lblNext_MouseLeave(object sender, EventArgs e)
        {
            lblNext.BackColor = Color.Transparent;
        }
    }
}

