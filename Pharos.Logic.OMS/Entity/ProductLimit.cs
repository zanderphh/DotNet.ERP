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
	/// 产品对应的权限信息
	/// </summary>
	[Serializable]
	public partial class ProductLimit
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
		public string ProductId { get; set; }

		/// <summary>
		/// 编号
		/// [长度：10]
		/// [不允许为空]
		/// </summary>
		public int? LimitId { get; set; }

		/// <summary>
		/// 菜单
		/// [长度：10]
		/// [不允许为空]
		/// </summary>
		public int? MenuId { get; set; }

		/// <summary>
		/// 
		/// [长度：5]
		/// [不允许为空]
		/// </summary>
		public short SortOrder { get; set; }

		/// <summary>
		/// 
		/// [长度：50]
		/// [不允许为空]
		/// </summary>
		public string Title { get; set; }
        public string Memo { get; set; }
		/// <summary>
		/// 是否可用
		/// [长度：1]
		/// [不允许为空]
		/// [默认值：((1))]
		/// </summary>
		public bool? Status { get; set; }

		/// <summary>
		/// getdate()
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
