using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using WinFrmTalk.Model;

namespace WinFrmTalk.Helper
{
    public class CollectUtils
    {
        private static string collmsgId = null;

        #region http收藏消息

        /// <summary>
        /// 单条收藏
        /// </summary>
        /// <param name="msg"></param>  
        internal static void CollectMessage(MessageObject msg, bool saveEmoji = false)
        {
            if (msg == null || UIUtils.IsNull(msg.messageId))
            {
                LogUtils.Log("收藏数据为空");
                return;
            }

            if (msg.messageId.Equals(collmsgId))
            {
                Console.WriteLine("快速点击收藏");
                return;
            }

            if (!(msg.type == kWCMessageType.Video || msg.type == kWCMessageType.Text || msg.type == kWCMessageType.Image || msg.type == kWCMessageType.File || msg.type == kWCMessageType.Voice))
            {
                HttpUtils.Instance.ShowTip("文字/图片/语音/视频/文件 消息才能被收藏", true);
                return;
            }


            // 修改禅道8282bug,存表情和收藏参数emoji有细微的差别，emoji是msg，收藏是msgid
            string emoji = saveEmoji ? toSaveEmojiParams(msg) : toCollectParams(msg);
            collmsgId = msg.messageId;


            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "user/emoji/add") //收藏
                .AddParams("access_token", Applicate.Access_Token)
                .AddParams("emoji", emoji)
                .NoErrorTip()
                 .Build().AddErrorListener((code, err) =>
                 {
                     if (code == 0)
                     {
                         HttpUtils.Instance.ShowTip("不能重复收藏消息");
                     }
                     else
                     {
                         //code == 105002 端到端加密消息
                         HttpUtils.Instance.ShowTip(err);
                     }
                     collmsgId = null;
                 })
                .Execute((sccess, room) =>
                {
                    collmsgId = null;
                    if (sccess)
                    {
                        if (saveEmoji)
                        {
                            HttpUtils.Instance.ShowTip("存表情成功");
                        }
                        else
                        {
                            HttpUtils.Instance.ShowTip("收藏成功");
                            // 更新收藏页
                            Messenger.Default.Send("1", MessageActions.UPDATE_COLLECT_LIST);
                        }
                    }
                });
        }

