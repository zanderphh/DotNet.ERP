﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pharos.Logic.ApiData.Pos.Entity.LocalCeEntity
{
    public class CommodityDiscount:BaseEntity
    {
        public string CommodityId { get; set; }
        public string Barcode { get; set; }
        public int CategorySN { get; set; }
        public decimal? DiscountRate { get; set; }
        public decimal? DiscountPrice { get; set; }
        public decimal MinPurchaseNum { get; set; }
        public short Way { get; set; }
        public short? CategoryGrade { get; set; }
    }
}