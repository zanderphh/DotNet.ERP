﻿using Pharos.Logic;
using Pharos.Logic.BLL;
using Pharos.Logic.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pharos.Utility.Helpers;
using Pharos.Utility;
using Pharos.CRM.Retailing;
using Pharos.CRM.Retailing.Models;

namespace Pharos.CRM.Retailing.Controllers
{
    public class StoreController : BaseController
    {
        //
        // GET: /Store/
        private readonly Pharos.Sys.BLL.SysMenuBLL _menuBLL = new Pharos.Sys.BLL.SysMenuBLL();

        #region 首页
        public ActionResult Index()
        {
            #region 如未登录 或 非门店登录，则跳转到 门店登录页
            if (!Sys.CurrentUser.IsLogin)
            {
                Sys.CurrentUser.Exit();
                return RedirectToAction("Login");
                //Response.Redirect("/Store/Login");
            }
            #endregion
            /*
            csID csid2 = ipLocalhost();

            //url中cid
            string s_cid = "";

            if (!RouteData.Values["cid"].IsNullOrEmpty())
            {
                s_cid = RouteData.Values["cid"].ToString();
            }
            //url中sid
            string s_sid = "";

            if (!RouteData.Values["sid"].IsNullOrEmpty())
            {
                s_sid = RouteData.Values["sid"].ToString();
            }

            if (csid2.message == "禁止访问")
            {
                Response.Redirect("/Account/noBusiness");
                return null;
            }

            if (!s_cid.IsNullOrEmpty() && !s_sid.IsNullOrEmpty())
            {
                Authorize authorize = new Authorize();
                csID csid = authorize.getCidSid(s_cid, s_sid);
                if (csid.message == "格式错误")
                {
                    Response.Redirect("/Account/noBusiness");
                    return null;
                }
                else if (csid.message == "域名的store后面必须是数字")
                {
                    Response.Redirect("/Account/noBusiness");
                    return null;
                }
                else if (csid.message == "success")
                {
                    //如未登录 或 非门店登录，则跳转到 门店登录页
                    if (!Sys.CurrentUser.IsLogin || !Sys.CurrentUser.IsStore)
                    {
                        Sys.CurrentUser.Exit();

                        //ip、localhost门店登录
                        if (csid2.message == "localhost" || csid2.message == "ip")
                        {
                            Response.Redirect("/store" + csid2.cid + "-" + csid2.sid);
                            return null;
                        }

                        Response.Redirect("/Store/Login");
                        return null;
                    }
                    else
                    {
                        if (Cookies.IsExist("remuc"))
                        {
                            //cookie的CID
                            string cid = Cookies.Get("remuc", "_cid").Trim();
                            //cookie的门店ID
                            string sid = Server.UrlDecode(Cookies.Get("remuc", "_storeId"));
                            sid = sid.Split('~')[0];

                            string ss = sid.Split('~')[0];

                            if (cid.IsNullOrEmpty())
                            {
                                cid = "0";
                            }
                            if (sid.IsNullOrEmpty())
                            {
                                sid = "0";
                            }

                            if (csid.cid.Trim() != cid || csid.sid.Trim() != sid)
                            {
                                Sys.CurrentUser.Exit();

                                //ip、localhost门店登录
                                if (csid2.message == "localhost" || csid2.message == "ip")
                                {
                                    Response.Redirect("/store" + csid2.cid + "-" + csid2.sid);
                                    return null;
                                }

                                Response.Redirect("/Store/Login");
                                return null;
                            }
                        }
                    }

                }
            }
            else
            {
                Response.Redirect("/Account/noBusiness");
                return null;
            }
            //是否localhost、ip门店登录
            string isLocalhostIp = "0";
            if (csid2.message == "localhost" || csid2.message == "ip")
            {
                isLocalhostIp = "1";
            }
            */
            ViewBag.isLocalhostIp = "1";
            ViewBag.cid = Pharos.Utility.Config.GetAppSettings("CompanyId");
            ViewBag.sid = Sys.SysCommonRules.CurrentStore;
            

            ViewBag.WelcomeText = "欢迎光临";
            ViewBag.CurUserName = Sys.CurrentUser.FullName;
            ViewBag.CurLoginName = Sys.CurrentUser.UserName;
            ViewBag.comptitle = Sys.CurrentUser.StoreName;
            //获取活动列表 
            var activityList = CommodityPromotionService.GetNewestActivity(3);
            //获取公告列表
            var noticeList = NoticeService.GetNewestNotice(3);
            //采购订单列表
            ViewBag.OrderList = OrderService.GetNewOrder(3);

            List<ActivityNoticeModel> activityNoticeList = new List<ActivityNoticeModel>();
            if (activityList != null)
            {
                foreach (var activity in activityList)
                {
                    activityNoticeList.Add(new ActivityNoticeModel(activity.Id, Enum.GetName(typeof(PromotionType), activity.PromotionType),
                        DateTime.Parse(activity.StartDate.ToString()).ToString("yyyy-MM-dd") + "至" + DateTime.Parse(activity.EndDate.ToString()).ToString("yyyy-MM-dd"),
                        Enum.GetName(typeof(SaleState), activity.State), activity.CreateDT, 1));
                }
            }
            if (noticeList != null)
            {
                foreach (var notice in noticeList)
                {
                    activityNoticeList.Add(new ActivityNoticeModel(notice.Id.ToString(), notice.Theme, notice.BeginDate.ToString("yyyy-MM-dd") + "至" + notice.ExpirationDate.ToString("yyyy-MM-dd"),
                        notice.State == 1 ? "已发布" : "未发布", notice.CreateDT, 2));
                }
            }
            activityNoticeList = activityNoticeList.OrderByDescending(o => o.CreateDT).Take(3).ToList();
            if (activityNoticeList == null)
                activityNoticeList = new List<ActivityNoticeModel>();
            ViewBag.activityNoticeList = activityNoticeList;//活动公告

            //近3天数据
            var beginTime = DateTime.Parse(DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd"));
            var endTime = DateTime.Parse(DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"));
            ViewBag.newMemberNumber = MembersService.GetNewMemberNumber(beginTime, endTime, Sys.CurrentUser.StoreId);//新增会员数量
            ViewBag.newSalesVolume = ReportBLL.GetSalesVolume(beginTime, endTime, Sys.CurrentUser.StoreId);//新增销售量
            var saleOrderList3Day = SaleOrdersService.GetIndexSaleOrder(beginTime, endTime, Sys.CurrentUser.StoreId);//3天内的销售订单
            ViewBag.newSaleOrderNumber = saleOrderList3Day.Count();//新增客单量
            decimal newSaleTotal = 0;
            newSaleTotal = saleOrderList3Day.Sum(o => o.TotalAmount);
            ViewBag.newSaleTotal = newSaleTotal;//新增销售额

            //近7天数据
            var dayTitleList = new List<string>();
            var saleTotalList = new List<decimal>();
            var saleOederNumberList = new List<int>();
            var hotProductNameList = new List<string>();
            var hotProductSaleNumList = new List<int>();
            for (int i = 6; i >= 0; i--)
            {
                var time1 = DateTime.Parse(DateTime.Now.AddDays(0 - i).ToString("yyyy-MM-dd"));
                var time2 = DateTime.Parse(DateTime.Now.AddDays(0 - i + 1).ToString("yyyy-MM-dd"));
                var saleOrderList = SaleOrdersService.GetIndexSaleOrder(time1, time2, Sys.CurrentUser.StoreId);

                dayTitleList.Add(int.Parse(DateTime.Now.AddDays(0 - i).ToString("dd")) + "日");
                saleTotalList.Add(saleOrderList.Sum(o => o.TotalAmount));
                saleOederNumberList.Add(saleOrderList.Count());
            }

            var hotProductBeginTime = DateTime.Parse(DateTime.Now.AddDays(-6).ToString("yyyy-MM-dd"));
            var hotProductEndTime = DateTime.Parse(DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"));

            ReportBLL.GetHotProduct(hotProductBeginTime, hotProductEndTime, out hotProductNameList, out hotProductSaleNumList, Sys.CurrentUser.StoreId);

            //近7天热销商品
            ViewBag.hotProductNameList = hotProductNameList.ToJson();
            ViewBag.hotProductSaleNumList = hotProductSaleNumList.ToJson();
            ViewBag.hotProductNameListNotJson = hotProductNameList;

            ViewBag.dayTitleList = dayTitleList.ToJson();
            ViewBag.saleTotalList = saleTotalList.ToJson();//近7天销售额
            ViewBag.saleOederNumberList = saleOederNumberList.ToJson();//近7天客单量

            var list = new List<Pharos.Sys.Models.MenuModel>();
            list = _menuBLL.GetHomeMenusByUID(Sys.CurrentUser.UID,1);
            //var pids = new string[] { "1", "3", "15", "25", "68" };
            ViewBag.Menus = list;
            var set = new Pharos.Sys.BLL.SysWebSettingBLL().GetWebSetting();
            return View(set);
        }
        #endregion

        #region 旧首页
        public ActionResult Old_Index()
        {
            #region 如未登录 或 非门店登录，则跳转到 门店登录页
            if (!Sys.CurrentUser.IsLogin || !Sys.CurrentUser.IsStore)
            {
                Sys.CurrentUser.Exit();
                return RedirectToAction("Login");
            }
            #endregion

            ViewBag.WelcomeText = "欢迎光临";
            ViewBag.CurUserName = Sys.CurrentUser.FullName;
            ViewBag.CurLoginName = Sys.CurrentUser.UserName;

            //获取公告列表
            ViewBag.NoticeList = NoticeService.GetNewestNotice(3);
            //获取活动列表 
            ViewBag.ActivityList = CommodityPromotionService.GetNewestActivity(3);
            ViewBag.OrderList = OrderService.GetNewOrder(7);
            var categories = new List<string>();
            var stores = new List<string>();
            var hours = new TimeSpan[] { new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0), new TimeSpan(12, 0, 0), new TimeSpan(14, 0, 0), new TimeSpan(16, 0, 0), new TimeSpan(18, 0, 0), new TimeSpan(20, 0, 0) };
            var list = new List<Pharos.Sys.Models.MenuModel>();
            list = _menuBLL.GetHomeMenusByUID("");
            var pids = new string[] {"1","3", "15", "25", "68" };
            ViewBag.Menus = list.Where(o => pids.Contains(o.Id)).ToList();
            ViewBag.categoryData = ReportBLL.QueryIndexSaleCategorys(ref categories).ToJson();
            ViewBag.chart1Data = ReportBLL.QueryIndexSaleCategorys(ref categories).FirstOrDefault() == null ? new List<object>() : ReportBLL.QueryIndexSaleCategorys(ref categories).FirstOrDefault().data;
            ViewBag.categoryJSON = categories.ToJson();
            ViewBag.hourData = ReportBLL.QueryIndexSaleHour(categories, hours).ToJson();
            ViewBag.chart2Data = ReportBLL.QueryIndexSaleHour(categories, hours).FirstOrDefault() == null ? new List<object>() : ReportBLL.QueryIndexSaleHour(categories, hours).FirstOrDefault().data;
            ViewBag.hoursJSON = hours.Select(o => o.Hours.ToString("00") + ":" + o.Minutes.ToString("00")).ToJson();
            ViewBag.storeData = ReportBLL.QueryIndexSaleStore(categories, ref stores).ToJson();
            ViewBag.chart3Data = ReportBLL.QueryIndexSaleStore(categories, ref stores).FirstOrDefault() == null ? new List<object>() : ReportBLL.QueryIndexSaleStore(categories, ref stores).FirstOrDefault().data;
            ViewBag.storeJSON = stores.ToJson();
            return View();
        }
        #endregion 

        #region 入库登记
        public ActionResult InboundGoods(string inboundGoodsId)
        {
            ViewBag.shops = ListToSelect(WarehouseService.GetList().Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title }), emptyTitle: "请选择");
            ViewBag.users = ListToSelect(UserInfoService.GetList().Select(o => new SelectListItem() { Value = o.UID, Text = o.FullName }), emptyTitle: "请选择");
            //ViewBag.suppliers = ListToSelect(SupplierService.GetList().Where(o => o.BusinessType == 1).Select(o => new SelectListItem() { Value = o.Id, Text = o.Title }));
            //ViewBag.suppliers = ListToSelect(SupplierService.GetList().Where(o => o.BusinessType == 1).Select(o => new SelectListItem() { Value = o.Id, Text = o.Title }), emptyTitle: "请选择");
            ViewBag.inboundType = EnumToSelect(typeof(InboundType));
            var curUserName = Sys.CurrentUser.UserName;
            var createUID = Sys.CurrentUser.UID;
            var model = new InboundGoods() { InboundType=1,Source=1 };
            var supplierTitle = "";
            if (!string.IsNullOrEmpty(inboundGoodsId))
            {
                model = InboundGoodsBLL.Find(o => o.InboundGoodsId == inboundGoodsId);
                if (model != null)
                {
                    createUID = model.CreateUID;
                    var user = UserInfoService.Find(o => o.UID == model.CreateUID);
                    if (user != null)
                        curUserName = user.FullName;

                    var supplierID = model.SupplierID;
                    if (supplierID == "-1")
                        supplierTitle = "其它";
                    else
                    {
                        var supp= SupplierService.Find(o => o.Id == supplierID);
                        supplierTitle = supp != null ? supp.Title : "";
                    }
                }
            }
            ViewBag.CurUserName = curUserName;
            ViewBag.CreateUID = createUID;
            ViewBag.SupplierTitle = supplierTitle;
            return View(model);
        }
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult SaveInboundGoods(InboundGoods obj)
        {
            var re = InboundGoodsBLL.SaveOrUpdate(obj);
            return Content(re.ToJson());
        }
        [HttpPost]
        public ActionResult IsNotExistIndentOrderId(string indentOrderId)
        {
            var re = new OpResult();
            if (!string.IsNullOrEmpty(indentOrderId))
            { 
                var inboundgood = InboundGoodsBLL.Find(o => o.IndentOrderId == indentOrderId);
                if (inboundgood == null)
                    re.Successed = true;
            }
            return new JsonNetResult(re);
        }
        [HttpPost]
        public ActionResult GetInboundListByInboundGoodsId(string inboundGoodsId)
        {
            int count = 0;
            var list = InboundGoodsBLL.GetInboundListByInboundGoodsId(inboundGoodsId, out count);
            return ToDataGrid(list, count);
        }
        #endregion

