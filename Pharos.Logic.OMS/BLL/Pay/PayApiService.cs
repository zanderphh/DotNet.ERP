﻿using Pharos.Logic.OMS.Entity;
using Pharos.Logic.OMS.IDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pharos.Utility.Helpers;
using Pharos.Utility;
using QCT.Pay.Common;

namespace Pharos.Logic.OMS.BLL
{
    /// <summary>
    /// 金融接口-支付接口相关业务操作
    /// </summary>
    public class PayApiService
    {
        [Ninject.Inject]
        IBaseRepository<PayApi> PayApiRepository { get; set; }
        [Ninject.Inject]
        IBaseRepository<PayChannelManage> PayChannelReposit { get; set; }
        [Ninject.Inject]
        IBaseRepository<SysUser> UserRepository { get; set; }

        /// <summary>
        /// 支付接口列表-获取分页数据
        /// </summary>
        /// <returns></returns>
        public IEnumerable<dynamic> GetPayApiPaging(System.Collections.Specialized.NameValueCollection nvl, out int totalCount)
        {
            var pms = new
            {
                ChannelNo = nvl["ChannelNo"] == null ? 0 : nvl["ChannelNo"].ToObject<int>(),
                State = nvl["State"] == null ? new List<short>() : nvl["State"].Split(',').Select(o => short.Parse(o)).ToList()
            };
            var query = PayApiRepository.GetQuery();
            if (pms.ChannelNo > 0)
                query = query.Where(o => o.ChannelNo == pms.ChannelNo);
            if (pms.State.Count > 0)
                query = query.Where(o => pms.State.Contains(o.State));
            else
            {
                query = query.Where(o => o.State != (short)PayApiState.Expired);
            }

            query = from upay in query
                    join juc in UserRepository.GetQuery() on upay.CreateUID equals juc.UserId into iuc
                    from uc in iuc.DefaultIfEmpty()
                    join jur in UserRepository.GetQuery() on upay.CreateUID equals jur.UserId into iur
                    from ur in iur.DefaultIfEmpty()
                    join jpc in PayChannelReposit.GetQuery() on upay.ChannelNo equals jpc.ChannelNo into ipc
                    from pc in ipc.DefaultIfEmpty()
                    select new PayApiExt()
                    {
                        Id = upay.Id,
                        Method = upay.Method,
                        Title = upay.Title,
                        ApiNo = upay.ApiNo,
                        ApiUrl = upay.ApiUrl,
                        Version = upay.Version,
                        State = upay.State,
                        TradeMode = upay.TradeMode,
                        Memo = upay.Memo,
                        ChannelNo = upay.ChannelNo,
                        ChannelCode = pc.ChannelCode,
                        OptType = upay.OptType,
                        CreateDT = upay.CreateDT,
                        CreateUID = upay.CreateUID,
                        Creater = uc.FullName,
                        ReleasedDT = upay.ReleasedDT,
                        ReleasedUID = upay.ReleasedUID,
                        Releaseder = ur.FullName
                    };
            totalCount = query.Count();
            return query.ToPageList();
        }

