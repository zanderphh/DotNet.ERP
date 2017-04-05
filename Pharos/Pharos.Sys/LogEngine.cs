﻿// --------------------------------------------------
// Copyright (C) 2015 版权所有
// 创 建 人：日志引擎
// 创建时间：2015-05-27
// 描述信息：公共字典
// --------------------------------------------------

using System;
using System.Web;
using System.Linq;
using System.Data.SqlClient;
using AX.CSF.Log;
using Pharos.DBFramework;
using Pharos.Sys.Extensions;
using Pharos.Sys.BLL;
using Pharos.Sys.DAL;

namespace Pharos.Sys
{
    /// <summary>
    /// 日志引擎类
    /// </summary>
    /// <example>
    /// <![CDATA[
    /// 使用示例：
    /// Sys.LogEngine log = new Sys.LogEngine();
    /// log.WriteInsert("成功：新增XXX信息");
    /// ]]>
    /// </example>
    public class LogEngine
    {
        string Uid = null;
        public LogEngine() { }
        public LogEngine(string uid) {
            Uid = uid;
        }
        #region 写入日志

        #region 业务操作类型

        /// <summary>
        /// 写入普通信息日志（todo：该方法为临时使用）
        /// </summary>
        /// <param name="msg">自定义信息</param>
        public void WriteInfo(string msg)
        {
            this.Analysis(msg, null, LogType.调试);
        }

        /// <summary>
        /// 写入新增日志
        /// </summary>
        /// <param name="msg">自定义信息</param>
        /// <param name="module">LogModule 日志模块</param>
        public void WriteInsert(string msg, LogModule module)
        {
            this.Analysis(msg, null, LogType.新增, module);
        }

        /// <summary>
        /// 写入修改日志
        /// </summary>
        /// <param name="msg">自定义信息</param>
        /// <param name="module">LogModule 日志模块</param>
        public void WriteUpdate(string msg, LogModule module)
        {
            this.Analysis(msg, null, LogType.修改, module);
        }

        /// <summary>
        /// 写入删除日志
        /// </summary>
        /// <param name="msg">自定义信息</param>
        /// <param name="module">LogModule 日志模块</param>
        public void WriteDelete(string msg, LogModule module)
        {
            this.Analysis(msg, null, LogType.删除, module);
        }

        #endregion

        #region 系统操作类型

        /// <summary>
        /// 写入登录日志
        /// </summary>
        /// <param name="msg">自定义信息</param>
        public void WriteLogin(string msg)
        {
            this.Analysis(msg, null, LogType.登录);
        }
        /// <summary>
        /// 写入登录日志
        /// </summary>
        /// <param name="msg">自定义信息</param>
        /// <param name="module">LogModule 日志模块</param>
        public void WriteLogin(string msg, LogModule module)
        {
            this.Analysis(msg, null, LogType.登录, module);
        }

        /// <summary>
        /// 写入退出日志
        /// </summary>
        /// <param name="msg">自定义信息</param>
        public void WriteLogout(string msg)
        {
            this.Analysis(msg, null, LogType.退出);
        }
        /// <summary>
        /// 写入退出日志
        /// </summary>
        /// <param name="msg">自定义信息</param>
        /// <param name="module">LogModule 日志模块</param>
        public void WriteLogout(string msg, LogModule module)
        {
            this.Analysis(msg, null, LogType.退出, module);
        }

        /// <summary>
        /// 写入异常日志
        /// </summary>
        /// <param name="ex">Exception 异常信息</param>
        public void WriteError(Exception ex)
        {
            this.Analysis(string.Empty, ex, LogType.异常);
        }

        /// <summary>
        /// 写入异常日志
        /// </summary>
        /// <param name="ex">Exception 异常信息</param>
        /// <param name="module">LogModule 日志模块</param>
        public void WriteError(Exception ex, LogModule module)
        {
            this.Analysis(string.Empty, ex, LogType.异常, module);
        }

        /// <summary>
        /// 写入自定义信息的异常日志
        /// </summary>
        /// <param name="msg">自定义信息（优先级）</param>
        /// <param name="ex">Exception 异常信息</param>
        public void WriteError(string msg, Exception ex)
        {
            this.Analysis(msg, ex, LogType.异常);
        }

        /// <summary>
        /// 写入自定义信息的异常日志
        /// </summary>
        /// <param name="msg">自定义信息（优先级）</param>
        /// <param name="ex">Exception 异常信息</param>
        /// <param name="module">LogModule 日志模块</param>
        public void WriteError(string msg, Exception ex, LogModule module)
        {
            this.Analysis(msg, ex, LogType.异常, module);
        }

        #endregion

