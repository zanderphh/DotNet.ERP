﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pharos.Utility;
using System.ComponentModel.DataAnnotations.Schema;
namespace Pharos.Logic.Entity
{
    [Excel("商品信息")]
    public class VwProduct:BaseProduct
    {
        public string PrefixTitle { get; set; }
        public string BigCategoryTitle { get; set; }
        public string SubCategoryTitle { get; set; }
        public string MidCategoryTitle { get; set; }

        //public string CategoryTitle { get { return BigCategoryTitle+(!string.IsNullOrWhiteSpace(MidCategoryTitle) ? "/"+MidCategoryTitle+(!string.IsNullOrWhiteSpace(SubCategoryTitle)?"/"+SubCategoryTitle:""):""); } }
        public string CategoryTitle { get; set; }
        public string ValuationTypeTitle { get { return ValuationType == 1 ? "计件" : "称重"; } }
        public string BrandTitle { get; set; }
        public string SupplierTitle { get; set; }
        public string BigUnit { get; set; }
        public string SubUnit { get; set; }
        /// <summary>
        /// 收货量
        /// </summary>
        public decimal? AcceptNums { get; set; }
        /// <summary>
        /// 库存量=收货量-出售量
        /// </summary>
        public decimal StockNums { get { return AcceptNums.HasValue?AcceptNums.Value - PurchaseNumbers:0; } }
        /// <summary>
        /// 门店库存量
        /// </summary>
        [NotMapped]
        public decimal? StoreStockNums { get; set; }
        /// <summary>
        /// 折后价
        /// </summary>
        public decimal DiscountPrice { get; set; }
        /// <summary>
        /// 出售数量
        /// </summary>
        public int PurchaseNumbers { get; set; }
        /// <summary>
        /// 产品状态
        /// </summary>
        public string StateTitle { get { return State == 1 ?"已上架": "已下架" ; } }
        /// <summary>
        /// 退货状态
        /// </summary>
        public string ReturnTitle { get { return IsReturnSale == 1 ? "允许" : "不允许"; } }
        /// <summary>
        /// 订货状态
        /// </summary>
        public string AcceptTitle { get { return IsAcceptOrder == 1 ? "允许":"不允许"; } }
        /// <summary>
        /// 前台优惠状态
        /// </summary>
        public string FavorTitle { get { return Favorable == 1 ? "允许":"不允许"; } }
        /// <summary>
        /// 拆分子商品数
        /// </summary>
        public int ChildCount { get; set; }
    }
}