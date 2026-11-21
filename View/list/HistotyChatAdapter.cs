using Newtonsoft.Json;
using RichTextBoxLinks;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFrmTalk.Controls;
using WinFrmTalk.Controls.CustomControls;
using WinFrmTalk.Dictionarys;
using WinFrmTalk.Model;

namespace WinFrmTalk.View.list
{
    class HistotyChatAdapter : IBaseAdapter
    {
        #region 全局变量
        private List<MessageObject> datas;
        public bool isclickFileButton;
        RichTextBoxEx richText = new RichTextBoxEx();
        PictureBox pictureBoxGif = new PictureBox();
        //PictureBox pictureBoxImage = new PictureBox();
        FilePanelLeft panel_file = new FilePanelLeft();
        Panel pnlReplay = new Panel();
        #endregion
        public FrmHistoryChat FrmHistoryChat { get; set; }
        public void BindFriendData(List<MessageObject> data)
        {
            datas = data;
        }
        public override int GetItemCount()
        {
            if (datas != null)
            {
                return datas.Count;
            }

            return 0;
        }

        public override Control OnCreateControl(int index)
        {
            int crlWidth = FrmHistoryChat.xListView.Width - 20 - 33;
            MessageObject message = datas[index];
            ChatItem chat = new ChatItem();
            if (!isclickFileButton)
            {
                //锚边
                string content = message.content;
                if (message.isReadDel == 1)
                {
                    content = "[阅后即焚消息]";
                }

                switch (message.type)
                {
                    case kWCMessageType.ImageTextSingle:
                    case kWCMessageType.ImageTextMany:
                        content = message.type == kWCMessageType.ImageTextMany ? "[多条图文]" : "[单条图文]";


                        System.Windows.Forms.Label text = new System.Windows.Forms.Label();
                        text.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                        text.Font = new Font(Applicate.SetFont, 10);
                        //  text.Size = new Size(message.BubbleWidth, message.BubbleHeight);
                        text.AutoSize = true;
                        text.Location = new Point(chat.lblName.Location.X, chat.lblName.Location.Y + chat.lblName.Height + 5);
                        text.BorderStyle = BorderStyle.None;

                        text.BackColor = Color.WhiteSmoke;

                        text.BringToFront();
                        text.Text = content;
                        text.Tag = message;

                        chat.Controls.Add(text);

                        break;
                    case kWCMessageType.Text:
                        richText = new RichTextBoxEx();
                        //richText.SelectionFont = new Font(Applicate.SetFont, 10);
                        richText.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                        richText.Font = new Font(Applicate.SetFont, 10);
                        richText.Location = new Point(chat.lblName.Location.X, chat.lblName.Location.Y + chat.lblName.Height + 5);
                        richText.BorderStyle = BorderStyle.None;
                        richText.ScrollBars = RichTextBoxScrollBars.None;
                        richText.ReadOnly = true;
                        richText.BackColor = Color.WhiteSmoke;
                        richText.Multiline = true;
                        richText.MaximumSize = new Size(crlWidth - richText.Location.X, 0);
                        richText.BringToFront();
                        richText.Text = content;
                        richText.Tag = message;
                        richText.SetReadMode();  //不显示光标
                        //超链接
                        richText.DetectUrls = true;
                        //FrmHistoryChat.Calc_PanelWidth(richText);
                        //点击超链接
                        richText.LinkClicked += (sender, e) =>
                        {
                            MessageObject msg = message.CopyMessage();
                            msg.content = e.LinkText;
                            //打开浏览器窗口
                            FrmBrowser frm = new FrmBrowser();
                            frm.BrowserShow(msg);
                        };
                        //自适应高度
                        //richText.ContentsResized += FrmHistoryChat.RichTextBox_Height;
                        FrmHistoryChat.CalculateWidthAndHeight_Text(message, crlWidth - 60);

                        if (message.isReadDel == 1)
                        {
                            richText.Size = new Size(220, message.BubbleHeight);
                        }
                        else
                        {
                            richText.Size = new Size(message.BubbleWidth + 15, message.BubbleHeight);
                        }

                        chat.Size = new Size(crlWidth, richText.Height + 55);
                        chat.Controls.Add(richText);
                        //右键菜单
                        if (message.isReadDel != 1)     //阅后即焚消息不添加右键菜单
                        {
                            FrmHistoryChat.BindMenuItemList(richText, message);
                        }

                        break;
                    case kWCMessageType.File:
                        UserFileLeft panel_file = new UserFileLeft();
                        panel_file.Size = new Size(380,panel_file.Size.Height);
                        panel_file.Anchor = AnchorStyles.Top | AnchorStyles.Left;

                        panel_file.Cursor = Cursors.Hand;
                        string fileNames = FileUtils.GetFileName(message.fileName);
                        fileNames = string.IsNullOrEmpty(message.signature) ? fileNames : message.signature;
                        fileNames = EQControlManager.StrAddEllipsis(fileNames, panel_file.lab_fileName.Font, 300);
                        panel_file.lab_fileName.Text = fileNames;
                        panel_file.lab_fileSize.Text = UIUtils.FromatFileSize(message.fileSize) + " " + message.fromUserName;
                        //panel_file.Location = new Point(70, 12);
                        panel_file.Location = new Point(chat.lblName.Location.X, chat.lblName.Location.Y);
                        panel_file.BringToFront();
                        panel_file.Tag = message;
                        FrmHistoryChat.TypeFileToImage(message.content, panel_file.lab_icon);
                        chat.lblName.Visible = false;

                        foreach (Control itemControl in panel_file.Controls)
                        {
                            itemControl.MouseClick += (sender, e) =>
                            {
                                if (e.Button == MouseButtons.Left)
                                {
                                    string filePath = message.fileName;
                                    if (!File.Exists(filePath))
                                    {
                                        string path = "";
                                        string fileName = "";
                                        path = message.content;
                                        fileName = FileUtils.GetFileName(message.content);
                                        filePath = Applicate.LocalConfigData.RoomFileFolderPath + fileName;
                                        //下载
                                        FrmHistoryChat.DownFile(filePath, path);
                                    }
                                    else
                                    {
                                        //打开文件
                                        System.Diagnostics.Process.Start(filePath);
                                    }
                                }
                            };
                            if (itemControl.Controls.Count > 0)
                                foreach (Control itemcl in itemControl.Controls)
                                    itemcl.MouseClick += (sender, e) =>
                                    {
                                        if (e.Button == MouseButtons.Left)
                                        {
                                            string filePath = message.fileName;
                                            if (!File.Exists(filePath))
                                            {
                                                string path = "";
                                                string fileName = "";
                                                path = message.content;
                                                fileName = FileUtils.GetFileName(message.content);
                                                filePath = Applicate.LocalConfigData.RoomFileFolderPath + fileName;
                                                //下载
                                                FrmHistoryChat.DownFile(filePath, path);
                                            }
                                            else
                                            {
                                                //打开文件
                                                System.Diagnostics.Process.Start(filePath);
                                            }
                                        }
                                    };
                        }
                        chat.Controls.Add(panel_file);
                        chat.Size = new Size(crlWidth, 97);
                        FrmHistoryChat.BindMenuItemList(panel_file, message);
                        break;
                    case kWCMessageType.Image:
                        //实例化控件
                        content = "";
                        chat.Size = new Size(crlWidth, 120 + 55);
                        var pictureBoxImage = new ImageViewx();
                        pictureBoxImage.Location = new Point(chat.lblName.Location.X, chat.lblName.Location.Y + chat.lblName.Height + 5);
                        pictureBoxImage.Size = new Size(120, 120);

                        ImageLoader.Instance.Load(message.content).NoCache().Into((bitmap, path) =>
                        {
                            if (!BitmapUtils.IsNull(bitmap))
                            {
                                pictureBoxImage.Image = bitmap;
                            }
                        });
                        pictureBoxImage.Dock = DockStyle.None;
                        //绑定当前的索引
                        pictureBoxImage.Name = message.messageId;
                        pictureBoxImage.Tag = message;
                        pictureBoxImage.Cursor = Cursors.Hand;
                        pictureBoxImage.MouseClick += FrmHistoryChat.MouseClie;
                        chat.Controls.Add(pictureBoxImage);
                        FrmHistoryChat.BindMenuItemList(pictureBoxImage, message);
                        break;
                        
                    case kWCMessageType.Gif:
                        //实例化控件
                        content = "";
                        chat.Size = new Size(crlWidth, 120 + 55);
                        pictureBoxGif = new PictureBox();
                        pictureBoxGif.Location = new Point(chat.lblName.Location.X, chat.lblName.Location.Y + chat.lblName.Height + 5);
                        pictureBoxGif.Size = new Size(120, 120);
                        pictureBoxGif.SizeMode = PictureBoxSizeMode.Zoom;
                        if (message.content.Contains("http"))
                        {
                            ImageLoader.Instance.Load(message.content).NoCache().Into((bitmap, path) =>
                           {
                               if (!BitmapUtils.IsNull(bitmap))
                               {
                                   pictureBoxGif.BackgroundImageLayout = ImageLayout.Zoom;
                                   pictureBoxGif.BackgroundImage = EQControlManager.ClipImage(bitmap, pictureBoxGif.Width, pictureBoxGif.Height);
                               }
                           });
                        }
                        else
                        {
                            ImageLoader.Instance.Load(Applicate.LocalConfigData.GifFolderPath + message.content).NoCache().Into(pictureBoxGif);
                        }
                        pictureBoxGif.SizeMode = PictureBoxSizeMode.Zoom;
                        pictureBoxGif.Dock = DockStyle.None;
                        //绑定当前的索引
                        pictureBoxGif.Name = message.messageId;
                        pictureBoxGif.Tag = message;
                        //pictureBoxGif.MouseClick += FrmHistoryChat.MouseClie;
                        chat.Controls.Add(pictureBoxGif);
                        FrmHistoryChat.BindMenuItemList(pictureBoxGif, message);

                        break;

                    case kWCMessageType.Video:
                        //chat.Size = new Size(564, 175);
                        chat.Size = new Size(crlWidth, 175);

                        ImageViewxVideo pictureBoxVideo = new ImageViewxVideo();
                        pictureBoxVideo.Location = new Point(chat.lblName.Location.X, chat.lblName.Location.Y + chat.lblName.Height + 5);
                        pictureBoxVideo.Size = new Size(130, 130);

                        //绑定当前的索引
                        pictureBoxVideo.Tag = message;

                        ThubImageLoader.Instance.LoadImage(message.content, pictureBoxVideo);

                        pictureBoxVideo.MouseClick += FrmHistoryChat.Vido_clike;
                        chat.Controls.Add(pictureBoxVideo);
                        FrmHistoryChat.BindMenuItemList(pictureBoxVideo, message);
                        break;
                    case kWCMessageType.Location:
                        content = "";
                        chat.Size = new Size(crlWidth, 150);
                        //实例化控件
                        Panel pnlLoction = new Panel();
                        pnlLoction.Cursor = Cursors.Hand;
                        pnlLoction.Location = new Point(chat.lblName.Location.X, chat.lblName.Location.Y + chat.lblName.Height + 5);
                        pnlLoction.Tag = message.messageId;
                        //地图
                        PictureBox pic_image = new PictureBox();
                        pic_image.Size = new Size(196, 70);
                        pic_image.Location = new Point(1, 1);
                        pic_image.SizeMode = PictureBoxSizeMode.StretchImage;
                        pic_image.Dock = DockStyle.Top;
                        ImageLoader.Instance.Load(message.content).NoCache().Into((bitmap, path) =>
                        {
                            if (!BitmapUtils.IsNull(bitmap))
                            {
                                pic_image.BackgroundImageLayout = ImageLayout.Zoom;
                                pic_image.BackgroundImage = EQControlManager.ClipImage(bitmap, pic_image.Width, pic_image.Height);
                            }
                        });
                        pnlLoction.Controls.Add(pic_image);
                        //增加布局
                        Label lab_name = new Label();
                        lab_name.BackColor = Color.White;
                        lab_name.Text = message.objectId;
                        lab_name.TextAlign = ContentAlignment.MiddleCenter;
                        lab_name.AutoSize = false;
                        lab_name.Size = new Size(200, 22);
                        lab_name.Location = new Point(1, 71);
                        pnlLoction.Controls.Add(lab_name);
                        pnlLoction.Size = new Size(200, pic_image.Height + lab_name.Height);
                        chat.Controls.Add(pnlLoction);
                        pic_image.MouseClick += (sender, e) =>
                        {
                            if (e.Button == MouseButtons.Left)
                            {
                                FrmBrowser frm = new FrmBrowser();
                                frm.BrowserShow(message);
                            }
                        };
                        break;
                    case kWCMessageType.History:
                        chat.Size = new Size(crlWidth, 81);
                        Label lblText = new Label();
                        lblText.Font = new Font(Applicate.SetFont, 10);
                        lblText.AutoSize = false;
                        lblText.Size = new Size(150, 26);
                        lblText.Location = new Point(chat.lblName.Location.X, 40);
                        lblText.Text = LanguageXmlUtils.GetValue("History", "[聊天记录]");
                        chat.Controls.Add(lblText);
                        lblText.Cursor = Cursors.Hand;
                        lblText.MouseClick += (sender, e) =>
                        {
                            if (e.Button == MouseButtons.Left)
                            {
                                //修改禅道12297
                                //FrmHistoryMsg frmHistory = new FrmHistoryMsg();
                                //frmHistory.content = message.content;
                                //frmHistory.TitleText = datas[index].toUserName;
                                //frmHistory.FromLocal = false;
                                //frmHistory.Show();
                            }
                        };
                        break;
                    case kWCMessageType.Voice:
                        chat.Size = new Size(crlWidth, 100);
                        UserFileLeft panel_voice = new UserFileLeft();

                        panel_voice.Cursor = Cursors.Hand;
                        panel_voice.lab_fileName.Text = LanguageXmlUtils.GetValue("Voice", "语音");
                        panel_voice.lab_fileSize.Text = "00:" + datas[index].timeLen.ToString().PadLeft(2, '0');
                        panel_voice.Location = new Point(chat.lblName.Location.X, chat.lblName.Location.Y + chat.lblName.Height + 5);
                        FrmHistoryChat.TypeFileToImage(datas[index].content, panel_voice.lab_icon);

                        panel_voice.Click += (sen, eve) =>
                        {


                            FrmvoiceTest frm = Applicate.GetWindow<FrmvoiceTest>();

                            if (frm == null)
                            {
                                frm = new FrmvoiceTest();
                            }
                            else
                            {
                                frm.Activate();
                                return;
                            }

                            if (frm.GetDevicde())
                            {
                                HttpUtils.Instance.ShowTip("未发现音频播放设备");
                                return;
                            }
                            frm.userVoiceplayer1.VidoShowList(new MessageObject() { content = datas[index].content, fileSize = datas[index].fileSize, timeLen = Convert.ToInt32(datas[index].timeLen) });
                            Point point = FrmHistoryChat.flpTable.PointToScreen(FrmHistoryChat.flpTable.Location);
                            frm.Location = new Point(point.X - FrmHistoryChat.flpTable.Location.X, point.Y - 20);
                            frm.BringToFront();

                            frm.Show();
                        };
                        chat.Tag = panel_voice.lab_fileName.Text;

                        chat.Controls.Add(panel_voice);
                        panel_voice.BringToFront();

                        //Label label = new Label();
                        //label.Size = new Size(300, 26);
                        //label.Location = new Point(chat.lblName.Location.X, chat.lblName.Location.Y + chat.lblName.Height + 5);
                        //label.Text = LanguageXmlUtils.GetValue("Voice", "[语音]");
                        //chat.Size = new Size(crlWidth, 81);
                        //chat.Controls.Add(label);
                        break;
                    case kWCMessageType.Reply:
                        //获取准确的尺寸
                        Dictionary<string, Size> dic = EQControlManager.CalculateWidthAndHeight_Reply(message, false, 380);
                        //Size replySize = dic["replyTextBox"];
                        Size richSize = dic["richTextBox"];

                        pnlReplay = new Panel();
                        pnlReplay.Location = new Point(chat.lblName.Location.X, chat.lblName.Location.Y + chat.lblName.Height + 5);
                        Label lblNickName = new Label();
                        lblNickName.Location = new Point(0, 4);
                        lblNickName.AutoSize = false;
                        lblNickName.Size = new Size(200, 20);
                        var data = JsonConvert.DeserializeObject<MessageObject>(message.objectId);
                        lblNickName.Text = LanguageXmlUtils.GetValue("history_reply",
                            "回复  " + data.fromUserName + ": " /*+ data.content*/, true).Replace("%s", data.fromUserName);
                        richText = new RichTextBoxEx();
                        richText.Location = new Point(0, 20);
                        richText.Font = new Font(Applicate.SetFont, 9);
                        richText.Size = new Size(richSize.Width, richSize.Height);
                        richText.BorderStyle = BorderStyle.None;
                        richText.ScrollBars = RichTextBoxScrollBars.None;
                        richText.ReadOnly = true;
                        richText.BackColor = Color.WhiteSmoke;
                        richText.Multiline = true;
                        richText.Text = content;
                        richText.Tag = message;
                        //自适应高度
                        //richText.ContentsResized += FrmHistoryChat.RichTextBox_Height;
                        //超链接
                        richText.DetectUrls = true;
                        FrmHistoryChat.Calc_PanelWidth(richText);
                        pnlReplay.Controls.Add(lblNickName);
                        pnlReplay.Controls.Add(richText);
                        pnlReplay.Size = new Size(crlWidth, richText.Height + lblNickName.Height);
                        chat.Controls.Add(pnlReplay);
                        chat.Size = new Size(crlWidth, pnlReplay.Height + 50);
                        //点击超链接
                        richText.LinkClicked += (sender, e) =>
                        {
                            MessageObject msg = message.CopyMessage();
                            msg.content = e.LinkText;
                            //打开浏览器窗口
                            FrmBrowser frm = new FrmBrowser();
                            frm.BrowserShow(msg);
                        };
                        FrmHistoryChat.BindMenuItemList(richText, message);
                        break;

                    case kWCMessageType.RedPacket://红包
                        chat.Size = new Size(crlWidth, 100);
                        UserFileLeft panel_redpack = new UserFileLeft();
                        panel_redpack.lab_fileName.Location = new Point(panel_redpack.lab_fileName.Location.X, panel_redpack.lab_fileName.Location.Y + 10);
                        panel_redpack.lab_fileName.Font = new Font("微软雅黑", 15);

                        panel_redpack.Cursor = Cursors.Hand;
                        panel_redpack.lab_fileName.Text = datas[index].content;
                        panel_redpack.lab_fileSize.Visible = false;
                        panel_redpack.Location = new Point(chat.lblName.Location.X, chat.lblName.Location.Y + chat.lblName.Height + 5);
                        FrmHistoryChat.TypeFileToImage("12", panel_redpack.lab_icon);
                        chat.Tag = panel_redpack.lab_fileName.Text;
                        chat.Controls.Add(panel_redpack);
                        break;
                    case kWCMessageType.TRANSFER://转账
                        chat.Size = new Size(crlWidth, 100);
                        UserFileLeft panel_transfer = new UserFileLeft();

                        panel_transfer.Cursor = Cursors.Hand;
                        panel_transfer.lab_fileName.Location = new Point(panel_transfer.lab_fileName.Location.X, panel_transfer.lab_fileName.Location.Y + 10);
                        panel_transfer.lab_fileName.Font = new Font("微软雅黑", 15);

                        panel_transfer.lab_fileName.Text = datas[index].content;

                        panel_transfer.lab_fileSize.Visible = false;
                        panel_transfer.Location = new Point(chat.lblName.Location.X, chat.lblName.Location.Y + chat.lblName.Height + 5);
                        FrmHistoryChat.TypeFileToImage("13", panel_transfer.lab_icon);
                        chat.Tag = panel_transfer.lab_fileName.Text;
                        chat.Controls.Add(panel_transfer);
                        break;
                    case kWCMessageType.Type_Dice://骰子
                        content = "";
                        chat.Size = new Size(crlWidth, 80);
                        LabelEx lblDice = new LabelEx();
                        lblDice.Font = Applicate.myFont;
                        lblDice.Text = "[表情]";
                        lblDice.Location = new Point(chat.lblName.Location.X, chat.lblName.Location.Y + chat.lblName.Height + 5);
                        chat.Controls.Add(lblDice);
                        break;
                    case kWCMessageType.Type_Rps://石头剪刀布
                        content = "";
                        chat.Size = new Size(crlWidth, 80);
                        LabelEx lblRps = new LabelEx();
                        lblRps.Font = Applicate.myFont;
                        lblRps.Text = "[表情]";
                        lblRps.Location = new Point(chat.lblName.Location.X, chat.lblName.Location.Y + chat.lblName.Height + 5);
                        chat.Controls.Add(lblRps);
                        break;
                        
                }
                #region 加载信息
                ImageLoader.Instance.DisplayAvatar(message.fromUserId, chat.pboxHead);
                chat.Time = TimeUtils.ChatLastTime(message.timeSend);
                chat.fileName = message.fromUserName;
                if (message.type == kWCMessageType.Text || message.type == kWCMessageType.Reply)
                {
                    chat.Tag = message.content;
                }
                if (chat.Time.Length == 2)
                {
                    chat.Time = "  " + TimeUtils.ChatLastTime(message.timeSend);
                }
                else if (chat.Time.Length == 3)
                {
                    chat.Time = "" + TimeUtils.ChatLastTime(message.timeSend);
                }
                else
                {
                    chat.Time = TimeUtils.ChatLastTime(message.timeSend);
                }
                #endregion
            }
            else
            {
                chat = FrmHistoryChat.FileInfonmation(message);
            }
            //chat.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            return chat;
        }

