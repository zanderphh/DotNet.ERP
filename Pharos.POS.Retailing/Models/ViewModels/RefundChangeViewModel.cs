﻿using Pharos.Wpf.ViewModelHelpers;

namespace Pharos.POS.Retailing.Models.ViewModels
{
    public class RefundChangeViewModel : BaseViewModel
    {
        public RefundChangeViewModel()
        {
            Change = new RefundOrChanging(Models.PosModels.ChangeStatus.Change);
            Refund = new RefundOrChanging(Models.PosModels.ChangeStatus.Refund);
            RefundOrder = new RefundOrderViewModel();
            Current = this;
        }
        public static RefundChangeViewModel Current { get; set; }

        private RefundOrChanging refund;

        public RefundOrChanging Refund
        {
            get { return refund; }
            set
            {
                refund = value;
                change.CurrentWindow = CurrentWindow;
                this.OnPropertyChanged(o => o.Refund);
            }
        }

        private RefundOrChanging change;

        public RefundOrChanging Change
        {
            get { return change; }
            set
            {
                change = value;
                change.CurrentWindow = CurrentWindow;
                this.OnPropertyChanged(o => o.Change);
            }
        }
        private RefundOrderViewModel _RefundOrder;

        public RefundOrderViewModel RefundOrder
        {
            get { return _RefundOrder; }
            set { _RefundOrder = value;  this.OnPropertyChanged(o => o.RefundOrder);}
        }
        


    }
}