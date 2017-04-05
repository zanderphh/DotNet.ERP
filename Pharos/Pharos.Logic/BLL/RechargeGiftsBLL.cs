﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pharos.Sys.Entity;
using Pharos.Utility;

namespace Pharos.Logic.BLL
{
    public class RechargeGiftsBLL
    {
        private readonly RechargeGiftsService _service = new RechargeGiftsService();


        public object FindRechargeGiftPageList(int state, string createUID, out int count)
        {
            return _service.FindRechargeGiftPageList(state, createUID, out count);
        }

        public OpResult UpdateRechargeGiftState(int state, string ids)
        {
            return _service.UpdateRechargeGiftState(state, ids);
        }
        /// <summary>
        /// add a new entity
        /// </summary>
        /// <param name="_rechargeGift"></param>
        /// <returns></returns>
        public OpResult CreateRechargeGift(Entity.RechargeGifts _rechargeGift)
        {
            if (_rechargeGift.Id == 0)
            {//add
                _rechargeGift.RuleId = Pharos.Logic.CommonRules.GUID;
                _rechargeGift.CreateDT = DateTime.Now;
                _rechargeGift.CreateUID = Sys.CurrentUser.UID;
                _rechargeGift.CompanyId = CommonService.CompanyId;
                var currentDate = DateTime.Now;
                if (!string.IsNullOrEmpty(_rechargeGift.ExpiryStart) && !string.IsNullOrEmpty(_rechargeGift.ExpiryEnd))
                {
                    //get state

                    if ((Convert.ToDateTime(_rechargeGift.ExpiryStart) > currentDate && currentDate < Convert.ToDateTime(_rechargeGift.ExpiryEnd).AddDays(1)) || currentDate.ToString("yyyy-MM-dd") == _rechargeGift.ExpiryStart)
                    {
                        _rechargeGift.State = 1;//activing
                    }
                    else if (Convert.ToDateTime(_rechargeGift.ExpiryStart) > currentDate)
                    {
                        _rechargeGift.State = 0;
                    }
                    else if (Convert.ToDateTime(_rechargeGift.ExpiryEnd).AddDays(1) < currentDate)
                    {
                        _rechargeGift.State = 2;//actived
                    }
                }
                else if (!string.IsNullOrEmpty(_rechargeGift.ExpiryStart))
                {
                    //ExpiryEnd is null
                    if (Convert.ToDateTime(_rechargeGift.ExpiryStart) > currentDate)
                    {
                        _rechargeGift.State = 0;
                    }
                    else
                    {
                        _rechargeGift.State = 1;
                    }
                }
                return BaseService<Entity.RechargeGifts>.Add(_rechargeGift);
            }
            else
            {
                //update
                return _service.UpdateRechargeGift(_rechargeGift);
            }
        }
        /// <summary>
        /// get a rechargegifts by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Entity.RechargeGifts GetRechargeGiftsById(int id)
        {
            return BaseService<Entity.RechargeGifts>.FindById(id);
        }

        public IEnumerable<SysUserInfo> GetCreateUserToDropDown()
        {
            return _service.GetCreateUserToDropDown();
        }

        public object GetRechargeGiftInfo(decimal rechargeMoney)
        {
            return _service.GetRechargeGiftInfo(rechargeMoney, 0);//0=充值
        }
    }
}