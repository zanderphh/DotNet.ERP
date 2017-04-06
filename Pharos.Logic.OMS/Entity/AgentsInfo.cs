// --------------------------------------------------
// Copyright (C) 2016 版权所有
// 创 建 人：
// 创建时间：2016-11-21
// 描述信息：代理商档案
// --------------------------------------------------

using System;

namespace Pharos.Logic.OMS.Entity
{
	/// <summary>
	/// 代理商档案
	/// </summary>
	[Serializable]
	public class AgentsInfo
	{
		/// <summary>
		/// 
		/// [主键：√]
		/// [长度：10]
		/// [不允许为空]
		/// </summary>
		public int Id
		{
			get { return _Id; }
			set { _Id = value; }
		}
		private int _Id;

		/// <summary>
		/// 代理商编号，全局唯一（6位，从100001开始递增到999999）
		/// [长度：10]
		/// [不允许为空]
		/// </summary>
		public int AgentsId
		{
			get { return _AgentsId; }
			set { _AgentsId = value; }
		}
		private int _AgentsId;

		/// <summary>
		/// 代理商类型（来自字典）
		/// [长度：10]
		/// [不允许为空]
		/// </summary>
		public int Type
		{
			get { return _Type; }
			set { _Type = value; }
		}
		private int _Type;

		/// <summary>
		/// 代理商全称
		/// [长度：50]
		/// [不允许为空]
		/// </summary>
		public string FullName
		{
			get { return _FullName; }
			set { _FullName = value; }
		}
		private string _FullName;

		/// <summary>
		/// 代理商简称
		/// [长度：20]
		/// [不允许为空]
		/// </summary>
		public string Name
		{
			get { return _Name; }
			set { _Name = value; }
		}
		private string _Name;

		/// <summary>
        /// 代理商状态（1:待审，2:正常，3:终止，4:到期）
		/// [长度：10]
		/// [不允许为空]
		/// [默认值：((0))]
		/// </summary>
		public int Status
		{
			get { return _Status; }
			set { _Status = value; }
		}
		private int _Status;

		/// <summary>
		/// 上级代理商编号
		/// [长度：10]
		/// [不允许为空]
		/// </summary>
		public int PAgentsId
		{
			get { return _PAgentsId; }
			set { _PAgentsId = value; }
		}
		private int _PAgentsId;

		/// <summary>
		/// 有效年限（来自字典）
		/// [长度：10]
		/// [不允许为空]
		/// </summary>
		public int ValidityYear
		{
			get { return _ValidityYear; }
			set { _ValidityYear = value; }
		}
		private int _ValidityYear;

		/// <summary>
		/// 有效-起始日期
		/// [长度：10]
		/// [不允许为空]
		/// </summary>
		public string StartTime
		{
			get { return _StartTime; }
			set { _StartTime = value; }
		}
		private string _StartTime;

		/// <summary>
		/// 有效-终止日期
		/// [长度：10]
		/// [不允许为空]
		/// </summary>
		public string EndTime
		{
			get { return _EndTime; }
			set { _EndTime = value; }
		}
		private string _EndTime;

		/// <summary>
        /// 代理区域（城市ID，多个以逗号为分隔，空为不限）
		/// [长度：4000]
		/// </summary>
		public string AgentAreaIds
		{
			get { return _AgentAreaIds; }
			set { _AgentAreaIds = value; }
		}
		private string _AgentAreaIds;

        /// <summary>
        /// 代理区域（城市名称，多个以逗号为分隔，空为不限）
        /// </summary>
        public string AgentAreaNames { get; set; }

		/// <summary>
		/// 合同编号
		/// [长度：20]
		/// </summary>
		public string Contract
		{
			get { return _Contract; }
			set { _Contract = value; }
		}
		private string _Contract;

		/// <summary>
		/// 法人姓名
		/// [长度：20]
		/// </summary>
		public string CorporateName
		{
			get { return _CorporateName; }
			set { _CorporateName = value; }
		}
		private string _CorporateName;

		/// <summary>
		/// 法人身份证
		/// [长度：20]
		/// </summary>
		public string IdCard
		{
			get { return _IdCard; }
			set { _IdCard = value; }
		}
		private string _IdCard;

		/// <summary>
		/// 证件照
		/// [长度：200]
		/// </summary>
		public string IdCardPhoto
		{
			get { return _IdCardPhoto; }
			set { _IdCardPhoto = value; }
		}
		private string _IdCardPhoto;

		/// <summary>
		/// 公司电话
		/// [长度：20]
		/// </summary>
		public string CompanyPhone
		{
			get { return _CompanyPhone; }
			set { _CompanyPhone = value; }
		}
		private string _CompanyPhone;

		/// <summary>
		/// 公司地址
		/// [长度：100]
		/// </summary>
		public string Address
		{
			get { return _Address; }
			set { _Address = value; }
		}
		private string _Address;

		/// <summary>
		/// 联系人
		/// [长度：20]
		/// </summary>
		public string LinkMan
		{
			get { return _LinkMan; }
			set { _LinkMan = value; }
		}
		private string _LinkMan;

		/// <summary>
		/// 联系电话1
		/// [长度：20]
		/// </summary>
		public string Phone1
		{
			get { return _Phone1; }
			set { _Phone1 = value; }
		}
		private string _Phone1;

		/// <summary>
		/// 联系电话2
		/// [长度：20]
		/// </summary>
		public string Phone2
		{
			get { return _Phone2; }
			set { _Phone2 = value; }
		}
		private string _Phone2;

		/// <summary>
		/// QQ
		/// [长度：20]
		/// </summary>
		public string QQ
		{
			get { return _QQ; }
			set { _QQ = value; }
		}
		private string _QQ;

		/// <summary>
		/// Email
		/// [长度：50]
		/// </summary>
		public string Email
		{
			get { return _Email; }
			set { _Email = value; }
		}
		private string _Email;

		/// <summary>
		/// 微信号
		/// [长度：50]
		/// </summary>
		public string Weixin
		{
			get { return _Weixin; }
			set { _Weixin = value; }
		}
		private string _Weixin;

		/// <summary>
		/// 创建时间
		/// [长度：23，小数位数：3]
		/// [不允许为空]
		/// [默认值：(getdate())]
		/// </summary>
		public DateTime CreateTime
		{
			get { return _CreateTime; }
			set { _CreateTime = value; }
		}
		private DateTime _CreateTime;

		/// <summary>
		/// 修改时间
		/// [长度：23，小数位数：3]
		/// [不允许为空]
		/// [默认值：(getdate())]
		/// </summary>
		public DateTime UpdateTime
		{
			get { return _UpdateTime; }
			set { _UpdateTime = value; }
		}
		private DateTime _UpdateTime;

        /// <summary>
        /// 代理商编号2
        /// </summary>
        public string AgentsId2 { get; set; }

		/// <summary>
		/// 创建人GUID（来自SysUser/AgentsUsers表）
		/// [长度：40]
		/// [不允许为空]
		/// </summary>
		public string CreateUid
		{
			get { return _CreateUid; }
			set { _CreateUid = value; }
		}
		private string _CreateUid;

		/// <summary>
		/// 指派人GUID（来自SysUser/AgentsUsers表编号）
		/// [长度：40]
		/// [不允许为空]
		/// </summary>
		public string AssignUid
		{
			get { return _AssignUid; }
			set { _AssignUid = value; }
		}
		private string _AssignUid;

        /// <summary>
        /// 创建类型（1是平台创建，2是代理商创建）
        /// </summary>
        public int CreateType { get; set; }
	}
}