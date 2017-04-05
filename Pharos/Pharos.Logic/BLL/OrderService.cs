﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Pharos.Logic.Entity;
using Pharos.Utility;
using Pharos.Utility.Helpers;
using Pharos.Sys.Entity;
namespace Pharos.Logic.BLL
{
    public class OrderService : BaseService<IndentOrder>
    {
        /// <summary>
        /// 保存或修改
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static OpResult SaveOrUpdate(IndentOrder obj)
        {
            var re = new OpResult();
            try
            {
                var details = new List<IndentOrderList>();
                var gifts = new List<IndentOrderList>();
                if (!string.IsNullOrWhiteSpace(obj.Inserted))
                {
                    var adds = obj.Inserted.ToObject<List<IndentOrderList>>();
                    if (adds.Any())
                    {
                        details.AddRange(adds.Where(o => !string.IsNullOrWhiteSpace(o.Barcode)));
                        details.Each(o =>
                        {
                            if (!o.Gift.IsNullOrEmpty())
                            {
                                var gfs = o.Gift.Split(',');
                                gfs.Each(i =>
                                {
                                    if (!i.IsNullOrEmpty())
                                    {
                                        gifts.Add(new IndentOrderList()
                                        {
                                            Barcode = i.Split('~')[0],
                                            IndentNum = decimal.Parse(i.Split('~')[1]),
                                            Nature = 1,
                                            ResBarcode = o.Barcode,
                                            Memo = "赠品"
                                        });
                                    }
                                });
                            }
                        });
                    }
                }
                var gifBars = gifts.Select(o => o.Barcode).ToList();
                if (gifBars.Any())
                {
                    var pros = ProductService.FindList(o => gifBars.Contains(o.Barcode));
                    ProductService.SetSysPrice(obj.StoreId, pros, supplierId: obj.SupplierID);
                    gifts.Each(o =>
                    {
                        var p = pros.FirstOrDefault(i => i.Barcode == o.Barcode);
                        if (p != null)
                        {
                            o.Price = p.BuyPrice;
                            o.Subtotal = o.Price * o.IndentNum;
                            o.SysPrice = p.SysPrice;
                        }
                    });
                }
                details.AddRange(gifts);
                if (details.Any())
                {
                    var procs = ProductService.FindList(o => !(o.Barcodes == null || o.Barcodes == ""));
                    foreach (var dt in details)
                    {
                        var p = procs.FirstOrDefault(o => ("," + o.Barcodes + ",").Contains("," + dt.Barcode + ","));
                        if (p == null) continue;
                        dt.AssistBarcode = dt.Barcode;
                        dt.Barcode = p.Barcode;
                    }
                }
                obj.CompanyId = CommonService.CompanyId;
                if (obj.Id == 0)
                {
                    obj.IndentOrderId = Logic.CommonRules.OrderSN;
                    details.Each(o =>
                    {
                        o.IndentOrderId = obj.IndentOrderId;
                    });
                    obj.CreateDT = DateTime.Now;
                    obj.CreateUID = Sys.CurrentUser.UID;
                    Add(obj, false);
                    re = BaseService<IndentOrderList>.AddRange(details);
                }
                else
                {
                    var sour = OrderService.FindById(obj.Id);
                    var list = BaseService<IndentOrderList>.FindList(o => o.IndentOrderId == sour.IndentOrderId);
                    var uid = sour.CreateUID;
                    var create = sour.CreateDT;
                    obj.ToCopyProperty(sour);
                    sour.CreateDT = create;
                    sour.CreateUID = uid;
                    details.Each(o =>
                    {
                        o.IndentOrderId = sour.IndentOrderId;
                    });
                    if (!string.IsNullOrWhiteSpace(obj.Deleted))
                    {
                        var dels = obj.Deleted.ToObject<List<IndentOrderList>>();
                        if (dels.Any())
                        {
                            var ids = dels.Select(o => o.Id).ToList();
                            var mainBars = dels.Select(o => o.Barcode).ToList();
                            var deletes = list.Where(o => ids.Contains(o.Id)).ToList();
                            deletes.AddRange(list.Where(o => mainBars.Contains(o.ResBarcode)));
                            BaseService<IndentOrderList>.CurrentRepository.RemoveRange(deletes, false);
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(obj.Updated))
                    {
                        var upts = obj.Updated.ToObject<List<IndentOrderList>>();
                        //foreach (var detail in list)
                        //{
                        //    //detail.Subtotal = detail.IndentNum * detail.Price;
                        //    var up = upts.FirstOrDefault(o => o.Id == detail.Id);
                        //    if (up == null) continue;
                        //    detail.IndentNum = up.IndentNum;
                        //    detail.Memo = up.Memo;
                        //}
                        if (upts.Any())
                        {
                            var ids = upts.Select(o => o.Id).ToList();
                            var mainBars = upts.Select(o => o.Barcode).ToList();
                            var deletes = list.Where(o => ids.Contains(o.Id)).ToList();
                            deletes.AddRange(list.Where(o => mainBars.Contains(o.ResBarcode)));
                            upts.Each(o =>
                            {
                                if (!o.Gift.IsNullOrEmpty())
                                {
                                    var gfs = o.Gift.Split(',');
                                    gfs.Each(i =>
                                    {
                                        if (!i.IsNullOrEmpty())
                                        {
                                            gifts.Add(new IndentOrderList()
                                            {
                                                IndentOrderId = o.IndentOrderId,
                                                Barcode = i.Split('~')[0],
                                                IndentNum = decimal.Parse(i.Split('~')[1]),
                                                Nature = 1,
                                                ResBarcode = o.Barcode,
                                                Memo = "赠品"
                                            });
                                        }
                                    });
                                }
                            });
                            upts.AddRange(gifts);
                            BaseService<IndentOrderList>.CurrentRepository.RemoveRange(deletes, false);
                            BaseService<IndentOrderList>.AddRange(upts, false);
                            #region 操作日志
                            foreach (var item in deletes)
                            {
                                var msg = Pharos.Sys.LogEngine.CompareModelToLog<IndentOrderList>(Sys.LogModule.采购订单, null, item);
                                new Pharos.Sys.LogEngine().WriteDelete(msg, Sys.LogModule.采购订单);
                            }
                            foreach (var item in upts)
                            {
                                var msg = Pharos.Sys.LogEngine.CompareModelToLog<IndentOrderList>(Sys.LogModule.采购订单, item);
                                new Pharos.Sys.LogEngine().WriteInsert(msg, Sys.LogModule.采购订单);
                            }
                            #endregion
                        }
                    }
                    BaseService<IndentOrderList>.AddRange(details, false);
                    foreach (var item in details)
                    {
                        var msg = Pharos.Sys.LogEngine.CompareModelToLog<IndentOrderList>(Sys.LogModule.采购订单, item);
                        new Pharos.Sys.LogEngine().WriteDelete(msg, Sys.LogModule.采购订单);
                    }
                    re = Update(sour);
                }
            }
            catch (Exception ex)
            {
                re.Message = ex.Message;
            }
            return re;
        }
        /// <summary>
        /// 用于datagrid列表
        /// </summary>
        /// <param name="nvl">传递条件</param>
        /// <param name="recordCount">返回总行数</param>
        /// <returns>list</returns>
        public static object FindPageList(NameValueCollection nvl, out int recordCount)
        {
            var query = BaseService<VwOrder>.CurrentRepository.QueryEntity.Where(o => o.CompanyId == CommonService.CompanyId);
            var state = nvl["State"];//状态
            var orderMan = nvl["OrderMan"];//订货人
            var startDate = nvl["StartDate"];//订货日期
            var endDate = nvl["EndDate"];
            var sup = nvl["sup"];//订单配送
            var supplierId = nvl["supplierId"];//供应商
            var orderNo = nvl["orderNo"];
            var expin = nvl["expin"];//排除已入库
            var searchField = nvl["searchField"];
            var searchText = nvl["searchText"];
            if (!state.IsNullOrEmpty())
            {
                var st = state.Split(',').Select(o=>short.Parse(o)).ToList();
                query = query.Where(d =>st.Contains( d.State));
            }
            if (!orderMan.IsNullOrEmpty())
            {
                query = query.Where(d => d.CreateUID == orderMan);
            }
            if (!orderNo.IsNullOrEmpty())
            {
                query = query.Where(d => d.IndentOrderId.Contains(orderNo));
            }
            if (!startDate.IsNullOrEmpty())
            {
                var st1 = DateTime.Parse(startDate);
                query = query.Where(d => d.CreateDT >= st1);
            }
            if (!endDate.IsNullOrEmpty())
            {
                var st2 = DateTime.Parse(endDate).AddDays(1);
                query = query.Where(d => d.CreateDT < st2);
            }
            if (sup == "1")
                query = query.Where(o => o.State > 0);
            if (!expin.IsNullOrEmpty())
            {
                query = query.Where(i => !InboundGoodsBLL.CurrentRepository.Entities.Any(o => o.IndentOrderId == i.IndentOrderId));
            }
            if (!Sys.CurrentUser.StoreId.IsNullOrEmpty())
                query = query.Where(o => o.StoreId == Sys.CurrentUser.StoreId);
            if (!supplierId.IsNullOrEmpty())
                query = query.Where(o => o.SupplierID == supplierId);
            //var queryDetail = BaseService<IndentOrderList>.CurrentRepository.QueryEntity;
            //var q = from qo in query
            //        let o = from qd in queryDetail
            //                where qd.IndentOrderId == qo.Id
            //                select qd
            //        select new { 
            //            qo.Id,
            //            qo.OrdererUID,
            //            AcceptNums= o.Sum(i=>i.AcceptNum),
            //            DeliveryNums = o.Sum(i => i.DeliveryNum),
            //            IndentNums = o.Sum(i => i.IndentNum)
            //        };
            if (!searchText.IsNullOrEmpty())
            {
                if (searchField == "barcode")
                    query = query.Where(o => BaseService<IndentOrderList>.CurrentRepository.QueryEntity.Any(i => i.IndentOrderId == o.IndentOrderId && i.Barcode.Contains(searchText)));
                else if (searchField == "IndentOrderId")
                    query = query.Where(o => o.IndentOrderId.Contains(searchText));
            }
            recordCount = query.Count();
            return query.ToPageList(nvl);
        }
        public static new IndentOrder FindById(object id, params string[] includeParams)
        {
            //var obj=CurrentRepository.FindById(id, "Details");
            var strId = Convert.ToInt32(id);
            var obj = CurrentRepository.QueryEntity.FirstOrDefault(o => o.Id == strId);
            obj.Details = BaseService<IndentOrderList>.FindList(o => o.IndentOrderId == obj.IndentOrderId);
            return obj;
        }
        /// <summary>
        /// 用于修改回显列表
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="recordCount">返回记录数</param>
        /// <returns>list</returns>
        public static System.Data.DataTable LoadDetailList(string orderId, out int recordCount, ref object footer)
        {
            recordCount = 0;
            var dal = new Pharos.Logic.DAL.OrderDAL();
            var dt = dal.LoadDetailList(orderId,CommonService.CompanyId);
            decimal total = 0, nums = 0;
            foreach (System.Data.DataRow dr in dt.Rows)
            {
                dr["Detail"] = System.Web.HttpUtility.HtmlDecode(dr["Detail"].ToString());
                total += Convert.ToDecimal(dr["Subtotal"]);
                nums += Convert.ToDecimal(dr["IndentNum"]);
                if (!(dr["AssistBarcode"] is DBNull))
                    dr["Barcode"] = dr["AssistBarcode"];
            }
            footer = new List<object>() { 
                new {Subtotal=total,IndentNum=nums,Price="合计:"}
            };
            return dt;
        }
        public static List<VwOrder> GetNewOrder(int num)
        {
            var query = BaseService<VwOrder>.CurrentRepository.QueryEntity.Where(o=>o.CompanyId==CommonService.CompanyId);
            if (!Sys.CurrentUser.StoreId.IsNullOrEmpty())
                query = query.Where(o => o.StoreId == Sys.CurrentUser.StoreId);
            return query.OrderByDescending(o => o.CreateDT).Take(num).ToList();
        }

        public static object ReportDetail(string type, string orderId)
        {
            if (type == "入库")
            {
                var inbound = from x in InboundGoodsBLL.CurrentRepository.QueryEntity
                              where x.CompanyId == CommonService.CompanyId && x.IndentOrderId == orderId
                              select new { 
                                    x.IndentOrderId,
                                    StoreTitle=WarehouseService.CurrentRepository.QueryEntity.Where(o=>o.StoreId==x.StoreId && o.CompanyId==x.CompanyId).Select(o=>o.Title).FirstOrDefault(),
                                    CreateName= UserInfoService.CurrentRepository.QueryEntity.Where(o=>o.UID== x.CreateUID).Select(o=>o.FullName).FirstOrDefault(),
                                    BuyerName = UserInfoService.CurrentRepository.QueryEntity.Where(o => o.UID == x.BuyerUID).Select(o => o.FullName).FirstOrDefault(),
                                    SupperTitle=SupplierService.CurrentRepository.QueryEntity.Where(o=>o.Id==x.SupplierID).Select(o=>o.FullTitle).FirstOrDefault(),
                                    x.ReceivedDT,
                                    x.CreateDT,
                                    x.DistributionBatch
                              };
                return inbound.FirstOrDefault();
            }
            else
            {
                var inbound = from x in CurrentRepository.QueryEntity
                              where x.CompanyId == CommonService.CompanyId && x.IndentOrderId == orderId
                              select new
                              {
                                  x.IndentOrderId,
                                  StoreTitle = WarehouseService.CurrentRepository.QueryEntity.Where(o => o.StoreId == x.StoreId && o.CompanyId == x.CompanyId).Select(o => o.Title).FirstOrDefault(),
                                  CreateName = UserInfoService.CurrentRepository.QueryEntity.Where(o => o.UID == x.CreateUID).Select(o => o.FullName).FirstOrDefault(),
                                  RecipientName = UserInfoService.CurrentRepository.QueryEntity.Where(o => o.UID == x.RecipientsUID).Select(o => o.FullName).FirstOrDefault(),
                                  SupperTitle = SupplierService.CurrentRepository.QueryEntity.Where(o => o.Id == x.SupplierID).Select(o => o.FullTitle).FirstOrDefault(),
                                  x.ReceivedDT,
                                  x.CreateDT,
                                  x.ShippingAddress,
                                  x.Phone
                              };
                return inbound.FirstOrDefault();
            }
        }
        public static System.Data.DataTable LoadReportDetailList(string orderId, out int recordCount, ref object footer)
        {
            recordCount = 0;
            var dal = new Pharos.Logic.DAL.OrderDAL();
            var dt = dal.LoadReportDetailList(orderId,CommonService.CompanyId);
            decimal total = 0, nums = 0;
            foreach (System.Data.DataRow dr in dt.Rows)
            {
                total += Convert.ToDecimal(dr["Subtotal"]);
                nums += Convert.ToDecimal(dr["IndentNum"]);
            }
            footer = new List<object>() { 
                new {Subtotal=total,IndentNum=nums,Price="合计:"}
            };
            return dt;
        }
    }

}