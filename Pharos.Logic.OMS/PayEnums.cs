﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Pharos.Logic.OMS
{
    #region 支付商户枚举
    /// <summary>
    /// 商户支付许可状态
    /// </summary>
    public enum TraderPayLicenseState : short
    {
        /// <summary>
        /// 未审核
        /// </summary>
        [Description("未审核")]
        NotAuditing = 1,

        /// <summary>
        /// 被驳回
        /// </summary>
        [Description("被驳回")]
        Reject = 2,

        /// <summary>
        /// 已审核
        /// </summary>
        [Description("已审核")]
        Audited = 3,

        /// <summary>
        /// 暂停
        /// </summary>
        [Description("暂停")]
        Pause = 4,

        /// <summary>
        /// 注销
        /// </summary>
        [Description("注销")]
        Cancel = 5,

        /// <summary>
        /// 无效
        /// </summary>
        [Description("无效")]
        Invalid = 6
    }

    /// <summary>
    /// 商家结算账户状态
    /// </summary>
    public enum TraderBalanceAccountState : int
    {

        /// <summary>
        /// 未审核
        /// </summary>
        [Description("未审核")]
        NotAuditing = 1,

        /// <summary>
        /// 可用
        /// </summary>
        [Description("可用")]
        Enabled = 2,
        /// <summary>
        /// 被驳回
        /// </summary>
        [Description("被驳回")]
        Reject = 3,

        /// <summary>
        /// 暂停
        /// </summary>
        [Description("暂停")]
        Pause = 4,

        /// <summary>
        /// 注销
        /// </summary>
        [Description("注销")]
        Cancel = 5,

        /// <summary>
        /// 无效
        /// </summary>
        [Description("无效")]
        Invalid = 6
    }

    /// <summary>
    /// 商家支付通道状态
    /// </summary>
    public enum TraderPayCchannelState : short
    {
        /// <summary>
        /// 未审核
        /// </summary>
        [Description("未审核")]
        NotAuditing = 0,

        /// <summary>
        /// 可用
        /// </summary>
        [Description("可用")]
        Enabled = 1,

        /// <summary>
        /// 暂停
        /// </summary>
        [Description("暂停")]
        Pause = 2,

        /// <summary>
        /// 注销
        /// </summary>
        [Description("注销")]
        Cancel = 3,

        /// <summary>
        /// 无效
        /// </summary>
        [Description("无效")]
        Invalid = 4
    }

    /// <summary>
    /// 商家门店状态
    /// </summary>
    public enum TraderStoreState : short
    {

        /// <summary>
        /// 未审核
        /// </summary>
        [Description("未审核")]
        NotAuditing = 0,

        /// <summary>
        /// 可用
        /// </summary>
        [Description("可用")]
        Enabled = 1,

        /// <summary>
        /// 暂停
        /// </summary>
        [Description("暂停")]
        Pause = 2,

        /// <summary>
        /// 注销
        /// </summary>
        [Description("注销")]
        Cancel = 3,

        /// <summary>
        /// 无效
        /// </summary>
        [Description("无效")]
        Invalid = 4
    }
    /// <summary>
    /// 商家状态
    /// </summary>
    public enum TraderState : short
    {
        /// <summary>
        /// 未审核
        /// </summary>
        [Description("未审核")]
        NotAuditing = 0,

        /// <summary>
        /// 已审核
        /// </summary>
        [Description("已审核")]
        Audited = 1,

        /// <summary>
        /// 无效
        /// </summary>
        [Description("无效")]
        Invalid = 2
    }
    #endregion
}
