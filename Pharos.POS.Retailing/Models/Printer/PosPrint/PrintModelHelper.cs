﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using Pharos.POS.Retailing.Models.Printer;

namespace Pharos.POS.Retailing.Models.Printer
{
    public class PrintModelHelper
    {

        #region 日结单
        /// <summary>
        /// 打印日结单 
        /// </summary>
        public void PrintDayReport()
        {
            DayReportModel dayReport = new DayReportModel();
            dayReport.TicketWidth = 30;
            dayReport.StoreName = "小慧1店";
            dayReport.Title = "日结报表";
            dayReport.Title2 = "POS机号：001";
            dayReport.StockDateStr = "日结时间:" + DateTime.Now.ToString("yyyy-MM-dd");
            dayReport.PrintDate = DateTime.Now;
            List<TransactionItemModel> transactionItemList = new List<TransactionItemModel>();
            transactionItemList.Add(new TransactionItemModel("销售合计", 6400, 77670000.40M));
            transactionItemList.Add(new TransactionItemModel("退货合计", 0, 0.00M));
            transactionItemList.Add(new TransactionItemModel("赠送合计", 7, 175.00M));
            transactionItemList.Add(new TransactionItemModel("换货合计", 71, 7767.40M));
            dayReport.TransactionItemList = transactionItemList;

            List<EmployeeModel> employeeList = new List<EmployeeModel>();
            EmployeeModel employee1 = new EmployeeModel();
            employee1.EmployeeSN = "001";
            employee1.Name = "员工1";
            employee1.BeginTime = DateTime.Now.AddDays(-1);
            employee1.EndTime = DateTime.Now;
            employee1.EmployeeTransactionItems = new List<TransactionItemModel>();
            var employee1ChildItems1 = new Dictionary<string, decimal>();
            employee1ChildItems1.Add("RMB-现金", 4361.00M);
            employee1ChildItems1.Add("RMB-银行卡", 891.00M);
            employee1ChildItems1.Add("RMB-自动抹零", 0.00M);
            employee1.EmployeeTransactionItems.Add(new TransactionItemModel("销售合计", 44, 445252.00M, employee1ChildItems1));
            //var employee1ChildItems2 = new Dictionary<string, decimal>();
            //employee1ChildItems2.Add("RMB-赠送", 200.00M);
            employee1.EmployeeTransactionItems.Add(new TransactionItemModel("赠送合计", 6, 3000M));
            employee1.EmployeeTransactionItems.Add(new TransactionItemModel("入款合计", 10, 2510.12M));
            employee1.EmployeeTransactionItems.Add(new TransactionItemModel("出款合计", 10, 310.22M));
            employee1.EmployeeTransactionItems.Add(new TransactionItemModel("退货合计", 10, 500.72M));
            employee1.EmployeeTransactionItems.Add(new TransactionItemModel("换货合计", 10, 510.10M));
            employee1.EmployeeTransactionItems.Add(new TransactionItemModel("剩余现金", 610.12M));

            EmployeeModel employee2 = new EmployeeModel();
            employee2.EmployeeSN = "002";
            employee2.Name = "员工2";
            employee2.BeginTime = DateTime.Now.AddDays(-1);
            employee2.EndTime = DateTime.Now;
            employee2.EmployeeTransactionItems = new List<TransactionItemModel>();
            var employee2ChildItems1 = new Dictionary<string, decimal>();
            employee2ChildItems1.Add("RMB-现金", 4361.00M);
            employee2ChildItems1.Add("RMB-银行卡", 891.00M);
            employee2ChildItems1.Add("RMB-自动抹零", 0.00M);
            employee2.EmployeeTransactionItems.Add(new TransactionItemModel("销售合计", 44, 445252.00M, employee2ChildItems1));
            //var employee2ChildItems2 = new Dictionary<string, decimal>();
            //employee2ChildItems2.Add("RMB-赠送", 200.00M);
            employee2.EmployeeTransactionItems.Add(new TransactionItemModel("赠送合计", 6, 3000M));
            employee2.EmployeeTransactionItems.Add(new TransactionItemModel("入款合计", 10, 2510.12M));
            employee2.EmployeeTransactionItems.Add(new TransactionItemModel("出款合计", 10, 310.22M));
            employee2.EmployeeTransactionItems.Add(new TransactionItemModel("退货合计", 10, 500.72M));
            employee2.EmployeeTransactionItems.Add(new TransactionItemModel("换货合计", 10, 510.10M));
            employee2.EmployeeTransactionItems.Add(new TransactionItemModel("剩余现金", 610.12M));

            employeeList.Add(employee1);
            employeeList.Add(employee2);

            dayReport.EmployeeList = employeeList;

            string printStr = GetDailyReportStr(dayReport);
            PrintHelper.Print(printStr, null, true);
        }

