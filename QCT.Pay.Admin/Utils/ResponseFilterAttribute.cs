﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Pharos.Utility.Helpers;
using Pharos.Component.qrcode;
using System.Text;
using Pharos.Logic.OMS;

namespace QCT.Pay.Admin
{
    public class ResponseFilterAttribute : ActionFilterAttribute
    {
        [Ninject.Inject]
        LogEngine logEngine { get; set; }
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            //base.OnActionExecuted(actionExecutedContext);
            if (actionExecutedContext.Exception != null)
            {
                Log.Error(actionExecutedContext.Request.RequestUri.AbsolutePath, "异常信息：" + actionExecutedContext.Exception.Message);
                new LogEngine().WriteError(actionExecutedContext.Exception);
            }
            else
            {
                var content = actionExecutedContext.Response.Content;
                var res = content == null ? null : content.ReadAsAsync<object>().Result;//返回纯json
                var result = new ApiRetrunResult<object>()
                {
                    Code = "200",
                    Result = res
                };
                var response = actionExecutedContext.Request.CreateResponse<object>(System.Net.HttpStatusCode.OK, res, "application/json");
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                actionExecutedContext.Response = response;
            }
        }
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            string content = "";
            if (actionContext.Request.Method == HttpMethod.Get)
                content = actionContext.Request.RequestUri.Query;
            else
            {
                if (actionContext.Request.Content.IsMimeMultipartContent("form-data"))
                {
                    var provider = new MultipartMemoryStreamProvider();
                    System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        provider = actionContext.Request.Content.ReadAsMultipartAsync(provider).Result;
                    },
                    System.Threading.CancellationToken.None,
                    System.Threading.Tasks.TaskCreationOptions.LongRunning, // guarantees separate thread
                    System.Threading.Tasks.TaskScheduler.Default).Wait();
                    foreach (var pro in provider.Contents)
                    {
                        var title = pro.Headers.ContentDisposition.Name.Replace("\"", string.Empty);
                        if (pro.Headers.ContentType == null)
                            content += "," + title + "=" + pro.ReadAsStringAsync().Result;
                        else
                            content += ",附件名称:" + title;
                    }
                    content = content.TrimStart(',');
                }
                else
                    content = actionContext.Request.Content.ReadAsStringAsync().Result;//content-type=json

                //var nvl= actionContext.Request.Content.ReadAsFormDataAsync().Result;
                //var byts= actionContext.Request.Content.ReadAsByteArrayAsync().Result;
                //var st= actionContext.Request.Content.ReadAsStreamAsync().Result;
                if (content.IsNullOrEmpty() && actionContext.ActionArguments.Any())
                {
                    content = actionContext.ActionArguments.Values.ToJson();
                    object obj = null;
                    if (actionContext.ActionArguments.TryGetValue("requestParams", out obj))
                    {
                        var cid = obj.GetPropertyValue("CID");
                        if (!cid.IsNullOrEmpty())
                            System.Web.HttpContext.Current.Items["CID"] = cid;
                    }
                }
            }
            if (!content.IsNullOrEmpty())
            {
                var parms = System.Web.HttpUtility.UrlDecode(content, Encoding.UTF8);
                if (parms.Length > 5)
                {
                    var json = " 参数:" + parms;
                    Log.Debug(actionContext.Request.RequestUri.AbsolutePath, json);
                }
            }
            base.OnActionExecuting(actionContext);
        }
    }
    public class ApiRetrunResult<T>
       where T : class
    {
        public string Code { get; set; }

        public string Message { get; set; }
        public object ErrorInfo { get; set; }
        public T Result { get; set; }
    }
}