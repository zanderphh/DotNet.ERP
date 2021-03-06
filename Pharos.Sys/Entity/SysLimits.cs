// --------------------------------------------------
// Copyright (C) 2015 版权所有
// 创 建 人：
// 创建时间：2015-07-29
// 描述信息：
// --------------------------------------------------

using Pharos.Utility;
using System;
using System.Runtime.Serialization;

namespace Pharos.Sys.Entity
{
    /// <summary>
    /// 用于管理本系统的全局权限Code信息
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class SysLimits : BaseEntity
    {
        /// <summary>
        /// 记录 ID
        /// [主键：√]
        /// [长度：10]
        /// [不允许为空]
        /// </summary>
        //[DataMember]
        //public int Id { get; set; }

        /// <summary>
        /// 权限名称
        /// [长度：50]
        /// [不允许为空]
        /// </summary>
        [DataMember]
        public string Title { get; set; }

        /// <summary>
        /// 权限 ID （全局唯一）
        /// [长度：10]
        /// [不允许为空]
        /// [默认值：((0))]
        /// </summary>
        [DataMember]
        public int LimitId { get; set; }

        /// <summary>
        /// 隶属权限 ID（ 0:顶级）
        /// [长度：10]
        /// [不允许为空]
        /// </summary>
        [DataMember]
        public int PLimitId { get; set; }

        /// <summary>
        /// 深度（1-9）
        /// [长度：5]
        /// </summary>
        [DataMember]
        public short Depth { get; set; }
        [DataMember]
        public int SortOrder { get; set; }
        /// <summary>
        /// 状态（0:关闭/停用、1:显示/默认选中、2:可选）
        /// [长度：5]
        /// [不允许为空]
        /// [默认值：((2))]
        /// </summary>
        [DataMember]
        public short Status { get; set; }
    }
}
