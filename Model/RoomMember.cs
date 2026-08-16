using Newtonsoft.Json;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using WinFrmTalk.Model;

namespace WinFrmTalk
{


    #region RoomMember
    /// <summary>
    /// 群聊中群聊个人的RoomMember
    /// </summary>
    public class RoomMember
    {
        /// <summary>
        /// 实例化对象时，rommId永远不能为空
        /// </summary>
        public RoomMember()
        {
        }


        #region Public Member

        /// <summary>
        /// 数据库预留（自增长主键id修改数据类型int）
        /// </summary>
        //[SugarColumn(IsNullable = false, IsPrimaryKey = true, IsIdentity = true)]
        //public int Id { get; set; }

        /// <summary>
        /// 群组Id
        /// <para>非Jid，roomjid仅用于连接，roomid用于查询和接口和其他</para>
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public string roomId { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        [SugarColumn(IsNullable = false, IsPrimaryKey = true)]
        public string userId { get; set; }

        /// <summary>
        /// 用户入群时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int createTime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int modifyTime { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        /// </summary>
        [JsonProperty("nickname")]
        [SugarColumn(IsNullable = true)]
        public string nickName { get; set; }

        /// <summary>
        /// 消息免打扰1 是 0 否
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int offlineNoPushMsg { get; set; }
        /// <summary>
        /// 此用户在群组中的身份
        /// <para>1=创建者 2=管理员 3=成员  4 隐身人</para>
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int role { get; set; }
        /// <summary>
        /// 是否屏蔽消息0屏蔽消息，1不屏蔽
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int sub { get; set; }

        /// <summary>
        /// 禁言时间
        /// </summary
        [SugarColumn(IsNullable = false)]
        public long talkTime { get; set; }
        /// <summary>
        /// 群主对群内成员的备注名 仅群主可见
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string remarkName { get; set; }

        /// <summary>
        /// 群名片名称
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string cardName { get; set; }


        [SugarColumn(IsIgnore = true)]
        private string TabName
        {
            get { return GetTabName(); }
        }


        /// <summary>
        /// 数据库表名
        /// <para>RoomId为空时，返回值为"Member_Null"</para>
        /// </summary>
        private string GetTabName()
        {
            if (this.roomId == null)
                return "Member_Null";
            return "Member_" + this.roomId;
        }
        #endregion





        #region 获取群成员身份
        /// <summary>
        /// 获取群成员身份
        /// <para>必须先给对象的roomId和userId赋值</para>
        /// </summary>
        /// <returns></returns>
        public int GetRoleByUserId()
        {
            return RoomMemberDao.Instance.GetMemberRole(this.roomId, this.userId);
        }
        #endregion



        #region 保存某个成员
        /// <summary>
        /// 保存或更新成员
        /// <para>必须先给roomId赋值</para>
        /// </summary>
        /// <param name="members">成员</param>
        public void InsertOrUpdate()
        {
            RoomMemberDao.Instance.InsertOrUpdate(this);
        }
        #endregion

        #region 删除单个用户
        /// <summary>
        /// 删除单个用户
        /// <para>必须先给userId赋值</para>
        /// </summary>
        public int DeleteByUserId()
        {
            return RoomMemberDao.Instance.DeleteMemberById(roomId, userId);
        }
        #endregion

        #region 删除某群的所有成员
        /// <summary>
        /// 删除某群的所有成员
        /// </summary>
        /// <returns>受影响行数</returns>
        public int DeleteByRoomId()
        {
            return 0;
        }
        #endregion

        #region 更新群权限
        /// <summary>
        /// 更新用户的群权限
        /// <para>必须先给对象的role赋值</para>
        /// </summary>
        /// <returns>受影响行数</returns>
        public int UpdateRole()
        {
            return RoomMemberDao.Instance.UpdateMemberRole(roomId, userId, role);
        }
        #endregion


        #region 获取对象
        /// <summary>
        /// 根据UserId和roomId获取对象
        /// <para>必须先给对象的roomId和userId赋值</para>
        /// </summary>
        /// <returns></returns>
        public RoomMember GetRoomMember()
        {
            var mem = RoomMemberDao.Instance.GetRoomMemberById(this.roomId, this.userId);
            return mem != null ? mem : new RoomMember();
        }
        #endregion


        #region 获取群昵称
        /// <summary>
        /// 获取群昵称
        /// </summary>
        /// <returns></returns>
        public string GetNickName()
        {
            return GetRoomMember().nickName;
        }
        #endregion



        #region liuhuan

        [SugarColumn(IsIgnore = true)]
        public int isAttritionNotice { get; set; }

        [SugarColumn(IsIgnore = true)]
        public int isLook { get; set; }

        [SugarColumn(IsIgnore = true)]
        public List<GroupNotices> NoticeLst { set; get; }

        #endregion


        public string GetSetname()
        {
            var mem = GetRoomMember();

            remarkName = mem.remarkName;
            cardName = mem.cardName;

            if (UIUtils.IsNull(nickName))
            {
                nickName = mem.nickName;
            }

            if (!UIUtils.IsNull(remarkName))
            {
                return remarkName;
            }
            else
            {
                if (!UIUtils.IsNull(cardName))
                {
                    return cardName;
                }
                else
                {
                    return nickName;
                }
            }

        }

        public string GetRoomShowName()
        {

            if (!UIUtils.IsNull(remarkName))
            {
                return remarkName;
            }
            else
            {
                if (!UIUtils.IsNull(cardName))
                {
                    return cardName;
                }
                else
                {
                    return nickName;
                }
            }
        }


        public List<RoomMember> TransToMember(Dictionary<string, object> keyValues, string roomid)
        {
            NoticeLst = new List<GroupNotices>();
            if (keyValues.ContainsKey("isAttritionNotice"))
            {
                isAttritionNotice = Convert.ToInt32(keyValues["isAttritionNotice"].ToString());

            }
            if (keyValues.ContainsKey("isLook"))
            {
                isLook = Convert.ToInt32(keyValues["isLook"].ToString());

            }

            talkTime = UIUtils.DecodeLong(keyValues, "talkTime");
            if (keyValues.ContainsKey("notices"))
            {
                var notices = JsonConvert.DeserializeObject<List<object>>(keyValues["notices"].ToString());
                foreach (var item in notices)
                {

                    GroupNotices grouptips = new GroupNotices();
                    var notic = JsonConvert.DeserializeObject<Dictionary<string, object>>(item.ToString());
                    grouptips.text = UIUtils.DecodeString(notic, "text");
                    grouptips.NickName = UIUtils.DecodeString(notic, "nickname");
                    grouptips.Userid = UIUtils.DecodeString(notic, "userId");
                    grouptips.Roomid = UIUtils.DecodeString(notic, "roomId");
                    grouptips.Id = UIUtils.DecodeString(notic, "id");
                    grouptips.Time = UIUtils.DecodeInt(notic, "time");
                    NoticeLst.Add(grouptips);

                }
            }


            List<RoomMember> memberLst = new List<RoomMember>();
            if (keyValues.ContainsKey("members"))
            {

                memberLst = RoomMemberDao.Instance.DecodeMemberList(keyValues["members"].ToString(), roomId, true);

                //var members = JsonConvert.DeserializeObject<List<object>>();
                //RoomMember room = new RoomMember();
                //room.roomId = roomId;
                //foreach (var item in members)
                //{
                //    var member = JsonConvert.DeserializeObject<Dictionary<string, object>>(item.ToString());
                //    RoomMember roomMember = new RoomMember();
                //    roomMember.role = UIUtils.DecodeInt(member, "role");
                //    roomMember.userId = UIUtils.DecodeString(member, "userId");
                //    roomMember.createTime = UIUtils.DecodeInt(member, "createTime");
                //    roomMember.modifyTime = UIUtils.DecodeInt(member, "modifyTime");
                //    roomMember.nickName = UIUtils.DecodeString(member, "nickname");
                //    roomMember.offlineNoPushMsg = UIUtils.DecodeInt(member, "offlineNoPushMsg");
                //    roomMember.sub = UIUtils.DecodeInt(member, "sub");
                //    roomMember.talkTime = UIUtils.DecodeLong(member, "talkTime");
                //    roomMember.remarkName = UIUtils.DecodeString(member, "remarkName");
                //    roomMember.cardName = UIUtils.DecodeString(member, "cardName");
                //    roomMember.roomId = roomid;

                //    memberLst.Add(roomMember);
                //    roomMember.InsertOrUpdate();

                //}
            }

            return memberLst;
        }

    }
    #endregion
}
