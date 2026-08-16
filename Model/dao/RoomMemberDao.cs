using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinFrmTalk.Model
{

    /// <summary>
    /// 群成员表数据库访问类
    /// </summary>
    public class RoomMemberDao
    {
        #region 单例模式
        private const string MEMBER_TAB_HEAD = "Member_";
        private static readonly RoomMemberDao instance = new RoomMemberDao();
        private Dictionary<string, bool> mCreateTable;

        // 保存我在每个群的状态  ket == roomId
        private Dictionary<string, RoomMember> mCacheMember;

        /// <summary>
        /// 显式的静态构造函数用来告诉C#编译器在其内容实例化之前不要标记其类型
        /// </summary>
        static RoomMemberDao() { }

        private RoomMemberDao()
        {
            mCreateTable = new Dictionary<string, bool>();
            mCacheMember = new Dictionary<string, RoomMember>();
        }

        public static RoomMemberDao Instance
        {
            get
            {
                return instance;
            }
        }
        #endregion

        #region 判断表是否存在
        private bool CreateMemberTab(string roomId)
        {
            if (UIUtils.IsNull(roomId))
            {
                return false;
            }

            string TabName = MEMBER_TAB_HEAD + roomId;

            if (mCreateTable.ContainsKey(TabName))
            {
                return true;
            }

            try
            {
                var db = DBSetting.SQLiteDBContext;
                db.MappingTables.RemoveAll(it => it.EntityName == "RoomMember");
                db.MappingTables.Add("RoomMember", TabName);

                //表存在
                if (db.Queryable<sqlite_master>().Where(s => s.Name == TabName && s.Type == "table").Any())
                {
                    mCreateTable.Add(TabName, true);
                    return true;
                }

                //创建数据库表
                db.CodeFirst.SetStringDefaultLength(100).InitTables(typeof(RoomMember));
                mCreateTable.Add(TabName, true);
                return true;
            }
            catch (Exception)
            {
                if (mCreateTable.ContainsKey(TabName))
                {
                    mCreateTable.Remove(TabName);
                }
                return false;
            }
        }
        #endregion

        #region 修改
        // 插入一个群成员
        internal void InsertOrUpdate(RoomMember member)
        {
            if (member == null || UIUtils.IsNull(member.userId))
            {
                return;
            }

            if (CreateMemberTab(member.roomId))
            {
                string TabName = MEMBER_TAB_HEAD + member.roomId;

                var result = DBSetting.SQLiteDBContext.Queryable<RoomMember>().AS(TabName).First(m => m.userId == member.userId);
                if (result == null)
                {
                    DBSetting.SQLiteDBContext.Insertable(member).AS(TabName).ExecuteCommand();
                }
                else
                {
                    UpdateMemberRole(member.roomId, member.userId, member.role);
                    return;
                }
            }

        }


        // 插入一个群成员
        internal void InsertOrUpdate(RoomMember member, bool isDecode)
        {

            if (!isDecode)
            {
                InsertOrUpdate(member);
                return;
            }

            if (member == null || UIUtils.IsNull(member.userId))
            {
                return;
            }

            if (CreateMemberTab(member.roomId))
            {
                string TabName = MEMBER_TAB_HEAD + member.roomId;

                var result = DBSetting.SQLiteDBContext.Queryable<RoomMember>().AS(TabName).First(m => m.userId == member.userId);
                if (result == null)
                {
                    DBSetting.SQLiteDBContext.Insertable(member).AS(TabName).ExecuteCommand();
                }
                else
                {
                    DBSetting.SQLiteDBContext.Updateable(member).AS(TabName).ExecuteCommand();
                    return;
                }
            }

            //"active": 1554106810,
            //"call": "305228",
            //"createTime": 1554106810,
            //"modifyTime": 0,
            //"nickname": "2007",
            //"offlineNoPushMsg": 0,
            //"role": 1,
            //"sub": 1,
            //"talkTime": 0,
            //"userId": 10009572,
            //"videoMeetingNo": "355228"
        }

        #endregion

        #region 修改

        /// <summary>
        /// 修改群成员身份
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="userId"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        internal int UpdateMemberRole(string roomId, string userId, int role)
        {
            bool queryMe = string.Equals(userId, Applicate.MyAccount.userId);


            if (CreateMemberTab(roomId))
            {
                if (queryMe && mCacheMember.ContainsKey(roomId))
                {
                    mCacheMember[roomId].role = role;
                }

                string TabName = MEMBER_TAB_HEAD + roomId;
                //保存数据库

                return DBSetting.SQLiteDBContext.Updateable<RoomMember>()
                    .UpdateColumns(it => new RoomMember() { role = role })
                    .Where(m => m.userId == userId && m.roomId == roomId)
                    .AS(TabName).ExecuteCommand();
            }

            return 0;
        }

        /// <summary>
        /// 更新群成员禁言时间
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="userId"></param>
        /// <param name="talkTime"></param>
        internal void UpdateMemberTalkTime(string roomId, string userId, long talkTime)
        {
            bool queryMe = string.Equals(userId, Applicate.MyAccount.userId);



            if (CreateMemberTab(roomId))
            {
                if (queryMe && mCacheMember.ContainsKey(roomId))
                {
                    mCacheMember[roomId].talkTime = talkTime;
                }
                string TabName = MEMBER_TAB_HEAD + roomId;

                //保存数据库
                var result = DBSetting.SQLiteDBContext.Updateable<RoomMember>()
                    .UpdateColumns(it => new RoomMember() { talkTime = talkTime })
                    .Where(m => m.userId == userId && m.roomId == roomId)
                    .AS(TabName)
                    .ExecuteCommand();
            }
        }

        /// <summary>
        /// 修改成员昵称
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="userId"></param>
        /// <param name="nickName"></param>
        /// <returns></returns>
        internal int UpdateNickname(string roomId, string userId, string nickName)
        {
            bool queryMe = string.Equals(userId, Applicate.MyAccount.userId);

            if (CreateMemberTab(roomId))
            {
                if (queryMe && mCacheMember.ContainsKey(roomId))
                {
                    mCacheMember[roomId].nickName = nickName;
                }
                string TabName = MEMBER_TAB_HEAD + roomId;
                var result = DBSetting.SQLiteDBContext.Updateable<RoomMember>().AS(TabName)
                    .UpdateColumns(it => new RoomMember() { nickName = nickName })
                    .Where(m => m.userId == userId && m.roomId == roomId)
                    .ExecuteCommand();
                return result;
            }
            return 0;
        }

        /// <summary>
        /// 修改成员备注
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="userId"></param>
        /// <param name="remarkName"></param>
        /// <returns></returns>
        internal int UpdateRemarkName(string roomId, string userId, string remarkName)
        {
            bool queryMe = string.Equals(userId, Applicate.MyAccount.userId);

            if (CreateMemberTab(roomId))
            {
                if (queryMe && mCacheMember.ContainsKey(roomId))
                {
                    mCacheMember[roomId].remarkName = remarkName;
                }
                string TabName = MEMBER_TAB_HEAD + roomId;
                var result = DBSetting.SQLiteDBContext.Updateable<RoomMember>().AS(TabName).
                UpdateColumns(it => new RoomMember() { remarkName = remarkName }).
                Where(m => m.userId == userId && m.roomId == roomId).ExecuteCommand();
                return result;
            }
            return 0;
        }


        /// <summary>
        /// 修改群成员名片
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="userId"></param>
        /// <param name="cardName"></param>
        /// <returns></returns>
        internal int UpdateCardname(string roomId, string userId, string cardName)
        {
            bool queryMe = string.Equals(userId, Applicate.MyAccount.userId);

            if (CreateMemberTab(roomId))
            {
                if (queryMe && mCacheMember.ContainsKey(roomId))
                {
                    mCacheMember[roomId].cardName = cardName;
                }
                string TabName = MEMBER_TAB_HEAD + roomId;
                var result = DBSetting.SQLiteDBContext.Updateable<RoomMember>().AS(TabName).
                UpdateColumns(it => new RoomMember() { cardName = cardName }).
                Where(m => m.userId == userId && m.roomId == roomId).ExecuteCommand();
                return result;
            }
            return 0;
        }

        #endregion

        #region 查询

        /// <summary>
        /// 获取群成员
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        internal RoomMember GetRoomMemberById(string roomId, string userId)
        {
            bool queryMe = string.Equals(userId, Applicate.MyAccount.userId);

            if (CreateMemberTab(roomId))
            {
                if (queryMe && mCacheMember.ContainsKey(roomId))
                {
                    return mCacheMember[roomId];
                }

                string TabName = MEMBER_TAB_HEAD + roomId;
                var result = DBSetting.SQLiteDBContext.Queryable<RoomMember>().AS(TabName).First(m => m.userId == userId);

                if (queryMe)
                {
                    if (result != null)
                    {
                        if (queryMe && !mCacheMember.ContainsKey(roomId))
                        {
                            mCacheMember.Add(roomId, result);
                        }
                        return result;
                    }
                    else
                    {
                        return new RoomMember();
                    }
                }
                else
                {
                    return result;
                }
            }

            return null;
        }



        internal List<RoomMember> GetRommMemberListByHead(string roomId)
        {
            if (CreateMemberTab(roomId))
            {
                string TabName = MEMBER_TAB_HEAD + roomId;
                var result = DBSetting.SQLiteDBContext.Queryable<RoomMember>().AS(TabName)
                    .Where(m => m.roomId == roomId)
                    .Where(m => m.role != 4)
                    .OrderBy(it => it.role, OrderByType.Asc)
                    .OrderBy(it => it.createTime, OrderByType.Desc)
                    .ToList();

                return result;
            }

            return null;
        }


        /// <summary>
        /// 获取群成员身份
        /// <para>必须先给对象的roomId和userId赋值</para>
        /// </summary>
        /// <returns>1=创建者 2=管理员 3=成员  4 隐身人</returns>
        public int GetMemberRole(string roomId, string userId)
        {
            var result = GetRoomMemberById(roomId, userId);
            return result == null ? 0 : result.role;
        }





        /// <summary>
        /// 根据RoomId获取群成员数量
        /// <para>必须先给对象的roomId赋值</para>
        /// </summary>
        /// <param name="roomid">RoomId</param>
        /// <returns></returns>
        internal int GetCountByRoomId(string roomId)
        {
            if (CreateMemberTab(roomId))
            {
                string TabName = MEMBER_TAB_HEAD + roomId;
                var result = DBSetting.SQLiteDBContext.Queryable<RoomMember>().AS(TabName).Where(m => m.roomId == roomId).Count();
                return result;
            }
            return 0;
        }
        #endregion

        /// <summary>
        /// 删除群成员
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        internal int DeleteMemberById(string roomId, string userId)
        {
            bool queryMe = string.Equals(userId, Applicate.MyAccount.userId);



            if (CreateMemberTab(roomId))
            {
                if (queryMe && mCacheMember.ContainsKey(roomId))
                {
                    mCacheMember.Remove(roomId);
                }

                string TabName = MEMBER_TAB_HEAD + roomId;
                return DBSetting.SQLiteDBContext.Deleteable<RoomMember>()
                    .Where(m => m.userId == userId)
                    .AS(TabName)
                    .ExecuteCommand();
            }

            return 0;
        }


        #region 解析方法

        //"member": {
        //"active": 1554106810,
        //"call": "305228",
        //"createTime": 1554106810,
        //"modifyTime": 0,
        //"nickname": "2007",
        //"offlineNoPushMsg": 0,
        //"role": 1,
        //"sub": 1,
        //"talkTime": 0,
        //"userId": 10009572,
        //"videoMeetingNo": "355228"
        //},

        //"members": [
        //{
        //"active": 1554106810,
        //"call": "305228",
        //"createTime": 1554106810,
        //"modifyTime": 0,
        //"nickname": "2007",
        //"offlineNoPushMsg": 0,
        //"role": 1,
        //"sub": 1,
        //"talkTime": 0,
        //"userId": 10009572,
        //"videoMeetingNo": "355228"
        //}
        //],


        internal RoomMember DecodeMember(string member, string roomId)
        {
            if (!UIUtils.IsNull(member))
            {

                RoomMember user = JsonConvert.DeserializeObject<RoomMember>(member);
                user.roomId = roomId;
                return user;

            }

            return null;
        }


        internal List<RoomMember> DecodeMemberList(string members, string roomId, bool updateMe)
        {
            if (!UIUtils.IsNull(members))
            {
                List<RoomMember> data = new List<RoomMember>();

                var mems = JsonConvert.DeserializeObject<List<RoomMember>>(members);
                foreach (var item in mems)
                {
                    item.roomId = roomId;
                    if (item.userId != Applicate.MyAccount.userId)
                    {
                        InsertOrUpdate(item, true);
                    }
                    data.Add(item);
                }
                return data;
            }
            return null;
        }

        #endregion
    }
}