        public override int OnMeasureHeight(int index)
        {
            int total_height = 0;
            if (datas[index].type == kWCMessageType.Text)
            {
                //EQControlManager.CalculateWidthAndHeight_Text(datas[index], false, 420);
                FrmHistoryChat.CalculateWidthAndHeight_Text(datas[index], FrmHistoryChat.xListView.Width - 20 - 33 - 60);
                total_height = datas[index].BubbleHeight + 55;
            }

            else if (datas[index].type == kWCMessageType.Image || datas[index].type == kWCMessageType.Video || datas[index].type == kWCMessageType.Gif)
            {
                total_height = 120 + 55;
            }

            else if (datas[index].type == kWCMessageType.Location)
            {
                //total_height = datas[index].BubbleHeight + 110;
                total_height = 145;
            }

            else if (datas[index].type == kWCMessageType.File)
            {
                total_height = 100;
            }

            else if (datas[index].type == kWCMessageType.History)
            {
                total_height = 81;
            }

            else if (datas[index].type == kWCMessageType.Reply)
            {
                Dictionary<string, Size> dic = EQControlManager.CalculateWidthAndHeight_Reply(datas[index], false, 380);
                Size ri_size = dic["richTextBox"];
                int ri_height = ri_size.Height;
                datas[index].BubbleHeight = 20 + ri_height + 10;
                //}
                total_height = datas[index].BubbleHeight + 50;
            }
            else if (datas[index].type == kWCMessageType.RedPacket || datas[index].type == kWCMessageType.Voice || datas[index].type == kWCMessageType.TRANSFER)
            {
                total_height = 100;
            }

            else
            {
                total_height = datas[index].BubbleHeight + 70;
            }
            return total_height;
        }

