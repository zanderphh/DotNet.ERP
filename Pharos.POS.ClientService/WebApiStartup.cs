﻿using Microsoft.Owin.Hosting;
using Owin;
using Pharos.Service.Retailing.Marketing;
using System;
using System.Web.Http;

namespace Pharos.Api.Retailing
{
    class WebApiStartup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            //StoreConfig.Register();
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
            WebApiConfig.Register(config);
            config.Filters.Add(new ApiExceptionFilterAttribute());
            config.Filters.Add(new ResponseFilterAttribute());
            appBuilder.UseWebApi(config);
        }

        public static IDisposable RunWebServer(string address = null)
        {
            if (string.IsNullOrEmpty(address))
            {
                address = string.Format("http://*:{0}", System.Configuration.ConfigurationManager.AppSettings["ApiPort"]);
            }
            return WebApp.Start<WebApiStartup>(url: address);
        }
    }
}