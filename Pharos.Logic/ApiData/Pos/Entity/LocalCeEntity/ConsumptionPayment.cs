﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pharos.Logic.ApiData.Pos.Entity.LocalCeEntity
{
    public class ConsumptionPayment : BaseEntity
    {
        public string PaySN { get; set; }
        public int ApiCode { get; set; }
        public string ApiOrderSN { get; set; }
        public decimal Amount { get; set; }
        public string Memo { get; set; }
        public short State { get; set; }
        public string CardNo { get; set; }


        public decimal Change { get; set; }

        public decimal Received { get; set; }
    }
}
