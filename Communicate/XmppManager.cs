using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.Xml.Dom;
using Newtonsoft.Json;
using PBMessage;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using TestSocket.socket;
using WinFrmTalk.Communicate;
using WinFrmTalk.Helper;
using WinFrmTalk.Helper.MVVM;
using WinFrmTalk.Model;
using WinFrmTalk.Model.dao;
using WinFrmTalk.socket;

using Message = agsXMPP.protocol.client.Message;

namespace WinFrmTalk
{
    /// <summary>
    /// Xmpp控制类
    /// </summary>
    public class XmppManager
    {

        private bool mConnected; // 是否连接成功过
        private bool isOpenBatch; // 是否开启批量回执
        private int timeCount;
        private StringBuilder messageIds;
        private string mRequestId;
        private const string SYNC_LOGIN_PASSWORD = "sync_login_password";
        private const string SYNC_PAY_PASSWORD = "sync_pay_password";
        private const string SYNC_PRIVATE_SETTINGS = "sync_private_settings";
        private const string SYNC_LABEL = "sync_label";

        private System.Timers.Timer MessageLooper = new System.Timers.Timer();
        private string mLoginUserId;

        private SocketCore mSocketCore;

        public SocketConnectionState ConnectState
        {
            get
            {
                if (mSocketCore == null)
                {
                    return SocketConnectionState.Disconnected;
                }

                return mSocketCore.ConnectState;
            }
        }

        #region 构造函数

        /// <summary>
        /// 构造函数：初始化XmppClientConnection对象
        /// </summary>
        public XmppManager()
        {
            // 初始化消息队列
            MessageLooper.Interval = 500; // 0.5秒没有消息就去更新界面
            MessageLooper.Elapsed += new ElapsedEventHandler(OnMessageLoop);
            MessageLooper.AutoReset = false;   //设置是执行一次（false）还是一直执行(true)；
            MessageLooper.Enabled = true;     //是否执行System.Timers.Timer.Elapsed事件；

            string ip = Applicate.URLDATA.data.XMPPHost;
            string userId = Applicate.MyAccount.userId;
            string token = Applicate.Access_Token;

            // 初始化 设置ip port
            mSocketCore = new SocketCore(ip, 5666);
            // 设置登录用户
            mSocketCore.SetLoginUser(token, userId);
            // 设置ping间隔时间 秒
            mSocketCore.PingTime = Applicate.URLDATA.data.xmppPingTime;
            // 设置socket状态回调
            mSocketCore.OnStateChanged += OnSocketStateChanged;
            // 设置收到消息回调
            mSocketCore.OnMessage += XmppCon_OnMessage;
            // 设置消息回执回调
            mSocketCore.OnReceipt += XmppCon_OnReceipt;
            // 设置连接断开回调
            //mSocketCore.OnClose += XmppCon_OnClose;

            mSocketCore.Connect();

        }

        #endregion

        #region 您已在其他设备登录
        /// <summary>
        /// Xmpp连接状态改变时 
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private int _loginConflictHandled = 0;

        private void LoginConflict()
        {
            // v23.10 修复（代码审查 B5）：单次处理保护 + 切 UI 线程 + 窗体有效性检查，
            // 防止重复状态通知导致多次弹窗/并发重启/循环退出
            if (Interlocked.Exchange(ref _loginConflictHandled, 1) != 0)
            {
                return;
            }
            var frm = Applicate.GetWindow<FrmMain>();
            if (frm == null || frm.IsDisposed || frm.Disposing)
            {
                return;
            }
            Action act = () =>
            {
                try
                {
                    var res = MessageBox.Show(frm, "账号已在其他设备登录，是否重新登录？", "其他设备登录提示", MessageBoxButtons.YesNo);
                    ShiKuManager.ApplicationExit();
                    if (res == DialogResult.Yes)
                    {
                        Application.Restart();
                    }
                }
                catch (Exception ex)
                {
                    LogUtils.Log("LoginConflict error: " + ex.Message);
                }
            };
            if (frm.InvokeRequired)
            {
                if (frm.IsHandleCreated)
                {
                    frm.BeginInvoke(act);
                }
            }
            else
            {
                act();
            }
        }
        #endregion