        #region 解析

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="msg">自定义信息（优先级）</param>
        /// <param name="ex">Exception 异常信息</param>
        /// <param name="type">LogType 日志类型</param>
        private void Analysis(string msg, Exception ex, LogType type)
        {
            this.Analysis(msg, ex, type, LogModule.其他);
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="msg">自定义信息（优先级）</param>
        /// <param name="ex">Exception 异常信息</param>
        /// <param name="type">LogType 日志类型</param>
        /// <param name="module">LogModule 日志模块</param>
        private void Analysis(string msg, Exception ex, LogType type, LogModule module)
        {
            try
            {
                string UId =Uid?? CurrentUser.UID;
                //将捕获的日志写入到数据库中
                LogHelper db;

                if (string.IsNullOrEmpty(msg))
                {
                    db = new LogHelper(ex, (LogHelper.LogType)type);
                }
                else
                {
                    db = new LogHelper(msg, (LogHelper.LogType)type);
                }

                if (ex != null)
                {
                    var e = ToInnerException(ex);
                    msg += (ex.Message == e.Message ? e.Message : ex.Message + "\r\n描述:" + e.Message) + "\r\n源:" + e.Source + "\r\n引发原因:" + e.TargetSite + "\r\n堆栈跟踪:" + e.StackTrace;
                }

                SqlParameter[] parms = {
                    new SqlParameter("@CompanyId", SysCommonRules.CompanyId),
                    new SqlParameter("@Type", (byte)type),
                    new SqlParameter("@UId", UId),
                    new SqlParameter("@Summary",msg),
                    new SqlParameter("@ClientIP",(db.ClientIP=="::1")?"127.0.0.1":db.ClientIP),
                    new SqlParameter("@ServerName",db.ServerNetBIOS + @"/" + db.ServerUser),
                    new SqlParameter("@ModuleName",Enum.GetName(typeof(LogModule), module)),
                };

                string sql = "INSERT INTO SysLog([CompanyId],[Type],[UId],[Summary],[ClientIP],[ServerName],[ModuleName]) ";
                sql += "VALUES (@CompanyId,@Type, @UId, @Summary, @ClientIP, @ServerName, @ModuleName); ";

                int resultDefault = new DBHelper().ExecuteNonQueryText(sql, parms);
            }
            catch (Exception sex)
            {
                LogHelper txt = new LogHelper(sex, (LogHelper.LogType)type);

                try
                {
                    //当数据库服务器无法连接时，则写入txt中
                    txt.SetLogPath("SysLog");
                    txt.SaveToTxt();
                }
                catch
                {
                    //若连txt都写入失败，则直接将异常输出到页面
                    //throw new Exception(e.Message);
                    GoErrorPage("[" + txt.ServerNetBIOS + "] 写入日志失败，请检查日志路径是否正常！");
                }
            }
        }
        /// <summary>
        /// 返回最内部的错误
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static Exception ToInnerException(Exception ex)
        {
            if (ex.InnerException == null) return ex;
            return ToInnerException(ex.InnerException);
        }

        #endregion

        #endregion

        #region 跳转到异常提示页面

        /// <summary>
        /// 跳转到异常提示页面
        /// </summary>
        /// <param name="message">要提示的信息</param>
        public void GoErrorPage(string message)
        {
            //todo: 该页跳转方式需转为 mapping 方式
            HttpContext.Current.Response.Redirect(SysConstPool.Page_GoError + HttpUtility.UrlEncode(message));
        }

        #endregion

        public static string CompareModelToLog<T>(LogModule action, T newObj, T oldObj = null)
            where T : class
        {
            var result = string.Empty;
            if (oldObj == null)
            {
                result = string.Format("成功添加{0}！", action.ToString());
                var props = typeof(T).GetProperties();
                var idprop = props.FirstOrDefault(o => o.Name.ToUpper() == "ID");
                var id = idprop.GetValue(newObj, null);
                result += string.Format("{0}Id:{1}{2}", Environment.NewLine, id, Environment.NewLine);
                foreach (var item in props)
                {
                    var attr = item.GetCustomAttributes(typeof(OperationLogAttribute), true).FirstOrDefault();
                    if (attr != null)
                    {
                        var attrObj = (attr as OperationLogAttribute);
                        object value = item.GetValue(newObj, null);
                        if (attrObj.IsDict)
                        {
                            switch (attrObj.Source)
                            {
                                case DictSource.InDBSysDict:
                                    SysDataDictionaryDAL _dal = new SysDataDictionaryDAL();
                                    _dal.GetExtModelByDicSN((int)value);
                                    break;
                                case DictSource.Prop:
                                    var titleProp = props.FirstOrDefault(o => o.Name == attrObj.Prop);
                                    if (titleProp != null)
                                    {
                                        value = titleProp.GetValue(newObj, null);
                                    }
                                    break;
                                case DictSource.Enum:
                                    var enumtitle = attrObj.ShowDicts.FirstOrDefault(o => o.Contains(value.ToString().ToLower() + ":"));
                                    if (enumtitle != null)
                                        value = enumtitle.Split(':')[1];
                                    break;
                            }
                        }
                        result += string.Format("{0}:{1},", attrObj.Title, value);
                    }
                }
                //new 
            }
            else if (newObj != null && oldObj != null)
            {
                result = string.Format("成功修改{0}!", action.ToString());
                var props = typeof(T).GetProperties();
                var idprop = props.FirstOrDefault(o => o.Name.ToUpper() == "ID");
                var id = idprop.GetValue(newObj, null);
                result += string.Format("{0}Id:{1}{2}", Environment.NewLine, id, Environment.NewLine);
                foreach (var item in props)
                {
                    var attr = item.GetCustomAttributes(typeof(OperationLogAttribute), true).FirstOrDefault();
                    if (attr != null)
                    {
                        var attrObj = (attr as OperationLogAttribute);
                        object value = item.GetValue(newObj, null);
                        object oldValue = item.GetValue(oldObj, null);
                        bool isEquals = false;
                        if (value == null)
                        {
                            isEquals = value != oldValue;
                        }
                        else
                        {
                            isEquals = !value.Equals(oldValue);
                        }

                        if (isEquals)
                        {
                            if (attrObj.IsDict)
                            {
                                switch (attrObj.Source)
                                {
                                    case DictSource.InDBSysDict:
                                        SysDataDictionaryDAL _dal = new SysDataDictionaryDAL();
                                        _dal.GetExtModelByDicSN((int)value);
                                        break;
                                    case DictSource.Prop:
                                        var titleProp = props.FirstOrDefault(o => o.Name == attrObj.Prop);
                                        if (titleProp != null)
                                        {
                                            value = titleProp.GetValue(newObj, null);
                                        }
                                        break;
                                    case DictSource.Enum:
                                        var enumtitle = attrObj.ShowDicts.FirstOrDefault(o => o.Contains(value.ToString().ToLower() + ":"));
                                        if (enumtitle != null)
                                            value = enumtitle.Split(':')[1];
                                        break;
                                }
                            }
                            result += string.Format("{0}:{1},", attrObj.Title, value);
                        }
                    }
                }
                //update

            }
            else
            {
                result = string.Format("成功删除{0}！", action.ToString());
                var props = typeof(T).GetProperties();
                var idprop = props.FirstOrDefault(o => o.Name.ToUpper() == "ID");
                var id = idprop.GetValue(oldObj, null);
                result += string.Format("{0}Id:{1}{2}", Environment.NewLine, id, Environment.NewLine);
                foreach (var item in props)
                {
                    var attr = item.GetCustomAttributes(typeof(OperationLogAttribute), true).FirstOrDefault();
                    if (attr != null)
                    {
                        var attrObj = (attr as OperationLogAttribute);
                        object value = item.GetValue(oldObj, null);
                        if (attrObj.IsDict)
                        {
                            switch (attrObj.Source)
                            {
                                case DictSource.InDBSysDict:
                                    SysDataDictionaryDAL _dal = new SysDataDictionaryDAL();
                                    _dal.GetExtModelByDicSN((int)value);
                                    break;
                                case DictSource.Prop:
                                    var titleProp = props.FirstOrDefault(o => o.Name == attrObj.Prop);
                                    if (titleProp != null)
                                    {
                                        value = titleProp.GetValue(oldObj, null);
                                    }
                                    break;
                                case DictSource.Enum:
                                    var enumtitle = attrObj.ShowDicts.FirstOrDefault(o => o.Contains(value.ToString().ToLower() + ":"));
                                    if (enumtitle != null)
                                        value = enumtitle.Split(':')[1];
                                    break;
                            }
                        }
                        result += string.Format("{0}:{1},", attrObj.Title, value);
                    }
                }
            }
            return result;
        }

    }

