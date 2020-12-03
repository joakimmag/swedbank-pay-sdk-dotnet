﻿using System;

namespace SwedbankPay.Sdk.PaymentInstruments
{
    internal class TransactionResponse : Identifiable, ITransactionResponse
    {
        public TransactionResponse(string id, TransactionDto transaction)
        {
            Id = new Uri(id, UriKind.RelativeOrAbsolute);
            Transaction = transaction.Map();
        }

        public ITransaction Transaction { get; }
    }
}