// --------------------------------------------------
// Copyright (C) 2017 版权所有
// 创 建 人：
// 创建时间：
// 描述信息：
// --------------------------------------------------

using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace Pharos.Logic.OMS.Entity
{
	/// <summary>
	/// 用于管理本系统的审核日志信息
	/// </summary>
	[Serializable]
	public class ApproveLog
	{
		/// <summary>
		/// 记录ID
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
		/// 模块编号（1:支付许可）
		/// [长度：5]
		/// [不允许为空]
		/// </summary>
		public short ModuleNum
		{
			get { return _ModuleNum; }
			set { _ModuleNum = value; }
		}
		private short _ModuleNum;

		/// <summary>
		/// 操作项目ID（来自具体的业务主表ID）
		/// [长度：40]
		/// [不允许为空]
		/// </summary>
		public string ItemId
		{
			get { return _ItemId; }
			set { _ItemId = value; }
		}
		private string _ItemId;

		/// <summary>
		/// 操作时间
		/// [长度：23，小数位数：3]
		/// [不允许为空]
		/// </summary>
		public DateTime CreateTime
		{
			get { return _CreateTime; }
			set { _CreateTime = value; }
		}
		private DateTime _CreateTime;

		/// <summary>
		/// 动作名称（1:提交申请、2:查阅、3:审批、4:驳回、5:重新提交）
		/// [长度：10]
		/// [不允许为空]
		/// </summary>
		public short OperationType
		{
			get { return _OperationType; }
			set { _OperationType = value; }
		}
		private short _OperationType;

		/// <summary>
		/// 描述说明
		/// [长度：1000]
		/// [不允许为空]
		/// </summary>
		public string Description
		{
			get { return _Description; }
			set { _Description = value; }
		}
		private string _Description;

		/// <summary>
		/// 操作人
		/// [长度：40]
		/// [不允许为空]
		/// </summary>
		public string OperatorUID
		{
			get { return _OperatorUID; }
			set { _OperatorUID = value; }
		}
		private string _OperatorUID;

	}
}
