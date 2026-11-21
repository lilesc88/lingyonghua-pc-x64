using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinFrmTalk.Model.dao
{
    class MsgRoamTaskDao
    {

        #region 单例模式

        private static readonly MsgRoamTaskDao instance = new MsgRoamTaskDao();
        private Dictionary<string, MsgRoamTask> cache;
        private bool tableExists = false;

        /// <summary>
        /// 显式的静态构造函数用来告诉C#编译器在其内容实例化之前不要标记其类型
        /// </summary>
        static MsgRoamTaskDao() { }

        private MsgRoamTaskDao()
        {
            cache = new Dictionary<string, MsgRoamTask>();
        }

        #endregion

        public static MsgRoamTaskDao Instance
        {
            get
            {
                return instance;
            }
        }



        #region 创建数据库表
        private bool CreateRoamTaskTable()
        {
            if (tableExists)
            {
                return true;
            }

            try
            {
                var result = DBSetting.SQLiteDBContext.Queryable<sqlite_master>().Where(s => s.Name == "MsgRoamTask" && s.Type == "table");
                if (result != null && result.Count() > 0)     //表存在
                {
                    tableExists = true;
                    return true;
                }

                //创建数据库表
                DBSetting.SQLiteDBContext.CodeFirst.SetStringDefaultLength(100).InitTables(typeof(MsgRoamTask));
                tableExists = true;
                return true;
            }
            catch (Exception ex)
            {
                tableExists = false;
                LogUtils.Log(ex.Message);
                return false;
            }
        }
        #endregion


        #region 创建任务
        public int CreateTask(string chatJid, MessageObject local)
        {
            if (CreateRoamTaskTable())
            {
                MsgRoamTask mMsgRoamTask = new MsgRoamTask();
                mMsgRoamTask.taskId = TimeUtils.CurrentTimeMillis();
                mMsgRoamTask.ownerId = Applicate.MyAccount.userId;
                mMsgRoamTask.userId = chatJid;
                mMsgRoamTask.startMsgId = local.messageId;
                mMsgRoamTask.startTime = local.timeSend;

                if (cache.ContainsKey(chatJid))
                {
                    cache[chatJid] = mMsgRoamTask;
                }
                else
                {
                    cache.Add(chatJid, mMsgRoamTask);
                }

                var result = DBSetting.SQLiteDBContext.Insertable(mMsgRoamTask).ExecuteCommand();
                Console.WriteLine("生成离线任务");

                return result;

            }
            return 0;
        }
        #endregion


        #region 删除任务
        public int DeleteTask(long taskId, string chatJid)
        {
            if (CreateRoamTaskTable())
            {
                if (cache.ContainsKey(chatJid))
                {
                    cache.Remove(chatJid);
                }

                string ownerId = Applicate.MyAccount.userId;

                int result = DBSetting.SQLiteDBContext.Deleteable<MsgRoamTask>()
                  .Where(m => m.ownerId == ownerId).
                  Where(m => m.userId == chatJid).
                  Where(m => m.taskId == taskId).
                  ExecuteCommand();
                return result;
            }

            return 0;
        }
        #endregion


        #region 删除任务
        public int DeleteTaskAll(string chatJid)
        {
            if (CreateRoamTaskTable())
            {
                cache.Clear();

                string ownerId = Applicate.MyAccount.userId;

                int result = DBSetting.SQLiteDBContext.Deleteable<MsgRoamTask>()
                  .Where(m => m.ownerId == ownerId)
                  .Where(m => m.userId == chatJid)
                  .ExecuteCommand();
                return result;
            }

            return 0;
        }
        #endregion

        #region 修改任务的endTime
        public int UpdateTaskEndTime(long taskId, string chatJid, double end)
        {
            if (CreateRoamTaskTable())
            {
                if (cache.ContainsKey(chatJid))
                {
                    if (cache[chatJid] != null)
                    {
                        cache[chatJid].endTime = end;
                    }
                }

                int result = DBSetting.SQLiteDBContext.Updateable<MsgRoamTask>()
                    .UpdateColumns(it => new MsgRoamTask() { endTime = end })
                    .Where(it => it.taskId == taskId)
                    .ExecuteCommand();

                return result;
            }

            return 0;
        }
        #endregion

        #region 查询最后一条任务
        /// <summary>
        /// 查询int类型的value
        /// </summary>
        /// <returns></returns>
        public MsgRoamTask QueryLastTask(string chatJid)
        {
            if (CreateRoamTaskTable())
            {
                if (cache.ContainsKey(chatJid))
                {
                    var temp = cache[chatJid];

                    if (temp != null)
                    {
                        long time = TimeUtils.CurrentTimeMillis();

                        // 缓存1分钟有效
                        if (time - temp.QueryTime < (60 * 1000))
                        {
                            return temp;
                        }
                    }
                }

                string ownerId = Applicate.MyAccount.userId;

                var result = DBSetting.SQLiteDBContext.Queryable<MsgRoamTask>()
                    .Where(it => it.ownerId == ownerId)
                    .Where(it => it.userId == chatJid)
                    .OrderBy(m => m.taskId, OrderByType.Desc)
                    .First();

                if (result != null)
                {
                    result.QueryTime = TimeUtils.CurrentTimeMillis();
                }

                if (cache.ContainsKey(chatJid))
                {
                    cache[chatJid] = result;
                }
                else
                {
                    cache.Add(chatJid, result);
                }

                return result;
            }

            return null;
        }

        #endregion

    }
}