        public override void RemoveData(int index)
        {
            datas.RemoveAt(index);
        }
        /// <summary>
        /// 传递含有emoji code的文本，返回转化为图片后的rtf字符串
        /// </summary>
        /// <param name="ric_text">含有emoji code的文本</param>
        /// <param name="bg_cloor">填充绘画底色的背景色</param>
        /// <param name="code_count">合计有多少emoji_code转为png</param>
        /// <returns></returns>
        private string GetEmoji(string ric_text, MessageObject messageObject)
        {
            try
            {
                RichTextBoxEx richTextBox = new RichTextBoxEx();
                richTextBox.Text = ric_text;

                bool isParallel = false;
                //是否为自己发送的消息
                bool isMin = messageObject.fromUserId == Applicate.MyAccount.userId;
                string rtf = richTextBox.Rtf;

                string[] rtfs = richTextBox.Rtf.Split(']');
                Parallel.For(0, rtfs.Length, (index, loopState) =>
                {
                    if (string.IsNullOrEmpty(rtfs[index]) || rtfs[index].IndexOf("[") < 0)
                        loopState.Break();
                    //匹配符合规则的表情code
                    MatchCollection matchs = Regex.Matches(rtfs[index] + "]", @"\[[a-z_-]*\]", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    if (matchs.Count > 0)
                    {
                        string emoji_rtf = EmojiCodeDictionary.GetEmojiRtfByCode(matchs[0].Value, isMin);
                        if (!string.IsNullOrEmpty(emoji_rtf))
                        {
                            isParallel = true;
                            rtfs[index] = (rtfs[index] + "]").Replace(matchs[0].Value, emoji_rtf);
                        }
                    }
                });

                if (isParallel)
                {
                    string new_rtf = string.Empty;
                    for (int i = 0; i < rtfs.Length; i++)
                    {
                        new_rtf += rtfs[i];
                    }
                    richTextBox.Rtf = new_rtf;
                }

                //把链接改为超链接
                MatchCollection matchs_link = Regex.Matches(richTextBox.Text, @"^http://([\w-]+\.)+[\w-]+(/[\w-./?%&=]*)?$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                foreach (Match match in matchs_link)
                {
                    int str_index = richTextBox.Text.IndexOf(match.Value);  //查找匹配超链接格式的索引位置
                    richTextBox.SelectionStart = str_index;     //设置复选文本框的光标位置
                    richTextBox.SelectionLength = match.Value.Length;   //设置选中的字符数量
                    richTextBox.SelectedText = "";      //去除文本中原本的超链接字符
                    richTextBox.InsertLink(match.Value);    //插入超链接格式和文本
                }

                string result = richTextBox.Rtf;
                richTextBox.Dispose();
                return result;
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
