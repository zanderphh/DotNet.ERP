﻿using AX.CSF.Encrypt;
using Newtonsoft.Json.Linq;
using Pharos.Store.Retailing.Models;
using Pharos.Sys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Pharos.Utility;
using Pharos.Utility.Helpers;
using System.Web;
using System.Web.Routing;
using System.Web.Mvc;
using Pharos.Logic.BLL;
namespace Pharos.Store.Retailing
{
    public class Authorize
    {
        /// <summary>
        /// 从序列号返回对象
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        public static CompanyAuthorize AnalysisSN(string sn)
        {
            if (!string.IsNullOrEmpty(sn))
            {
                try
                {
                    var text = DES.Decrypt(sn);
                    JObject json = JObject.Parse(text);
                    CompanyAuthorize company = new CompanyAuthorize();

                    company.CID = Convert.ToInt32(json["CID"]);
                    company.Title = Convert.ToString(json["Title"]);
                    company.Source = Convert.ToInt16(json["Source"]);
                    company.Way = Convert.ToInt16(json["Way"]);
                    company.BusinessMode = Convert.ToInt16(json["BusinessMode"]);

                    company.UserNum = Convert.ToInt16(json["UserNum"]);
                    company.StoreNum = Convert.ToInt16(json["StoreNum"]);
                    company.StoreProper = Convert.ToString(json["StoreProper"]);
                    company.AppProper = Convert.ToString(json["AppProper"]);
                    company.PosMinorDisp = Convert.ToString(json["PosMinorDisp"]);

                    company.OpenVersionId = Convert.ToInt16(json["OpenVersionId"]);
                    company.OpenScopeId = Convert.ToString(json["OpenScopeId"]);
                    company.EffectiveDT = Convert.ToString(json["EffectiveDT"]);
                    company.ExpirationDT = Convert.ToString(json["ExpirationDT"]);
                    company.ValidityNum = Convert.ToInt16(json["ValidityNum"]);

                    company.SupperAccount = Convert.ToString(json["SupperAccount"]);
                    company.SupperPassword = Convert.ToString(json["SupperPassword"]);
                    company.MachineSN = Convert.ToString(json["MachineSN"]);
                    company.Status = Convert.ToInt16(json["Status"]);
                    return company;
                }
                catch { }
            }
            return null;
        }
        public static OpResult Activate(string serialno)
        {
            var op = new OpResult();
            DictRegister[SysCommonRules.CompanyId] = null;
            op = HasRegister(serialno.IsNullOrEmpty() ? null : serialno);
            if (op.Successed)
            {
                var company = CurrentStoreUser.Company as CompanyAuthorize;
                #region 独立方式写入配置文件
                if (company.Way == 2)
                {
                    var key = Pharos.Utility.Config.GetAppSettings(_SerialKey);
                    if (!serialno.IsNullOrEmpty() && serialno != key)
                    {
                        if (!new Pharos.Utility.Config().SetAppSettings(_SerialKey, serialno))
                        {
                            op.Message = "注册系列号写入失败，请确认是否有写入权限!";
                            op.Successed = false;
                        }
                    }
                }
                #endregion
                if (op.Successed && Pharos.Logic.BLL.UserInfoService.GetUserCount(SysCommonRules.CompanyId) <= 0)
                {
                    var batchlist = new List<Pharos.Logic.Entity.BatchTranEntity>();
                    #region 类目
                    if (!company.OpenScopeId.IsNullOrEmpty())
                    {
                        var firsts = company.OpenScopeId.Split(',');
                        var sources = Pharos.Logic.BLL.ProductCategoryService.FindList(o => o.CompanyId == SysCommonRules.CompanyId && firsts.Contains(o.Title));
                        var maxode = Pharos.Logic.BLL.ProductCategoryService.MaxCode(0);
                        var maxsn = Pharos.Logic.BLL.ProductCategoryService.MaxSn;
                        var productCategorylist = new List<Pharos.Logic.Entity.ProductCategory>();
                        foreach (var first in firsts)
                        {
                            if (sources.Any(o => o.Title == first)) continue;
                            productCategorylist.Add(new Logic.Entity.ProductCategory()
                            {
                                CategoryPSN = 0,
                                State = 1,
                                CategoryCode = (++maxode),
                                CompanyId = SysCommonRules.CompanyId,
                                CategorySN = (++maxsn),
                                Title = first,
                                Grade = 1
                            });
                        }
                        batchlist.Add(new Logic.Entity.BatchTranEntity(Logic.CommandEnum.Insert, productCategorylist, typeof(Logic.Entity.ProductCategory)));
                    }
                    #endregion
                    #region 超管
                    if (company.SupperAccount.IsNullOrEmpty()) company.SupperAccount = "superadmin";
                    if (company.SupperPassword.IsNullOrEmpty()) company.SupperAccount = "888888";
                    var user = new Sys.Entity.SysUserInfo()
                    {
                        CompanyId = SysCommonRules.CompanyId,
                        UID = SysCommonRules.GUID,
                        UserCode = SysCommonRules.UserCode,
                        FullName = company.SupperAccount,
                        LoginName = company.SupperAccount,
                        LoginPwd = Pharos.Utility.Security.MD5_Encrypt(company.SupperPassword),
                        Sex = true,
                        Status = 1,
                        LoginDT = DateTime.Now,
                        LoginNum = 0,
                        RoleIds = "9",
                        BranchId = company.CID.GetValueOrDefault(),
                        CreateDT = DateTime.Now
                    };
                    batchlist.Add(new Logic.Entity.BatchTranEntity(Logic.CommandEnum.Insert, new List<Sys.Entity.SysUserInfo>() { user }, typeof(Sys.Entity.SysUserInfo)));
                    #endregion
                    #region 组织机构
                    var depart = new Sys.Entity.SysDepartments()
                    {
                        CompanyId = SysCommonRules.CompanyId,
                        Type = 1,
                        DepId = company.CID.GetValueOrDefault(),
                        PDepId = 0,
                        Title = company.Title,
                        Status = true
                    };
                    batchlist.Add(new Logic.Entity.BatchTranEntity(Logic.CommandEnum.Insert, new List<Sys.Entity.SysDepartments>() { depart }, typeof(Sys.Entity.SysDepartments)));
                    #endregion
                    #region 初始化sql文件
                    try
                    {
                        var filePath = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\init.sql";
                        string sql = "";
                        using (System.IO.StreamReader sr = new System.IO.StreamReader(filePath, System.Text.Encoding.GetEncoding("gb2312")))
                        {
                            sql = sr.ReadToEnd();
                            sr.Close();
                            if (!sql.IsNullOrEmpty())
                                sql = sql.Replace("@companyId", SysCommonRules.CompanyId.ToString());
                        }
                        batchlist.Add(new Logic.Entity.BatchTranEntity(Logic.CommandEnum.Sql, new List<string>() { sql }));
                    }
                    catch (Exception ex)
                    {
                        op.Successed = false;
                        op.Message = ex.Message;
                    }
                    #endregion
                    op = Pharos.Logic.BLL.CommonService.ExecuteTranList(batchlist.ToArray());
                }

            }
            return op;
        }
        public const string _SerialKey = "SerialNo";
        static Dictionary<int, OpResult> DictRegister = new Dictionary<int, OpResult>();
        public static OpResult HasRegister(string serialno = null)
        {
            var result = true;
            if (DictRegister.ContainsKey(SysCommonRules.CompanyId) && DictRegister[SysCommonRules.CompanyId] != null)
                result = DictRegister[SysCommonRules.CompanyId].Successed;
            if (!result || !DictRegister.ContainsKey(SysCommonRules.CompanyId) || DictRegister[SysCommonRules.CompanyId] == null)
            {
                var op = new OpResult();
                DictRegister[SysCommonRules.CompanyId] = op;
                CurrentStoreUser.Company = null;
                string key = serialno ?? Pharos.Utility.Config.GetAppSettings(_SerialKey);
                var companyId = SysCommonRules.CompanyId;
                if (!string.IsNullOrWhiteSpace(key) && companyId > 0)//离线
                {
                    CompanyAuthorize auth = AnalysisSN(key);
                    if (auth == null)
                    {
                        op.Message = "注册序列号不正确!";
                        return op;
                    }
                    if (auth.CID != companyId)
                    {
                        op.Message = "商户号输入不正确!";
                        return op;
                    }
                    return ValidateProperty(auth);
                }
                else if (companyId > 0)
                {
                    return ValidateProperty(new CompanyAuthorize() { CID = companyId, Way = 1 });
                }
            }
            return DictRegister[SysCommonRules.CompanyId];
        }
        static OpResult ValidateProperty(CompanyAuthorize source)
        {
            var companyId = source.ToJson();
            if (source.Way == 1)
            {
                var omsurl = OmsUrl + "api/outerapi/GetCompany";
                companyId = HttpClient.HttpPost(omsurl, companyId);
                if (companyId == "404")//在线且返回空时
                {
                    RemoveCurrentAuth();
                    return OpResult.Fail("连接OMS管理平台失败，请检查网络是否正常!");
                }
            }
            var company = companyId.ToObject<CompanyAuthorize>();
            if (company == null)
            {
                return OpResult.Fail("获取商户信息失败，请确认是否已注册商户!");
            }
            if (company.Status != 1)
            {
                return OpResult.Fail("您处于停用或未审核状态，请联系管理员!");
            }
            if (DateTime.Parse(company.EffectiveDT) > DateTime.Now)
            {
                return OpResult.Fail("未到生效日期暂不能使用!");
            }
            if (DateTime.Parse(company.ExpirationDT) <= DateTime.Now)
            {
                return OpResult.Fail("已过使用期,请联系管理员!");
            }
            if (company.Way == 2 && company.CID == 0)
            {
                return OpResult.Fail("基础信息与注册不一致,请联系管理员!");
            }
            DictRegister[SysCommonRules.CompanyId] = OpResult.Success();
            CurrentStoreUser.Company = company;
            return DictRegister[SysCommonRules.CompanyId];

        }
        public static OpResult RegisterAgain(CompanyAuthorize comp)
        {
            comp.SerialNo = "";
            comp.MachineSN = Machine.GetMAC;
            comp.CID = SysCommonRules.CompanyId;
            if (comp.AppProper != "Y") comp.AppProper = "N";
            if (comp.StoreProper != "Y") comp.StoreProper = "N";
            if (comp.PosMinorDisp != "Y") comp.PosMinorDisp = "N";
            var rt = HttpClient.HttpPost(OmsUrl + "api/outerapi/RegisterAgain", comp.ToJson());
            if (rt == "0") return OpResult.Fail("连接OMS管理平台失败，请检查网络是否正常！");
            new Pharos.Utility.Config().SetAppSettings(_SerialKey, "");
            RemoveCurrentAuth();
            return OpResult.Success();
        }
        public static OpResult LoinValidator(int companyId)
        {
            if (companyId <= 0) return OpResult.Fail("商户号为空！");
            var op = new OpResult();
            var omsurl = OmsUrl + "api/outerapi/GetCompany";
            if (Pharos.Logic.BLL.UserInfoService.GetUserCount(companyId) <= 0)//首次
            {
                var auth = new CompanyAuthorize() { CID = companyId };
                var json = HttpClient.HttpPost(omsurl, auth.ToJson());
                if (json == "404")
                {
                    return OpResult.Fail("连接OMS管理平台失败，请检查网络是否正常！");
                }
                var obj = json.ToObject<CompanyAuthorize>();
                if (obj == null)
                {
                    string msg = string.Format("<script>if(confirm('商户未注册是否填写注册信息?')) window.location.href='{0}';</script>", Url.GetApplicationPath + "Authorization/outregister?cid=" + companyId);
                    //HttpContext.Current.Response.Write(msg);
                    op.Successed = true;
                }
                else if (obj.Way == 2)
                {
                    string msg = string.Format("<script>if(confirm('商户未激活是否激活?')) window.location.href='{0}';</script>", Url.GetApplicationPath + "Authorization/activate?again=1&cid=" + companyId);
                    HttpContext.Current.Response.Write(msg);
                }
                else
                    return Activate(null);
            }
            else//再次
            {
                var company = CurrentStoreUser.Company as CompanyAuthorize;
                if (company == null) RemoveCurrentAuth();
                op = HasRegister();
                #region 独立方式附加控制
                if (op.Successed)
                {
                    company = CurrentStoreUser.Company as CompanyAuthorize;
                    if (company != null && company.Way == 2)
                    {
                        var key = Pharos.Utility.Config.GetAppSettings(_SerialKey);
                        if (!key.IsNullOrEmpty())
                        {
                            var apppath = Url.GetApplicationPath;
                            System.Threading.Tasks.Task.Factory.StartNew(() =>
                            {
                                omsurl = OmsUrl + "api/outerapi/RegisterAgain";
                                var sno = "";
                                if (company.MachineSN.IsNullOrEmpty())
                                {
                                    company.MachineSN = Machine.GetMAC;
                                    sno = HttpClient.HttpPost(omsurl, company.ToJson());//更新机器码
                                }
                                omsurl = OmsUrl + "api/outerapi/GetSerialNo";
                                sno = HttpClient.HttpPost(omsurl, companyId.ToString());
                                if (!sno.IsNullOrEmpty()) sno = sno.Replace("\"", "");
                                if (AnalysisSN(sno) != null && sno != key)
                                {
                                    new Pharos.Utility.Config(apppath).SetAppSettings(_SerialKey, sno);//更新配置
                                    DictRegister[companyId] = null;
                                }
                            });
                        }
                        else
                        {
                            string msg = string.Format("<script>if(confirm('商户未激活是否激活?')) window.location.href='{0}';</script>", Url.GetApplicationPath + "Authorization/activate?again=1&cid=" + companyId);
                            HttpContext.Current.Response.Write(msg);
                            RemoveCurrentAuth();
                            op.Message = "独立方式，注册系列号不能为空，请联系管理员!";
                            op.Successed = false;
                        }
                        if (!company.MachineSN.IsNullOrEmpty() && company.MachineSN != Machine.GetMAC)
                        {
                            op.Message = "请在注册时的服务器上使用!";
                            op.Successed = false;
                        }
                    }
                }
                #endregion
            }
            return op;
        }
        public static string OmsUrl
        {
            get { return Pharos.Utility.Config.GetAppSettings("omsurl"); }
        }
        public static void RemoveCurrentAuth()
        {
            DictRegister[SysCommonRules.CompanyId] = null;
            CurrentStoreUser.Company = null;
        }
        /// <summary>
        /// 获取CID
        /// </summary>
        /// <returns>-2是请求API发生错误，-1是输入的二级域名是空，0是输入的域名不存在商户，大于0是输入的域名存在商户,-3是输入的域名是保留二级域名</returns>
        public static int getCID(string dom)
        {
            //二级域名
            if (!dom.IsNullOrEmpty())
            {
                var omsurl = OmsUrl + "api/OuterApi/GetCIDByRealm";
                string v = HttpClient.HttpGet(omsurl, "name=" + dom);
                if (v == "error")
                {
                    //请求API发生错误
                    return -2;
                }
                else if (v == "-1")
                {
                    //输入的域名是保留二级域名
                    return -3;
                }
                else if (v == "0" || v.IsNullOrEmpty())
                {
                    //输入的域名不存在商户
                    return 0;
                }
                else
                {
                    //输入的域名存在商户
                    return Convert.ToInt32(v);
                }
            }
            //输入的二级域名是空
            else
            {
                return -1;
            }
        }


