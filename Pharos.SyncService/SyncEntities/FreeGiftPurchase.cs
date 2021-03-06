﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pharos.SyncService.SyncEntities
{
    /// <summary>
    /// 买赠促销
    /// </summary>
    [Serializable]
    public class FreeGiftPurchase : SyncDataObject
    {
        public int CompanyId { get; set; }
        public string CommodityId { get; set; }
        public string GiftId { get; set; }
        public decimal MinPurchaseNum { get; set; }
        public short RestrictionBuyNum { get; set; }
        public short GiftType { get; set; }
        public string BarcodeOrCategorySN { get; set; }
        public int BrandSN { get; set; }
        public short? CategoryGrade { get; set; }
    }
}
