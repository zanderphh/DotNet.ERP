﻿using Pharos.Logic.OMS;
using Pharos.Logic.OMS.Entity;
using Pharos.Logic.OMS.Entity.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pharos.Utility.Helpers;
using Pharos.Utility;
using Pharos.Logic.OMS.BLL;
using System.Data;

namespace Pharos.OMS.Retailing.Controllers
{
    public class PayAccountController : BaseController
    {
        [Ninject.Inject]
        TradersService tradersService { get; set; }

        [Ninject.Inject]
        TradersPaySecretKeyService tradersPaySecretKeyService { get; set; }

        [Ninject.Inject]
        PayLicenseService payLicenseService { get; set; }

        [SysPermissionValidate]
        public ActionResult Index()
        {
            //指派人
            ViewBag.user = ListToSelect(tradersService.getUserList().Select(o => new SelectListItem() { Value = o.UserId, Text = o.FullName }), emptyTitle: "全部");
            return View();
        }

        [SysPermissionValidate(124)]
        public ActionResult Save(int? id)
        {
            //支付方式
            List<TradersPayChannel> TradersPayChannelList = new List<TradersPayChannel>();
            //商户资料
            Traders traders = new Traders();
            var obj = new TradersPaySecretKey
            {
                SecretKey=CommonService.GUID.ToUpper()
            };
            if (id.HasValue)
            {
                obj = tradersPaySecretKeyService.GetEntityById(id.Value);
                TradersPayChannelList = tradersPaySecretKeyService.GetTradersPayChannel(Convert.ToInt32(id));
                traders = tradersService.GetEntityByWhere(o => o.CID == obj.CID);
            }
            //指派人
            ViewBag.user = ListToSelect(tradersService.getUserList().Select(o => new SelectListItem() { Value = o.UserId, Text = o.FullName }), emptyTitle: "请选择");
            //支付通道
            ViewBag.ClNo = ListToSelect(tradersPaySecretKeyService.GetPayChannelManage().Select(o => new SelectListItem() { Value = o.ChannelNo.ToString(), Text = o.ChannelCode }), emptyTitle: "请选择");
            //支付方式
            ViewBag.Channel = TradersPayChannelList;
            //商户资料
            ViewBag.Tra = traders;
            return View(obj.IsNullThrow());
        }

        [HttpPost]
        public ActionResult Save(int Id)
        {
            TradersPaySecretKey tradersPaySecretKey = new TradersPaySecretKey();
            DateTime dt = DateTime.Now;
            if (Id == 0)
            {
                tradersPaySecretKey.TPaySecrectId = CommonService.GUID.ToUpper();
                tradersPaySecretKey.CreateUID = CurrentUser.UID;
                tradersPaySecretKey.CreateDT = dt;
            }
            else
            {
                tradersPaySecretKey = tradersPaySecretKeyService.GetEntityById(Id);
            }
            tradersPaySecretKey.ModifyUID = CurrentUser.UID;
            tradersPaySecretKey.ModifyDT = dt;
            TryUpdateModel<TradersPaySecretKey>(tradersPaySecretKey);
            var op = tradersPaySecretKeyService.Save(tradersPaySecretKey, Id, dt, Request.Params);
            return new OpActionResult(op);
        }

        public ActionResult FindPageList()
        {
            var count = 0;
            var list = tradersPaySecretKeyService.GetPageList(Request.Params, out count);
            return ToDataGrid(list, count);
        }

        /// <summary>
        /// 获取CID
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ActionResult GetCidWhere(string keyword)
        {
            var list = tradersPaySecretKeyService.GetCIDWhere(Request.Params);
            return ToDataGrid(list, 0);
        }

        /// <summary>
        /// 获取支付方式
        /// </summary>
        /// <param name="ChannelNo"></param>
        /// <returns></returns>
        public ActionResult GetPayManner(int ChannelNo)
        {
            var list = tradersPaySecretKeyService.GetPayChannelDetail(ChannelNo);
            return new JsonNetResult(list);
        }


        [HttpPost]
        public ActionResult UpState(string ids, short state)
        {
            return new JsonNetResult(tradersPaySecretKeyService.UpState(ids, state));
        }

        /// <summary>
        /// 获取商户资料
        /// </summary>
        /// <param name="CID"></param>
        /// <returns></returns>
        public string GetEntityTraders(int CID = 0)
        {
            Traders traders = new Traders();
            PayLicense payLicense = payLicenseService.GetEntityByWhere(o=>o.CID==CID);
            if (payLicense != null)
            {
                traders = tradersService.GetTraderByCID(CID);
            }
            return traders.ToJson();
        }
    }
}