        /// <summary>
        /// 获取商户号和门店ID（域名）
        /// </summary>
        /// <param name="cidSid"></param>
        /// <returns></returns>
        public csID getCidSid(string s_cid, string s_sid)
        {
            //测试
            bool aa = IsNumeric("0");

            bool bb = IsNumeric("1");
            csID csid = new csID();

            string cid = s_cid;
            string sid = s_sid;

            //格式错误
            if (string.IsNullOrEmpty(cid) || string.IsNullOrEmpty(sid))
            {
                csid.message = "格式错误";
                return csid;
            }
            else
            {
                //cid或者sid没有大于0
                if (!IsNumeric(cid) || !IsNumeric(sid))
                {
                    csid.message = "域名的store后面必须是数字";
                    return csid;
                }
                else
                {
                    csid.message = "success";
                    csid.cid = cid;
                    csid.sid = sid;
                    return csid;
                }
            }
        }

        /// <summary>
        /// localhost门店登录、ip门店登录
        /// </summary>
        public csID ipLocalhostStore(string localhost, string ip1, string ip2, string ip3, string ip4, string cid, string sid)
        {
            csID csid = new csID();
            csid.cid = cid;
            csid.sid = sid;

            //localhost门店登录
            if (!localhost.IsNullOrEmpty())
            {
                if (localhost.ToLower().Contains("localhost"))
                {
                    csid.message = "localhost";
                }
                else
                {
                    csid.message = "发生错误";
                }
            }
            //ip门店登录
            else if (!ip1.IsNullOrEmpty() && !ip2.IsNullOrEmpty() && !ip3.IsNullOrEmpty() && !ip4.IsNullOrEmpty())
            {
                csid.message = "ip";
            }
            //禁止访问（比如：erp.qcterp.com/store3-1）
            else if (!ip1.IsNullOrEmpty() && !ip2.IsNullOrEmpty() && !ip3.IsNullOrEmpty() && ip4.IsNullOrEmpty())
            {
                csid.message = "禁止访问";
            }
            else
            {
                csid.message = "发生错误";
            }

            if (csid.message == "ip" || csid.message == "localhost")
            {
                if (csid.cid.IsNullOrEmpty())
                {
                    csid.cid = System.Configuration.ConfigurationManager.AppSettings["CompanyId"];
                }
                if (csid.sid.IsNullOrEmpty())
                {
                    csid.sid = System.Configuration.ConfigurationManager.AppSettings["StoreId"];
                }
            }
            return csid;
        }

        ///验证输入的数据是不是数字
        ///传入字符串
        ///返回true或者false
        public bool IsNumeric(string str)
        {
            System.Text.RegularExpressions.Regex reg1 = new System.Text.RegularExpressions.Regex(@"^[0-9]\d*$");
            return reg1.IsMatch(str);
        }

        /// <summary>
        /// 是否为ip
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public bool IsIP(string ip)
        {
            //判断
            return System.Text.RegularExpressions.Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }

    }

}