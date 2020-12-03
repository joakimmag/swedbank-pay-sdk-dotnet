﻿using SwedbankPay.Sdk.PaymentInstruments;
using System;

namespace SwedbankPay.Sdk.PaymentOrders
{
    internal class PaymentOrderPaymentDto
    {
        public string CorporationId { get; set; }
        public string Id { get; set; }
        public long Number { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public string Instrument { get; set; }
        public string Operation { get; set; }
        public string Intent { get; set; }
        public string State { get; set; }
        public string Currency { get; set; }
        public PricesDto Prices { get; set; }
        public int Amount { get; set; }
        public int RemainingReversalAmount { get; set; }
        public string Description { get; set; }
        public string UserAgent { get; set; }
        public string Language { get; set; }
        public PaymentOrderTransactionsDto Transactions { get; set; }
        public PaymentOrderAuthorizationsDto Authorizations { get; set; }
        public PaymentOrderCapturesDto Captures { get; set; }
        public PaymentOrderCancellationsDto Cancellations { get; set; }
        public UrlsDto Urls { get; set; }
        public PaymentOrderPayeeInfoDto PayeeInfo { get; set; }
        public MetadataResponse Metadata { get; set; }
        public string PayerReference { get; set; }
        public string PaymentToken { get; set; }
        public PaymentOrderReversals Reversals { get; set; }
        public SalesTransactionListResponseDto Sales { get; set; }
    }
}