﻿using Pharos.Logic.BLL.DataSynchronism;
using Pharos.Logic.BLL.DataSynchronism.Dtos;
using Pharos.Logic.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pharos.Logic.BLL.DataSynchronism.Services
{
    public class PromotionBlendDataSyncService : BaseDataSyncService<PromotionBlend, PromotionBlendForLocal>
    {
        public override IEnumerable<PromotionBlend> Download(string storeId, string entityType)
        {
            var ids = CommodityPromotionService.GetEffectiveId(storeId).ToList();
            return CurrentRepository.FindList(o => ids.Contains(o.CommodityId)).ToList();
        }
    }
}