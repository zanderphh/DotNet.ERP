﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pharos.Logic.BLL;
using Pharos.Logic.Entity;
using Pharos.Utility.Helpers;


namespace Pharos.Logic.DAL
{
    public class ScaleSettingsService : BaseService<ScaleSettings>
    {
        public List<ScaleSettings> FindPageListByCID(int cid, out int count)
        {
            var query = CurrentRepository.Entities.Where(o => o.CompanyId == cid && o.Store == Sys.SysCommonRules.CurrentStore);
            count = query.Count();
            return query.ToPageList();
        }
    }
}