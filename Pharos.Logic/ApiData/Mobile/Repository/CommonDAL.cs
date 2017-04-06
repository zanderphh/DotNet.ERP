﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Pharos.Logic.DAL;

namespace Pharos.Logic.ApiData.Mobile.Repository
{
    internal class CommonDAL:BaseDAL
    {
        public DataTable SaleDetailReport(string storeId,DateTime start,DateTime end)
        {
            string sql = @"SELECT TOP 10 * FROM (
                SELECT a.Barcode,a.Title,SUM(a.PurchaseNumber) PurchaseNumber,SUM(a.PurchaseNumber*a.ActualPrice) ActualTotal FROM dbo.SaleDetail a 
                INNER JOIN dbo.SaleOrders b ON a.PaySN=b.PaySN where State=0 and Type=0 and storeid='{0}' AND a.CompanyId={3} and createdt between '{1}' and '{2}'
                GROUP BY a.Barcode,a.Title) t ORDER BY t.ActualTotal desc";
            sql = string.Format(sql, storeId, start.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"),Sys.SysCommonRules.CompanyId);
            return _db.DataTableText(sql, null);
        }
        public DataTable GetNoticeList(string userCode,bool notRead)
        {
            string sql = @"SELECT * FROM (
                    SELECT id,Theme AS Title,NoticeContent Content,State,url,CONVERT(VARCHAR(30),CreateDT,120) CreateDT,CASE WHEN EXISTS(SELECT 1 FROM dbo.Reader WHERE Type=1 AND MainId=a.id AND ReadCode='{0}') THEN 1 ELSE 0 END Flag,a.CompanyId FROM dbo.Notice a
                ) t where State=1 {1} and companyid={2} ORDER BY t.Flag,t.CreateDT desc";
            sql = string.Format(sql, userCode, notRead ? " and Flag=0 " : "",Sys.SysCommonRules.CompanyId);
            return _db.DataTableText(sql, null);
        }
        public DataTable SaleGiftReport(DateTime start, DateTime end)
        {
            string sql = @"SELECT * FROM(SELECT a.StoreId,a.Title,ISNULL(SUM(d.PurchaseNumber),0) PurchaseNumber,ISNULL(SUM(d.SysPrice*d.PurchaseNumber),0) GiftTotal FROM dbo.Warehouse a
			LEFT JOIN (SELECT c.PurchaseNumber,c.SysPrice,b.StoreId FROM dbo.SaleOrders b ,dbo.SaleDetail c WHERE b.PaySN=c.PaySN AND b.CompanyId=c.CompanyId AND b.CompanyId=@companyid AND c.SalesClassifyId IN(161,49)
			AND CreateDT BETWEEN @start AND @end) AS d ON a.StoreId=d.StoreId
			GROUP BY a.StoreId,a.Title) t ORDER BY t.GiftTotal desc";
            SqlParameter[] parms = new SqlParameter[] { 
                new SqlParameter("start",start),
                new SqlParameter("@end",end),
                new SqlParameter("@companyid",Sys.SysCommonRules.CompanyId)
            };
            return _db.DataTableText(sql,parms);
        }
    }
}