        public string GetDailyReportStr(DayReportModel dayReport)
        {
            TicketSet ticketSet = new TicketSet();

            ticketSet.TicketWidth = dayReport.TicketWidth;
            ticketSet.SignWeight = '-';
            ticketSet.SignLight = '-';
            ticketSet.Colper1 = 0.37M;
            ticketSet.Colper2 = 0.18M;
            ticketSet.Colper3 = 0.45M;

            ticketSet.DayReportModel = dayReport;

            var ticketStr = ticketSet.DayReport();
            return ticketStr;

        }
        #endregion

        #region 售卖单
        /// <summary>
        /// 打印售卖单 
        /// </summary>
        public void PrintSaleCarte()
        {
            TicketModel ticketModel = new TicketModel();
            ticketModel.TicketWidth = 40;//发票宽度，按字符数计算，根据打印机型号有所区别(通常在30-70之间),建议系统提供配置入口
            ticketModel.StoreName = "小慧1店";
            ticketModel.DeviceNumber = "XM01";
            ticketModel.SN = "1058";
            ticketModel.Cashier = "001";

            List<ProductModel> productList = new List<ProductModel>();
            ProductModel productModel = new ProductModel();
            productModel.Code = "6912345678901";
            productModel.Name = "花生酥商品名称测试测试ABCDEFG10234567899AAA";
            productModel.Num = 1.02M;
            productModel.Price = 15000.00M;
            productModel.SubTotal = 15000.00M;
            productModel.IsPromotion = true;
            productList.Add(productModel);

            ProductModel productModel2 = new ProductModel();
            productModel2.Code = "012345678912345678";
            productModel2.Name = "牛轧糖";
            productModel2.Num = 1;
            productModel2.Price = 105.00M;
            productModel2.SubTotal = 105.00M;
            productList.Add(productModel2);

            ProductModel productModel3 = new ProductModel();
            productModel3.Code = "012345678912345679";
            productModel3.Name = "辣条";
            productModel3.Num = 1;
            productModel3.Price = 1000.00M;
            productModel3.SubTotal = 1000.00M;
            productList.Add(productModel3);

            ticketModel.ProductList = productList;

            ticketModel.CountNum = 3;
            ticketModel.TotalPrice = 16105.00M.ToString("0.###");
            ticketModel.PayType = "现金";
            ticketModel.Receivable = 16105.00M.ToString("0.###");
            ticketModel.Change = 0.00M;
            ticketModel.Weigh = "100斤";
            List<string> footItemList = new List<string>();
            //footItemList.Add("称重商品数量请参照条码标签");
            footItemList.Add("欢迎光临");
            footItemList.Add("服务电话：0592-1234567");
            footItemList.Add("请保留电脑小票，作为退换货凭证");
            ticketModel.FootItemList = footItemList;

            string titleStr = string.Empty;
            string printStr = GetPrintStr(ticketModel, out titleStr);
            PrintHelper.Print(printStr, titleStr);
        }
        #endregion

        public string GetPrintStr(TicketModel ticketModel, out string titleStr)
        {
            TicketSet ticketSet = new TicketSet();

            ticketSet.TicketWidth = ticketModel.TicketWidth - 5;
            ticketSet.SignWeight = '-';
            ticketSet.SignLight = '-';
            ticketSet.Colper1 = 0.50M;
            //ticketSet.Colper2 = 0.16M;
            ticketSet.Colper3 = 0.23M;
            ticketSet.Colper4 = 0.26M;
            ticketSet.KeyColper = 0.22M;
            ticketSet.ValueColper = 0.22M;
            ticketSet.KeyTopColper = 0.19M;
            ticketSet.ValueTopColper = 0.15M;
            ticketSet.MidCol1KeyColper = 0.16M;
            ticketSet.MidCol1ValueColper = 0.26M;

            ticketSet.TicketSignature = "";
            ticketSet.TicketTitle = ticketModel.StoreName;
            ticketSet.AddKeyAndValueTop("机号：", ticketModel.DeviceNumber);
            ticketSet.AddKeyAndValueTop("收银员：", ticketModel.Cashier);
            ticketSet.AddKeyAndValueTop("流水号：", ticketModel.SN);
            string timeStr = "";
            switch (ticketModel.OrderType)
            {
                case 1:
                case 2:
                    timeStr = "退换时间：";
                    break;
                case 3:
                    timeStr = "退单时间：";
                    break;
                default:
                    timeStr = "销售时间：";
                    break;
            }
            ticketSet.AddKeyAndValueTop(timeStr, ticketModel.CreateDT.ToString("yyyy-MM-dd HH:mm:ss"));
            ticketSet.AddKeyAndValueTop("打印时间：", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));


