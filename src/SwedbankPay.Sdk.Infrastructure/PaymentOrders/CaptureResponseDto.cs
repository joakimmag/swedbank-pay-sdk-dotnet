﻿using SwedbankPay.Sdk.PaymentInstruments;
using System;

namespace SwedbankPay.Sdk.PaymentOrders
{
    internal class CaptureResponseDto
    {
        public Uri Payment { get; set; }

        public TransactionDto Capture { get; set; }

        public ITransaction Map()
        {
            return Capture.Map();
        }
    }
}