﻿using Pharos.Logic.ApiData.Pos.DataAdapter;
using Pharos.Logic.ApiData.Pos.ValueObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pharos.Logic.ApiData.Pos.Exceptions;
using Pharos.Logic.ApiData.Pos.Sale.Barcodes;
using Pharos.Logic.ApiData.Pos.Sale.Members;
using Pharos.Logic.ApiData.Pos.Sale.Marketings;
using Pharos.ObjectModels;
using Newtonsoft.Json;
using Pharos.Infrastructure.Data.Normalize;

namespace Pharos.Logic.ApiData.Pos.Sale
{
    /// <summary>
    /// 购物车
    /// </summary>
    public class ShoppingCart
    {
        public IEnumerable<MarketingContext> _Marketings { get; set; }

        /// <summary>
        /// 初始化购物车，生成订单编号
        /// </summary>
        public ShoppingCart()
        {
            OrderList = new List<IBarcode>();
            EnableRangeMarketings = true;
        }

        #region 属性

        /// <summary>
        /// 会员Id
        /// </summary>
        public string MemberId { get; set; }



        public MemberInfo MemberInfo { get; set; }
        /// <summary>
        /// 特殊活动编号
        /// </summary>
        public int ActivityId { get; set; }

        /// <summary>
        /// 购物车与结算POS机关联信息
        /// </summary>
        public MachineInformation MachineInformation { get; set; }

        /// <summary>
        /// 购物车编号/订单编号
        /// </summary>
        public string OrderSN { get; set; }
        [JsonIgnore]
        public string CustomOrderSN
        {
            get
            {
                if (generalOrderSN == null)
                    NewAndResetOrderSN(false);
                return GeneralOrderSN.ToString();
            }
        }
        Pharos.Infrastructure.Data.Normalize.PaySn generalOrderSN = null;

        [JsonIgnore]
        public Pharos.Infrastructure.Data.Normalize.PaySn GeneralOrderSN
        {
            get
            {
                if (generalOrderSN == null)
                    NewAndResetOrderSN(false);
                return generalOrderSN;
            }
            set { generalOrderSN = value; }
        }


        public string PayOrderSn { get; set; }
        /// <summary>
        /// 订单列表
        /// </summary>
        public List<IBarcode> OrderList { get; set; }

        /// <summary>
        /// 件数
        /// </summary>
        public int RecordCount
        {
            get
            {
                var products = OrderList.Where(o => o.ProductType != ProductType.Weigh);
                var weighProducts = OrderList.Where(o => o.ProductType == ProductType.Weigh);
                var count = Convert.ToInt32(products.Sum(o => o.SaleNumber));
                count += weighProducts.Count();
                return count;
            }
        }
        public decimal TotalPreferential { get; set; }
        /// <summary>
        /// 满元促销立减金额
        /// </summary>
        public decimal ManYuanMinus { get; set; }
        /// <summary>
        /// 组合促销立减金额
        /// </summary>
        public decimal ZuHeMinus { get; set; }
        #endregion 属性

        #region 方法
        public IBarcode Add(string barcodeString, SaleStatus status)
        {
            //匹配已存在商品（1、状态相同(Promotion促销状态此时默认与正常状态为同一状态)2、条码相同（一品多码、多条码串、主条目）3、该商品不能为称重商品 4、商品要允许改价）
            var product = OrderList.Where(o => o.Details.EnableEditNum && (o.Details.SaleStatus == status || (status == SaleStatus.Normal && o.Details.SaleStatus == SaleStatus.Promotion)) && o.ProductType != ProductType.Weigh && !o.HasEditPrice && o.SameProduct(barcodeString)).FirstOrDefault();
            if (product != null)
            {
                product.SaleNumber++;
            }
            else
            {
                product = BarcodeFactory.Factory(MachineInformation.StoreId, MachineInformation.MachineSn, MachineInformation.CompanyId, barcodeString, status);
                switch (status)
                {
                    case SaleStatus.POSGift:
                    case SaleStatus.ActivityGifts:
                        product.SalePrice = 0;
                        break;
                }
                OrderList.Add(product);
            }
            //移除活动赠品
            ResetProduct(status);
            return product;
        }

