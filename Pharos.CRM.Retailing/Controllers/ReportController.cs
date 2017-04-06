﻿using Pharos.Logic.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pharos.Utility.Helpers;
using Pharos.Utility;
using System.Data;
namespace Pharos.CRM.Retailing.Controllers
{
    public class ReportController : BaseController
    {
        //
        // GET: /Report/

        public ActionResult Index()
        {
            return View();
        }
        #region 商品销售明细报表
        public ActionResult ProductSaleDetails()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            //ViewBag.suppliers = ListToSelect(SupplierService.GetList().Where(o => o.BusinessType == 1).Select(o => new SelectListItem() { Value = o.Id.ToString(), Text = o.Title }), emptyTitle: "全部");
            ViewBag.brandsns = ListToSelect(ProductBrandService.GetList(true).Select(o => new SelectListItem() { Value = o.BrandSN.ToString(), Text = o.Title }), emptyTitle: "全部");
            return View();
        }
        [HttpPost]
        public ActionResult QueryProductSaleDetailsPageList()
        {
            object footer = new object();
            decimal count = 0;
            var dt = ReportBLL.QueryProductSaleDetailsPageList(Request.Params, ref footer, ref count);
            Session["sales"] = dt;
            return ToDataGrid(dt, Convert.ToInt32(count), footer);
        }
        public ActionResult ProductSaleDetailDays()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            //ViewBag.suppliers = ListToSelect(SupplierService.GetList().Where(o => o.BusinessType == 1).Select(o => new SelectListItem() { Value = o.Id.ToString(), Text = o.Title }), emptyTitle: "全部");
            ViewBag.brandsns = ListToSelect(ProductBrandService.GetList(true).Select(o => new SelectListItem() { Value = o.BrandSN.ToString(), Text = o.Title }), emptyTitle: "全部");
            return View();
        }
        [HttpPost]
        public ActionResult QueryProductSaleDetailDaysPageList()
        {
            object footer = new object();
            decimal count = 0;
            var dt = ReportBLL.QueryProductSaleDetailDaysPageList(Request.Params, ref footer, ref count);
            return ToDataGrid(dt, Convert.ToInt32(count), footer);
        }
        #endregion
        #region 进销存统计报表
        public ActionResult InvoicingSummary()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            return View();
        }
        [HttpPost]
        public ActionResult QueryInvoicingSummaryPageList()
        {
            object footer = new object();
            int count = 0;
            var dt = ReportBLL.QueryInvoicingSummaryPageList(Request.Params, ref footer, ref count);
            return ToDataGrid(dt, count, footer);
        }
        #endregion
        #region 销售员日结报表
        public ActionResult CashierSaleOrderDay()
        {
            ViewBag.cashiers = ListToSelect(SaleOrdersService.GetCashiers(), emptyTitle: "全部");
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            object footer = null;
            int count = 0;
            var dt = ReportBLL.QueryCashierSaleOrderDayPageList(Request.Params, ref footer, ref count);
            ViewBag.columns = GetDynamicCols(dt);
            return View();
        }
        [HttpPost]
        public ActionResult QueryCashierSaleOrderDayPageList()
        {
            object footer = null;
            int count = 0;
            var dt = ReportBLL.QueryCashierSaleOrderDayPageList(Request.Params, ref footer, ref count);
            Session["sales"] = dt;
            return ToDataGrid(dt, count, footer);
        }
        Dictionary<string, string> GetDynamicCols(DataTable dt)
        {
            var columns = new Dictionary<string, string>();
            if (dt != null && dt.Columns.Count > 0)
            {
                foreach (DataColumn col in dt.Columns)
                {
                    if (col.ColumnName.EndsWith("_dy"))
                        columns[col.ColumnName] = col.ColumnName.Substring(0,col.ColumnName.IndexOf("_"));
                }
            }
            return columns;
        }
        #endregion
        #region 销售员月结报表
        public ActionResult CashierSaleOrderMonth()
        {
            ViewBag.cashiers = ListToSelect(SaleOrdersService.GetCashiers(), emptyTitle: "全部");
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            object footer = null;
            int count = 0;
            var dt = ReportBLL.QueryCashierSaleOrderMonthPageList(Request.Params, ref footer, ref count);
            ViewBag.columns = GetDynamicCols(dt);
            return View();
        }
        [HttpPost]
        public ActionResult QueryCashierSaleOrderMonthPageList()
        {
            object footer = null;
            int count = 0;
            var dt = ReportBLL.QueryCashierSaleOrderMonthPageList(Request.Params, ref footer, ref count);
            Session["sales"] = dt;
            return ToDataGrid(dt, count, footer);
        }
        #endregion
        #region 导购员日结报表
        public ActionResult SalerSaleOrderDay()
        {
            ViewBag.salers = ListToSelect(SaleOrdersService.GetSalers(), emptyTitle: "全部");
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            object footer = null;
            int count = 0;
            var dt = ReportBLL.QuerySalerSaleOrderDayPageList(Request.Params, ref footer, ref count);
            ViewBag.columns = GetDynamicCols(dt);
            return View();
        }
        [HttpPost]
        public ActionResult QuerySalerSaleOrderDayPageList()
        {
            object footer = null;
            int count = 0;
            var dt = ReportBLL.QuerySalerSaleOrderDayPageList(Request.Params, ref footer, ref count);
            Session["sales"] = dt;
            return ToDataGrid(dt, count, footer);
        }
        #endregion
        #region 销售员月结报表
        public ActionResult SalerSaleOrderMonth()
        {
            ViewBag.salers = ListToSelect(SaleOrdersService.GetSalers(), emptyTitle: "全部");
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            object footer = null;
            int count = 0;
            var dt = ReportBLL.QuerySalerSaleOrderMonthPageList(Request.Params, ref footer, ref count);
            ViewBag.columns = GetDynamicCols(dt);
            return View();
        }
        [HttpPost]
        public ActionResult QuerySalerSaleOrderMonthPageList()
        {
            object footer = null;
            int count = 0;
            var dt = ReportBLL.QuerySalerSaleOrderMonthPageList(Request.Params, ref footer, ref count);
            Session["sales"] = dt;
            return ToDataGrid(dt, count, footer);
        }
        #endregion
        #region 门店日结报表
        public ActionResult StoreSaleOrderDay()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            object footer = null;
            int count = 0;
            var dt = ReportBLL.QueryStoreSaleOrderDayPageList(Request.Params, ref footer, ref count);
            ViewBag.columns = GetDynamicCols(dt);
            return View();
        }
        [HttpPost]
        public ActionResult QueryStoreSaleOrderDayPageList()
        {
            object footer = null;
            int count = 0;
            var dt = ReportBLL.QueryStoreSaleOrderDayPageList(Request.Params, ref footer, ref count);
            Session["sales"] = dt;
            return ToDataGrid(dt, count, footer);
        }
        #endregion
        #region 门店月结报表
        public ActionResult StoreSaleOrderMonth()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            object footer = null;
            int count = 0;
            var dt = ReportBLL.QueryStoreSaleOrderMonthPageList(Request.Params, ref footer, ref count);
            ViewBag.columns = GetDynamicCols(dt);
            return View();
        }
        [HttpPost]
        public ActionResult QueryStoreSaleOrderMonthPageList()
        {
            object footer = null;
            int count = 0;
            var dt = ReportBLL.QueryStoreSaleOrderMonthPageList(Request.Params, ref footer, ref count);
            Session["sales"] = dt;
            return ToDataGrid(dt, count, footer);
        }
        #endregion
        #region 供应商销售明细月报表
        public ActionResult SupplierSaleDetail()
        {
            ViewBag.suppliers = ListToSelect(SupplierService.GetList().Where(o => o.BusinessType == 1).Select(o => new SelectListItem() { Value = o.Id.ToString(), Text = o.Title }), emptyTitle: "全部");
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            return View();
        }
        [HttpPost]
        public ActionResult QuerySupplierSaleDetailPageList()
        {
            object footer = null;
            var dt = ReportBLL.QuerySupplierSaleDetailPageList(Request.Params, ref footer);
            Session["sales"] = dt;
            return ToDataGrid(dt, 0, footer);
        }
        #endregion
        #region 商品销售明细日报表
        public ActionResult ProductSaleDetailDay()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            return View();
        }
        [HttpPost]
        public ActionResult QueryProductSaleDetailDayPageList()
        {
            object footer = new object();
            var dt = ReportBLL.QueryProductSaleDetailDayPageList(Request.Params, ref footer);
            Session["sales"] = dt;
            return ToDataGrid(dt, 0, footer);
        }
        #endregion
        #region 商品销售明细月报表
        public ActionResult ProductSaleDetail()
        {
            ViewBag.parenttypes = ListToSelect(ProductCategoryService.GetParentTypes().Select(o => new SelectListItem() { Value = o.CategorySN.ToString(), Text = o.Title }), emptyTitle: "全部");
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            return View();
        }
        [HttpPost]
        public ActionResult QueryProductSaleDetailPageList()
        {
            object footer = new object();
            var dt = ReportBLL.QueryProductSaleDetailPageList(Request.Params, ref footer);
            Session["sales"] = dt;
            return ToDataGrid(dt, 0, footer);
        }
        #endregion
        #region 品牌销售明细月报表
        public ActionResult BrandSaleDetail()
        {
            ViewBag.brandsns = ListToSelect(ProductBrandService.GetList(true).Select(o => new SelectListItem() { Value = o.BrandSN.ToString(), Text = o.Title }), emptyTitle: "全部");
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            return View();
        }
        [HttpPost]
        public ActionResult QueryBrandSaleDetailPageList()
        {
            object footer = new object();
            var dt = ReportBLL.QueryBrandSaleDetailPageList(Request.Params, ref footer);
            Session["sales"] = dt;
            return ToDataGrid(dt, 0, footer);
        }
        #endregion
        #region 特价销售明细月报表
        public ActionResult PromotionSaleDetail()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            ViewBag.parenttypes = ListToSelect(ProductCategoryService.GetParentTypes().Select(o => new SelectListItem() { Value = o.CategorySN.ToString(), Text = o.Title }), emptyTitle: "全部");
            ViewBag.suppliers = ListToSelect(SupplierService.GetList().Where(o => o.BusinessType == 1).Select(o => new SelectListItem() { Value = o.Id.ToString(), Text = o.Title }), emptyTitle: "全部");
            return View();
        }
        [HttpPost]
        public ActionResult QueryPromotionSaleDetailPageList()
        {
            object footer = new object();
            var dt = ReportBLL.QueryPromotionSaleDetailPageList(Request.Params, ref footer);
            Session["sales"] = dt;
            return ToDataGrid(dt, 0, footer);
        }
        #endregion
        #region 导出
        public ActionResult SaleExport(int? type, string date, string date2)
        {
            var dt = Session["sales"] as System.Data.DataTable;
            List<string> fields = new List<string>() { "Barcode", "Title", "CategoryTitle", "SubUnit", "SysPrice", "ActualPrice", "PurchaseNumber", "SaleTotal", "BuyTotal", "MLL", "MLE" };
            List<string> names = new List<string>() { "商品条码", "商品名称", "品类", "单位", "系统均价", "实际均价", "销售数量", "销售金额", "进价金额", "毛利率", "毛利额" };
            var totalCols = new int[4];
            var mergerCols = new int[0];
            string title = DateTime.Parse(date).ToString("yyyy年MM月");
            object footer = null;
            string fileName = "统计报表";
            if (type == 1)
            {
                title += " 商品销售明细";
                fileName = "商品销售明细";
                totalCols = new int[] { 5, 6, 7, 9 };
                if (dt == null) dt = ReportBLL.QueryProductSaleDetailPageList(Request.Params, ref footer);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("ProductSaleDetail", "暂无数据可导出！");
            }
            else if (type == 2)
            {
                totalCols = new int[] { 6, 7, 8, 10 };
                fields.Insert(2, "BrandTitle");
                names.Insert(2, "品牌");
                title += " 品牌销售明细";
                fileName = "品牌销售明细";
                if (dt == null) dt = ReportBLL.QueryBrandSaleDetailPageList(Request.Params, ref footer);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("BrandSaleDetail", "暂无数据可导出！");
            }
            else if (type == 3)
            {
                totalCols = new int[] { 6, 7, 8, 10 };
                fields.Insert(2, "SupplierTitle");
                names.Insert(2, "供应商");
                title += " 供应商销售明细";
                fileName = "供应商销售明细";
                if (dt == null) dt = ReportBLL.QuerySupplierSaleDetailPageList(Request.Params, ref footer);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("SupplierSaleDetail", "暂无数据可导出！");
            }
            else if (type == 4)
            {
                fileName = "特价销售明细";
                totalCols = new int[] { 9, 10, 11, 13 };
                fields = new List<string>() { "Barcode", "Title", "SupplierTitle", "StoreTitle", "BrandTitle", "CategoryTitle", "SubUnit", "SysPrice", "DiscountPrice", "PurchaseNumber", "SaleTotal", "BuyTotal", "MLL", "MLE", "ZB" };
                names = new List<string>() { "商品条码", "商品名称", "供应商", "门店", "品牌", "品类", "单位", "系统均价", "促销均价", "销售数量", "销售金额", "进价金额", "毛利率", "毛利额", "销售占比" };
                title += " 特价销售明细";
                if (dt == null) dt = ReportBLL.QueryPromotionSaleDetailPageList(Request.Params, ref footer);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("PromotionSaleDetail", "暂无数据可导出！");
            }
            else if (type == 5)
            {
                fileName = "商品销售明细";
                totalCols = new int[] { 5, 6, 7, 9 };
                fields = new List<string>() { "Barcode", "Title", "CategoryTitle", "SubUnit", "SysPrice", "ActualPrice", "PurchaseNumber", "SaleTotal", "BuyTotal", "MLL", "MLE" };
                names = new List<string>() { "商品条码", "商品名称", "大类/中类", "单位", "系统均价", "实际均价", "销售数量", "销售金额", "进价金额", "毛利率", "毛利额" };
                title = DateTime.Parse(date).ToString("yyyy年MM月dd日") + ((date2.IsNullOrEmpty() || date == date2) ? "" : "至" + DateTime.Parse(date2).ToString("yyyy年MM月dd日")) + " 商品销售明细";
                if (dt == null) dt = ReportBLL.QueryPromotionSaleDetailPageList(Request.Params, ref footer);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("ProductSaleDetailDay", "暂无数据可导出！");
            }
            else if (type == 6)
            {
                fileName = "收银员日结报表";
                totalCols = new int[] { 4,5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19,20,21,22,23 };
                fields = new List<string>() { "Date", "Cashier", "FirstTime", "LastTime", "SSCount", "SSMoney", "XSCount", "XSMoney", "HUANCount", "HUANMoney", "TUIHuanCount", "TUIHuanMoney", "TUICount", "TUIMoney", "CZCount", "CZMoney", "FCZCount", "FCZMoney", "ZSCount", "ZSMoney", "RLCount", "RLMoney", "MLCount", "MLMoney" };
                names = new List<string>() { "日结时间", "收银员", "首笔时间", "末笔时间", "合计笔数", "合计金额", "销售笔数", "销售金额", "换货入款笔数", "换货入款", "退换出款笔数", "退换出款", "退单笔数", "退单金额", "充值笔数", "充值金额", "反结算笔数", "反结算金额", "赠送笔数", "赠送金额", "整单让利笔数", "整单让利金额", "抹零笔数", "抹零金额" };
                title = DateTime.Parse(date).ToString("yyyy年MM月dd日") + ((date2.IsNullOrEmpty() || date == date2) ? "" : "至" + DateTime.Parse(date2).ToString("yyyy年MM月dd日")) + " 收银员日结报表";
                var nvl = new System.Collections.Specialized.NameValueCollection() { Request.Params };
                nvl["ispage"] = "0";
                int count = 0;
                dt = ReportBLL.QueryCashierSaleOrderDayPageList(Request.Params, ref footer, ref count);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("OrderSaleDetailDay", "暂无数据可导出！");
            }
            else if (type == 7)
            {
                fileName = "商品销售明细汇总表";
                totalCols = new int[] { 5,6,9,10,11, 12, 13, 14,15,16, 18 };
                fields = new List<string>() { "Barcode", "Title", "SupplierTitle", "BrandTitle", "SubUnit", "PurchaseNumber", "SaleTotal", "SysPrice", "ActualPrice", "HuanNumber", "HuanTotal", "ReturnHuangNumber", "ReturnHuangTotal", "ReturnNumber", "ReturnTotal", "GiftNumber", "GiftTotal", "MLL", "MLE" };
                names = new List<string>() { "商品条码", "商品名称", "主供应商", "品牌", "单位", "销售数量", "销售金额", "系统均价", "实际均价", "换货入款数量", "换货入款", "退换出款数量", "退换出款", "退单数量", "退单金额", "赠送数量", "赠送金额", "毛利率", "毛利额" };
                title = DateTime.Parse(date).ToString("yyyy年MM月dd日") + ((date2.IsNullOrEmpty() || date == date2) ? "" : "至" + DateTime.Parse(date2).ToString("yyyy年MM月dd日")) + " 商品销售明细汇总表";
                var nvl = new System.Collections.Specialized.NameValueCollection() { Request.Params };
                nvl["ispage"] = "0";
                decimal total = 0;
                dt = ReportBLL.QueryProductSaleDetailsPageList(nvl, ref footer, ref total);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("ProductSaleDetails", "暂无数据可导出！");
            }
            else if (type == 8)
            {
                fileName = "进销存统计";
                totalCols = null;
                title = DateTime.Parse(date).ToString("yyyy年MM月dd日") + ((date2.IsNullOrEmpty() || date == date2) ? "" : "至" + DateTime.Parse(date2).ToString("yyyy年MM月dd日")) + " 进销存统计报表";
                var nvl = new System.Collections.Specialized.NameValueCollection() { Request.Params };
                nvl["ispage"] = "0";
                int total = 0;
                dt = ReportBLL.QueryInvoicingSummaryPageList(nvl, ref footer, ref total);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("InvoicingSummary", "暂无数据可导出！");
                fields = new List<string>();
                names = new List<string>();
                dt.Columns.Remove("RSNO");
                dt.Columns.Remove("RecordTotal");
                dt.Columns.Remove("RecordStart");
                dt.Columns.Remove("RecordEnd");
                dt.Columns.Remove("StockRate");
                dt.Columns.Remove("BuyPrice");
                dt.Columns.MoveTo("期初金额", 7, ref dt);

                foreach (DataColumn col in dt.Columns)
                    if (!col.ColumnName.EndsWith("s"))
                    {
                        string columnName = col.ColumnName;
                        fields.Add(columnName);
                        names.Add(columnName.StartsWith("组合") && columnName.EndsWith("减") ? columnName + "(-)" : columnName.StartsWith("组合") && columnName.EndsWith("加") ? columnName + "(+)" : columnName.StartsWith("拆分") && columnName.EndsWith("减") ? columnName + "(-)" : columnName.StartsWith("拆分") && columnName.EndsWith("加") ? columnName + "(+)" : columnName);
                    }
            }
            else if (type == 9)
            {
                fileName = "门店日结报表";
                totalCols = new int[] { 5, 6, 7, 8, 9, 10, 11, 12, 13, 14,15,16,17,18,19,20,21,22,23,24 };
                fields = new List<string>() { "Date", "Store", "FirstTime", "LastTime", "SaleOrderAverage", "SSCount", "SSMoney", "XSCount", "XSMoney", "HUANCount", "HUANMoney", "TUIHuanCount", "TUIHuanMoney", "TUICount", "TUIMoney", "CZCount", "CZMoney", "FCZCount", "FCZMoney", "ZSCount", "ZSMoney", "RLCount", "RLMoney", "MLCount", "MLMoney" };
                names = new List<string>() { "日结时间", "门店", "首笔时间", "末笔时间", "客单价", "合计笔数", "合计金额", "销售笔数", "销售金额", "换货入款笔数", "换货入款", "退换出款笔数", "退换出款", "退单笔数", "退单金额", "充值笔数", "充值金额", "反结算笔数", "反结算金额", "赠送笔数", "赠送金额", "整单让利笔数", "整单让利金额", "抹零笔数", "抹零金额" };
                title = DateTime.Parse(date).ToString("yyyy年MM月dd日") + ((date2.IsNullOrEmpty() || date == date2) ? "" : "至" + DateTime.Parse(date2).ToString("yyyy年MM月dd日")) + " 门店日结报表";
                var nvl = new System.Collections.Specialized.NameValueCollection() { Request.Params };
                nvl["ispage"] = "0";
                int count = 0;
                dt = ReportBLL.QueryStoreSaleOrderDayPageList(Request.Params, ref footer, ref count);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("StoreSaleOrderDay", "暂无数据可导出！");
            }
            else if (type == 10)
            {
                fileName = "门店月结报表";
                totalCols = new int[] { 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22,23,24 };
                fields = new List<string>() { "Date", "Store", "FirstTime", "LastTime", "SaleOrderAverage", "SSCount", "SSMoney", "XSCount", "XSMoney", "HUANCount", "HUANMoney", "TUIHuanCount", "TUIHuanMoney", "TUICount", "TUIMoney", "CZCount", "CZMoney", "FCZCount", "FCZMoney", "ZSCount", "ZSMoney", "RLCount", "RLMoney", "MLCount", "MLMoney" };
                names = new List<string>() { "月结时间", "门店", "首笔时间", "末笔时间", "客单价", "合计笔数", "合计金额", "销售笔数", "销售金额", "换货入款笔数", "换货入款", "退换出款笔数", "退换出款", "退单笔数", "退单金额", "充值笔数", "充值金额", "反结算笔数", "反结算金额", "赠送笔数", "赠送金额", "整单让利笔数", "整单让利金额", "抹零笔数", "抹零金额" };
                title = DateTime.Parse(date).ToString("yyyy年MM月") + ((date2.IsNullOrEmpty() || date == date2) ? "" : "至" + DateTime.Parse(date2).ToString("yyyy年MM月")) + " 门店月结报表";
                var nvl = new System.Collections.Specialized.NameValueCollection() { Request.Params };
                nvl["ispage"] = "0";
                int count = 0;
                dt = ReportBLL.QueryStoreSaleOrderMonthPageList(Request.Params, ref footer, ref count);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("StoreSaleOrderMonth", "暂无数据可导出！");
            }
            else if (type == 11)
            {
                fileName = "收银员月结报表";
                totalCols = new int[] { 4,5, 6, 7, 8, 9, 10, 11, 12, 13, 14,15,16,17,18,19,20,21,22,23 };
                fields = new List<string>() { "Date", "Cashier", "FirstTime", "LastTime", "SSCount", "SSMoney", "XSCount", "XSMoney", "HUANCount", "HUANMoney", "TUIHuanCount", "TUIHuanMoney", "TUICount", "TUIMoney", "CZCount", "CZMoney", "FCZCount", "FCZMoney", "ZSCount", "ZSMoney", "RLCount", "RLMoney", "MLCount", "MLMoney" };
                names = new List<string>() { "月结时间", "收银员", "首笔时间", "末笔时间", "合计笔数", "合计金额", "销售笔数", "销售金额", "换货入款笔数", "换货入款", "退换出款笔数", "退换出款", "退单笔数", "退单金额", "充值笔数", "充值金额", "反结算笔数", "反结算金额", "赠送笔数", "赠送金额", "整单让利金额", "抹零笔数", "抹零笔数", "抹零金额" };
                title = DateTime.Parse(date).ToString("yyyy年MM月") + ((date2.IsNullOrEmpty() || date == date2) ? "" : "至" + DateTime.Parse(date2).ToString("yyyy年MM月")) + " 收银员月结报表";
                var nvl = new System.Collections.Specialized.NameValueCollection() { Request.Params };
                nvl["ispage"] = "0";
                int count = 0;
                dt = ReportBLL.QueryCashierSaleOrderMonthPageList(Request.Params, ref footer, ref count);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("OrderSaleDetailMonth", "暂无数据可导出！");
            }
            else if (type == 12)
            {
                fileName = "商品采购明细报表";
                totalCols = new int[] { 7,8,10,11,13};
                mergerCols = new int[] { 1, 0, 1, 2 };
                fields = new List<string>() { "SupplierTitle", "InboundGoodsId", "OperatDT", "ProductCode", "Barcode", "Title", "SubUnit", "数量", "赠品数量", "单价", "采购金额", "零售金额", "未税单价", "未税金额", "StoreTitle" };
                names = new List<string>() { "主供应商", "单据编号", "操作日期", "商品编码", "商品条码", "商品名称", "单位", "进货数量", "赠品数量", "均价", "采购金额", "零售金额", "未税单价", "未税金额", "门店" };
                title = DateTime.Parse(date).ToString("yyyy年MM月dd日") + ((date2.IsNullOrEmpty() || date == date2) ? "" : "至" + DateTime.Parse(date2).ToString("yyyy年MM月dd日")) + " 商品采购明细报表";
                var nvl = new System.Collections.Specialized.NameValueCollection() { Request.Params };
                nvl["ispage"] = "0";
                int count = 0;
                dt = ReportBLL.ProductOrderDetailPageList(nvl, ref footer, ref count);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("ProductOrderDetail", "暂无数据可导出！");
            }
            else if (type == 13)
            {
                fileName = "商品库存汇总报表";
                totalCols = new int[] { 7,9,10, 11 };
                fields = new List<string>() { "CategoryTitle", "ProductCode", "Barcode", "Title", "SubUnit", "StoreTitle", "BalanceDate", "StockNumber", "平均成本价", "售价金额", "成本金额含", "成本金额未", "当前零售价", "SupplierTitle" };
                names = new List<string>() { "品类", "商品编码", "商品条码", "商品名称", "单位", "门店", "结余日期", "库存数量", "平均成本价", "售价金额", "成本金额(含)", "成本金额(未)", "当前零售价", "供应商" };
                title = DateTime.Parse(date).ToString("yyyy年MM月dd日") + ((date2.IsNullOrEmpty() || date == date2) ? "" : "至" + DateTime.Parse(date2).ToString("yyyy年MM月dd日"))+ " 商品库存汇总报表";
                int count = 0;
                var nvl = new System.Collections.Specialized.NameValueCollection() { Request.Params };
                nvl["ispage"] = "0";
                if (dt == null) dt = ReportBLL.StoreStockDetailPageList(nvl, ref footer, ref count);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("StoreStockDetail", "暂无数据可导出！");
            }
            else if (type == 14)
            {
                fileName = "商品销售日明细表";
                totalCols = new int[] { 7, 8, 11,12,13,14,15,16,17,18,20 };
                fields = new List<string>() { "ProductCode", "Barcode", "Title", "SupplierTitle", "BrandTitle", "SubUnit", "CreateDT", "PurchaseNumber", "SaleTotal", "SysPrice", "ActualPrice", "HuanNumber", "HuanTotal", "TuiHuanNumber", "TuiHuanTotal", "ReturnNumber", "ReturnTotal", "GiftNumber", "GiftTotal", "MLL", "MLE" };
                names = new List<string>() { "商品编码", "商品条码", "商品名称", "主供应商", "品牌", "单位", "销售日期", "销售数量", "销售金额", "系统均价", "实际均价", "换货入款数量", "换货入款", "退换出款数量", "退换出款", "退单数量", "退单金额", "赠送数量", "赠送金额", "毛利率", "毛利额" };
                title = DateTime.Parse(date).ToString("yyyy年MM月dd日") + ((date2.IsNullOrEmpty() || date == date2) ? "" : "至" + DateTime.Parse(date2).ToString("yyyy年MM月dd日")) + " 商品销售明细表";
                var nvl = new System.Collections.Specialized.NameValueCollection() { Request.Params };
                nvl["ispage"] = "0";
                decimal total = 0;
                dt = ReportBLL.QueryProductSaleDetailDaysPageList(nvl, ref footer, ref total);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("ProductSaleDetailDays", "暂无数据可导出！");
            }
            else if (type == 15)
            {
                fileName = "商品采购汇总报表";
                totalCols = new int[] { 6,7, 8, 10 };
                fields = new List<string>() { "SupplierTitle", "StoreTitle", "ProductCode", "Barcode", "Title", "SubUnit", "数量", "赠品数量", "采购金额", "零售金额", "未税单价", "未税金额" };
                names = new List<string>() { "主供应商", "门店", "商品编码", "商品条码", "商品名称", "单位", "进货数量", "赠品数量", "采购金额", "零售金额", "未税单价", "未税金额" };
                title = DateTime.Parse(date).ToString("yyyy年MM月dd日") + ((date2.IsNullOrEmpty() || date == date2) ? "" : "至" + DateTime.Parse(date2).ToString("yyyy年MM月dd日")) + " 商品采购汇总报表";
                var nvl = new System.Collections.Specialized.NameValueCollection() { Request.Params };
                nvl["ispage"] = "0";
                int count = 0;
                dt = ReportBLL.ProductOrderDetailPageList(nvl, ref footer, ref count);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("ProductOrderSummary", "暂无数据可导出！");
            }
            else if (type == 16)
            {
                fileName = "前台销售商品门店汇总报表";
                totalCols = new int[] { 6, 7, 8,9, 10,11,12,13,14,15,17 };
                fields = new List<string>() { "StoreId", "StoreTitle", "CategoryTitle", "ProductCode", "Barcode", "Title", "PurchaseNumber", "SaleTotal",  "HuanNumber", "HuanTotal", "TuiHuanNumber", "TuiHuanTotal", "ReturnNumber", "ReturnTotal", "GiftNumber", "GiftTotal", "MLL", "MLE" };
                names = new List<string>() { "门店号", "门店名称", "类别", "商品编码", "商品条码", "商品名称","销售数量", "销售金额", "换货入款数量", "换货入款", "退换出款数量", "退换出款", "退单数量", "退单金额", "赠送数量", "赠送金额", "毛利率", "毛利额" };
                title = DateTime.Parse(date).ToString("yyyy年MM月dd日") + ((date2.IsNullOrEmpty() || date == date2) ? "" : "至" + DateTime.Parse(date2).ToString("yyyy年MM月dd日")) + " 前台销售商品门店汇总报表";
                var nvl = new System.Collections.Specialized.NameValueCollection() { Request.Params };
                nvl["ispage"] = "0";
                int count = 0;
                dt = ReportBLL.BeforeSaleSummaryPageList(nvl, ref footer, ref count);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("BeforeSaleSummary", "暂无数据可导出！");
            }
            else if (type == 17)
            {
                fileName = "前台销售商品门店明细报表";
                totalCols = new int[] { 7, 8, 9, 10, 11, 12, 13, 14, 15,16,18 };
                fields = new List<string>() { "StoreId", "StoreTitle", "CategoryTitle", "ProductCode", "Barcode", "Title", "CreateDT", "PurchaseNumber", "SaleTotal", "HuanNumber", "HuanTotal", "TuiHuanNumber", "TuiHuanTotal", "ReturnNumber", "ReturnTotal", "GiftNumber", "GiftTotal", "MLL", "MLE" };
                names = new List<string>() { "门店号", "门店名称", "类别", "商品编码", "商品条码", "商品名称", "销售日期", "销售数量", "销售金额", "换货入款数量", "换货入款", "退换出款数量", "退换出款", "退单数量", "退单金额", "赠送数量", "赠送金额", "毛利率", "毛利额" };
                title = DateTime.Parse(date).ToString("yyyy年MM月dd日") + ((date2.IsNullOrEmpty() || date == date2) ? "" : "至" + DateTime.Parse(date2).ToString("yyyy年MM月dd日")) + " 前台销售商品门店明细报表";
                var nvl = new System.Collections.Specialized.NameValueCollection() { Request.Params };
                nvl["ispage"] = "0";
                int count = 0;
                dt = ReportBLL.BeforeSaleSummaryPageList(nvl, ref footer, ref count);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("BeforeSaleSummary", "暂无数据可导出！");
            }
            else if (type == 18)
            {
                fileName = "其它出库汇总报表";
                totalCols = new int[] { 5,6,7, 8};
                fields = new List<string>() { "StoreId", "StoreTitle", "Barcode", "Title", "SubUnit", "OutboundNumber", "出库金额", "SaleTotal", "未税金额"};
                names = new List<string>() { "门店号", "门店名称", "商品条码", "商品名称", "单位", "出库数量", "金额", "零售金额", "未税金额" };
                title = DateTime.Parse(date).ToString("yyyy年MM月dd日") + ((date2.IsNullOrEmpty() || date == date2) ? "" : "至" + DateTime.Parse(date2).ToString("yyyy年MM月dd日")) + " 其它出库汇总报表";
                var nvl = new System.Collections.Specialized.NameValueCollection() { Request.Params };
                nvl["ispage"] = "0";
                int count = 0;
                dt = ReportBLL.OutboundPageList(nvl, ref footer, ref count);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("OutBoundSummary", "暂无数据可导出！");
            }
            else if (type == 19)
            {
                fileName = "其它出库明细报表";
                totalCols = new int[] { 6, 7, 8,9 };
                fields = new List<string>() { "StoreId", "StoreTitle", "Barcode", "Title", "SubUnit", "VerifyTime", "OutboundNumber", "出库金额", "SaleTotal", "未税金额", "Operator" };
                names = new List<string>() { "门店号", "门店名称", "商品条码", "商品名称", "单位", "操作日期", "出库数量", "金额", "零售金额", "未税金额", "操作员" };
                title = DateTime.Parse(date).ToString("yyyy年MM月dd日") + ((date2.IsNullOrEmpty() || date == date2) ? "" : "至" + DateTime.Parse(date2).ToString("yyyy年MM月dd日")) + " 其它出库明细报表";
                var nvl = new System.Collections.Specialized.NameValueCollection() { Request.Params };
                nvl["ispage"] = "0";
                int count = 0;
                dt = ReportBLL.OutboundPageList(nvl, ref footer, ref count);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("OutBoundSummary", "暂无数据可导出！");
            }
            else if (type == 20)
            {
                fileName = "调拨明细报表";
                totalCols = new int[] { 8, 11,12,13 };
                fields = new List<string>() { "OutStoreTitle", "InStoreTitle", "CategoryTitle", "ProductCode", "Barcode", "Title", "SubUnit", "ActualDT", "ActualQuantity", "调价", "未税调价", "调价金额", "未税金额", "税额", "Opertaor" };
                names = new List<string>() { "调出门店", "调入门店", "类别", "商品编码", "商品条码", "商品名称", "单位", "操作日期", "数量", "调价", "调价(未)", "调价金额(含)", "调价金额(未)", "税额", "操作员" };
                title = DateTime.Parse(date).ToString("yyyy年MM月dd日") + ((date2.IsNullOrEmpty() || date == date2) ? "" : "至" + DateTime.Parse(date2).ToString("yyyy年MM月dd日")) + " 调拨明细报表";
                var nvl = new System.Collections.Specialized.NameValueCollection() { Request.Params };
                nvl["ispage"] = "0";
                int count = 0;
                dt = ReportBLL.HouseMovePageList(nvl, ref footer, ref count);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("OutBoundSummary", "暂无数据可导出！");
            }
            else if (type == 21)
            {
                fileName = "调拨汇总报表";
                totalCols = new int[] { 7, 8, 9,10 };
                fields = new List<string>() { "OutStoreTitle", "InStoreTitle", "CategoryTitle", "ProductCode", "Barcode", "Title", "SubUnit", "ActualQuantity", "调价金额", "未税金额", "税额" };
                names = new List<string>() { "调出门店", "调入门店", "类别", "商品编码", "商品条码", "商品名称", "单位", "数量", "调价金额(含)", "调价金额(未)", "税额" };
                title = DateTime.Parse(date).ToString("yyyy年MM月dd日") + ((date2.IsNullOrEmpty() || date == date2) ? "" : "至" + DateTime.Parse(date2).ToString("yyyy年MM月dd日")) + " 调拨汇总报表";
                var nvl = new System.Collections.Specialized.NameValueCollection() { Request.Params };
                nvl["ispage"] = "0";
                int count = 0;
                dt = ReportBLL.HouseMovePageList(nvl, ref footer, ref count);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("OutBoundSummary", "暂无数据可导出！");
            }
            else if (type == 22)
            {
                fileName = "批发汇总报表";
                totalCols = new int[] { 1, 2, 3, 4 };
                fields = new List<string>() { "ApplyOrg", "OutboundNumber", "批发金额", "SaleTotal", "未税金额" };
                names = new List<string>() { "批发商名称", "批发数量", "批发金额", "零售金额", "批发金额(未)"};
                title = DateTime.Parse(date).ToString("yyyy年MM月dd日") + ((date2.IsNullOrEmpty() || date == date2) ? "" : "至" + DateTime.Parse(date2).ToString("yyyy年MM月dd日")) + " 批发汇总报表";
                var nvl = new System.Collections.Specialized.NameValueCollection() { Request.Params };
                nvl["ispage"] = "0";
                int count = 0;
                dt = ReportBLL.WholesalPageList(nvl, ref footer, ref count);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("WholesalSummary", "暂无数据可导出！");
            }
            else if (type == 23)
            {
                fileName = "批发明细报表";
                totalCols = new int[] { 5, 6, 7, 8 };
                fields = new List<string>() { "ApplyOrg", "Barcode", "Title", "SubUnit", "VerifyTime", "OutboundNumber", "批发金额", "SaleTotal", "未税金额", "Operator" };
                names = new List<string>() { "批发商名称", "商品条码", "商品名称", "单位", "操作日期", "批发数量", "批发金额", "零售金额", "批发金额(未)", "操作人" };
                title = DateTime.Parse(date).ToString("yyyy年MM月dd日") + ((date2.IsNullOrEmpty() || date == date2) ? "" : "至" + DateTime.Parse(date2).ToString("yyyy年MM月dd日")) + " 批发明细报表";
                var nvl = new System.Collections.Specialized.NameValueCollection() { Request.Params };
                nvl["ispage"] = "0";
                int count = 0;
                dt = ReportBLL.WholesalPageList(nvl, ref footer, ref count);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("WholesalSummary", "暂无数据可导出！");
            }
            else if (type == 24)
            {
                fileName = "损耗汇总报表";
                totalCols = new int[] { 6, 7, 8,9 };
                fields = new List<string>() { "StoreId", "StoreTitle", "CategoryTitle", "Barcode", "Title", "SubUnit", "BreakageNumber", "BreakageTotal", "SaleTotal", "未税金额" };
                names = new List<string>() { "门店号", "门店名称", "类别", "条码", "商品名称", "单位", "报损数量", "报损金额", "售价金额", "未税金额" };
                title = DateTime.Parse(date).ToString("yyyy年MM月dd日") + ((date2.IsNullOrEmpty() || date == date2) ? "" : "至" + DateTime.Parse(date2).ToString("yyyy年MM月dd日")) + " 损耗汇总报表";
                var nvl = new System.Collections.Specialized.NameValueCollection() { Request.Params };
                nvl["ispage"] = "0";
                int count = 0;
                dt = ReportBLL.BreakagePageList(nvl, ref footer, ref count);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("WholesalSummary", "暂无数据可导出！");
            }
            else if (type == 25)
            {
                fileName = "损耗明细报表";
                totalCols = new int[] { 8,9,10,11 };
                fields = new List<string>() { "StoreId", "StoreTitle", "CategoryTitle", "Barcode", "Title", "SubUnit", "CreateDT", "Receipt", "BreakageNumber", "BreakageTotal", "SaleTotal", "未税金额" };
                names = new List<string>() { "门店号", "门店名称", "类别", "条码", "商品名称", "单位", "操作日期", "单据编号", "报损数量", "报损金额", "售价金额", "未税金额" };
                title = DateTime.Parse(date).ToString("yyyy年MM月dd日") + ((date2.IsNullOrEmpty() || date == date2) ? "" : "至" + DateTime.Parse(date2).ToString("yyyy年MM月dd日")) + " 损耗明细报表";
                var nvl = new System.Collections.Specialized.NameValueCollection() { Request.Params };
                nvl["ispage"] = "0";
                int count = 0;
                dt = ReportBLL.BreakagePageList(nvl, ref footer, ref count);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("WholesalSummary", "暂无数据可导出！");
            }
            else if (type == 26)
            {
                fileName = "导购员日结报表";
                totalCols = new int[] { 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 };
                fields = new List<string>() { "Date", "Salesman", "FirstTime", "LastTime", "SSCount", "SSMoney", "XSCount", "XSMoney", "HUANCount", "HUANMoney", "TUIHuanCount", "TUIHuanMoney", "TUICount", "TUIMoney", "ZSCount", "ZSMoney", "RLCount", "RLMoney", "MLCount", "MLMoney" };
                names = new List<string>() { "日结时间", "导购员", "首笔时间", "末笔时间", "合计笔数", "合计金额", "销售笔数", "销售金额", "换货入款笔数", "换货入款", "退换出款笔数", "退换出款", "退单笔数", "退单金额", "赠送笔数", "赠送金额", "整单让利笔数", "整单让利金额", "抹零笔数", "抹零金额" };
                title = DateTime.Parse(date).ToString("yyyy年MM月dd日") + ((date2.IsNullOrEmpty() || date == date2) ? "" : "至" + DateTime.Parse(date2).ToString("yyyy年MM月dd日")) + " 导购员日结报表";
                var nvl = new System.Collections.Specialized.NameValueCollection() { Request.Params };
                nvl["ispage"] = "0";
                int count = 0;
                dt = ReportBLL.QuerySalerSaleOrderDayPageList(Request.Params, ref footer, ref count);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("OrderSaleDetailDay", "暂无数据可导出！");
            }
            else if (type == 27)
            {
                fileName = "导购员月结报表";
                totalCols = new int[] { 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 };
                fields = new List<string>() { "Date", "Salesman", "FirstTime", "LastTime", "SSCount", "SSMoney", "XSCount", "XSMoney", "HUANCount", "HUANMoney", "TUIHuanCount", "TUIHuanMoney", "TUICount", "TUIMoney", "ZSCount", "ZSMoney", "RLCount", "RLMoney", "MLCount", "MLMoney" };
                names = new List<string>() { "月结时间", "导购员", "首笔时间", "末笔时间", "合计笔数", "合计金额", "销售笔数", "销售金额", "换货入款笔数", "换货入款", "退换出款笔数", "退换出款", "退单笔数", "退单金额", "赠送笔数", "赠送金额", "整单让利金额", "抹零笔数", "抹零笔数", "抹零金额" };
                title = DateTime.Parse(date).ToString("yyyy年MM月") + ((date2.IsNullOrEmpty() || date == date2) ? "" : "至" + DateTime.Parse(date2).ToString("yyyy年MM月")) + " 导购员月结报表";
                var nvl = new System.Collections.Specialized.NameValueCollection() { Request.Params };
                nvl["ispage"] = "0";
                int count = 0;
                dt = ReportBLL.QuerySalerSaleOrderMonthPageList(Request.Params, ref footer, ref count);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("OrderSaleDetailMonth", "暂无数据可导出！");
            }
            new ExportExcel() { IsBufferOutput = true, HeaderText = title }.ToExcel(fileName, dt, fields.ToArray(), names.ToArray(), mergerCols, totalCols);
            return new EmptyResult();
        }
        public ActionResult SaleStatisticsExport(int? type, string date)
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            List<string> fields = new List<string>() { "SaleTotal", "BuyTotal", "MLL", "MLE", "Percent" };
            List<string> names = new List<string>() { "销售金额", "进价金额", "毛利率", "毛利额", "销售占比" };
            var totalCols = new int[] { 1, 2, 4 };
            string title = DateTime.Parse(date).ToString("yyyy年MM月");
            object footer = null;
            if (type == 1)
            {
                title += " 供应商汇总";
                fields.Insert(0, "SupplierTitle");
                names.Insert(0, "供应商");
                dt = ReportBLL.QuerySupplierStatisticsPageList(Request.Params, ref footer);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("SupplierStatistics", "暂无数据可导出！");
            }
            else if (type == 2)
            {
                title += " 大类销售汇总";
                fields.Insert(0, "BigCategoryTitle");
                names.Insert(0, "大类");
                dt = ReportBLL.QueryBigCategoryStatisticsPageList(Request.Params, ref footer);
                if (dt.Rows.Count <= 0)
                    return RedirectAlert("BigCategoryStatistics", "暂无数据可导出！");
            }
            new ExportExcel() { IsBufferOutput = true, HeaderText = title }.ToExcel(DateTime.Now.ToString("yyyyMMdd"), dt, fields.ToArray(), names.ToArray(), null, totalCols);
            return new EmptyResult();
        }
        #endregion
        #region 供应商汇总月报
        public ActionResult SupplierStatistics()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            return View();
        }
        [HttpPost]
        public ActionResult QuerySupplierStatisticsPageList()
        {
            object footer = new object();
            var dt = ReportBLL.QuerySupplierStatisticsPageList(Request.Params, ref footer);
            return ToDataGrid(dt, 0, footer);
        }
        #endregion
        #region 大类销售汇总月报
        public ActionResult BigCategoryStatistics()
        {
            ViewBag.parenttypes = ListToSelect(ProductCategoryService.GetParentTypes().Select(o => new SelectListItem() { Value = o.CategorySN.ToString(), Text = o.Title }), emptyTitle: "全部");
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            return View();
        }
        [HttpPost]
        public ActionResult QueryBigCategoryStatisticsPageList()
        {
            object footer = new object();
            var dt = ReportBLL.QueryBigCategoryStatisticsPageList(Request.Params, ref footer);
            return ToDataGrid(dt, dt.Rows.Count, footer);
        }
        #endregion
        #region 特价商品销售明细
        public ActionResult CommodityPromotionSaleDetail()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title }), emptyTitle: "全部");
            ViewBag.parenttypes = ListToSelect(ProductCategoryService.GetParentTypes().Select(o => new SelectListItem() { Value = o.CategorySN.ToString(), Text = o.Title }), emptyTitle: "全部");
            ViewBag.suppliers = ListToSelect(SupplierService.GetList().Where(o => o.BusinessType == 1).Select(o => new SelectListItem() { Value = o.Id.ToString(), Text = o.Title }), emptyTitle: "全部");
            return View();
        }
        public ActionResult QueryCommodityPromotionSaleDetailPageList()
        {
            return null;
        }
        #endregion
        #region 年度赠品统计报表
        public ActionResult GiftAnnualStatistical()
        {
            ViewBag.suppliers = ListToSelect(SupplierService.FindList(o => true).Where(o => o.BusinessType == 1).Select(o => new SelectListItem() { Value = o.Id, Text = o.Title }).ToList(), emptyTitle: "全部");
            var result = WarehouseService.FindList(null).OrderBy(o => o.Id).Select(p => new { Title = p.Title, StoreId = p.StoreId }).ToList();
            ViewBag.storeSelectItems = ListToSelect(result.Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }).ToList(), emptyTitle: "全部");
            ViewBag.categories = ListToSelect(ProductCategoryService.GetParentTypes().Select(o => new SelectListItem() { Value = o.CategorySN.ToString(), Text = o.Title }), emptyTitle: "全部");
            ViewData["Stores"] = result;
            return View();
        }
        public ActionResult GiftAnnualStatisticalPageList()
        {
            object footer = new object();
            var dt = ReportBLL.QueryGiftAnnualStatisticalPageList(Request.Params, ref footer);
            return ToDataGrid(dt, 0, footer);
        }

        public ActionResult GiftAnnualStatisticalExport()
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            List<string> fields = new List<string>() { "month" };
            List<string> names = new List<string>() { "月份" };

            var store = Request.Params["store"].ToString();

            var stores = WarehouseService.FindList(o => o.State == 1 && (o.StoreId == store || store == "" || store == null)).ToDictionary(o => o.StoreId);

            var totalCols = new int[] { };
            string title = "年度赠品统计报表";
            object footer = null;
            dt = ReportBLL.QueryGiftAnnualStatisticalPageList(Request.Params, ref footer) as DataTable;
            if (dt.Rows.Count <= 0)
                return RedirectAlert("GiftAnnualStatistical", "暂无数据可导出！");
            foreach (DataColumn dc in dt.Columns)
            {
                var arr = new string[3] { "TSale", "Tgift", "Ratio" };
                var storeid = dc.ColumnName.Split(arr, StringSplitOptions.RemoveEmptyEntries)[0];
                if (storeid.ToLower() != "month")
                {
                    fields.Add(dc.ColumnName);
                    names.Add(stores.ContainsKey(storeid) ? stores[storeid].Title : "" + (dc.ColumnName.Contains("TSale") ? "销售额" : dc.ColumnName.Contains("TSale") ? "赠品金额" : dc.ColumnName.Contains("TSale") ? "赠品占比" : ""));
                }
            }
            new ExportExcel() { IsBufferOutput = true, HeaderText = title }.ToExcel(DateTime.Now.ToString("yyyyMMdd"), dt, fields.ToArray(), names.ToArray(), null, totalCols);
            return new EmptyResult();
        }

        #endregion 年度赠品统计报表

        #region 月度赠品统计报表
        public ActionResult GiftMonthlyStatistical()
        {
            ViewBag.suppliers = ListToSelect(SupplierService.FindList(o => true).Where(o => o.BusinessType == 1).Select(o => new SelectListItem() { Value = o.Id, Text = o.Title }).ToList(), emptyTitle: "全部");
            var result = WarehouseService.FindList(null).OrderBy(o => o.Id).Select(p => new { Title = p.Title, StoreId = p.StoreId }).ToList();
            ViewBag.storeSelectItems = ListToSelect(result.Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }).ToList(), emptyTitle: "全部");
            ViewBag.categories = ListToSelect(ProductCategoryService.GetParentTypes().Select(o => new SelectListItem() { Value = o.CategorySN.ToString(), Text = o.Title }), emptyTitle: "全部");
            ViewData["Stores"] = result;
            return View();
        }
        public ActionResult GiftMonthlyStatisticalPageList()
        {
            object footer = new object();
            var dt = ReportBLL.QueryGiftMonthlyStatisticalPageList(Request.Params, ref footer);
            return ToDataGrid(dt, 0, footer);
        }

        public ActionResult GiftMonthlyStatisticalExport()
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            List<string> fields = new List<string>() { "store", "riqi", "supplier", "CategoryTitle", "brand", "barcode", "title", "unit", "num", "price", "total" };
            List<string> names = new List<string>() { "门店 ", "日期 ", "供应商名称", "品类", "品牌", "条码", "商品名称", "单位", "数量", "单价", "金额" };

            var store = Request.Params["store"].ToString();

            var stores = WarehouseService.FindList(o => o.State == 1 && (o.StoreId == store || store == "" || store == null)).ToDictionary(o => o.StoreId);

            var totalCols = new int[] { };
            string title = "月度赠品统计报表";
            object footer = null;
            dt = ReportBLL.QueryGiftMonthlyStatisticalPageList(Request.Params, ref footer) as DataTable;
            if (dt.Rows.Count <= 0)
                return RedirectAlert("GiftMonthlyStatistical", "暂无数据可导出！");

            new ExportExcel() { IsBufferOutput = true, HeaderText = title }.ToExcel(DateTime.Now.ToString("yyyyMMdd"), dt, fields.ToArray(), names.ToArray(), null, totalCols);
            return new EmptyResult();
        }

        #endregion 月度赠品统计报表

        #region 销售同比汇总报表
        public ActionResult SalesSummary()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            ViewBag.CurrentMonthTitle = DateTime.Now.ToString("yyyy年MM月dd日");
            ViewBag.SameMonthTitle = DateTime.Now.AddYears(-1).ToString("yyyy年MM月dd日");
            return View();
        }
        [HttpPost]
        public ActionResult QuerySalesSummaryPageList()
        {
            var dt = ReportBLL.QuerySalesSummaryPageList(Request.Params);
            Session["sales"] = dt;
            return ToDataGrid(dt, 0);
        }
        public ActionResult SalesSummaryExport()
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            List<string> fields = new List<string>() { "StoreTitle", "Type", "SameMonth", "CurrentMonth", "YOY" };
            var typeField = Request.Params["typeField"];
            var serchDate = Request.Params["date"];
            List<string> names = new List<string>() { "门店", "项目", "", "", "同比" };
            if (typeField == "AverageAnnual")
            {
                if (serchDate.IsNullOrEmpty()) serchDate = DateTime.Now.ToString("yyyy");
                names = new List<string>() { "门店", "项目", DateTime.Parse(serchDate + "-01-01").AddYears(-1).ToString("yyyy年"), DateTime.Parse(serchDate + "-01-01").ToString("yyyy年"), "同比" };
            }
            else if (typeField == "AverageMonthly")
            {
                if (serchDate.IsNullOrEmpty()) serchDate = DateTime.Now.ToString("yyyy-MM");
                names = new List<string>() { "门店", "项目", DateTime.Parse(serchDate + "-01").AddYears(-1).ToString("yyyy年MM月"), DateTime.Parse(serchDate + "-01").ToString("yyyy年MM月"), "同比" };
            }
            else
            {
                if (serchDate.IsNullOrEmpty()) serchDate = DateTime.Now.ToString("yyyy-MM-dd");
                names = new List<string>() { "门店", "项目", DateTime.Parse(serchDate).AddYears(-1).ToString("yyyy年MM月dd日"), DateTime.Parse(serchDate).ToString("yyyy年MM月dd日"), "同比" };
            }
            var store = Request.Params["store"].ToString();

            var stores = WarehouseService.FindList(o => o.State == 1 && (o.StoreId == store || store == "" || store == null)).ToDictionary(o => o.StoreId);

            var totalCols = new int[] { };
            string title = "销售同比汇总报表";
            dt = ReportBLL.QuerySalesSummaryPageList(Request.Params) as DataTable;
            if (dt.Rows.Count <= 0)
                return RedirectAlert("SalesSummary", "暂无数据可导出！");

            new ExportExcel() { IsBufferOutput = true, HeaderText = title }.ToExcel(DateTime.Now.ToString("yyyyMMdd"), dt, fields.ToArray(), names.ToArray(), null, totalCols);
            return new EmptyResult();
        }

        #endregion

        #region 大类同比汇总月报
        public ActionResult BigCategorySummary()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            return View();
        }
        [HttpPost]
        public ActionResult QueryBigCategorySummaryPageList()
        {
            object footer = new object();
            var dt = ReportBLL.QueryBigCategorySummaryPageList(Request.Params, ref footer);
            return ToDataGrid(dt, dt.Rows.Count, footer);
        }
        public ActionResult BigCategorySummaryExport()
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            var date = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(Request.Params["date"]))
            {
                date = DateTime.Parse(Request.Params["date"]);
            }
            List<string> fields = new List<string>() { "CategoryTitle", "SameSaleTotal", "SaleTotal", "SaleYOY", "SameJMLL", "JMLL", "JMLLYOY", "SameJML", "JML", "JMLYOY" };
            List<string> names = new List<string>() { "大类/中类", date.AddYears(-1).ToString("yyyy年MM月") + "销售金额", date.ToString("yyyy年MM月") + "销售金额", "销售金额同比",
                                                      date.AddYears(-1).ToString("yyyy年MM月") + "毛利率", date.ToString("yyyy年MM月") + "毛利率", "毛利率同比",
                                                         date.AddYears(-1).ToString("yyyy年MM月") + "毛利额", date.ToString("yyyy年MM月") + "毛利额", "毛利额同比",
                                                    };

            var totalCols = new int[] { };
            object footer = null;
            string title = "大类同比汇总月报";
            dt = ReportBLL.QueryBigCategorySummaryPageList(Request.Params, ref footer) as DataTable;
            if (dt.Rows.Count <= 0)
                return RedirectAlert("BigCategorySummary", "暂无数据可导出！");

            new ExportExcel() { IsBufferOutput = true, HeaderText = title }.ToExcel(DateTime.Now.ToString("yyyyMMdd"), dt, fields.ToArray(), names.ToArray(), null, totalCols);
            return new EmptyResult();
        }
        #endregion

        #region 测试(过期)
        public ActionResult DoTest(string parm1, string parm2)
        {
            new Ninject_Extend.Ninject().Finish(parm1, parm2);
            return View();
        }
        #endregion

        #region 供应商同比汇总月报
        public ActionResult SupplierSummary()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            return View();
        }
        [HttpPost]
        public ActionResult QuerySupplierSummaryPageList()
        {
            object footer = new object();
            var dt = ReportBLL.QuerySupplierSummaryPageList(Request.Params, ref footer);
            return ToDataGrid(dt, dt.Rows.Count, footer);
        }
        public ActionResult SupplierSummaryExport()
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            var date = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(Request.Params["date"]))
            {
                date = DateTime.Parse(Request.Params["date"]);
            }
            List<string> fields = new List<string>() { "SupplierTitle", "SameSaleTotal", "SaleTotal", "SaleYOY", "SameJMLL", "JMLL", "JMLLYOY", "SameJML", "JML", "JMLYOY" };
            List<string> names = new List<string>() { "供应商", date.AddYears(-1).ToString("yyyy年MM月") + "销售金额", date.ToString("yyyy年MM月") + "销售金额", "销售金额同比",
                                                      date.AddYears(-1).ToString("yyyy年MM月") + "毛利率", date.ToString("yyyy年MM月") + "毛利率", "毛利率同比",
                                                         date.AddYears(-1).ToString("yyyy年MM月") + "毛利额", date.ToString("yyyy年MM月") + "毛利额", "毛利额同比",
                                                    };
            var totalCols = new int[] { };
            object footer = null;
            string title = "供应商同比汇总月报";
            dt = ReportBLL.QuerySupplierSummaryPageList(Request.Params, ref footer) as DataTable;
            if (dt.Rows.Count <= 0)
                return RedirectAlert("SupplierSummary", "暂无数据可导出！");

            new ExportExcel() { IsBufferOutput = true, HeaderText = title }.ToExcel(DateTime.Now.ToString("yyyyMMdd"), dt, fields.ToArray(), names.ToArray(), null, totalCols);
            return new EmptyResult();
        }
        #endregion


        #region 同比明细月报
        public ActionResult ProductSaleSummary()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            return View();
        }
        [HttpPost]
        public ActionResult QueryProductSaleSummaryPageList()
        {
            object footer = new object();
            var dt = ReportBLL.QueryProductSaleSummaryPageList(Request.Params, ref footer);
            return ToDataGrid(dt, dt.Rows.Count, footer);
        }
        public ActionResult ProductSaleSummaryExport()
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            var date = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(Request.Params["date"]))
            {
                date = DateTime.Parse(Request.Params["date"]);
            }
            List<string> fields = new List<string>() { "Barcode", "SupplierTitle", "StoreTitle", "BrandTitle", "CategoryTitle", "Title", "SubUnit", "SysPrice", "PurchaseNumber", "SamePurchaseNumber",
                                                        "ChainPurchaseNumber","SaleTotal","SameSaleTotal","ChainSaleTotal","BuyPrice","SameBuyPrice","ChainBuyPrice","JMLL",
                                                        "SameJMLL","ChainJMLL","JML","SameJML","ChainJML"};
            List<string> names = new List<string>() { "商品编码", "供应商","门店","品牌","大类/中类","商品名称","单位","系统售价","销售数量","同期销售数量","环期销售数量","销售金额",
                                                      "同期销售金额","环期销售金额","进价金额","同期进价金额","环期进价金额","毛利率","同期毛利率","环期毛利率","毛利额","同期毛利额","环期毛利额"
                                                    };
            var totalCols = new int[] { };
            object footer = null;
            string title = date.ToString("yyyy年MM月") + "同环比销售数据";
            dt = ReportBLL.QueryProductSaleSummaryPageList(Request.Params, ref footer) as DataTable;
            if (dt.Rows.Count <= 0)
                return RedirectAlert("ProductSaleSummary", "暂无数据可导出！");

            new ExportExcel() { IsBufferOutput = true, HeaderText = title }.ToExcel(DateTime.Now.ToString("yyyyMMdd"), dt, fields.ToArray(), names.ToArray(), null, totalCols);
            return new EmptyResult();
        }
        #endregion

        #region 会员销售明细日报表
        public ActionResult MembersSaleDetailDay()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            return View();
        }
        [HttpPost]
        public ActionResult QueryMembersSaleDetailDayPageList()
        {
            object footer = new object();
            int count = 0;
            var dt = ReportBLL.QueryMembersSaleDetailDayPageList(Request.Params, ref footer, ref count);
            Session["sales"] = dt;
            return ToDataGrid(dt, count, footer);
        }
        public ActionResult MembersSaleDetailDayExport()
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            List<string> fields = new List<string>() { "SaleDate", "PaySN", "MobilePhone", "Weixin", "Quantity", "Weigh", "SaleTotal", "PreferentialPrice", "PayTitle" };
            List<string> names = new List<string>() { "销售时间", "流水号", "手机号", "微信号", "件数", "称重", "销售额", "优惠额", "结算方式" };
            var totalCols = new int[] { 4, 5, 6, 7 };
            object footer = null;
            string title = "会员销售明细日报表";
            int count = 0;
            var nvl = new System.Collections.Specialized.NameValueCollection() { Request.Params };
            nvl["ispage"] = "0";
            dt = ReportBLL.QueryMembersSaleDetailDayPageList(nvl, ref footer, ref count) as DataTable;
            if (dt.Rows.Count <= 0)
                return RedirectAlert("MembersSaleDetailDay", "暂无数据可导出！");

            new ExportExcel() { IsBufferOutput = true, HeaderText = title }.ToExcel(DateTime.Now.ToString("yyyyMMdd"), dt, fields.ToArray(), names.ToArray(), null, totalCols);
            return new EmptyResult();
        }

        #endregion

        #region 导购销售明细日报表
        public ActionResult ShoppingGuideSaleDetailDay()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            return View();
        }
        [HttpPost]
        public ActionResult QueryShoppingGuideSaleDetailDayPageList()
        {
            object footer = new object();
            var dt = ReportBLL.QueryShoppingGuideSaleDetailDayPageList(Request.Params, ref footer);
            Session["sales"] = dt;
            return ToDataGrid(dt, 0, footer);
        }
        public ActionResult ShoppingGuideSaleDetailDayExport()
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            List<string> fields = new List<string>() { "SalesManName", "SaleDate", "PaySN", "Quantity", "Weigh", "SaleTotal", "PreferentialPrice", "PayTitle" };
            List<string> names = new List<string>() { "导购员", "销售时间", "流水号", "件数", "称重", "销售额", "优惠额", "结算方式" };
            var totalCols = new int[] { };
            object footer = null;
            string title = "导购销售明细日报表";
            dt = ReportBLL.QueryShoppingGuideSaleDetailDayPageList(Request.Params, ref footer) as DataTable;
            if (dt.Rows.Count <= 0)
                return RedirectAlert("ShoppingGuideSaleDetailDay", "暂无数据可导出！");

            new ExportExcel() { IsBufferOutput = true, HeaderText = title }.ToExcel(DateTime.Now.ToString("yyyyMMdd"), dt, fields.ToArray(), names.ToArray(), null, totalCols);
            return new EmptyResult();
        }

        #endregion

        #region 订单详情（会员销售明细日报表、导购销售明细日报表）
        public ActionResult SaleOrderDetail(string paySN)
        {
            object footer = null;
            var list = ReportBLL.SaleOrderDetail(paySN, ref footer);
            ViewBag.footer = footer.ToJson();
            return View(list);
        }
        public ActionResult Detail()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DetailList()
        {
            object footer = null;
            int count=0;
            var list = ReportBLL.SaleDetail(Request.Params,ref count, ref footer);
            ViewBag.footer = footer.ToJson();
            return ToDataGrid(list, count, footer);
        }
        public ActionResult OrderDetail()
        {
            object footer = null;
            var list = ReportBLL.OrderDetail(Request.Params, ref footer);
            ViewBag.footer = footer.ToJson();
            return View(list);
        }
        #endregion

        #region 前台商品销售流水数据
        public ActionResult ForegroundSaleOrderExport(string date, string date2)
        {
            var dt = Session["sales"] as System.Data.DataTable;
            List<string> fields = new List<string>() { "Barcode", "Title", "CategoryTitle", "SubUnit", "SysPrice", "ActualPrice", "PurchaseNumber", "SaleTotal", "BuyTotal", "MLL", "MLE" };
            List<string> names = new List<string>() { "商品条码", "商品名称", "品类", "单位", "系统均价", "实际均价", "销售数量", "销售金额", "进价金额", "毛利率", "毛利额" };
            var totalCols = new int[4];
            string title = DateTime.Parse(date).ToString("yyyy年MM月");
            object footer = null;
            string fileName = "统计报表";

            var nvl = new System.Collections.Specialized.NameValueCollection() { Request.Params };
            fileName = "前台商品销售流水数据";
            totalCols = new int[] { 6, 9,10 };
            string saleNum = "销售数量",saleTotal="销售金额";
            if(nvl["tuihuan"]=="1")
            {
                saleNum = "退换数量"; saleTotal = "退换补价";
            }
            fields = new List<string>() { "Store", "PaySNFormat", "Sort", "Barcode", "Title", "SalesClassifyId", "PurchaseNumber", "SysPrice", "ActualPrice", "TotalAmount", "OrderDiscount", "ApiTitle", "ApiOrderSN", "CreateDT", "Cashier", "Saler", "MobilePhone" };
            names = new List<string>() { "门店", "流水号", "序号", "条码", "品名", "销售方式", saleNum, "系统售价", "实际售价", saleTotal, "整单让利", "结算方式", "第三方交易号", "销售时间", "收银员", "导购员", "会员手机" };
            title = DateTime.Parse(date).ToString("yyyy年MM月dd日") + ((date2.IsNullOrEmpty() || date == date2) ? "" : "至" + DateTime.Parse(date2).ToString("yyyy年MM月dd日")) + " 前台商品销售流水数据";
            nvl["ispage"] = "0";
            int count = 0;
            dt = SaleOrdersService.QuerySaleOrdersPageList(nvl, out count, out footer);
            if (dt.Rows.Count <= 0)
                return RedirectAlert("Index", "暂无数据可导出！", "SaleManagement");
            var mergeCols = new int[] { 1, 0, 1, 9, 10, 11, 12, 13, 14, 15,16};
            new ExportExcel() { IsBufferOutput = true, HeaderText = title }.ToExcel(fileName, dt, fields.ToArray(), names.ToArray(), mergeCols, totalCols);
            return new EmptyResult();
        }

        #endregion
        #region 商品采购明细汇总
        public ActionResult ProductOrderDetail()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            //ViewBag.suppliers = ListToSelect(SupplierService.GetList().Where(o => o.BusinessType == 1).Select(o => new SelectListItem() { Value = o.Id.ToString(), Text = o.Title }), emptyTitle: "全部");
            ViewBag.brandsns = ListToSelect(ProductBrandService.GetList(true).Select(o => new SelectListItem() { Value = o.BrandSN.ToString(), Text = o.Title }), emptyTitle: "全部");
            return View();
        }
        [HttpPost]
        public ActionResult ProductOrderPageList()
        {
            object footer = new object();
            int count = 0;
            var dt = ReportBLL.ProductOrderDetailPageList(Request.Params, ref footer, ref count);
            return ToDataGrid(dt, count, footer);
        }
        public ActionResult ProductOrderSummary()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            ViewBag.brandsns = ListToSelect(ProductBrandService.GetList(true).Select(o => new SelectListItem() { Value = o.BrandSN.ToString(), Text = o.Title }), emptyTitle: "全部");
            return View();
        }
        
        #endregion
        #region 商品销售退换
        public ActionResult ProductSaleTuiHanDetail()
        {
            ViewBag.cashiers = ListToSelect(SaleOrdersService.GetCashiers(), emptyTitle: "全部");
            ViewBag.salers = ListToSelect(SaleOrdersService.GetSalers(), emptyTitle: "全部");
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            ViewBag.apiCodes = ListToSelect(ApiLibraryService.FindList(a => a.State == 1).Select(a => new SelectListItem() { Value = a.ApiCode.ToString(), Text = a.Title }), emptyTitle: "全部");
            return View();
        }
        [HttpPost]
        public ActionResult ProductSaleTuiHanDetails()
        {
            int count;
            object footer;
            var list = SaleOrdersService.QuerySaleOrdersPageList(Request.Params, out count, out footer);
            return ToDataGrid(list, count, footer);
        }
        #endregion
        #region 商品库存报表
        public ActionResult StoreStockDetail()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            return View();
        }
        [HttpPost]
        public ActionResult StoreStockDetails()
        {
            object footer = new object();
            int count = 0;
            var list = ReportBLL.StoreStockDetailPageList(Request.Params, ref footer, ref count);
            return ToDataGrid(list, count, footer);
        }
        #endregion
        #region 前台销售
        public ActionResult BeforeSaleSummary()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            return View();
        }
        [HttpPost]
        public ActionResult BeforeSalePageList()
        {
            object footer = new object();
            int count = 0;
            var dt = ReportBLL.BeforeSaleSummaryPageList(Request.Params, ref footer, ref count);
            return ToDataGrid(dt, count, footer);
        }
        public ActionResult BeforeSaleDetail()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            return View();
        }
        #endregion
        #region 报损
        public ActionResult BreakageSummary()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            return View();
        }
        [HttpPost]
        public ActionResult BreakagePageList()
        {
            object footer = new object();
            int count = 0;
            var dt = ReportBLL.BreakagePageList(Request.Params, ref footer, ref count);
            return ToDataGrid(dt, count, footer);
        }
        public ActionResult BreakageDetail()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            return View();
        }
        #endregion
        #region 调拨
        public ActionResult HouseMoveSummary()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            return View();
        }
        [HttpPost]
        public ActionResult HouseMovePageList()
        {
            object footer = new object();
            int count = 0;
            var dt = ReportBLL.HouseMovePageList(Request.Params, ref footer, ref count);
            return ToDataGrid(dt, count, footer);
        }
        public ActionResult HouseMoveDetail()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            return View();
        }
        #endregion
        #region 其它出库
        public ActionResult OutboundSummary()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            return View();
        }
        [HttpPost]
        public ActionResult OutboundPageList()
        {
            object footer = new object();
            int count = 0;
            var dt = ReportBLL.OutboundPageList(Request.Params, ref footer, ref count);
            return ToDataGrid(dt, count, footer);
        }
        public ActionResult OutboundDetail()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList(true).Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title, Selected = Sys.CurrentUser.StoreId == o.StoreId }), emptyTitle: "全部");
            return View();
        }
        #endregion
        #region 批发出库
        public ActionResult WholesalSummary()
        {
            ViewBag.wholes = ListToSelect(SupplierService.GetWholesalerList().Select(o => new SelectListItem() { Value = o.Id, Text = o.Title }), emptyTitle: "全部");
            return View();
        }
        [HttpPost]
        public ActionResult WholesalPageList()
        {
            object footer = new object();
            int count = 0;
            var dt = ReportBLL.WholesalPageList(Request.Params, ref footer, ref count);
            return ToDataGrid(dt, count, footer);
        }
        public ActionResult WholesalDetail()
        {
            ViewBag.wholes = ListToSelect(SupplierService.GetWholesalerList().Select(o => new SelectListItem() { Value = o.Id, Text = o.Title }), emptyTitle: "全部");
            return View();
        }
        #endregion
        [HttpPost]
        public ActionResult GetDataForSearch(string searchText, string searchField,short? product)
        {
            var list=new List<DropdownItem>();
            if (product.HasValue)
                list = ProductService.GetDataForSearch(searchText, searchField);
            else
                list = SaleOrdersService.GetDataForSearch(searchText, searchField);
            return new JsonNetResult(list);
        }
    }
}