﻿using Pharos.Logic.ApiData.Pos.ValueObject;
using Pharos.Logic.BLL;
using Pharos.Logic.DAL;
using Pharos.Logic.Entity;
using Pharos.ObjectModels.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pharos.Logic.ApiData.Pos.Services
{
    public class ProductCategorySerivce : BaseGeneralService<ProductCategory, EFDbContext>
    {
        public static IEnumerable<CategoryDAO> GetStoreCategories(string storeId, int companyId)
        {
            try
            {
                var result = CurrentRepository._context.Database.SqlQuery<CategoryDAO>(@"with cte as
                (
                    select l.CategoryPSN,l.CategorySN,l.Title,r.StoreId,l.OrderNum,l.Grade,0 as lvl from ProductCategory l
                    right join Warehouse r on  ','+r.CategorySN+',' like '%,'+ convert(varchar(200),l.CategorySN) +',%' 
	                where   r.StoreId=@p0 and l.CompanyId =@p1 and r.CompanyId =@p1
                    union all
                    select d.CategoryPSN,d.CategorySN,d.Title,c.StoreId,d.OrderNum,d.Grade,lvl+1 from cte c 
	                inner join ProductCategory d on c.CategorySN = d.CategoryPSN and d.companyId = @p1
                )
                select CategoryPSN,CategorySN,Title,OrderNum,Grade  from cte ", storeId, companyId).ToList();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static IEnumerable<CategoryDAO> GetLastDepthStoreCategories(string storeId, int category, int depth, int companyId)
        {
            var result = CurrentRepository._context.Database.SqlQuery<CategoryDAO>(@"with cte as
            (
                select l.CategoryPSN,l.CategorySN,l.Title,r.StoreId,l.OrderNum,l.Grade,0 as lvl from ProductCategory l
                right join Warehouse r on  ','+r.CategorySN+',' like '%,'+ convert(varchar(200),l.CategorySN) +',%' 
	            where   r.StoreId=@p0 and (@p2 != l.Grade or (@p2 = l.Grade and l.CategorySN = @p1)) and l.CompanyId =@p3 and r.CompanyId =@p3
                union all
                select d.CategoryPSN,d.CategorySN,d.Title,c.StoreId,d.OrderNum,d.Grade,lvl+1 from cte c 
	            inner join ProductCategory d on c.CategorySN = d.CategoryPSN and d.companyId = @p3
                where  (@p2 != d.Grade or (@p2 = d.Grade and d.CategorySN = @p1))
            )
            select CategoryPSN,CategorySN,Title,OrderNum,Grade  from cte WHERE Grade=@p2", storeId, category, depth, companyId).ToList();
            return result;
        }
    }
}