        /// <summary>
        /// 批量收藏
        /// </summary>
        /// <param name="msglst"></param>
        internal static void CollectMessage(List<MessageObject> msgList)
        {
            if (msgList == null || UIUtils.IsNull(msgList))
            {
                LogUtils.Log("收藏数据为空");
                return;
            }

            for (int index = msgList.Count - 1; index > -1; index--)
            {
                MessageObject msg = msgList[index];

                if (!(msg.type == kWCMessageType.Video || msg.type == kWCMessageType.Text || msg.type == kWCMessageType.Image || msg.type == kWCMessageType.File || msg.type == kWCMessageType.Voice))
                {
                    msgList.Remove(msg);
                    continue;
                }

                if (msg.isReadDel == 1)
                {
                    msgList.Remove(msg);
                    continue;
                }
            }

            if (UIUtils.IsNull(msgList))
            {
                HttpUtils.Instance.ShowTip("选中消息中 文字/图片/语音/视频/文件 消息才能被收藏", true);
                return;
            }

            //收藏
            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "user/emoji/add")
                .AddParams("access_token", Applicate.Access_Token)
                .AddParams("emoji", toCollectParams(msgList))
                .Build()
                .Execute((sccess, room) =>
                {
                    if (sccess)
                    {
                        HttpUtils.Instance.ShowTip("全部收藏成功");
                        Messenger.Default.Send("1", MessageActions.UPDATE_COLLECT_LIST);
                    }
                });
        }


        // 多条收藏字符串拼接
        private static string toCollectParams(List<MessageObject> msgs)
        {
            JArray jSONArray = new JArray();
            foreach (var datas in msgs)
            {
                JObject data = new JObject();
                if (!(datas.type == kWCMessageType.Video || datas.type == kWCMessageType.Text || datas.type == kWCMessageType.Image || datas.type == kWCMessageType.File || datas.type == kWCMessageType.Voice))
                {
                    continue;
                }

                if (datas.type == kWCMessageType.Gif)
                {
                    data.Add("url", datas.content);
                }
                else
                {
                    data.Add("msgId", datas.messageId);
                }
                data.Add("msg", datas.content);
                data.Add("type", GetType(datas.type));

                if (datas.isGroup == 1)
                {
                    data.Add("roomJid", datas.toUserId);
                }

                jSONArray.Add(data);
            }
            return jSONArray.ToString();
        }


        // 单条收藏字符串拼接
        private static string toCollectParams(MessageObject datas)
        {

            JArray jSONArray = new JArray();
            JObject data = new JObject();
            data.Add("msgId", datas.messageId);
            data.Add("msg", datas.content);
            data.Add("type", GetType(datas.type));

            if (datas.isGroup == 1)
            {
                data.Add("roomJid", datas.toUserId);
            }
            jSONArray.Add(data);
            return jSONArray.ToString();
        }

        // 存表情字符串拼接
        private static string toSaveEmojiParams(MessageObject datas)
        {
            JArray jSONArray = new JArray();
            JObject data = new JObject();
            data.Add("url", datas.content);
            data.Add("msg", datas.content);
            data.Add("type", 6);
            // 增加roomJid会导致存表情到收藏中去
            //if (datas.isGroup == 1)
            //{
            //    data.Add("roomJid", datas.toUserId);
            //}
            jSONArray.Add(data);
            return jSONArray.ToString();
        }

        // 存链接字符串拼接
        private static string toSaveLinkParams(string url)
        {
            JArray jSONArray = new JArray();
            JObject data = new JObject();
            data.Add("type", "5");
            data.Add("msg", url);
            data.Add("collectContent", url);
            data.Add("collectType", "-1");
            jSONArray.Add(data);
            return jSONArray.ToString();
        }


        // 将消息类型转换为收藏类型
        private static string GetType(kWCMessageType type)
        {
            int value = 0;
            switch (type)
            {
                case kWCMessageType.Image:
                    value = 1;
                    break;
                case kWCMessageType.Video:
                    value = 2;
                    break;
                case kWCMessageType.File:
                    value = 3;
                    break;
                case kWCMessageType.Voice:
                    value = 4;
                    break;
                case kWCMessageType.Text:
                    value = 5;
                    break;
                case kWCMessageType.Gif:
                    value = 6;
                    break;
                case kWCMessageType.SDKLink:
                    value = 7;
                    break;
            }
            return value.ToString();
        }

        #endregion


        #region http收藏表情
        /// <summary>
        /// 收藏表情
        /// </summary>
        /// <param name="msg"></param>
        internal static void CollectExpression(MessageObject msg)
        {
            if (msg == null || UIUtils.IsNull(msg.messageId))
            {
                LogUtils.Log("收藏表情为空");
                return;
            }

            CollectMessage(msg, true);
        }
        #endregion


        #region http收藏链接
        /// <summary>
        /// 收藏链接
        /// </summary>
        /// <param name="url"></param>
        /// <param name="roomJid"></param>
        internal static void CollectLink(string url, string roomJid)
        {

            MessageObject message = new MessageObject();
            message.type = kWCMessageType.SDKLink;
            message.content = url;

            if (!UIUtils.IsNull(roomJid))
            {
                message.isGroup = 1;
                message.toUserId = roomJid;
            }
            message.messageId = Guid.NewGuid().ToString("N");

            CollectMessage(message);
        }

        #endregion


        #region 浏览器收藏一个链接
        /// <summary>
        /// 收藏链接
        /// </summary>
        /// <param name="url"></param>
        /// <param name="roomJid"></param>
        internal static void CollectLink(string url)
        {
            string text = toSaveLinkParams(url);

            HttpUtils.Instance.Post().Url(Applicate.URLDATA.data.apiUrl + "user/emoji/add") //收藏
                .AddParams("access_token", Applicate.Access_Token)
                .AddParams("emoji", text)
                .NoErrorTip()
                 .Build().AddErrorListener((code, err) =>
                 {
                     if (code == 0)
                     {
                         HttpUtils.Instance.ShowTip("不能重复收藏消息");
                     }
                     else
                     {
                         //code == 105002 端到端加密消息
                         HttpUtils.Instance.ShowTip(err);
                     }
                 })
                .Execute((sccess, room) =>
                {
                    if (sccess)
                    {
                        HttpUtils.Instance.ShowTip("收藏成功");
                        // 更新收藏页
                        Messenger.Default.Send("1", MessageActions.UPDATE_COLLECT_LIST);
                    }
                });
        }

        #endregion

        /// <summary>
        /// 单条服务器删除
        /// </summary>
        /// <param name="msg"></param>
        internal static void DelServerMessages(MessageObject msg, bool isLastMsg)
        {
            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "tigase/deleteMsg"). //删除群组
                AddParams("access_token", Applicate.Access_Token).
                AddParams("type", msg.isGroup == 0 ? "1" : "2").    //1 单聊  2 群聊
                AddParams("delete", "1").   //1： 删除属于自己的消息记录 2：撤回 删除整条消息记录
                AddParams("messageId", msg.messageId).
                AddParams("roomJid", msg.toUserId).
                Build().ExecuteJson<object>((sccess, obj) =>   //返回值说明： text：加密后的内容
                {
                    //删除成功
                    if (sccess)
                    {
                        int result = msg.DeleteData();
                        HttpUtils.Instance.ShowTip("删除成功");
                        //通知最近聊天列表更新
                        if (isLastMsg)
                        {

                            // 更新最后一条消息内容
                            MessageObject local = msg.GetLastMessage();
                            var friend = msg.GetFriend();

                            string content = string.Empty;
                            double timesend = TimeUtils.CurrentTimeDouble();
                            if (!UIUtils.IsNull(local.messageId))
                            {
                                if (local.isReadDel == 1)
                                {
                                    content = "[阅后即焚消息]";
                                }
                                else
                                {
                                    timesend = local.timeSend;
                                    content = friend.ToLastContentTip(local.type, local.content, local.fromUserId, local.fromUserName, local.toUserName);
                                    if (friend.IsGroup == 1)
                                    {
                                        content = local.fromUserName + ":" + content;
                                    }
                                }

                              
                            }

                            friend.UpdateLastContent(content, timesend);
                            Messenger.Default.Send(friend, token: MessageActions.UPDATE_FRIEND_LAST_CONTENT);
                        }

                        Messenger.Default.Send(msg, MessageActions.CLEAR_CURRENT_CONTENT);
                    }
                });
        }

        /// <summary>
        /// 多选服务器删除
        /// </summary>
        /// <param name="list_msgs"></param>
        internal static void DelServerMessages(List<MessageObject> list_msgs, bool isLastMsg)
        {
            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "tigase/deleteMsg"). //删除群组
                AddParams("access_token", Applicate.Access_Token).
                AddParams("type", list_msgs[0].isGroup == 0 ? "1" : "2").    //1 单聊  2 群聊
                AddParams("delete", "1").   //1： 删除属于自己的消息记录 2：撤回 删除整条消息记录
                AddParams("messageId", UIUtils.AppendMessageIds(list_msgs)).
                AddParams("roomJid", list_msgs[0].toUserId).
                Build().ExecuteJson<object>((sccess, obj) =>   //返回值说明： text：加密后的内容
                {
                    //删除成功
                    if (sccess)
                    {
                        // 从数据库中移除
                        foreach (var item in list_msgs)
                        {
                            item.DeleteData();
                        }

                        //通知最近聊天列表更新
                        if (isLastMsg)
                        {
                            // 更新最后一条消息内容
                            MessageObject local = list_msgs[list_msgs.Count - 1].GetLastMessage();
                            var friend = list_msgs[list_msgs.Count - 1].GetFriend();

                            string content = string.Empty;
                            double timesend = TimeUtils.CurrentTimeDouble();
                            if (!UIUtils.IsNull(local.messageId))
                            {
                                timesend = local.timeSend;
                                content = friend.ToLastContentTip(local.type, local.content, local.fromUserId, local.fromUserName, local.toUserName);
                                if (friend.IsGroup == 1)
                                {
                                    content = local.fromUserName + ":" + content;
                                }
                            }

                            friend.UpdateLastContent(content, timesend);
                            Messenger.Default.Send(friend, token: MessageActions.UPDATE_FRIEND_LAST_CONTENT);
                        }

                        HttpUtils.Instance.ShowTip("全部删除成功");
                    }
                });
        }



        public static bool EnableForward(Friend friend)
        {
            if (friend == null || UIUtils.IsNull(friend.UserId))
            {
                HttpUtils.Instance.ShowTip("数据错误");
                return true;
            }

            if (friend.IsSecretGroup == 1)
            {
                if (UIUtils.IsNull(friend.ChatKeyGroup))
                {
                    // 没有密钥
                    HttpUtils.Instance.ShowTip("不能转发消息到密钥丢失群");
                    return true;
                }

                if (friend.IsLostKeyGroup == 1)
                {
                    // 没有密钥
                    HttpUtils.Instance.ShowTip("不能转发消息到密钥丢失群");
                    return true;
                }
            }

            if (friend.IsGroup == 1)
            {

                var member = RoomMemberDao.Instance.GetRoomMemberById(friend.RoomId, Applicate.MyAccount.userId);
                int role = member.role;

                if (role == 4)
                {
                    HttpUtils.Instance.ShowTip("隐身人不能发送消息");
                    return true;
                }

                if (role == 3)
                {
                    //管理员和群主除外 是否全体禁言
                    if (friend.AllBannedTalk != 0)
                    {
                        // 全体禁言
                        HttpUtils.Instance.ShowTip("不能在全体禁言群发言");
                        return true;
                    }

                    //是否单个禁言
                    if (member.talkTime > TimeUtils.CurrentTime())
                    {
                        HttpUtils.Instance.ShowTip("您已被禁止在此群发言");
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 指示是否可以转发
        /// </summary>
        /// <param name="friend"></param>
        /// <returns>返回值为0则可以转发，<0则不允许转发
        /// <para>-1=friend null;-2=没有群密钥;-3=隐身人;-4=禁言</para>
        /// </returns>
        public static int EnableForward_NotTip(Friend friend)
        {
            if (friend == null || UIUtils.IsNull(friend.UserId))
            {
                //HttpUtils.Instance.ShowTip("数据错误");
                return -1;
            }

            if (friend.IsSecretGroup == 1)
            {
                if (UIUtils.IsNull(friend.ChatKeyGroup))
                {
                    // 没有密钥
                    //HttpUtils.Instance.ShowTip("不能转发消息到密钥丢失群");
                    return -2;
                }

                if (friend.IsLostKeyGroup == 1)
                {
                    // 没有密钥
                    //HttpUtils.Instance.ShowTip("不能转发消息到密钥丢失群");
                    return -2;
                }
            }

            if (friend.IsGroup == 1)
            {
                RoomMember member = RoomMemberDao.Instance.GetRoomMemberById(friend.RoomId, Applicate.MyAccount.userId);

                if (member.role == 4)
                {
                    //HttpUtils.Instance.ShowTip("隐身人不能发送消息");
                    return -3;
                }

                if (member.role == 3)
                {

                    //管理员和群主除外 是否全体禁言
                    if (friend.AllBannedTalk != 0)
                    {
                        // 全体禁言
                        //HttpUtils.Instance.ShowTip("不能发送讲课到全体禁言群");
                        return -4;
                    }

                    //是否单个禁言
                    if (member.talkTime != 0)
                    {
                        //HttpUtils.Instance.ShowTip("您已被禁止在此群发言");
                        return -4;
                    }
                }
            }

            return 0;
        }
    }
}

