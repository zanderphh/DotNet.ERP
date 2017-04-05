﻿using Pharos.ObjectModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pharos.Logic.ApiData.Pos.Sale.Barcodes
{
    public class BarcodeIdentity : IIdentification
    {
        public ProductType ProductType { get; set; }

        public string MainBarcode { get; set; }

        public IEnumerable<string> MultiCode { get; set; }

        public bool HasEditPrice { get; set; }

        public string RecordId { get; set; }
    }
}