        #region --------------OnXmppStateChanged----------------
        /// <summary>
        /// Xmpp连接状态改变时 
        /// <param name="sender"></param>
        /// <param name="state"></param>
        public void OnSocketStateChanged(SocketConnectionState state)
        {

            LogUtils.Log("------OnSocketStateChanged：" + state);
            Messenger.Default.Send(state, MessageActions.XMPP_UPDATE_STATE);
            switch (state)
            {
                case SocketConnectionState.Disconnected:
                    break;
                case SocketConnectionState.Connecting:
                    break;
                case SocketConnectionState.Connected:
                    break;
                case SocketConnectionState.Authenticating:
                    break;
                case SocketConnectionState.Authenticated:
                    _loginConflictHandled = 0; // v23.10 重置登录冲突标记，下次冲突可再次提示
                    mLoginUserId = Applicate.MyAccount.userId;
                    // 去下载最近消息列表
                    DownChatList();
                    // 同步其他设备操作数据
                    SyscMultiDeviceData();
                    break;
                case SocketConnectionState.LoginConflict:
                    LoginConflict();
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region XMPP连接关闭时
        private void XmppCon_OnClose(SocketConnectionState state)
        {

        }

        public void Disconnect()
        {
            if (mSocketCore != null)
            {
                mSocketCore.Disconnect();
            }
        }
        #endregion

        #region 收到消息回执
        /// <summary>
        /// 收到消息回执  
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="success"></param>
        public void XmppCon_OnReceipt(string messageid, int success)
        {
            if (success == 1)
            {
                XmppReceiptManager.Instance.OnReceiveReceipt(XmppReceiptManager.RECEIPT_YES, messageid);
            }
            else if (success == -2)
            {
                // 敏感词
                XmppReceiptManager.Instance.OnReceiveReceipt(XmppReceiptManager.RECEIPT_ERR_DIS, messageid);
            }
            else
            {
                XmppReceiptManager.Instance.OnReceiveReceipt(XmppReceiptManager.RECEIPT_ERR, messageid);
            }
        }
        #endregion

        #region XmppCon_OnMessage接收到消息---------------------------
        /// <summary>
        /// Xmpp接收到信息时  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="xmppMsg"></param>
        public void XmppCon_OnMessage(ChatMessage chat)
        {
            // 2026-09-01 多设备修复: 消息处理异常兜底, 防止异常导致程序崩溃闪退(异常时记录日志, 不影响正常消息流程)
            try
            {
            //LogUtils.Log("XmppCon_OnMessage：  " + xmppMsg);

            // 协议消息ChatMessage 转换成 pc消息实体MessageObject
            MessageObject message = ToMessageObject(chat);
            if (message == null)
            {
                return;
            }
            // 已送达: 收到的消息都是成功的消息
            message.isSend = 1;

            // 判断是否离线消息
            bool isOfflieMsg = chat.messageHead.offline;

            if ("pc".Equals(message.toUserId))
            {
                message.toUserId = mLoginUserId;
                message.toUserName = Applicate.MyAccount.nickname;
            }

            // 处理群聊漫游任务机制
            CreateGroupDelayTask(message, isOfflieMsg);

            // 本地已存在的消息不处理
            if (message.IsExist())
            {
                return;
            }

            // 消息解密
            if (message.isEncrypt > 0)
            {
                SkSSLUtils.DecryptXmppMessage(message, message.isGroup == 1);

                if (message.isEncrypt > 0)
                {
                    // 标记消息解密失败
                    message.IsDecryptFail = 1;
                }
            }


            // 一条正常的消息 做下一步的逻辑处理
            ReceMessage(message, isOfflieMsg);
            }
            catch (Exception ex)
            {
                // 2026-09-01 多设备修复: 消息处理异常兜底, 不再导致程序崩溃闪退(记录日志供后续精确定位)
                try
                {
                    LogUtils.Log("XmppCon_OnMessage EXCEPTION: " + ex);
                }
                catch (Exception) { }
                Console.WriteLine("XmppCon_OnMessage EXCEPTION: " + ex.Message);
            }
        }

        // 群漫游机制
        private void CreateGroupDelayTask(MessageObject chatMessage, bool isOfflieMsg)
        {
            // 群聊离线消息
            if (chatMessage.isGroup == 1 && isOfflieMsg)
            {
                // 离线100条消息，导致额外的100条sql查询
                var task = MsgRoamTaskDao.Instance.QueryLastTask(chatMessage.ChatJid);

                if (task != null)
                {
                    if (task.startMsgId.Equals(chatMessage.messageId))
                    {
                        MsgRoamTaskDao.Instance.DeleteTask(task.taskId, task.userId);
                    }

                    if (task.endTime == 0)
                    {
                        MsgRoamTaskDao.Instance.UpdateTaskEndTime(task.taskId, task.userId, chatMessage.timeSend);
                    }
                }
            }
        }

        #endregion

        // xmpp 收到消息
        private void ReceMessage(MessageObject msg, bool isOfflieMsg)
        {
            if (msg.type == kWCMessageType.IsRead)
            {
                ProcessReadMessage(msg);//处理已读消息
                return;
            }

            // 正在输入
            if (msg.type == kWCMessageType.Typing)
            {
                if (!msg.IsMySend() && !isOfflieMsg)
                {
                    Messenger.Default.Send(msg, MessageActions.XMPP_UPDATE_NORMAL_MESSAGE);
                }

                return;
            }

            //申请建立会话
            if (msg.type == kWCMessageType.CustomerBuiltConnnect)
            {
                var content = "您好我是客户" + msg.fromUserName + ",很高兴为您服务，请问有什么问题可以帮您的?";
                var friend = msg.GetFriend();

                ShiKuManager.SendTextMessage(friend, content);
                return;
            }

            // 结束会话
            if (msg.type == kWCMessageType.CustomerEndConnnect)
            {
                var friend = msg.GetFriend();
                // 从朋友表中删除
                friend.DeleteByUserId();
                // 通知界面更新
                MessageObject msg1 = new MessageObject() { FromId = friend.UserId, ToId = mLoginUserId, type = kWCMessageType.RoomExit };
                Messenger.Default.Send(msg1, MessageActions.XMPP_UPDATE_ROOM_DELETE);
                return;
            }

            // 双向删除
            if (msg.type == kWCMessageType.SYNC_CLEAN)
            {
                Messenger.Default.Send(msg.fromUserId, MessageActions.CLEAR_FRIEND_MSGS);

                Messenger.Default.Send(msg.fromUserId, MessageActions.CLEAR_FRIEND_MSGS);

                HttpUtils.Instance.ShowTip(msg.fromUserName + " 清除了您与他的聊天记录");
                return;
            }

            // 截图
            if (msg.type == kWCMessageType.TYPE_SCREENSHOT)
            {
                HttpUtils.Instance.ShowTip(msg.fromUserName + "正在对您的阅后即焚消息截图");
                return;
            }

            //群控制消息
            if (msg.type >= kWCMessageType.RoomMemberNameChange && msg.type <= kWCMessageType.RoomNoticeEdit)
            {

                if (msg.type == kWCMessageType.TYPE_SERVICE_LOCK_GROUP)
                {
                    ProcessServiceLockMessage(msg, isOfflieMsg);
                }
                else
                {
                    //处理群聊控制消息
                    ProcessGroupManageMessage(msg, isOfflieMsg);
                }

                return;
            }

            //新朋友验证消息
            if (msg.type >= kWCMessageType.FriendRequest && msg.type <= kWCMessageType.PhoneContactToFriend)
            {
                ProcessFriendVerifyMessage(msg);//新朋友验证消息
                return;
            }

            //群文件消息
            if (msg.type >= kWCMessageType.RoomFileUpload && msg.type <= kWCMessageType.RoomFileDownload)
            {
                ProcessGroupFileMessage(msg);//群文件消息

                return;
            }

            //音视频消息
            if (msg.type >= kWCMessageType.AudioChatAsk && msg.type <= kWCMessageType.AudioMeetingSetSpeaker)
            {
                ProcessPhoneCallMessage(msg, isOfflieMsg);//音视频消息
                return;
            }


            // 特殊的消息类型显示 。 合并转发，戳一戳，图文
            if (msg.type >= kWCMessageType.ImageTextSingle && msg.type <= kWCMessageType.TYPE_93 && msg.type != kWCMessageType.Link)
            {
                ProcessSpecialMessage(msg);//特殊的消息类型显示
                return;
            }

            // 多设备同步消息
            if (msg.type >= kWCMessageType.Device_SYNC_OTHER && msg.type <= kWCMessageType.Device_SYNC_GROUP)
            {
                if (msg.InsertData() > 0) // 数据库成功插入
                {
                    ProcessDeviceSyncMessage(msg, isOfflieMsg);//多设备同步消息
                }
                return;
            }

            // 端到端加密相关消息
            if (msg.type >= kWCMessageType.TYPE_SECURE_REFRESH_KEY && msg.type <= kWCMessageType.TYPE_SECURE_NOTIFY_REFRESH_KEY)
            {
                /* 端到端加密相关消息 离线消息不处理 */
                if (!isOfflieMsg && Applicate.ENABLE_ASY_ENCRYPT)
                {
                    ProcessSecureMessage(msg);
                }

                return;
            }

            // 支付凭证
            if (msg.type == kWCMessageType.TYPE_PAY_CERTIFICATE)
            {
                return;
            }

            // 撤回消息
            if (msg.type == kWCMessageType.Withdraw)
            {
                /* 此处应修改消息内容为"xx撤回了一条消息" */
                ProcessRecallMessage(msg);
                return;
            }

            // 后台管理相关消息
            if (msg.type >= kWCMessageType.NewContactRegistered && msg.type <= kWCMessageType.TYPE_DELETE_SELF)
            {
                ProcessServiceManagerMessage(msg);
                return;
            }

            // 后台锁定
            if (msg.type == kWCMessageType.TYPE_SERVICE_LOCK_USER)
            {
                ProcessServiceLockMessage(msg, isOfflieMsg);
                return;
            }

            // 红包消息
            if (msg.type == kWCMessageType.RedPacket || msg.type == kWCMessageType.TRANSFER)
            {
                if (!Applicate.ENABLE_RED_PACKAGE)//没有开启红包
                {
                    msg.content = msg.type == kWCMessageType.RedPacket ? "收到红包请在手机端查看" : "收到转账请在手机端查看";
                    UpdateFriendLastContentTip(msg);
                    return;
                }
            }

            // 容错处理
            if (string.IsNullOrEmpty(msg.content))
            {
                return;
            }
            
            // 能走到这里就是普通消息了
            if (msg.InsertData() > 0)//数据库成功插入
            {
                int result = msg.UpdateLastSend();
                if (result == -1)
                {
                    var frd = msg.GetLastFriend();
                    // 2026-09-01 澶氳澶囦慨澶? 澶氳澶囧悓姝ュ洖浼犳秷鎭紱鏈湴鏃犺鏃?/鑱婂ぉ璁板綍鏃剁┖鍊煎閬垮厤寮傚父
                    if (frd != null)
                    {
                        frd.Content = msg.content;
                        HandlerNotRoomMethod(frd, true);
                    }
                }
                else
                {
                    NotifactionNormalMessage(msg);//  通知UI  //通知各页面收到消息
                }
            }


            // @群成员消息 必须要等消息存了之后才能去发广播
            if (msg.isGroup == 1 && msg.type == kWCMessageType.Text && (!string.IsNullOrEmpty(msg.objectId)))
            {
                Friend room = msg.GetFriend();
                if (room != null && room.IsAtMe == 0)
                {
                    if (room.UserId.Equals(msg.objectId))
                    {
                        // @全体成员
                        room.UpdateAtMeState(2);
                        LocalDataUtils.SetStringData(room.UserId + "GROUP_AT_MESSAGEID" + msg.myUserId, msg.messageId);
                        Messenger.Default.Send(room, MessageActions.ROOM_UPDATE_AT_ME);//通知界面刷新
                    }
                    else if (msg.objectId.Contains(msg.myUserId))
                    {
                        // 我被@了
                        room.UpdateAtMeState(1);
                        LocalDataUtils.SetStringData(room.UserId + "GROUP_AT_MESSAGEID" + msg.myUserId, msg.messageId);
                        Messenger.Default.Send(room, MessageActions.ROOM_UPDATE_AT_ME);//通知界面刷新
                    }
                }
            }
        }
        
        // xmpp发送消息
        public void SendMessage(MessageObject msg)
        {
            if (!mSocketCore.Authenticated())
            {
                msg.isSend = -1;
                msg.UpdateIsSend(-1, msg.messageId);
                XmppReceiptManager.Instance.PushFailMessage(msg);
                // 更新UI消息发送失败标志 消息已发送失败 参数 
                Messenger.Default.Send(msg, MessageActions.XMPP_UPDATE_SEND_FAILED);
                return;
            }


            if (!HttpUtils.Instance.AvailableNetwork())
            {
                msg.isSend = -1;
                msg.UpdateIsSend(-1, msg.messageId);
                XmppReceiptManager.Instance.PushFailMessage(msg);
                // 更新UI消息发送失败标志 消息已发送失败 参数 
                Messenger.Default.Send(msg, MessageActions.XMPP_UPDATE_SEND_FAILED);
                return;
            }

            MessageObject chatMessage = msg.CopyMessage();

            // 消息加密传输
            if (!NotEncrypt(chatMessage))
            {
                var friend = new Friend() { UserId = msg.toUserId }.GetByUserId();
                SkSSLUtils.EncryptMessage(friend, chatMessage);
                msg.signature = chatMessage.signature;
            }
            else
            {
                chatMessage.isEncrypt = 0;
            }

            // 是否是发给我的设备的消息
            if ("android".Equals(msg.toUserId) || "ios".Equals(msg.toUserId) || "web".Equals(msg.toUserId) || "mac".Equals(msg.toUserId))
            {
                chatMessage.toUserName = msg.toUserId;
                chatMessage.toUserId = msg.fromUserId;
            }

            // 修改禅道bug # 8033
            if (chatMessage.type == kWCMessageType.File)
            {
                chatMessage.fileName = FileUtils.GetFileName(chatMessage.fileName);
            }

            ChatMessage chat = ToChatMessage(chatMessage);

            // 添加到回执管理器
            XmppReceiptManager.Instance.AddWillSendMessage(msg);
            mSocketCore.SendMessage(chat, Command.COMMAND_CHAT_REQ);
            Console.WriteLine("发送消息" + chat.content);
        }

        private bool NotEncrypt(MessageObject chatMessage)
        {
            if (UIUtils.IsNull(chatMessage.content))
            {
                return true;
            }

            if (chatMessage.type == kWCMessageType.Reply)
            {
                // 修改禅道9407 回复消息需要加密
                return false;
            }

            // 大于10类型的消息全部不加密
            if (chatMessage.type >= kWCMessageType.Remind)
            {
                return true;
            }

            if (chatMessage.type == kWCMessageType.IsRead)
            {
                return true;
            }

            if (chatMessage.type == kWCMessageType.TYPE_SECURE_SEND_KEY)
            {
                return true;
            }

            // 转发给其他端的消息也不加密
            if (mLoginUserId.Equals(chatMessage.toUserId))
            {
                return true;
            }

            return false;
        }

        #region ========================================================消息处理模块====================================================
        /// <summary>
        /// 新朋友验证相关消息处理
        /// </summary>
        /// <param name="msg">消息</param>
        private void ProcessFriendVerifyMessage(MessageObject msg)
        {
            if (msg.IsExist())
            {
                return;
            }

            // 做一个容错处理
            var tmpFriend = msg.GetFriend();
            if (tmpFriend == null || string.IsNullOrEmpty(tmpFriend.UserId))
            {
                tmpFriend = new Friend()
                {
                    UserId = msg.fromUserId,
                    IsGroup = 0,
                    NickName = msg.fromUserName,
                };

                tmpFriend.InsertAuto();//添加对应好友至数据库
            }


            switch (msg.type)
            {
                case kWCMessageType.DeleteFriend://被删除
                    // 显示提示
                    if (string.Equals(msg.fromUserId, msg.myUserId))
                    {
                        HttpUtils.Instance.ShowTip("已删除" + tmpFriend.NickName);
                        // 设置把删除
                        tmpFriend.UpdateFriendState(tmpFriend.UserId, Friend.STATUS_16);
                    }
                    else
                    {
                        HttpUtils.Instance.ShowTip("我已被" + tmpFriend.NickName + "删除");
                        // 设置为被删除
                        tmpFriend.UpdateFriendState(tmpFriend.UserId, Friend.STATUS_17);
                    }

                    // 通知界面刷新
                    Messenger.Default.Send(tmpFriend, MessageActions.DELETE_FRIEND);
                    // 清空消息记录
                    msg.DeleteTable();
                    // 更新新的朋友未读数量
                    Messenger.Default.Send(Friend.ID_NEW_FRIEND, FriendListLayout.NOTIFY_FRIENDLIST_UNREAD_COUNT);
                    break;
                case kWCMessageType.BlackFriend://被拉黑

                    // 显示提示
                    if (string.Equals(msg.fromUserId, msg.myUserId))
                    {
                        HttpUtils.Instance.ShowTip("已拉黑" + tmpFriend.NickName);
                        // 设置为被拉黑
                        tmpFriend.UpdateFriendState(tmpFriend.UserId, Friend.STATUS_18);
                    }
                    else
                    {
                        HttpUtils.Instance.ShowTip("我已被" + tmpFriend.NickName + "拉黑");
                        // 设置为被拉黑
                        tmpFriend.UpdateFriendState(tmpFriend.UserId, Friend.STATUS_19);
                    }

                    // 通知界面刷新
                    Messenger.Default.Send(tmpFriend, MessageActions.ADD_BLACKLIST);

                    // 清空列表 拉黑操作不在删除消息表#9559
                    //msg.DeleteTable();
                    // 更新新的朋友未读数量
                    Messenger.Default.Send(Friend.ID_NEW_FRIEND, FriendListLayout.NOTIFY_FRIENDLIST_UNREAD_COUNT);
                    break;
                case kWCMessageType.RequestRefuse://对方回话
                case kWCMessageType.FriendRequest://对方请求添加我
                                                  // 更新新的朋友未读数量
                    Messenger.Default.Send(Friend.ID_NEW_FRIEND, FriendListLayout.NOTIFY_FRIENDLIST_UNREAD_COUNT);
                    break;
                case kWCMessageType.RequestFriendDirectly://直接添加好友
                case kWCMessageType.RequestAgree://被同意
                case kWCMessageType.CancelBlackFriend://被对方取消黑名单
                case kWCMessageType.PhoneContactToFriend://被对方从手机联系人直接添加成好友

                    MessageObject tip = msg.CopyMessage();
                    tip.type = kWCMessageType.Remind;
                    tip.content = "你们已经成为好友，快来聊天吧";
                    if (tip.InsertData() == 1)
                    {
                        Console.WriteLine("保存成功");
                    }//保存提示消息

                    tmpFriend.BecomeFriend(msg.type);
                    // 更新好友信息
                    HandleFriendUpdate(tmpFriend.UserId);

                    // 更新聊天记录页
                    Messenger.Default.Send(tip, MessageActions.XMPP_UPDATE_NORMAL_MESSAGE);
                    Messenger.Default.Send(tip, MessageActions.XMPP_SHOW_SINGLE_MESSAGE);

                    //添加至好友列表
                    Messenger.Default.Send(tmpFriend.UserId, FriendListLayout.ADD_EXISTS_FRIEND);
                    // 更新新的朋友未读数量
                    Messenger.Default.Send(Friend.ID_NEW_FRIEND, FriendListLayout.NOTIFY_FRIENDLIST_UNREAD_COUNT);

                    break;
                default:
                    break;
            }

            // 刷新新的朋友界面
            if (string.Equals(msg.fromUserId, msg.myUserId))
            {
                Messenger.Default.Send(msg, MessageActions.XMPP_UPDATE_VERIFY_RECEPIT);// 更新UI消息发送失败标志 消息已发送失败 参数 messageObject
            }
            else
            {
                NotifactionVerifyMessage(msg);
            }
        }


        // 撤回消息处理
        private void ProcessRecallMessage(MessageObject msg)
        {
            // 存库防止消息重复处理
            //msg.InsertData();
            string content = null;
            msg.messageId = msg.content;
            MessageObject oMsg = msg.GetMessageObject();


            if (string.Equals(msg.fromUserId, msg.myUserId))
            {
                if (string.Equals(oMsg.fromUserId, oMsg.myUserId))
                {
                    content = "你撤回了一条消息";
                }
                else
                {
                    content = "你撤回了 " + UIUtils.QuotationName(oMsg.fromUserName) + "的一条消息";
                }
            }
            else
            {
                if (string.Equals(oMsg.fromUserId, oMsg.myUserId))
                {
                    content = UIUtils.QuotationName(msg.fromUserName) + "撤回了一条你的消息";
                }
                else
                {
                    if (string.Equals(oMsg.fromUserId, msg.fromUserId))
                    {
                        content = UIUtils.QuotationName(msg.fromUserName) + "撤回了一条消息";
                    }
                    else
                    {
                        content = UIUtils.QuotationName(msg.fromUserName) + "撤回了一条成员的消息";
                    }
                }
            }

            oMsg.content = content;
            oMsg.type = kWCMessageType.Remind;

            // 去更新撤回消息
            if (oMsg.UpdateData() > 0)
            {
                NotifactionRecallMessage(oMsg);
            }

            Friend friend = msg.GetFriend();
            if (friend.UpdateLastContent(content, msg.timeSend) > 0)
            {
                Messenger.Default.Send(msg, MessageActions.XMPP_SHOW_SINGLE_MESSAGE);
            }
        }

        /// <summary>
        /// 处理音视频聊天消息
        /// </summary>
        /// <param name="msg"></param>
        private void ProcessPhoneCallMessage(MessageObject msg, bool delay)
        {
            if (msg.IsExist())
            {
                return;
            }


            if (msg.type == kWCMessageType.AudioChatAsk || msg.type == kWCMessageType.VideoChatAsk)
            {
                if (string.Equals(msg.fromUserId, msg.myUserId))
                {
                    return;
                }
            }

            // 修改禅道7474问题，web挂断消息出现在公众号的问题
            if ("10000".Equals(msg.toUserId))
            {
                return;
            }

            // 离线消息不在处理 禅道11263  time:2020-3-16 16:03:24
            if (!delay)
            {
                Messenger.Default.Send(msg, MessageActions.XMPP_UPDATE_MEETING_MESSAGE);//
            }


            switch (msg.type)
            {
                case kWCMessageType.AudioChatCancel://

                    if (msg.timeLen == 1)
                    {
                        msg.content = "已拒绝";
                    }
                    else
                    {
                        msg.content = LanguageXmlUtils.GetValue("AudioChatCancel", "取消了语音通话");
                    }

                    UpdateFriendLastContentTip(msg);
                    break;
                case kWCMessageType.AudioChatEnd://
                    msg.content = "结束了语音通话：" + TimeUtils.FromatCountdown(msg.timeLen);
                    UpdateFriendLastContentTip(msg);
                    break;
                case kWCMessageType.VideoChatCancel://
                    if (msg.timeLen == 1)
                    {
                        msg.content = "已拒绝";
                    }
                    else
                    {
                        msg.content = LanguageXmlUtils.GetValue("VideoChatCancel", "取消了视频通话");
                    }

                    UpdateFriendLastContentTip(msg);
                    break;
                case kWCMessageType.VideoChatEnd://
                    msg.content = "结束了视频通话：" + TimeUtils.FromatCountdown(msg.timeLen);
                    UpdateFriendLastContentTip(msg);
                    break;
                case kWCMessageType.AudioMeetingSetSpeaker://
                    msg.content = "对方忙线中";
                    UpdateFriendLastContentTip(msg);
                    break;
            }
        }

        /// <summary>
        /// 处理红包相关消息
        /// </summary>
        /// <param name="msg"></param>
        private void ProcessMoenyMessage(MessageObject msg)
        {
            switch (msg.type)
            {

                case kWCMessageType.RedBack:
                    msg.content = "有红包退回请在手机端查看";
                    break;
                case kWCMessageType.TYPE_TRANSFER_RECEIVE://删除群文件
                    msg.content = "转账已被领取请在手机端查看";
                    break;
                case kWCMessageType.TYPE_93:
                    msg.content = "收到收款消息请在手机端查看";
                    break;
                case kWCMessageType.TYPE_TRANSFER_BACK:
                    msg.content = "转账已退回请在手机端查看";
                    break;
                default:
                    msg.content = "暂不支持此类型的消息显示";
                    break;
            }

            UpdateFriendLastContentTip(msg);
        }

        /// <summary>
        /// 处理红包相关消息
        /// </summary>
        /// <param name="msg"></param>
        private void ProcessSpecialMessage(MessageObject msg)
        {
            // 红包 转账相关消息 
            if (msg.type >= kWCMessageType.RedBack && msg.type <= kWCMessageType.TYPE_93)
            {
                switch (msg.type)
                {
                    case kWCMessageType.RedBack:
                        msg.content = "有红包退回请在手机端查看";
                        break;
                    case kWCMessageType.TYPE_TRANSFER_RECEIVE:
                        msg.content = "转账已被领取请在手机端查看";
                        break;
                    case kWCMessageType.TYPE_93:
                        msg.content = "收到收款消息请在手机端查看";
                        break;
                    case kWCMessageType.TYPE_TRANSFER_BACK:
                        msg.content = "转账已退回请在手机端查看";
                        break;
                    case kWCMessageType.TYPE_CODE_ACCOUNT:
                        msg.fromUserName = "系统通知";
                        msg.content = "暂不支持此类型的消息显示";
                        break;
                    case kWCMessageType.TYPE_CODE_PAY:
                        msg.content = "暂不支持此类型的消息显示";
                        break;
                    default:
                        msg.content = "暂不支持此类型的消息显示";
                        break;
                }

                UpdateFriendLastContentTip(msg);
                return;
            }

            // 特殊消息类型显示
            switch (msg.type)
            {
                case kWCMessageType.ImageTextSingle:
                case kWCMessageType.ImageTextMany:
                    if (msg.InsertData() > 0)//数据库成功插入
                    {
                        msg.UpdateLastSend();
                        NotifactionNormalMessage(msg);//  通知UI  //通知各页面收到消息
                    }
                    break;
                case kWCMessageType.PokeMessage:
                    msg.content = "戳一戳";
                    UpdateFriendLastContentTip(msg);
                    break;
                case kWCMessageType.Link:
                    UpdateFriendLastContentTip(msg);
                    break;
                case kWCMessageType.TYPE_83:
                    string fromName;
                    string toName;
                    string seetext = "";
                    if (!Applicate.ENABLE_RED_PACKAGE)
                    {
                        seetext = ",请在手机上查看";
                    }
                    if (msg.fromUserId.Equals(msg.myUserId))
                    {
                        fromName = "你";
                    }
                    else
                    {
                        fromName = msg.fromUserName;
                    }

                    if (msg.toUserId.Equals(msg.myUserId) || msg.fromUserId.Equals(msg.toUserId))
                    {
                        if (msg.fromUserId.Equals(msg.myUserId))
                        {
                            toName = "自己";
                        }
                        else
                        {
                            toName = msg.toUserName;
                        }
                    }
                    else
                    {
                        toName = msg.toUserName;
                    }
                    msg.ToId = msg.myUserId;
                    msg.FromId = msg.objectId;
                    msg.isGroup = 1;
                    msg.content = fromName + "领取了" + toName + "的红包" + seetext;
                    UpdateFriendLastContentTip(msg);
                    break;
                case kWCMessageType.History:

                    if (msg.InsertData() > 0)//数据库成功插入
                    {
                        msg.UpdateLastSend();
                        NotifactionNormalMessage(msg);//  通知UI  //通知各页面收到消息
                    }

                    break;
            }
        }

        /// <summary>
        /// 已读消息的处理
        /// </summary>
        /// <param name="msg"></param>
        private void ProcessReadMessage(MessageObject msg)
        {

            // 先去找到我发出的这条消息
            var target = new MessageObject()
            {
                FromId = msg.ChatJid,
                ToId = msg.myUserId,
                messageId = msg.content

            }.GetMessageObject();



            if (target == null || string.IsNullOrEmpty(target.messageId))
            {
                Console.WriteLine("收到已读消息：" + msg.content + "本地没有找到原消息");
                return;
            }

            Console.WriteLine("收到已读消息：" + msg.content + "内容：" + target.content);

            if (msg.isGroup == 1)
            {
                //msg.FromId = target.FromId;
                //msg.ToId = target.ToId;

                if (msg.InsertReadData() > 0)
                {
                    int preson = target.readPersons + 1;
                    if (target.UpdateIsReadPersons(preson) > 0)
                    {
                        Messenger.Default.Send(target, MessageActions.XMPP_UPDATE_RECEIVED_READ);//已读消息的处理
                    }
                    else
                    {
                        Console.WriteLine("已读人数修改失败：" + msg.content + " , " + target.content);
                    }
                }
                else
                {
                    Console.WriteLine("已读消息插入失败：" + msg.content + " , " + target.content);
                }
            }
            else
            {
                if (msg.InsertData() > 0)
                {
                    if (target.isReadDel == 1)
                    {
                        var friend = target.GetFriend();
                        if (string.Equals(target.fromUserId, target.myUserId))
                        {
                            // 对方查看了我的一条阅后即焚消息
                            friend.UpdateLastContent("对方查看了您的一条阅后即焚消息", msg.timeSend);
                            // 刷新最后一条聊天记录
                            Messenger.Default.Send(friend, token: MessageActions.UPDATE_FRIEND_LAST_CONTENT);
                        }
                        else
                        {
                            // 更新消息列表
                            var local = msg.GetLastMessage();
                            if (local != null && !UIUtils.IsNull(local.messageId))
                            {
                                var content = friend.ToLastContentTip(local.type, local.content, local.fromUserId, local.fromUserName, local.toUserName);
                                if (friend.IsGroup == 1 && local.type < kWCMessageType.Remind)
                                {
                                    if (local.isReadDel == 1)
                                    {
                                        content = "[阅后即焚消息]";
                                    }
                                    else
                                    {
                                        content = local.fromUserName + ":" + content;
                                    }


                                }

                                friend.UpdateLastContent(content, msg.timeSend);
                            }
                            else
                            {
                                friend.UpdateLastContent(string.Empty, TimeUtils.CurrentIntTime());
                            }

                            Messenger.Default.Send(friend, token: MessageActions.UPDATE_FRIEND_LAST_CONTENT);
                        }
                    }


                    // 去更新消息界面 已读
                    if (target.UpdateIsRead(msg.content) > 0)
                    {
                        Messenger.Default.Send(target, MessageActions.XMPP_UPDATE_RECEIVED_READ);//已读消息的处理
                    }
                }
            }
        }

        /// <summary>
        /// 群文件消息处理
        /// </summary>
        /// <param name="msg"></param>     
        public void ProcessGroupFileMessage(MessageObject msg)
        {
            msg.FromId = msg.objectId;
            msg.fromUserId = msg.objectId;

            Messenger.Default.Send(msg, MessageActions.XMPP_UPDATE_ROOM_CHANGE_MESSAGE);
            // 更新最后一条消息内容，通知UI刷新
            switch (msg.type)
            {
                case kWCMessageType.RoomFileUpload://上传群文件
                    msg.content = UIUtils.QuotationName(msg.fromUserName) + "上传了群文件:" + msg.fileName;
                    break;
                case kWCMessageType.RoomFileDelete://删除群文件
                    msg.content = UIUtils.QuotationName(msg.fromUserName) + "删除了群文件:" + msg.fileName;
                    break;
                case kWCMessageType.RoomFileDownload://下载群文件
                    msg.content = UIUtils.QuotationName(msg.fromUserName) + "下载了群文件:" + msg.fileName;
                    break;
                default:
                    break;
            }

            UpdateFriendLastContentTip(msg);
        }

        /// <summary>
        /// 群控制消息处理
        /// <para>
        /// RoomMemberNameChange和RoomManagerTransfer之间的群设置消息
        /// 先执行数据库操作再执行UI
        /// </para>
        /// </summary>
        /// <param name="msg">对应的处理消息</param>
        private void ProcessGroupManageMessage(MessageObject msg, bool deley)
        {
            msg.FromId = msg.objectId;
            msg.ToId = mLoginUserId;
            msg.isGroup = 1;

            var room = FriendDao.Instance.GetFriendById(msg.ChatJid);
            if (room == null)
            {
                room = new Friend();
                room.RoomId = msg.fileName;//RoomId
                room.UserId = msg.objectId;//设置RoomJid
                room.NickName = msg.content;
                room.IsGroup = 1;
            }

            switch (msg.type)
            {
                case kWCMessageType.RoomMemberNameChange://改群内昵称

                    // 通知聊天记录页刷新
                    NotifactionRoomControlMsg(msg);

                    RoomMemberDao.Instance.UpdateNickname(room.RoomId, msg.toUserId, msg.content);

                    // 更新最后一条消息内容，通知UI刷新
                    msg.content = UIUtils.QuotationName(msg.toUserName) + "修改昵称为:" + UIUtils.QuotationName(msg.content);

                    msg.type = kWCMessageType.Remind;
                    if (msg.InsertData() > 0)
                    {
                        msg.UpdateLastSend();

                        NotifactionNormalMessage(msg);
                    }

                    break;
                case kWCMessageType.RoomNameChange://改群名

                    // 通知聊天记录页刷新
                    NotifactionRoomControlMsg(msg);

                    room.NickName = msg.content;
                    room.UpdateNickName();

                    msg.content = UIUtils.QuotationName(msg.fromUserName) + "修改群名为:" + UIUtils.QuotationName(msg.content);

                    msg.type = kWCMessageType.Remind;

                    if (msg.InsertData() > 0)
                    {
                        msg.UpdateLastSend();
                        NotifactionNormalMessage(msg);
                    }

                    break;
                case kWCMessageType.RoomDismiss://解散

                    if (msg.InsertData() > 0)
                    {
                        // xmpp退出群组
                        ExitRoom(msg.objectId);
                        // 从朋友表中删除
                        room.DeleteByUserId();
                        // 通知界面更新
                        Messenger.Default.Send(msg, MessageActions.XMPP_UPDATE_ROOM_DELETE);

                        Messenger.Default.Send(room, MessageActions.DELETE_FRIEND);
                    }

                    break;
                case kWCMessageType.RoomExit://退群

                    // 离线消息导致群控制通知的异常加减人数（获取后仍加减的问题，禅道#9810）
                    if (deley)
                    {
                        msg.readPersons = -1;
                    }

                    // 我退出群组，或者我被t出
                    if (msg.toUserId.Equals(msg.myUserId))
                    {
                        // xmpp退出群组
                        ExitRoom(msg.objectId);

                        // 从朋友表中删除
                        room.DeleteByUserId();
                        // 通知界面更新
                        Messenger.Default.Send(msg, MessageActions.XMPP_UPDATE_ROOM_DELETE);
                        Messenger.Default.Send(room, MessageActions.DELETE_FRIEND);

                        msg.InsertData();
                        msg.DeleteTable();
                    }
                    else
                    {
                        // 通知聊天记录页刷新
                        NotifactionRoomControlMsg(msg);

                        if (UIUtils.IsNull(room.RoomId))
                        {
                            room = FriendDao.Instance.GetFriendById(room.UserId);
                        }

                        if (msg.fromUserId.Equals(msg.toUserId))
                        {
                            msg.content = UIUtils.QuotationName(msg.fromUserName) + "退出了群组";
                        }
                        else
                        {
                            msg.content = UIUtils.QuotationName(msg.fromUserName) + "移除成员:" + UIUtils.QuotationName(msg.toUserName);
                        }

                        if (room != null)
                        {
                            // 删除群成员
                            RoomMemberDao.Instance.DeleteMemberById(room.RoomId, msg.toUserId);
                            // 刷新群头像
                            Messenger.Default.Send(room.RoomId, MessageActions.UPDATE_HEAD);
                        }

                        // 刷新最近消息列表
                        UpdateFriendLastContentTip(msg);
                    }
                    break;
                case kWCMessageType.RoomNotice://公告

                    // 通知聊天记录页刷新
                    NotifactionRoomControlMsg(msg);

                    // 更新最后一条消息内容，通知UI刷新
                    msg.content = UIUtils.QuotationName(msg.fromUserName) + "发布新公告: \n" + msg.content;
                    UpdateFriendLastContentTip(msg);

                    break;
                case kWCMessageType.RoomInvite://进群

                    if (msg.IsExist())
                    {
                        return;
                    }

                    if (deley)  //离线消息导致群控制通知的异常加减人数（获取后仍加减的问题，禅道#9810）
                    {
                        msg.readPersons = -1;
                    }

                    // 通知聊天记录页刷新


                    string desc = "";

                    if (msg.fromUserId.Equals(msg.toUserId))
                    {
                        desc = UIUtils.QuotationName(msg.fromUserName) + "进入群组";
                        //msg.content = desc;
                    }
                    else
                    {
                        desc = UIUtils.QuotationName(msg.fromUserName) + "邀请成员:" + UIUtils.QuotationName(msg.toUserName);
                    }

                    Console.WriteLine("xmpp 加入群组  :" + desc);
                    NotifactionRoomControlMsg(msg);

                    if (!UIUtils.IsNull(msg.other) && msg.toUserId.Equals(msg.myUserId))
                    {
                        Dictionary<string, object> otherPairs = JsonConvert.DeserializeObject<Dictionary<string, object>>(msg.other);

                        if (otherPairs.ContainsKey("chatKeyGroup"))
                        {
                            string enkey = otherPairs["chatKeyGroup"].ToString();
                            // 解密出 密钥G
                            string key = RSA.DecryptFromBase64Pk1(enkey, Applicate.MyAccount.rsaPrivateKey);
                            // 加密成 chatkeygroup
                            string chatKey = SecureChatUtil.EncryptChatKey(msg.objectId, key, Applicate.API_KEY);

                            room.ChatKeyGroup = chatKey;
                            Console.WriteLine("更新密钥1" + room.ChatKeyGroup);
                            room.UpdateChatKeyGroup(room.ChatKeyGroup);
                        }

                        //更新群设置，被邀请进群，群设置是默认的，不是最新的，解决bug10189
                        if (otherPairs.ContainsKey("showMember"))
                        {
                            int enkey = Convert.ToInt32(otherPairs["showMember"]);
                            room.ShowMember = enkey;
                            room.UpdateShowMember(room.ShowMember);
                        }
                        if (otherPairs.ContainsKey("allowSendCard"))
                        {
                            int enkey = Convert.ToInt32(otherPairs["allowSendCard"]);
                            room.AllowSendCard = enkey;
                            room.UpdateAllowSendCard(room.AllowSendCard);
                        }
                        if (otherPairs.ContainsKey("isSecretGroup"))
                        {
                            int enkey = Convert.ToInt32(otherPairs["isSecretGroup"]);
                            room.IsSecretGroup = enkey;
                            room.UpdateSecretGroup(room.IsSecretGroup);
                        }
                        if (otherPairs.ContainsKey("showRead"))
                        {
                            int enkey = Convert.ToInt32(otherPairs["showRead"].ToString());
                            room.ShowRead = enkey;
                            room.UpdateShowRead();
                        }
                    }


                    bool isExistsRoom = room.ExistsFriend();
                    if (isExistsRoom) // 如果我已经在群里了
                    {
                        room.UpdateLastContent(desc, msg.timeSend);

                        if (!UIUtils.IsNull(room.ChatKeyGroup))
                        {
                            Console.WriteLine("更新密钥2" + room.ChatKeyGroup);
                            room.UpdateChatKeyGroup(room.ChatKeyGroup);
                        }
                    }
                    else
                    {
                        room.Status = 2;
                        room.LastMsgTime = (int)msg.timeSend;
                        var result = room.InsertAuto();
                        Console.WriteLine("保存群组状态：：：：：：：" + result);
                    }

                    msg.content = desc;
                    //msg.fromUserId.Equals(msg.toUserId ）主动进入群组的时候这两个id也是一样的,例如搜索进群 #禅道10092
                    if (msg.fromUserId.Equals(msg.toUserId) && msg.toUserId.Equals(msg.myUserId))
                    {
                        // 群主不再显示自己进入群组的消息
                        msg.type = kWCMessageType.kWCMessageTypeNone;
                    }
                    else
                    {
                        msg.type = kWCMessageType.Remind;
                    }

                    if (msg.InsertData() > 0)
                    {
                        if (msg.toUserId.Equals(msg.myUserId))
                        {
                            room.UpdateCreateTime(msg.timeSend);
                            JoinRoom(msg.objectId, 0);//Xmpp加入群聊(处理完群组后加入)
                        }
                        msg.UpdateLastSend(deley);
                        NotifactionNormalMessage(msg);
                    }


                    if (!isExistsRoom) // && !deley
                    {
                        Messenger.Default.Send(room, MessageActions.ROOM_UPDATE_INVITE);//刷新列表
                    }
                    else
                    {
                        Messenger.Default.Send(room.RoomId, MessageActions.UPDATE_HEAD);//发送刷新头像通知
                    }

                    break;
                case kWCMessageType.RoomReadVisiblity://显示阅读人数

                    // 通知聊天记录页刷新
                    NotifactionRoomControlMsg(msg);

                    room.ShowRead = int.Parse(msg.content);
                    room.UpdateShowRead();

                    if (room.ShowRead == 1)
                    {
                        msg.content = UIUtils.QuotationName(msg.fromUserName) + "开启了显示消息已读人数功能";
                    }
                    else
                    {
                        msg.content = UIUtils.QuotationName(msg.fromUserName) + "关闭了显示消息已读人数功能";
                    }

                    msg.type = kWCMessageType.Remind;
                    if (msg.InsertData() > 0)
                    {
                        msg.UpdateLastSend();
                        NotifactionNormalMessage(msg);
                    }
                    //更新群聊是否显示人数  room.UpdateShowRead(int.Parse(msg.content));
                    break;
                case kWCMessageType.RoomIsVerify://群验证

                    //修改禅道bug10093 多点登录的时候安卓申请入群，pc收到了一条群验证消息，事实上这条消息是群主才能收到，但这条消息的fromuserid是自己也是申请人，touserid是群主，但是fromusername跟tousername一样 
                    if (msg.fromUserName == msg.toUserName)
                    {
                        return;
                    }

                    // 通知聊天记录页刷新
                    NotifactionRoomControlMsg(msg);

                    if ("0".Equals(msg.content) || "1".Equals(msg.content))
                    {
                        room.IsNeedVerify = int.Parse(msg.content);
                        room.UpdateNeedVerify();
                        msg.content = room.IsNeedVerify == 0 ? " 已关闭进群验证" : " 已开启进群验证";
                        msg.content = UIUtils.QuotationName(msg.fromUserName) + msg.content;
                        UpdateFriendLastContentTip(msg);
                    }
                    else
                    {

                        string objecid = msg.objectId;
                        var notic = JsonConvert.DeserializeObject<Dictionary<string, object>>(objecid.ToString());
                        string roomJid = UIUtils.DecodeString(notic, "roomJid");


                        if (string.Equals(msg.toUserId, msg.myUserId))
                        {
                            string[] userids;
                            string nickname = msg.fromUserName;
                            string useids = UIUtils.DecodeString(notic, "userIds");
                            userids = useids.Split(',');
                            if (msg.fromUserId == useids)
                            {
                                //msg.content = msg.fromUserName + "申请加入";//自己主动加入
                                msg.content = msg.fromUserName + LanguageXmlUtils.GetValue("apply_to_join_group", "申请加入群聊");//自己主动加入
                            }
                            else
                            {
                                //msg.content = msg.fromUserName + "想邀请" + userids.Length + "位朋友加入群聊";
                                msg.content = msg.fromUserName + LanguageXmlUtils.GetValue("invite_fd_join_group", "想邀请" + userids.Length.ToString() + "位朋友进入群聊").Replace("%s", userids.Length.ToString());
                            }
                        }
                        else
                        {
                            msg.type = kWCMessageType.Remind;
                            //msg.content = "群聊邀请已发送给群主,请等待群主确认";
                            msg.content = LanguageXmlUtils.GetValue("wait_invite_result", "群聊邀请已发送给群主,请等待群主确认");
                        }

                        msg.FromId = roomJid;
                        msg.toUserId = Applicate.MyAccount.userId;
                        if (msg.InsertData() > 0)
                        {
                            msg.UpdateLastSend();
                            NotifactionNormalMessage(msg);
                        }
                    }

                    break;
                case kWCMessageType.RoomUnseenRole://隐身人

                    // 修改本地状态
                    RoomMember roomMember = new RoomMember();
                    roomMember.roomId = room.RoomId;
                    roomMember.role = "1".Equals(msg.content) ? 4 : 3;
                    roomMember.userId = msg.toUserId;
                    roomMember.nickName = msg.toUserName;
                    RoomMemberDao.Instance.InsertOrUpdate(roomMember);

                    // 通知聊天记录页刷新
                    NotifactionRoomControlMsg(msg);

                    if (msg.fromUserId.Equals(msg.myUserId) || msg.myUserId.Equals(msg.toUserId))
                    {

                        if ("1".Equals(msg.content))
                        {
                            msg.content = UIUtils.QuotationName(msg.fromUserName) + "设置隐身人" + UIUtils.QuotationName(msg.toUserName);
                        }
                        else
                        {
                            msg.content = UIUtils.QuotationName(msg.fromUserName) + "取消隐身人" + UIUtils.QuotationName(msg.toUserName);
                        }

                        msg.type = kWCMessageType.Remind;
                        if (msg.InsertData() > 0)
                        {
                            msg.UpdateLastSend();
                            NotifactionNormalMessage(msg);
                        }
                    }


                    break;
                case kWCMessageType.RoomAdmin://管理员

                    // 通知聊天记录页刷新
                    NotifactionRoomControlMsg(msg);
                    if (msg.content == "0")//取消管理员
                    {
                        msg.content = UIUtils.QuotationName(msg.fromUserName) + "取消管理员" + UIUtils.QuotationName(msg.toUserName);
                    }
                    else if (msg.content == "1")//设置管理员8
                    {
                        msg.content = UIUtils.QuotationName(msg.fromUserName) + "设置:" + UIUtils.QuotationName(msg.toUserName) + "为管理员";
                    }

                    // 更新最后一条消息内容，通知UI刷新
                    UpdateFriendLastContentTip(msg);

                    break;
                case kWCMessageType.RoomIsPublic://公开群

                    // 通知聊天记录页刷新
                    NotifactionRoomControlMsg(msg);
                    msg.content = msg.content == "0" ? " 将本群修改为公开群组" : " 将本群修改为私密群组";
                    msg.content = UIUtils.QuotationName(msg.fromUserName) + msg.content;
                    // 更新最后一条消息内容，通知UI刷新
                    UpdateFriendLastContentTip(msg);

                    break;
                case kWCMessageType.RoomInsideVisiblity://显示群内成员
                                                        // 通知聊天记录页刷新
                    NotifactionRoomControlMsg(msg);
                    room.UpdateShowMember(int.Parse(msg.content));
                    msg.content = msg.content == "0" ? " 关闭了查看群成员功能" : " 开启了查看群成员功能";
                    msg.content = UIUtils.QuotationName(msg.fromUserName) + msg.content;
                    // 更新最后一条消息内容，通知UI刷新
                    UpdateFriendLastContentTip(msg);

                    break;
                case kWCMessageType.RoomUserRecommend://是否允许发送名片
                    NotifactionRoomControlMsg(msg); // 通知聊天记录页刷新
                    room.UpdateAllowSendCard(int.Parse(msg.content));//
                    msg.content = msg.content == "0" ? " 关闭了群内私聊功能" : " 开启了群内私聊功能";
                    msg.content = UIUtils.QuotationName(msg.fromUserName) + msg.content;
                    // 更新最后一条消息内容，通知UI刷新
                    UpdateFriendLastContentTip(msg);
                    break;
                case kWCMessageType.RoomMemberBan://禁言成员

                    double banTime = double.Parse(msg.content);

                    if (msg.myUserId.Equals(msg.toUserId)) // 我被禁言
                    {
                        if (banTime > TimeUtils.CurrentTime() + 3)
                        {
                            RoomMemberDao.Instance.UpdateMemberTalkTime(room.RoomId, msg.myUserId, Convert.ToInt64(banTime));
                        }
                        else
                        {
                            // 取消禁言
                            RoomMemberDao.Instance.UpdateMemberTalkTime(room.RoomId, msg.myUserId, 0);
                        }

                        Messenger.Default.Send(msg, MessageActions.ROOM_UPDATE_BANNED_TALK);
                    }

                    if (banTime > TimeUtils.CurrentTime() + 3)
                    {
                        var date = Helpers.StampToDatetime(banTime);
                        string time = date.Month + "-" + date.Day + " " + date.Hour + ":" + date.Minute;
                        msg.content = UIUtils.QuotationName(msg.fromUserName) + "对" + UIUtils.QuotationName(msg.toUserName) + "设置了禁言,截止:" + time;
                    }
                    else
                    {
                        msg.content = UIUtils.QuotationName(msg.toUserName) + "已被" + UIUtils.QuotationName(msg.fromUserName) + "取消禁言";
                    }

                    // 更新最后一条消息内容，通知UI刷新
                    UpdateFriendLastContentTip(msg);

                    break;
                case kWCMessageType.RoomAllBanned://群组全员禁言 


                    long state = Convert.ToInt64(msg.content);
                    FriendDao.Instance.UpdateAllBannedTalk(room.UserId, state);
                    room.AllBannedTalk = state;
                    if (!deley)
                    {
                        Messenger.Default.Send(msg, MessageActions.ROOM_UPDATE_BANNED_TALK);
                    }

                    msg.content = msg.content == "0" ? "已关闭全体禁言" : "已开启全体禁言";
                    // 更新最后一条消息内容，通知UI刷新
                    UpdateFriendLastContentTip(msg);

                    break;
                case kWCMessageType.RoomAllowMemberInvite://是否允许群内普通成员邀请陌生人
                    NotifactionRoomControlMsg(msg); // 通知聊天记录页刷新
                    room.UpdateAllowInviteFriend(int.Parse(msg.content));
                    msg.content = msg.content == "0" ? " 关闭了普通成员邀请功能" : " 开启了普通成员邀请功能";
                    msg.content = UIUtils.QuotationName(msg.fromUserName) + msg.content;

                    // 更新最后一条消息内容，通知UI刷新
                    UpdateFriendLastContentTip(msg);

                    break;
                case kWCMessageType.RoomManagerTransfer://转让群主

                    // 修改老群组身份
                    RoomMemberDao.Instance.UpdateMemberRole(room.RoomId, msg.fromUserId, 3);
                    // 修改新群主身份
                    RoomMemberDao.Instance.UpdateMemberRole(room.RoomId, msg.toUserId, 1);

                    NotifactionRoomControlMsg(msg); // 通知聊天记录页刷新

                    msg.content = UIUtils.QuotationName(msg.toUserName) + "已成为新群主";
                    // 更新最后一条消息内容，通知UI刷新
                    UpdateFriendLastContentTip(msg);

                    break;
                case kWCMessageType.RoomAllowUploadFile://是否允许成员上传群文件
                    NotifactionRoomControlMsg(msg); // 通知聊天记录页刷新

                    room.UpdateAllowUploadFile(int.Parse(msg.content));
                    msg.content = msg.content == "0" ? " 关闭了普通成员上传群共享" : " 开启了普通成员上传群共享";
                    msg.content = UIUtils.QuotationName(msg.fromUserName) + msg.content;
                    // 更新最后一条消息内容，通知UI刷新
                    UpdateFriendLastContentTip(msg);

                    break;
                case kWCMessageType.RoomAllowConference://是否允许群会议
                    NotifactionRoomControlMsg(msg); // 通知聊天记录页刷新 
                    room.UpdateAllowConference(int.Parse(msg.content));
                    msg.content = msg.content == "0" ? " 关闭了普通成员发起会议功能" : " 开启了普通成员发起会议功能";
                    msg.content = UIUtils.QuotationName(msg.fromUserName) + msg.content;
                    // 更新最后一条消息内容，通知UI刷新
                    UpdateFriendLastContentTip(msg);

                    break;
                case kWCMessageType.RoomAllowSpeakCourse://是否允许群成员开课

                    NotifactionRoomControlMsg(msg); // 通知聊天记录页刷新 
                    room.UpdateAllowSpeakCourse(int.Parse(msg.content));
                    msg.content = msg.content == "0" ? " 关闭了普通成员发送课件功能" : " 开启了普通成员发送课件功能";
                    msg.content = UIUtils.QuotationName(msg.fromUserName) + msg.content;
                    // 更新最后一条消息内容，通知UI刷新
                    UpdateFriendLastContentTip(msg);
                    break;
                case kWCMessageType.RoomNewOverDate://群消息过期
                    LogUtils.Log("收到群消息过期   " + msg.content);
                    LocalDataUtils.SetStringData(msg.ChatJid + "chatRecordTimeOut" + Applicate.MyAccount.userId, msg.content);
                    break;
                case kWCMessageType.RoomNoticeEdit://编辑群公告
                                                   // 通知聊天记录页刷新
                    NotifactionRoomControlMsg(msg);

                    // 更新最后一条消息内容，通知UI刷新
                    msg.content = UIUtils.QuotationName(msg.fromUserName) + "修改了群公告:\n" + msg.content;
                    UpdateFriendLastContentTip(msg);

                    break;
                default:
                    return;
            }

        }

        #endregion

        #region 多点登录同步消息处理
        /// <summary>
        /// 多点登录同步消息处理
        /// </summary>
        /// <param name="msg"></param>
        private void ProcessDeviceSyncMessage(MessageObject msg, bool isOfflieMsgppMsg)
        {
            Console.WriteLine("ProcessDeviceSyncMessage  " + msg.messageId);
            if (msg.type == kWCMessageType.Device_SYNC_OTHER)
            {

                if (SYNC_LOGIN_PASSWORD.Equals(msg.objectId))
                {
                    // 2026-08-31 修复：新注册账号登录后服务器会推送 sync_login_password 同步消息，
                    // 原逻辑误判为"密码已被修改"导致弹窗+清空密码+杀进程重启（新账号必现、无限循环）。
                    // 方案A：忽略该同步消息，新账号可正常登录使用。
                    Console.WriteLine("收到 sync_login_password 同步消息（已忽略）");
                }
                else if (SYNC_PAY_PASSWORD.Equals(msg.objectId))
                {
                    Console.WriteLine("其他端修改了支付密码");

                }
                else if (SYNC_PRIVATE_SETTINGS.Equals(msg.objectId))
                {
                    Console.WriteLine("其他端修改了隐私设置");

                }
                else if (SYNC_LABEL.Equals(msg.objectId))
                {
                    Console.WriteLine("其他端修改了标签");
                    FriendLabelDao.Instance.UpdateLableByHttp(null);
                    Messenger.Default.Send("1", MessageActions.UPDATE_LABLE_LIST);
                }
            }
            else if (msg.type == kWCMessageType.Device_SYNC_FRIEND)
            {

                if (string.Equals(msg.toUserId, msg.myUserId))
                {
                    Console.WriteLine("多点登录同步--->更新自己的信息");
                }
                else
                {
                    Console.WriteLine("多点登录同步--->更新好友的信息");
                    HandleFriendUpdate(msg.toUserId);
                }
            }
            else if (msg.type == kWCMessageType.Device_SYNC_GROUP)
            {
                if (!isOfflieMsgppMsg)
                {
                    Console.WriteLine("多点登录同步--->更新群组的信息");
                    HandleGroupUpdate(msg.toUserId);
                }
            }
        }

        /// <summary>
        /// 密码已被修改需重新登录
        /// </summary>
        private void UpdatePwdRestart()
        {
            HttpUtils.Instance.ShowTip("密码已被修改需重新登录");

            //登录记住密码清空
            Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            cfa.AppSettings.Settings["passWord"].Value = String.Empty;
            cfa.Save();

            ShiKuManager.ApplicationExit();
            LocalDataUtils.SetStringData(Applicate.QUIT_TIME, TimeUtils.CurrentIntTime().ToString());
            //此处调用退出接口
            Application.ExitThread();
            Application.Exit();
            Application.Restart();
            Process.GetCurrentProcess().Kill();
        }
        #endregion

        #region 端到端加密通讯消息处理
        /// <summary>
        /// 端到端加密通讯消息处理
        /// </summary>
        private void ProcessSecureMessage(MessageObject msg)
        {
            // 群成员请求群组密钥密钥 
            if (msg.type == kWCMessageType.TYPE_SECURE_LOST_KEY)
            {
                if (msg.InsertData() > 0)//数据库成功插入
                {
                    msg.UpdateLastSend();
                    NotifactionNormalMessage(msg);//  通知UI  //通知各页面收到消息

                    if (string.Equals(msg.fromUserId, msg.myUserId))
                    {
                        var room = msg.GetFriend();
                        room.IsLostKeyGroup = 1;
                        room.UpdateLostKeyGroup();
                        Messenger.Default.Send(room, MessageActions.UPDATE_CHATKEY_STATE);//群控制相关消息通知
                    }
                }
                return;
            }


            if (msg.type == kWCMessageType.TYPE_SECURE_SEND_KEY)
            {

                var room = new Friend() { UserId = msg.objectId, IsGroup = 1 }.GetByUserId();

                // 其他群成员正在给我发送的密钥
                if (msg.toUserId.Equals(msg.myUserId))
                {
                    // 解密出 密钥G
                    string key = RSA.DecryptFromBase64Pk1(msg.content, Applicate.MyAccount.rsaPrivateKey);

                    if (!UIUtils.IsNull(room.NickName) && !UIUtils.IsNull(key))
                    {
                        // 加密成 chatkeygroup
                        string chatKey = SecureChatUtil.EncryptChatKey(msg.objectId, key, Applicate.API_KEY);


                        room.IsLostKeyGroup = 0;
                        room.UpdateLostKeyGroup();

                        room.UpdateChatKeyGroup(chatKey);
                        // 刷新UI状态
                        Messenger.Default.Send(room, MessageActions.UPDATE_CHATKEY_STATE);

                        List<MessageObject> messages = room.GetFailMessageList();

                        foreach (var message in messages)
                        {
                            //message.isEncrypt = 3;
                            if (SkSSLUtils.DHDecryptFailMessage(chatKey, message))
                            {
                                message.UpdateDecryptStateSuccess();
                                Messenger.Default.Send(message, MessageActions.XMPP_REFRESH_MESSAGE);
                            };
                        }

                        // 更新最后一条消息内容
                        MessageObject local = new MessageObject() { isGroup = 1, FromId = room.UserId }.GetLastMessage();
                        if (!UIUtils.IsNull(local.messageId) && !NotEncrypt(local))
                        {
                            string content = local.fromUserName + ":" + room.ToLastContentTip(local.type, local.content);
                            room.UpdateLastContent(content, local.timeSend);
                            Messenger.Default.Send(room, token: MessageActions.UPDATE_FRIEND_LAST_CONTENT);
                        }

                        // 更新自己的chatkey
                        HandleGroupChatkeyUpdate(room.RoomId, msg.content);
                    }
                }

                if (string.Equals(msg.toUserId, msg.ChatJid))
                {
                    // 更新请求状态
                    msg.UpdateDownloadState(msg.content, 1);
                }

                return;
            }

            // 群主重置密钥
            if (kWCMessageType.TYPE_SECURE_NOTIFY_REFRESH_KEY == msg.type)
            {
                var room = msg.GetFriend();
                HandleGroupUpdate(room.RoomId);

                //// 更新最后一条消息内容，通知UI刷新
                msg.type = kWCMessageType.Remind;
                msg.content = "群主重置了群组密钥";
                if (msg.InsertData() > 0)
                {
                    msg.DeleteMessageLow(msg.timeSend);
                    Messenger.Default.Send(room.UserId, token: EQFrmInteraction.ClearFdMsgsSingle);
                    msg.UpdateLastSend();
                    NotifactionNormalMessage(msg);
                }

                room.IsLostKeyGroup = 0;
                room.UpdateLostKeyGroup();
                Messenger.Default.Send(room, MessageActions.UPDATE_CHATKEY_STATE);//群控制相关消息通知

                return;
            }

            // 忘记密钥 更新密钥对
            if (kWCMessageType.TYPE_SECURE_REFRESH_KEY == msg.type)
            {
                HandleFriendUpdate(msg.fromUserId);
                return;
            }


            LogUtils.Save("==========未处理的端到端协议===========\n" + msg.ToJson(true));
        }

        #endregion

        #region 后台管理相关消息
        /// <summary>
        /// 端到端加密通讯消息处理
        /// </summary>
        private void ProcessServiceManagerMessage(MessageObject msg)
        {
            // 被后台删除（删除该好友账户）
            if (msg.type == kWCMessageType.TYPE_REMOVE_ACCOUNT)
            {
                var tmpFriend = new Friend() { UserId = msg.objectId }.GetByUserId();
                // 设置为被删除
                tmpFriend.UpdateFriendState(tmpFriend.UserId, Friend.STATUS_26);
                // 清空消息记录
                msg.FromId = tmpFriend.UserId;
                msg.ToId = msg.myUserId;
                msg.DeleteTable();
                // 通知界面刷新
                Messenger.Default.Send(tmpFriend, MessageActions.DELETE_FRIEND);
                // 显示提示
                HttpUtils.Instance.ShowTip("服务器删除了" + tmpFriend.NickName);

                return;
            }

            //后台删除好友(解除用户与好友关系)
            if (msg.type == kWCMessageType.TYPE_DELETEFRIENDS)
            {
                var list = JsonConvert.DeserializeObject<Dictionary<string, object>>(msg.objectId);
                string fromUserId = UIUtils.DecodeString(list, "fromUserId");
                string fromUserName = UIUtils.DecodeString(list, "fromUserName");
                string toUserId = UIUtils.DecodeString(list, "toUserId");
                string toUserName = UIUtils.DecodeString(list, "toUserName");

                var tmpFriend = new Friend() { UserId = toUserId }.GetByUserId();
                // 设置为被删除
                tmpFriend.UpdateFriendState(tmpFriend.UserId, Friend.STATUS_26);
                // 清空消息记录
                msg.FromId = tmpFriend.UserId;
                msg.ToId = msg.myUserId;
                msg.DeleteTable();
                // 通知界面刷新
                Messenger.Default.Send(tmpFriend, MessageActions.DELETE_FRIEND);
                // 显示提示
                HttpUtils.Instance.ShowTip("服务器解除了你与" + tmpFriend.NickName + "的好友关系");
                return;
            }

            //后台加入黑名单
            if (msg.type == kWCMessageType.TYPE_JOINBLACKLIST)
            {
                var list = JsonConvert.DeserializeObject<Dictionary<string, object>>(msg.objectId);
                string fromUserId = UIUtils.DecodeString(list, "fromUserId");
                string fromUserName = UIUtils.DecodeString(list, "fromUserName");
                string toUserId = UIUtils.DecodeString(list, "toUserId");
                string toUserName = UIUtils.DecodeString(list, "toUserName");
                Friend friend = new Friend { UserId = toUserId }.GetByUserId();

                ShiKuManager.SendBlockfriend(friend);//发送拉黑好友推送
            }

            //从黑名单移除
            if (msg.type == kWCMessageType.TYPE_MOVE_BALACKLIST)
            {
                var list = JsonConvert.DeserializeObject<Dictionary<string, object>>(msg.objectId);
                string fromUserId = UIUtils.DecodeString(list, "fromUserId");
                string fromUserName = UIUtils.DecodeString(list, "fromUserName");
                string toUserId = UIUtils.DecodeString(list, "toUserId");
                string toUserName = UIUtils.DecodeString(list, "toUserName");
                Friend friend = new Friend { UserId = toUserId }.GetByUserId();

                ShiKuManager.SendCancelBlockFriendMsg(friend);//发送取消拉黑好友推送
            }


            // 我被后台删除了
            if (msg.type == kWCMessageType.TYPE_DELETE_SELF)
            {
                ShiKuManager.ApplicationExit();
                // 关闭任务栏图标
                Messenger.Default.Send("1030102", MessageActions.CLOSE_NOTIFY_INCO);
                //此处调用退出接口
                Application.ExitThread();

                Applicate.ConflictClose();

                var res = MessageBox.Show(Applicate.GetWindow<FrmLogin>(), "此账号已被后台管理员删除", "后台提示");
            }
        }

        #endregion

        #region 后台锁定用户相关消息
        /// <summary>
        /// 端到端加密通讯消息处理
        /// </summary>
        private void ProcessServiceLockMessage(MessageObject msg, bool delay)
        {
            // 被后台锁定账号
            if (msg.type == kWCMessageType.TYPE_SERVICE_LOCK_USER)
            {
                if (!delay)
                {
                    var res = MessageBox.Show(Applicate.GetWindow<FrmMain>(), "当前账号已被后台管理员锁定!", "后台锁定");
                    mSocketCore.Disconnect();
                    LocalDataUtils.SetStringData(Applicate.QUIT_TIME, TimeUtils.CurrentIntTime().ToString());
                    //此处调用退出接口
                    Application.ExitThread();
                    Application.Exit();
                    Application.Restart();
                    Process.GetCurrentProcess().Kill();
                }

                return;
            }

            // 被后台锁定群组
            if (msg.type == kWCMessageType.TYPE_SERVICE_LOCK_GROUP)
            {
                var tmpFriend = new Friend() { UserId = msg.objectId }.GetByUserId();

                if ("-1".Equals(msg.content))
                {
                    // 锁定此用户
                    tmpFriend.IsDismiss = 2;
                    tmpFriend.UpdateDismiss(2);
                }
                else if ("1".Equals(msg.content))
                {
                    // 解锁此用户
                    tmpFriend.IsDismiss = 0;
                    tmpFriend.UpdateDismiss(0);
                }

                // 刷新 消息界面
                Messenger.Default.Send(tmpFriend, MessageActions.UPDATE_SERVICE_LOCK);
                return;
            }
        }

        #endregion



        #region ========================================================通知模块========================================================  

        private List<MessageObject> mssageList = new List<MessageObject>();

        private delegate void DelegateMessage(MessageObject msg, Message xmpp);

        private void OnMessageLoop(object source, ElapsedEventArgs e)
        {
            LogUtils.Error("[DIAG-XM] loop count=" + mssageList.Count);
            //Console.WriteLine("OnMessageLoop  + OnMessageLoop");

            if (mssageList.Count == 0)
            {
                return;
            }

            if (mssageList.Count > 1)
            {
                // 一次性更新多条 参数 List<MessageObject>
                NotifactionListAllMessage(mssageList);
            }
            else
            {
                // 单条刷新 参数 MessageObject
                NotifactionListSingleMessage(mssageList[0]);
            }

            mssageList.Clear();// 清理消息队列
        }

        public void NotifactionListAllMessage(List<MessageObject> messages)
        {
            LogUtils.Log("xmpp 通知 更新项 all");
            Messenger.Default.Send(messages, MessageActions.XMPP_SHOW_ALL_MESSAGE);
        }


        public void NotifactionListSingleMessage(MessageObject messages)
        {
            Messenger.Default.Send(messages, MessageActions.XMPP_SHOW_SINGLE_MESSAGE);
        }


        /// <summary>
        /// 普通消息的通知
        /// </summary>
        /// <param name="msg"></param>
        public void NotifactionNormalMessage(MessageObject msg)
        {
            LogUtils.Error("[DIAG-XM] normal msg jid=" + msg.ChatJid + " type=" + msg.type + " isReadDel=" + msg.isReadDel + " len=" + (msg.content == null ? 0 : msg.content.Length));
            // 消息排队机制 -> 针对与消息列表
            if (mssageList.Count > 0)
            {
                mssageList.Add(msg); // 已经启动过计时器了，直接往列表里加消息就可以
                MessageLooper.Stop();
                MessageLooper.Start();
            }
            else
            {
                MessageLooper.Start();// 启动定时器 -> 0.5秒后调用 OnMessageLoop 更新列表
                mssageList.Add(msg);
            }

            // 更新聊天记录页
            Messenger.Default.Send(msg, MessageActions.XMPP_UPDATE_NORMAL_MESSAGE);
        }

        /// <summary>
        /// 撤回消息的通知
        /// </summary>
        /// <param name="msg"></param>
        public void NotifactionRecallMessage(MessageObject msg)
        {
            Messenger.Default.Send(msg, MessageActions.XMPP_UPDATE_RECALL_MESSAGE);//通知各页面收到消息
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="msg"></param>
        public void NotifactionVerifyMessage(MessageObject msg)
        {
            Messenger.Default.Send(msg, MessageActions.XMPP_UPDATE_VERIFY_MESSAGE);//朋友验证相关消息通知
        }

        /// <summary>
        /// 群控制相关消息通知
        /// </summary>
        /// <param name="msg"></param>
        public void NotifactionRoomControlMsg(MessageObject msg)
        {
            Messenger.Default.Send(msg, MessageActions.XMPP_UPDATE_ROOM_CHANGE_MESSAGE);//群控制相关消息通知
        }

        /// <summary>
        /// 音视频协议相关消息通知
        /// </summary>
        /// <param name="msg"></param>
        public void NotifactionMeetingMessage(MessageObject msg)
        {
            Messenger.Default.Send(msg, MessageActions.XMPP_UPDATE_MEETING_MESSAGE);//收到了一个音视频协议消息
        }

        #endregion

        #region ========================================================群组相关========================================================

        /// <summary>
        /// 创建群组
        /// </summary>
        /// <param name="roomName">群名字</param>
        /// <param name="description">群描述</param>
        /// <returns>roomJid</returns>
        public string CreateGroup(string roomName, string description, string jid = null)
        {
            if (jid == null)
            {
                jid = Guid.NewGuid().ToString("N");
            }
            else
            {
                jid = jid;
            }

            JoinRoom(jid, 0);

            return jid;
        }

        /// <summary>
        /// 加入群聊
        /// </summary>
        /// <param name="roomJid">RoomJId</param>
        /// <param name="lastExitTime"></param>
        public void JoinRoom(string roomJid, long seconds)
        {
            MessageHead head = new MessageHead();
            head.chatType = 2;
            head.from = mLoginUserId + "/" + ShiKuManager.Resource;
            head.to = "service";
            head.messageId = Guid.NewGuid().ToString("N");

            JoinGroupMessageProBuf join = new JoinGroupMessageProBuf();
            join.jid = roomJid;
            join.seconds = seconds;
            join.messageHead = head;

            mSocketCore.SendMessage(join, Command.COMMAND_JOIN_GROUP_REQ);
            Console.WriteLine("发送加入群组消息");

        }

        /// <summary>
        /// 退出群组
        /// </summary>
        /// <param name="roomJid">RoomJId</param>
        public void ExitRoom(string roomJid)
        {

            MessageHead head = new MessageHead();
            head.chatType = 2;
            head.from = mLoginUserId + "/" + ShiKuManager.Resource;
            head.to = "service";
            head.messageId = Guid.NewGuid().ToString("N");

            ExitGroupMessageProBuf exit = new ExitGroupMessageProBuf();
            exit.jid = roomJid;
            exit.messageHead = head;

            mSocketCore.SendMessage(exit, Command.COMMAND_EXIT_GROUP_REQ);
            Console.WriteLine("发送退出群组消息");
        }


        #endregion

        #region ========================================================内部方法========================================================

        // ============ 2026-11-21 新增：登录后补拉离线消息 + 未读红点（复用现有解密/插入链路） ============

        /// <summary>本地消息表最后一条消息时间（秒）。裸 SQL，不建表，避免误操作</summary>
        private static double GetLocalLastMsgTime(string jid)
        {
            try
            {
                object o = DBSetting.SQLiteDBContext.Ado.GetScalar("select max(timeSend) from \"msg_" + jid + "\"");
                if (o == null || o == DBNull.Value)
                {
                    return 0;
                }
                return Convert.ToDouble(o);
            }
            catch { return 0; }
        }

        /// <summary>是否数字单聊 jid（群/服务号 jid 调单聊接口会被服务器拒绝，必须排除）</summary>
        private static bool IsSingleNumberJid(string jid)
        {
            if (string.IsNullOrEmpty(jid)) { return false; }
            long v;
            return long.TryParse(jid, out v);
        }

        private static DateTime _lastUnreadSyncTime = DateTime.MinValue;

        /// <summary>登录后：找出服务器有新消息而本地缺失的单聊会话，逐个补拉</summary>
        private void SyncUnreadAndRoam(JsonLastChats list)
        {
            try
            {
                if (list == null || list.data == null || list.data.Count == 0) { return; }
                // getLastChatList 登录时会回调两次，5 秒内只执行一次，避免重复拉取
                if ((DateTime.Now - _lastUnreadSyncTime).TotalSeconds < 5) { return; }
                _lastUnreadSyncTime = DateTime.Now;

                List<string> jids = new List<string>();
                List<long> ends = new List<long>();

                for (int i = 0; i < list.data.Count; i++)
                {
                    DataItem item = list.data[i];
                    if (item == null || string.IsNullOrEmpty(item.jid)) { continue; }
                    if (item.isRoom == 1) { continue; }                   // 群聊沿用原有 MsgRoamTask 机制，不动
                    if (item.type == 90 || item.type == 91) { continue; } // 收付款凭证
                    if (!IsSingleNumberJid(item.jid)) { continue; }        // 只处理数字单聊
                    if (Applicate.MyAccount == null) { continue; }
                    if (item.jid.Equals(Applicate.MyAccount.userId)) { continue; }
                    if (item.timeSend <= 0) { continue; }

                    double localLast = GetLocalLastMsgTime(item.jid);
                    if (item.timeSend > localLast * 1000 + 1000)          // 服务器更新时间 > 本地最后一条
                    {
                        jids.Add(item.jid);
                        ends.Add(item.timeSend);
                    }
                }

                if (jids.Count == 0) { return; }
                LogHelper.log.Error("[SYNC] 需补拉会话=" + jids.Count + " : " + string.Join(",", jids.ToArray()));
                PullRoamChat(jids, ends, 0);
            }
            catch (Exception ex) { LogHelper.log.Error("[SYNC-EX] " + ex.Message); }
        }

        /// <summary>逐个会话补拉缺失消息，补完写未读数并刷新会话列表</summary>
        private void PullRoamChat(List<string> jids, List<long> ends, int index)
        {
            if (index >= jids.Count)
            {
                Messenger.Default.Send("1", MessageActions.DOWN_CHATLIST_COMPT);
                return;
            }

            string jid = jids[index];
            long endMs = ends[index];

            try
            {
                double localLast = GetLocalLastMsgTime(jid);
                bool hasLocal = localLast > 0;
                long startMs = hasLocal ? (long)Math.Floor(localLast * 1000) : 1262275200000L;

                HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "tigase/shiku_msgs")
                    .AddParams("access_token", Applicate.Access_Token)
                    .AddParams("receiver", jid)
                    .AddParams("startTime", startMs.ToString())
                    .AddParams("endTime", (endMs + 1000).ToString())
                    .AddParams("pageIndex", "0")
                    .AddParams("pageSize", "100")
                    .NoErrorTip()
                    .Build().ExecuteList<MessageRecordList>((success, result) =>
                    {
                        int unread = 0;
                        int insert = 0;
                        bool apiOk = false;
                        try
                        {
                            if (success && result != null && result.data != null)
                            {
                                apiOk = true;
                                for (int i = 0; i < result.data.Count; i++)
                                {
                                    // 与 DownMsgRecord 完全一致的解析链路：message(json) -> ChatMessage -> MessageObject
                                    MessageObject mo = result.data[i].ToMessageObject();
                                    if (mo == null) { continue; }
                                    if (!mo.IsVisibleMsg()) { continue; }
                                    if (mo.deleteTime > 0 && mo.deleteTime < TimeUtils.CurrentTimeDouble()) { continue; }

                                    // 统一解密入口：isEncrypt=1/2/3 自动处理，不加密原样返回
                                    SkSSLUtils.DecryptRecordMessage(mo, false);

                                    bool isNew = hasLocal && mo.timeSend > localLast;
                                    mo.isSend = 1;
                                    mo.isRead = isNew ? 0 : 1;

                                    if (mo.InsertData() > 0)   // InsertData 按 messageId 去重
                                    {
                                        insert++;
                                        if (isNew && !jid.Equals(mo.fromUserId)) { unread++; }
                                    }
                                }
                            }
                        }
                        catch (Exception ex) { LogHelper.log.Error("[ROAM] jid=" + jid + " " + ex.Message); }

                        try
                        {
                            // 以“本次补拉到的新消息条数”为准：有 2 条就显示 2，没有就清零
                            if (apiOk)
                            {
                                new Friend() { UserId = jid }.UpdateRedNum(Math.Min(unread, 99));
                            }
                            new Friend() { UserId = jid }.UpdateDownTime(0, 0); // 清漫游窗口标记
                            LogHelper.log.Error("[ROAM-DONE] jid=" + jid + " insert=" + insert + " unread=" + unread);
                        }
                        catch (Exception ex) { LogHelper.log.Error("[ROAM-DB] " + ex.Message); }

                        PullRoamChat(jids, ends, index + 1);
                    });
            }
            catch (Exception ex)
            {
                LogHelper.log.Error("[ROAM-OUTER] jid=" + jid + " " + ex.Message);
                PullRoamChat(jids, ends, index + 1);
            }
        }

        private void UpdateChatListData(JsonLastChats list)
        {
            if (list == null || UIUtils.IsNull(list.data))
            {
                Messenger.Default.Send("1", MessageActions.DOWN_CHATLIST_COMPT);
                return;
            }

            // 2026-11-21 修复：服务器数据可能滞后，先取本地时间快照，避免旧数据覆盖本地新消息


            var localFriends = new Friend().GetRecentList();


            var localTimeMap = new System.Collections.Generic.Dictionary<string, double>();


            for (int li = 0; li < localFriends.Count; li++)


            {


                localTimeMap[localFriends[li].UserId] = localFriends[li].LastMsgTime;


            }


            List<Friend> fd_list = new List<Friend>();
            //int count = 0;
            foreach (var item in list.data)
            {

                Friend friend = new Friend()
                {
                    UserId = item.jid,
                    IsGroup = item.isRoom,
                };

                // 过滤掉收付款凭证
                if (item.type == 90 || item.type == 91)
                {
                    continue;
                }

                string lastContent = SkSSLUtils.DecryptLastMessage(item);

                // 聊天记录漫游
                long startTime = new MessageObject() { FromId = friend.UserId }.GetLastTimeStamp();
                if (item.timeSend > startTime)
                {
                    if (friend.IsGroup == 1)
                    {
                        if (startTime != 0)
                        {
                            MessageObject local = new MessageObject() { isGroup = 1, FromId = friend.UserId }.GetLastMessage();
                            // 生成离线任务
                            MsgRoamTaskDao.Instance.CreateTask(friend.UserId, local);
                        }
                        else
                        {
                            // 本地没有记录， 直接就是正常的漫游逻辑
                            long time = startTime == 0 ? 1262275200000L : startTime;
                            friend.UpdateDownTime(time, item.timeSend);
                        }
                    }
                    else
                    {
                        long time = startTime == 0 ? 1262275200000L : startTime;
                        friend.UpdateDownTime(time, item.timeSend);
                    }
                }

                if (!UIUtils.IsNull(lastContent) && lastContent.Length > 30)
                {
                    lastContent = lastContent.Substring(0, 30);
                }

                item.content = lastContent;
                lastContent = friend.ToLastContentTip(item);

                if (item.isRoom == 1 && item.type < 10)
                {
                    lastContent = item.fromUserName + ":" + lastContent;
                }

                friend.Content = lastContent;
                        // 修复：用最近聊天时间填充 LastMsgTime（毫秒转秒），保证最近聊天排序正确
                        if (item.timeSend > 0)
                        {
                            friend.LastMsgTime = item.timeSend / 1000.0;
                        }

                //int a = friend.UpdateFriendLastContent(lastContent, item.type, (int)item.timeSend);

                //if (a == 0 && item.isRoom == 1)
                //{
                //    // 说明本地没有这个好友
                //    HandlerNotRoomMethod(friend, false);
                //}
                fd_list.Add(friend);
            }
            Thread thread = new Thread(() =>
            {
                bool ret = new Friend().UpdateFriendLastContent(fd_list);
                SyncUnreadAndRoam(list);
                Messenger.Default.Send("1", MessageActions.DOWN_CHATLIST_COMPT);
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private void HandlerNotRoomMethod(Friend notFriend, bool refresh)
        {
            // 2026-09-01 多设备修复: null 保护, 避免空引用异常
            if (notFriend == null)
            {
                return;
            }
            Console.WriteLine("出现了一个本地没有此群组的消息：" + notFriend.UserId);

            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/getRoomByJid") //获取群详情
                    .AddParams("access_token", Applicate.Access_Token)
                    .AddParams("roomJid", notFriend.UserId)
                    .NoErrorTip()
                    .Build().Execute((success, result) =>
                    {
                        if (success)
                        {
                            Friend friend = DecodeFriend(result);
                            // 我在此群的状态
                            string roomByme = UIUtils.DecodeString(result, "member"); ;
                            if (UIUtils.IsNull(roomByme))
                            {
                                // 被踢出群组
                                if (friend != null)
                                {
                                    // xmpp退出群组
                                    ExitRoom(friend.UserId);
                                    // 从朋友表中删除
                                    friend.DeleteByUserId();
                                    // 通知界面更新
                                    MessageObject msg = new MessageObject()
                                    { type = kWCMessageType.RoomExit, FromId = friend.UserId, ToId = Applicate.MyAccount.userId };
                                    Messenger.Default.Send(msg, MessageActions.XMPP_UPDATE_ROOM_DELETE);


                                }
                            }
                            friend.MsgNum = 1;
                            friend.Content = notFriend.Content;
                            friend.DeleteByUserId();
                            DBSetting.SQLiteDBContext.Insertable(friend).ExecuteCommand();
                            var msg1 = MessageObjectDao.Instance.GetLastMessage(friend.UserId);
                            if (msg1 != null)
                            {
                                Messenger.Default.Send(msg1, MessageActions.XMPP_SHOW_SINGLE_MESSAGE);
                            }
                        }
                    });
        }

        public void SyscMultiDeviceData()
        {
            // 获取最后离线时间
            string quitTime = Applicate.MyAccount.OfflineTime.ToString();

            // 本地最后的离线时间 修改多点登录被邀请上线后群组丢失问题
            string localTime = LocalDataUtils.GetStringData(Applicate.QUIT_TIME);
            if (!UIUtils.IsNull(localTime))
            {
                long ld = Convert.ToInt64(localTime);
                if (ld > Applicate.DEF_START_TIME)
                {
                    quitTime = localTime;
                }
            }

            // 修改禅道bug9856
            //if (string.Equals(quitTime, Applicate.DEF_START_TIME.ToString()))
            //{
            //    return;
            //}

            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "user/offlineOperation")
                 .AddParams("access_token", Applicate.Access_Token)
                 .AddParams("offlineTime", quitTime)
                 .Build().ExecuteList<SyncBeanList>((sccess, list) =>
                 {
                     if (sccess)
                     {
                         foreach (var item in list.data)
                         {
                             switch (item.tag)
                             {
                                 case "label":
                                     break;
                                 case "friend":
                                     HandleFriendUpdate(item.friendId);
                                     break;
                                 case "room":
                                     HandleGroupUpdate(item.friendId);
                                     break;
                                 default:
                                     break;
                             }
                         }
                     }

                 });
        }

        // 更新自己在群组的密钥
        private void HandleGroupChatkeyUpdate(string roomId, string enchatkey)
        {
            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/updateGroupChatKey")
                .AddParams("roomId", roomId)
                .AddParams("key", enchatkey)
                .NoErrorTip()
                .Build().Execute((state, data) =>
                {
                    Console.WriteLine("" + state);
                });
        }


        //更新好友的信息
        public void HandleFriendUpdate(string userId)
        {
            if (userId.Equals(Applicate.MyAccount.userId))
            {
                // 更新自己的数据
                //handleSelfUpdate();
                return;
            }

            Console.WriteLine("多点登录同步--->更新好友的信息" + userId);

            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "user/get")
                   .AddParams("access_token", Applicate.Access_Token)
                   .AddParams("userId", userId)
                   .Build().Execute((suus, data) =>
                   {
                       if (suus)
                       {
                           Friend jsonFriend = JsonConvert.DeserializeObject<Friend>(JsonConvert.SerializeObject(data)); //使用Friend解析出来

                           //服务器存在关系 新增本地数据
                           if (data.ContainsKey("friends"))
                           {
                               // 解析服务器数据
                               AttentionFriend attention = JsonConvert.DeserializeObject<AttentionFriend>(data["friends"].ToString());
                               jsonFriend.Status = attention.ToFriendStatus();
                               jsonFriend.RemarkName = attention.remarkName;
                               jsonFriend.DhPublicKey = attention.dhMsgPublicKey;
                               jsonFriend.RsaPublicKey = attention.rsaMsgPublicKey;
                               jsonFriend.IsEncrypt = attention.encryptType;

                               // 获取本地数据
                               Friend local = new Friend() { UserId = userId }.GetFdByUserId();
                               // 本地不存在， 新增
                               if (local == null || UIUtils.IsNull(local.NickName))
                               {
                                   // 本地不存在新增
                                   string name = UIUtils.IsNull(attention.remarkName) ? attention.toNickname : attention.remarkName;
                                   //jsonFriend.fristAscII = PinYinUtils.GetFristASCIICode(name);
                                   jsonFriend.TopTime = attention.openTopChatTime;
                                   jsonFriend.InsertAuto();

                                   //添加至好友列表
                                   if (jsonFriend.Status == Friend.STATUS_FRIEND || jsonFriend.Status == 4)
                                   {
                                       Messenger.Default.Send(jsonFriend.UserId, FriendListLayout.ADD_EXISTS_FRIEND);
                                   }
                               }
                               else
                               {
                                   if (local.IsEncrypt != jsonFriend.IsEncrypt)
                                   {
                                       local.IsEncrypt = jsonFriend.IsEncrypt;
                                       local.UpdateEncrypt(local.IsEncrypt);
                                   }

                                   if (!string.Equals(local.RemarkName, jsonFriend.RemarkName))
                                   {
                                       // 其他设备更新了好友备注
                                       local.RemarkName = jsonFriend.RemarkName;
                                       local.UpdateRemarkName();
                                       Messenger.Default.Send(local, MessageActions.UPDATE_FRIEND_REMARKS);
                                   }


                                   local.UpdateFriendState(jsonFriend.Status);
                                   // 本地存在，更新状态
                                   if (jsonFriend.Status == Friend.STATUS_FRIEND || jsonFriend.Status == 4)//是好友关系
                                   {
                                       // 修改 公众置顶后出现在好友列表2020年2月28日12:14:53
                                       if (jsonFriend.UserType < 2)
                                       {
                                           Messenger.Default.Send(local.UserId, FriendListLayout.ADD_EXISTS_FRIEND);
                                       }

                                   }

                                   if (jsonFriend.Status == Friend.STATUS_UNKNOW)//非好友关系
                                   {
                                       Messenger.Default.Send(local, MessageActions.DELETE_FRIEND);
                                   }

                                   if (jsonFriend.Status == Friend.STATUS_18 || jsonFriend.Status == Friend.STATUS_23)//黑名单关系
                                   {
                                       Messenger.Default.Send(local, MessageActions.DELETE_FRIEND);
                                   }


                                   if (jsonFriend.Status == Friend.STATUS_UNKNOW || jsonFriend.Status == Friend.STATUS_UNKNOW)//是好友关系
                                   {
                                       Messenger.Default.Send(local, MessageActions.ADD_BLACKLIST);
                                   }

                                   // 刷新公私钥状态
                                   if (!string.Equals(local.RsaPublicKey, jsonFriend.RsaPublicKey))
                                   {
                                       local.UpdateRsaPublicKey(jsonFriend.RsaPublicKey);
                                   }

                                   if (!string.Equals(local.DhPublicKey, jsonFriend.DhPublicKey))
                                   {
                                       local.UpdateDhPublicKey(jsonFriend.DhPublicKey);
                                   }


                                   // 更新消息保存天数
                                   LocalDataUtils.SetStringData(local.UserId + "chatRecordTimeOut" + Applicate.MyAccount.userId, attention.chatRecordTimeOut.ToString());

                                   // 刷新阅后即焚状态
                                   if (local.IsOpenReadDel != attention.isOpenSnapchat)
                                   {
                                       local.IsOpenReadDel = attention.isOpenSnapchat;
                                       local.UpdateReadDel();
                                       Messenger.Default.Send(local, MessageActions.UPDATE_FRIEND_READDEL);//刷新列表
                                   }

                                   // 刷新免打扰状态
                                   if (attention.offlineNoPushMsg != local.Nodisturb)
                                   {
                                       local.Nodisturb = attention.offlineNoPushMsg;
                                       local.UpdateNodisturb();
                                       Messenger.Default.Send(local, MessageActions.UPDATE_FRIEND_DISTURB);//刷新列表
                                   }


                                   // 刷新置顶状态
                                   if (attention.openTopChatTime > 0 && local.TopTime > 0)
                                   {
                                       // 本地和服务器状态一致 不要刷新
                                       return;
                                   }

                                   if (attention.openTopChatTime == 0 && local.TopTime == 0)
                                   {
                                       // 本地和服务器状态一致 不要刷新
                                       return;
                                   }


                                   Console.WriteLine("更新置顶状态 " + attention.openTopChatTime);
                                   // 更新置顶状态
                                   local.TopTime = attention.openTopChatTime;
                                   local.UpdateTopTime(local.TopTime);
                                   Messenger.Default.Send(local, MessageActions.UPDATE_FRIEND_TOP);
                               }
                           }
                           else
                           {
                               // 服务器不存在关系 删除本地数据
                               Friend local = new Friend() { UserId = userId }.GetFdByUserId();
                               if (local != null)
                               {
                                   if (local.Status == 2)
                                   {
                                       local.UpdateFriendState(Friend.STATUS_UNKNOW);
                                       // 通知界面刷新
                                       Messenger.Default.Send(local, MessageActions.DELETE_FRIEND);
                                   }
                                   else
                                   {
                                       local.UpdateFriendState(Friend.STATUS_UNKNOW);
                                   }
                               }
                           }
                       }
                   });

        }

        //更新群组的信息
        public void HandleGroupUpdate(string roomId)
        {
            Console.WriteLine("多点登录同步--->更新群组的信息");

            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/getRoom") //获取群详情
                    .AddParams("access_token", Applicate.Access_Token)
                    .AddParams("roomId", roomId)
                    .NoErrorTip()
                    .Build().Execute((success, result) =>
                    {
                        if (success)
                        {
                            Friend friend = DecodeFriend(result);
                            // 我在此群的状态
                            string roomByme = UIUtils.DecodeString(result, "member"); ;
                            if (UIUtils.IsNull(roomByme))
                            {
                                // 被踢出群组
                                if (friend != null)
                                {
                                    // xmpp退出群组
                                    ExitRoom(friend.UserId);
                                    // 从朋友表中删除
                                    friend.DeleteByUserId();
                                    // 通知界面更新
                                    MessageObject msg = new MessageObject()
                                    { type = kWCMessageType.RoomExit, FromId = friend.UserId, ToId = Applicate.MyAccount.userId };
                                    Messenger.Default.Send(msg, MessageActions.XMPP_UPDATE_ROOM_DELETE);
                                }
                            }
                            else
                            {
                                if (friend.ExistsFriend())
                                {   // 更新
                                    // 获取本地状态
                                    Friend local = friend.GetFdByUserId();
                                    Member member = JsonConvert.DeserializeObject<Member>(roomByme);

                                    // 更新密钥
                                    if (!UIUtils.IsNull(member.chatKeyGroup))
                                    {
                                        // 解密出 密钥G
                                        string key = RSA.DecryptFromBase64Pk1(member.chatKeyGroup, Applicate.MyAccount.rsaPrivateKey);
                                        // 加密成 chatkeygroup
                                        string chatKey = SecureChatUtil.EncryptChatKey(friend.UserId, key, Applicate.API_KEY);
                                        friend.UpdateChatKeyGroup(chatKey);
                                    }

                                    // 更新消息免打扰
                                    if (local.Nodisturb != member.offlineNoPushMsg)
                                    {
                                        local.Nodisturb = member.offlineNoPushMsg;
                                        local.UpdateNodisturb();
                                        Messenger.Default.Send(local, MessageActions.UPDATE_FRIEND_DISTURB);//刷新列表
                                    }

                                    // 更新置顶状态
                                    if (member.openTopChatTime > 0 && local.TopTime > 0)
                                    {
                                        // 本地和服务器状态一致 不要刷新
                                        return;
                                    }

                                    if (member.openTopChatTime == 0 && local.TopTime == 0)
                                    {
                                        // 本地和服务器状态一致 不要刷新
                                        return;
                                    }

                                    // 更新置顶状态
                                    local.TopTime = member.openTopChatTime;
                                    local.UpdateTopTime(local.TopTime);
                                    Messenger.Default.Send(local, MessageActions.UPDATE_FRIEND_TOP);
                                }
                                else
                                {
                                    // 创建
                                    Member member = JsonConvert.DeserializeObject<Member>(roomByme);

                                    // 更新密钥
                                    if (!UIUtils.IsNull(member.chatKeyGroup))
                                    {
                                        // 解密出 密钥G
                                        string key = RSA.DecryptFromBase64Pk1(member.chatKeyGroup, Applicate.MyAccount.rsaPrivateKey);
                                        // 加密成 chatkeygroup
                                        string chatKey = SecureChatUtil.EncryptChatKey(friend.UserId, key, Applicate.API_KEY);
                                        friend.ChatKeyGroup = chatKey;
                                        friend.IsSecretGroup = 1;
                                    }

                                    friend.Status = Friend.STATUS_FRIEND;
                                    friend.InsertAuto();
                                    long seconds = TimeUtils.CurrentTime() - friend.CreateTime;
                                    JoinRoom(friend.UserId, seconds);
                                    Messenger.Default.Send(friend, MessageActions.ROOM_UPDATE_INVITE);//刷新列表
                                }
                            }
                        }
                        else
                        {
                            // 该群组被解散了
                            Friend friend = new Friend().GetFriendByRoomId(roomId);
                            if (friend != null)
                            {
                                // xmpp退出群组
                                ExitRoom(friend.UserId);
                                // 从朋友表中删除
                                friend.DeleteByUserId();
                                // 通知界面更新
                                MessageObject msg = new MessageObject() { FromId = friend.UserId, ToId = mLoginUserId, type = kWCMessageType.RoomExit };
                                Messenger.Default.Send(msg, MessageActions.XMPP_UPDATE_ROOM_DELETE);
                            }
                        }
                    });
        }


        /// <summary>
        /// 群组信息接口数据 转成 friend 
        /// </summary>
        /// <param name="result"></param>
        private Friend DecodeFriend(Dictionary<string, object> keyValues)
        {

            Friend firend = new Friend();

            //是否显示群已读
            firend.ShowRead = UIUtils.DecodeInt(keyValues, "showRead");
            // 显示群成员
            firend.ShowMember = UIUtils.DecodeInt(keyValues, "showMember");
            // 允许普通群成员私聊
            firend.AllowSendCard = UIUtils.DecodeInt(keyValues, "allowSendCard");
            //允许普通群成员邀请好友
            firend.AllowInviteFriend = UIUtils.DecodeInt(keyValues, "allowInviteFriend");
            //允许普通群成员上传文件
            firend.AllowUploadFile = UIUtils.DecodeInt(keyValues, "allowUploadFile");
            //允许普通群成员召开会议
            firend.AllowConference = UIUtils.DecodeInt(keyValues, "allowConference");
            //允许普通群成员发起讲课
            firend.AllowSpeakCourse = UIUtils.DecodeInt(keyValues, "allowSpeakCourse");
            //是否开启群主验证
            firend.IsNeedVerify = UIUtils.DecodeInt(keyValues, "isNeedVerify");

            //"name": "飞飞",
            //"desc": "1",
            //"subject": "",

            //"userSize": 6,
            //"maxUserSize": 1000,

            //"nickname": "2007",
            //"userId": 10009572,

            //"id": "5ca1c9b90c03d0640281ad58",
            //"jid": "d0495fea6ce34adea87a0fe8764bdd24",
            firend.UserId = UIUtils.DecodeString(keyValues, "jid");
            firend.RoomId = UIUtils.DecodeString(keyValues, "id");
            firend.NickName = UIUtils.DecodeString(keyValues, "name");
            firend.Description = UIUtils.DecodeString(keyValues, "desc");
            firend.LastMsgTime = UIUtils.DecodeDouble(keyValues, "modifyTime");
            firend.CreateTime = UIUtils.DecodeInt(keyValues, "createTime");
            firend.IsGroup = 1;

            // 获取禁言状态
            long talkTime = UIUtils.DecodeLong(keyValues, "talkTime");
            firend.AllBannedTalk = talkTime > 0 ? 1 : 0;

            // 设置群聊消息过期时间
            string outtime = UIUtils.DecodeString(keyValues, "chatRecordTimeOut");
            LocalDataUtils.SetStringData(firend.UserId + "chatRecordTimeOut" + Applicate.MyAccount.userId, outtime);

            return firend;
            //"allowHostUpdate": 1,  // 是否允许群主修改群属性
            //"chatRecordTimeOut":      消息保存天数
            //"createTime": 1554106810, 创建时间
            //"isAttritionNotice": 1,群组减员通知
            //"isLook": 1, // 是否公开群组
            //"modifyTime": 1554116921, // 最后一次发言时间
            //"talkTime": 0, // 禁言时间

            //"videoMeetingNo": "355228" // 群会议地址
            //"call": "305228",// 群会议地址

            //"areaId": 440307,
            //"category": 0,
            //"cityId": 440300,
            //"countryId": 1,
            //"provinceId": 440000,
            //"latitude": 22.608988,
            //"longitude": 114.066209,

            //"s": 1,// 群组状态 -1 锁定 1 正常
        }


        /// <summary>
        ///下载最近聊天列表，必须要下载完所有好友后才能调用
        /// </summary>
        public void DownChatList()
        {
            // 获取最后离线时间
            // 获取最后离线时间
            string quitTime = LocalDataUtils.GetStringData(Applicate.QUIT_TIME);
            if (UIUtils.IsNull(quitTime))
            {
                quitTime = "1546315200000";
            }
            else
            {
                // 偏移一点时间差，防止同一秒的时间
                quitTime = (Convert.ToDouble(quitTime) * 1000 - 5000).ToString();
            }

            //LogUtils.Log("down chat list ing start time: " + quitTime);
            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "tigase/getLastChatList")
                 .AddParams("access_token", Applicate.Access_Token)
                 .AddParams("startTime", quitTime)
                 .AddParams("pageSize", "100")
                 .NoErrorTip()
                 .Build().ExecuteList<JsonLastChats>((sccess, list) =>
                 {

                     if (sccess)
                     {
                         Task.Factory.StartNew(() =>
                         {

                             long tiem = TimeUtils.CurrentTimeMillis();

                             UpdateChatListData(list);
                SyncUnreadAndRoam(list);
                             Console.WriteLine("down chat list compte........");

                             // 批量拉取群聊离线消息
                             BatchJoinMucChat();
                         });
                     }
                     else
                     {
                         // 通知最近消息列表刷新
                         Messenger.Default.Send("0", MessageActions.DOWN_CHATLIST_COMPT);
                     }
                 });
        }

        /// <summary>
        /// 批量拉取群聊离线消息
        /// </summary>
        private void BatchJoinMucChat()
        {
            List<Friend> rooms = new Friend().GetRecentGroupList();
            if (UIUtils.IsNull(rooms))
            {
                return;
            }

            MessageHead head = new MessageHead();
            head.chatType = 2;
            head.from = mLoginUserId + "/" + ShiKuManager.Resource;
            head.to = "service";
            head.messageId = Guid.NewGuid().ToString("N");

            // 获取时间最后上线时间
            long tmp = 0;
            long lasttime = 0;

            PullBatchGroupMessageReqProBuf batch = new PullBatchGroupMessageReqProBuf();
            MessageObject message = new MessageObject();

            foreach (var room in rooms)
            {
                message.FromId = room.UserId;
                lasttime = Convert.ToInt64(message.GetLastTimeStamp());

                if (lasttime != 0)
                {
                    // 如果本地消息不为空，则取本地最后一条，这样会比全局的更准确
                    tmp = lasttime - 30000;
                }
                else
                {
                    tmp = Convert.ToInt64(Applicate.MyAccount.OfflineTime * 1000);
                }

                batch.jidList.Add(room.UserId + "," + tmp);

            }
            batch.endTime = UIUtils.CurrentTimeMillis();
            batch.messageHead = head;
            mSocketCore.SendMessage(batch, Command.COMMAND_BATCH_JOIN_GROUP_REQ);

            Console.WriteLine("批量拉取群聊离线消息");
        }


        private void UpdateFriendLastContentTip(MessageObject msg)
        {
            msg.type = kWCMessageType.Remind;

            if (msg.InsertData() > 0)
            {
                msg.UpdateLastSend();
                NotifactionNormalMessage(msg);
            }
        }

        #endregion

        #region ===================ChatMessage转换成MessageObject======================
        // ChatMessage转换成MessageObject
        public MessageObject ToMessageObject(ChatMessage message)
        {

            MessageObject chat = new MessageObject();
            chat.fromUserId = message.fromUserId;
            chat.fromUserName = message.fromUserName;
            chat.toUserId = message.toUserId;
            chat.toUserName = message.toUserName;
            chat.timeSend = message.timeSend / 1000.0;

            //touserid 为空就是自己
            if (UIUtils.IsNull(chat.toUserId) || "pc".Equals(chat.toUserId))
            {
                chat.toUserId = Applicate.MyAccount.userId;
                message.toUserId = Applicate.MyAccount.userId;
            }

            if (UIUtils.IsNull(chat.toUserName))
            {
                chat.toUserName = Applicate.MyAccount.nickname;
            }

            if (chat.deleteTime > 0)
            {
                chat.deleteTime = message.deleteTime / 1000.0;
            }
            else
            {
                chat.deleteTime = message.deleteTime;
            }

            chat.type = (kWCMessageType)message.type;
            chat.isEncrypt = message.encryptType;
            // 兼容老版本
            if (message.encryptType == 0 && message.isEncrypt)
            {
                chat.isEncrypt = 1;
            }
            chat.signature = message.signature;
            chat.isReadDel = message.isReadDel ? 1 : 0;
            chat.content = message.content;
            chat.objectId = message.objectId;
            chat.fileName = message.fileName;
            chat.fileSize = message.fileSize;
            chat.timeLen = Convert.ToInt32(message.fileTime);
            chat.location_x = message.location_x;
            chat.location_y = message.location_y;
            chat.other = message.other;

            MessageHead head = message.messageHead;

            // 非群聊全部当成单聊处理
            if (head.chatType == 2)
            {
                chat.isGroup = 1;
            }
            else
            {
                chat.isGroup = 0;
            }

            if (chat.isGroup == 1)
            {
                chat.FromId = message.messageHead.to;
                chat.ToId = mLoginUserId;
            }
            else
            {
                chat.FromId = chat.fromUserId;
                chat.ToId = head.to;
            }

            chat.messageId = head.messageId;

            // 我的设备的消息
            if (message.fromUserId.Equals(message.toUserId) && message.fromUserId.Equals(mLoginUserId))
            {
                string device = SplitDevice(head.from);
                string toId = SplitDevice(head.to);
                if (!"Server".Equals(device))
                {
                    chat.FromId = device;
                    chat.ToId = mLoginUserId;
                    chat.fromUserName = chat.FromId;
                    chat.fromUserId = chat.FromId;
                }

                // 修改禅道#9482
                if (!UIUtils.IsNull(toId) && !"pc".Equals(toId))
                {
                    return null;
                }
            }

            return chat;
        }
        #endregion

        private string SplitDevice(string from)
        {

            if (!UIUtils.IsNull(from) && from.Contains("/"))
            {
                string[] value = from.Split('/');
                if (value.Length > 1)
                {
                    return value[1];
                }
                else
                {
                    return "";
                }
            }
            return "";
        }

        #region ===================MessageObject转换成ChatMessage======================
        // MessageObject转换成ChatMessage
        private ChatMessage ToChatMessage(MessageObject message)
        {


            ChatMessage chat = new ChatMessage();
            chat.fromUserId = message.fromUserId;
            chat.fromUserName = message.fromUserName;
            chat.toUserId = message.toUserId;
            chat.toUserName = message.toUserName;
            chat.timeSend = Convert.ToInt64(message.timeSend * 1000);
            if (message.deleteTime > 0)
            {
                chat.deleteTime = Convert.ToInt64(message.deleteTime * 1000); ;
            }
            else
            {
                chat.deleteTime = Convert.ToInt64(message.deleteTime);
            }

            chat.type = Convert.ToInt32(message.type);


            // 加密
            chat.encryptType = message.isEncrypt;
            chat.signature = message.signature;
            chat.isEncrypt = message.isEncrypt == 1;
            chat.isReadDel = message.isReadDel == 1;
            chat.content = message.content;
            chat.objectId = message.objectId;
            chat.fileName = message.fileName;
            chat.fileSize = message.fileSize;
            chat.fileTime = message.timeLen;
            chat.location_x = message.location_x;
            chat.location_y = message.location_y;

            MessageHead head = new MessageHead();
            head.messageId = message.messageId;
            head.chatType = message.isGroup + 1; // 单聊1， 群聊2
            head.from = message.FromId + "/" + ShiKuManager.Resource;

            if (message.fromUserId.Equals(message.toUserId))
            {
                // 发送给我的设备的消息
                head.to = message.FromId + "/" + message.toUserName;
                chat.toUserName = message.fromUserName;
            }
            else
            {
                head.to = message.toUserId;
            }

            chat.messageHead = head;
            return chat;
        }
        #endregion
    }
}