        #region 入库导入
        public ActionResult InboundImport()
        {
            var obj = BaseService<ImportSet>.Find(o => o.CompanyId == CommonService.CompanyId && o.TableName == "InboundGoods");
            return View(obj ?? new ImportSet());
        }
        [HttpPost]
        public ActionResult InboundImport(ImportSet imp)
        {
            imp.TableName = "InboundGoods";
            imp.CompanyId = CommonService.CompanyId;
            var op = InboundGoodsBLL.InboundImport(imp, Request.Files, Request["FieldName"], Request["ColumnName"]);
            return Content(op.ToJson());
        }
        #endregion

        #region 入库管理
        //入库管理
        public ActionResult InboundGoodsManagement()
        {
            ViewBag.users = ListToSelect(UserInfoService.GetList().Select(o => new SelectListItem() { Value = o.UID, Text = o.FullName }), emptyTitle: "全部");
            ViewBag.stores = ListToSelect(WarehouseService.GetList().Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title }), emptyTitle: "全部");

            Supplier supplierAppend = new Supplier() { Id = "-1", Title = "其它", ClassifyId = -1, FullTitle = "其它", Linkman = "qita", Designee = "qita", MasterAccount = "qita@163.com", BusinessType = 1 };
            var suppList = SupplierService.GetList();
            suppList.Add(supplierAppend);
            ViewBag.suppliers = ListToSelect(suppList.Where(o => o.BusinessType == 1).Select(o => new SelectListItem() { Value = o.Id, Text = o.FullTitle }), emptyTitle: "全部");
            //ViewBag.suppliers = ListToSelect(SupplierService.GetList().Where(o => o.BusinessType == 1).Select(o => new SelectListItem() { Value = o.Id, Text = o.Title }), emptyTitle: "全部");
            ViewBag.inboundState = EnumToSelect(typeof(InboundState), emptyTitle: "全部");
            return View();
        }

        [HttpPost]
        public ActionResult FindInboundList()
        {
            int count = 0;
            var list = InboundGoodsBLL.FindInboundGoodsList(Request.Params, out count);
            return ToDataGrid(list, count);
        }

        /// <summary>
        /// 修改入库状态为已验
        /// </summary>
        /// <param name="Ids">一组Id</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetInboundStateToChecked(string Ids)
        {
            var re = InboundGoodsBLL.SetInboundStateToChecked(Ids);
            return new JsonNetResult(re);
        }

        /// <summary>
        /// 删除入库单
        /// </summary>
        /// <param name="Ids">一组Id</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Delete(string[] Ids)
        {
            var re = InboundGoodsBLL.DeleteInboundById(Ids);
            return new JsonNetResult(re);
        }
        /// <summary>
        /// 打印入库单
        /// </summary>
        /// <returns></returns>
        public ActionResult InboundGoodsPrintView(string Ids)
        {
            List<PrintInboundModel> printModelList = new List<PrintInboundModel>();
            var inBounds = InboundGoodsBLL.GetPrintInboundGoods(Ids);
            foreach (var inBound in inBounds)
            {
                var store = WarehouseService.Find(o => o.StoreId == inBound.StoreId && o.CompanyId==CommonService.CompanyId);
                var supplier = SupplierService.Find(o => o.Id == inBound.SupplierID);
                Supplier noSupplier = new Supplier() { Id = "-1", Title = " ", ClassifyId = -1, FullTitle = "其它", Linkman = " ", Designee = " ", MasterAccount = "qita@163.com", BusinessType = 1 };
                supplier = (supplier == null ? noSupplier : supplier);
                var createUser = UserInfoService.Find(o => o.UID == inBound.CreateUID);
                PrintInboundModel printModel = new PrintInboundModel();
                printModel.InboundGood = inBound;
                printModel.StoreName = store == null ? "" : store.Title;
                printModel.Supplier = supplier;
                printModel.CreateUserName = createUser == null ? "" : createUser.FullName;
                printModelList.Add(printModel);
            }
            ViewBag.PrintModelList = printModelList;
            return View();
        }
        #endregion

        #region 退货登记
        public ActionResult Return(string returnId)
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList().Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title }));
            //ViewBag.suppliers = ListToSelect(SupplierService.GetList().Where(o => o.BusinessType == 1).Select(o => new SelectListItem() { Value = o.Id, Text = o.Title }));
            var curUserName = Sys.CurrentUser.UserName;
            var createUID = Sys.CurrentUser.UID;
            var model = new CommodityReturns();
            if (!string.IsNullOrEmpty(returnId))
            {
                model = CommodityReturnsBLL.Find(o => o.ReturnId == returnId);
                if (model != null)
                {
                    createUID = model.CreateUID;
                    var user = UserInfoService.Find(o => o.UID == model.CreateUID);
                    if (user != null)
                        curUserName = user.FullName;
                }
            }
            ViewBag.CurUserName = curUserName;
            ViewBag.CreateUID = createUID;
            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult SaveCommodityReturns(CommodityReturns obj)
        {
            var re = CommodityReturnsBLL.SaveOrUpdate(obj);
            return Content(re.ToJson());
        }

        //[HttpPost]
        //public ActionResult IsNotExistReturnId(string returnId)
        //{
        //    var re = new OpResult();
        //    var Return = CommodityReturnsBLL.Find(o => o.ReturnId == returnId);
        //    if (Return == null)
        //        re.Successed = true;
        //    return new JsonNetResult(re);
        //}

        [HttpPost]
        public ActionResult GetCommodityReturnsById(string returnId)
        {
            int count = 0;
            var list = CommodityReturnsBLL.GetCommodityReturnsById(returnId, out count);
            return ToDataGrid(list, count);
        }
       

        #endregion

        #region 退货导入
        public ActionResult ReturnImport()
        {
            var obj = BaseService<ImportSet>.Find(o => o.CompanyId == CommonService.CompanyId && o.TableName == "CommodityReturnsDetail");
            return View(obj ?? new ImportSet());
        }
        [HttpPost]
        public ActionResult ReturnImport(ImportSet imp)
        {
            imp.TableName = "CommodityReturnsDetail";
            imp.CompanyId = CommonService.CompanyId;
            var op = CommodityReturnsBLL.ReturnImport(imp, Request.Files, Request["FieldName"], Request["ColumnName"]);
            return Content(op.ToJson());
        }
        #endregion

        #region 退货管理
        public ActionResult ReturnManagement()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList().Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title }));

            ViewBag.returnstates = EnumToSelect(typeof(OrderReturnState), emptyTitle: "全部");
            return View();
        }
        [HttpPost]
        public ActionResult CommodityReturnList()
        {
            int count = 0;
            var list = CommodityReturnsBLL.CommodityReturnList(Request.Params, out count);
            return ToDataGrid(list, count);
        }

        /// <summary>
        /// 修改主表退货状态（及其明细表）
        /// </summary>
        /// <param name="Ids">一组Id</param>
        /// <param name="state">状态</param>
        /// <returns>修改后的列表</returns>
        [HttpPost]
        public ActionResult SetStates(string Ids, short state)
        {
            var re = CommodityReturnsBLL.SetStates(Ids, state);
            return new JsonNetResult(re);
        }

        /// <summary>
        /// 操作栏_修改明细表退货状态（及其主表）
        /// </summary>
        /// <param name="id">当前行id</param>
        /// <param name="state">要修改成这个状态</param>
        /// <returns>修改后的列表</returns>
        [HttpPost]
        public ActionResult setState_Editor(int id, short state)
        {
            var re = CommodityReturnsBLL.setState_Editor(id, state);
            return new JsonNetResult(re);
        }
        /// <summary>
        /// 打印退货单
        /// </summary>
        /// <returns></returns>
        public ActionResult ReturnPrintView(string Ids)
        {
            List<PrintReturnModel> printModelList = new List<PrintReturnModel>();
            var commodityReturns = CommodityReturnsBLL.GetPrintReturn(Ids);
            foreach (var commodityReturn in commodityReturns)
            {
                var store = WarehouseService.Find(o =>o.CompanyId==CommonService.CompanyId && o.StoreId == commodityReturn.StoreId);
                var supplier = SupplierService.Find(o => o.Id == commodityReturn.SupplierID);
                var createUser = UserInfoService.Find(o => o.UID == commodityReturn.CreateUID);
                PrintReturnModel printModel = new PrintReturnModel();
                printModel.CommodityReturn = commodityReturn;
                printModel.StoreName = store == null ? "" : store.Title;
                printModel.Supplier = supplier;
                printModel.CreateUserName = createUser == null ? "" : createUser.FullName;
                printModelList.Add(printModel);
            }
            ViewBag.PrintModelList = printModelList;
            return View();
        }
        #endregion

        #region 出库登记
        //出库登记
        public ActionResult OutboundGoods(string outboundId,byte? selectType)
        {
            //ViewBag.stores = ListToSelect(WarehouseService.GetList().Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title }));
            ViewBag.stores = ListToSelect(WarehouseService.GetList().Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title }), emptyTitle: "请选择");
            //ViewBag.applys = ListToSelect(WarehouseService.GetList().Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title }), emptyTitle: "请选择");
            //ViewBag.supplier = ListToSelect(SupplierService.GetList().Where(o => o.BusinessType == 2 && o.MasterState==1).Select(o => new SelectListItem() { Value = o.Id, Text = o.Title }), emptyTitle: "请选择");
            //ViewBag.users = ListToSelect(UserInfoService.GetList().Select(o => new SelectListItem() { Value = o.UID, Text = o.FullName }), emptyTitle: "请选择");
            ViewBag.channels = EnumToSelect(typeof(OutboundChannel),selectValue:selectType.GetValueOrDefault());
            ViewBag.outboundType = EnumToSelect(typeof(OutboundType));

            var curUserName = Sys.CurrentUser.UserName;
            var curUID = Sys.CurrentUser.UID;
            var model = new OutboundGoods() { OutboundType=1 };
            if (!string.IsNullOrEmpty(outboundId))
            {
                model = OutboundGoodsBLL.Find(o => o.OutboundId == outboundId);
                if (model != null)
                {
                    curUID = model.OperatorUID;
                    var user = UserInfoService.Find(o => o.UID == model.OperatorUID);
                    if (user != null)
                        curUserName = user.FullName;
                }
            }
            ViewBag.applyOrg = model.Channel == 0 ? ViewBag.applys : ViewBag.supplier;        
            ViewBag.CurUserName = curUserName;
            ViewBag.CurUID = curUID;

            return View(model);
        }

        /// <summary>
        /// 出货清单商品自动填入
        /// </summary>
        /// <param name="searchName">条码关键字</param>
        /// <param name="storeId">出货仓库</param>
        /// <returns>出货仓库中与条码关键字相关的商品列表</returns>
        [HttpPost]
        public ActionResult GetProductFromCommodity(string searchName, string storeId, string supplierID, short? type, short? nature, string applyorgId)
        {
            if (searchName.IsNullOrEmpty()) return new EmptyResult();
            var express = DynamicallyLinqHelper.Empty<VwProduct>();
            express = express.And(o => (o.Barcode != null && o.Barcode.StartsWith(searchName)) || o.Title.Contains(searchName) || (o.Barcodes != null && o.Barcodes.Contains(searchName)))
                .And(o => o.Nature == nature.Value, !nature.HasValue).And(o=>o.CompanyId==CommonService.CompanyId);
                
            if (!supplierID.IsNullOrEmpty())
            {
                var sp = supplierID.Split(',').ToList();
                var bars = BaseService<ProductMultSupplier>.FindList(o => sp.Contains(o.SupplierId)).Select(o => o.Barcode).Distinct().ToList();
                express = express.And(o => (o.SupplierId == supplierID || bars.Contains(o.Barcode)), supplierID.IsNullOrEmpty());
            }
            if (!storeId.IsNullOrEmpty())
            {
                var ware = WarehouseService.Find(o => o.StoreId == storeId && o.CompanyId == CommonService.CompanyId);
                if (ware != null)
                {
                    var categorySNs = ware.CategorySN.Split(',').Select(o => int.Parse(o)).ToList();
                    var childs = ProductCategoryService.GetChildSNs(categorySNs,true);
                    express = express.And(o => childs.Contains(o.CategorySN));
                }
            }
            var list = BaseService<VwProduct>.FindList(express, takeNum: 20);
            list = ProductService.SetAssistBarcode(list);
            if (type == 1)
            {
                OutboundGoodsBLL.SetTradePrice<VwProduct>(applyorgId, list, type.GetValueOrDefault(), supplierID);
            }
            else
            {
               ProductService.SetSysPrice<VwProduct>(storeId, list, type.GetValueOrDefault(),supplierID);
            }
            return ToDataGrid(list, 20);
        }


        /// <summary>
        /// 报损清单商品自动填入
        /// </summary>
        /// <param name="searchName">条码关键字</param>
        /// <param name="storeId">出货仓库</param>
        /// <returns>出货仓库中与条码关键字相关的商品列表</returns>
        //  [HttpPost]
      //  public ActionResult GetProductFromBreak(string searchName, string storeId, string supplierID, short? type, short? nature)
      //{
      //      if (searchName.IsNullOrEmpty()) return new EmptyResult();
      //      var express = DynamicallyLinqHelper.Empty<VwProduct>();
      //      express = express.And(o => (o.Barcode != null && o.Barcode.StartsWith(searchName)) || o.Title.Contains(searchName) || (o.Barcodes != null && o.Barcodes.Contains(searchName)))
      //          .And(o => o.Nature == nature.Value, !nature.HasValue);

      //      if (!supplierID.IsNullOrEmpty())
      //      {
      //          var sp = supplierID.Split(',').ToList();
      //          var bars = BaseService<ProductMultSupplier>.FindList(o => sp.Contains(o.SupplierId)).Select(o => o.Barcode).Distinct().ToList();
      //          express = express.And(o => (o.SupplierId == supplierID || bars.Contains(o.Barcode)), supplierID.IsNullOrEmpty());
      //      }
      //      if (!storeId.IsNullOrEmpty())
      //      {
      //          var ware = WarehouseService.Find(o => o.StoreId == storeId);
      //          if (ware != null)
      //          {
      //              var categorySNs = ware.CategorySN.Split(',').Select(o => int.Parse(o)).ToList();
      //              var childs = ProductCategoryService.GetChildSNs(categorySNs);
      //              express = express.And(o => childs.Contains(o.CategorySN));
      //          }
      //      }
            
      //      var list = ProductService.FindProductInvent(express,storeId);
      //      list = ProductService.SetAssistBarcode(list);
      //      ProductService.SetSysPrice<VwProduct>(storeId, list, type.GetValueOrDefault());
      //      return ToDataGrid(list, 20);
      //  }

        /// <summary>
        /// 出库管理-提货单位，联动下拉
        /// </summary>
        /// <param name="id"></param>
        /// <param name="showTitle"></param>
        /// <returns></returns>
        public ActionResult ParentTypeSelect(int? id, short? showTitle,string value)
        {           
            var childtypes = new List<DropdownItem>();
            if (id == 0)
            {
                Warehouse warehouseAppend = new Warehouse() { StoreId = "-1", Title = "其它", CategorySN = "1,2,3,45", CreateUID = "qita" };
                var warehouseList = WarehouseService.GetList();
                warehouseList.Add(warehouseAppend);
                childtypes = warehouseList.Select(o => new DropdownItem(o.StoreId, o.Title)).ToList();               
                //childtypes = WarehouseService.GetList().Select(o => new DropdownItem(o.StoreId, o.Title)).ToList();               
            }
            if (id == 1)
            {
                Supplier wholesalerAppend = new Supplier() { Id = "-1", Title = "其它", ClassifyId = -1, FullTitle = "qita", Linkman = "qita", Designee = "qita", MasterAccount = "qita@163.com", BusinessType = 2 };
                var wholesalerList = SupplierService.GetWholesalerList();
                wholesalerList.Add(wholesalerAppend);
                childtypes = wholesalerList.Select(o => new DropdownItem(o.Id, o.Title)).ToList();   
                //childtypes = SupplierService.GetWholesalerList().Select(o => new DropdownItem(o.Id, o.Title)).ToList();             
            }
            if (showTitle == 1)
            {
                childtypes.Insert(0, new DropdownItem("", "请选择", true));                          
            }
            if (!value.IsNullOrEmpty() && childtypes.Any(o => o.Value == value))
            {
                childtypes.Each(o => { o.IsSelected = o.Value == value; });
            }
            return new JsonNetResult(childtypes);
        }

        /// <summary>
        /// 出库登记-提货单位，联动下拉
        /// </summary>
        /// <param name="id"></param>
        /// <param name="showTitle"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ActionResult applyParentTypeSelect(int? id, short? showTitle, string value)
        {
            var childtypes = new List<DropdownItem>();
            if (id == 0)
            {
                childtypes = WarehouseService.GetList().Select(o => new DropdownItem(o.StoreId, o.Title)).ToList();               
            }
            if (id == 1)
            {
                childtypes = SupplierService.GetWholesalerList().Select(o => new DropdownItem(o.Id, o.Title)).ToList();             
            }
            if (showTitle == 1)
            {
                childtypes.Insert(0, new DropdownItem("", "请选择", true));
            }
            if (!value.IsNullOrEmpty() && childtypes.Any(o => o.Value == value))
            {
                childtypes.Each(o => { o.IsSelected = o.Value == value; });
            }
            return new JsonNetResult(childtypes);
        }
           
        /// <summary>
        /// 保存出货清单
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        [HttpPost]
        [SysPermissionValidate(Code = Sys.SysConstLimits.库存管理_出入库登记)]
        public ActionResult SaveOutboundGoods(OutboundGoods obj)
        {
            var re = OutboundGoodsBLL.SaveOrUpdate(obj);
            return Content(re.ToJson());
        }

        [HttpPost]
        public ActionResult GetOutboundListByOutboundId(string outboundId)
        {
            int count = 0;
            var list =outboundId.IsNullOrEmpty()?null:OutboundGoodsBLL.GetOutboundListByOutboundId(outboundId, out count);
            return ToDataGrid(list, count);
        }
        #endregion

        #region 出库导入
        public ActionResult OutboundImport()
        {
            var obj = BaseService<ImportSet>.Find(o => o.CompanyId == CommonService.CompanyId && o.TableName == "OutboundGoods");
            return View(obj ?? new ImportSet());
        }
        [HttpPost]
        public ActionResult OutboundImport(ImportSet imp)
        {
            imp.TableName = "OutboundGoods";
            imp.CompanyId = CommonService.CompanyId;
            var op = OutboundGoodsBLL.OutboundImport(imp, Request.Files, Request["FieldName"], Request["ColumnName"]);
            return Content(op.ToJson());
        }
        #endregion

        #region 出库管理
        //出库管理
        public ActionResult OutboundGoodsRecord(byte? selectType)
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList().Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title }), emptyTitle: "全部");
            //ViewBag.users = ListToSelect(UserInfoService.GetList().Select(o => new SelectListItem() { Value = o.UID, Text = o.FullName }), emptyTitle: "全部");
            //ViewBag.supplier = ListToSelect(SupplierService.GetList().Select(o => new SelectListItem() { Value = o.Id, Text = o.Title }), emptyTitle: "请选择");
            ViewBag.channels = EnumToSelect(typeof(OutboundChannel),selectValue:selectType.GetValueOrDefault());
            return View();
        }

        [HttpPost]
        public ActionResult FindOutboundList()
        {
            int count = 0;
            var list = OutboundGoodsBLL.FindOutboundGoodsList(Request.Params, out count);
            return ToDataGrid(list, count);
        }

        /// <summary>
        /// 删除出库单
        /// </summary>
        /// <param name="Ids">一组Id</param>
        /// <returns></returns>
        [HttpPost]
        [SysPermissionValidate(Code = Sys.SysConstLimits.库存管理_移除出入库)]
        public ActionResult DeleteOutbounds(string[] Ids)
        {
            var re = OutboundGoodsBLL.DeleteOutboundById(Ids);
            return new JsonNetResult(re);
        }

        /// <summary>
        /// 修改出库状态为已审
        /// </summary>
        /// <param name="Ids">一组Id</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetOutboundStateToChecked(string Ids)
        {
            var re = OutboundGoodsBLL.SetOutboundStateToChecked(Ids);
            return new JsonNetResult(re);
        }
        /// <summary>
        /// 判断出库单是否库存充足
        /// </summary>
        /// <param name="Ids">一组出库单Id</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult IsOutboundHasCommodity(string Ids)
        {
            var re = OutboundGoodsBLL.IsOutboundHasCommodity(Ids);
            return new JsonNetResult(re);
        }

        /// <summary>
        /// 打印出库单
        /// </summary>
        /// <returns></returns>
        public ActionResult OutboudsGoodsPrintView(string Ids)
        {
            List<PrintOutboundModel> printModelList = new List<PrintOutboundModel>();
            var outBounds = OutboundGoodsBLL.GetPrintOutboundGoods(Ids);
            foreach (var outBound in outBounds)
            {
                string applyOrgTitle = string.Empty;
                var suppiler = new Supplier();
                var pickingStore = new Warehouse();
                var store = WarehouseService.Find(o =>o.CompanyId==CommonService.CompanyId && o.StoreId == outBound.StoreId);
                if (outBound.Channel == 1)
                {
                    suppiler = SupplierService.Find(o => o.Id == outBound.ApplyOrgId);
                    applyOrgTitle = suppiler == null ? "其它" : suppiler.Title;
                }
                if (outBound.Channel == 0)
                {
                    pickingStore = WarehouseService.Find(o => o.CompanyId == CommonService.CompanyId && o.StoreId == outBound.ApplyOrgId);
                    applyOrgTitle = pickingStore == null ? "其它" : pickingStore.Title;
                }
                var createUser = UserInfoService.Find(o => o.UID == outBound.OperatorUID);
                PrintOutboundModel printModel = new PrintOutboundModel();
                printModel.OutboundGood = outBound;
                printModel.StoreName = store == null ? "" : store.Title;
                printModel.ApplyOrgTitle = applyOrgTitle;
                printModel.CreateUserName = createUser == null ? "" : createUser.FullName;
                printModelList.Add(printModel);
            }
            ViewBag.PrintModelList = printModelList;
            return View();
        }
        #endregion

        #region 批发管理
        public ActionResult WholesaleRegister(string outboundId)
        {
            return RedirectToAction("OutboundGoods", new { outboundId = outboundId, selectType = 1 });
        }
        public ActionResult WholesaleRecord()
        {
            return RedirectToAction("OutboundGoodsRecord", new { selectType = 1 });
        }
        #endregion

        #region 报损登记
        //报损登记
        public ActionResult BreakageGoods(string breakageGoodsId)
        {
            ViewBag.shops = ListToSelect(WarehouseService.GetList().Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title }), emptyTitle: "请选择");
            ViewBag.users = ListToSelect(UserInfoService.GetList().Select(o => new SelectListItem() { Value = o.UID, Text = o.FullName }), emptyTitle: "请选择");
            ViewBag.breakageType = EnumToSelect(typeof(BreakageType));
            var curUserName = Sys.CurrentUser.UserName;
            var createUID = Sys.CurrentUser.UID;
            var model = new BreakageGoods();
            if (!string.IsNullOrEmpty(breakageGoodsId))
            {
                model = BreakageGoodsBLL.Find(o => o.BreakageGoodsId == breakageGoodsId);
                if (model != null)
                {
                    createUID = model.OperatorUID;
                    var user = UserInfoService.Find(o => o.UID == model.OperatorUID);
                    if (user != null)
                        curUserName = user.FullName;
                }
            }
            ViewBag.CurUserName = curUserName;
            ViewBag.CreateUID = createUID;
            return View(model);
        }
        /// <summary>
        /// 保存报损清单
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult SaveBreakageGoods(BreakageGoods obj)
        {
            var re = BreakageGoodsBLL.SaveOrUpdate(obj);
            return Content(re.ToJson());
        }

        [HttpPost]
        public ActionResult GetBreakageListByBreakageGoodId(string breakageGoodsId)
        {
            int count = 0;
            var list = BreakageGoodsBLL.GetBreakageListByBreakageGoodId(breakageGoodsId, out count);
            return ToDataGrid(list, count);
        }
        #endregion

        #region 报损导入
        public ActionResult BreakageImport()
        {
            var obj = BaseService<ImportSet>.Find(o => o.CompanyId == CommonService.CompanyId && o.TableName == "BreakageGoods");
            return View(obj ?? new ImportSet());
        }
        [HttpPost]
        public ActionResult BreakageImport(ImportSet imp)
        {
            imp.TableName = "BreakageGoods";
            imp.CompanyId = CommonService.CompanyId;
            var op = BreakageGoodsBLL.BreakageImport(imp, Request.Files, Request["FieldName"], Request["ColumnName"]);
            return Content(op.ToJson());
        }
        #endregion

        #region 报损管理
        //报损管理
        public ActionResult BreakageGoodsRecord()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList().Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title }), emptyTitle: "全部");
            ViewBag.users = ListToSelect(UserInfoService.GetList().Select(o => new SelectListItem() { Value = o.UID, Text = o.FullName }), emptyTitle: "全部");
            ViewBag.breakageType = EnumToSelect(typeof(BreakageType), emptyTitle: "全部");
            ViewBag.breakageState = EnumToSelect(typeof(BreakageState), emptyTitle: "全部");
            return View();
        }

        [HttpPost]
        public ActionResult FindBreakageGoodList()
        {
            int count = 0;
            var list = BreakageGoodsBLL.FindBreakageGoodsList(Request.Params, out count);
            return ToDataGrid(list, count);
        }

        /// <summary>
        /// 删除报损单
        /// </summary>
        /// <param name="Ids">一组Id</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteBreakageGoods(string[] Ids)
        {
            var re = BreakageGoodsBLL.DeleteBreakageGoodById(Ids);
            return new JsonNetResult(re);
        }

        /// <summary>
        /// 修改报损单状态为已审
        /// </summary>
        /// <param name="Ids">一组Id</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetBreakageGoodsStateToChecked(string Ids)
        {
            var re = BreakageGoodsBLL.SetBreakageGoodsStateToChecked(Ids);
            return new JsonNetResult(re);
        }
        #endregion

        #region 库存查询
        //库存查询
        public ActionResult QueryInventory()
        {
            ViewBag.stores = ListToSelect(WarehouseService.GetList().Select(o => new SelectListItem() { Value = o.StoreId, Text = o.Title }), emptyTitle: "全部");
            //ViewBag.suppliers = ListToSelect(SupplierService.GetList().Where(o => o.BusinessType == 1).Select(o => new SelectListItem() { Value = o.Id, Text = o.FullTitle }), emptyTitle: "全部");
            ViewBag.brands = ListToSelect(ProductBrandService.GetList().Select(o => new SelectListItem() { Value = o.BrandSN.ToString(), Text = o.Title }), emptyTitle: "全部");
            return View();
        }
        [HttpPost]
        public ActionResult QueryInventoryPageList()
        {
            int count = 0;
            object footer=null;
            var list = CommodityService.QueryInventoryPageList(Request.Params, out count, ref footer);
            return ToDataGrid(list, count,footer);
        }
        public ActionResult Detail()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DetailList()
        {
            int count = 0;
            var list = CommodityService.DetailPageList(Request.Params, out count);
            return ToDataGrid(list, count);
        }
        public void Export()
        {
            var nvl = new System.Collections.Specialized.NameValueCollection() { Request.Params };
            nvl["ispage"] = "0";
            int count = 0;
            object footer = null;
            var dt = CommodityService.QueryInventoryPageList(nvl, out count, ref footer);
            var dv= dt.DefaultView;
            //dv.Sort = "Id desc";
            List<string> fields = new List<string>() { "ProductCode", "Barcode", "CategoryTitle", "Title", "Size", "SubUnit", "BalanceDate", "StockNumber", "StockAmount", "SaleAveragePrice", "SaleAmount" };
            List<string> names = new List<string>() { "货号", "商品条码", "品类", "商品名称", "规格", "单位", "结余日期", "库存量", "库存金额", "平均售价", "销售金额" };
            if(Sys.CurrentUser.IsStore)
            {
                //fields.Add("DiscountPrice");
                //names.Add("促销价");
                //fields.Add("StateTitle2");
                //names.Add("活动状态");
            }
            else 
            {
                //if (!Request["store"].IsNullOrEmpty())
                //{
                //    fields.Add("DiscountPrice");
                //    names.Add("促销价");
                //}
                //fields.Add("StateTitle");
                //names.Add("状态");
            }
            var totalCols = new int[] {  7,8, 10 };
            if (nvl["StartDate"]== nvl["EndDate"])
            {
                fields.Remove("BalanceDate");
                names.Remove("结余日期");
                totalCols = new int[] { 6,7, 9 };
            }
            new ExportExcel() { IsBufferOutput = true, HeaderText = "商品库存信息" }.ToExcel("商品库存", dv.ToTable(), fields.ToArray(), names.ToArray(), null, totalCols);
        }
        #endregion

        #region 门店维护
        //仓库维护
        public ActionResult StorageMaintenance()
        {
            ViewBag.states = ListToSelect(new List<SelectListItem>() { new SelectListItem() { Value = "1", Text = "经营"}, new SelectListItem() { Value = "0", Text = "停业" } }, emptyTitle: "请选择");
            return View();
        }
        [HttpPost]
        public ActionResult FindStoragePageList()
        {
            int count = 0;
            var list = WarehouseService.FindStoragePageList(Request.Params, out count);
            return ToDataGrid(list, count);
        }

        /// <summary>
        /// 新增门店
        /// </summary>
        /// <returns></returns>
        public ActionResult AddStorage(int? id)
        {

            ViewBag.categorys = ProductCategoryService.GetParentTypes().Select(o => new SelectListItem()
            {
                Text = o.Title,
                Value = o.CategorySN.ToString()
            }).ToList();
            if (id != null)
            {
                var model = WarehouseService.FindById(id);
                return View(model);
            }
            return View(new Logic.Entity.Warehouse());
        }
        [HttpPost]
        public ActionResult AddStorage(Warehouse obj)
        {
            var re = new OpResult();
            if(WarehouseService.IsExist(o=>o.CompanyId==CommonService.CompanyId && o.Title==obj.Title && o.Id!=obj.Id))
                re.Message="门店名称重复";
            else if (obj.Id == 0)
            {
                var auth = Pharos.Sys.CurrentUser.Company as CompanyAuthorize;
                if (auth != null && auth.StoreNum > 0)
                {
                    if (WarehouseService.FindList(o =>o.CompanyId==CommonService.CompanyId).Count > auth.StoreNum)
                    {
                        re.Message = "门店数超过允许的数量,不能再添加!";
                        return Content(re.ToJson());
                    }
                }
                obj.CreateDT = DateTime.Now;
                obj.CreateUID = Sys.CurrentUser.UID;
                obj.State = 1;
                obj.StoreId = WarehouseService.MaxSn;
                obj.CategorySN = Request["CategorySN"];
                obj.CompanyId = CommonService.CompanyId;
                re = WarehouseService.Add(obj);
            }
            else
            {
                var resoure = WarehouseService.FindById(obj.Id);
                resoure.Title = obj.Title;
                resoure.Address = obj.Address;
                resoure.AdminState = obj.AdminState;
                resoure.CategorySN = Request["CategorySN"];
                re = WarehouseService.Update(resoure);
                    
            }
            return Content(re.ToJson());
        }
        [HttpPost]
        public ActionResult SetState(string Ids, short state,short type)
        {
            var ids = Ids.Split(',').Select(o => int.Parse(o));
            var list = WarehouseService.FindList(o => ids.Contains(o.Id));
            list.ForEach(o => {
                if (type == 1)
                    o.State = state;
                else if (type == 2)
                    o.AdminState = state;
            });
            var re = WarehouseService.Update(list);
            return new JsonNetResult(re);
        }
        #endregion

        #region 登陆
        public ActionResult Login(string id)
        {
            /*var user = new UserLogin();
            csID csid2 = ipLocalhost();

            //url中cid
            string s_cid = "";

            if (!RouteData.Values["cid"].IsNullOrEmpty())
            {
                s_cid = RouteData.Values["cid"].ToString();
            }
            //url中sid
            string s_sid = "";

            if (!RouteData.Values["sid"].IsNullOrEmpty())
            {
                s_sid = RouteData.Values["sid"].ToString();
            }

            if (csid2.message == "禁止访问")
            {
                Response.Redirect("/Account/noBusiness");
                return null;
            }

            csID csid = new csID();
            if (!s_cid.IsNullOrEmpty() && !s_sid.IsNullOrEmpty())
            {
                Authorize authorize = new Authorize();
                csid = authorize.getCidSid(s_cid, s_sid);
                if (csid.message == "格式错误")
                {
                    Response.Redirect("/Account/noBusiness");
                    return null;
                }
                else if (csid.message == "域名的store后面必须是数字")
                {
                    Response.Redirect("/Account/noBusiness");
                    return null;
                }
                else if (csid.message == "success")
                {
                    user.CID = Convert.ToInt32(csid.cid);
                    if (Cookies.IsExist("remuc"))
                    {
                        //cookie的CID
                        string cid = Cookies.Get("remuc", "_cid").Trim();
                        //cookie的门店ID
                        string sid = Server.UrlDecode(Cookies.Get("remuc", "_storeId"));
                        sid = sid.Split('~')[0];

                        if (cid.IsNullOrEmpty())
                        {
                            cid = "0";
                        }
                        if (sid.IsNullOrEmpty())
                        {
                            sid = "0";
                        }

                        if (csid.cid.Trim() == cid || csid.sid.Trim() == sid)
                        {
                            user.UserName = Cookies.Get("remuc", "_uname");
                            user.UserPwd = Cookies.Get("remuc", "_pwd");
                            user.StoreId = Server.UrlDecode(Cookies.Get("remuc", "_storeId"));
                            user.RememberMe = true;
                        }
                    }
                }
            }
            else
            {
                Response.Redirect("/Account/noBusiness");
                return null;
            }
            

            List<SelectListItem> list = ListToSelect(WarehouseService.GetAdminList(Convert.ToInt32(csid.cid), csid.sid).Select(o => new SelectListItem() { Text = o.Title, Value = o.StoreId + "~" + o.Title, Selected = o.StoreId == csid.sid })).ToList();
            if (list.Count == 0)
            {
                Response.Redirect("/Account/error?msg=" + Pharos.Utility.DESEncrypt.Encrypt("无效门店，请联系管理员检查该门店是否存在或开放！"));
                return null;
            }
            

            ViewBag.stores = list;
            return View(user);
            */

            var user = new UserLogin();
            user.CID = Pharos.Utility.Config.GetAppSettings("CompanyId").ToType<int>();
            user.StoreId = Sys.SysCommonRules.CurrentStore;
            ViewBag.stores = ListToSelect(WarehouseService.GetAdminList().Select(o => new SelectListItem() { Text = o.Title, Value = o.StoreId + "~" + o.Title, Selected = o.StoreId == user.StoreId }));
            if (Cookies.IsExist("storeremuc"))
            {
                user.UserName = Cookies.Get("storeremuc", "_uname");
                user.StoreId = Server.UrlDecode(Cookies.Get("storeremuc", "_storeId"));
                user.RememberMe = true;
            }
            return View(user);

        }
        [HttpPost]
        public ActionResult Login(UserLogin user)
        {
            if (!ModelState.IsValid) return View(user);
            var obj = UserInfoService.GetStoreUserBy(user.UserName,Pharos.Utility.Security.MD5_Encrypt(user.UserPwd), user.StoreId.Split('~')[0]);
            if (obj == null)
            {
                ViewBag.msg = "帐户或密码输入不正确!";
                //ViewBag.stores = ListToSelect(WarehouseService.GetAdminList().Select(o => new SelectListItem() { Text = o.Title, Value = o.StoreId + "~" + o.Title }), emptyTitle: "请选择门店");
                ViewBag.stores = ListToSelect(WarehouseService.GetAdminList(user.CID, user.StoreId.Split('~')[0]).Select(o => new SelectListItem() { Text = o.Title, Value = o.StoreId + "~" + o.Title, Selected = o.StoreId == user.StoreId.Split('~')[0] }));
                return View(user);
            }
            var op = Authorize.HasRegister();
            if (op.Successed && Pharos.Sys.CurrentUser.Company != null && Pharos.Sys.CurrentUser.Company.StoreProper == "N")
            {
                ViewBag.msg = "该门店系统未开启授权！";
               // ViewBag.stores = ListToSelect(WarehouseService.GetAdminList().Select(o => new SelectListItem() { Text = o.Title, Value = o.StoreId + "~" + o.Title }), emptyTitle: "请选择门店");
                ViewBag.stores = ListToSelect(WarehouseService.GetAdminList(user.CID, user.StoreId.Split('~')[0]).Select(o => new SelectListItem() { Text = o.Title, Value = o.StoreId + "~" + o.Title, Selected = o.StoreId == user.StoreId.Split('~')[0] }));
                return View(user);
            }
            //obj.StoreId = user.StoreId;
            obj.RoleIds = "10";
            new Sys.CurrentStoreUser().StoreLogin(obj,user.StoreId, user.RememberMe);

            //csID csid = ipLocalhost();
            //if (csid.message == "禁止访问")
            //{
            //    Response.Redirect("/Account/noBusiness");
            //    return null;
            //}
            ////ip、localhost门店登录
            //else if (csid.message == "localhost" || csid.message == "ip")
            //{
            //    Response.Redirect("/store"+csid.cid+"-"+csid.sid+"/Store/Index");
            //    return null;
            //}

            return Redirect(Url.Action("Index"));
        }
        public ActionResult Logout()
        {
            Sys.CurrentUser.Exit();
            /*
            if (Request["isLocalhostIp"] != null && Request["cid"] != null && Request["sid"] != null)
            {
                //ip、localhost门店登录
                if (Request["isLocalhostIp"].Trim() == "1")
                {
                    Response.Redirect("/store" + Request["cid"].Trim() + "-" + Request["sid"].Trim());
                    return null;
                }
            }
            Response.Redirect("/");
            return null;*/

            return RedirectToAction("Login");
        }

        /// <summary>
        /// ip门店登录、localhost门店登录
        /// </summary>
        /// <returns></returns>
        public csID ipLocalhost()
        {
            //url中localhost
            string localhost = "";
            if (!RouteData.Values["localhost"].IsNullOrEmpty())
            {
                localhost = RouteData.Values["localhost"].ToString();
            }

            //url中cid
            string cid = "";
            if (!RouteData.Values["cid"].IsNullOrEmpty())
            {
                cid = RouteData.Values["cid"].ToString();
            }

            //url中sid
            string sid = "";
            if (!RouteData.Values["sid"].IsNullOrEmpty())
            {
                sid = RouteData.Values["sid"].ToString();
            }

            //url中ip1
            string ip1 = "";
            if (!RouteData.Values["ip1"].IsNullOrEmpty())
            {
                ip1 = RouteData.Values["ip1"].ToString();
            }

            //url中ip2
            string ip2 = "";
            if (!RouteData.Values["ip2"].IsNullOrEmpty())
            {
                ip2 = RouteData.Values["ip2"].ToString();
            }

            //url中ip3
            string ip3 = "";
            if (!RouteData.Values["ip3"].IsNullOrEmpty())
            {
                ip3 = RouteData.Values["ip3"].ToString();
            }

            //url中ip4
            string ip4 = "";
            if (!RouteData.Values["ip4"].IsNullOrEmpty())
            {
                ip4 = RouteData.Values["ip4"].ToString();
            }

            Authorize authorize = new Authorize();
            return authorize.ipLocalhostStore(localhost, ip1, ip2, ip3, ip4, cid, sid);
        }

        #endregion

        #region 门店自动完成
        /// <summary>
        /// 输入自动完成商品
        /// </summary>
        /// <param name="searchName"></param>
        /// <param name="supplierID"></param>
        /// <returns></returns>
        public ActionResult GetStoreInput(string searchName, int? bigId)
        {
            var category = bigId.HasValue ? bigId.ToString() : "";
            var express = DynamicallyLinqHelper.Empty<Warehouse>()
                .And(o=>o.CompanyId==CommonService.CompanyId)
                .And(o => (o.StoreId.StartsWith(searchName)) || o.Title.Contains(searchName),searchName.IsNullOrEmpty())
                .And(o => (","+o.CategorySN+",").Contains(","+category+","), !bigId.HasValue);

            var list = BaseService<Warehouse>.FindList(express);
            return ToDataGrid(list, 0);
        }
        [HttpPost]
        public ActionResult GetStoreList(bool? showAll, string emptyTitle)
        {
            var list= WarehouseService.GetList(showAll.GetValueOrDefault()).Select(o => new DropdownItem() { Value = o.StoreId, Text = o.Title, IsSelected = Sys.CurrentUser.StoreId == o.StoreId }).ToList();
            if (!emptyTitle.IsNullOrEmpty())
            {
                var isSel = list.Any(o => o.IsSelected);
                list.Insert(0, new DropdownItem("", emptyTitle, !isSel));
            }
            return new JsonNetResult(list);
        }

        #endregion
    }
}