            DataTable productListDT = new DataTable();
            productListDT.Columns.Add("条码/品名0", typeof(string));
            productListDT.Columns.Add("条码/品名", typeof(string));
            productListDT.Columns.Add("数量", typeof(string));
            //productListDT.Columns.Add("售价", typeof(string));
            productListDT.Columns.Add("小计", typeof(string));

            int productNo = 1;
            string productNoStr = string.Empty;
            foreach (var productModel in ticketModel.ProductList)
            {
                if (productNo < 10) productNoStr = "0" + productNo + ".";
                if (productNo >= 10) productNoStr = productNo + ".";

                DataRow newRow1 = productListDT.NewRow();
                newRow1["条码/品名0"] = "   " + productModel.Code;
                string productNumStr = productModel.Num.ToString("0.###");
                if (productModel.IsPromotion)
                {
                    newRow1["条码/品名"] = productNoStr + productModel.Name + "（促销）";
                }
                else
                {
                    newRow1["条码/品名"] = productNoStr + productModel.Name;

                }
                newRow1["数量"] = " " + productNumStr + " ";
                //newRow1["售价"] = productModel.Price.ToString("f2");
                newRow1["小计"] = productModel.SubTotal.ToString("f2");
                productListDT.Rows.Add(newRow1);

                productNo++;
            }

            ticketSet.DtGoodsList = productListDT;

            ticketSet.AddKeyAndValueMid("件数：", ticketModel.CountNum.ToString("f0"));
            if (Convert.ToDecimal(ticketModel.TotalPrice) >= 0)
            {
                ticketSet.AddKeyAndValueMid("应收：", "￥" + ticketModel.TotalPrice);
            }
            else
            {
                ticketSet.AddKeyAndValueMid("应收：", ticketModel.TotalPrice);

            }
            if (ticketModel.PayType != null)
                ticketSet.AddKeyAndValueMid("结算：", ticketModel.PayType);
            if (ticketModel.OrderType == 1 && Convert.ToDecimal(ticketModel.Receivable) > 0)
            {
                ticketSet.AddKeyAndValueMid("实收：", "￥" + ticketModel.Receivable);
            }
            else if (Convert.ToDecimal(ticketModel.Receivable) < 0)
            {
                ticketSet.AddKeyAndValueMid("退款：", ticketModel.Receivable);
            }
            else
            {
                ticketSet.AddKeyAndValueMid("实收：", "￥" + ticketModel.Receivable);
            }

            // ticketSet.AddKeyAndValueMid("称重：", Conver.FormatterZeroWeigh(ticketModel.Weigh));
            if (ticketModel.Change != 0)//3=退单
            {
                ticketSet.AddKeyAndValueMid("找零：", "￥" + ticketModel.Change.ToString("f2"));

            }
            //if (ticketModel.OrderType != 2)
            //{
            if (ticketModel.Preferential > 0 && ticketModel.OrderType != 3)
            {
                ticketSet.AddKeyAndValueMid("已优惠：", "￥" + ticketModel.Preferential.ToString("0.##"));
            }
            //退整单时显示原优惠
            if (ticketModel.Preferential > 0 && ticketModel.OrderType == 3)
            {
                ticketSet.AddKeyAndValueMid("原优惠：", "￥" + ticketModel.Preferential.ToString("0.##"));
            }
            //}
            //有导购员就显示导购员
            if (!string.IsNullOrEmpty(ticketModel.SaleMan))
            {
                if (ticketModel.SaleMan.Contains("["))
                {
                    var s = ticketModel.SaleMan.IndexOf('[');
                    var e = ticketModel.SaleMan.IndexOf(']');
                    ticketSet.AddKeyAndValueMid("导购员：", ticketModel.SaleMan.Substring(s + 1, e - s - 1));
                }
                else
                {
                    ticketSet.AddKeyAndValueMid("导购员：", ticketModel.SaleMan);
                }
            }
            //会员卡支付时显示余额
            if (ticketModel.CardAndBalances != null && ticketModel.CardAndBalances.Count > 0)
            {
                foreach (var item in ticketModel.CardAndBalances)
                {
                    ticketSet.AddKeyAndValueListForCard("卡号：" + item.FirstOrDefault().Key.ToString(), "\r\n余额：" + Convert.ToDecimal(item.FirstOrDefault().Value).ToString("f2"));
                }

            }

