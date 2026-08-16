using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinFrmTalk.Model.dao
{
    public class MessageObjectDao
    {
        #region 单例模式
        private Dictionary<string, bool> mCreateTable;
        private static readonly MessageObjectDao instance = new MessageObjectDao();

        /// <summary>
        /// 显式的静态构造函数用来告诉C#编译器在其内容实例化之前不要标记其类型
        /// </summary>
        static MessageObjectDao() { }

        private MessageObjectDao()
        {
            mCreateTable = new Dictionary<string, bool>();
        }

        public static MessageObjectDao Instance
        {
            get
            {
                return instance;
            }
        }
        #endregion

        #region 创建表
        /// <summary>
        /// 修改数据和插入数据使用此方法
        /// <param name="tableName">表名</param>
        /// </summary>
        public bool CreatMessageTable(string tableName)
        {

            if ("msg_Null".Equals(tableName))
            {
                return false;
            }

            if ("msg_".Equals(tableName))
            {
                return false;
            }

            if (mCreateTable.ContainsKey(tableName))
            {
                return true;
            }

            var db = DBSetting.SQLiteDBContext;
            db.MappingTables.Add("MessageObject", tableName);

            //创建数据库表
            try
            {
                var result = db.Queryable<sqlite_master>().Where(s => SqlFunc.Equals(s.Name, tableName) && SqlFunc.Equals(s.Type, "table"));
                if (result != null && result.Count() < 1)
                {
                    db.CodeFirst.SetStringDefaultLength(100).InitTables(typeof(MessageObject));
                }

                mCreateTable.Add(tableName, true);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 判断表是否存在：查询和删除使用此方法
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns></returns>
        public bool IsTableExists(string tableName)
        {
            if ("msg_Null".Equals(tableName))
            {
                return false;
            }

            if ("msg_".Equals(tableName))
            {
                return false;
            }

            if (mCreateTable.ContainsKey(tableName))
            {
                return true;
            }

            var db = DBSetting.SQLiteDBContext;
            db.MappingTables.Add("MessageObject", tableName);

            //创建数据库表
            try
            {
                var result = db.Queryable<sqlite_master>().Where(s => SqlFunc.Equals(s.Name, tableName) && SqlFunc.Equals(s.Type, "table"));
                return result != null && result.Count() > 0; ;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region 最近消息列表搜索文字
        /// <summary>
        /// 分页获取数据列表（模糊查询）
        /// <para>按时间降序排序</para>
        /// </summary>
        /// <param name="type">获取对应类型的消息(小于零时获取全部)</param>
        /// <param name="PageIndex">第几页（0和1都是第一页）</param>
        /// <param name="PageSize">一页多少行消息（同一功能调用该方法请固定使用一个值）</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public List<MessageObject> GetTextContentList(string content, string friendUserId)
        {
            string tabName = MessageObject.Prefex + friendUserId;

            if (IsTableExists(tabName))
            {
                //var db = DBSetting.SQLiteDBContext;
                //try
                //{
                //    var AsynResult = DBSetting.SQLiteDBContext.Ado.UseTranAsync(() =>
                //    {
                //        var list = db.Queryable<MessageObject>().AS(tabName)
                //         .Where(m => m.type == kWCMessageType.Text)
                //         .Where(m => m.content.Contains(content))
                //         .Where(m => m.isReadDel == 0)
                //         .OrderBy(it => it.timeSend, OrderByType.Desc)
                //         .ToPageList(1, 10);

                //        return list;
                //    });
                //    return AsynResult.Result.Data;
                //}
                //catch (Exception ex)
                //{
                //    db.Ado.RollbackTran();
                //    throw ex;
                //}

                var list = DBSetting.SQLiteDBContext.Queryable<MessageObject>().AS(tabName)
                     .Where(m => m.type == kWCMessageType.Text)
                     .Where(m => m.content.Contains(content))
                     .Where(m => m.isReadDel == 0)
                     .OrderBy(it => it.timeSend, OrderByType.Desc)
                     .ToPageList(1, 10);

                return list;
            }
            else
            {
                return null;
            }
        }

        public Dictionary<Friend, List<MessageObject>> GetTextContentDic_Ado(List<Friend> fds, string content)
        {
            Dictionary<Friend, List<MessageObject>> result = new Dictionary<Friend, List<MessageObject>>();
            var db = DBSetting.SQLiteDBContext;
            try
            {
                db.Ado.BeginTran();
                foreach (Friend fd in fds)
                {
                    string tabName = MessageObject.Prefex + fd.UserId;

                    if (IsTableExists(tabName))
                    {
                        var list = db.Queryable<MessageObject>().AS(tabName)
                         .Where(m => m.type == kWCMessageType.Text)
                         .Where(m => m.content.Contains(content))
                         .Where(m => m.isReadDel == 0)
                         .OrderBy(it => it.timeSend, OrderByType.Desc)
                         .ToPageList(1, 10);

                        result.Add(fd, list);
                    }
                }
                db.Ado.CommitTran();
            }
            catch (Exception ex)
            {
                db.Ado.RollbackTran();
                throw ex;
            }
            return result;
        }
        #endregion


 


        #region  获取消息列表-消息记录页面
        /// <summary>
        /// 获取消息记录
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="frist"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        internal List<MessageObject> GetMessageRecords(string userId, MessageObject frist, int pagerIndex, int PageSize = 30)
        {
            string tabName = MessageObject.Prefex + userId;
            if (IsTableExists(tabName))
            {

                if (frist == null)
                {
                    var list = DBSetting.SQLiteDBContext.Queryable<MessageObject>().AS(tabName).
                  Where(it => it.deleteTime > TimeUtils.CurrentIntTime() || it.deleteTime <= 0).
                  Where(it => it.type != kWCMessageType.IsRead).
                  Where(it => it.type != kWCMessageType.Withdraw).
                  Where(it => it.type != kWCMessageType.RoomLiveForceStop).
                  Where(it => it.type != kWCMessageType.RoomLiveStart).
                  Where(it => it.type != kWCMessageType.RoomLiveStop).
                  OrderBy(it => it.timeSend, OrderByType.Desc).
                  OrderBy(it => it.insertId, OrderByType.Desc)
                  .ToPageList(pagerIndex, PageSize);
                    return list;
                }
                else
                {
                    var list = DBSetting.SQLiteDBContext.Queryable<MessageObject>().AS(tabName).
                  Where(it => it.timeSend < frist.timeSend).
                  Where(it => it.deleteTime > TimeUtils.CurrentIntTime() || it.deleteTime <= 0).
                  Where(it => it.type != kWCMessageType.IsRead).
                  Where(it => it.type != kWCMessageType.Withdraw).
                  Where(it => it.type != kWCMessageType.RoomLiveForceStop).
                  Where(it => it.type != kWCMessageType.RoomLiveStart).
                  Where(it => it.type != kWCMessageType.RoomLiveStop).
                  OrderBy(it => it.timeSend, OrderByType.Desc).
                  OrderBy(it => it.insertId, OrderByType.Desc)
                  .ToPageList(pagerIndex, PageSize);
                    return list;
                }
            }

            return null;
        }


        #region 分页获取数据列表（过滤某些数据类型）
        /// <summary>
        /// 获取消息记录，在某个时间区间的数据
        /// </summary>
        /// <returns></returns>
        public List<MessageObject> GetMessageRecordsByTime(string userId, double starttime, double endtime, int PageSize = 30)
        {
            string tabName = MessageObject.Prefex + userId;
            if (IsTableExists(tabName))
            {
                var list = DBSetting.SQLiteDBContext.Queryable<MessageObject>().AS(tabName).
                    Where(it => it.timeSend >= starttime).
                    Where(it => it.timeSend < endtime).
                    Where(it => it.type != kWCMessageType.IsRead).
                    Where(it => it.type != kWCMessageType.Withdraw).
                    Where(it => it.deleteTime > TimeUtils.CurrentIntTime() || it.deleteTime <= 0).
                    OrderBy(it => it.timeSend, OrderByType.Desc).
                    OrderBy(it => it.insertId, OrderByType.Desc)
                    .ToPageList(1, PageSize);
                return list;
            }

            return null;
        }

        /// <summary>
        /// 获取消息记录，查询某个时间之后的消息
        /// </summary>
        public List<MessageObject> GetMessageRecordsByTimeAfter(string userId, double time, int pagerIndex = 1, int PageSize = 30)
        {
            string tabName = MessageObject.Prefex + userId;
            if (IsTableExists(tabName))
            {
                var list = DBSetting.SQLiteDBContext.Queryable<MessageObject>().AS(tabName).
                    Where(it => it.type != kWCMessageType.IsRead).
                    Where(it => it.type != kWCMessageType.Withdraw).
                    Where(it => it.timeSend > time).
                    Where(it => it.deleteTime > TimeUtils.CurrentIntTime() || it.deleteTime <= 0).
                    OrderBy(it => it.timeSend, OrderByType.Desc).
                    OrderBy(it => it.insertId, OrderByType.Desc).
                    ToPageList(pagerIndex, PageSize);
                return list;
            }

            return null;
        }

        /// <summary>
        /// 获取消息记录，查询某个时间之后的消息
        /// </summary>
        public List<MessageObject> GetMessageRecordsByTimeBefore(string userId, double time, int pagerIndex = 1, int PageSize = 30)
        {
            string tabName = MessageObject.Prefex + userId;
            if (IsTableExists(tabName))
            {
                var list = DBSetting.SQLiteDBContext.Queryable<MessageObject>().AS(tabName).
                    Where(it => it.type != kWCMessageType.IsRead).
                    Where(it => it.type != kWCMessageType.Withdraw).
                    Where(it => it.timeSend < time).
                    Where(it => it.deleteTime > TimeUtils.CurrentIntTime() || it.deleteTime <= 0).
                    OrderBy(it => it.timeSend, OrderByType.Desc).
                    OrderBy(it => it.insertId, OrderByType.Desc).
                    ToPageList(pagerIndex, PageSize);
                return list;
            }

            return null;
        }


        #endregion


        #endregion

        #region 获取消息列表-历史消息记录页面

        /// <summary>
        /// 查询所有历史消息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="pagerIndex"></param>
        /// <returns></returns>
        public List<MessageObject> GetHistoryMessagesAll(string userId, int pagerIndex)
        {
            string tabName = MessageObject.Prefex + userId;

            if (IsTableExists(tabName))
            {
                var list = DBSetting.SQLiteDBContext.Queryable<MessageObject>().AS(tabName)
                        .Where(it => it.type != kWCMessageType.IsRead)
                        .Where(it => it.type != kWCMessageType.Withdraw)
                        .Where(it => it.type != kWCMessageType.Remind)
                        .Where(it => it.type != kWCMessageType.Card)
                        .Where(it => it.type != kWCMessageType.AudioChatAsk)
                        .Where(it => it.type != kWCMessageType.kWCMessageTypeNone)
                        .Where(it => it.deleteTime > TimeUtils.CurrentIntTime() || it.deleteTime <= 0)
                        .Where(it => it.isReadDel == 0)
                        .OrderBy(it => it.timeSend, OrderByType.Desc)
                        .ToPageList(pagerIndex, 30);
                return list;
            }

            return null;
        }

        /// <summary>
        /// 查询某个消息类型的历史消息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <param name="pagerIndex"></param>
        /// <returns></returns>
        public List<MessageObject> GetHistoryMessagesByType(string userId, kWCMessageType type, int pagerIndex)
        {
            string tabName = MessageObject.Prefex + userId;

            if (IsTableExists(tabName))
            {
                var list = DBSetting.SQLiteDBContext.Queryable<MessageObject>().AS(tabName)
                        .Where(it => it.type == type)
                        .Where(it => it.isReadDel == 0)
                        .Where(it => it.deleteTime > TimeUtils.CurrentIntTime() || it.deleteTime <= 0)
                        .OrderBy(it => it.timeSend, OrderByType.Desc)
                        .ToPageList(pagerIndex, 30);
                return list;
            }

            return null;
        }

        /// <summary>
        /// 搜索指定内容的文字消息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="content"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        internal List<MessageObject> GetHistoryListByContent(string userId, string content, int pageNum)
        {
            string tabName = MessageObject.Prefex + userId;

            if (IsTableExists(tabName))
            {
                var list = DBSetting.SQLiteDBContext.Queryable<MessageObject>().AS(tabName)
                            .Where(it => it.type == kWCMessageType.Text)
                            .Where(it => it.content.Contains(content))
                            .Where(it => it.isReadDel == 0)
                            .Where(it => it.deleteTime > TimeUtils.CurrentIntTime() || it.deleteTime <= 0)
                            .OrderBy(it => it.timeSend, OrderByType.Desc)
                            .ToPageList(pageNum, 30);
                return list == null ? new List<MessageObject>() : list;
            }

            return new List<MessageObject>();
        }

        #endregion

        #region 获取朋友的最后一条消息
        /// <summary>
        /// 获取朋友的最后一条消息
        /// <para>按时间降序排序</para>
        /// </summary>
        /// <returns></returns>
        public MessageObject GetLastMessage(string userId)
        {
            string tabName = MessageObject.Prefex + userId;
            if (IsTableExists(tabName))
            {
                var result = DBSetting.SQLiteDBContext.Queryable<MessageObject>()
                    .AS(tabName)
                    .Where(it => it.type != kWCMessageType.IsRead)
                    .OrderBy(m => m.timeSend, OrderByType.Desc)
                    .First();

                return result;
            }

            return null;
        }

        #endregion

        #region 获取朋友的最早一条消息
        /// <summary>
        /// 获取朋友的最早一条消息
        /// </summary>
        public MessageObject GetFristMessage(string userId)
        {
            string tabName = MessageObject.Prefex + userId;
            if (IsTableExists(tabName))
            {
                var result = DBSetting.SQLiteDBContext.Queryable<MessageObject>()
                    .AS(tabName)
                    .Where(it => it.type != kWCMessageType.IsRead)
                    .OrderBy(m => m.timeSend, OrderByType.Asc)
                    .First();

                return result;
            }

            return null;
        }

        #endregion

        /// <summary>
        /// 删除消息表
        /// </summary>
        /// <returns></returns>
        public int DeleteMessageTable(string userId)
        {
            string tabName = MessageObject.Prefex + userId;
            if (IsTableExists(tabName))
            {

                if (mCreateTable.ContainsKey(tabName))
                {
                    mCreateTable.Remove(tabName);
                }

                int rows = DBSetting.SQLiteDBContext.Deleteable<MessageObject>().AS(tabName).ExecuteCommand();
                return rows;
            }

            return 0;
        }

        /// <summary>
        /// 删除某个时间之后的所有消息       
        /// </summary>
        public int DeleteMessageByTimeAfter(string userId, double time)
        {
            string tabName = MessageObject.Prefex + userId;
            if (IsTableExists(tabName))
            {
                int rows = DBSetting.SQLiteDBContext.Deleteable<MessageObject>()
                    .AS(tabName)
                    .Where(m => m.timeSend > time)
                    .ExecuteCommand();
                return rows;
            }
            return 0;
        }

        /// <summary>
        /// 删除某个时间之前的所有消息       
        /// </summary>
        public int DeleteMessageByTimeBefore(string userId, double time)
        {
            string tabName = MessageObject.Prefex + userId;
            if (IsTableExists(tabName))
            {
                int rows = DBSetting.SQLiteDBContext.Deleteable<MessageObject>()
                    .AS(tabName)
                    .Where(m => m.timeSend < time)
                    .ExecuteCommand();
                return rows;
            }
            return 0;
        }
    }
}