    /// <summary>
    /// 日志类型
    /// </summary>
    public enum LogType : byte
    {
        /// <summary>
        /// 登录
        /// </summary>
        登录 = 1,
        /// <summary>
        /// 退出
        /// </summary>
        退出 = 2,
        /// <summary>
        /// 异常
        /// </summary>
        异常 = 3,
        /// <summary>
        /// 新增
        /// </summary>
        新增 = 4,
        /// <summary>
        /// 修改
        /// </summary>
        修改 = 5,
        /// <summary>
        /// 删除
        /// </summary>
        删除 = 6,
        /// <summary>
        /// 调试
        /// </summary>
        调试 = 7,
        /// <summary>
        /// 演练
        /// </summary>
        演练 = 8,
        /// <summary>
        /// 其他
        /// </summary>
        其他 = 10
    }

    /// <summary>
    /// 日志模块
    /// </summary>
    public enum LogModule
    {
        通知公告 = 1,
        邮件管理 = 2,
        采购管理 = 3,
        返利管理 = 4,
        促销管理 = 5,
        库存管理 = 6,
        统计报表 = 7,
        系统管理 = 8,
        档案管理 = 156,
        会员管理 = 183,
        APP手机端 = 184,

        数据字典 = 185,
        品牌管理 = 186,
        批发商 = 187,
        种类管理 = 188,
        产品变价 = 189,
        合同管理 = 190,
        采购订单 = 191,
        消费积分 = 192,
        会员分组 = 193,
        会员充值 = 194,

        其他 = 0,
    }
}