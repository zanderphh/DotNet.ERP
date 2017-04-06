// --------------------------------------------------
// Copyright (C) 2017 版权所有
// 创 建 人：
// 创建时间：2017-02-14
// 描述信息：
// --------------------------------------------------

using System;

namespace Pharos.Logic.OMS.Entity
{
	/// <summary>
	/// 产品更新日志信息
	/// </summary>
	[Serializable]
	public class ProductUpdateLog
	{
		/// <summary>
		/// 
		/// [主键：√]
		/// [长度：10]
		/// [不允许为空]
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// 发布编号
		/// [长度：10]
		/// [不允许为空]
		/// </summary>
		public int PublishId { get; set; }

		/// <summary>
		/// 商户号
		/// [长度：10]
		/// [不允许为空]
		/// </summary>
		public int CID { get; set; }

		/// <summary>
		/// 状态(0-成功，1-失败)
		/// [长度：5]
		/// [不允许为空]
		/// </summary>
		public short Status { get; set; }

		/// <summary>
		/// 
		/// [长度：-1]
		/// </summary>
		public string Description { get; set; }

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
		/// </summary>
		public string CreateUID { get; set; }

		/// <summary>
		/// 创建人
		/// [长度：50]
		/// [不允许为空]
		/// </summary>
		public string Creater { get; set; }
	}
}