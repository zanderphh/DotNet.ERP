// --------------------------------------------------
// Copyright (C) 2015 版权所有
// 创 建 人：蔡少发
// 创建时间：2015-05-22
// 描述信息：用于管理本系统的所有订货（进货）的退换信息
// --------------------------------------------------

using System;

namespace Pharos.Logic.Entity
{
	/// <summary>
	/// 订货退换信息
	/// </summary>
	[Serializable]
	public partial class OrderReturns:BaseEntity
	{
		/// <summary>
		/// 退换ID
		/// [主键：√]
		/// [长度：10]
		/// [不允许为空]
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// 订货单ID
		/// [长度：40]
		/// [不允许为空]
		/// </summary>
		public string IndentOrderId { get; set; }

		/// <summary>
		/// 配送ID 
		/// [长度：40]
		/// </summary>
		public string DistributionId { get; set; }
        /// <summary>
        /// 条码
        /// </summary>
        public string Barcode { get; set; }
        /// <summary>
        /// 配送批次
        /// </summary>
        public string DistributionBatch { get; set; }

		/// <summary>
		/// 退换方式（0:退货、1:换货）
		/// [长度：5]
		/// [不允许为空]
		/// </summary>
		public short? ReturnType { get; set; }

		/// <summary>
		/// 退换数量 
		/// [不允许为空]
		/// </summary>
        public decimal? ReturnNum { get; set; }

		/// <summary>
		/// 退换理由ID（来自数据字典）
		/// [长度：10]
        /// [默认值：-1)]
		/// [不允许为空]
		/// </summary>
		public int ReasonId { get; set; }

		/// <summary>
		/// 申请时间
		/// [长度：23，小数位数：3]
		/// [不允许为空]
		/// [默认值：(getdate())]
		/// </summary>
        [Pharos.Utility.Exclude]
		public DateTime CreateDT { get; set; }

		/// <summary>
		/// 申请人UID
		/// [长度：40]
		/// [不允许为空]
		/// </summary>
        [Pharos.Utility.Exclude]
		public string CreateUID { get; set; }

        /// <summary>
        /// 状态（0：未处理；1：处理中；2：已完成）
        /// [不允许为空]
        /// [默认值：0]
        /// </summary>
        [Pharos.Utility.Exclude]
        public Int16? State { get; set; }

        /// <summary>
        /// 备注
        /// [长度：200]
        /// </summary>
        public string Memo { get; set; }

	}
}
