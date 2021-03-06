﻿using Pharos.Logic.OMS.DAL;
using Pharos.Logic.OMS.Entity;
using Pharos.Logic.OMS.Entity.View;
using Pharos.Logic.OMS.IDAL;
using Pharos.Utility;
using Pharos.Utility.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Pharos.Logic.OMS.Cache;
using Pharos.Infrastructure.Data.Normalize;

namespace Pharos.Logic.OMS.BLL
{
    /// <summary>
    /// BLL-----商户资料
    /// </summary>
    public class TradersService : BaseService<Traders>
    {
        [Ninject.Inject]
        // 商户资料
        IBaseRepository<Traders> TradersRepository { get; set; }

        // 商户资料
        //[Ninject.Inject]
        ITradersRepository tRepository
        {
            get
            {
                return new TradersRepository();
            }
        }

        [Ninject.Inject]
        // 商户分类
        IBaseRepository<TraderType> TraderTypeRepository { get; set; }

        [Ninject.Inject]
        //区域管理信息
        public IBaseRepository<Area> AreaRepository { get; set; }

        [Ninject.Inject]
        //数据字典信息
        public IBaseRepository<SysDataDictionary> SysDataDictionaryRepository { get; set; }

        [Ninject.Inject]
        //行业管理信息
        public IBaseRepository<Business> BusinessRepository { get; set; }

        //[Ninject.Inject]
        //帐户管理
        public IBaseRepository<SysUser> SysUserInfoRepository
        {
            get
            {
                return new BaseRepository<SysUser>();
            }
        }

        [Ninject.Inject]
        //采购意向清单
        public IBaseRepository<OrderList> OrderListRepository { get; set; }

        [Ninject.Inject]
        //回访跟踪记录
        public IBaseRepository<VisitTrack> VisitTrackRepository { get; set; }

        //[Ninject.Inject]
        public IBaseRepository<SysUser> SysUserRepository
        {
            get
            {
                return new BaseRepository<SysUser>();
            }
        }

        //BLL-----回访跟踪记录
        [Ninject.Inject]
        VisitTrackService visitTrackService { get; set; }

         //BLL-----采购意向清单
        [Ninject.Inject]
         OrderListService  orderListService { get; set; }

        [Ninject.Inject]
        
        public IBaseRepository<CompanyAuthorize> CompanyAuthorizeRepository { get; set; }
        [Ninject.Inject]

        public CompAuthorService CompAuthorService { get; set; }

        [Ninject.Inject]
        //设备授权信息
        public IBaseRepository<DeviceAuthorize> DeviceAuthorizeRepository { get; set; }
        [Ninject.Inject]
        ImportSetService ImportSetService { get; set; }
        public List<ViewTrader> GetPageList(System.Collections.Specialized.NameValueCollection nvl, out int recordCount, int all = 0)
        {

            //省份
            var CurrentProvinceId = (nvl["CurrentProvinceId"] ?? "").Trim();
            //城市
            var CurrentCityId = (nvl["CurrentCityId"] ?? "").Trim();
            //登记日期（开始）
            var CreateDT_begin = (nvl["CreateDT_begin"] ?? "").Trim();
            //登记日期（结束）
            var CreateDT_end = (nvl["CreateDT_end"] ?? "").Trim();
            //跟踪状态
            var TrackStautsId = (nvl["TrackStautsId"] ?? "").Trim();
            //业务员
            var AssignerUID = (nvl["AssignerUID"] ?? "").Trim();
            //客户类型
            var BusinessModeId = (nvl["BusinessModeId"] ?? "").Trim();
            //经营范围
            var BusinessScopeId = (nvl["BusinessScopeId"] ?? "").Trim();
            //关键字类型
            var keywordType = (nvl["keywordType"] ?? "").Trim();
            //关键字
            var keyword = (nvl["keyword"] ?? "").Trim();
            //支付类型
            var pay = (nvl["pay"] ?? "").Trim();

            var pageIndex = 1;
            var pageSize = 20;
            if (!nvl["page"].IsNullOrEmpty())
                pageIndex = int.Parse(nvl["page"]);
            if (!nvl["rows"].IsNullOrEmpty())
                pageSize = int.Parse(nvl["rows"]);

            string strw = "";
            if (!CurrentProvinceId.IsNullOrEmpty() && CurrentProvinceId != "0")
            {
                strw = strw + " and t.CurrentProvinceId=" + CurrentProvinceId;
            }
            if (!CurrentCityId.IsNullOrEmpty() && CurrentCityId != "0")
            {
                strw = strw + " and t.CurrentCityId=" + CurrentCityId;
            }
            if (!CreateDT_begin.IsNullOrEmpty())
            {
                string c = CreateDT_begin + " " + "00:00:00";
                strw = strw + " and t.CreateDT >='"+c+"'";
            }
            if (!CreateDT_end.IsNullOrEmpty())
            {
                var c = CreateDT_end + " " + "23:59:59";
                strw = strw + " and t.CreateDT <='"+c+"'";
            }
            if (!TrackStautsId.IsNullOrEmpty())
            {
                strw = strw + " and t.TrackStautsId=" + TrackStautsId;
            }
            if (!AssignerUID.IsNullOrEmpty())
            {
                string[] aUID = AssignerUID.Split(',');
                string newAUID = "";
                if (aUID.Length > 0)
                {
                    for (int i = 0; i < aUID.Length; i++)
                    {
                        if (newAUID == "")
                        {
                            newAUID = "'" + aUID[i] + "'";
                        }
                        else
                        {
                            newAUID = newAUID + ",'" + aUID[i] + "'";
                        }
                    }
                    strw = strw + " and t.AssignerUID in (" + newAUID + ")";
                    //全部显示
                    if (all == 0)
                    {
                        var UserId = getUserIdByDeptId(CurrentUser.DeptId);
                        string newUserId = "";
                        foreach (var v in UserId)
                        {
                            if (newUserId == "")
                            {
                                newUserId = "'" + v + "'";
                            }
                            else
                            {
                                newUserId = newUserId + ",'" + v + "'";
                            }
                        }
                        //仅限指派本部门未分配的客户 (含查看)
                        if (CurrentUser.HasPermiss(74) && !CurrentUser.HasPermiss(250) && !CurrentUser.HasPermiss(246) && !CurrentUser.HasPermiss(248))
                        {
                            strw = strw + " and (t.AssignerUID is null or t.AssignerUID='') and t.CreateUID in (" + newUserId + ")";
                        }
                        //查看本部门所有客户
                        if (CurrentUser.HasPermiss(246) && !CurrentUser.HasPermiss(248))
                        {
                            strw = strw + " and t.AssignerUID in (" + newAUID + ") and t.CreateUID in (" + newUserId + ")";
                        }
                        //没有权限
                        if (!CurrentUser.HasPermiss(74) && !CurrentUser.HasPermiss(250) && !CurrentUser.HasPermiss(246) && !CurrentUser.HasPermiss(248))
                        {
                            strw = strw + " and t.AssignerUID in (" + newAUID + ") and t.CreateUID is null";
                        }
                    }
                }
            }
            else
            {
                //全部显示
                if (all == 0)
                {
                    var UserId = getUserIdByDeptId(CurrentUser.DeptId);
                    string newUserId = "";
                    foreach (var v in UserId)
                    {
                        if (newUserId == "")
                        {
                            newUserId = "'" + v + "'";
                        }
                        else
                        {
                            newUserId = newUserId + ",'" + v + "'";
                        }
                    }
                    //仅限指派本部门未分配的客户 (含查看)
                    if (CurrentUser.HasPermiss(74) && !CurrentUser.HasPermiss(250) && !CurrentUser.HasPermiss(246) && !CurrentUser.HasPermiss(248))
                    {
                        strw = strw + " and (t.AssignerUID is null or t.AssignerUID='') and t.CreateUID in (" + newUserId + ")";
                    }
                    //查看本部门所有客户
                    if (CurrentUser.HasPermiss(246) && !CurrentUser.HasPermiss(248))
                    {
                        strw = strw + " and t.CreateUID in (" + newUserId + ")";
                    }
                    //没有权限
                    if (!CurrentUser.HasPermiss(74) && !CurrentUser.HasPermiss(250) && !CurrentUser.HasPermiss(246) && !CurrentUser.HasPermiss(248))
                    {
                        strw = strw + " and t.CreateUID is null";
                    }
                }
            }

            if (!BusinessModeId.IsNullOrEmpty())
            {
                strw = strw + " and t.BusinessModeId=" + BusinessModeId;
            }
            if (!BusinessScopeId.IsNullOrEmpty())
            {
                strw = strw + " and t.BusinessScopeId like '%" + BusinessScopeId + "%'";
            }

            if (!keywordType.IsNullOrEmpty()&&!keyword.IsNullOrEmpty())
            {
                if (keywordType == "0")
                {
                    if (!IsNumber(keyword) || keyword.Length > 9)
                    {
                        keyword = "0";
                    }
                    strw = strw + " and t.CID=" + keyword;
                }
                if (keywordType == "1")
                {
                    strw = strw + " and t.Title like '%" + keyword + "%'";
                }
                if (keywordType == "2")
                {
                    strw = strw + " and t.LinkMan like '%" + keyword + "%'";
                }
                if (keywordType == "3")
                {
                    strw = strw + " and t.MobilePhone like '%" + keyword + "%'";
                }
            }

            if (!pay.IsNullOrEmpty())
            {
                strw = strw + " and CHARINDEX('"+pay+"',t.Pay)>0";
            }

            //recordCount = q.Count();
            //var list = q.ToPageList();
            //recordCount = 0;
            //var bussineScopeids= list.Where(o => !o.BusinessScopeId.IsNullOrEmpty()).SelectMany(o => o.BusinessScopeId.Split(',')).Distinct().ToList();
            //var bussines= BusinessRepository.GetQuery(o => bussineScopeids.Contains(o.ById)).ToList();

            //return list.Select(o=>new{
            //    bussineTitle = GetBussinTitle(bussines,o.BusinessScopeId),

            //});

           List<ViewTrader> list=  tRepository.getPageList(pageIndex, pageSize, strw, out recordCount);
           return list;  
        }

