﻿using Pharos.Logic.OMS.BLL;
using Pharos.Logic.OMS.Entity;
using Pharos.Utility.Helpers;
using Pharos.OMS.Retailing.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;

namespace Pharos.OMS.Retailing.Areas.webapp.Controllers
{
    public class TraderController : BaseController
    {
        [Ninject.Inject]
        TraderTypeService TraderTypeService { get; set; }
        [Ninject.Inject]
        TradersService TraderService { get; set; }
        [Ninject.Inject]
        DictionaryService DictionaryService { get; set; }
        [Ninject.Inject]
        BusinessService BusinessService { get; set; }
        [Ninject.Inject]
        AreaService AreaService { get; set; }
        [Ninject.Inject]
        SysUserService UserService { get; set; }
        public ActionResult AddTrader(string uid)
        {
            var datas = DictionaryService.GetChildList(new List<int>() { 205,300,320,340,221},false);
            var areas = AreaService.GetList();
            //ViewBag.types=ListToSelect( TraderTypeService.getList().Select(o=>new SelectListItem(){Text=o.Title,Value=o.TraderTypeId}),"未选择");
            ViewBag.trackstauts = ListToSelect(datas.Where(o=>o.DicPSN==205).Select(o => new SelectListItem() { Value = o.DicSN.ToString(), Text = o.Title }), emptyTitle: "未选择");
            ViewBag.businessScopes = ListToSelect(BusinessService.GetList(false).Select(o => new SelectListItem() {Value=o.ById,Text=o.Title }));
            ViewBag.pays = ListToSelect(datas.Where(o => o.DicPSN == 300).Select(o => new SelectListItem() { Value = o.DicSN.ToString(), Text = o.Title }));
            ViewBag.modes = ListToSelect(datas.Where(o => o.DicPSN == 221).Select(o => new SelectListItem() { Value = o.DicSN.ToString(), Text = o.Title }));
            ViewBag.storenums = ListToSelect(datas.Where(o => o.DicPSN == 320).Select(o => new SelectListItem() { Value = o.DicSN.ToString(), Text = o.Title }), emptyTitle: "未选择");
            ViewBag.posnums = ListToSelect(new List<SelectListItem>() { new SelectListItem() { Value = "1", Text = "1" }, new SelectListItem() { Value = "2", Text = "2" }, new SelectListItem() { Value = "3", Text = "3" } }, emptyTitle: "未选择");
            ViewBag.personnums = ListToSelect(datas.Where(o => o.DicPSN == 340).Select(o => new SelectListItem() { Value = o.DicSN.ToString(), Text = o.Title }), emptyTitle: "未选择");
            ViewBag.provs = areas.Where(o => o.Type == 2).Select(o => new { text = o.Title, value = o.AreaID.ToString() }).ToJson();
            ViewBag.citys = GetCity(areas.Where(o => o.Type == 2),areas).ToJson();
            ViewBag.dists = GetCity(areas.Where(o => o.Type == 3), areas).ToJson();
            string name = "";
            var trader = (Traders)Session["traders"] ?? new Traders() { Assigner=uid};
            //if ((uid.IsNullOrEmpty() || uname.IsNullOrEmpty()) && Common.GetNickName(out name))
            //{
            //    var user = UserService.GetOneByWeixin(name);
            //    if (user != null)
            //    {
            //        trader.AssignerUID = user.UserId;
            //        trader.Assigner = user.FullName;
            //        ViewBag.msg = "";
            //    }
            //    else
            //        ViewBag.msg = "在用户管理中未配置您的微信号！";
            //}
            //else
            //    ViewBag.msg = name;
            return View(trader);
        }
        Dictionary<string,object> GetCity(IEnumerable<Area> currents, List<Area> alls)
        {
            var dict=new Dictionary<string,object>();
            foreach(var area in currents)
            {
                dict[area.AreaID.ToString()] = alls.Where(o => o.AreaPID == area.AreaID).Select(o => new { text = o.Title, value = o.AreaID.ToString() });
            }
            return dict;
        }
        [HttpPost]
        public ActionResult AddTrader(Traders obj)
        {
            string msg="";
            if (!UserService.CheckUserByCode(obj.Assigner, obj.AssignerUID, ref msg))
            {
                return new OpActionResult(Utility.OpResult.Fail(msg));
            }
            if(TraderService.ExistsTitle(obj.Title,obj.FullTitle))
                return new OpActionResult(Utility.OpResult.Fail("客户简称或全称已存在！"));
            obj.TraderTypeId = "";
            obj.Pay = Request["Pay"];
            obj.BusinessScopeId = Request["BusinessScopeId"];
            var citys = obj.Cities;
            if(!citys.IsNullOrEmpty())
            {
                var cs=citys.Split(',').Select(o => short.Parse(o)).ToList();
                if(cs.Count==3)
                {
                    obj.CurrentProvinceId = cs[0];
                    obj.CurrentCityId = cs[1];
                    obj.CurrentCounty = cs[2];
                }
            }
            Session["traders"] = obj;
            return new OpActionResult(Utility.OpResult.Success());
        }
        public ActionResult AddOrder()
        {
            var datas = DictionaryService.GetChildList(new List<int>() { 197, 360}, false);
            ViewBag.devices = ListToSelect(datas.Where(o => o.DicPSN == 197).Select(o => new SelectListItem() { Value = o.DicSN.ToString(), Text = o.Title }),emptyTitle:"请选择");
            ViewBag.units = ListToSelect(datas.Where(o => o.DicPSN == 360).Select(o => new SelectListItem() { Value = o.DicSN.ToString(), Text = o.Title }));
            return View();
        }
        [HttpPost]
        public ActionResult AddOrder(short[] DeviceId, short[] OrderNum, int[] UnitID, string[] Remark, string[] Title, string[] Unit)
        {
            var trader = (Traders)Session["traders"];
            var orders = new List<OrderList>();
            for (int i = 0; i < DeviceId.Length; i++)
            {
                if (DeviceId[i] == 0 || OrderNum[i]==0) continue;
                orders.Add(new OrderList()
                {
                    DeviceId=DeviceId[i],
                    OrderNum=OrderNum[i],
                    UnitID=UnitID[i],
                    Remark=Remark[i],
                    Title=Title[i],
                    UnitName=Unit[i]
                });
            }
            var op= TraderService.AddTrader(trader, orders);
            if (op.Successed) Session["traders"] = null;
            return new OpActionResult(op);
        }
    }
}