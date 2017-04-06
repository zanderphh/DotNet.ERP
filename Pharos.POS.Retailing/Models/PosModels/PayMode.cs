﻿
namespace Pharos.POS.Retailing.Models.PosModels
{
    public enum PayMode
    {
        /// <summary>
        /// 现金支付
        /// </summary>
        CashPay = 1,
        /// <summary>
        /// 银联支付
        /// </summary>
        UnionPay = 2,
        /// <summary>
        /// 支付宝支付(保留)
        /// </summary>
        Zhifubao = 3,
        /// <summary>
        /// 微信支付(保留)
        /// </summary>
        Weixin = 4,
        /// <summary>
        /// 代金券
        /// </summary>
        CashCoupon = 5,
        /// <summary>
        /// 储值卡
        /// </summary>
        StoredValueCard = 6,
        /// <summary>
        /// 多方式支付
        /// </summary>
        Multiply = 7,
        /// <summary>
        /// 支付宝扫描支付(保留)
        /// </summary>
        ScanZhifubao = 8,
        /// <summary>
        /// 微信扫描支付(保留)
        /// </summary>
        ScanWeixin = 9,
        /// <summary>
        /// 支付宝(保留)
        /// </summary>
        AliPay = 20,
        /// <summary>
        /// 微信(保留)
        /// </summary>
        WeChat = 21,
        /// <summary>
        /// 即付宝
        /// </summary>
        JiFuBao = 19,
        /// <summary>
        /// 点百趣
        /// </summary>
        DianBaiQuPay = 22,
        /// <summary>
        /// 银联支付
        /// </summary>
        UnionPayCTPOSM = 23,
        /// <summary>
        /// E道积分支付
        /// </summary>
        EDaoPay = 24,
        /// <summary>
        /// 融合支付-动态二维码
        /// </summary>
        RongHeDynamicQRCodePay = 25,
        /// <summary>
        /// 融合支付-客户动态二维码
        /// </summary>
        RongHeCustomerDynamicQRCodePay = 26,

    }
}