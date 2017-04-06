﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pharos.Logic.OMS.Entity;
using Pharos.Logic.OMS.Entity.View;

namespace Pharos.Logic.OMS.IDAL
{
    /// <summary>
    /// 代理商档案接口
    /// </summary>
    public interface IAgentsInfoRepository : IBaseRepository<AgentsInfo>
    {
        List<ViewAgentsInfo> getPageList(int CurrentPage, int PageSize, string strw, out int Count);
    }
}