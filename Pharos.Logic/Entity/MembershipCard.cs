// --------------------------------------------------
// Copyright (C) 2016 版权所有
// 创 建 人：蔡少发
// 创建时间：2016-08-02
// 描述信息：
// --------------------------------------------------

using System;
using System.Runtime.Serialization;
using Pharos.Sys.Extensions;
using Pharos.Utility;

namespace Pharos.Logic.Entity
{
    /// <summary>
    /// 用于管理本系统的所有会员卡信息 
    /// [ps:这个entity存在大数据写入数据库，按表结构顺序维护实体]
    /// </summary>
    [Serializable]
    public class MembershipCard
    {

        ///// <summary>
        ///// 会员
        ///// </summary>
        public int Id { get; set; }

        public int CompanyId { get; set; }
        /// <summary>
        /// 制卡批次 
        /// [长度：40]
        /// </summary>
        public string BatchSN { get; set; }

        /// <summary>
        /// 卡号 
        /// [长度：40]
        /// [不允许为空]
        /// </summary>
        public string CardSN { get; set; }

        /// <summary>
        /// 卡片类型 GUID 
        /// [长度：10]
        /// [不允许为空]
        /// </summary>
        public string CardTypeId { get; set; }

        /// <summary>
        /// 关联会员 UID 

        /// [长度：40]
        /// </summary>
        public string MemberId { get; set; }

        /// <summary>
        /// 累计充值
        /// [长度：19，小数位数：4]
        /// [不允许为空]
        /// [默认值：((0))]
        /// </summary>
        public decimal ReChargeTotal { get; set; }

        /// <summary>
        /// 累计赠送金额 
        /// [长度：19，小数位数：4]
        /// [不允许为空]
        /// [默认值：((0))]
        /// </summary>
        public decimal GiveTotal { get; set; }

        /// <summary>
        /// 可用余额 
        /// [长度：19，小数位数：4]
        /// [不允许为空]
        /// [默认值：((0))]
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// 押金 
        /// [长度：19，小数位数：4]
        /// [不允许为空]
        /// [默认值：((0))]
        /// </summary>
        public decimal Deposit { get; set; }

        /// <summary>
        /// 使用状态(0:未激活；1：正常；2 已挂失；3：已作废；4 已退卡) 
        /// [长度：10]
        /// [不允许为空]
        /// [默认值：((-1))]
        /// </summary>
        [OperationLog("状态", false)]
        public int State { get; set; }

        /// <summary>
        /// 有效期-起始 
        /// [长度：10]
        /// [不允许为空]
        /// </summary>
        public DateTime ExpiryStart { get; set; }

        /// <summary>
        /// 有效期-截止
        /// [长度：10]
        /// [不允许为空]
        /// </summary>
        public DateTime? ExpiryEnd { get; set; }

        /// <summary>
        /// 创建时间
        /// [长度：23，小数位数：3]
        /// [不允许为空]
        /// [默认值：(getdate())]
        /// </summary>
        public DateTime CreateDT { get; set; }

        /// <summary>
        /// 创建人
        /// [长度：40]
        /// [不允许为空]
        /// </summary>
        public string CreateUID { get; set; }
        /// <summary>
        /// 领用时间
        /// </summary>
        public DateTime? LeadTime { get; set; }

        /// <summary>
        /// 防伪码
        /// </summary>
        public string SecurityCode { get; set; }
        /// <summary>
        /// 导出时间
        /// [长度：23，小数位数：3]
        /// [不允许为空]
        /// [默认值：(getdate())]
        /// </summary>
        public DateTime? ExportDT { get; set; }

        /// <summary>
        /// 导出人
        /// [长度：40]
        /// [不允许为空]
        /// </summary>
        public string ExportUID { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
        [Pharos.Utility.Exclude]
        public byte[] SyncItemVersion { get; set; }
        Guid _SyncItemId = Guid.NewGuid();
        [Pharos.Utility.Exclude]
        public Guid SyncItemId { get { return _SyncItemId; } set { _SyncItemId = value; } }
    }
}
