﻿using Pharos.Logic.OMS;
using Pharos.Logic.OMS.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pharos.Utility.Helpers;
using Pharos.Utility;
using Pharos.Logic.OMS.BLL;

namespace Pharos.OMS.Retailing.Controllers
{
    public class DeviceController : BaseController
    {
        [Ninject.Inject]
        DevicesService devicesService { get; set; }

        [Ninject.Inject]
        ImportSetService ImportSetService { get; set; }

        [Ninject.Inject]
        ProductService ProductService { get; set; }

        [Ninject.Inject]
        TradersService tradersService { get; set; }

        [SysPermissionValidate]
        public ActionResult Index()
        {
            //维护人
            //ViewBag.user = ListToSelect(tradersService.getUserList().Select(o => new SelectListItem() { Value = o.FullName, Text = o.FullName }), emptyTitle: "请选择或输入");
            ViewBag.user = tradersService.getUserList().CreateSelect("FullName", "FullName", "全部", "全部");
            ViewBag.Category = ListToSelect(devicesService.getDataList().Select(o => new SelectListItem() { Value = o.DicSN.ToString(), Text = o.Title }), emptyTitle: "全部");
            return View();
        }

        public ActionResult FindPageList()
        {
            var count = 0;
            var list = devicesService.GetPageList(Request.Params, out count);
            return ToDataGrid(list, count);
        }

        public ActionResult Save(int? id)
        {
            ViewBag.Category = ListToSelect(devicesService.getDataList().Select(o => new SelectListItem() { Value = o.DicSN.ToString(), Text = o.Title }), emptyTitle: "请选择");
            var obj = new Devices();
            obj.Status = 1;
            if (id.HasValue)
            {
                obj = devicesService.GetOne(id.Value);
            }
            return View(obj.IsNullThrow());
        }

        [HttpPost]
        public ActionResult Save(Devices obj)
        {
            obj.DeviceId = CommonService.GUID.ToUpper();
            obj.CreateUID = CurrentUser.UID;
            obj.CreateDT = DateTime.Now;

            var op = devicesService.SaveOrUpdate(obj);
            return new OpActionResult(op);
        }

        [HttpPost]
        public ActionResult Delete(int[] ids)
        {
            return new JsonNetResult(devicesService.Deletes(ids));
        }

        /// <summary>
        /// 设置可用，设置停用
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetState(string ids, short state)
        {
            return new JsonNetResult(devicesService.SetState(ids,state));
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <returns></returns>
        public ActionResult Import()
        {
            var obj = ImportSetService.GetOne("Devices");
            if (obj == null)
            {
                obj = new ImportSet();
            }
            obj.RefCreate = false;
            return View(obj);
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="imp"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Import(ImportSet imp)
        {
            imp.TableName = "Devices";
            var op = devicesService.Import(imp, Request.Files, Request["FieldName"], Request["ColumnName"]);
            return Content(op.ToJson());
        }
        [HttpPost]
        public ActionResult GetDeviceInput(string searchName)
        {
            var list = devicesService.GetDeviceInput(searchName);
            return ToDataGrid(list, list.Count);
        }

        /// <summary>
        /// 获取品牌
        /// </summary>
        /// <param name="searchName"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult getBrand(string brand)
        {
            var list = devicesService.getBrand(brand);
            return ToDataGrid(list, 0);
        }
    }

}
