// --------------------------------------------------
// Copyright (C) 2016 版权所有
// 创 建 人：linbl
// 创建时间：2016-12-30
// 描述信息：
// --------------------------------------------------

using System;

namespace Pharos.Logic.OMS.Entity
{
	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public class SysUser
	{
		/// <summary>
		/// 
		/// [主键：√]
		/// [长度：10]
		/// [不允许为空]
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// 
		/// [长度：40]
		/// [不允许为空]
		/// </summary>
		public string UserId { get; set; }

		/// <summary>
		/// 
		/// [长度：20]
		/// [不允许为空]
		/// </summary>
		public int? UserCode { get; set; }

		/// <summary>
		/// 
		/// [长度：20]
		/// [不允许为空]
		/// </summary>
		public string FullName { get; set; }

		/// <summary>
		/// 
		/// [长度：100]
		/// [不允许为空]
		/// </summary>
		public string QuanPin { get; set; }

		/// <summary>
		/// 
		/// [长度：20]
		/// [不允许为空]
		/// </summary>
		public string LoginName { get; set; }

		/// <summary>
		/// 
		/// [长度：50]
		/// [不允许为空]
		/// </summary>
		public string LoginPwd { get; set; }

		/// <summary>
		/// 性别（-1:保密、0:女、1:男）
		/// [长度：5]
		/// [不允许为空]
		/// [默认值：((-1))]
		/// </summary>
		public short Sex { get; set; }

		/// <summary>
		/// 岗位
		/// [长度：40]
		/// [不允许为空]
		/// </summary>
		public string PositId { get; set; }

		/// <summary>
		/// 
		/// [长度：2000]
		/// </summary>
		public string RoleIds { get; set; }

		/// <summary>
		/// 
		/// [长度：1]
		/// [不允许为空]
		/// [默认值：((0))]
		/// </summary>
		public bool IsSuper { get; set; }

		/// <summary>
		/// 部门
		/// [长度：10]
		/// [不允许为空]
		/// </summary>
		public int DeptId { get; set; }

		/// <summary>
		/// 状态（1:正常、2:锁定、3:注销）
		/// [长度：5]
		/// [不允许为空]
		/// </summary>
		public short Status { get; set; }

		/// <summary>
		/// 
		/// [长度：10]
		/// [不允许为空]
		/// [默认值：((0))]
		/// </summary>
		public int LoginNum { get; set; }

		/// <summary>
		/// 
		/// [长度：23，小数位数：3]
		/// [不允许为空]
		/// [默认值：(getdate())]
		/// </summary>
		public DateTime LoginDT { get; set; }

		/// <summary>
		/// 
		/// [长度：23，小数位数：3]
		/// [不允许为空]
		/// [默认值：(getdate())]
		/// </summary>
		public DateTime CreateDT { get; set; }

		/// <summary>
		/// 
		/// [长度：40]
		/// [不允许为空]
		/// </summary>
		public string CreateUID { get; set; }
	}
}