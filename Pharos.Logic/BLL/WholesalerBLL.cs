﻿using Pharos.Logic.Entity;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Data.Entity;
using Pharos.Utility.Helpers;
using Pharos.Utility;
using Pharos.Sys.Entity;
using Pharos.Sys;

namespace Pharos.Logic.BLL
{
    public class WholesalerBLL : BaseService<Supplier>
    {


        /// <summary>
        /// 用于datagrid列表
        /// </summary>
        /// <param name="nvl">传递条件</param>
        /// <param name="recordCount">返回总行数</param>
        /// <returns>list</returns>
        public static object FindPageList(NameValueCollection nvl, out int recordCount)
        {
            var query = BaseService<VwSupplier>.CurrentRepository.Entities.Where(o => o.CompanyId == CommonService.CompanyId);
            var sn = nvl["searchText"].Trim();
            var state = nvl["state"].IsNullOrEmpty() ? -1 : short.Parse(nvl["state"]);
            var express = DynamicallyLinqHelper.True<VwSupplier>().And(o =>
                    (o.Title != null && o.Title.Contains(sn)) ||
                    (o.FullTitle != null && o.FullTitle.Contains(sn)) ||
                    (o.MobilePhone != null && o.MobilePhone.Contains(sn)) ||
                    (o.Linkman != null && o.Linkman.Contains(sn)), sn.IsNullOrEmpty())
                    .And(o => o.State == state, state == -1);
            var p = query.Where(express);
            var q = p.Where(o => o.BusinessType == 2);
            recordCount = q.Count();
            var pages = q.ToPageList(nvl);
            var list = pages.Select(o => new
            {
                o.Id,
                o.Title,
                o.MasterAccount,
                o.Linkman,
                o.MobilePhone,
                o.Tel,
                o.ClassName,
                o.ApplyNum,
                o.Designeer,
                o.BusinessType
            }).ToList();
            return list;
        }

        public static OpResult SaveOrUpdate(Supplier obj)
        {
            var re = new OpResult();
            if (!obj.MasterAccount.IsNullOrEmpty() && SupplierService.IsExist(o => o.Id != obj.Id && o.MasterAccount == obj.MasterAccount && o.BusinessType == 2 && o.CompanyId==CommonService.CompanyId))
                re.Message = "该账号已存在，请重新填写！";
            else if (!obj.Title.IsNullOrEmpty() && SupplierService.IsExist(o => o.Id != obj.Id && o.Title == obj.Title && o.BusinessType == 2 && o.CompanyId == CommonService.CompanyId))
                re.Message = "该简称已存在，请重新填写！";
            else if (!obj.FullTitle.IsNullOrEmpty() && SupplierService.IsExist(o => o.Id != obj.Id && o.FullTitle == obj.FullTitle && o.BusinessType == 2 && o.CompanyId == CommonService.CompanyId))
                re.Message = "该全称已存在，请重新填写！";
            else if (obj.Id.IsNullOrEmpty())
            {
                obj.Id = Logic.CommonRules.GUID;
                obj.BusinessType = 2;
                obj.CompanyId = CommonService.CompanyId;
                re = SupplierService.Add(obj);
                #region 操作日志
                try
                {

                    LogEngine logEngine = new LogEngine();
                    var logMsg = LogEngine.CompareModelToLog<Supplier>(LogModule.批发商, obj);
                    logEngine.WriteInsert(logMsg, LogModule.批发商);
                }
                catch
                {
                }
                #endregion
            }
            else
            {
                var supp = SupplierService.FindById(obj.Id);
                Supplier _oInfo = new Supplier();
                if (supp != null)
                {
                    ExtendHelper.CopyProperty<Supplier>(_oInfo, supp);
                }
                var exc = new List<string>();
                if (obj.MasterPwd.IsNullOrEmpty())
                    exc.Add("MasterPwd");
                exc.Add("CompanyId");
                obj.BusinessType = supp.BusinessType;
                obj.ToCopyProperty(supp, exc);
                re = SupplierService.Update(supp);
                #region 操作日志
                try
                {

                    LogEngine logEngine = new LogEngine();
                    var logMsg = LogEngine.CompareModelToLog<Supplier>(LogModule.批发商, obj, _oInfo);
                    logEngine.WriteUpdate(logMsg, LogModule.批发商);
                }
                catch
                {
                }
                #endregion
            }
            return re;
        }
        public static List<Supplier> GetList()
        {
            return FindList(null);
        }
        static string GetTitle(List<SysDataDictionary> types, int id)
        {
            var obj = types.FirstOrDefault(o => o.DicSN == id);
            if (obj == null) return "";
            return obj.Title;
        }

