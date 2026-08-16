using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinFrmTalk.Model
{
    class MsgRoamTask
    {

        #region public member
        /// <summary>
        /// 唯一标识
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsNullable = false)]
        public long taskId { get; set; }

        /// <summary>
        /// 字符串型型value
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string ownerId { get; set; }

        /// <summary>
        /// 当前任务属于哪个群组
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string userId { get; set; }

        /// <summary>
        /// 起始msgId
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string startMsgId { get; set; }

        /// <summary>
        /// 漫游开始时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public double startTime { get; set; }

        /// <summary>
        /// 漫游结束时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public double endTime { get; set; }

        /// <summary>
        /// 是否完成
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int isFinish { get; set; }


        /// <summary>
        /// 查询时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public long QueryTime { get; set; }

        #endregion


    }
}
