using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WinFrmTalk
{
    public class DBContextTransaction
    {
        private static bool isRunning = false;
        private static object locker = new object();
        private static object listLocker = new object();
        private static List<Action<SqlSugarClient>> actions = null;
        /// <summary>
        /// 事务中的操作，常用于插入和删除
        /// </summary>
        public static List<Action<SqlSugarClient>> Actions
        {
            get
            {
                if (actions != null)
                {
                    if (actions != null)
                    {
                        return actions;
                    }
                    else
                    {
                        actions = new List<Action<SqlSugarClient>>();
                        return actions;
                    }
                }
                else
                {
                    actions = new List<Action<SqlSugarClient>>();
                    return actions;
                }
            }
        }

        public static void Start()
        {
            return;
            isRunning = true;
            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    //设置事务批量处理的间隔
                    //Thread.Sleep(500);
                    if (!isRunning)
                        continue;
                    if (Actions != null && Actions.Count > 0)
                    {
                        lock (locker)
                        {
                            if (Actions != null && Actions.Count > 0)
                            {
                                Stop();
                                List<Action<SqlSugarClient>> runActions = new List<Action<SqlSugarClient>>();
                                runActions.AddRange(Actions);
                                Actions.Clear();
                                using (var db = DBSetting.SQLiteDBContext)
                                {
                                    try
                                    {
                                        db.Ado.BeginTran(System.Data.IsolationLevel.ReadCommitted);
                                        foreach (Action<SqlSugarClient> action in runActions)
                                        {
                                            if (action != null)
                                                action(db);
                                        }
                                        db.Ado.CommitTran();
                                    }
                                    catch (Exception ex)
                                    {
                                        db.Ado.RollbackTran();
                                        throw ex;
                                    }
                                }
                                Restart();
                            }
                        }
                    }
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        public static void Stop()
        {
            isRunning = false;
        }

        public static void Restart()
        {
            isRunning = true;
        }

        public static bool StartTransactionSTA(Action<SqlSugarClient> action)
        {
            //Thread thread = new Thread( new ParameterizedThreadStart((ret) =>
            //{
            bool result = false;
            using (var db = DBSetting.SQLiteDBContext)
            {
                try
                {
                    db.Ado.BeginTran(System.Data.IsolationLevel.ReadCommitted);
                    action?.Invoke(db);
                    db.Ado.CommitTran();
                    result = true;
                }
                catch (Exception ex)
                {
                    db.Ado.RollbackTran();
                    result = false;
                    throw ex;
                }
            }
            return result;
            //}));
            //thread.IsBackground = true;
            //thread.Start(result);
        }
    }
}
