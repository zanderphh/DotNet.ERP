﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Pharos.Wpf.HotKeyHelper
{
    public class HotKeyRule
    {
        public HotKeyRule()
        {
            IsShowInHelp = true;
            IsShowInMainWindow = false;
            EnableSet = false;
        }

        public bool EnableSet { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Keys { get;set; }
        public bool IsShowInHelp { get; set; }

        public bool IsShowInMainWindow { get; set; }

        /// <summary>
        /// 应用窗体类型
        /// </summary>
        public List<Type> Effectivity { get; set; }


        private IHotKeyCommand tmpCommand;
        public IHotKeyCommand Command
        {
            get
            {
                if (tmpCommand == null)
                {
                    string typeName = "."+Name + "Command";
                    var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                    foreach (var item in assemblies)
                    {
                        var type = item.GetTypes().FirstOrDefault(o => o.ToString().EndsWith(typeName));
                        if (type != null && type.GetInterfaces().Contains(typeof(IHotKeyCommand)))
                        {
                            tmpCommand = item.CreateInstance(type.ToString()) as IHotKeyCommand;
                            return tmpCommand;
                        }
                    }
                }
                return tmpCommand;
            }
        }
    }
}