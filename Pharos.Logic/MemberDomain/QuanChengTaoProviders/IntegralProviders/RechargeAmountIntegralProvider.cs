﻿using Pharos.Logic.Entity;
using Pharos.Logic.MemberDomain.QuanChengTaoProviders.Scenes;
using Pharos.Logic.MemberDomain.QuanChengTaoProviders.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Pharos.ObjectModels.Events;

namespace Pharos.Logic.MemberDomain.QuanChengTaoProviders.IntegralProviders
{
    public class RechargeAmountIntegralProvider : BaseIntegralRuleProvider<RechargeAmountScene, Members>
    {
        public override IEnumerable<Interfaces.IIntegralRule> GetRuleDatas(object info = null)
        {
            return Pharos.Logic.BLL.ReturnRulesService.GetProviderRules((int)info, ProviderId).Select(o => new CommonIntegralRule<RechargeAmountScene>()
            {
                Id = o.Id.ToString(),
                MeteringMode = o.Mode,
                LimitItems = o.LimitItems,
                IntegralExpression = o.GetIntegralExpression<RechargeAmountScene>(GetMemberExpression),
                VerifyExpression = o.GetVerifyExpression<RechargeAmountScene>(ProviderId, GetMemberExpression)
            }).Where(o => o.VerifyExpression != null).ToList();
        }

        private MemberExpression GetMemberExpression(int mode, Expression p)
        {
            MemberExpression propertyName = null;
            switch (mode)// 计量模式（eg:金额，次数。。）
            {
                case (int)MeteringMode.AmountCounter:
                    propertyName = Expression.Property(p, "Amount");
                    break;
            }
            return propertyName;
        }

        public override int GetProviderId()
        {
            return (int)IntegralProviderType.RechargeAmountIntegralProvider;
        }

        public override RechargeAmountScene GetTScene(object channelMessage, Interfaces.IIntegralRule rule, Members member)
        {
            if (!(channelMessage is RechargeCompletedEvent))
                return null;
            var channelDatas = (RechargeCompletedEvent)channelMessage;
            return new RechargeAmountScene()
            {
                Amount = channelDatas.ReceiveAmount,
                Member = member
            };
        }
    }
}