        public void Edit(string barcodeString, decimal number, decimal salePrice, SaleStatus status, bool recordHadEditPrice, string recordId)
        {
            var product = OrderList.FirstOrDefault(o => o.RecordId == recordId);
            if (product == null)
                throw new PosException("未找到商品【{0}】的记录", barcodeString);


            //处理改量
            var editNum = Math.Abs(product.SaleNumber - number) > 0.0001m;
            if (!product.Details.EnableEditNum && editNum)
            {
                throw new SaleException("603", "该商品不允许修改数量！");
            }
            else if (product.Details.EnableEditNum && editNum)
            {
                product.SaleNumber = number;
            }


            //处理改价
            var editPrice = Math.Abs(product.MarketingPrice - salePrice) >= 0.01m;
            if (editPrice && !product.Details.EnableEditPrice)
            {
                throw new SaleException("603", "该商品不允许修改售价！");
            }
            else if (editPrice && product.Details.BuyPrice > salePrice)
            {
                throw new SaleException("603", "改价失败，售价必须大于进价！");
            }
            else if (editPrice && product.Details.EnableEditPrice)
            {
                product.SalePrice = salePrice;
                product.EnableMarketing = false;
                product.HasEditPrice = true;
            }
            //重置商品状态为促销运算做准备
            ResetProduct(status);
        }

        internal void ResetProduct(SaleStatus status)
        {
            //重置订单历史小计
            EditTotal = -1;

            //重置促销赠品

            //重置促销信息
            foreach (var item in OrderList)
            {
                if (item.ProductType == ProductType.Bundling && item is BundlingBarcode && (item.Details.SaleStatus == SaleStatus.Normal))
                {
                    item.Details.SaleStatus = SaleStatus.Promotion;
                }
                else if (item.Details.SaleStatus == SaleStatus.Promotion)
                {
                    item.Details.SaleStatus = SaleStatus.Normal;
                }
                else if (item.Details.SaleStatus == SaleStatus.ActivityGifts)
                {
                    item.Details.IsActivityGiftsTimeOut = true;
                }
                item.MarketingMarks = new List<ActiveMarketingRule>();
                item.MarketingPrice = item.SalePrice;
                item.Details.Total = item.SalePrice * item.SaleNumber;
                item.Details.CollectionMarketingPrice = item.MarketingPrice;
            }

            //重置促销造成的同商品分条目显示（合并促销的同商品条目）
            var groupOrders = OrderList.Where(o => !o.HasEditPrice && o.ProductType != ProductType.Weigh)
                .GroupBy(o => new { o.MainBarcode, o.Details.SaleStatus }).ToList();
            //合并商品
            foreach (var item in groupOrders)
            {
                if (item.Count() > 1)
                {
                    IBarcode barcode = item.FirstOrDefault();
                    var list = item.ToList();

                    if (list.Count > 1)
                    {
                        barcode.SaleNumber = list.Sum(o => o.SaleNumber);
                        list.Remove(barcode);
                        OrderList.RemoveAll(o => list.Exists(p => p == o));
                        barcode.MarketingPrice = barcode.SalePrice;
                        barcode.Details.Total = barcode.SalePrice * barcode.SaleNumber;
                        barcode.Details.CollectionMarketingPrice = barcode.MarketingPrice;
                    }
                }
            }
        }


        public void Remove(string barcodeString, SaleStatus status, bool recordHadEditPrice, string recordId)
        {
            var product = OrderList.FirstOrDefault(o => o.RecordId == recordId);
            if (product == null)
                throw new PosException("未找到商品【{0}】的记录", barcodeString);
            //移除商品
            OrderList.Remove(product);

            //重置商品状态为促销运算做准备
            ResetProduct(status);

        }





