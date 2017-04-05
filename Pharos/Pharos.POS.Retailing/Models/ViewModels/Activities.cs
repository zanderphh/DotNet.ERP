﻿using Newtonsoft.Json;
using Pharos.POS.Retailing.Models.ApiParams;
using Pharos.POS.Retailing.Models.ApiReturnResults;
using Pharos.Wpf.ViewModelHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Pharos.POS.Retailing.Models.ViewModels
{
    public class ActivityViewModel : BaseViewModel
    {
        public ActivityViewModel()
        {
            Task.Factory.StartNew(() =>
            {
                Task.Factory.StartNew(() =>
                {
                    var _machinesInfo = Global.MachineSettings.MachineInformations;
                    BaseApiParams _params = new BaseApiParams()
                    {
                        StoreId = _machinesInfo.StoreId,
                        MachineSn = _machinesInfo.MachineSn,
                        CID = _machinesInfo.CompanyId
                    };
                    var result = ApiManager.Post<BaseApiParams, ApiRetrunResult<IEnumerable<Activity>>>(@"api/Activities", _params);
                    CurrentWindow.Dispatcher.Invoke(new Action(() =>
                    {
                        if (result.Code == "200")
                        {
                            var list = result.Result.ToList();
                            list.Insert(0, new Activity() { CurrentWindow = CurrentWindow, Id = 0, No = 0, Theme = "不参与活动" });
                            Activities = list;
                            var count = Activities.Count();
                            for (var i = 1; i < count; i++)
                            {
                                var item = Activities.ElementAt(i);
                                item.CurrentWindow = CurrentWindow;
                                item.No = i;
                            }
                            this.OnPropertyChanged(o => o.Activities);
                        }
                        else
                        {
                            Toast.ShowMessage(result.Message, CurrentWindow);
                        }
                    }));
                });
            });
        }

        public IEnumerable<Activity> Activities { get; set; }
    }
    public class Activity : BaseViewModel
    {
        public int No { get; set; }
        public string Theme { get; set; }

        public int Id { get; set; }

        [JsonIgnore]
        public ICommand SetCommand
        {
            get
            {
                return new GeneralCommand<object>((o1, o2) =>
                {
                    Task.Factory.StartNew(() =>
                    {
                        var _machinesInfo = Global.MachineSettings.MachineInformations;
                        ActivityParams _params = new ActivityParams()
                        {
                            StoreId = _machinesInfo.StoreId,
                            MachineSn = _machinesInfo.MachineSn,
                            CID = _machinesInfo.CompanyId,
                            ActivityId = Id
                        };
                        var result = ApiManager.Post<ActivityParams, ApiRetrunResult<IEnumerable<Activity>>>(@"api/SetActivityId", _params);
                        CurrentWindow.Dispatcher.Invoke(new Action(() =>
                        {
                            if (result.Code == "200")
                            {
                                PosViewModel.Current.ActivityTitle = (No == 0 ? "" : Theme);
                                CurrentWindow.Close();
                            }
                            else
                            {
                                Toast.ShowMessage(result.Message, CurrentWindow);
                            }
                        }));
                    });
                });
            }
        }
    }
}