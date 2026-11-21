
using System;
using System.IO;
using Newtonsoft.Json.Linq;
using WinFrmTalk.Model;

namespace WinFrmTalk
{
    /// <summary>
    /// 2026-11-20 msgChanges incremental sync: apply edit/delete changes missed while offline.
    /// </summary>
    public class MsgChangeSync
    {
        private static string TsFile()
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinFrmTalk");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return Path.Combine(dir, "msgchange_ts.txt");
        }

        private static long ReadTs()
        {
            try
            {
                string p = TsFile();
                if (!File.Exists(p)) return 0;
                long v = 0;
                long.TryParse(File.ReadAllText(p).Trim(), out v);
                return v;
            }
            catch { return 0; }
        }

        private static void WriteTs(long ts)
        {
            try { File.WriteAllText(TsFile(), ts.ToString()); } catch { }
        }

        public static void Start()
        {
            try { Sync(); }
            catch (Exception ex) { try { LogUtils.Error("MsgChangeSync.Start " + ex.Message); } catch { } }
        }

        public static void Sync()
        {
            if (Applicate.URLDATA == null || Applicate.URLDATA.data == null) return;
            if (string.IsNullOrEmpty(Applicate.Access_Token)) return;
            long since = ReadTs();
            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "tigase/msgChanges")
                .AddParams("access_token", Applicate.Access_Token)
                .AddParams("since", since.ToString())
                .AddParams("pageSize", "200")
                .Build().ExecuteJson<JObject>((ok, obj) =>
                {
                    try
                    {
                        if (!ok || obj == null) return;
                        JObject data = obj["data"] as JObject;
                        if (data == null) return;
                        JArray list = data["list"] as JArray;
                        if (list != null)
                        {
                            foreach (JToken it in list) { ApplyOne(it); }
                        }
                        long next = since;
                        JToken nsTok = data["nextSince"];
                        if (nsTok != null) long.TryParse(nsTok.ToString(), out next);
                        if (next > since) WriteTs(next);
                    }
                    catch (Exception ex) { try { LogUtils.Error("MsgChangeSync.apply " + ex.Message); } catch { } }
                });
        }

        private static void ApplyOne(JToken it)
        {
            try
            {
                if (it == null) return;
                string action = it.Value<string>("action");
                string mid = it.Value<string>("messageId");
                string newContent = it.Value<string>("content");
                string peerId = it.Value<string>("peerId");
                string fromId = it.Value<string>("fromUserId");
                int chatType = it.Value<int>("chatType");
                if (string.IsNullOrEmpty(mid)) return;

                MessageObject probe = new MessageObject();
                probe.messageId = mid;
                probe.isGroup = chatType == 2 ? 1 : 0;
                if (chatType == 2)
                {
                    probe.toUserId = peerId;
                }
                else
                {
                    probe.fromUserId = fromId;
                    probe.toUserId = peerId;
                }

                MessageObject target = probe.GetMessageObject();
                if (target == null || string.IsNullOrEmpty(target.messageId)) return;

                if ("read" == action)
                {
                    probe.UpdateIsRead(mid);
                    try { Messenger.Default.Send(probe, MessageActions.XMPP_UPDATE_RECEIVED_READ); } catch { }
                    return;
                }

                if ("delete" == action)
                {
                    target.DeleteData();
                }
                else
                {
                    if (newContent == null) return;
                    if (target.content == newContent) return;
                    target.content = newContent;
                    target.UpdateData();
                }

                try { Messenger.Default.Send(target, MessageActions.XMPP_UPDATE_NORMAL_MESSAGE); } catch { }
            }
            catch (Exception ex) { try { LogUtils.Error("MsgChangeSync.one " + ex.Message); } catch { } }
        }
    }
}
