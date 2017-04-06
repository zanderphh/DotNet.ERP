﻿using Pharos.POS.Retailing.ChildWin;
using Pharos.POS.Retailing.ChildWin.Pay;
using Pharos.POS.Retailing.Models.ApiParams;
using Pharos.POS.Retailing.Models.ApiReturnResults;
using Pharos.Wpf.ViewModelHelpers;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Pharos.POS.Retailing.Models.ViewModels
{
    public class AllOrderDiscount : BaseViewModel
    {
        public AllOrderDiscount(decimal _amount)
        {
            Amount = _amount;
            this.PropertyChanged += AllOrderDiscount_PropertyChanged;
            this.OnPropertyChanged(o => o.Discount);

        }
        void AllOrderDiscount_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DiscountPrice" && flag)
            {
                flag = false;
                if (DiscountPrice <= 0)
                {
                    DiscountPrice = 0;
                }
                if (Math.Abs(DiscountPrice) > Amount)
                {
                    DiscountPrice = Amount;
                }
                if (DiscountPrice > 0)
                {
                    DiscountPrice = DiscountPrice;
                }
                Price = Amount - DiscountPrice;
                Discount = Price / Amount * 10;
                flag = true;
            }
            else if (e.PropertyName == "Discount" && flag)
            {
                flag = false;
                Price = Math.Round(Amount * Discount / 10, 2, MidpointRounding.AwayFromZero);
                if (price != 0)
                    DiscountPrice = Amount - Price;
                else
                    DiscountPrice = Amount;
                flag = true;
            }
            else if (e.PropertyName == "Price" && flag)
            {
                flag = false;
                Price = Math.Round(Price, 2, MidpointRounding.AwayFromZero);

                Discount = Price / Amount * 10;
                if (price != 0)
                    DiscountPrice = Amount - Price;
                else
                    DiscountPrice = Amount;
                flag = true;
            }
            //flag = true;
        }
        /// <summary>
        /// 订单金额
        /// </summary>
        private decimal amount;

        public decimal Amount
        {
            get { return amount; }
            set { amount = value; this.OnPropertyChanged(o => o.Amount); }
        }
        /// <summary>
        /// 变更属性名
        /// </summary>
        bool flag = true;

        /// <summary>
        /// 订单折扣
        /// </summary>
        private decimal discount = 10m;

        public decimal Discount
        {
            get { return discount; }
            set
            {
                discount = value;
                this.OnPropertyChanged(o => o.Discount);
                if (Discount < 0)
                {
                    Discount = 0m;
                }
                //if (Discount > 10)
                //{
                //    Discount = 10m;
                //}

            }
        }
        /// <summary>
        /// 折后金额
        /// </summary>
        private decimal price;

        public decimal Price
        {
            get { return price; }
            set
            {
                price = value;
                if (Discount < 0)
                {
                    Discount = 0m;
                }
                this.OnPropertyChanged(o => o.Price);
            }
        }
        /// <summary>
        /// 整单让利
        /// </summary>
        private decimal discountPrice;

        public decimal DiscountPrice
        {
            get { return discountPrice; }
            set
            {
                discountPrice = value;
                //if (DiscountPrice < 0)
                //{
                //    DiscountPrice = 0m;
                //}
                this.OnPropertyChanged(o => o.DiscountPrice);
            }
        }


        /// <summary>
        /// 店长授权密码
        /// </summary>
        private string password;

        public string Password
        {
            get { return password; }
            set { password = value; this.OnPropertyChanged(o => o.Password); }
        }

        /// <summary>
        /// 获取店长授权
        /// </summary>
        public ICommand ConfirmCommand
        {
            get
            {
                return new GeneralCommand<object>((o1, o2) =>
                {
                    Task.Factory.StartNew(() =>
                    {
                        if (string.IsNullOrEmpty(Password))
                        {
                            Toast.ShowMessage("请输入授权密码！", CurrentWindow);
                            return;
                        }
                        var _machinesInfo = Global.MachineSettings.MachineInformations;
                        AuthorizationParams _params = new AuthorizationParams()
                        {
                            StoreId = _machinesInfo.StoreId,
                            MachineSn = _machinesInfo.MachineSn,
                            CID = _machinesInfo.CompanyId,
                            Password = Password
                        };
                        var result = ApiManager.Post<AuthorizationParams, ApiRetrunResult<object>>(@"api/Authorization", _params);
                        CurrentWindow.Dispatcher.Invoke(new Action(() =>
                        {
                            if (result.Code == "200")
                            {
                                //
                                // var isNonCashWipeZero = Global.MachineSettings.MachineInformations.IsNonCashWipeZero;
                                //if (isNonCashWipeZero)
                                //{
                                //    Price = Math.Round(Price, 1, MidpointRounding.AwayFromZero);
                                //}

                                //整单折扣时同步更近主界面金额
                                PosViewModel.Current.Receivable = Price;
                                PosViewModel.Current.Preferential = PosViewModel.Current.Preferential + (Amount - Price);


                                //跳到支付界面
                                MultiPay page = new MultiPay(Price, PosModels.PayAction.Sale);
                                page.Owner = Application.Current.MainWindow;
                                CurrentWindow.Hide();
                                page.ShowDialog();
                                CurrentWindow.Close();
                                //ZhiFuFangShi page = new ZhiFuFangShi(Price, PosModels.PayAction.Sale);
                                //page.Owner = Application.Current.MainWindow;

                                //CurrentWindow.Hide();
                                //page.ShowDialog();
                                //CurrentWindow.Close();
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