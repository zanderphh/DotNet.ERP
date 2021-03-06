﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Pharos.Logic.DAL
{
    internal class PrivilegeProductDAL
    {
        /// <summary>
        /// 系列中显示赠品
        /// </summary>
        /// <param name="solid"></param>
        /// <returns></returns>
        public DataTable GetProductGifts(int solid)
        {
            string sql = @"select a.Id,a.BarcodeOrCategorySN+'~0' StrId,0 BrandSN,
                    a.BarcodeOrCategorySN CategorySN,
                    dbo.F_ProductCategoryDescForSN(a.BarcodeOrCategorySN,1,1,@companyId) BigCategoryTitle,
                    dbo.F_ProductCategoryDescForSN(a.BarcodeOrCategorySN,2,1,@companyId) MidCategoryTitle ,
                    dbo.F_ProductCategoryDescForSN(a.BarcodeOrCategorySN,3,1,@companyId) SubCategoryTitle
                FROM dbo.PrivilegeProduct a
                WHERE a.Type=2 AND a.PrivilegeSolutionId=@id";
            DataTable dt = new DataTable();
            using (EFDbContext db = new EFDbContext())
            {
                //var dt= db.Database.SqlQuery<DataTable>(sql, new SqlParameter[] { });
                var conn = db.Database.Connection;
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.Add(new SqlParameter("@id", solid));
                cmd.Parameters.Add(new SqlParameter("@companyId", Sys.SysCommonRules.CompanyId));
                var dr = cmd.ExecuteReader();
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    dt.Columns.Add(dr.GetName(i), dr.GetFieldType(i));
                }
                int k=0;
                while(dr.Read())
                {
                    var row= dt.NewRow();
                    foreach(DataColumn col in dt.Columns)
                    {
                        var obj = dr[col.ColumnName];
                        row[col.ColumnName] = obj;
                    }
                    dt.Rows.Add(row);
                    k++;
                }
                dr.Close();
                conn.Close();
            }
            return dt;
        }
        /// <summary>
        /// 订单计算赠品
        /// </summary>
        /// <param name="supplierId"></param>
        /// <param name="barcodeOrCategorySN"></param>
        /// <param name="ordernum"></param>
        /// <returns></returns>
        public DataTable GetProductGifts(string supplierId,IEnumerable<string> barcodeOrCategorySN, decimal ordernum)
        {
            string sql = @"SELECT  b.StartVal,b.EndVal,c.BarcodeOrCategorySN,c.Type,
                STUFF((SELECT '<br/>'+Barcode+' '+dbo.F_ProductNameBybarcode(Barcode,CompanyId)+' '+ CAST(GiftNumber AS VARCHAR(20))+'件' from PrivilegeProductGift WHERE RegionValId=d.Id FOR XML PATH('')),1,11,'') detail,
                STUFF((SELECT ','+Barcode+'~'+CAST(GiftNumber AS VARCHAR(20)) from PrivilegeProductGift WHERE RegionValId=d.Id FOR XML PATH('')),1,1,'') barnum
                FROM dbo.PrivilegeSolution a 
                INNER join dbo.PrivilegeRegion b ON b.PrivilegeSolutionId=a.Id
                INNER join dbo.PrivilegeProduct c ON c.PrivilegeSolutionId=a.Id
                INNER JOIN PrivilegeRegionVal d ON d.PrivilegeProductId=c.Id AND d.PrivilegeRegionId=b.Id
                WHERE a.ModeSN=46 and a.CompanyId=" + Sys.SysCommonRules.CompanyId+" and  ','+a.SupplierIds+',' LIKE '%," + supplierId + ",%' AND c.BarcodeOrCategorySN IN(" + string.Join(",", barcodeOrCategorySN.Select(o => "'" + o + "'")) + ") ORDER BY c.Type ";
            //sql += "AND b.StartVal<=" + ordernum + " and b.EndVal>=" + ordernum;
            DataTable dt = new DataTable();
            using (EFDbContext db = new EFDbContext())
            {
                //var dt= db.Database.SqlQuery<DataTable>(sql, new SqlParameter[] { });
                var conn = db.Database.Connection;
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                var dr = cmd.ExecuteReader();
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    dt.Columns.Add(dr.GetName(i), dr.GetFieldType(i));
                }
                int k = 0;
                while (dr.Read())
                {
                    var row = dt.NewRow();
                    foreach (DataColumn col in dt.Columns)
                    {
                        var obj = dr[col.ColumnName];
                        row[col.ColumnName] = obj;
                    }
                    dt.Rows.Add(row);
                    k++;
                }
                dr.Close();
                conn.Close();
            }
            return dt;
        }
    }
}
