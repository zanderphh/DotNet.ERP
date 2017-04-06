﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Pharos.Utility;

namespace Pharos.Sys.Entity
{
    public class OMS_CompanyAuthorize
    {
        short _Category = 1;
        [System.ComponentModel.DataAnnotations.Key]
        public int Id { get; set; }
        /// <summary>
        /// 编号
        /// </summary>
        public int? Code { get; set; }
        /// <summary>
        /// 审核状态(0-未审、1-已审、2-注销)
        /// </summary>
        public short State { get; set; }
        /// <summary>
        /// 类别(1:在线/2:独立)
        /// </summary>
        public short Category { get { return _Category; } set { _Category = value; } }
        /// <summary>
        /// 单位简称
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 单位全称
        /// </summary>
        public string FullTitle { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>
        public string LinkMan { get; set; }
        /// <summary>
        /// 联系电话
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 用户数
        /// </summary>
        public short UserNum { get; set; }
        /// <summary>
        /// 门店数
        /// </summary>
        public short StoreNum { get; set; }
        /// <summary>
        /// 分店专属后台(Y/N)
        /// </summary>
        public string StoreProper { get; set; }
        /// <summary>
        /// APP手机端(Y/N)
        /// </summary>
        public string AppProper { get; set; }
        /// <summary>
        /// POS次屏显示(Y/N)
        /// </summary>
        public string PosMinorDisp { get; set; }
        /// <summary>
        /// 供应商专属后台(Y/N)
        /// </summary>
        public string SupplierProper { get; set; }
        /// <summary>
        /// 批发商专属后台(Y/N)
        /// </summary>
        public string WholesalerProper { get; set; }
        /// <summary>
        /// 开通时间
        /// </summary>
        public DateTime CreateDT { get; set; }
        /// <summary>
        /// 生效日期
        /// </summary>
        [JsonConverter(typeof(JsonShortDate))]
        public DateTime? EffectiveDT { get; set; }
        /// <summary>
        /// 有效期(年)
        /// </summary>
        public short ValidityYear { get; set; }
        /// <summary>
        /// 截止日期
        /// </summary>
        [JsonConverter(typeof(JsonShortDate))]
        public DateTime? ExpirationDT { get { return EffectiveDT.HasValue ? EffectiveDT.Value.AddYears(ValidityYear) : new Nullable<DateTime>(); } }
        /// <summary>
        /// 可用状态(Y/N)
        /// </summary>
        public string Useable { get; set; }

        /// <summary>
        /// 机器码（设备码）
        /// </summary>
        public string MachineSN { get { return _MachineSN; } set { _MachineSN = value; } }
        string _MachineSN = string.Empty;
        /// <summary>
        /// 序列号
        /// </summary>
        public string SerialNo { get; set; }
        /// <summary>
        /// 区号
        /// </summary>
        public string AreaCode { get; set; }
    }
}