        /// <summary>
        /// 指派业务员
        /// </summary>
        /// <param name="nvl"></param>
        /// <param name="recordCount"></param>
        /// <returns></returns>
        public List<ViewTrader> GetListAssigner(System.Collections.Specialized.NameValueCollection nvl, out int recordCount)
        {
            var ids = (nvl["ids"] ?? "").Trim();
            if(ids.IsNullOrEmpty())
            {
                ids="0";
            }
            var pageIndex = 1;
            var pageSize = 20;
            if (!nvl["page"].IsNullOrEmpty())
                pageIndex = int.Parse(nvl["page"]);
            if (!nvl["rows"].IsNullOrEmpty())
                pageSize = int.Parse(nvl["rows"]);

            string strw = "";
            if (!ids.IsNullOrEmpty())
            {
                var idss = ids.Split(',').Select(o => int.Parse(o));
                string newIds="";
                foreach(var v in idss)
                {
                    if (v > 0)
                    {
                        if (newIds == "")
                        {
                            newIds = v.ToString();
                        }
                        else
                        {
                            newIds = newIds + "," + v;
                        }
                    }
                }
                strw = strw + " and t.Id in ("+newIds+")";
            }
            
            List<ViewTrader> list = tRepository.getPageList(pageIndex, pageSize, strw, out recordCount);
            return list;
        }

        //string GetBussinTitle(List<Business> list,string bussinScopeId)
        //{
        //    if (bussinScopeId.IsNullOrEmpty()) return "";
        //    var ids = bussinScopeId.Split(',');
        //    return string.Join(",", list.Where(o=>ids.Contains(o.ById)).Select(o=>o.Title));
        //}


        /// <summary>
        /// 获取最大CID
        /// </summary>
        /// <returns></returns>
        public int getMaxCID()
        {
            //return TradersRepository.GetQuery().Max(o => (int?)o.CID).GetValueOrDefault() + 1;
            int cid = 0;
            cid = TradersRepository.GetQuery().Max(o => (int?)o.CID).GetValueOrDefault();
            if (cid < 101)
            {
                cid = 101;
            }
            else
            {
                cid = cid + 1;
                if (cid > 9999900)
                {
                    cid= -1;
                }
            }
            return cid;
        }

        public Traders GetOne(object id)
        {
            return TradersRepository.Get(id);
        }
        public Traders GetTraderByCID(int cid)
        {
            return GetEntityByWhere(o => o.CID == cid);
        }
        public object GetOneByCID(int cid)
        {
            if (cid <= 0) return null;
            var queryTrader= TradersRepository.GetQuery(o=>o.CID==cid);
            var queryAuth = CompanyAuthorizeRepository.GetQuery();
            var query = from x in queryTrader
                        join y in queryAuth on x.CID equals y.CID into tmp
                        from z in tmp.DefaultIfEmpty()
                        select new
                        {
                            x.CID,
                            x.Title,
                            x.FullTitle,
                            x.Address,
                            x.BusinessModeId,
                            x.BusinessScopeId,
                            x.CurrentCityId,
                            x.CurrentCounty,
                            x.CurrentProvinceId,
                            x.EachStorePersonNum,
                            x.EachStorePosNum,
                            x.ExistDeviceName,
                            x.ExistStoreNum,
                            x.ExistsystemName,
                            x.LinkMan,
                            x.Memo,
                            x.MobilePhone,
                            x.PlanExpandStoreNum,
                            x.Status,
                            x.TakeStockDates,
                            x.TraderTypeId,
                            z.AppProper,
                            z.PosMinorDisp,
                            z.StoreProper,
                            z.OpenVersionId,
                            z.Way,
                            z.CreateDT
                        };

            return query.OrderByDescending(o => o.CreateDT).FirstOrDefault();
        }
        /// <summary>
        /// 增加、修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public OpResult SaveOrUpdate(Traders model, string[] BusinessScopeId, string[] Pay)
        {
            //客户简称是否存在
            IQueryable<Traders> isExistTitle = null;
            if (model.Id == 0)
            {
                isExistTitle = TradersRepository.GetQuery(o => o.Title == model.Title);
            }
            else
            {
                isExistTitle = TradersRepository.GetQuery(o =>  o.Title == model.Title && o.Id != model.Id);
            }
            if (isExistTitle.Any())
            {
                return OpResult.Fail("该客户简称已经存在");
            }

            //客户全称是否存在
            IQueryable<Traders> isExistFullTitle = null;
            if (model.Id == 0)
            {
                isExistFullTitle = TradersRepository.GetQuery(o => o.FullTitle == model.FullTitle);
            }
            else
            {
                isExistFullTitle = TradersRepository.GetQuery(o => o.FullTitle == model.FullTitle && o.Id != model.Id);
            }
            if (isExistFullTitle.Any())
            {
                return OpResult.Fail("该客户全称已经存在");
            }

            model.BusinessScopeId = "";
            
            if (BusinessScopeId!=null && BusinessScopeId.Length > 0)
            {
                foreach (var v in BusinessScopeId)
                {
                    if (model.BusinessScopeId == "")
                    {
                        model.BusinessScopeId = v;
                    }
                    else
                    {
                        model.BusinessScopeId = model.BusinessScopeId + "," + v;
                    }

                }
            }

            model.Pay = "";
            if (Pay != null && Pay.Length > 0)
            {
                foreach (var v in Pay)
                {
                    if (model.Pay == "")
                    {
                        model.Pay = v;
                    }
                    else
                    {
                        model.Pay = model.Pay + "," + v;
                    }

                }
            }
            
            if (model.Id == 0)
            {
                model.AddType = 0;
                model.CreateUID = CurrentUser.UID;
                model.CreateDT = DateTime.Now;
                model.UpdateDT = DateTime.Now;
                model.CID = getMaxCID();
                TradersRepository.Add(model);
            }
            else
            {
                model.UpdateDT = DateTime.Now;
                var source = TradersRepository.Get(model.Id);
                model.CID = source.CID;
                model.ToCopyProperty(source, new List<string>() { "CreateUID", "CreateDT", "CID", "Status", "AddType" });
                var auths= CompanyAuthorizeRepository.GetQuery(o => o.CID == model.CID && o.Status != 3).ToList();
                auths.Each(o => o.Title = model.FullTitle);
            }

            if (TradersRepository.SaveChanges())
            {
                LogEngine.WriteUpdate(model.Id + "," + model.Title, LogModule.商户资料);
            }

            return OpResult.Success(model.CID.ToString());
        }

