﻿using Pharos.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pharos.Utility.Helpers;
using Pharos.Logic.OMS.BLL;
using Pharos.Logic.OMS.Entity;
namespace Pharos.OMS.Retailing.Controllers
{
    public class DepartMentController : BaseController
    {
        #region 私有对象
        [Ninject.Inject]
        SysUserService UserService { get; set; }
        [Ninject.Inject]
        DepartMentService DeptService { get; set; }
        #endregion
        [SysPermissionValidate]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult FindPageList()
        {
            int count = 0;
            var list = DeptService.GetPageList();
            return ToDataGrid(list, count);
        }

        [HttpPost]
        public ActionResult Delete(int[] ids)
        {
            return new JsonNetResult(DeptService.Deletes(ids));
        }

        [SysPermissionValidate(211)]
        public ActionResult Save(int? deptid, int? pdeptId)
        {
            var obj = new SysDepartments() { PDeptId=pdeptId.GetValueOrDefault(),Status=true };
            if (deptid.HasValue)
            {
                obj = DeptService.Get(deptid.Value);
            }
            ViewBag.ParentDepth = 0;
            if(pdeptId.HasValue)
            {
                var pm= DeptService.Get(pdeptId.Value);
                if (pm != null)
                {
                    ViewBag.Parent = pm.Title;
                    ViewBag.ParentDepth = pm.Depth;
                }
            }
            ViewBag.users = ListToSelect(UserService.GetList().Select(o => new SelectListItem() { Text = o.FullName, Value = o.UserId }), emptyTitle: "请选择");
            return View(obj.IsNullThrow());
        }
        [HttpPost]
        public ActionResult Save(SysDepartments obj, short parentDepth)
        {
            var re = DeptService.SaveOrUpdate(obj, parentDepth);
            return new OpActionResult(re);
        }
        
        [HttpPost]
        public void MoveItem(short mode, int deptId)
        {
            if (mode == 1 || mode == 2)
                DeptService.MoveItem(mode, deptId);
            else
                DeptService.MoveUpItem(mode, deptId);
        }
        [HttpPost]
        public void SetState(short mode, int deptId)
        {
            DeptService.SetState(mode, deptId);
        }
        [HttpPost]
        public ActionResult GetInput(string searchName)
        {
            var list = DeptService.GetInput(searchName);
            return ToDataGrid(list, 0);
        }
    }
}