        /// <summary>
        /// 获取当前订单信息
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IBarcode> GetOrdeList()
        {
            return OrderList;
        }
        /// <summary>
        /// 运算促销获取总额优惠
        /// </summary>
        /// <returns></returns>
        public void RunMarketings()
        {
            var zuHe = 0m;
            var manYuan = 0m;
            IEnumerable<MarketingContext> marketings;
            TotalPreferential = new MarketingManager(MachineInformation.StoreId, MachineInformation.CompanyId).Match(this, out marketings, out zuHe, out manYuan);
            OrderList.RemoveAll(o => (o.Details.SaleStatus == SaleStatus.ActivityGifts) && o.Details.IsActivityGiftsTimeOut);
            ZuHeMinus = zuHe;
            ManYuanMinus = manYuan;
            _Marketings = marketings;
        }

        /// <summary>
        /// 获取销售清单
        /// </summary>
        /// <returns></returns>
        public IEnumerable<OrderItem> GetBuyList()
        {
            return OrderList.Select(o => new OrderItem()
            {
                ActualPrice = o.MarketingPrice,
                Price = o.Details.SystemPrice,
                Number = o.SaleNumber,
                Barcode = o.CurrentString,
                EnableEditNum = o.Details.EnableEditNum,
                EnableEditPrice = o.Details.EnableEditPrice,
                Status = o.Details.SaleStatus,
                Title = o.Details.Title,
                Total = o.Details.Total,
                Unit = o.Details.Unit,
                Category = o.Details.Category,
                Brand = o.Details.Brand,
                Size = o.Details.Size,
                IsMultiCode = o.IsMultiCode,
                RecordId = o.RecordId,
                HasEditPrice = o.HasEditPrice,
                ProductStatus = o.ProductStatus
            });
        }
        /// <summary>
        /// 订单统计
        /// </summary>
        /// <returns>统计结果</returns>
        public SaleStatistics GetSaleStatistics()
        {
            var totalPreferential = TotalPreferential;
            var total = Math.Round(OrderList.Sum(o => ((o.SalePrice <= o.Details.SystemPrice) ? o.Details.SystemPrice : o.SalePrice) * o.SaleNumber), 2, MidpointRounding.AwayFromZero);
            var sale = OrderList.Sum(o => o.Details.Total);
            sale -= ZuHeMinus;
            if (EnableRangeMarketings)
            {
                sale -= ManYuanMinus;
            }
            else
            {
                totalPreferential -= ManYuanMinus;
            }
            var receivable = EditTotal >= 0 ? EditTotal : sale;//实收
            var preferential = total - receivable;
            return new SaleStatistics()
            {
                Total = total,//
                Receivable = receivable,//实收
                Preferential = (preferential > 0 ? preferential : 0m),//优惠
                Num = RecordCount,//数量
                ManJianPreferential = totalPreferential,//满减优惠
                OrderSn = CustomOrderSN,//流水号
                ZuHePreferential = ZuHeMinus,//组合满减
                ManYuanPreferential = ManYuanMinus//满元立减
            };
        }
        bool enableRangeMarketings = true;
        public bool EnableRangeMarketings { get { return enableRangeMarketings; } set { enableRangeMarketings = value; EditTotal = -1; } }
        internal decimal OrderDiscount { get; set; }
        private decimal EditTotal { get; set; }
        /// <summary>
        /// 导购员
        /// </summary>
        public string SaleMan { get; set; }


        public decimal WipeZeroAfter { get; set; }

        /// <summary>
        /// 清空订单
        /// </summary>
        public void Clear(bool isNewOrderSn = false)
        {
            EditTotal = -1;
            TotalPreferential = 0m;
            OrderList.Clear();
            ActivityId = 0;
            EnableRangeMarketings = true;
            if (isNewOrderSn)
                NewAndResetOrderSN();
            SaleMan = string.Empty;//清空导购员
            MemberId = string.Empty;
            MemberInfo = null;
        }
        /// <summary>
        /// 更新订单编号，返回新订单编号
        /// </summary>
        /// <returns></returns>
        public string NewAndResetOrderSN(bool resetOrderSn = true)
        {
            if (resetOrderSn)
                OrderSN = Guid.NewGuid().ToString("N");
            GeneralOrderSN = new Pharos.Infrastructure.Data.Normalize.PaySn(MachineInformation.CompanyId, MachineInformation.StoreId, MachineInformation.MachineSn);
            return OrderSN;
        }
        /// <summary>
        /// 设置购物车与会员关联
        /// </summary>
        /// <param name="phone">手机</param>
        /// <param name="mode">会员来源模式</param>
        public MemberInfo SetMember(string cardNo, string phone, MembersSourseMode mode)
        {
            var memberCardManager = new MemberCardManager();
            var memberInfo = memberCardManager.GetMemberInfo(MachineInformation.StoreId, MachineInformation.MachineSn, MachineInformation.CompanyId, phone, cardNo, mode, MachineInformation.CashierUid);
            MemberInfo = memberInfo;
            if (memberInfo != null)
                MemberId = memberInfo.RecordId;
            return memberInfo;
        }
        /// <summary>
        /// 处理，过滤特殊活动编号
        /// </summary>
        /// <param name="id">活动编号</param>
        public void SetActivityId(int id)
        {
            ActivityId = id;
        }
        #endregion 方法