        /// <summary>
        /// 获取PayApi支付表里已有支付通道的收单渠道list
        /// </summary>
        /// <returns></returns>
        public List<DropdownItem> GetPayChannels()
        {
            var result = new List<DropdownItem>();
            var query = from pa in PayApiRepository.GetQuery()
                        join jpc in PayChannelReposit.GetQuery() on pa.ChannelNo equals jpc.ChannelNo into ipc
                        from pc in ipc.DefaultIfEmpty()
                        where pa.State == 1
                        select new { Text = pc.ChannelCode, Value = pa.ChannelNo };
            var data = query.Distinct().ToList();
            if (data != null && data.Count > 0)
            {
                data.ForEach(o => { result.Add(new DropdownItem() { Text = o.Text, Value = o.Value.ToString() }); });
            }
            return result;
        }
        /// <summary>
        /// 支付接口列表-新增或编辑支付接口表单-保存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public OpResult SaveOrUpdate(PayApi model)
        {
            var source = model;
            var existsObj = PayApiRepository.GetQuery(o => o.Id != model.Id && o.ChannelNo==model.ChannelNo && o.Method == model.Method).FirstOrDefault();
            if (existsObj != null)
                return OpResult.Fail(message: "接口参数名称已经存在，不可重复");
            if (source.Id > 0)
            {
                source = PayApiRepository.GetQuery(o => o.Id == model.Id).FirstOrDefault();
                model.ToCopyProperty(source, new List<string>() { "Id", "ApiNo", "ChannelNo", "State","Version", "CreateDT", "CreateUID", "ReleasedDT", "Releaseder" });
            }
            else
            {
                var existsCodeObj = PayApiRepository.GetQuery(o => o.ChannelNo==model.ChannelNo && o.Method == model.Method && o.Version==model.Version).FirstOrDefault();
                if (existsCodeObj != null)
                    return OpResult.Fail(message: "接口参数名称已经存在，不可重复");

                source.CreateDT = DateTime.Now;
                source.CreateUID = CurrentUser.UID;
                source.ApiNo = PayRules.GetMaxNo("PayApis", "ApiNo");
                PayApiRepository.Add(source, false);
            }

            var result = PayApiRepository.SaveChanges();
            if (result)
                return OpResult.Success(data: source);
            else
                return OpResult.Fail(message: "数据未修改不可保存");
        }
        /// <summary>
        /// 支付接口列表-新增或编辑支付接口表单-获取支付接口Model
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PayApiExt GetOne(int id)
        {
            var obj = (from upay in PayApiRepository.GetQuery(o => o.Id == id)
                       join juc in UserRepository.GetQuery() on upay.CreateUID equals juc.UserId into iuc
                       from uc in iuc.DefaultIfEmpty()
                       join jur in UserRepository.GetQuery() on upay.CreateUID equals jur.UserId into iur
                       from ur in iur.DefaultIfEmpty()
                       select new PayApiExt()
                       {
                           Id = upay.Id,
                           Method = upay.Method,
                           Title = upay.Title,
                           ApiNo = upay.ApiNo,
                           ApiUrl = upay.ApiUrl,
                           Version = upay.Version,
                           State = upay.State,
                           ChannelPayMode = upay.ChannelPayMode,
                           TradeMode = upay.TradeMode,
                           OptType = upay.OptType,
                           Memo = upay.Memo,
                           ChannelNo = upay.ChannelNo,
                           CreateDT = upay.CreateDT,
                           CreateUID = upay.CreateUID,
                           Creater = uc.FullName,
                           ReleasedDT = upay.ReleasedDT,
                           ReleasedUID = upay.ReleasedUID,
                           Releaseder = ur.FullName
                       }).FirstOrDefault();
            if (obj == null)
                obj = new PayApiExt();
            return obj;
        }
        /// <summary>
        /// 设置启用或关闭接口
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public OpResult SetState(int id, short state)
        {
            var entity = PayApiRepository.GetQuery(o => o.Id == id).FirstOrDefault();
            if (entity != null)
            {
                entity.State = state;
                entity.ReleasedDT = DateTime.Now;
                entity.ReleasedUID = CurrentUser.UID;
                return OpResult.Result(PayApiRepository.SaveChanges());
            }
            else
            {
                return OpResult.Fail("所选项状态已失效！");
            }
        }
        /// <summary>
        /// 删除PayApi
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public OpResult RemovePayApi(int id) {
            var entity = PayApiRepository.GetQuery(o => o.Id == id && o.State == (short)PayApiState.NotReleased).FirstOrDefault();
            if (entity != null)
            {//未发布状态的通道才可以删除
                PayApiRepository.Remove(entity, true);
                return OpResult.Result(true);
            }
            else
            {
                return OpResult.Fail("所选项状态已失效！");
            }
        }
    }
}
