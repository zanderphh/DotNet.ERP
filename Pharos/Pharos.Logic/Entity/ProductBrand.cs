// --------------------------------------------------
// Copyright (C) 2015 版权所有
// 创 建 人：蔡少发
// 创建时间：2015-05-22
// 描述信息：用于管理本系统的产品档案所附属的品牌信息
// --------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Pharos.Utility;
using Pharos.Sys.Extensions;

namespace Pharos.Logic.Entity
{
    /// <summary>
    /// 品牌信息表
    /// </summary>
    [Serializable]

    [Excel("品牌信息")]
    public class ProductBrand : SyncEntity
    {


        ///// <summary>
        ///// 记录ID
        ///// [主键：√]
        ///// [长度：10]
        ///// [不允许为空]
        ///// </summary>
        //[OperationLog("ID", false)]
        //public int Id { get; set; }

        [Excel("品牌分类ID", 1)]
        [OperationLog("品牌分类ID", true)]
        /// <summary>
        /// 品牌分类ID（来自数据字典表）
        /// [长度：10]
        /// [不允许为空]
        /// [默认值：((-1))]
        /// </summary>
        public int ClassifyId { get; set; }

        [Excel("品牌编号", 2)]
        [OperationLog("品牌编号", false)]
        /// <summary>
        /// 品牌编号（全局唯一)
        /// [长度：10]
        /// [不允许为空]
        /// </summary>
        public int BrandSN { get; set; }

        [Excel("品牌名称", 3)]
        [OperationLog("品牌名称", false)]
        /// <summary>
        /// 品牌名称
        /// [长度：20]
        /// [不允许为空]
        /// </summary>
        public string Title { get; set; }

        [Excel("品牌简拼", 4)]
        [OperationLog("品牌简拼", false)]
        /// <summary>
        /// 品牌简拼
        /// [长度：10]
        /// </summary>
        public string JianPin { get; set; }

        [Excel("状态", 5)]
        [OperationLog("状态", "0:禁用", "1:可用")]
        /// <summary>
        /// 状态（0:禁用、1:可用） 
        /// [长度：5]
        /// [不允许为空]
        /// [默认值：((1))]
        /// </summary>
        public short State { get; set; }
        /// <summary>
        /// 商品数
        /// </summary>
        [NotMapped]
        public virtual int Num { get; set; }
    }
}