        static string GetState(short? state)
        {
            if (state == null) return "";
            return Enum.GetName(typeof(ContractState), state);
        }
        static Contract GetContract(ICollection<Contract> list, string state)
        {
            if (list == null) return null;
            if (state.IsNullOrEmpty()) return list.OrderByDescending(i => i.CreateDT).FirstOrDefault();
            var st = short.Parse(state);
            return list.Where(o => o.State == st).OrderByDescending(i => i.CreateDT).FirstOrDefault();
        }
        static string GetUser(string uid, List<SysUserInfo> list)
        {
            if (uid.IsNullOrEmpty()) return "";
            var obj = list.FirstOrDefault(o => o.UID == uid);
            if (obj == null) return "";
            return obj.FullName;
        }

        public static IEnumerable<Supplier> GetAllSupplier()
        {
            return CurrentRepository.FindList(o => true).ToList();
        }


        /// <summary>
        /// 获取出库单信息
        /// </summary>
        /// <param name="sId">当前行批发商id</param>
        /// <param name="recordCount">返回记录数</param>
        /// <returns>出库单列表</returns>
        public static object FindOutboundGoodsList(string sId, out int recordCount)
        {
            var queryOutboundGoods = BaseService<OutboundGoods>.CurrentRepository.QueryEntity.Where(o => o.CompanyId == CommonService.CompanyId);
            var queryOutboundList = BaseService<OutboundList>.CurrentRepository.QueryEntity;
            var queryWarehouse = BaseService<Warehouse>.CurrentRepository.QueryEntity;
            var queryUser = UserInfoService.CurrentRepository.QueryEntity;
            var querySupplier = BaseService<Supplier>.CurrentRepository.QueryEntity;

            var groupbyOutboundList = from a in queryOutboundList
                                      group a by a.OutboundId into g
                                      select new
                                      {
                                          g.Key,
                                          OutboundPrice = g.Sum(a => a.SysPrice * a.OutboundNumber),
                                          OutboundNumber = g.Sum(a => a.OutboundNumber)
                                      };

            var query = from x in queryOutboundGoods
                        join y in groupbyOutboundList on x.OutboundId equals y.Key
                        join s in queryWarehouse on x.StoreId equals s.StoreId into temp1
                        from z in temp1.DefaultIfEmpty()
                        join l in queryUser on x.OperatorUID equals l.UID into temp3
                        from m in temp3.DefaultIfEmpty()
                        join r in queryWarehouse on x.ApplyOrgId equals r.StoreId into temp4
                        from q in temp4.DefaultIfEmpty()
                        join a in querySupplier on x.ApplyOrgId equals a.Id into tempSupplier
                        from b in tempSupplier.DefaultIfEmpty()
                        select new
                        {
                            x.Id,
                            x.OutboundId,
                            x.StoreId,
                            StoreTitle = z.Title,
                            x.OperatorUID,
                            Operator = m.FullName,
                            y.OutboundNumber,
                            y.OutboundPrice,
                            x.CreateDT,
                            x.State,
                            x.Channel,
                            x.ApplyOrgId,
                        };
            query = query.Where(o => o.Channel == 1 && o.ApplyOrgId == sId);
            recordCount = query.Count();
            return query.ToPageList().Select(o => new
            {
                o.Id,
                o.OutboundId,
                o.StoreTitle,
                o.Operator,
                o.OutboundNumber,
                o.OutboundPrice,
                o.CreateDT,
                CreateDTStr = o.CreateDT.ToString("yyyy-MM-dd"),
                o.State,
                StateTitle = Enum.GetName(typeof(OutBoundState), o.State),
            });
        }




    }
}
