﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pharos.POS.Retailing.Models.ApiParams
{
    public class StoredValueCardRechargeParams : BaseApiParams
    {
        public string CardNo { get; set; }
        public decimal Amount { get; set; }

        public int Type { get; set; }
    }
}