        /// <summary>
        /// 获取省
        /// </summary>
        /// <returns></returns>
        public List<Area> getProvince(string defaultTitle = "请选择")
        {
            var province = AreaRepository.GetQuery(o => o.Type == 2).ToList();
            province.Insert(0, new Area() { AreaID = 0, Title = defaultTitle });
            return province;
        }

        /// <summary>
        /// 获取城市
        /// </summary>
        /// <returns></returns>
        public List<Area> getCity(int ProvinceID, string defaultTitle = "请选择")
        {
            List<Area> list = new List<Area>();
            if (ProvinceID > 0)
            {
                list = AreaRepository.GetQuery(o => o.AreaPID == ProvinceID).ToList();
            }
            list.Insert(0, new Area() { AreaID = 0, Title = defaultTitle });
            return list;
        }

        /// <summary>
        /// 获取区县
        /// </summary>
        /// <returns></returns>
        public List<Area> getDistrict(int CityID)
        {
            List<Area> list = new List<Area>();
            if (CityID > 0)
            {
                list = AreaRepository.GetQuery(o => o.AreaPID == CityID).ToList();
            }
            list.Insert(0, new Area() { AreaID = 0, Title = "请选择" });
            return list;
        }
        public List<Traders> GetTraderInput(string searchName)
        {
            if (searchName.IsNullOrEmpty()) return null;
            int cid = 0;
            int.TryParse(searchName, out cid);
            var query= TradersRepository.GetQuery(o=>o.Status==1);
            //if (cid > 0) query = query.Where(o => o.CID == cid);
            query = query.Where(o =>  o.CID == cid || o.Title.Contains(searchName) || o.FullTitle.Contains(searchName));
            return query.Take(20).ToList();
        }

        /// <summary>
        /// 获取商户分类
        /// </summary>
        /// <returns></returns>
        public List<TraderType> getTraderTypeList()
        {
            return TraderTypeRepository.GetQuery().ToList();
        }

        /// <summary>
        /// 获取经营模式
        /// </summary>
        /// <returns></returns>
        public List<SysDataDictionary> getDataList()
        {
            return SysDataDictionaryRepository.GetQuery(w => w.DicPSN == 221&&w.Status).OrderBy(o => o.SortOrder).ToList();
        }

        /// <summary>
        /// 获取字典
        /// </summary>
        /// <returns></returns>
        public List<SysDataDictionary> getDataList(int DicPSN)
        {
            return SysDataDictionaryRepository.GetQuery(w => w.DicPSN == DicPSN&&w.Status).OrderBy(o => o.SortOrder).ToList();
        }

        /// <summary>
        /// 获取经营类目
        /// </summary>
        /// <returns></returns>
        public List<Business> getBusinessList()
        {
            //var  dd= BusinessRepository.GetQuery().ToList();
            //return null;
            return BusinessRepository.GetQuery().ToList();
        }
        public bool ExistsTitle(string title,string fullTitle)
        {
            return TradersRepository.GetQuery(o => o.Title == title || o.FullTitle == fullTitle).Any();
        }
        /// <summary>
        /// 获取登记人
        /// </summary>
        /// <returns></returns>
        public string getFullName(string UserId)
        {
            string uid = "";
            if (UserId.IsNullOrEmpty())
            {
                uid = CurrentUser.UID;
            }
            else
            {
                uid = UserId;
            }
            return SysUserInfoRepository.GetQuery(o => o.UserId == uid).Select(o=>o.FullName).FirstOrDefault();
        }