            if (ticketModel.FootItemList != null)
            {
                foreach (var footItem in ticketModel.FootItemList)
                {
                    ticketSet.AddKeyAndValueFoot(footItem, "");
                }
            }

            ticketSet.TicketFooter = "";

            var ticketStr = ticketSet.Ticket();
            var ticketTitle = ticketSet.GetTicketTitle();
            titleStr = ticketTitle;
            return ticketStr;

        }

        /// <summary>
        /// 获取充值小票模版
        /// </summary>
        /// <param name="ticketModel"></param>
        /// <param name="titleStr"></param>
        /// <returns></returns>
        public string GetPrintStrByRecharge(TicketModel ticketModel, out string titleStr)
        {
            TicketSet ticketSet = new TicketSet();

            ticketSet.TicketWidth = ticketModel.TicketWidth - 5;
            ticketSet.SignWeight = '-';
            ticketSet.SignLight = '-';
            ticketSet.Colper1 = 0.33M;
            //ticketSet.Colper2 = 0.16M;
            ticketSet.Colper3 = 0.33M;
            ticketSet.Colper4 = 0.33M;
            ticketSet.KeyColper = 0.22M;
            ticketSet.ValueColper = 0.22M;
            ticketSet.KeyTopColper = 0.19M;
            ticketSet.ValueTopColper = 0.15M;
            ticketSet.MidCol1KeyColper = 0.16M;
            ticketSet.MidCol1ValueColper = 0.26M;

            ticketSet.TicketSignature = "";
            ticketSet.TicketTitle = ticketModel.StoreName;
            ticketSet.AddKeyAndValueTop("机号：", ticketModel.DeviceNumber);
            ticketSet.AddKeyAndValueTop("收银员：", ticketModel.Cashier);
            ticketSet.AddKeyAndValueTop("充值卡号：", ticketModel.SN);
            ticketSet.AddKeyAndValueTop("充值时间：", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            ticketSet.AddKeyAndValueTop("打印时间：", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            DataTable productListDT = new DataTable();
            productListDT.Columns.Add("", typeof(string));
            productListDT.Columns.Add("充前金额", typeof(string));
            productListDT.Columns.Add("充值金额", typeof(string));
            //productListDT.Columns.Add("售价", typeof(string));
            productListDT.Columns.Add("可用金额", typeof(string));

            //int productNo = 1;
            string productNoStr = string.Empty;
            //foreach (var productModel in ticketModel.ProductList)
            //{
            //if (productNo < 10) productNoStr = "0" + productNo + ".";
            //if (productNo >= 10) productNoStr = productNo + ".";

            DataRow newRow1 = productListDT.NewRow();
            //newRow1["条码/品名0"] = "   " + productModel.Code;
            //string productNumStr = productModel.Num.ToString("0.###");

            newRow1["充前金额"] = ticketModel.rechargeModel.BeforeAmount.ToString("f2");

            newRow1["充值金额"] = " " + ticketModel.rechargeModel.RechargeAmount.ToString("f2") + " ";
            //newRow1["售价"] = productModel.Price.ToString("f2");
            newRow1["可用金额"] = ticketModel.rechargeModel.CurrentBalance.ToString("f2");
            productListDT.Rows.Add(newRow1);

            //productNo++;
            //}

            ticketSet.DtGoodsList = productListDT;

            ticketSet.AddKeyAndValueMid("充值方式：", ticketModel.PayType);
            ticketSet.AddKeyAndValueMid("实收：", "￥" + ticketModel.Receivable);

            if (ticketModel.FootItemList != null)
            {
                foreach (var footItem in ticketModel.FootItemList)
                {
                    ticketSet.AddKeyAndValueFoot(footItem, "");
                }
            }

            ticketSet.TicketFooter = "";

            var ticketStr = ticketSet.Ticket(true);
            var ticketTitle = ticketSet.GetTicketTitle();
            titleStr = ticketTitle;
            return ticketStr;
        }
    }
}
