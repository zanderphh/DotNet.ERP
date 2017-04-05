﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pharos.Logic.Entity
{
    /// <summary>
    /// 面向会员的活动推送表
    /// </summary>
    [Serializable]
    public class Push:BaseEntity
    {
        /// <summary>
        /// 记录Id
        /// [主键]
        /// [不允许为空]
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 推送Id
        /// （全局唯一）
        /// [不允许为空]
        /// </summary>
        public string PushId { get; set; }
        /// <summary>
        /// 内容
        /// [长度：1000]
        /// [允许为空]
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 状态：0-未推送 1-已推送
        /// [不允许为空]
        /// </summary>
        public short State { get; set; }
        /// <summary>
        /// 创建人UID
        /// [长度：40]
        /// [不允许为空]
        /// </summary>
        public string CreateUID { get; set; }
        /// <summary>
        /// 创建时间
        /// [不允许为空]
        /// </summary>
        public DateTime CreateDT { get; set; }
        /// <summary>
        /// 推送方式（存储dicSN，以逗号拼接）
        /// </summary>
        public string Type { get; set; }
    }
}