        /// <summary>
        /// 获取上级领导
        /// </summary>
        /// <returns></returns>
        public string getLeaderFullName(string UserId)
        {
            if (!UserId.IsNullOrEmpty())
            {
                return SysUserInfoRepository.GetQuery(o => o.UserId == UserId).Select(o => o.FullName).FirstOrDefault();
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 获取采购意向清单
        /// </summary>
        /// <param name="TradersCID">企业ID</param>
        /// <returns></returns>
        public List<ViewOrderList> getOrderList(int TradersCID)
        {
            //数据字典信息
            var data = SysDataDictionaryRepository.GetQuery();
            //采购意向清单
            var orderList = OrderListRepository.GetQuery(o=>o.CID==TradersCID);

            var order = from o in orderList
                        join d in data on o.DeviceId equals d.DicSN
                        into dd
                        from ddd in dd.DefaultIfEmpty()
                        join d2 in data on o.UnitID equals d2.DicSN
                        into dd2
                        from ddd2 in dd2.DefaultIfEmpty()
                        select new ViewOrderList
                        {
                            pName=ddd==null?"":ddd.Title,
                            OrderNum=o.OrderNum,
                            uName = ddd2 == null ? "" : ddd2.Title,
                            Remark=o.Remark,
                            DeviceId=o.DeviceId,
                            UnitID=o.UnitID
                        };
            //测试
            string ssss = order.ToString();

            return order.ToList();
        }

        /// <summary>
        /// 获取业务员
        /// </summary>
        /// <param name="TradersCID">企业ID</param>
        /// <returns></returns>
        public List<SysUser> getUserList()
        {
            var uer = SysUserInfoRepository.GetQuery();
            return uer.ToList();
        }

        /// <summary>
        /// 获取客户汇总业务员
        /// </summary>
        public List<SysUser> getSummaryUserList()
        {
            IQueryable<SysUser> uer = SysUserInfoRepository.GetQuery();
            //查看我的客户汇总 
            if (CurrentUser.HasPermiss(81) && !CurrentUser.HasPermiss(243) && !CurrentUser.HasPermiss(244))
            {
                uer = SysUserInfoRepository.GetQuery(o => o.UserId == CurrentUser.UID);
            }
            //查看本部门的客户汇总
            if (CurrentUser.HasPermiss(243) && !CurrentUser.HasPermiss(244))
            {
                var UserId = getUserIdByDeptId(CurrentUser.DeptId);
                uer = SysUserInfoRepository.GetQuery(o=>UserId.Contains(o.UserId));
            }
            return uer.ToList();
        }

        /// <summary>
        /// 获取业务员(查看所有客户)
        /// </summary>
        public List<SysUser> getAllUserList()
        {
            IQueryable<SysUser> uer = SysUserInfoRepository.GetQuery();
            //仅限指派本部门未分配的客户 (含查看)、查看本部门所有客户
            if ( (CurrentUser.HasPermiss(74) || CurrentUser.HasPermiss(246) )&& !CurrentUser.IsSuper)
            {
                var UserId = getUserIdByDeptId(CurrentUser.DeptId);
                uer = SysUserInfoRepository.GetQuery(o => UserId.Contains(o.UserId));
            }
            return uer.ToList();
        }

        /// <summary>
        /// 获取业务员(查看所有客户的指派业务员)
        /// </summary>
        public List<SysUser> getAssignerUserList()
        {
            IQueryable<SysUser> uer = SysUserInfoRepository.GetQuery();
            //仅限指派本部门未分配的客户 (含查看)
            if (CurrentUser.HasPermiss(74) && !CurrentUser.IsSuper)
            {
                var UserId = getUserIdByDeptId(CurrentUser.DeptId);
                uer = SysUserInfoRepository.GetQuery(o => UserId.Contains(o.UserId));
            }
            return uer.ToList();
        }

        

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="traders">商户资料</param>
        /// <param name="h_OrderList">采购意向清单</param>
        /// <param name="h_VisitTrack">回访小结</param>
        /// <param name="h_VisitTrack">经营类目</param>
        /// <param name="Pay">支付方式</param>
        /// <returns></returns>
        public OpResult Save(Traders traders, string h_OrderList, string h_VisitTrack, string[] BusinessScopeId, string[] Pay)
        {
            OpResult op = SaveOrUpdate(traders, BusinessScopeId, Pay);
            if (op.Successed)
            {
                int CID = Convert.ToInt32(op.Message);
                if (CID > 0)
                {
                    if (!h_OrderList.IsNullOrEmpty())
                    {
                        //采购意向清单
                        JObject jObj = null;
                        jObj = JObject.Parse(h_OrderList);
                        JArray jlist = JArray.Parse(jObj["OrderList"].ToString());
                        orderListService.Deletes(CID);
                        foreach (JObject item in jlist)
                        {
                            short Id = Convert.ToInt16(item["Id"]);
                            string Title = item["Title"].ToString();
                            short OrderNum = Convert.ToInt16(item["OrderNum"]);
                            string uName = item["uName"].ToString();
                            int uId = Convert.ToInt32(item["uId"]);
                            string remark = item["remark"].ToString();

                            OrderList orderList = new OrderList();
                            orderList.CID = CID;
                            orderList.DeviceId = Id;
                            orderList.Title = Title;
                            orderList.OrderNum = OrderNum;
                            orderList.UnitName = uName;
                            orderList.UnitID = uId;
                            orderList.Remark = remark;
                            orderListService.SaveOrUpdate(orderList);
                        }
                    }
                    if (!h_VisitTrack.IsNullOrEmpty())
                    {
                        //回访小结
                        JObject jVisitTrack = null;
                        jVisitTrack = JObject.Parse(h_VisitTrack);
                        JArray jVisitTrackList = JArray.Parse(jVisitTrack["VisitTrack"].ToString());
                        visitTrackService.Deletes(CID);
                        foreach (JObject item in jVisitTrackList)
                        {
                            string content = item["content"].ToString();
                            string VisitDT = item["VisitDT"].ToString();
                            string CreateUID = item["CreateUID"].ToString();
                            DateTime addDT = Convert.ToDateTime(item["addDT"]);
                            DateTime upDT = Convert.ToDateTime(item["upDT"]);

                            VisitTrack visitTrack = new VisitTrack();
                            visitTrack.CID = CID;
                            visitTrack.Content = content;
                            visitTrack.VisitDT = VisitDT;
                            visitTrack.CreateUID = CreateUID;
                            visitTrack.CreateDT = addDT;
                            visitTrack.UpdateDT = upDT;
                            visitTrackService.SaveOrUpdate(visitTrack);
                        }
                    }
                    return OpResult.Success();
                }
                else
                {
                    return OpResult.Fail("保存失败");
                }  
            }
            else
            {
                return op;
            }
        }
        /// <summary>
        /// webapp保存
        /// </summary>
        /// <param name="traders"></param>
        /// <param name="orders"></param>
        /// <returns></returns>
        public OpResult AddTrader(Traders traders,List<OrderList> orders)
        {
            if (traders == null) return OpResult.Success("操作时间过长，请重新添加！");
            var user = SysUserInfoRepository.Find(o => o.LoginName == traders.Assigner);
            if (user == null) return OpResult.Fail("业务员不存在！");
            traders.AddType = 1;
            traders.CreateUID = user.UserId;
            traders.AssignerUID = traders.CreateUID;
            traders.CreateDT = DateTime.Now;
            traders.UpdateDT = DateTime.Now;
            traders.CID = getMaxCID();
            orders.Each(o =>
            {
                o.CID = traders.CID;
                o.CreateDT = traders.CreateDT;
                o.CreateUID = traders.CreateUID;
            });
            TradersRepository.Add(traders,false);
            try
            {
                orderListService.OrderListRepository.AddRange(orders);
            }
            catch(Exception ex)
            {
                LogEngine.WriteError(ex);
                return OpResult.Fail("保存出现异常！");
            }
            return OpResult.Success("添加成功！");
        }
        public OpResult Save(Traders traders, string h_OrderList, short way, short openVersionId, string storeProper, string posMinorDisp, string appProper, string machine)
        {
            if(traders.CID>0)
            {
                var td = TradersRepository.Find(o => o.CID == traders.CID);
                if (td != null)
                {
                    traders.Id = td.Id;
                    traders.TrackStautsId = td.TrackStautsId;
                    traders.Source = td.Source;
                }
            }
            var op = Save(traders, h_OrderList, "", traders.BusinessScopeId.IsNullOrEmpty() ? new string[] { } : traders.BusinessScopeId.Split(','), new string[] { });
            if(op.Successed)
            {
                op= CompAuthorService.SaveOrUpdate(new CompanyAuthorize()
                {
                    CID=traders.CID,
                    Title=traders.FullTitle,
                    Way=way,
                    StoreProper=storeProper,
                    PosMinorDisp=posMinorDisp,
                    AppProper=appProper,
                    StoreNum=Convert.ToInt16(traders.ExistStoreNum),
                    UserNum = Convert.ToInt16(traders.EachStorePersonNum),
                    BusinessMode=traders.BusinessModeId,
                    OpenScopeId=traders.BusinessScopeId,
                    OpenVersionId=openVersionId,
                    SupperAccount="",
                    SupperPassword="",
                    ContractNo="",
                    EffectiveDT=DateTime.Now.ToString("yyyy-MM-dd"),
                    ValidityNum=12,
                    MachineSN = machine
                });
            }
            return op;
        }
        /// <summary>
        /// 商户审核通过、设为无效商户
        /// </summary>
        public OpResult setStatus(string ids, short status)
        {
            var sId = ids.Split(',').Select(o => int.Parse(o));
            var olist = TradersRepository.GetQuery(o => sId.Contains(o.Id)).ToList();
            olist.Each(o => o.Status = status);
            return OpResult.Result(TradersRepository.SaveChanges());
        }

        /// <summary>
        /// 更新客户分类
        /// </summary>
        public OpResult upType(string ids, string TraderTypeId)
        {
            var sId = ids.Split(',').Select(o => int.Parse(o));
            var olist = TradersRepository.GetQuery(o => sId.Contains(o.Id)).ToList();
            olist.Each(o => o.TraderTypeId = TraderTypeId);
            return OpResult.Result(TradersRepository.SaveChanges());
        }


        /// <summary>
        /// 删除
        /// </summary>
        public Utility.OpResult Deletes(int[] ids)
        {
            var op = new OpResult();
            try
            {
                var tra = TradersRepository.GetQuery(o => ids.Contains(o.Id));
                var cid = tra.Select(o => o.CID).ToArray();
                var CompanyAuthorize = CompanyAuthorizeRepository.GetQuery(o => cid.Contains(o.CID ?? 0));
                if (CompanyAuthorize.Any())
                {
                    op.Message = "无法删除，软件服务包含了选择的商户";
                    return op;
                }

                var DAuthorize = DeviceAuthorizeRepository.GetQuery(o => cid.Contains(o.CID ?? 0));
                if (DAuthorize.Any())
                {
                    op.Message = "无法删除，设备服务包含了选择的商户";
                    return op;
                }

                var oList = OrderListRepository.GetQuery(o => cid.Contains(o.CID));
                var vList = VisitTrackRepository.GetQuery(o => cid.Contains(o.CID));
                

                OrderListRepository.RemoveRange(oList.ToList());
                VisitTrackRepository.RemoveRange(vList.ToList());
                TradersRepository.RemoveRange(tra.ToList());

                op.Successed = true;
                LogEngine.WriteDelete("删除商户", LogModule.商户资料);
            }
            catch (Exception ex)
            {
                op.Message = ex.Message;
                LogEngine.WriteError(ex);
            }
            return op;
        }

        /// <summary> 
        /// 判断给定的字符串(strNumber)是否是数值型 
        /// </summary> 
        /// <param name="strNumber">要确认的字符串</param> 
        /// <returns>是则返加true 不是则返回 false</returns> 
        public bool IsNumber(string strNumber)
        {
            return new Regex(@"^([0-9])[0-9]*(\.\w*)?$").IsMatch(strNumber);
        }

        readonly TraderCache traderCache = new TraderCache();
        public List<RedisTraders> ImportList(System.Collections.Specialized.NameValueCollection nvl, out int recordCount)
        {
            //省份
            var CurrentProvinceId = (nvl["CurrentProvinceId"] ?? "").Trim();
            //城市
            var CurrentCityId = (nvl["CurrentCityId"] ?? "").Trim();
            //区
            var CurrentCounty = (nvl["CurrentCounty"] ?? "").Trim();
            //跟踪状态
            var TrackStautsId = (nvl["TrackStautsId"] ?? "").Trim();
            //业务员
            var AssignerUID = (nvl["AssignerUID"] ?? "").Trim();
            //客户类型
            var BusinessModeId = (nvl["BusinessModeId"] ?? "").Trim();
            //经营范围
            var BusinessScopeId = (nvl["BusinessScopeId"] ?? "").Trim();
            //关键字类型
            var keywordType = (nvl["keywordType"] ?? "").Trim();
            //关键字
            var keyword = (nvl["keyword"] ?? "").Trim();
            var pageIndex = 1;
            var pageSize = 200;
            if (!nvl["page"].IsNullOrEmpty())
                pageIndex = int.Parse(nvl["page"]);
            if (!nvl["rows"].IsNullOrEmpty())
                pageSize = int.Parse(nvl["rows"]);

            List<RedisTraders> list = traderCache.Get(CacheKey);
            if (!AssignerUID.IsNullOrEmpty() && AssignerUID!="全部")
            {
                string[] aUID = AssignerUID.Split(',');
                list = list.Where(o=>aUID.Contains(o.AssignerUID)).ToList();
            }
            if (!BusinessModeId.IsNullOrEmpty() && BusinessModeId!="全部")
            {
                list = list.Where(o => o.BusinessModeId == BusinessModeId).ToList();
            }
            if (!TrackStautsId.IsNullOrEmpty() && TrackStautsId != "全部")
            {
                list = list.Where(o=>o.TrackStautsId==TrackStautsId).ToList();
            }
            if (!BusinessScopeId.IsNullOrEmpty() && BusinessScopeId != "全部")
            {
                list = list.Where(o => o.BusinessScopeId.Contains(BusinessScopeId)).ToList();
            }
            if (!CurrentProvinceId.IsNullOrEmpty() && CurrentProvinceId != "全部" && CurrentCityId == "全部" && CurrentCounty == "全部")
            {
                list = list.Where(o => o.Area.Contains(CurrentProvinceId)).ToList();
            }
            if (!CurrentProvinceId.IsNullOrEmpty() && CurrentProvinceId != "全部" && !CurrentCityId.IsNullOrEmpty() && CurrentCityId != "全部" && CurrentCounty == "全部")
            {
                list = list.Where(o => o.Area.Contains(CurrentProvinceId + "-" + CurrentCityId)).ToList();
            }
            if (!CurrentProvinceId.IsNullOrEmpty() && CurrentProvinceId != "全部" && !CurrentCityId.IsNullOrEmpty() && CurrentCityId != "全部" && !CurrentCounty.IsNullOrEmpty() && CurrentCounty != "全部")
            {
                list = list.Where(o => o.Area.Contains(CurrentProvinceId + "-" + CurrentCityId+"-"+CurrentCounty)).ToList();
            }
            if (!keywordType.IsNullOrEmpty() && !keyword.IsNullOrEmpty())
            {
                if (keywordType == "0")
                {
                    list = list.Where(o => o.Title.Contains(keyword)).ToList();
                }
                if (keywordType == "1")
                {
                    list = list.Where(o => o.LinkMan.Contains(keyword)).ToList();
                }
                if (keywordType == "2")
                {
                    list = list.Where(o => o.MobilePhone.Contains(keyword)).ToList();
                }
            }
            recordCount = list.Count();
            return list;
        }
        public OpResult Import(ImportSet obj, System.Web.HttpFileCollectionBase httpFiles, string fieldName, string columnName)
        {
            var op = new OpResult();
            var errLs = new List<string>();
            int count = 0;
            try
            {
                Dictionary<string, char> fieldCols = null;
                DataTable dt = null;
                op = ImportSetService.ImportSet(obj, httpFiles, fieldName, columnName, ref fieldCols, ref dt);
                if (!op.Successed) return op;

                //数据字典
                var dataDictionary = SysDataDictionaryRepository.GetQuery();

                //业务员
                int AssignerUIDIdx = -1;
                if (fieldCols.ContainsKey("AssignerUID"))
                {
                    AssignerUIDIdx = Convert.ToInt32(fieldCols["AssignerUID"]) - 65;
                }

                //客户简称
                int TitleIdx = -1;
                if (fieldCols.ContainsKey("Title"))
                {
                    TitleIdx = Convert.ToInt32(fieldCols["Title"]) - 65;
                }

                //客户全称
                int FullTitleIdx = -1;
                if (fieldCols.ContainsKey("FullTitle"))
                {
                    FullTitleIdx = Convert.ToInt32(fieldCols["FullTitle"]) - 65;
                }

                //客户类型
                int BusinessModeIdIdx = -1;
                if (fieldCols.ContainsKey("BusinessModeId"))
                {
                    BusinessModeIdIdx = Convert.ToInt32(fieldCols["BusinessModeId"]) - 65;
                }

                //客户来源
                int SourceIdx = -1;
                if (fieldCols.ContainsKey("Source"))
                {
                    SourceIdx = Convert.ToInt32(fieldCols["Source"]) - 65;
                }

                //更进状态
                int TrackStautsIdIdx = -1;
                if (fieldCols.ContainsKey("TrackStautsId"))
                {
                    TrackStautsIdIdx = Convert.ToInt32(fieldCols["TrackStautsId"]) - 65;
                }

                //联系人
                int LinkManIdx = -1;
                if (fieldCols.ContainsKey("LinkMan"))
                {
                    LinkManIdx = Convert.ToInt32(fieldCols["LinkMan"]) - 65;
                }

                //电话/手机
                int MobilePhoneIdx = -1;
                if (fieldCols.ContainsKey("MobilePhone"))
                {
                    MobilePhoneIdx = Convert.ToInt32(fieldCols["MobilePhone"]) - 65;
                }

                //经营类别
                int BusinessScopeIdIdx = -1;
                if (fieldCols.ContainsKey("BusinessScopeId"))
                {
                    BusinessScopeIdIdx = Convert.ToInt32(fieldCols["BusinessScopeId"]) - 65;
                }

                //支付方式
                int PayIdx = -1;
                if (fieldCols.ContainsKey("Pay"))
                {
                    PayIdx = Convert.ToInt32(fieldCols["Pay"]) - 65;
                }

                //所在区域
                int AreaIdx = -1;
                if (fieldCols.ContainsKey("Area"))
                {
                    AreaIdx = Convert.ToInt32(fieldCols["Area"]) - 65;
                }

                //街道地址
                int AddressIdx = -1;
                if (fieldCols.ContainsKey("Address"))
                {
                    AddressIdx = Convert.ToInt32(fieldCols["Address"]) - 65;
                }

                //现有系统名称/品牌
                int ExistsystemNameIdx = -1;
                if (fieldCols.ContainsKey("ExistsystemName"))
                {
                    ExistsystemNameIdx = Convert.ToInt32(fieldCols["ExistsystemName"]) - 65;
                }

                //现有设备名称/品牌
                int ExistDeviceNameIdx = -1;
                if (fieldCols.ContainsKey("ExistDeviceName"))
                {
                    ExistDeviceNameIdx = Convert.ToInt32(fieldCols["ExistDeviceName"]) - 65;
                }

                //现有门店数量
                int ExistStoreNumIdx = -1;
                if (fieldCols.ContainsKey("ExistStoreNum"))
                {
                    ExistStoreNumIdx = Convert.ToInt32(fieldCols["ExistStoreNum"]) - 65;
                }

                //每门店收银电脑台数
                int EachStorePosNumIdx = -1;
                if (fieldCols.ContainsKey("EachStorePosNum"))
                {
                    EachStorePosNumIdx = Convert.ToInt32(fieldCols["EachStorePosNum"]) - 65;
                }

                //每门店人均数
                int EachStorePersonNumIdx = -1;
                if (fieldCols.ContainsKey("EachStorePersonNum"))
                {
                    EachStorePersonNumIdx = Convert.ToInt32(fieldCols["EachStorePersonNum"]) - 65;
                }

                //近2年计划扩张门店数量
                int PlanExpandStoreNumIdx = -1;
                if (fieldCols.ContainsKey("PlanExpandStoreNum"))
                {
                    PlanExpandStoreNumIdx = Convert.ToInt32(fieldCols["PlanExpandStoreNum"]) - 65;
                }

                //竞争对手
                int RivalIdx = -1;
                if (fieldCols.ContainsKey("Rival"))
                {
                    RivalIdx = Convert.ToInt32(fieldCols["Rival"]) - 65;
                }

                //竞争对手营销模式
                int MarketingIdx = -1;
                if (fieldCols.ContainsKey("Marketing"))
                {
                    MarketingIdx = Convert.ToInt32(fieldCols["Marketing"]) - 65;
                }

                //备注说明
                int MemoIdx = -1;
                if (fieldCols.ContainsKey("Memo"))
                {
                    MemoIdx = Convert.ToInt32(fieldCols["Memo"]) - 65;
                }

                count = dt.Rows.Count;
                for (int i = dt.Rows.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        var dr = dt.Rows[i];
                        #region 验证

                        //业务员
                        var AssignerUIDText = dr[AssignerUIDIdx].ToString().Trim();
                        if (AssignerUIDText.IsNullOrEmpty())
                        {
                            errLs.Add("行号[" + (i + 1) + "]业务员为空!");
                            dt.Rows.RemoveAt(i);
                            continue;
                        }
                        else
                        {
                            var user = SysUserInfoRepository.GetQuery(o=>o.FullName==AssignerUIDText);
                            if (!user.Any())
                            {
                                errLs.Add("行号[" + (i + 1) + "]业务员不存在!");
                                dt.Rows.RemoveAt(i);
                                continue;
                            }
                        }

                        //客户简称
                        var TitleText = dr[TitleIdx].ToString().Trim();
                        if (TitleText.IsNullOrEmpty())
                        {
                            errLs.Add("行号[" + (i + 1) + "]客户简称为空!");
                            dt.Rows.RemoveAt(i);
                            continue;
                        }
                        else
                        {
                            if (TitleText.Length < 2 || TitleText.Length > 10)
                            {
                                errLs.Add("行号[" + (i + 1) + "]客户简称没有在2-10个字符之间!");
                                dt.Rows.RemoveAt(i);
                                continue;
                            }
                            var Title = TradersRepository.GetQuery(o=>o.Title==TitleText);
                            if (Title.Any())
                            {
                                errLs.Add("行号[" + (i + 1) + "]客户简称已经存在!");
                                dt.Rows.RemoveAt(i);
                                continue;
                            }
                        }

                        //客户全称
                        var FullTitleText = dr[FullTitleIdx].ToString().Trim();
                        if (FullTitleText.IsNullOrEmpty())
                        {
                            errLs.Add("行号[" + (i + 1) + "]客户全称为空!");
                            dt.Rows.RemoveAt(i);
                            continue;
                        }
                        else
                        {
                            if (FullTitleText.Length < 2 || FullTitleText.Length > 50)
                            {
                                errLs.Add("行号[" + (i + 1) + "]客户全称没有在2-50个字符之间!");
                                dt.Rows.RemoveAt(i);
                                continue;
                            }
                            var FullTitle = TradersRepository.GetQuery(o => o.FullTitle == FullTitleText);
                            if (FullTitle.Any())
                            {
                                errLs.Add("行号[" + (i + 1) + "]客户全称已经存在!");
                                dt.Rows.RemoveAt(i);
                                continue;
                            }
                        }

                        //客户类型
                        var BusinessModeIdText = dr[BusinessModeIdIdx].ToString().Trim();
                        if (BusinessModeIdText.IsNullOrEmpty())
                        {
                            errLs.Add("行号[" + (i + 1) + "]客户类型为空!");
                            dt.Rows.RemoveAt(i);
                            continue;
                        }
                        else
                        {
                            var BusinessModeId = dataDictionary.Where(o => o.Title == BusinessModeIdText&&o.Status);
                            if (!BusinessModeId.Any())
                            {
                                errLs.Add("行号[" + (i + 1) + "]客户类型在字典中不存在!");
                                dt.Rows.RemoveAt(i);
                                continue;
                            }
                        }

                        //客户来源
                        var SourceText = dr[SourceIdx].ToString().Trim();
                        if (SourceText.IsNullOrEmpty())
                        {
                            errLs.Add("行号[" + (i + 1) + "]客户来源为空!");
                            dt.Rows.RemoveAt(i);
                            continue;
                        }
                        else
                        {
                            if (SourceText.Length > 20)
                            {
                                errLs.Add("行号[" + (i + 1) + "]客户来源超过20个字符!");
                                dt.Rows.RemoveAt(i);
                                continue;
                            }
                        }

                        //跟进状态
                        var TrackStautsIdText = dr[TrackStautsIdIdx].ToString().Trim();
                        if (TrackStautsIdText.IsNullOrEmpty())
                        {
                            errLs.Add("行号[" + (i + 1) + "]跟进状态为空!");
                            dt.Rows.RemoveAt(i);
                            continue;
                        }
                        else
                        {
                            var TrackStautsId = dataDictionary.Where(o => o.Title == TrackStautsIdText&&o.Status);
                            if (!TrackStautsId.Any())
                            {
                                errLs.Add("行号[" + (i + 1) + "]跟进状态在字典中不存在!");
                                dt.Rows.RemoveAt(i);
                                continue;
                            }
                        }

                        //联系人
                        var LinkManText = dr[LinkManIdx].ToString().Trim();
                        if (LinkManText.IsNullOrEmpty())
                        {
                            errLs.Add("行号[" + (i + 1) + "]联系人为空!");
                            dt.Rows.RemoveAt(i);
                            continue;
                        }


                        //电话/手机
                        if (MobilePhoneIdx != -1)
                        {
                            var MobilePhoneText = dr[MobilePhoneIdx].ToString().Trim();
                            if (!MobilePhoneText.IsNullOrEmpty())
                            {
                                if (MobilePhoneText.Length < 7 || MobilePhoneText.Length > 20)
                                {
                                    errLs.Add("行号[" + (i + 1) + "]电话/手机没有在7-20个字符之间!");
                                    dt.Rows.RemoveAt(i);
                                    continue;
                                }
                            }
                        }

                        //经营类别
                        if (BusinessScopeIdIdx != -1)
                        {
                            var BusinessScopeIdText = dr[BusinessScopeIdIdx].ToString().Trim();
                            if (!BusinessScopeIdText.IsNullOrEmpty())
                            {
                                if (BusinessScopeIdText.Contains("|"))
                                {
                                    string[] BusinessScopeIdArray = BusinessScopeIdText.Split('|');
                                    string err = "";
                                    for (int j = 0; j < BusinessScopeIdArray.Length; j++)
                                    {
                                        if (!BusinessScopeIdArray[j].IsNullOrEmpty())
                                        {
                                            var BusinessScopeId = dataDictionary.Where(o => o.Title == BusinessScopeIdArray[j] && o.Status);
                                            if (!BusinessScopeId.Any())
                                            {
                                                err = "行号[" + (i + 1) + "]经营类别（" + BusinessScopeIdArray[j] + "）在字典中不存在!";
                                                break;
                                            }
                                        }
                                    }
                                    if (err != "")
                                    {
                                        errLs.Add(err);
                                        dt.Rows.RemoveAt(i);
                                        continue;
                                    }
                                }
                                else
                                {
                                    var BusinessScopeId = dataDictionary.Where(o => o.Title == BusinessScopeIdText && o.Status);
                                    if (!BusinessScopeId.Any())
                                    {
                                        errLs.Add("行号[" + (i + 1) + "]经营类别（" + BusinessScopeIdText + "）在字典中不存在!");
                                        dt.Rows.RemoveAt(i);
                                        continue;
                                    }
                                }
                            }
                        }


                        //支付方式
                        if (PayIdx != -1)
                        {
                            var PayText = dr[PayIdx].ToString().Trim();
                            if (!PayText.IsNullOrEmpty())
                            {
                                if (PayText.Contains("|"))
                                {
                                    string[] PayArray = PayText.Split('|');
                                    string err = "";
                                    for (int j = 0; j < PayArray.Length; j++)
                                    {
                                        if (!PayArray[j].IsNullOrEmpty())
                                        {
                                            var Pay = dataDictionary.Where(o => o.Title == PayArray[j] && o.Status);
                                            if (!Pay.Any())
                                            {
                                                err = "行号[" + (i + 1) + "]支付方式（" + PayArray[j] + "）在字典中不存在!";
                                                break;
                                            }
                                        }
                                    }
                                    if (err != "")
                                    {
                                        errLs.Add(err);
                                        dt.Rows.RemoveAt(i);
                                        continue;
                                    }
                                }
                                else
                                {
                                    var Pay = dataDictionary.Where(o => o.Title == PayText && o.Status);
                                    if (!Pay.Any())
                                    {
                                        errLs.Add("行号[" + (i + 1) + "]支付方式（" + PayText + "）在字典中不存在!");
                                        dt.Rows.RemoveAt(i);
                                        continue;
                                    }
                                }

                            }
                        }

                        //所在区域
                        var AreaText = dr[AreaIdx].ToString().Trim();
                        if (!AreaText.IsNullOrEmpty())
                        {
                            if (AreaText.Contains("-"))
                            {
                                string[] AreaArray = AreaText.Split('-');
                                if (AreaArray.Length == 3)
                                {
                                    string err = "";
                                    for (int j = 0; j < AreaArray.Length; j++)
                                    {
                                        if (!AreaArray[j].IsNullOrEmpty())
                                        {
                                            var Area = AreaRepository.GetQuery(o => o.Title == AreaArray[j]);
                                            if (!Area.Any())
                                            {
                                                err = "行号[" + (i + 1) + "]所在区域不存在!";
                                                break; 
                                            }
                                        }
                                        else
                                        {
                                            err = "行号[" + (i + 1) + "]所在区域格式不正确!";
                                            break;
                                        }
                                    }
                                    if (err != "")
                                    {
                                        errLs.Add(err);
                                        dt.Rows.RemoveAt(i);
                                        continue;
                                    }
                                }
                                else
                                {
                                    errLs.Add("行号[" + (i + 1) + "]所在区域格式不正确!");
                                    dt.Rows.RemoveAt(i);
                                    continue;
                                }
                            }
                            else
                            {
                                errLs.Add("行号[" + (i + 1) + "]所在区域格式不正确!");
                                dt.Rows.RemoveAt(i);
                                continue;
                            }
                        }
                        else
                        {
                            errLs.Add("行号[" + (i + 1) + "]所在区域为空!");
                            dt.Rows.RemoveAt(i);
                            continue;
                        }

                        //现有系统名称/品牌
                        if (ExistsystemNameIdx != -1)
                        {
                            var ExistsystemNameText = dr[ExistsystemNameIdx].ToString().Trim();
                            if (!ExistsystemNameText.IsNullOrEmpty())
                            {
                                if (ExistsystemNameText.Length > 50 )
                                {
                                    errLs.Add("行号[" + (i + 1) + "]现有系统名称/品牌超过50个字符");
                                    dt.Rows.RemoveAt(i);
                                    continue;
                                }
                            }
                        }

                        //现有设备名称/品牌
                        if (ExistDeviceNameIdx != -1)
                        {
                            var ExistDeviceNameText = dr[ExistDeviceNameIdx].ToString().Trim();
                            if (!ExistDeviceNameText.IsNullOrEmpty())
                            {
                                if (ExistDeviceNameText.Length > 50)
                                {
                                    errLs.Add("行号[" + (i + 1) + "]现有设备名称/品牌超过50个字符");
                                    dt.Rows.RemoveAt(i);
                                    continue;
                                }
                            }
                        }

                        //现有门店数量
                        if (ExistStoreNumIdx != -1)
                        {
                            var ExistStoreNumText = dr[ExistStoreNumIdx].ToString().Trim();
                            if (!ExistStoreNumText.IsNullOrEmpty())
                            {
                                var ExistStoreNum = dataDictionary.Where(o => o.Title == ExistStoreNumText && o.Status);
                                if (!ExistStoreNum.Any())
                                {
                                    errLs.Add("行号[" + (i + 1) + "]现有门店数量在字典中不存在!");
                                    dt.Rows.RemoveAt(i);
                                    continue;
                                }
                            }
                        }

                        //每门店收银电脑台数
                        if (EachStorePosNumIdx != -1)
                        {
                            var EachStorePosNumText = dr[EachStorePosNumIdx].ToString().Trim();
                            if (!EachStorePosNumText.IsNullOrEmpty())
                            {
                                var EachStorePosNum = dataDictionary.Where(o => o.Title == EachStorePosNumText && o.Status);
                                if (!EachStorePosNum.Any())
                                {
                                    errLs.Add("行号[" + (i + 1) + "]每门店收银电脑台数在字典中不存在!");
                                    dt.Rows.RemoveAt(i);
                                    continue;
                                }
                            }
                        }

                        //每门店人均数
                        if (EachStorePersonNumIdx != -1)
                        {
                            var EachStorePersonNumText = dr[EachStorePersonNumIdx].ToString().Trim();
                            if (!EachStorePersonNumText.IsNullOrEmpty())
                            {
                                var EachStorePersonNum = dataDictionary.Where(o => o.Title == EachStorePersonNumText && o.Status);
                                if (!EachStorePersonNum.Any())
                                {
                                    errLs.Add("行号[" + (i + 1) + "]每门店人均数在字典中不存在!");
                                    dt.Rows.RemoveAt(i);
                                    continue;
                                }
                            }
                        }

                        //近2年计划扩张门店数量
                        if (PlanExpandStoreNumIdx != -1)
                        {
                            var PlanExpandStoreNumText = dr[PlanExpandStoreNumIdx].ToString().Trim();
                            if (!PlanExpandStoreNumText.IsNullOrEmpty())
                            {
                                var PlanExpandStoreNum = dataDictionary.Where(o => o.Title == PlanExpandStoreNumText && o.Status);
                                if (!PlanExpandStoreNum.Any())
                                {
                                    errLs.Add("行号[" + (i + 1) + "]近2年计划扩张门店数量在字典中不存在!");
                                    dt.Rows.RemoveAt(i);
                                    continue;
                                }
                            }
                        }

                        //竞争对手
                        if (RivalIdx != -1)
                        {
                            var RivalText = dr[RivalIdx].ToString().Trim();
                            if (!RivalText.IsNullOrEmpty())
                            {
                                if (RivalText.Length > 50)
                                {
                                    errLs.Add("行号[" + (i + 1) + "]竞争对手超过50个字符");
                                    dt.Rows.RemoveAt(i);
                                    continue;
                                }
                            }
                        }

                        //竞争对手营销模式
                        if (MarketingIdx != -1)
                        {
                            var MarketingText = dr[MarketingIdx].ToString().Trim();
                            if (!MarketingText.IsNullOrEmpty())
                            {
                                if (MarketingText.Length > 50)
                                {
                                    errLs.Add("行号[" + (i + 1) + "]竞争对手营销模式超过50个字符");
                                    dt.Rows.RemoveAt(i);
                                    continue;
                                }
                            }
                        }

                        //备注说明
                        if (MemoIdx != -1)
                        {
                            var MemoText = dr[MemoIdx].ToString().Trim();
                            if (!MemoText.IsNullOrEmpty())
                            {
                                if (MemoText.Length > 200)
                                {
                                    errLs.Add("行号[" + (i + 1) + "]备注说明超过200个字符");
                                    dt.Rows.RemoveAt(i);
                                    continue;
                                }
                            }
                        }

                        #endregion
                    }
                    catch (Exception e)
                    {
                        throw new Exception("导入出现异常," + e.Message, e);
                    }
                }

                if (dt.Rows.Count > 0)
                {
                    List<RedisTraders> list = new List<RedisTraders>();
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        try
                        {
                            var dr = dt.Rows[j];
                            RedisTraders redisTraders = new RedisTraders();

                            //Id
                            var Id = j + 1;
                            //业务员
                            var AssignerUIDText = dr[AssignerUIDIdx].ToString().Trim();
                            //客户简称
                            var TitleText = dr[TitleIdx].ToString().Trim();
                            //客户全称
                            var FullTitleText = dr[FullTitleIdx].ToString().Trim();
                            //客户类型
                            var BusinessModeIdText = dr[BusinessModeIdIdx].ToString().Trim();
                            //客户来源
                            var SourceText = dr[SourceIdx].ToString().Trim();
                            //跟进状态
                            var TrackStautsIdText = dr[TrackStautsIdIdx].ToString().Trim();
                            //联系人
                            var LinkManText = dr[LinkManIdx].ToString().Trim();
                            //电话/手机
                            var MobilePhoneText = "";
                            if (MobilePhoneIdx != -1)
                            {
                                MobilePhoneText = dr[MobilePhoneIdx].ToString().Trim();
                            }
                            //经营类别
                            var BusinessScopeIdText = "";
                            if (BusinessScopeIdIdx != -1)
                            {
                                BusinessScopeIdText = dr[BusinessScopeIdIdx].ToString().Trim();
                            }
                            //支付方式
                            var PayText = "";
                            if (PayIdx != -1)
                            {
                                PayText = dr[PayIdx].ToString().Trim();
                            }
                            //所在区域
                            var AreaText = dr[AreaIdx].ToString().Trim();
                            //现有系统名称/品牌
                            var ExistsystemNameText = "";
                            if (ExistsystemNameIdx != -1)
                            {
                                ExistsystemNameText = dr[ExistsystemNameIdx].ToString().Trim();
                            }
                            //现有设备名称/品牌
                            var ExistDeviceNameText = "";
                            if (ExistDeviceNameIdx != -1)
                            {
                                ExistDeviceNameText = dr[ExistDeviceNameIdx].ToString().Trim();
                            }
                            //现有门店数量
                            var ExistStoreNumText = "";
                            if (ExistStoreNumIdx != -1)
                            {
                                ExistStoreNumText = dr[ExistStoreNumIdx].ToString().Trim();
                            }
                            //每门店收银电脑台数
                            var EachStorePosNumText = "";
                            if (EachStorePosNumIdx != -1)
                            {
                                EachStorePosNumText = dr[EachStorePosNumIdx].ToString().Trim();
                            }
                            //每门店人均数
                            var EachStorePersonNumText = "";
                            if (EachStorePersonNumIdx != -1)
                            {
                                EachStorePersonNumText = dr[EachStorePersonNumIdx].ToString().Trim();
                            }
                            //近2年计划扩张门店数量
                            var PlanExpandStoreNumText = "";
                            if (PlanExpandStoreNumIdx != -1)
                            {
                                PlanExpandStoreNumText = dr[PlanExpandStoreNumIdx].ToString().Trim();
                            }
                            //竞争对手
                            var RivalText = "";
                            if (RivalIdx != -1)
                            {
                                RivalText = dr[RivalIdx].ToString().Trim();
                            }
                            //竞争对手营销模式
                            var MarketingText = "";
                            if (MarketingIdx != -1)
                            {
                                MarketingText = dr[MarketingIdx].ToString().Trim();
                            }
                            //备注说明
                            var MemoText = "";
                            if (MemoIdx != -1)
                            {
                                MemoText = dr[MemoIdx].ToString().Trim();
                            }

                            redisTraders.Id = Id;
                            redisTraders.AssignerUID = AssignerUIDText;
                            redisTraders.Title = TitleText;
                            redisTraders.FullTitle = FullTitleText;
                            redisTraders.BusinessModeId = BusinessModeIdText;
                            redisTraders.Source = SourceText;
                            redisTraders.TrackStautsId = TrackStautsIdText;
                            redisTraders.LinkMan = LinkManText;
                            redisTraders.MobilePhone = MobilePhoneText;
                            redisTraders.BusinessScopeId = BusinessScopeIdText;
                            redisTraders.Pay = PayText;
                            redisTraders.Area = AreaText;
                            redisTraders.ExistsystemName = ExistsystemNameText;
                            redisTraders.ExistDeviceName = ExistDeviceNameText;
                            redisTraders.ExistStoreNum = ExistStoreNumText;
                            redisTraders.EachStorePosNum = EachStorePosNumText;
                            redisTraders.EachStorePersonNum = EachStorePersonNumText;
                            redisTraders.PlanExpandStoreNum = PlanExpandStoreNumText;
                            redisTraders.Rival = RivalText;
                            redisTraders.Marketing = MarketingText;
                            redisTraders.Memo = MemoText;
                            list.Add(redisTraders);
                        }
                        catch (Exception e)
                        {
                            throw new Exception("导入出现异常," + e.Message, e);
                        }
                       
                    }
                    traderCache.Set(CacheKey, list);
                }
            }
            catch (Exception ex)
            {
                op.Message = ex.Message;
                op.Successed = false;
                LogEngine.WriteError(ex);
                errLs.Add("导入出现异常!");
            }
            return CommonService.GenerateImportHtml(errLs, count);
        }

