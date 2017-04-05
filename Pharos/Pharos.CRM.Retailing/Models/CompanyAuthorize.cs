﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pharos.CRM.Retailing.Models
{
    public class CompanyAuthorize
    {
        /// <summary>
        /// 
        /// [长度：10]
        /// [不允许为空]
        /// </summary>
        public int? CID { get; set; }

        /// <summary>
        /// 客户来源(1-本司2-代理商)
        /// [长度：5]
        /// [不允许为空]
        /// </summary>
        public short Source { get; set; }

        /// <summary>
        /// 入驻方式(1-在线2-独立)
        /// [长度：5]
        /// [不允许为空]
        /// </summary>
        public short Way { get; set; }

        /// <summary>
        /// 客户名称
        /// [长度：50]
        /// [不允许为空]
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 经营模式(取字典表)
        /// [长度：5]
        /// [不允许为空]
        /// </summary>
        public short BusinessMode { get; set; }

        /// <summary>
        /// 开通版本ID(1：零售版、2：餐饮版、3：鞋服版)
        /// [长度：5]
        /// [不允许为空]
        /// </summary>
        public short OpenVersionId { get; set; }
        /// <summary>
        /// 开通版本
        /// </summary>
        public string OpenVersion { get { return OpenVersionId == 1 ? "零售版" : OpenVersionId == 2 ? "餐饮版" : OpenVersionId == 3 ? "鞋服版" : ""; } }

        /// <summary>
        /// 经营类目(取行业类别ID，多个以逗号隔开)
        /// [长度：200]
        /// [不允许为空]
        /// </summary>
        public string OpenScopeId { get; set; }
        /// <summary>
        /// 合同编号
        /// [长度：50]
        /// [不允许为空]
        /// </summary>
        public string ContractNo { get; set; }

        /// <summary>
        /// 用户数量
        /// [长度：5]
        /// [不允许为空]
        /// [默认值：((20))]
        /// </summary>
        public short UserNum { get; set; }

        /// <summary>
        /// 门店数量
        /// [长度：5]
        /// [不允许为空]
        /// [默认值：((10))]
        /// </summary>
        public short StoreNum { get; set; }

        /// <summary>
        /// 会员共享(Y/N)
        /// [长度：1]
        /// [不允许为空]
        /// [默认值：('Y')]
        /// </summary>
        public string MemberShared { get; set; }

        /// <summary>
        /// 分店专属后台(Y/N)
        /// [长度：1]
        /// [不允许为空]
        /// [默认值：('N')]
        /// </summary>
        public string StoreProper { get; set; }

        /// <summary>
        /// APP手机端(Y/N)
        /// [长度：1]
        /// [不允许为空]
        /// [默认值：('N')]
        /// </summary>
        public string AppProper { get; set; }

        /// <summary>
        /// POS次屏显示(Y/N)
        /// [长度：1]
        /// [不允许为空]
        /// [默认值：('N')]
        /// </summary>
        public string PosMinorDisp { get; set; }

        /// <summary>
        /// 超管帐号
        /// [长度：50]
        /// [不允许为空]
        /// </summary>
        public string SupperAccount { get; set; }

        /// <summary>
        /// 超管密码
        /// [长度：50]
        /// [不允许为空]
        /// </summary>
        public string SupperPassword { get; set; }

        /// <summary>
        /// 生效日期(yyyy-MM-dd)
        /// [长度：10]
        /// [不允许为空]
        /// </summary>
        public string EffectiveDT { get; set; }

        /// <summary>
        /// 有效期月数
        /// [长度：5]
        /// [不允许为空]
        /// [默认值：((12))]
        /// </summary>
        public short? ValidityNum { get; set; }

        /// <summary>
        /// 截止日期(yyyy-MM-dd)
        /// [长度：10]
        /// [不允许为空]
        /// </summary>
        public string ExpirationDT { get; set; }

        /// <summary>
        /// 状态(0:待审,1:正常,2:停用)
        /// [长度：5]
        /// [不允许为空]
        /// [默认值：((0))]
        /// </summary>
        public short Status { get; set; }
        public string StatusTitle { get { return Status == 0 ? "待审" : Status == 1 ? "正常" : Status == 2 ? "停用" : ""; } }
        /// <summary>
        /// 机器码（设备码,适用于私有部署）
        /// [长度：50]
        /// </summary>
        public string MachineSN { get; set; }

        /// <summary>
        /// 序列号
        /// [长度：2000]
        /// </summary>
        public string SerialNo { get; set; }

        /// <summary>
        /// 指派人UID（-1:未指派）
        /// [长度：40]
        /// </summary>
        public string AssignerUID { get; set; }



    }
}