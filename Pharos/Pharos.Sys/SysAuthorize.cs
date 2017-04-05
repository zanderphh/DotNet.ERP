﻿// --------------------------------------------------
// Copyright (C) 2015 版权所有
// 创 建 人：蔡少发
// 创建时间：2016-03-05
// 描述信息：系统授权许可
// --------------------------------------------------

using System;
using System.IO;
using System.Web;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Pharos.Utility;
using Pharos.Utility.Helpers;
using Pharos.Sys.Entity;
using Pharos.Utility.Caching;
using Pharos.Sys.BLL;
using AX.CSF.Encrypt;

namespace Pharos.Sys
{
    /// <summary>
    /// 系统授权许可
    /// </summary>
    public class SysAuthorize
    {
        /// <summary>
        /// 注册请求
        /// </summary>
        /// <param name="company">单位简称</param>
        /// <param name="fullCompany">单位全称</param>
        /// <returns>true:请求成功，false:请求失败</returns>
        public OpResult Register(OMS_CompanyAuthorize obj)
        {
            //todo:
            //1. 获取该服务器机器码
            obj.MachineSN = Machine.GetMAC;
            obj.Useable = "Y";
            //2. 将机器码、单位名称 提交给OMS生成客户授权
            var op= OMSCompanyAuthrizeBLL.Update(obj);
            var config = new Pharos.Utility.Config();
            config.SetAppSettings("CompanyId", obj.Code.GetValueOrDefault().ToString());
            return op;
        }

        /// <summary>
        /// 生成序列号
        /// </summary>
        /// <param name="company">OMS_CompanyAuthorize 单位授权实体类</param>
        /// <returns></returns>
        public string GenerateSN(OMS_CompanyAuthorize company)
        {
            if (company.Code > 0)
            {
                JObject json = new JObject();
                json["Code"] = company.Code;
                json["Title"] = company.Title;
                json["FullTitle"] = company.FullTitle;
                json["State"] = company.State;
                json["Category"] = company.Category;

                json["UserNum"] = company.UserNum;
                json["StoreNum"] = company.StoreNum;
                json["StoreProper"] = company.StoreProper;
                json["AppProper"] = company.AppProper;
                json["PosMinorDisp"] = company.PosMinorDisp;

                json["SupplierProper"] = company.SupplierProper;
                json["WholesalerProper"] = company.WholesalerProper;
                json["EffectiveDT"] = company.EffectiveDT;
                json["ExpirationDT"] = company.ExpirationDT;
                json["ValidityYear"] = company.ValidityYear;

                json["Useable"] = company.Useable;
                json["MachineSN"] = company.MachineSN;
                json["State"] = company.State;

                return DES.Encrypt(json.ToString());
            }
            else
            {
                return DES.Encrypt("-1");
            }
        }

        /// <summary>
        /// 验证激活
        /// </summary>
        /// <param name="sn">序列号</param>
        /// <returns>true:成功，false:失败</returns>
        public OpResult VerifyActivate(string sn)
        {
            var op = new OpResult();
            DictRegister[SysCommonRules.CompanyId] = null;
            string message="";
            if (HasRegister(ref message, sn))
            {
                var comp = CurrentUser.Company;
                if(comp==null)
                {
                    DictRegister[SysCommonRules.CompanyId] = false;
                }
                else if (comp.Category == 2)
                {
                    Config config = new Config();
                    config.SetAppSettings(_SerialKey, sn);
                }
            }
            op.Successed = DictRegister[SysCommonRules.CompanyId].GetValueOrDefault();
            op.Message = message;
            return op;
        }

