﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pharos.Sys;
using Pharos.Sys.BLL;
using Pharos.Sys.Entity;
using Pharos.Utility.Helpers;
using Pharos.CRM.Retailing.Models;
using Pharos.Utility;
namespace Pharos.CRM.Retailing.Controllers
{
    public class AuthorizationController : BaseController
    {
        #region 产品注册
        public ActionResult Register(int again=0)
        {
            return RedirectToAction("Activate");
            var companyId = SysCommonRules.CompanyId;
            var obj =CurrentUser.Company as CompanyAuthorize ?? new CompanyAuthorize() { Way = 1 };
            return View(obj);
        }
        [HttpPost]
        public ActionResult Register(CompanyAuthorize obj)
        {
            var op= Authorize.RegisterAgain(obj);
            return Content(op.ToJson());
        }
        public ActionResult Activate(int? again)
        {
            OpResult op=null;
            var comp = Authorize.GetCompanyByConnect(ref op);
            ViewBag.code =op!=null? op.Code:"";
            return View(comp);
        }
        [HttpPost]
        public ActionResult Activate(string serialNo)
        {
            var op = Authorize.Activate(serialNo);
            return Content(op.ToJson());
        }

        public ActionResult OutRegister(int cid)
        {
            var omsurl = Authorize.OmsUrl + "api/outerapi/GetRegisterData?companyId=" + cid;
            var json = HttpClient.HttpPost(omsurl, "");
            if (json == "404") 
                return RedirectAlert(Request.UrlReferrer.ToString(), "连接OMS管理平台失败，请检查网络是否正常！");
            var data = Newtonsoft.Json.Linq.JObject.Parse(json);
            var traderTypes = data.Property("traderTypes", true);
            var modes = data.Property("modes", true);
            var busines = data.Property("busines", true);
            var tracks = data.Property("tracks", true);
            var orderList = data.Property("orderList", true);
            var trader = data.Property("trader", true);
            //商户分类
            ViewBag.TraderTypes = ListToSelect(traderTypes.ToObject<List<SelectListItem>>(), emptyTitle: "请选择");

            //经营模式
            ViewBag.ModeIds = ListToSelect(modes.ToObject<List<SelectListItem>>(), emptyTitle: "请选择");

            //经营类目
            ViewBag.BusinessScopeIds = busines.ToObject<List<SelectListItem>>();

            //跟踪状态
            ViewBag.StautsIds = ListToSelect(tracks.ToObject<List<SelectListItem>>(), emptyTitle: "请选择");

            //采购意向清单
            ViewBag.OrderList = orderList.ToObject<List< ViewOrderList>>();

            return View(trader.ToObject<Traders>() ?? new Traders() { Way=1});
        }

        /// <summary>
        /// 保存、修改
        /// </summary>
        /// <param name="traders">商户资料</param>
        /// <param name="h_OrderList">采购意向清单</param>
        /// <param name="h_VisitTrack">回访小结</param>
        /// <param name="BusinessScopeId">经营类目</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult OutRegister(Traders traders, string h_OrderList, string way, string openVersionId, string storeProper, string posMinorDisp, string appProper)
        {
            traders.BusinessScopeId = Request["BusinessScopeId"];
            storeProper = storeProper == "Y" ? "Y" : "N";
            posMinorDisp = posMinorDisp == "Y" ? "Y" : "N";
            appProper = appProper == "Y" ? "Y" : "N";
            var omsurl=Authorize.OmsUrl+"api/outerapi/Register";
            omsurl += string.Format("?orderList={0}&way={1}&storeProper={2}&posMinorDisp={3}&appProper={4}&openVersionId={5}&machine={6}", h_OrderList, way, storeProper, posMinorDisp, appProper, openVersionId, Machine.GetMAC);
            var rt= Pharos.Utility.HttpClient.HttpPost(omsurl, traders.ToJson());
            var op = new OpResult();
            op.Successed = rt == "1";//todo:异常时处理
            op.Message = "";
            if (op.Successed) Authorize.RemoveCurrentAuth();
            return Content(op.ToJson());
        }


        #endregion
    }
}
