﻿using Pharos.POS.Retailing.Models.ViewModels;
using Pharos.Wpf.Controls;
using System;
using Pharos.POS.Retailing.Extensions;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Pharos.POS.Retailing.Models.PosModels;
using Pharos.Wpf.Extensions;
using Pharos.POS.Retailing.Models.ViewModels.Pay;
using System.Threading.Tasks;
using System.Threading;

namespace Pharos.POS.Retailing.ChildWin.Pay
{
    /// <summary>
    /// 多方式支付
    /// </summary>
    public partial class MultiPay : DialogWindow02
    {
        MultiPayViewModel MultiPayViewModel;
        public MultiPay(decimal amount, PayAction action)
        {
            InitializeComponent();
            MultiPayViewModel = new MultiPayViewModel(amount, action);
            MultiPayViewModel.PayItems = new ObservableCollection<MultiPayItemViewModel> {
                new MultiPayItemViewModel(){ IsFrist=true, IsLast=false},
                new MultiPayItemViewModel(){ IsFrist=false, IsLast=true},
            };
            this.ApplyBindings(this, MultiPayViewModel);
            this.InitDefualtSettings();

            this.PreviewKeyDown += MultiPay_PreviewKeyDown;
            this.Closing += MultiPay_Closing;
            this.Loaded += MultiPay_Loaded;
        }
        /// <summary>
        /// 加载指定支付
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MultiPay_Loaded(object sender, RoutedEventArgs e)
        {
            if (payItem != null)
            {
                switch (payItem.Mode)
                {
                    case PayMode.CashPay:
                        CallAddPay(PayMode.CashPay);
                        break;
                    case PayMode.CashCoupon:
                        CallAddPay(PayMode.CashCoupon);
                        break;
                    default:
                        AddPay(payItem, MultiPayViewModel.WipeZeroAfter);
                        break;
                }
            }
            else
            {
                CallAddPay();
            }
        }
        public MultiPay(decimal amount, PayAction action, PayItem _payItem)
            : this(amount, action)
        {
            this.payItem = _payItem;
        }
        private PayItem payItem;
        /// <summary>
        /// 窗口关闭时，阻止必须完成支付的支付关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MultiPay_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MultiPayViewModel.PayItems.Any(o => !o.EnableClose && !o.IsLast && !o.IsFrist) && !MultiPayViewModel.isComplate)
            {
                Toast.ShowMessage("结算过程中不允许直接关闭窗口！", this);
                e.Cancel = true;
                return;
            }
            if (MultiPayViewModel.CurrentPayItem != null && MultiPayViewModel.CurrentPayItem.token != null && MultiPayViewModel.CurrentPayItem.IsRunning)
            {
                Toast.ShowMessage("如要关闭支付，请先在设备上操作【取消】！", this);
                e.Cancel = true;
                return;
            }
            if (page != null && page.IsVisible)
            {
                page.PosCancelButton_Click(null, null);
                page = null;
            }
            PosViewModel.Current.Change = 0m;
            PosViewModel.Current.MultiPayItemViewModel = null;
        }
        /// <summary>
        /// 加号添加新支付，F5刷新支付
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MultiPay_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Add:
                case Key.OemPlus:
                    Label_MouseDown(null, null);
                    e.Handled = true;
                    break;
                case Key.F5:
                    if (MultiPayViewModel != null && MultiPayViewModel.CurrentPayItem != null && MultiPayViewModel.CurrentPayItem.PayItem != null)
                    {
                        switch (MultiPayViewModel.CurrentPayItem.PayItem.Mode)
                        {
                            case PayMode.UnionPayCTPOSM:
                                {
                                    if (MultiPayViewModel.CurrentPayItem.IsRunning)
                                    {
                                        var refresh = false;
                                        Confirm.ShowMessage("正在进行银联支付，确认是否重试！", this, (ConfirmMode mode) =>
                                        {
                                            if (mode == ConfirmMode.Confirmed)
                                                refresh = true;
                                        });
                                        if (!refresh)
                                            return;
                                    }
                                    if (MultiPayViewModel.CurrentPayItem.EnableClose)
                                        MultiPayViewModel.CurrentPayItem.DoPay();
                                    e.Handled = true;
                                }
                                break;
                            case PayMode.StoredValueCard:
                                {
                                    if (MultiPayViewModel.CurrentPayItem.EnableClose)
                                    {
                                        MultiPayViewModel.CurrentPayItem.isRetry = true;
                                        MultiPayViewModel.CurrentPayItem.DoPay();
                                    }
                                    e.Handled = true;
                                }
                                break;
                            case PayMode.RongHeDynamicQRCodePay:
                                {
                                    if (MultiPayViewModel.CurrentPayItem.EnableClose)
                                        MultiPayViewModel.CurrentPayItem.DoPay();
                                    e.Handled = true;
                                }
                                break;
                        }

                    }
                    break;
            }

        }
        public void SetPayItem(PayMode mode)
        {
            var payItem = MultiPayViewModel.PayItems.LastOrDefault(o => o.PayItem != null && o.PayItem.Mode == mode);
            if (payItem != null)
            {
                RemovePayItem(payItem);
            }

        }

        public void AddPay(PayItem payItem, decimal amount)
        {
            if (payItem != null)
            {
                //if (amount <= 0)
                //{
                //    Toast.ShowMessage("支付金额应大于0！", this);
                //    return;
                //}
                if ((MultiPayViewModel.PayAction == PayAction.Refund || MultiPayViewModel.PayAction == PayAction.Change || MultiPayViewModel.PayAction == PayAction.RefundAll) && MultiPayViewModel.Amount < 0 && amount > 0)
                {
                    amount = -amount;
                }
                var cashCoupon = 0m;
                cashCoupon = MultiPayViewModel.PayItems.Where(o => o.PayItem != null && o.PayItem.Mode == PayMode.CashCoupon).Sum(o => o.Amount);
                var notCash = MultiPayViewModel.PayItems.Where(o => o.PayItem != null && o.PayItem.Mode != PayMode.CashPay && o.PayItem.Mode != PayMode.CashCoupon).Sum(o => o.Amount);
                switch (payItem.Mode)
                {
                    case PayMode.CashPay:
                        break;
                    case PayMode.CashCoupon:
                        cashCoupon += amount;
                        break;
                    default:
                        notCash += amount;
                        break;
                }
                var needNotCash = (MultiPayViewModel.WipeZeroAfter - cashCoupon);
                if (notCash > needNotCash && notCash > 0)
                {
                    Toast.ShowMessage("非现金收款金额超出！", this);
                    return;
                }
                MultiPayViewModel.CurrentPayItem = new MultiPayItemViewModel() { IsFrist = false, PayItem = payItem, CurrentWindow = this, IsLast = false, IsSelected = true, Amount = amount, PayName = payItem.Title };
                MultiPayViewModel.PayItems.Insert(MultiPayViewModel.PayItems.Count - 1, MultiPayViewModel.CurrentPayItem);

                Statistics();
                switch (payItem.Mode)
                {
                    case PayMode.StoredValueCard:
                    case PayMode.UnionPayCTPOSM:
                    case PayMode.RongHeDynamicQRCodePay:
                        MultiPayViewModel.CurrentPayItem.DoPay();
                        break;
                }
            }
        }
        private void Statistics()
        {
            MultiPayViewModel.Received = MultiPayViewModel.PayItems.Sum(o => o.Amount);
            var shangqian = MultiPayViewModel.WipeZeroAfter - MultiPayViewModel.Received;
            if ((shangqian >= 0 && MultiPayViewModel.Amount >= 0) || (shangqian < 0 && MultiPayViewModel.Amount < 0))
            {
                MultiPayViewModel.StillOwe = shangqian;
                MultiPayViewModel.Change = 0m;
            }
            else
            {
                MultiPayViewModel.StillOwe = 0m;
                if (MultiPayViewModel.PayItems.Count == 3 && MultiPayViewModel.PayItems.Any(o => o.PayItem != null && o.PayItem.Mode == PayMode.CashCoupon))
                {
                    MultiPayViewModel.Change = 0m;
                }
                else
                {
                    MultiPayViewModel.Change = -shangqian;
                }
            }
        }
        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CallAddPay();
        }
        ChoosePayWindow page = null;
        private void CallAddPay(PayMode? mode = null)
        {
            if (MultiPayViewModel.StillOwe == 0 && MultiPayViewModel.CurrentPayItem != null)
            {
                Toast.ShowMessage("已全额收款，请注意未支付金额！", this);
                return;
            }
            if (MultiPayViewModel.CurrentPayItem != null && MultiPayViewModel.CurrentPayItem.HasOperat)
            {
                Toast.ShowMessage(MultiPayViewModel.CurrentPayItem.RequestOperatMessage, this);
                return;
            }
            page = new ChoosePayWindow(MultiPayViewModel.PayAction);
            page.Owner = this;
            if (mode.HasValue)
            {
                page.SetPayItem(mode.Value);
            }
            page.Closed += (o1, o2) =>
            {
                if (!page.isCannel)
                    AddPay(page.PayItem, page.Amount);
                page = null;
                this.Focus();
                EnableMove = true;
            };
            EnableMove = false;
            page.Show();
        }
        private void RemovePay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RemovePayItem((sender as Border).DataContext as MultiPayItemViewModel);
        }
        private void RemovePayItem(MultiPayItemViewModel model)
        {
            if (model != null && model.EnableClose)
            {
                if (model.token != null && model.IsRunning)
                {
                    Toast.ShowMessage("支付未完成，如要“取消”，请先在设备上操作“取消”，再进行“移除”操作！", this);
                    return;
                }
                if (model.token != null)
                {
                    model.Cannel();
                }
                MultiPayViewModel.PayItems.Remove(model);
                var item = MultiPayViewModel.PayItems.LastOrDefault(o => !o.IsLast && !o.IsFrist);
                MultiPayViewModel.CurrentPayItem = item;
                PosViewModel.Current.MultiPayItemViewModel = null;
                Statistics();
            }
        }

        private void InputCardNoPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    var ctrl = ((TextBox)sender);
                    ///ctrl.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    e.Handled = true;
                    if (MultiPayViewModel.CurrentPayItem != null && MultiPayViewModel.CurrentPayItem.EnableClose && MultiPayViewModel.CurrentPayItem.IsRequestEnd)
                    {
                        MultiPayViewModel.CurrentPayItem.CardNo = ctrl.Text;
                        MultiPayViewModel.CurrentPayItem.DoPay();
                        ctrl.SelectAll();
                    }
                    break;

            }
        }

        private void txtcardno_Initialized(object sender, EventArgs e)
        {
            Task.Factory.StartNew((o) =>
            {
                Thread.Sleep(200);
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    ((TextBox)o).Focus();
                }));
            }, sender);
        }
    }

}