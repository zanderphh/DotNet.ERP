﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pharos.POS.Retailing.Models.ApiParams
{
    public class AddMemberParams : BaseApiParams
    {
        /// <summary>
        /// 会员编号
        /// [长度：100]
        /// [不允许为空]
        /// </summary>
        public string MemberNo { get; set; }

        /// <summary>
        /// 会员姓名
        /// [长度：20]
        /// </summary>
        public string RealName { get; set; }
        /// <summary>
        /// 性别（ -1: 未知、 0:女、 1: 男）
        /// </summary>
        public bool Sex { get; set; }
        /// <summary>
        /// 微信号
        /// </summary>
        public string Weixin { get; set; }
        /// <summary>
        /// 支付宝
        /// </summary>
        public string Zhifubao { get; set; }
        /// <summary>
        /// 手机号（全局唯一）
        /// [长度：11]
        /// </summary>
        public string MobilePhone { get; set; }

        public decimal YaJin { get; set; }
        public string CardNo { get; set; }


        /// <summary>
        /// Email（全局唯一）
        /// [长度：100]
        /// </summary>
        public string Email { get; set; }


        /// <summary>
        /// 生日（yyyy-MM-dd）
        /// [长度：10]
        /// </summary>
        public string Birthday { get; set; }

        /// <summary>
        /// 所在城市ID
        /// [长度：10]
        /// </summary>
        public int CurrentCityId { get; set; }
        /// <summary>
        /// 省份ID
        /// </summary>
        public int ProvinceID { get; set; }

        /// <summary>
        /// 所在区县ID
        /// [长度：10]
        /// </summary>
        public int CurrentCountyId { get; set; }
        /// <summary>
        /// 联系地址
        /// </summary>
        public string Address { get; set; }
    }
}