        public const string _SerialKey = "SerialNo";
        public OMS_CompanyAuthorize AnalysisSN(string sn)
        {
            if (!string.IsNullOrEmpty(sn))
            {
                try
                {
                    var text = DES.Decrypt(sn);
                    JObject json = JObject.Parse(text);
                    OMS_CompanyAuthorize company = new OMS_CompanyAuthorize();

                    company.Code = Convert.ToInt32(json["Code"]);
                    company.Title = Convert.ToString(json["Title"]);
                    company.FullTitle = Convert.ToString(json["FullTitle"]);
                    company.State = Convert.ToInt16(json["State"]);
                    company.Category = Convert.ToInt16(json["Category"]);

                    company.UserNum = Convert.ToInt16(json["UserNum"]);
                    company.StoreNum = Convert.ToInt16(json["StoreNum"]);
                    company.StoreProper = Convert.ToString(json["StoreProper"]);
                    company.AppProper = Convert.ToString(json["AppProper"]);
                    company.PosMinorDisp = Convert.ToString(json["PosMinorDisp"]);

                    company.SupplierProper = Convert.ToString(json["SupplierProper"]);
                    company.WholesalerProper = Convert.ToString(json["WholesalerProper"]);
                    company.EffectiveDT = Convert.ToDateTime(json["EffectiveDT"]);
                    company.ValidityYear = Convert.ToInt16(json["ValidityYear"]);

                    company.Useable = Convert.ToString(json["Useable"]);
                    company.MachineSN = Convert.ToString(json["MachineSN"]);

                    return company;
                }
                catch { }
            }
            return null;
        }
        static Dictionary<int,bool?> DictRegister =new Dictionary<int,bool?>();
        public bool HasRegister(ref string message, string serialno = null)
        {
            if (!DictRegister.ContainsKey(SysCommonRules.CompanyId) || DictRegister[SysCommonRules.CompanyId]==null)
            {
                DictRegister[SysCommonRules.CompanyId] = false;
                CurrentUser.Company=null;
                string key =serialno?? Config.GetAppSettings(_SerialKey);
                var companyId = SysCommonRules.CompanyId;
                if (!string.IsNullOrWhiteSpace(key) && companyId > 0)//离线
                {
                    OMS_CompanyAuthorize auth = AnalysisSN(key);
                    if(auth==null)
                    {
                        message = "注册序列号不正确!";
                    }
                    else
                        return ValidateProperty(key, ref message);
                }
                else if (companyId > 0)
                {
                    return ValidateProperty(companyId.ToString(), ref message);
                }
            }
            return DictRegister[SysCommonRules.CompanyId].GetValueOrDefault();
        }
        bool ValidateProperty(string companyId,ref string message)
        {
            var omsurl = Config.GetAppSettings("omsurl") + "Authorization/GetCompany";
            var json = HttpClient.HttpGet(omsurl, "companyId=" + companyId);
            int comp=0;
            if (string.IsNullOrWhiteSpace(json) && int.TryParse(companyId,out comp))
            {
                message = "连接OMS管理平台失败!";
            }
            else
            {
                var company = json.ToObject<OMS_CompanyAuthorize>();
                if (company == null)
                {
                    message = "获取公司信息序列化失败!";
                }
                else if (company.Code == 0)
                {
                    message = "基础信息与注册不一致,请联系管理员!";
                }
                else if (!(company.Useable == "Y" && company.State == 1))
                {
                    message = "该公司处于不可用状态或未审核，请联系管理员!";
                }
                else if (company != null && company.ExpirationDT <= DateTime.Now)
                {
                    message = "已过使用期,请重新激活!";
                }
                else
                {
                    DictRegister[SysCommonRules.CompanyId] = true;
                    CurrentUser.Company = company;
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 验证匹配字段
        /// </summary>
        /// <param name="auth"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public bool ValidateCompany(OMS_CompanyAuthorize auth, OMS_CompanyAuthorize source)
        {
            if (auth == null || source == null) return false;
            return (auth.Category == source.Category && auth.Title == source.Title && auth.FullTitle == source.FullTitle && auth.MachineSN == source.MachineSN
                && auth.StoreNum==source.StoreNum && auth.Useable==source.Useable && auth.ValidityYear==source.ValidityYear);
        }
        /// <summary>
        /// 解析序列号
        /// </summary>
        public OMS_CompanyAuthorize GetSerialNO
        {
            get
            {
                string key = Config.GetAppSettings(_SerialKey);
                var companyId = SysCommonRules.CompanyId;
                OMS_CompanyAuthorize company = null;
                if (!string.IsNullOrWhiteSpace(key) && companyId>0)//离线
                {
                    company = AnalysisSN(key);
                }
                else if (companyId>0)
                {
                    company = OMSCompanyAuthrizeBLL.GetByCode(companyId);
                }
                return company;
            }
        }
    }
}