        /// <summary>
        /// 删除导入
        /// </summary>
        public OpResult DeleteImport(int[] ids)
        {
            var op = new OpResult();
            try
            {
                List<RedisTraders> list = traderCache.Get(CacheKey);
                if (ids.Length > 0)
                {
                    for (int i = 0; i < ids.Length; i++)
                    {
                        list.Remove(list.Where(o => o.Id == ids[i]).SingleOrDefault());
                    }
                }
                traderCache.Set(CacheKey, list);
                op.Successed = true;
            }
            catch (Exception ex)
            {
                op.Message = ex.Message;
                LogEngine.WriteError(ex);
            }
            return op;
        }

        public OpResult SureImport()
        {
            var op = new OpResult();
            //try
            //{
            //    List<RedisTraders> list = traderCache.Get(CacheKey);
            //    if (list == null || list.Count == 0)
            //    {
            //        return OpResult.Fail("预览已过期，请重新选择导入！");
            //    }
            //    for (int i = 0; i < list.Count; i++)
            //    {
                    
            //    }

            //    if (op.Successed)
            //    {
            //         saleOrderCache.Remove(CacheKey);
            //         Log.WriteInsert("销售数据导入", Sys.LogModule.其他);
            //         var stores = string.Join(",", saleOrders.Select(o => o.StoreId).Distinct());
            //         Pharos.Infrastructure.Data.Redis.RedisManager.Publish("SyncDatabase", new Pharos.ObjectModels.DTOs.DatabaseChanged() { CompanyId = Sys.SysCommonRules.CompanyId, StoreId = stores, Target = "SalePackage" });
            //    }
            //}
            //catch (Exception ex)
            //{
            //    op.Message = ex.Message;
            //    op.Successed = false;
            //    LogEngine.WriteError(ex);
            //}
            return op;
        }
        public OpResult DeleteAllImport()
        {
            OpResult op = new OpResult();
            try
            {
                traderCache.Remove(CacheKey);
                op.Successed = true;
            }
            catch (Exception ex)
            {
                op.Message = ex.Message;
                LogEngine.WriteError(ex);
            }
            return op;
        }
        string CacheKey { get { return KeyFactory.UserKeyFactory(0, CurrentUser.UID) + "_"; } }

        /// <summary>
        /// 获取当前登录用户所在部门(包括该部门、该部门所有子部门)所有人员ID
        /// </summary>
        /// <param name="DeptId"></param>
        /// <returns></returns>
        public IEnumerable<string> getUserIdByDeptId(int DeptId)
        {
            var dep = tRepository.getDepartID(CurrentUser.DeptId).Select(o => o.DeptId);
            var UserId = SysUserRepository.GetQuery(o => dep.Contains(o.DeptId)).ToList().Select(o => o.UserId);
            return UserId;
        }

        /// <summary>
        /// 指派业务员
        /// </summary>
        public OpResult UpAssignerUID(string ids, string AssignerUID)
        {
            var list = getAssignerUserList().Where(o => o.UserId == AssignerUID);
            if (!list.Any())
            {
                return OpResult.Fail("业务员不正确！");
            }
            var sId = ids.Split(',').Select(o => int.Parse(o));
            return UpListByWhere(o=>sId.Contains(o.Id),o=>o.AssignerUID = AssignerUID);
        }
    }
}
