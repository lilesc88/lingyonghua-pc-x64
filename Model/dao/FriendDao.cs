using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinFrmTalk.Model.dao
{
    class FriendDao
    {

        #region 单例模式

        private static readonly FriendDao instance = new FriendDao();
        private bool TableExists;

        /// <summary>
        /// 显式的静态构造函数用来告诉C#编译器在其内容实例化之前不要标记其类型
        /// </summary>
        static FriendDao() { }

        private FriendDao() { }

        #endregion

        public static FriendDao Instance
        {
            get
            {
                return instance;
            }
        }

        internal bool CreateTable()
        {
            if (TableExists)
            {
                return true;
            }

            try
            {
                var result = DBSetting.SQLiteDBContext.Queryable<sqlite_master>().Where(s => SqlFunc.Equals(s.Name, "Friend") && SqlFunc.Equals(s.Type, "table"));

                //不存在表
                if (result == null || result.Count() == 0)
                {
                    //创建数据库表
                    DBSetting.SQLiteDBContext.CodeFirst.SetStringDefaultLength(100).InitTables(typeof(Friend));
                }

                TableExists = true;
                return true;
            }
            catch (Exception ex)
            {
                LogUtils.Log(ex.Message);
                TableExists = false;
                return false;
            }
        }


        internal Friend GetFriendById(string userId)
        {
            if (CreateTable())
            {
                var result = DBSetting.SQLiteDBContext.Queryable<Friend>()
                         .Where(f => f.UserId == userId)
                         .Single();
                return result;
            }
            return null;
        }

        internal List<Friend> GetFriendsByInvite()
        {
            if (CreateTable())
            {
                var result = DBSetting.SQLiteDBContext.Queryable<Friend>()
                         .Where(f => f.IsGroup == 0)
                         .Where(f => f.UserType <= FriendType.USER_TYPE_NEW)
                         .OrderBy(it => it.TopTime, OrderByType.Desc)
                         .OrderBy(it => it.LastMsgTime, OrderByType.Desc)
                         .ToList();

                return result == null ? new List<Friend>() : result;
            }

            return new List<Friend>();
        }


        internal List<Friend> GetFriendsByLable(string userIdList)
        {
            if (CreateTable())
            {
                var result = DBSetting.SQLiteDBContext.Queryable<Friend>()
                          .Where(f => f.IsGroup == 0)
                          .Where(f => f.UserType <= FriendType.USER_TYPE_NEW)
                          .Where(f => userIdList.Contains(f.UserId))
                          .OrderBy(it => it.LastMsgTime, OrderByType.Desc)
                          .ToList();
                return result;
            }

            return null;
        }


        internal List<Friend> GetFriendsByName(string name)
        {
            if (CreateTable())
            {
                var result = DBSetting.SQLiteDBContext.Queryable<Friend>()
                         .Where(f => f.IsGroup == 0)
                         .Where(f => f.UserType <= FriendType.USER_TYPE_NEW)
                         .Where(f => f.NickName.Contains(name) || f.RemarkName.Contains(name))
                         .OrderBy(it => it.TopTime, OrderByType.Desc)
                         .OrderBy(it => it.LastMsgTime, OrderByType.Desc)
                         .ToList();

                return result == null ? new List<Friend>() : result;
            }

            return new List<Friend>();
        }

        internal void UpdateAllBannedTalk(string userId, long state)
        {
            if (CreateTable())
            {
                try
                {
                    var result = DBSetting.SQLiteDBContext.Updateable<Friend>()
                        .UpdateColumns(it => new Friend() { AllBannedTalk = state })
                        .Where(it => it.UserId == userId).ExecuteCommand();
                }
                catch (Exception ex)
                {
                    ConsoleLog.Output(ex.Message);
                }
            }
        }


    }
}
