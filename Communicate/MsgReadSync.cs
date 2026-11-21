using System;

namespace WinFrmTalk.Communicate
{
    /// <summary>
    /// 2026-11-19 已读回执落库：把服务端对应消息标为已读，重登后仍显示已读
    /// </summary>
    public static class MsgReadSync
    {
        public static void MarkRead(string messageId)
        {
            if (string.IsNullOrEmpty(messageId)) return;
            try
            {
                string userId = Applicate.MyAccount == null ? "0" : Applicate.MyAccount.userId.ToString();
                HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "tigase/markMsgRead")
                    .AddParams("access_token", Applicate.Access_Token)
                    .AddParams("messageId", messageId)
                    .AddParams("senderId", userId)
                    .Build().ExecuteJson<object>((success, obj) => { });
            }
            catch { }
        }
    }
}
