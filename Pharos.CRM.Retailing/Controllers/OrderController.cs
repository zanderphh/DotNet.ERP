﻿using Pharos.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pharos.Utility.Helpers;
using Pharos.Logic.BLL;
using Pharos.Logic.DAL;
using Pharos.Logic.Entity;
using Pharos.Logic;
using Pharos.Sys;
namespace Pharos.CRM.Retailing.Controllers
{
    public class OrderController : BaseController
    {
        //
        // GET: /Order/

        public ActionResult Index()
        {
            ViewBag.states = EnumToSelect(typeof(OrderState), emptyTitle: "全部");
            //ViewBag.suppliers = ListToSelect(SupplierService.GetList().Where(o => o.BusinessType == 1).Select(o => new SelectListItem() { Value = o.Id, Text = o.Title }), emptyTitle: "全部");
            return View();
        }

        [HttpPost]
        public ActionResult FindPageList()
        {
            int count=0;
            var list= OrderService.FindPageList(Request.Params, out count);
            return ToDataGrid(list, count);
        }

        public ActionResult Save(int? id, string supplierId)
        {
            ViewBag.companys = ListToSelect(WarehouseService.GetList().Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title,Selected=o.StoreId==CurrentUser.StoreId }), emptyTitle: "请选择");
            ViewBag.parenttypes = ListToSelect(ProductCategoryService.GetParentTypes().Select(o => new SelectListItem() { Value = o.Id.ToString(), Text = o.Title }), emptyTitle: "请选择");
            //ViewBag.suppliers = ListToSelect(SupplierService.GetList().Where(o => o.BusinessType == 1 && o.MasterState == 1).Select(o => new SelectListItem() { Value = o.Id, Text = o.Title, Selected = o.Id == supplierId }));
            var obj = new Logic.Entity.IndentOrder() { CreateUID = Sys.CurrentUser.UID, State = (short)OrderState.未提交 };
            if (Sys.CurrentUser.IsStore) obj.StoreId = Sys.CurrentUser.StoreId;
            if (id.HasValue)
            {
                obj = OrderService.FindById(id);
            }
            return View(obj.IsNullThrow());
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Save(IndentOrder obj)
        {
            var re = OrderService.SaveOrUpdate(obj);
            return Content(re.ToJson());
        }
        /// <summary>
        /// 加载明细
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult LoadDetailList(string orderId)
        {
            int count = 0;
            object footer=null;
            var list = OrderService.LoadDetailList(orderId, out count,ref footer);
            return ToDataGrid(list, count,footer);
        }
        /// <summary>
        /// 设置状态
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetState(string ids, short state)
        {
            var sid = ids.Split(',').Select(o=>int.Parse(o)).ToList();
            var list = OrderService.FindList(o => sid.Contains(o.Id));
            list.ForEach(o =>
            {
                o.State = state;
                if (state == 3)
                {
                    o.PeiSongEndDate = DateTime.Now.ToString("yyyy-MM-dd");
                }
                else if (state == 1)
                {
                    o.ApproveDT = DateTime.Now;
                }
            });
            var re = OrderService.Update(list);
            return new JsonNetResult(re);
        }
        /// <summary>
        /// 发信息提醒发货
        /// </summary>
        /// <param name="supplierIds"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Remind(string supplierIds,string orderIds)
        {
            var ids = supplierIds.Split(',');
            var list = SupplierService.FindList(o => ids.Contains(o.Id)).Where(o => !o.Email.IsNullOrEmpty()).Select(o=>o.Email);
            var re = CommonRules.SendEmail("发货提醒", "你好，订单号为["+orderIds+"],请发货。", list.ToArray());
            return new JsonNetResult(re);
        }
        /// <summary>
        /// 获取采购单信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetIndentOrderById(string id)
        {
            var obj = OrderService.FindById(id);
            return new JsonNetResult(obj);
        }

        /// <summary>
        /// 获取采购单信息
        /// </summary>
        /// <param name="indentOrderId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetIndentOrderByIndentOrderId(string indentOrderId)
        {
            var obj = OrderService.Find(o => o.IndentOrderId == indentOrderId);
            return new JsonNetResult(obj);
        }
        [HttpPost]
        public ActionResult GetStoreAddress(string storeId)
        {
            var obj= WarehouseService.Find(o => o.StoreId == storeId);
            if (obj == null) return Content("");
            return Content(obj.Address);
        }

        public ActionResult Detail(int? id, string supplierId)
        {
            ViewBag.companys = ListToSelect(WarehouseService.GetList().Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title }));
            ViewBag.parenttypes = ListToSelect(ProductCategoryService.GetParentTypes().Select(o => new SelectListItem() { Value = o.Id.ToString(), Text = o.Title }), emptyTitle: "请选择");
            ViewBag.suppliers = ListToSelect(SupplierService.GetList().Where(o => o.BusinessType == 1 && o.MasterState == 1).Select(o => new SelectListItem() { Value = o.Id, Text = o.FullTitle, Selected = o.Id == supplierId }));
            var obj = OrderService.FindById(id);
            var user=UserInfoService.Find(o=>o.UID==obj.CreateUID);
            if (user != null)
                ViewData["creator"] = user.FullName;
            user = UserInfoService.Find(o => o.UID == obj.RecipientsUID);
            if (user != null)
                ViewData["reciptor"] = user.FullName;
            return View(obj);
        }
        [HttpPost]
        public ActionResult GetGift(string supplierId, string barcodes,string categorys, decimal ordernum)
        {
            var list= PrivilegeOrderService.OrderCalc(supplierId, barcodes, categorys, ordernum);
            return Json(list);
        }

        public ActionResult ReportDetail(string type, string orderId)
        {
            object obj = OrderService.ReportDetail(type,orderId);

            return View(obj);
        }
        [HttpPost]
        public ActionResult LoadReportDetailList(string orderId)
        {
            int count = 0;
            object footer = null;
            var list = OrderService.LoadReportDetailList(orderId, out count, ref footer);
            return ToDataGrid(list, count, footer);
        }

        public ActionResult SelectOrder()
        {
            ViewBag.states = EnumToSelect(typeof(OrderState), emptyTitle: "全部");
            return View();
        }
    }
}
