
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;

namespace WinFrmTalk
{
    /// <summary>
    /// 2026-11-20 消息变更（编辑/删除）持久化重试队列。
    /// 服务器写失败时不丢操作：本地已生效 + 已发 203，这里在后台按退避重试，直到落库成功。
    /// </summary>
    public class MsgChangeRetry
    {
        private class Item
        {
            public string action { get; set; }
            public string messageId { get; set; }
            public string content { get; set; }
            public int type { get; set; }
            public string roomJid { get; set; }
            public int tries { get; set; }
            public long nextTry { get; set; }
        }

        private static readonly object LockObj = new object();
        private static List<Item> items = null;
        private static Timer timer = null;

        private static string FilePath
        {
            get
            {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinFrmTalk");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                return Path.Combine(dir, "pending_msg_changes.json");
            }
        }

        private static long NowMs()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        private static List<Item> Load()
        {
            if (items != null) return items;
            try
            {
                if (File.Exists(FilePath))
                {
                    items = JsonConvert.DeserializeObject<List<Item>>(File.ReadAllText(FilePath));
                }
            }
            catch { }
            if (items == null) items = new List<Item>();
            return items;
        }

        private static void Save()
        {
            try { File.WriteAllText(FilePath, JsonConvert.SerializeObject(items)); }
            catch { }
        }

        public static void EnqueueDelete(string messageId, int isGroup, string roomJid)
        {
            Enqueue(new Item
            {
                action = "delete",
                messageId = messageId,
                content = "",
                type = isGroup == 0 ? 1 : 2,
                roomJid = roomJid,
                tries = 0,
                nextTry = 0
            });
        }

        public static void EnqueueEdit(string messageId, string content, int isGroup, string roomJid)
        {
            Enqueue(new Item
            {
                action = "edit",
                messageId = messageId,
                content = content,
                type = isGroup == 0 ? 1 : 2,
                roomJid = roomJid,
                tries = 0,
                nextTry = 0
            });
        }

        private static void Enqueue(Item it)
        {
            if (it == null || string.IsNullOrEmpty(it.messageId)) return;
            lock (LockObj)
            {
                Load();
                items.RemoveAll(x => x.messageId == it.messageId && x.action == it.action);
                items.Add(it);
                Save();
            }
            Start();
        }

        private static void Start()
        {
            lock (LockObj)
            {
                if (timer != null) return;
                timer = new Timer(OnTick, null, 2000, 10000);
            }
        }

        private static void OnTick(object state)
        {
            List<Item> due;
            lock (LockObj)
            {
                Load();
                long now = NowMs();
                due = items.Where(x => x.nextTry <= now).ToList();
            }
            foreach (Item it in due) Send(it);
        }

        private static void Send(Item it)
        {
            try
            {
                if (string.IsNullOrEmpty(Applicate.Access_Token)) return;
                if (Applicate.URLDATA == null || Applicate.URLDATA.data == null) return;
                string apiUrl = Applicate.URLDATA.data.apiUrl;
                if (string.IsNullOrEmpty(apiUrl)) return;

                var req = HttpUtils.Instance.Get()
                    .Url(apiUrl + (it.action == "delete" ? "tigase/deleteMsg" : "tigase/updateMsg"))
                    .AddParams("access_token", Applicate.Access_Token)
                    .AddParams("type", it.type.ToString());

                if (it.action == "delete")
                {
                    req = req.AddParams("delete", "3")
                             .AddParams("messageId", it.messageId)
                             .AddParams("roomJid", it.roomJid == null ? "" : it.roomJid);
                }
                else
                {
                    req = req.AddParams("messageId", it.messageId)
                             .AddParams("content", it.content == null ? "" : it.content)
                             .AddParams("roomJid", it.roomJid == null ? "" : it.roomJid);
                }

                req.Build().ExecuteJson<object>((ok, obj) =>
                {
                    lock (LockObj)
                    {
                        Load();
                        if (ok)
                        {
                            items.RemoveAll(x => x.messageId == it.messageId && x.action == it.action);
                        }
                        else
                        {
                            Item cur = items.FirstOrDefault(x => x.messageId == it.messageId && x.action == it.action);
                            if (cur != null)
                            {
                                cur.tries = cur.tries + 1;
                                cur.nextTry = NowMs() + Backoff(cur.tries);
                            }
                        }
                        Save();
                    }
                });
            }
            catch { }
        }

        private static long Backoff(int tries)
        {
            if (tries <= 1) return 3000;
            if (tries == 2) return 10000;
            if (tries == 3) return 30000;
            return 60000;
        }
    }
}