        /// <summary>
        /// 记录销售信息
        /// </summary>
        /// <param name="apiCodes">支付方式编码（多个以，隔开）</param>
        /// <param name="amount">账单整单优惠后的金额，或者账单应收金额</param>
        /// <param name="wipeZeroAfter">账单抹零之后的金额</param>
        public void Record(string apiCodes, decimal amount, decimal wipeZeroAfter, string deviceSn, DateTime saveTime)
        {
            //OrderDiscount = amount - OrderList.Sum(o => o.Details.Total);//整单折扣=订单金额-订单小计金额
            OrderDiscount = OrderList.Sum(o => o.Details.Total) - amount;//2016-08-03 整单让利又要存成正的，又要改一次
            //订单小计《订单金额=没有整单折扣
            if (EnableRangeMarketings)
            {
                if (OrderDiscount > 0)
                {
                    OrderDiscount -= TotalPreferential;
                }
                else
                {
                    OrderDiscount = 0m;
                }
            }
            EditTotal = wipeZeroAfter;
            WipeZeroAfter = wipeZeroAfter;

            var total = GetSaleStatistics().Receivable;
            var detailsTotal = OrderList.Sum(o => o.Details.CollectionMarketingPrice * o.SaleNumber);
            var oldReceivable = (total - detailsTotal);
            foreach (var item in OrderList)
            {
                if (detailsTotal != 0)
                {
                    item.AveragePrice = (oldReceivable * (item.Details.CollectionMarketingPrice / detailsTotal)) + item.Details.CollectionMarketingPrice;
                }
            }
            foreach (var item in _Marketings)
            {
                item.MarketingRule.SetMarketingRecordNumber(MachineInformation.StoreId, MachineInformation.MachineSn, MachineInformation.CompanyId);
            }
            var dataAdapter = DataAdapterFactory.Factory(MachinesSettings.Mode, MachineInformation.StoreId, MachineInformation.MachineSn, MachineInformation.CompanyId, deviceSn);
            dataAdapter.SaveOrder(this, apiCodes, saveTime);
            new Pharos.Infrastructure.Data.Normalize.PaySn(MachineInformation.CompanyId, MachineInformation.StoreId, MachineInformation.MachineSn).NextSerialNumber();
            this.Clear(true);
            ShoppingCartFactory.ResetCache(this, MachineInformation.StoreId, MachineInformation.MachineSn, MachineInformation.CompanyId, MachineInformation.DeviceSn);

        }

        public SaleManInfo SetSaleMan(string saleMan)
        {
            if (saleMan == "0")
            {
                SaleMan = string.Empty;
                return new SaleManInfo();
            }
            var dataAdapter = DataAdapterFactory.Factory(MachinesSettings.Mode, MachineInformation.StoreId, MachineInformation.MachineSn, MachineInformation.CompanyId, DataAdapterFactory.DEFUALT);
            var saleManInfo = dataAdapter.GetUser(saleMan);
            if (saleManInfo != null)
            {
                SaleMan = saleManInfo.UID;
                //return string.Format("[{0};{1}]", saleManInfo.UserCode, saleManInfo.FullName);
                return new SaleManInfo() { SaleManCode = saleManInfo.UserCode, SaleManName = saleManInfo.FullName };
            }
            else
            {
                throw new PosException("606", "导购员不存在！");
            }
        }

    }
}
